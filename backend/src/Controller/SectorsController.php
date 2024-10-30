<?php

namespace App\Controller;

use App\DTO\SectorDto;
use App\Repository\SectorRepository;
use Nelmio\ApiDocBundle\Annotation\Model;
use Symfony\Bundle\FrameworkBundle\Controller\AbstractController;
use Symfony\Component\HttpFoundation\JsonResponse;
use Symfony\Component\Routing\Attribute\Route;
use OpenApi\Attributes as OA;

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
      response: 200,
      description: 'Returns a list of sectors',
      content: new OA\JsonContent(
        type: 'array',
        items: new OA\Items(ref: new Model(type: SectorDto::class))
      )
    )]
    public function index(): JsonResponse
    {
        $sectors = $this->sectorRepository->findAll();
        // dd($sectors);
        $sectorsArray = [];
        foreach ($sectors as $sector) {
            $sectorDto = new SectorDto (
                $sector->getId(),
                $sector->getName(),
                $sector->getDescription()
            );
            array_push($sectorsArray, get_object_vars($sectorDto));
        }

        return $this->json($sectorsArray);
    }

    #[Route('/sectors/{id}', name: 'sector', methods: ['GET'])]
    #[OA\Response(
      response: 200,
      description: 'Returns a list of sectors',
      content: new OA\JsonContent(
        type: 'array',
        items: new OA\Items(ref: new Model(type: SectorDto::class))
      )
    )]
    public function getSector($id): JsonResponse
    {
        $sector = $this->sectorRepository->findById($id);
        $sectorDto = new SectorDto (
            $sector->getId(),
            $sector->getName(),
            $sector->getDescription()
        );

        return $this->json(get_object_vars($sectorDto));
    }
}
