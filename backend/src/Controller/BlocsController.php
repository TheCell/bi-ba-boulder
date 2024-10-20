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

    #[Route('/blocs/by-sector/{id}', name: 'blocs-by-sector-id', methods: ['GET'])]
    #[OA\Response(
      response: 200,
      description: 'Returns the list of blocs for the sector',
      content: new OA\JsonContent(
        type: 'array',
        items: new OA\Items(ref: new Model(type: BlocDto::class))
      )
    )]
    public function getBlocsBySectorId($id): JsonResponse
    {
        $blocs = $this->blocRepository->findBySectorId($id);

        $blocsArray = [];
        foreach ($blocs as $bloc) {
            $blocDto = new BlocDto (
                $bloc->getId(),
                $bloc->getName(),
                $bloc->getDescription(),
                $bloc->getBlocLowRes(),
                $bloc->getBlocMedRes(),
                $bloc->getBlocHighRes()
            );
            array_push($blocsArray, get_object_vars($blocDto));
        }

        return $this->json($blocsArray);
    }

    #[Route('/blocs/{id}', name: 'bloc', methods: ['GET'])]
    public function getBloc($id): JsonResponse
    {
        $bloc = $this->blocRepository->findById($id);

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
