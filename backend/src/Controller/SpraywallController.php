<?php

namespace App\Controller;

use App\Entity\SpraywallProblem;
use App\DTO\SpraywallProblemDto;
use App\Repository\SpraywallRepository;
use Nelmio\ApiDocBundle\Attribute\Model;
use App\Repository\SpraywallProblemRepository;
use Symfony\Bundle\FrameworkBundle\Controller\AbstractController;
use Symfony\Component\HttpFoundation\JsonResponse;
use Symfony\Component\HttpFoundation\Request;
use Symfony\Component\Routing\Attribute\Route;
use OpenApi\Attributes as OA;
use Symfony\Component\Filesystem\Exception\IOExceptionInterface;
use Symfony\Component\Filesystem\Filesystem;
use Symfony\Component\HttpFoundation\Response;

#[Route('/api', name: '')]
final class SpraywallController extends AbstractController
{
    private $spraywallProblemRepository;
    private $spraywallRepository;
    private $filesystem;

    public function __construct(SpraywallProblemRepository $spraywallProblemRepository, SpraywallRepository $spraywallRepository)
    {
        $this->spraywallProblemRepository = $spraywallProblemRepository;
        $this->spraywallRepository = $spraywallRepository;
        $this->filesystem = new Filesystem();
    }

    // #[Route('/spraywall', name: 'app_spraywall')]
    // public function index(): JsonResponse
    // {
    //     return $this->json([
    //         'message' => 'Welcome to your new controller!',
    //         'path' => 'src/Controller/SpraywallController.php',
    //     ]);
    // }

    #[Route('/spraywall/{id}/problems/{problemId}', name: 'spraywall_problem_get', methods: ['GET'])]
    #[OA\Response(
        response: Response::HTTP_OK,
        description: 'Returns a spraywall problem',
        content: new OA\MediaType(
            mediaType: 'application/json',
            schema: new OA\Schema(ref: new Model(type: SpraywallProblemDto::class))
        )
    )]
    public function getProblem($id, $problemId): JsonResponse
    {
        $spraywallProblem = $this->spraywallProblemRepository->find($problemId);

        if (!$spraywallProblem) {
            return $this->json(['error' => 'Problem not found'], Response::HTTP_NOT_FOUND);
        }

        // $exists = $this->filesystem->exists("spraywalls/{$id}");

        $spraywallProblemDto = new SpraywallProblemDto(
            $spraywallProblem->getId(),
            $spraywallProblem->getName(),
            $this->getSpraywallProblemImage($id, $spraywallProblem->getId()),
            $spraywallProblem->getDescription());

        return $this->json($spraywallProblemDto, Response::HTTP_OK);
    }

    #[Route('/spraywall/{id}/problems', name: 'spraywall_problems', methods: ['GET'])]
    #[OA\Response(
        response: Response::HTTP_OK,
        description: 'Returns a list of problems',
        content: new OA\JsonContent(
            type: 'array',
            items: new OA\Items(ref: new Model(type: SpraywallProblemDto::class))
        )
    )]
    public function getProblems($id): JsonResponse
    {
        $spraywallProblems = $this->spraywallProblemRepository->findBySpraywallId($id);

        $exists = $this->filesystem->exists("spraywalls/{$id}");
        
        $spraywallProblemsDto = [];
        foreach ($spraywallProblems as $spraywallProblem) {
            $spraywallProblemsDto[] = new SpraywallProblemDto(
            $spraywallProblem->getId(),
            $spraywallProblem->getName(),
            $this->getSpraywallProblemImage($id, $spraywallProblem->getId()),
            $spraywallProblem->getDescription()
          );
        }
        return $this->json($spraywallProblemsDto, Response::HTTP_OK);
    }

    #[Route('/spraywall/{id}/problem', name: 'spraywall_problem_create', methods: ['POST'])]
    #[OA\RequestBody(
      required: true,
      content: new OA\JsonContent(
          type: 'object',
          properties: [
              new OA\Property(
                  property: 'name',
                  type: 'string',
                  description: 'Name of the spraywall problem'
              ),
              new OA\Property(
                  property: 'description',
                  type: 'string',
                  description: 'Description of the spraywall problem',
                  nullable: true
              ),
              new OA\Property(
                  property: 'image',
                  type: 'string',
                  description: 'PNG image as base64 string (format: data:image/png;base64,<base64-data>)'
              ),
              new OA\Property(
                  property: 'tempPwd',
                  type: 'string',
                  description: 'Temporary password for authentication'
              ),
          ],
          required: ['name', 'image', 'tempPwd']
      )
    )]
    #[OA\Response(
        response: Response::HTTP_CREATED,
        description: 'Returns the created spraywall problem',
        content: new OA\MediaType(
            mediaType: 'application/json',
            schema: new OA\Schema(ref: new Model(type: SpraywallProblemDto::class))
        )
    )]
    #[OA\Response(
        response: Response::HTTP_BAD_REQUEST,
        description: 'Bad request - invalid data',
        content: new OA\JsonContent(
            type: 'object',
            properties: [
            new OA\Property(
                property: 'error',
                type: 'string',
                description: 'Error message'
            )
          ]
        )
    )]
    #[OA\Response(
        response: Response::HTTP_NOT_FOUND,
        description: 'Spraywall not found',
        content: new OA\JsonContent(
            type: 'object',
            properties: [
                new OA\Property(
                    property: 'error',
                    type: 'string',
                    description: 'Error message'
                )
            ]
        )
    )]
    #[OA\Response(
        response: Response::HTTP_INTERNAL_SERVER_ERROR,
        description: 'Internal server error',
        content: new OA\JsonContent(
            type: 'object',
            properties: [
                new OA\Property(
                    property: 'error',
                    type: 'string',
                    description: 'Error message'
                )
            ]
        )
    )]
    #[OA\Response(
        response: Response::HTTP_UNAUTHORIZED,
        description: 'Unauthorized - invalid temporary password',
        content: new OA\JsonContent(
            type: 'object',
            properties: [
                new OA\Property(
                    property: 'error',
                    type: 'string',
                    description: 'Error message'
                )
            ]
        )
    )]
    public function addProblem($id, Request $request): JsonResponse
    {
        $testpasscode = $_ENV['TESTINGPASSCODE'];
        if (!$testpasscode || empty($testpasscode)) {
            return $this->json(['error' => 'Could not read environment variable'], Response::HTTP_INTERNAL_SERVER_ERROR);
        }

        // Find the spraywall to ensure it exists
        $spraywall = $this->spraywallRepository->find($id);
        if (!$spraywall) {
            return $this->json(['error' => 'Spraywall not found'], Response::HTTP_NOT_FOUND);
        }

        // Get and validate request data
        $data = json_decode($request->getContent(), true);
        
        if (!isset($data['tempPwd']) || $data['tempPwd'] !== $_ENV['TESTINGPASSCODE']) {
            return $this->json(['error' => 'Invalid temporary password'], Response::HTTP_UNAUTHORIZED);
        }
        
        if (!$data || !isset($data['name']) || empty(trim($data['name']))) {
            return $this->json(['error' => 'Name is required and cannot be empty'], Response::HTTP_BAD_REQUEST);
        }

        if (!isset($data['image']) || empty($data['image'])) {
            return $this->json(['error' => 'Image is required'], Response::HTTP_BAD_REQUEST);
        }

        // Validate and process base64 image
        $imageData = $data['image'];
        if (!preg_match('/^data:image\/png;base64,(.+)$/', $imageData, $matches)) {
            return $this->json(['error' => 'Image must be a valid base64 PNG string with data:image/png;base64, prefix'], Response::HTTP_BAD_REQUEST);
        }

        $base64Data = $matches[1];
        $binaryData = base64_decode($base64Data, true);
        
        if ($binaryData === false) {
            return $this->json(['error' => 'Invalid base64 image data'], Response::HTTP_BAD_REQUEST);
        }

        // Create new SpraywallProblem
        $spraywallProblem = new SpraywallProblem();
        $spraywallProblem->setName(trim($data['name']));
        $spraywallProblem->setSpraywall($spraywall);
        
        if (isset($data['description'])) {
            $spraywallProblem->setDescription($data['description']);
        }

        // Save to database first to get the ID
        $this->spraywallProblemRepository->addProblem($spraywallProblem);

        // Save image file using the generated problem ID
        try {
            $spraywallDir = "spraywalls/{$id}";
            if (!$this->filesystem->exists($spraywallDir)) {
                $this->filesystem->mkdir($spraywallDir);
            }
            
            // Convert 32-bit PNG to 24-bit PNG using ImageMagick
            $processedImageData = $this->convertTo24BitPng($binaryData);
            
            $imagePath = "{$spraywallDir}/{$spraywallProblem->getId()}.png";
            $this->filesystem->dumpFile($imagePath, $processedImageData);
            
        } catch (IOExceptionInterface $exception) {
            $this->spraywallProblemRepository->removeProblem($spraywallProblem);
            return $this->json(['error' => 'Failed to save image: ' . $exception->getMessage()], Response::HTTP_INTERNAL_SERVER_ERROR);
        }

        // Return the created problem with 201 status
        return $this->getProblem($id, $spraywallProblem->getId())->setStatusCode(201);
    }

    private function getSpraywallProblemImage($spraywallId, $spraywallProblemId): string {
        $contents = $this->filesystem->readFile("spraywalls/{$spraywallId}/{$spraywallProblemId}.png");
        return base64_encode($contents);
    }

    private function convertTo24BitPng(string $binaryData): string {
        if (!extension_loaded('gd')) {
            throw new \RuntimeException('GD extension is not available. Please install php-gd.');
        }

        try {
            $sourceImage = imagecreatefromstring($binaryData);
            
            if ($sourceImage === false) {
                throw new \InvalidArgumentException('Invalid image data - could not create image from string');
            }
            
            $width = 128;
            $height = 128;
            
            // Create a new 24-bit image (without alpha channel)
            $newImage = imagecreatetruecolor($width, $height);
            
            if ($newImage === false) {
                imagedestroy($sourceImage);
                throw new \RuntimeException('Failed to create new image canvas');
            }
            
            // Disable alpha blending for the destination image
            imagealphablending($newImage, false);
            
            $backgroundColor = imagecolorallocate($newImage, 0, 0, 0);
            
            if ($backgroundColor === false) {
                imagedestroy($sourceImage);
                imagedestroy($newImage);
                throw new \RuntimeException('Failed to allocate background color');
            }
            
            imagefill($newImage, 0, 0, $backgroundColor);
            
            // Enable alpha blending for copying the source image
            imagealphablending($newImage, true);
            
            // Copy the source image onto the new image
            // This will flatten any transparency against the background color
            $copyResult = imagecopy($newImage, $sourceImage, 0, 0, 0, 0, $width, $height);
            
            if (!$copyResult) {
                imagedestroy($sourceImage);
                imagedestroy($newImage);
                throw new \RuntimeException('Failed to copy source image to new canvas');
            }
            
            // Capture the PNG output using imagepng
            ob_start();
            $pngResult = imagepng($newImage, null, 9); // 9 = maximum compression level
            
            if (!$pngResult) {
                ob_end_clean();
                imagedestroy($sourceImage);
                imagedestroy($newImage);
                throw new \RuntimeException('Failed to generate PNG output');
            }
            
            $processedImageData = ob_get_contents();
            ob_end_clean();
            
            // Clean up memory
            imagedestroy($sourceImage);
            imagedestroy($newImage);
            
            if ($processedImageData === false || empty($processedImageData)) {
                throw new \RuntimeException('Generated PNG data is empty or invalid');
            }
            
            return $processedImageData;
            
        } catch (\Exception $e) {
            throw new \RuntimeException('Failed to process image with GD: ' . $e->getMessage());
        }
    }

}
