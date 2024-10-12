<?php

namespace App\Controller;

use App\DTO\BlocDto;
use App\Repository\BlocRepository;
use Nelmio\ApiDocBundle\Annotation\Model;
use Symfony\Bundle\FrameworkBundle\Controller\AbstractController;
use Symfony\Component\HttpFoundation\JsonResponse;
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

    #[Route('/blocs', name: 'blocs', methods: ['GET'])]
    #[OA\Response(
      response: 200,
      description: 'Returns the list of blocs',
      content: new OA\JsonContent(
        type: 'array',
        items: new OA\Items(ref: new Model(type: BlocDto::class))
      )
    )]
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

    #[Route('/blocs/{id}', name: 'bloc', methods: ['GET'])]
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
