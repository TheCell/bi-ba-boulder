<?php

namespace App\Controller;

use App\DTO\BlocDto;
use App\Repository\BlocRepository;
use Nelmio\ApiDocBundle\Attribute\Model;
use Symfony\Bundle\FrameworkBundle\Controller\AbstractController;
use Symfony\Component\HttpFoundation\JsonResponse;
use Symfony\Component\HttpFoundation\Response;
use Symfony\Component\Routing\Attribute\Route;
use OpenApi\Attributes as OA;

#[Route('/api', name: '')]
class BlocsController extends AbstractController
{
    private $blocRepository;
    public function __construct(BlocRepository $blocRepository)
    {
        $this->blocRepository = $blocRepository;
    }

    #[Route('/blocs/by-sector/{id}', name: 'blocs-by-sector-id', methods: ['GET'])]
    #[OA\Response(
        response: Response::HTTP_OK,
        description: 'Returns the list of blocs for the sector',
        content: new OA\JsonContent(
            type: 'array',
            items: new OA\Items(ref: new Model(type: BlocDto::class))
        )
    )]
    public function getBlocsBySectorId($id): JsonResponse
    {
        $blocs = $this->blocRepository->findBySectorId($id);

        $blocDtos = [];
        foreach ($blocs as $bloc) {
            $blocDtos[] = new BlocDto(
                $bloc->getId(),
                $bloc->getName(),
                $bloc->getDescription(),
                $bloc->getBlocLowRes(),
                $bloc->getBlocMedRes(),
                $bloc->getBlocHighRes()
            );
        }

        return $this->json($blocDtos, Response::HTTP_OK);
    }

    #[Route('/blocs/{id}', name: 'bloc', methods: ['GET'])]
    #[OA\Response(
      response: Response::HTTP_OK,
      description: 'Returns a bloc by ID',
      content: new OA\JsonContent(
        type: 'object',
        properties: [
            new OA\Property(property: 'id', type: 'integer'),
            new OA\Property(property: 'name', type: 'string'),
            new OA\Property(property: 'description', type: 'string'),
            new OA\Property(property: 'blocLowRes', type: 'string'),
            new OA\Property(property: 'blocMedRes', type: 'string'),
            new OA\Property(property: 'blocHighRes', type: 'string')
        ]
      )
    )]
    public function getBloc($id): JsonResponse
    {
        $bloc = $this->blocRepository->findById($id);
        if (!$bloc) {
            return $this->json(['error' => 'Bloc not found'], Response::HTTP_NOT_FOUND);
        }
        return $this->json([
            'id' => $bloc->getId(),
            'name' => $bloc->getName(),
            'description' => $bloc->getDescription(),
            'blocLowRes' => $bloc->getBlocLowRes(),
            'blocMedRes' => $bloc->getBlocMedRes(),
            'blocHighRes' => $bloc->getBlocHighRes(),
        ], Response::HTTP_OK);
    }
}
