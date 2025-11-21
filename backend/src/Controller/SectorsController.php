<?php

namespace App\Controller;

use App\DTO\SectorDto;
use App\DTO\ErrorDto;
use App\Repository\SectorRepository;
use Nelmio\ApiDocBundle\Attribute\Model;
use Symfony\Bundle\FrameworkBundle\Controller\AbstractController;
use Symfony\Component\HttpFoundation\JsonResponse;
use Symfony\Component\Routing\Attribute\Route;
use OpenApi\Attributes as OA;
use Symfony\Component\HttpFoundation\Response;

#[Route('/api', name: '')]
#[OA\Tag(name: "Sector")]
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
        $sectorDtos = [];
        foreach ($sectors as $sector) {
            $sectorDtos[] = new SectorDto(
                $sector->getId(),
                $sector->getName(),
                $sector->getDescription()
            );
        }

        return $this->json($sectorDtos, Response::HTTP_OK);
    }



    #[Route('/sectors/{id}', name: 'sector', methods: ['GET'])]
    #[OA\Response(
        response: Response::HTTP_OK,
        description: 'Returns a sector by ID',
        content: new OA\JsonContent(ref: new Model(type: SectorDto::class))
    )]
    #[OA\Response(
        response: Response::HTTP_NOT_FOUND,
        description: 'Sector not found',
        content: new OA\JsonContent(ref: new Model(type: ErrorDto::class))
    )]
    public function getSector($id): JsonResponse
    {
        $sector = $this->sectorRepository->findById($id);
        if (!$sector) {
            return $this->json(new ErrorDto('Sector not found', null), Response::HTTP_NOT_FOUND);
        }
        $sectorDto = new SectorDto(
            $sector->getId(),
            $sector->getName(),
            $sector->getDescription()
        );
        return $this->json($sectorDto, Response::HTTP_OK);
    }
}
