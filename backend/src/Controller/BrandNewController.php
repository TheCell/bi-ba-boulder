<?php

namespace App\Controller;

use Symfony\Bundle\FrameworkBundle\Controller\AbstractController;
use Symfony\Component\HttpFoundation\JsonResponse;
use Symfony\Component\Routing\Attribute\Route;

class BrandNewController extends AbstractController
{
    #[Route('/brand/new', name: 'app_brand_new')]
    public function index(): JsonResponse
    {
        return $this->json([
            'message' => 'Welcome to your new controller! "' . $_ENV['TESTVAR'] . '" "' . $_ENV['APP_ENV'] . '"',
            'path' => 'src/Controller/BrandNewController.php',
        ]);
    }
}
