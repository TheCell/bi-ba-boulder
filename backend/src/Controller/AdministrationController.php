<?php
namespace App\Controller;

use Symfony\Bundle\FrameworkBundle\Controller\AbstractController;
use Symfony\Component\HttpFoundation\JsonResponse;
use Symfony\Component\HttpFoundation\Response;
use Symfony\Component\Routing\Attribute\Route;
use Symfony\Component\Security\Http\Attribute\IsGranted;
use OpenApi\Attributes as OA;

#[Route('/api', name: '')]
#[OA\Tag(name: "Administration")]
#[IsGranted('ROLE_ADMIN')]
final class AdministrationController extends AbstractController
{
    public function __construct()
    {
        // Inject repositories/services here if needed in future
    }

    #[Route('/administration', name: 'app_administration', methods: ['GET'])]
    #[OA\Response(
        response: Response::HTTP_OK,
        description: 'Welcome message for administration endpoint',
        content: new OA\JsonContent(
            type: 'object',
            properties: [
                new OA\Property(property: 'message', type: 'string'),
                new OA\Property(property: 'path', type: 'string')
            ]
        )
    )]
    public function index(): JsonResponse
    {
        return $this->json([
            'message' => 'Welcome to your new controller!',
            'path' => 'src/Controller/AdministrationController.php',
        ], Response::HTTP_OK);
    }
}
