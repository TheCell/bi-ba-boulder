<?php

namespace App\Controller;

use App\DTO\BlocDto;
use App\Repository\BlocRepository;
use App\DTO\ErrorDto;
use Nelmio\ApiDocBundle\Attribute\Model;
use Symfony\Component\Uid\Uuid;
use Symfony\Bundle\FrameworkBundle\Controller\AbstractController;
use Symfony\Component\HttpFoundation\JsonResponse;
use Symfony\Component\HttpFoundation\Response;
use Symfony\Component\Routing\Attribute\Route;
use OpenApi\Attributes as OA;

#[Route('/api/blocs', name: '')]
#[OA\Tag(name: "Bloc")]
class BlocsController extends AbstractController
{
    private $blocRepository;
    public function __construct(BlocRepository $blocRepository)
    {
        $this->blocRepository = $blocRepository;
    }

    #[Route('/by-sector/{id}', name: 'blocs-by-sector-id', methods: ['GET'])]
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
        $blocs = $this->blocRepository->findBySectorId(Uuid::fromString($id));

        // var_dump($blocs);
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

    #[Route('/{id}', name: 'bloc', methods: ['GET'])]
    #[OA\Response(
    response: Response::HTTP_OK,
    description: 'Returns a bloc by ID',
    content: new OA\JsonContent(ref: new Model(type: BlocDto::class))
    )]
    public function getBloc($id): JsonResponse
    {
        $bloc = $this->blocRepository->findById(Uuid::fromString($id));
        if (!$bloc) {
            return $this->json(new ErrorDto('Bloc not found', null), Response::HTTP_NOT_FOUND);
        }

        $blocDto = new BlocDto(
            $bloc->getId(),
            $bloc->getName(),
            $bloc->getDescription(),
            $bloc->getBlocLowRes(),
            $bloc->getBlocMedRes(),
            $bloc->getBlocHighRes()
        );
        return $this->json($blocDto, Response::HTTP_OK);
    }
}
