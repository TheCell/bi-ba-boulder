<?php

namespace App\Controller;

use App\Entity\User;
use App\DTO\UserDto;
use App\DTO\ErrorDto;
use App\Repository\UserRepository;
use Nelmio\ApiDocBundle\Annotation\Model;
use Symfony\Component\Uid\Uuid;
use Symfony\Bundle\FrameworkBundle\Controller\AbstractController;
use Symfony\Component\HttpFoundation\JsonResponse;
use Symfony\Component\HttpFoundation\Response;
use OpenApi\Attributes as OA;
use Symfony\Component\Routing\Attribute\Route;
use Symfony\Component\Security\Http\Attribute\CurrentUser;
use Symfony\Component\Security\Http\Attribute\IsGranted;

#[Route('/api/users', name: '')]
#[OA\Tag(name: "User")]
final class UserController extends AbstractController
{
    public function __construct(private readonly UserRepository $userRepository) { }

    #[Route('/me', name: 'getSelf', methods: ['GET'])]
    #[IsGranted('ROLE_USER')]
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
    public function getSelf(#[CurrentUser] ?User $currentUser): JsonResponse
    {
        $userDto = new UserDto(
            $currentUser->getId(),
            $currentUser->getEmail(),
            $currentUser->getRoles()
        );

        return $this->json($userDto, Response::HTTP_OK);
    }

    #[Route('/{id}', name: 'getUserById', methods: ['GET'])]
    #[IsGranted('ROLE_ADMIN')]
    #[OA\Response(
        response: Response::HTTP_OK,
        description: 'Returns user by ID',
        content: new OA\JsonContent(ref: new Model(type: UserDto::class))
    )]
    #[OA\Response(
        response: Response::HTTP_NOT_FOUND,
        description: 'User not found',
        content: new OA\JsonContent(ref: new Model(type: ErrorDto::class))
    )]
    #[OA\Response(
        response: Response::HTTP_UNAUTHORIZED,
        description: 'Unauthorized access',
        content: new OA\JsonContent(ref: new Model(type: ErrorDto::class))
    )]
    public function getUserById(Uuid $id): JsonResponse
    {
        $user = $this->userRepository->findById($id);

        if ($user === null) {
            return $this->json(new ErrorDto('User not found', null), Response::HTTP_NOT_FOUND);
        }
        
        $userDto = new UserDto(
            $user->getId(),
            $user->getEmail(),
            $user->getRoles()
        );
        return $this->json($userDto, Response::HTTP_OK);
    }
}