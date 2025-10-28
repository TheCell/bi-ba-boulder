<?php

namespace App\Controller;

use App\DTO\SpraywallProblemsDto;
use Nelmio\ApiDocBundle\Attribute\Model;
use App\Repository\SpraywallProblemRepository;
use Symfony\Bundle\FrameworkBundle\Controller\AbstractController;
use Symfony\Component\HttpFoundation\JsonResponse;
use Symfony\Component\Routing\Attribute\Route;
use OpenApi\Attributes as OA;
use Symfony\Component\Filesystem\Exception\IOExceptionInterface;
use Symfony\Component\Filesystem\Filesystem;
use Symfony\Component\Filesystem\Path;

#[Route('/api', name: '')]
final class SpraywallController extends AbstractController
{
    private $spraywallProblemRepository;
    private $filesystem;

    public function __construct(SpraywallProblemRepository $spraywallProblemRepository)
    {
        $this->spraywallProblemRepository = $spraywallProblemRepository;
        $this->filesystem = new Filesystem();
    }

    #[Route('/spraywall', name: 'app_spraywall')]
    public function index(): JsonResponse
    {
        return $this->json([
            'message' => 'Welcome to your new controller!',
            'path' => 'src/Controller/SpraywallController.php',
        ]);
    }

    #[Route('/spraywall/{id}/problems', name: 'app_spraywall_problems')]
    #[OA\Response(
      response: 200,
      description: 'Returns a list of problems',
      content: new OA\JsonContent(
        type: 'array',
        items: new OA\Items(ref: new Model(type: SpraywallProblemsDto::class))
      )
    )]
    public function getProblems($id): JsonResponse
    {
        $spraywallProblems = $this->spraywallProblemRepository->findBySpraywallId($id);

        $exists = $this->filesystem->exists("spraywalls/{$id}");
        
        $spraywallProblemsDto = array_map(fn($spraywallProblem) => 
            new SpraywallProblemsDto(
                $spraywallProblem->getId(),
                $spraywallProblem->getName(),
                $this->getSpraywallProblemImage($id, $spraywallProblem->getId()),
                $spraywallProblem->getDescription()), $spraywallProblems);

        return $this->json($spraywallProblemsDto);
    }

    private function getSpraywallProblemImage($spraywallId, $spraywallProblemId): string {
        $contents = $this->filesystem->readFile("spraywalls/{$spraywallId}/{$spraywallProblemId}.png");
        return base64_encode($contents);
    }

    

    // #[Route('/sectors/{id}', name: 'sector', methods: ['GET'])]
    // #[OA\Response(
    //   response: 200,
    //   description: 'Returns a list of sectors',
    //   content: new OA\JsonContent(
    //     type: 'array',
    //     items: new OA\Items(ref: new Model(type: SectorDto::class))
    //   )
    // )]
    // public function getSector($id): JsonResponse
    // {
        // $sector = $this->sectorRepository->findById($id);
        // $sectorDto = new SectorDto (
        //     $sector->getId(),
        //     $sector->getName(),
        //     $sector->getDescription()
        // );

        // return $this->json(get_object_vars($sectorDto));
    // }

}
