<?php

namespace App\Controller;

use App\Entity\User;
use App\DTO\ErrorDto;
use App\MailTemplates\MailTemplates;
use App\Entity\SpraywallProblem;
use App\ImageModification\compressAndConvertTo24BitPng;
use App\DTO\SpraywallDto;
use App\DTO\SpraywallProblemDto;
use App\DTO\SpraywallProblemSearchDto;
use App\Entity\Enum\FontGrade;
use App\ImageModification\ImageModification;
use App\Mapper\Mapper;
use App\Repository\SpraywallRepository;
use Nelmio\ApiDocBundle\Attribute\Model;
use App\Repository\SpraywallProblemRepository;
use Symfony\Bundle\FrameworkBundle\Controller\AbstractController;
use Symfony\Component\HttpFoundation\JsonResponse;
use Symfony\Component\HttpFoundation\Request;
use Symfony\Component\Routing\Attribute\Route;
use OpenApi\Attributes as OA;
use Symfony\Component\Filesystem\Exception\IOExceptionInterface;
use Symfony\Component\Filesystem\Filesystem;
use Symfony\Component\HttpFoundation\Response;
use Symfony\Component\Uid\Uuid;
use Symfony\Component\Security\Http\Attribute\CurrentUser;
use Symfony\Component\Security\Http\Attribute\IsGranted;
use Symfony\Component\RateLimiter\RateLimiterFactoryInterface;

#[Route('/api/feedbacks', name: '')]
#[OA\Tag(name: "Feedbacks")]
final class FeedbacksController extends AbstractController
{
    public function __construct(
        private readonly MailTemplates $mailTemplates,
    ) { }

    #[Route('/send_feedback', name: 'send_feedback', methods: ['POST'])]
    #[IsGranted('ROLE_USER')]
    #[OA\RequestBody(
        required: false,
        content: new OA\JsonContent(
            type: 'object',
            properties: [
                new OA\Property(property: 'feedback', type: 'string', description: 'User feedback message')
            ]
        )
    )]
    #[OA\Response(
        response: Response::HTTP_OK,
        description: 'Confirms feedback submission'
    )]
    public function sendFeedback(Request $request, #[CurrentUser] ?User $currentUser): Response
    {
        $data = json_decode($request->getContent(), true);
        
        if (!$data || !isset($data['feedback']) || empty(trim($data['feedback']))) {
            return $this->json(new ErrorDto('Feedback is required and cannot be empty', null), Response::HTTP_BAD_REQUEST);
        }

        $template = $this->mailTemplates->getFeedbackTemplate(
            $currentUser->getEmail(),
            $data['feedback']
        );
        
        mail(
            to: $currentUser->getEmail(),
            subject: 'User Feedback From ' . $currentUser->getEmail(),
            message: $template,
            additional_headers: array(
                "Content-Type" => "text/html",
                "From" => $_ENV["SENDEREMAIL"],
                'Bcc' => $_ENV["ADMINMAIL"]));

        return new Response('', Response::HTTP_OK);
    }
}
