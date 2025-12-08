<?php

namespace App\Controller;

use App\Entity\User;
use App\DTO\ErrorDto;
use App\Entity\SpraywallProblem;
use App\DTO\SpraywallDto;
use App\DTO\SpraywallProblemDto;
use App\DTO\SpraywallProblemSearchDto;
use App\Entity\Enum\FontGrade;
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
use Symfony\Component\Uid\Uuid;
use Symfony\Component\Security\Http\Attribute\CurrentUser;
use Symfony\Component\Security\Http\Attribute\IsGranted;
use Symfony\Component\RateLimiter\RateLimiterFactoryInterface;

#[Route('/api/spraywalls', name: '')]
#[OA\Tag(name: "Spraywalls")]
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

    #[Route('', name: 'spraywalls', methods: ['GET'])]
    #[OA\Response(
        response: Response::HTTP_OK,
        description: 'Returns a spraywall problem',
        content: new OA\JsonContent(
            type: 'array',
            items: new OA\Items(ref: new Model(type: SpraywallProblemDto::class))
        )
    )]
    public function getSpraywalls(): JsonResponse
    {
        $spraywalls = $this->spraywallRepository->findAll();

        $spraywallsDto = [];
        foreach ($spraywalls as $spraywall) {
            $spraywallsDto[] = new SpraywallDto(
            $spraywall->getId(),
            $spraywall->getName(),
            $spraywall->getDescription()
          );
        }
        
        return $this->json($spraywallsDto, Response::HTTP_OK);
    }

    #[Route('/{id}/problems/{problemId}', name: 'problem', methods: ['GET'])]
    #[OA\Response(
        response: Response::HTTP_OK,
        description: 'Returns a spraywall problem',
        content: new OA\JsonContent(ref: new Model(type: SpraywallProblemDto::class))
    )]
    public function getProblem($id, $problemId): JsonResponse
    {
        $spraywallProblem = $this->spraywallProblemRepository->find($problemId);

        if (!$spraywallProblem) {
            return $this->json(['error' => 'Problem not found'], Response::HTTP_NOT_FOUND);
        }

        $spraywallProblemDto = new SpraywallProblemDto(
            $spraywallProblem->getId(),
            $spraywallProblem->getName(),
            $this->getSpraywallProblemImage($id, $spraywallProblem->getId()),
            $spraywallProblem->getFontGrade()->getValue(),
            $spraywallProblem->getCreatedBy()->getId(),
            $spraywallProblem->getCreatedBy()->getUsername(),
            $spraywallProblem->getCreatedDate(),
            $spraywallProblem->getDescription()
          );

        return $this->json($spraywallProblemDto, Response::HTTP_OK);
    }

    #[Route('/{id}/problems', name: 'search_problems', methods: ['POST'])]
    #[OA\RequestBody(
        required: false,
        content: new OA\JsonContent(
            type: 'object',
            properties: [
                new OA\Property(property: 'gradeMin', type: 'integer', description: 'min Font grade'),
                new OA\Property(property: 'gradeMax', type: 'integer', description: 'max Font grade'),
                new OA\Property(property: 'name', type: 'string', description: 'Problem name'),
                new OA\Property(property: 'creator', type: 'string', description: 'Creator username or ID'),
                new OA\Property(property: 'dateOrder', type: 'string', description: 'asc or desc')
            ]
        )
    )]
    #[OA\Response(
        response: Response::HTTP_OK,
        description: 'Returns paginated spraywall problems',
        content: new OA\JsonContent(ref: new Model(type: SpraywallProblemSearchDto::class))
    )]
    public function searchProblems(Request $request, Uuid $id): JsonResponse
    {
        $data = json_decode($request->getContent(), true) ?? [];
        $gradeMin = $data['gradeMin'] ?? null;
        $gradeMax = $data['gradeMax'] ?? null;
        $name = $data['name'] ?? null;
        $creator = $data['creator'] ?? null;
        $dateOrder = $data['dateOrder'] ?? 'desc';
        $page = isset($data['page']) ? max(1, (int)$data['page']) : 1;
        $pageSize = 30;

        $criteria = [];
        if ($gradeMin) {
            $criteria['gradeMin'] = $gradeMin;
        }
        if ($gradeMax) {
            $criteria['gradeMax'] = $gradeMax;
        }
        if ($name) {
            $criteria['name'] = $name;
        }
        if ($creator) {
            $criteria['createdBy'] = $creator;
        }
        if ($dateOrder) {
            $criteria['dateOrder'] = $dateOrder;
        }

        $offset = ($page - 1) * $pageSize;
        $filterByCriteria = $this->spraywallProblemRepository->filterByCriteria($id, $pageSize, $offset, $criteria);
        list($problems, $totalCount) = $filterByCriteria;

        $problemsDto = [];
        foreach ($problems as $problem) {
            $problemsDto[] = new SpraywallProblemDto(
                $problem->getId(),
                $problem->getName(),
                $this->getSpraywallProblemImage($problem->getSpraywall()->getId(), $problem->getId()),
                $problem->getFontGrade()->getValue(),
                $problem->getCreatedBy()->getId(),
                $problem->getCreatedBy()->getUsername(),
                $problem->getCreatedDate(),
                $problem->getDescription()
            );
        }

        $problemSearchDto = new SpraywallProblemSearchDto(
            totalCount: $totalCount,
            currentPage: $page,
            problems: $problemsDto
        );

        return $this->json($problemSearchDto, Response::HTTP_OK);
    }

    #[Route('/{id}/problem', name: 'create', methods: ['PUT'])]
    #[IsGranted('ROLE_EDITOR')]
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
                  property: 'fontGrade',
                  type: 'integer',
                  description: 'Font grade of the spraywall problem',
                  nullable: true
              )
          ],
          required: ['name', 'image', 'fontGrade']
      )
    )]
    #[OA\Response(
        response: Response::HTTP_CREATED,
        description: 'Returns the created spraywall problem',
        content: new OA\JsonContent(ref: new Model(type: SpraywallProblemDto::class))
    )]
    public function createProblem(Uuid $id, Request $request, #[CurrentUser] ?User $currentUser): JsonResponse
    {
        $spraywall = $this->spraywallRepository->findOneBy(['id' => $id]);
        if (!$spraywall) {
            return $this->json(['error' => 'Spraywall not found'], Response::HTTP_NOT_FOUND);
        }

        // Get and validate request data
        $data = json_decode($request->getContent(), true);
        
        if (!$data || !isset($data['name']) || empty(trim($data['name']))) {
            return $this->json(new ErrorDto('Name is required and cannot be empty', null), Response::HTTP_BAD_REQUEST);
        }

        if (!isset($data['image']) || empty($data['image'])) {
           return $this->json(new ErrorDto('Image is required', null), Response::HTTP_BAD_REQUEST);
        }

        if (!isset($data['fontGrade']) || FontGrade::tryFrom($data['fontGrade']) == null) {
            return $this->json(new ErrorDto('Font grade is required', null), Response::HTTP_BAD_REQUEST);
        }

        // Validate and process base64 image
        $imageData = $data['image'];
        if (!preg_match('/^data:image\/png;base64,(.+)$/', $imageData, $matches)) {
            return $this->json(new ErrorDto('Image must be a valid base64 PNG string with data:image/png;base64, prefix', null), Response::HTTP_BAD_REQUEST);
        }

        $base64Data = $matches[1];
        $binaryData = base64_decode($base64Data, true);
        
        if ($binaryData === false) {
            return $this->json(new ErrorDto('Invalid base64 image data', null), Response::HTTP_BAD_REQUEST);
        }

        // Create new SpraywallProblem
        $spraywallProblem = new SpraywallProblem();
        $spraywallProblem->setName(trim($data['name']));
        $spraywallProblem->setSpraywall($spraywall);
        $spraywallProblem->setCreatedDate(new \DateTime());
        $spraywallProblem->setCreatedBy($currentUser);
        
        if (isset($data['description'])) {
            $spraywallProblem->setDescription($data['description']);
        }

        if (isset($data['fontGrade'])) {
            $grade = $data['fontGrade'];
            if ($grade === null) {
                return $this->json(new ErrorDto('Invalid font grade value. Could not find matching enum for ' . $data['fontGrade'], null), Response::HTTP_BAD_REQUEST);
            }
            $spraywallProblem->setFontGrade(FontGrade::tryFrom($grade));
        }

        // Save to database first to get the ID
        $this->spraywallProblemRepository->addProblem($spraywallProblem);

        // Save image file using the generated problem ID
        try {
            $spraywallDir = 'spraywalls' . DIRECTORY_SEPARATOR . $id;
            if (!$this->filesystem->exists($spraywallDir)) {
                $this->filesystem->mkdir($spraywallDir);
            }

            // Convert 32-bit PNG to 24-bit PNG using ImageMagick
            $processedImageData = $this->convertTo24BitPng($binaryData);

            $imagePath = $spraywallDir . DIRECTORY_SEPARATOR . $spraywallProblem->getId() . '.png';
            $this->filesystem->dumpFile($imagePath, $processedImageData);
            
        } catch (IOExceptionInterface $exception) {
            $this->spraywallProblemRepository->removeProblem($spraywallProblem);
            return $this->json(new ErrorDto('Failed to save image: ' . $exception->getMessage(), null), Response::HTTP_INTERNAL_SERVER_ERROR);
        }

        // Return the created problem with 201 status
        return $this->getProblem($id, $spraywallProblem->getId())->setStatusCode(Response::HTTP_CREATED);
    }

    #[Route('/{id}/problem/{problemId}', name: 'problem', methods: ['DELETE'])]
    #[IsGranted('ROLE_ADMIN')]
    public function deleteProblem(Uuid $id, Uuid $problemId, RateLimiterFactoryInterface $anonymousApiLimiter): JsonResponse
    {
        $spraywallProblem = $this->spraywallProblemRepository->find($problemId);

        if (!$spraywallProblem) {
            return $this->json(['error' => 'Problem not found'], Response::HTTP_NOT_FOUND);
        }

        try {
            // Delete image file
            $imagePath = 'spraywalls' . DIRECTORY_SEPARATOR . $id . DIRECTORY_SEPARATOR . $problemId . '.png';
            if ($this->filesystem->exists($imagePath)) {
                $this->filesystem->remove($imagePath);
            }

            // Remove problem from database
            $this->spraywallProblemRepository->removeProblem($spraywallProblem);

        } catch (IOExceptionInterface $exception) {
            return $this->json(['error' => 'Failed to delete problem: ' . $exception->getMessage()], Response::HTTP_INTERNAL_SERVER_ERROR);
        }

        $this->spraywallProblemRepository->removeProblem($spraywallProblem);
        
        return $this->json(null, Response::HTTP_NO_CONTENT);
    }


    private function getSpraywallProblemImage(Uuid $spraywallId, Uuid $spraywallProblemId): string {
        $imagePath = 'spraywalls' . DIRECTORY_SEPARATOR . $spraywallId . DIRECTORY_SEPARATOR . $spraywallProblemId . '.png';
        $contents = $this->filesystem->readFile($imagePath);
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
                $sourceImage = null;
                throw new \RuntimeException('Failed to create new image canvas');
            }
            
            // Disable alpha blending for the destination image
            imagealphablending($newImage, false);
            
            $backgroundColor = imagecolorallocate($newImage, 0, 0, 0);
            
            if ($backgroundColor === false) {
                $sourceImage = null;
                $newImage = null;
                throw new \RuntimeException('Failed to allocate background color');
            }
            
            imagefill($newImage, 0, 0, $backgroundColor);
            
            // Enable alpha blending for copying the source image
            imagealphablending($newImage, true);
            
            // Copy the source image onto the new image
            // This will flatten any transparency against the background color
            $copyResult = imagecopy($newImage, $sourceImage, 0, 0, 0, 0, $width, $height);
            
            if (!$copyResult) {
                $sourceImage = null;
                $newImage = null;
                throw new \RuntimeException('Failed to copy source image to new canvas');
            }
            
            // Capture the PNG output using imagepng
            ob_start();
            $pngResult = imagepng($newImage, null, 9); // 9 = maximum compression level
            
            if (!$pngResult) {
                ob_end_clean();
                $sourceImage = null;
                $newImage = null;
                throw new \RuntimeException('Failed to generate PNG output');
            }
            
            $processedImageData = ob_get_contents();
            ob_end_clean();
            
            // Clean up memory
            $sourceImage = null;
            $newImage = null;
            
            if ($processedImageData === false || empty($processedImageData)) {
                throw new \RuntimeException('Generated PNG data is empty or invalid');
            }
            
            return $processedImageData;
            
        } catch (\Exception $e) {
            throw new \RuntimeException('Failed to process image with GD: ' . $e->getMessage());
        }
    }

}
