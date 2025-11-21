<?php

namespace App\Controller;

use App\DTO\SectorDto;
use App\Repository\SectorRepository;
use Nelmio\ApiDocBundle\Attribute\Model;
use Symfony\Bundle\FrameworkBundle\Controller\AbstractController;
use Symfony\Component\HttpFoundation\JsonResponse;
use Symfony\Component\Routing\Attribute\Route;
use OpenApi\Attributes as OA;
use Symfony\Component\HttpFoundation\Response;

#[Route('/api', name: '')]
class SectorsController extends AbstractController
{
    private $sectorRepository;
    public function __construct(SectorRepository $sectorRepository)
    {
        $this->sectorRepository = $sectorRepository;
    }



    #[Route('/sectors', name: 'sectors', methods: ['GET'])]
    #[OA\Response(
        response: Response::HTTP_OK,
        description: 'Returns a list of sectors',
        content: new OA\JsonContent(
            type: 'array',
            items: new OA\Items(ref: new Model(type: SectorDto::class))
        )
    )]
    public function index(): JsonResponse
    {
        $sectors = $this->sectorRepository->findAll();
        $sectorsArray = [];
        foreach ($sectors as $sector) {
            $sectorDto = new SectorDto(
                $sector->getId(),
                $sector->getName(),
                $sector->getDescription()
            );
            array_push($sectorsArray, get_object_vars($sectorDto));
        }

        return $this->json($sectorsArray, Response::HTTP_OK);
    }



    #[Route('/sectors/{id}', name: 'sector', methods: ['GET'])]
    #[OA\Response(
        response: Response::HTTP_OK,
        description: 'Returns a sector by ID',
        content: new OA\JsonContent(
            type: 'object',
            properties: [
                new OA\Property(property: 'id', type: 'integer'),
                new OA\Property(property: 'name', type: 'string'),
                new OA\Property(property: 'description', type: 'string')
            ]
        )
    )]
    public function getSector($id): JsonResponse
    {
        $sector = $this->sectorRepository->findById($id);
        if (!$sector) {
            return $this->json(['error' => 'Sector not found'], Response::HTTP_NOT_FOUND);
        }
        $sectorDto = new SectorDto(
            $sector->getId(),
            $sector->getName(),
            $sector->getDescription()
        );

        return $this->json(get_object_vars($sectorDto), Response::HTTP_OK);
    }
}
