<?php

namespace App\Controller;

use ApiPlatform\Metadata\ApiResource;
use App\DTO\BlocDto;
use Symfony\Component\HttpKernel\Attribute\AsController;
use App\Repository\BlocRepository;
use Symfony\Bundle\FrameworkBundle\Controller\AbstractController;
use Symfony\Component\HttpFoundation\JsonResponse;
use Symfony\Component\Routing\Attribute\Route;

#[AsController]
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
                'description' => $bloc->getDescription(),
                'blocLowRes' => $bloc->getBlocLowRes(),
                'blocMedRes' => $bloc->getBlocMedRes(),
                'blocHighRes' => $bloc->getBlocHighRes(),
            ];
        }

        return $this->json($blocsArray);
    }

    #[Route(
      path: '/blocs/{id}',
      name: 'get-bloc',
      methods: ['GET', 'HEAD'],
      defaults: [
        '_api_resource_class' => BlocDto::class,
        '_api_operation_name' => 'getBloc'
      ])]
    public function getBloc($id): JsonResponse
    {
        $bloc = $this->blocRepository->find($id);

        return $this->json([
          'id' => $bloc->getId(),
          'name' => $bloc->getName(),
          'description' => $bloc->getDescription(),
          'blocLowRes' => $bloc->getBlocLowRes(),
          'blocMedRes' => $bloc->getBlocMedRes(),
          'blocHighRes' => $bloc->getBlocHighRes(),
        ]);
    }
}
