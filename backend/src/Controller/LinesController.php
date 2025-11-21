<?php

namespace App\Controller;

use App\DTO\LineDto;
use App\Repository\LineRepository;
use Symfony\Bundle\FrameworkBundle\Controller\AbstractController;
use Symfony\Component\HttpFoundation\JsonResponse;
use Symfony\Component\HttpFoundation\Response;
use Symfony\Component\Routing\Attribute\Route;
use OpenApi\Attributes as OA;

#[Route('/api', name: '')]
class LinesController extends AbstractController
{
    private $lineRepository;
    public function __construct(LineRepository $lineRepository)
    {
        $this->lineRepository = $lineRepository;
    }


    #[Route('/lines/by-bloc/{blocId}', name: 'lines', methods: ['GET'])]
    #[OA\Response(
      response: Response::HTTP_OK,
      description: 'Returns the list of lines for the bloc',
      content: new OA\JsonContent(
        type: 'array',
        items: new OA\Items(ref: new \Nelmio\ApiDocBundle\Attribute\Model(type: LineDto::class))
      )
    )]
    public function getLinesByBloc($blocId): JsonResponse
    {
      $lines = $this->lineRepository->findLinesByBlocId($blocId);

      $lineArray = [];
      foreach ($lines as $line) {
        $lineDto = new LineDto(
          $line->getId(),
          $line->getBloc()->getId(),
          $line->getIdentifier(),
          $line->getDescription(),
          $line->getColor(),
          $line->getName()
        );
        array_push($lineArray, get_object_vars($lineDto));
      }

      return $this->json($lineArray, Response::HTTP_OK);
    }
}
