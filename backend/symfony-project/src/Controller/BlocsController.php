<?php

namespace App\Controller;

use App\Repository\BlocRepository;
use Symfony\Bundle\FrameworkBundle\Controller\AbstractController;
use Symfony\Component\HttpFoundation\JsonResponse;
use Symfony\Component\Routing\Attribute\Route;

class BlocsController extends AbstractController
{
    private $blocRepository;
    public function __construct(BlocRepository $blocRepository)
    {
        $this->blocRepository = $blocRepository;
    }

    #[Route('/blocs', name: 'get-blocs')]
    public function getBlocs(): JsonResponse
    {
        $blocs = $this->blocRepository->findAll();

        $blocsArray = [];
        foreach ($blocs as $bloc) {
            $blocsArray[] = [
                'id' => $bloc->getId(),
                'name' => $bloc->getName(),
                'description' => $bloc->getDescription()
            ];
        }

        return $this->json($blocsArray);
    }

    #[Route('/blocs/{id}', name: 'get-bloc', methods: ['GET', 'HEAD'])]
    public function getBloc($id): JsonResponse
    {
        $bloc = $this->blocRepository->find($id);

        return $this->json([
          'id' => $bloc->getId(),
          'name' => $bloc->getName(),
          'description' => $bloc->getDescription()
        ]);
    }
}
