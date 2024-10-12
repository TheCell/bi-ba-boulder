<?php

namespace App\Controller;

use App\Repository\SectorRepository;
use Symfony\Bundle\FrameworkBundle\Controller\AbstractController;
use Symfony\Component\HttpFoundation\JsonResponse;
use Symfony\Component\Routing\Attribute\Route;

#[Route('/api', name: 'api_')]
class SectorsController extends AbstractController
{
    private $sectorRepository;
    public function __construct(SectorRepository $sectorRepository)
    {
        $this->sectorRepository = $sectorRepository;
    }

    #[Route('/sectors', name: 'get-sectors')]
    public function index(): JsonResponse
    {
        $sectors = $this->sectorRepository->findAll();
        // dd($sectors);
        $sectorsArray = [];
        foreach ($sectors as $sector) {
            $sectorsArray[] = [
                'id' => $sector->getId(),
                'name' => $sector->getName(),
                'description' => $sector->getDescription()
            ];
        }

        return $this->json($sectorsArray);
    }

    #[Route('/sectors/{id}', name: 'get-sector', methods: ['GET', 'HEAD'])]
    public function getSector($id): JsonResponse
    {
        $sector = $this->sectorRepository->find($id);

        return $this->json([
          'id' => $sector->getId(),
          'name' => $sector->getName(),
          'description' => $sector->getDescription()
        ]);
    }
}
