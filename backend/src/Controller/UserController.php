<?php

namespace App\Controller;

use App\Entity\User;
use App\DTO\UserDto;
use Symfony\Bundle\FrameworkBundle\Controller\AbstractController;
use Symfony\Component\HttpFoundation\JsonResponse;
use Symfony\Component\HttpFoundation\Response;
use Symfony\Component\Routing\Attribute\Route;
use Symfony\Component\Security\Http\Attribute\CurrentUser;

#[Route('/api', name: '')]
final class UserController extends AbstractController
{
    #[Route('/users/{id}', name: 'getUserById', methods: ['GET'])]
    public function getUserById(User $user): JsonResponse
    {
        return $this->json([
            'id' => $user->getId(),
            'email' => $user->getEmail(),
            'roles' => $user->getRoles(),
        ]);
    }

    #[Route('/users/me', name: 'getCurrentUser', methods: ['GET'])]
    public function getCurrentUser(#[CurrentUser] ?User $user): JsonResponse
    {
        if (null === $user) {
            return $this->json(['error' => 'User not found'], Response::HTTP_NOT_FOUND);
        }
        
        $userDto = new UserDto(
            $user->getId(),
            $user->getEmail(),
            $user->getRoles());

        return $this->json($userDto);
    }
}