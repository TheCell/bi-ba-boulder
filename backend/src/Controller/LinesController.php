<?php

namespace App\Controller;

use App\DTO\LineDto;
use App\Repository\LineRepository;
use Symfony\Bundle\FrameworkBundle\Controller\AbstractController;
use Symfony\Component\HttpFoundation\JsonResponse;
use Symfony\Component\Routing\Attribute\Route;

class LinesController extends AbstractController
{
    private $lineRepository;
    public function __construct(LineRepository $lineRepository)
    {
        $this->lineRepository = $lineRepository;
    }

    #[Route('/lines/by-bloc/{blocId}', name: 'lines', methods: ['GET'])]
    public function getLinesByBloc($blocId): JsonResponse
    {
        $lines = $this->lineRepository->findLinesByBlocId($blocId);
        // dd($lines);

        $lineArray = [];
        foreach ($lines as $line) {
            $lineDto = new LineDto (
                $line->getId(),
                $line->getBloc().getId(),
                $line->getIdentifier(),
                $line->getDescription(),
                $line->getColor(),
                $line->getName()
            );
            array_push($sectorsArray, get_object_vars($lineDto));
        }

        return $this->json($lineArray);
    }
}
