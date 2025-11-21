<?php

namespace App\Controller;

use App\Entity\User;
use App\DTO\UserDto;
use Symfony\Bundle\FrameworkBundle\Controller\AbstractController;
use Symfony\Component\HttpFoundation\JsonResponse;
use Symfony\Component\HttpFoundation\Response;
use OpenApi\Attributes as OA;
use Symfony\Component\Routing\Attribute\Route;
use Symfony\Component\Security\Http\Attribute\CurrentUser;

#[Route('/api', name: '')]
final class UserController extends AbstractController
{

    #[Route('/users/{id}', name: 'getUserById', methods: ['GET'])]
    #[OA\Response(
        response: Response::HTTP_OK,
        description: 'Returns user by ID',
        content: new OA\JsonContent(
            type: 'object',
            properties: [
                new OA\Property(property: 'id', type: 'integer'),
                new OA\Property(property: 'email', type: 'string'),
                new OA\Property(property: 'roles', type: 'array', items: new OA\Items(type: 'string'))
            ]
        )
    )]
    public function getUserById(User $user): JsonResponse
    {
        return $this->json([
            'id' => $user->getId(),
            'email' => $user->getEmail(),
            'roles' => $user->getRoles(),
        ], Response::HTTP_OK);
    }


    #[Route('/users/me', name: 'getCurrentUser', methods: ['GET'])]
    #[OA\Response(
        response: Response::HTTP_OK,
        description: 'Returns current user',
        content: new OA\JsonContent(
            type: 'object',
            properties: [
                new OA\Property(property: 'id', type: 'integer'),
                new OA\Property(property: 'email', type: 'string'),
                new OA\Property(property: 'roles', type: 'array', items: new OA\Items(type: 'string'))
            ]
        )
    )]
    #[OA\Response(
        response: Response::HTTP_NOT_FOUND,
        description: 'User not found',
        content: new OA\JsonContent(
            type: 'object',
            properties: [
                new OA\Property(property: 'error', type: 'string')
            ]
        )
    )]
    public function getCurrentUser(#[CurrentUser] ?User $user): JsonResponse
    {
        if (null === $user) {
            return $this->json(['error' => 'User not found'], Response::HTTP_NOT_FOUND);
        }
        $userDto = new UserDto(
            $user->getId(),
            $user->getEmail(),
            $user->getRoles()
        );
        return $this->json($userDto, Response::HTTP_OK);
    }
}