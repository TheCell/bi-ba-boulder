<?php

namespace App\Controller;

use App\Entity\User;
use App\DTO\ErrorDto;
use App\Entity\SpraywallProblem;
use App\DTO\SpraywallDto;
use App\DTO\SpraywallProblemDto;
use App\DTO\SpraywallProblemSearchDto;
use App\Entity\Enum\FontGrade;
use App\ImageModification\ImageModification;
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
use App\Mapper\Mapper;

#[Route('/api/spraywall-problems', name: '')]
#[OA\Tag(name: "SpraywallProblems")]
final class SpraywallProblemController extends AbstractController
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

    #[Route('/{id}', name: 'update_spraywall_problem', methods: ['POST'])]
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
          required: ['name', 'image']
      )
    )]
    #[OA\Response(
        response: Response::HTTP_OK,
        description: 'Returns the updated spraywall problem',
        content: new OA\JsonContent(ref: new Model(type: SpraywallProblemDto::class))
    )]
    public function updateProblem(Uuid $id, Request $request, #[CurrentUser] ?User $currentUser): JsonResponse
    {
        $spraywallProblem = $this->spraywallProblemRepository->findOneBy(['id' => $id]);
        if (!$spraywallProblem) {
            return $this->json(['error' => 'Spraywallproblem not found'], Response::HTTP_NOT_FOUND);
        }

        if (null === $currentUser || $currentUser->getId() === null || $currentUser->getId() !== $spraywallProblem->getCreatedBy()?->getId()) {
            return $this->json(new ErrorDto('unauthorized', $currentUser), Response::HTTP_UNAUTHORIZED);
        }

        $spraywallId = $spraywallProblem->getSpraywall()?->getId();
        if ($spraywallId === null) {
            return $this->json(new ErrorDto('Spraywall not found for the problem', null), Response::HTTP_INTERNAL_SERVER_ERROR);
        }
        
        // Get and validate request data
        $data = json_decode($request->getContent(), true);
        
        if (!$data || !isset($data['name']) || empty(trim($data['name']))) {
            return $this->json(new ErrorDto('Name is required and cannot be empty', null), Response::HTTP_BAD_REQUEST);
        }

        if (!isset($data['image']) || empty($data['image'])) {
            return $this->json(new ErrorDto('Image is required', null), Response::HTTP_BAD_REQUEST);
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

        $spraywallProblem->setName(trim($data['name']));
        $spraywallProblem->setDescription($data['description']);

        if (isset($data['fontGrade'])) {
            $grade = $data['fontGrade'];
            if ($grade === null) {
                return $this->json(new ErrorDto('Invalid font grade value. Could not find matching enum for ' . $data['fontGrade'], null), Response::HTTP_BAD_REQUEST);
            }
            $spraywallProblem->setFontGrade(FontGrade::tryFrom($grade));
        }
        else
        {
            $spraywallProblem->setFontGrade(null);
        }

        $this->spraywallProblemRepository->updateProblem();
        
        // Update image file
        try {
            $spraywallDir = 'spraywalls' . DIRECTORY_SEPARATOR . $spraywallId;
            if (!$this->filesystem->exists($spraywallDir)) {
                throw new \RuntimeException('Spraywall directory does not exist: ' . $spraywallDir);
            }

            // Convert 32-bit PNG to 24-bit PNG using ImageMagick
            $processedImageData = ImageModification::compressAndConvertTo24BitPng($binaryData);

            $imagePath = $spraywallDir . DIRECTORY_SEPARATOR . $spraywallProblem->getId() . '.png';
            $this->filesystem->dumpFile($imagePath, $processedImageData);
        } catch (IOExceptionInterface $exception) {
            return $this->json(new ErrorDto('Failed to save image: ' . $exception->getMessage(), null), Response::HTTP_INTERNAL_SERVER_ERROR);
        }

        // Return the created problem with 201 status
        // return $this->getProblem($id, $spraywallProblem->getId())->setStatusCode(Response::HTTP_CREATED);
        return $this->json(['message' => 'Problem updated successfully'], Response::HTTP_OK);
    }

    #[Route('/{id}', name: 'problem', methods: ['GET'])]
    #[OA\Response(
        response: Response::HTTP_OK,
        description: 'Returns a spraywall problem',
        content: new OA\JsonContent(ref: new Model(type: SpraywallProblemDto::class))
    )]
    public function getProblem($id, #[CurrentUser] ?User $currentUser): JsonResponse
    {
        $spraywallProblem = $this->spraywallProblemRepository->find($id);

        if (!$spraywallProblem) {
            return $this->json(['error' => 'Problem not found'], Response::HTTP_NOT_FOUND);
        }

        $problem = Mapper::getProblem($spraywallProblem, $currentUser, $this->filesystem);
        return $this->json($problem, Response::HTTP_OK);
    }
    
    #[Route('/{id}', name: 'delete_spraywall_problem', methods: ['DELETE'])]
    #[OA\Response(
        response: Response::HTTP_OK,
        description: 'Deletes a spraywall problem',
        content: new OA\JsonContent()
    )]
    public function deleteProblem(Uuid $id, #[CurrentUser] ?User $currentUser): JsonResponse
    {
        $spraywallProblem = $this->spraywallProblemRepository->findById($id);
        if (!$spraywallProblem) {
            return $this->json(['error' => 'Spraywallproblem not found'], Response::HTTP_NOT_FOUND);
        }

        if ($currentUser === null || $currentUser->getId() === null) {
            return $this->json(new ErrorDto('Unauthorized', $currentUser), Response::HTTP_UNAUTHORIZED);
        }

        if ($currentUser->getId() === $spraywallProblem->getCreatedBy()?->getId() && !in_array('ROLE_EDITOR', $currentUser->getRoles())
            && !in_array('ROLE_ADMIN', $currentUser->getRoles())
            ) {
            return $this->json(new ErrorDto('Not your Creation', null), Response::HTTP_UNAUTHORIZED);
        }

        $spraywallId = $spraywallProblem->getSpraywall()?->getId();
        if ($spraywallId === null) {
            return $this->json(new ErrorDto('Spraywall not found for the problem', null), Response::HTTP_INTERNAL_SERVER_ERROR);
        }

        $this->spraywallProblemRepository->removeProblem($spraywallProblem);

        // Delete image file
        try {
            $spraywallDir = 'spraywalls' . DIRECTORY_SEPARATOR . $spraywallId;
            $imagePath = $spraywallDir . DIRECTORY_SEPARATOR . $id . '.png';
            if ($this->filesystem->exists($imagePath)) {
                $this->filesystem->remove($imagePath);
            }
        } catch (IOExceptionInterface $exception) {
            return $this->json(new ErrorDto('Failed to delete image: ' . $exception->getMessage(), null), Response::HTTP_INTERNAL_SERVER_ERROR);
        }

        return $this->json(['message' => 'Problem deleted successfully'], Response::HTTP_OK);
    }
}
