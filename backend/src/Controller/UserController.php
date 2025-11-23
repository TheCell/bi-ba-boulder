<?php

namespace App\Controller;

use App\Entity\User;
use App\DTO\UserDto;
use App\DTO\ErrorDto;
use Nelmio\ApiDocBundle\Annotation\Model;
use Symfony\Bundle\FrameworkBundle\Controller\AbstractController;
use Symfony\Component\HttpFoundation\JsonResponse;
use Symfony\Component\HttpFoundation\Response;
use OpenApi\Attributes as OA;
use Symfony\Component\Routing\Attribute\Route;
use Symfony\Component\Security\Http\Attribute\CurrentUser;

#[Route('/api/users', name: '')]
#[OA\Tag(name: "User")]
final class UserController extends AbstractController
{
    #[Route('/{id}', name: 'getUserById', methods: ['GET'])]
    #[OA\Response(
        response: Response::HTTP_OK,
        description: 'Returns user by ID',
        content: new OA\JsonContent(ref: new Model(type: UserDto::class))
    )]
    #[OA\Response(
        response: Response::HTTP_UNAUTHORIZED,
        description: 'Unauthorized access',
        content: new OA\JsonContent(ref: new Model(type: ErrorDto::class))
    )]
    public function getUserById(User $user, #[CurrentUser] ?User $currentUser): JsonResponse
    {
        if (null === $currentUser || (!in_array('ROLE_ADMIN', $currentUser->getRoles()))) {
            return $this->json(new ErrorDto('unauthorized', null), Response::HTTP_UNAUTHORIZED);
        }
        
        $userDto = new UserDto(
            $user->getId(),
            $user->getEmail(),
            $user->getRoles()
        );
        return $this->json($userDto, Response::HTTP_OK);
    }

    #[Route('/me', name: 'getCurrentUser', methods: ['GET'])]
    #[OA\Response(
        response: Response::HTTP_OK,
        description: 'Returns current user',
        content: new OA\JsonContent(ref: new Model(type: UserDto::class))
    )]
    #[OA\Response(
        response: Response::HTTP_NOT_FOUND,
        description: 'User not found',
        content: new OA\JsonContent(ref: new Model(type: ErrorDto::class))
    )]
    public function getCurrentUser(#[CurrentUser] ?User $currentUser): JsonResponse
    {
        if (null === $currentUser) {
            return $this->json(new ErrorDto('User not found', null), Response::HTTP_NOT_FOUND);
        }
        $userDto = new UserDto(
            $currentUser->getId(),
            $currentUser->getEmail(),
            $currentUser->getRoles()
        );
        return $this->json($userDto, Response::HTTP_OK);
    }
}