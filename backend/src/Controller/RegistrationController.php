<?php

namespace App\Controller;

use App\Entity\User;
use App\DTO\UserDto;
use App\Repository\UserRepository;
use Symfony\Bundle\FrameworkBundle\Controller\AbstractController;
use Symfony\Component\HttpFoundation\JsonResponse;
use Symfony\Component\HttpFoundation\Request;
use Symfony\Component\Routing\Attribute\Route;
use Symfony\Component\PasswordHasher\Hasher\UserPasswordHasherInterface;
use OpenApi\Attributes as OA;
use Nelmio\ApiDocBundle\Annotation\Model;
use Symfony\Component\HttpFoundation\Response;

#[Route('/api', name: '')]
final class RegistrationController extends AbstractController
{
    // private $userRepository;
    
    // public function __construct(UserRepository $userRepository)
    // {
    //     $this->userRepository = $userRepository;
    // }

    // #[Route('/registration', name: 'registration')]
    // #[OA\RequestBody(
    //   required: true,
    //   content: new OA\JsonContent(
    //     type: 'object',
    //     properties: [
    //       new OA\Property(
    //         property: 'email',
    //         type: 'string',
    //         description: 'Email of the user'
    //       ),
    //       new OA\Property(
    //         property: 'password',
    //         type: 'string',
    //         description: 'Password of the user',
    //       )
    //     ],
    //     required: ['email', 'password']
    //   )
    // )]
    // #[OA\Response(
    //   response: Response::HTTP_CREATED,
    //   description: 'Returns the created user',
    //   content: new OA\MediaType(
    //     mediaType: 'application/json',
    //     schema: new OA\Schema(ref: new Model(type: UserDto::class))
    //   )
    // )]
    // public function index(UserPasswordHasherInterface $passwordHasher, Request $request): JsonResponse
    // {
    //     $data = json_decode($request->getContent(), true);

    //     $user = new User();
    //     $user->setEmail(trim($data['email']));
    //     $user->setRoles(['ROLE_USER']);
        
    //     $plaintextPassword = $data['password'];

    //     $hashedPassword = $passwordHasher->hashPassword(
    //         $user,
    //         $plaintextPassword
    //     );
    //     $user->setPassword($hashedPassword);
    //     $this->userRepository->addUser($user);

    //     $userDto = new UserDto(
    //         $user->getId(),
    //         $user->getEmail(),
    //         $user->getRoles());

    //     return $this->json($userDto);
    // }
}
