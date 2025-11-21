<?php

namespace App\Controller;

use Symfony\Bundle\FrameworkBundle\Controller\AbstractController;
use Symfony\Component\HttpFoundation\JsonResponse;
use Symfony\Component\Routing\Attribute\Route;
use App\Entity\User;
use Symfony\Component\HttpFoundation\Response;
use Symfony\Component\Security\Http\Attribute\CurrentUser;
use OpenApi\Attributes as OA;

#[Route('/api', name: '')]
final class ApiLoginController extends AbstractController
{
    #[Route('/login', name: 'api_login', methods: ['POST'])]
    #[OA\RequestBody(
      required: true,
      content: new OA\JsonContent(
        type: 'object',
        properties: [
          new OA\Property(
            property: 'username',
            type: 'string',
            description: 'Email or username of the user'
          ),
          new OA\Property(
            property: 'password',
            type: 'string',
            description: 'Password of the user',
            nullable: true
          )
        ],
        required: ['username', 'password']
      )
    )]
    public function login(#[CurrentUser] ?User $user): JsonResponse
    {
        if ($user === null) {
            return $this->json([
                'message' => 'missing credentials.',
            ], Response::HTTP_UNAUTHORIZED);
        }

        $token = 'todo'; // somehow create an API token for $user

        return $this->json([
            'message' => 'Welcome to your new controller!',
            'token' => $token,
        ]);
    }

    #[Route('/logout', name: 'api_logout', methods: ['POST'])]
    public function logout(#[CurrentUser] ?User $user): JsonResponse
    {
        $this->denyAccessUnlessGranted('IS_AUTHENTICATED');

        // The security layer will intercept this request
        return $this->json([
            'message' => 'todo You have been logged out.',
        ]);
    }
}
