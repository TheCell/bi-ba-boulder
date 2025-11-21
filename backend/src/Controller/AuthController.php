<?php

namespace App\Controller;

use App\Entity\User;
use App\DTO\UserDto;
use App\Repository\UserRepository;
use Doctrine\ORM\EntityManagerInterface;
use OpenApi\Attributes as OA;
use Nelmio\ApiDocBundle\Annotation\Model;
use Lexik\Bundle\JWTAuthenticationBundle\Services\JWTTokenManagerInterface;
use Symfony\Bundle\FrameworkBundle\Controller\AbstractController;
use Symfony\Component\HttpFoundation\JsonResponse;
use Symfony\Component\HttpFoundation\Request;
use Symfony\Component\HttpFoundation\Response;
use Symfony\Component\PasswordHasher\Hasher\UserPasswordHasherInterface;
use Symfony\Component\Routing\Annotation\Route;
use Symfony\Component\Validator\Validator\ValidatorInterface;

#[Route('/api', name: '')]
final class AuthController extends AbstractController
{
    public function __construct(
        private EntityManagerInterface $entityManager,
        private UserPasswordHasherInterface $passwordHasher,
        private JWTTokenManagerInterface $jwtManager,
        private ValidatorInterface $validator,
        private UserRepository $userRepository
    ) {}

    #[Route('/register', methods: ['POST'])]
    #[OA\RequestBody(
      required: true,
      content: new OA\JsonContent(
        type: 'object',
        properties: [
          new OA\Property(
            property: 'email',
            type: 'string',
            description: 'Email of the user'
          ),
          new OA\Property(
            property: 'password',
            type: 'string',
            description: 'Password of the user',
          )
        ],
        required: ['email', 'password']
      )
    )]
    #[OA\Response(
      response: Response::HTTP_CREATED,
      description: 'Returns the created user',
      content: new OA\MediaType(
        mediaType: 'application/json',
        schema: new OA\Schema(ref: new Model(type: UserDto::class))
      )
    )]
    public function register(Request $request): JsonResponse
    {
        $data = json_decode($request->getContent(), true);
        if (!isset($data['password']) || strlen($data['password']) < 8) {
            return $this->json(['error' => 'Password must be at least 8 characters'], Response::HTTP_BAD_REQUEST);
        }

        $user = new User();
        $user->setEmail($data['email']);
        $user->setRoles(['ROLE_USER']);
        // $user->setName($data['name']);
        
        $plaintextPassword = $data['password'];

        $hashedPassword = $this->passwordHasher->hashPassword(
            $user,
            $plaintextPassword
        );
        $user->setPassword($hashedPassword);

        $errors = $this->validator->validate($user);
        if (count($errors) > 0) {
            $errorMessages = [];
            foreach ($errors as $error) {
                $errorMessages[$error->getPropertyPath()] = $error->getMessage();
            }
            return $this->json(['errors' => $errorMessages], Response::HTTP_BAD_REQUEST);
        }
        
        $this->userRepository->addUser($user);
        $this->entityManager->persist($user);
        $this->entityManager->flush();

        $userDto = new UserDto(
            $user->getId(),
            $user->getEmail(),
            $user->getRoles());

        return $this->json($userDto, Response::HTTP_CREATED);
    }

    #[Route('/login_check', methods: ['POST'])]
    public function login(): void
    {
        throw new \RuntimeException('This method should not be called directly.');
    }

    #[Route('/profile', methods: ['GET'])]
    public function getProfile(): JsonResponse
    {
        $user = $this->getUser();
        if (!$user instanceof User) {
            return $this->json(['error' => 'User not found'], Response::HTTP_NOT_FOUND);
        }
        $userDto = new UserDto(
            $user->getId(),
            $user->getEmail(),
            $user->getRoles());

        return $this->json($userDto, Response::HTTP_OK);
    }
}
