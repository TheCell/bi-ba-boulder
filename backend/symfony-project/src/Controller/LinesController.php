<?php

namespace App\Controller;

use App\Repository\LineRepository;
use Symfony\Bundle\FrameworkBundle\Controller\AbstractController;
use Symfony\Component\HttpFoundation\JsonResponse;
use Symfony\Component\Routing\Attribute\Route;

#[Route('/api', name: '')]
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
        // todo
        $lines = $this->lineRepository->findBy(['bloc' => $blocId]);
        dd($lines);

        return $this->json([
            'message' => 'Welcome to your new controller!',
            'path' => 'src/Controller/LinesController.php',
        ]);
    }
}
