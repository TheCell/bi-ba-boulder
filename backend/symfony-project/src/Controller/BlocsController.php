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

    #[Route('/blocs', name: 'app_blocs')]
    public function index(): JsonResponse
    {
        $blocs = $this->blocRepository->findAll();
        dd($blocs); // dd is a helper function that dumps the given variables and ends execution of the script

        return $this->json($blocs);
    }
}
