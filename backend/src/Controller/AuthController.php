<?php
namespace App\Controller;

use App\Entity\User;
use App\DTO\UserDto;
use App\DTO\ErrorDto;
use App\DTO\TokenDto;
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
use Symfony\Bundle\SecurityBundle\Security;
use Symfony\Component\Security\Http\Attribute\CurrentUser;
use Symfony\Component\EventDispatcher\EventDispatcherInterface;
use Symfony\Component\Security\Core\Authentication\Token\Storage\TokenStorageInterface;
use Symfony\Component\Security\Http\Event\LogoutEvent;
use SymfonyCasts\Bundle\VerifyEmail\Exception\VerifyEmailExceptionInterface;
use SymfonyCasts\Bundle\VerifyEmail\VerifyEmailHelperInterface;
use App\Security\EmailVerifier;

#[Route('/api', name: '')]
#[OA\Tag(name: "Auth")]
final class AuthController extends AbstractController
{
    public function __construct(
        private EntityManagerInterface $entityManager,
        private UserPasswordHasherInterface $passwordHasher,
        private JWTTokenManagerInterface $jwtManager,
        private ValidatorInterface $validator,
        private UserRepository $userRepository,
        private EventDispatcherInterface $eventDispatcher,
        private TokenStorageInterface $tokenStorage,
        private VerifyEmailHelperInterface $verifyEmailHelper,
        private EmailVerifier $emailVerifier
    ) {}

    #[Route('/register', name: 'register', methods: ['POST'])]
    #[OA\RequestBody(
        required: true,
        content: new OA\JsonContent(
            type: 'object',
            properties: [
                new OA\Property(property: 'email', type: 'string', description: 'Email of the user'),
                new OA\Property(property: 'password', type: 'string', description: 'Password of the user')
            ],
            required: ['email', 'password']
        )
    )]
    #[OA\Response(
        response: Response::HTTP_CREATED,
        description: 'Returns the created user',
        content: new OA\JsonContent(ref: new Model(type: UserDto::class))
    )]
    #[OA\Response(
        response: Response::HTTP_BAD_REQUEST,
        description: 'Validation error',
        content: new OA\JsonContent(ref: new Model(type: ErrorDto::class))
    )]
    public function register(Request $request): JsonResponse
    {
        $data = json_decode($request->getContent(), true);
        if (!isset($data['email']) || !filter_var($data['email'], FILTER_VALIDATE_EMAIL)) {
            return $this->json(new ErrorDto('Invalid email address', null), Response::HTTP_BAD_REQUEST);
        }
        if (!isset($data['password']) || strlen($data['password']) < 8) {
            return $this->json(new ErrorDto('Password must be at least 8 characters', null), Response::HTTP_BAD_REQUEST);
        }

        $user = new User();
        $user->setEmail($data['email']);
        $user->setRoles(['ROLE_USER']);
        $plaintextPassword = $data['password'];
        $hashedPassword = $this->passwordHasher->hashPassword($user, $plaintextPassword);
        $user->setPassword($hashedPassword);

        $errors = $this->validator->validate($user);
        if (count($errors) > 0) {
            $errorMessages = [];
            foreach ($errors as $error) {
                $errorMessages[$error->getPropertyPath()] = $error->getMessage();
            }
            return $this->json(new ErrorDto('Validation error', $errorMessages), Response::HTTP_BAD_REQUEST);
        }

        $this->userRepository->addUser($user);
        $this->entityManager->persist($user);
        $this->entityManager->flush();

        $userDto = new UserDto(
            $user->getId(),
            $user->getEmail(),
            $user->getRoles()
        );

        $this->emailVerifier->sendEmailConfirmation('verify_email', $user);

        $utc_time = new \DateTime("now", new \DateTimeZone("UTC"));
        $this->userRepository->updateVerifyMailSentTime($user, $utc_time);

        return $this->json($userDto, Response::HTTP_CREATED);
    }

    #[Route('/login_check', methods: ['POST'])]
    #[OA\RequestBody(
        required: true,
        content: new OA\JsonContent(
            type: 'object',
            properties: [
                new OA\Property(property: 'email', type: 'string'),
                new OA\Property(property: 'password', type: 'string')
            ],
            required: ['email', 'password']
        )
    )]
    #[OA\Response(
        response: Response::HTTP_OK,
        description: 'Returns JWT token',
        content: new OA\JsonContent(ref: new Model(type: TokenDto::class))
    )]
    #[OA\Response(
        response: Response::HTTP_UNAUTHORIZED,
        description: 'Invalid credentials',
        content: new OA\JsonContent(ref: new Model(type: ErrorDto::class))
    )]
    public function login(): void
    {
        throw new \RuntimeException('This method should not be called directly.');
    }

    // #[Route('/logout', name: 'logout', methods: ['POST'])]
    // #[OA\Response(
    //     response: Response::HTTP_OK,
    //     description: 'Returns user by ID',
    //     content: new OA\JsonContent(ref: new Model(type: UserDto::class))
    // )]
    // #[OA\Response(
    //     response: Response::HTTP_UNAUTHORIZED,
    //     description: 'Unauthorized access',
    //     content: new OA\JsonContent(ref: new Model(type: ErrorDto::class))
    // )]
    // public function logout(Request $request, #[CurrentUser] ?User $currentUser): JsonResponse
    // {
    //     $user = $this->getUser();
    //     if (!$user instanceof User) {
    //         return $this->json(['error' => 'User not found'], Response::HTTP_NOT_FOUND);
    //     }
    //     // $this->jwtManager->logout();

    //     if (null === $currentUser || (!in_array('ROLE_USER', $currentUser->getRoles()))) {
    //         return $this->json(new ErrorDto('unauthorized' . $currentUser, $currentUser), Response::HTTP_UNAUTHORIZED);
    //     }

    //     $this->eventDispatcher->dispatch(new LogoutEvent($request, $this->tokenStorage->getToken()));
    //     // $response = $this->security->logout();
    //     // return $response;
    //     return $this->json(['message' => 'Logged out successfully'], Response::HTTP_OK);
    // }

    #[Route('/verify-email', name: 'verify_email', methods: ['GET'])]
    public function verifyEmail(Request $request): JsonResponse
    {
        $id = $request->query->get('id'); // retrieve the user id from the url
 
        if (null === $id) {
            return $this->json(new ErrorDto('Invalid verification link', null), Response::HTTP_BAD_REQUEST);
        }
 
        $user = $this->userRepository->find($id);
 
        if (null === $user) {
            return $this->json(new ErrorDto('User not found', null), Response::HTTP_BAD_REQUEST);
        }

        if ($user->isVerified()) {
            return $this->json(new ErrorDto('Email already verified', null), Response::HTTP_BAD_REQUEST);
        }
        
        try {
            $this->verifyEmailHelper->validateEmailConfirmationFromRequest($request, $user->getId(), $user->getEmail());
        } catch (VerifyEmailExceptionInterface $exception) {
            return $this->json(new ErrorDto('Email verification failed: ' . $exception->getReason(), null), Response::HTTP_BAD_REQUEST);
        }

        $user->setIsVerified(true);
        $currentRoles = $user->getRoles();
        $currentRoles[] = "ROLE_EDITOR";
        $user->setRoles($currentRoles);
        $this->entityManager->persist($user);
        $this->entityManager->flush();

        return $this->json(['message' => 'Email verified successfully'], Response::HTTP_OK);
    }

    #[Route('/profile', methods: ['GET'])]
    #[OA\Response(
        response: Response::HTTP_OK,
        description: 'Returns the user profile',
        content: new OA\JsonContent(ref: new Model(type: UserDto::class))
    )]
    #[OA\Response(
        response: Response::HTTP_NOT_FOUND,
        description: 'User not found',
        content: new OA\JsonContent(ref: new Model(type: ErrorDto::class))
    )]
    public function getProfile(): JsonResponse
    {
        $user = $this->getUser();
        if (!$user instanceof User) {
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
