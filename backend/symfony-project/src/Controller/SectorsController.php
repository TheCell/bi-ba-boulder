<?php

namespace App\Controller;

use App\DTO\SectorDto;
use App\Repository\SectorRepository;
use Symfony\Bundle\FrameworkBundle\Controller\AbstractController;
use Symfony\Component\HttpFoundation\JsonResponse;
use Symfony\Component\Routing\Attribute\Route;
// use Symfony\Component\Serializer\Encoder\JsonEncoder;
// use Symfony\Component\Serializer\Encoder\XmlEncoder;
// use Symfony\Component\Serializer\Normalizer\ObjectNormalizer;
// use Symfony\Component\Serializer\Serializer;

// $encoders = [new XmlEncoder(), new JsonEncoder()];
// $normalizers = [new ObjectNormalizer()];
// $serializer = new Serializer($normalizers, $encoders);

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
        dd($sectors);

        return $this->json([
            'message' => 'Welcome to your new controller!',
            'path' => 'src/Controller/SectorsController.php',
        ]);
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
