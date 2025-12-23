<?php

namespace App\Mapper;

use Symfony\Component\Filesystem\Filesystem;
use App\Entity\User;
use App\ImageModification\ImageModification;
use App\DTO\SpraywallProblemDto;
use App\Entity\SpraywallProblem;

final class Mapper
{
    public static function getProblem(SpraywallProblem $spraywallProblem, ?User $currentUser, Filesystem $filesystem): SpraywallProblemDto
    {
        return new SpraywallProblemDto(
            id: $spraywallProblem->getId(),
            name: $spraywallProblem->getName(),
            image: ImageModification::getSpraywallProblemImage($spraywallProblem->getSpraywall()->getId(), $spraywallProblem->getId(), $filesystem),
            fontGrade: $spraywallProblem->getFontGrade()?->getValue(),
            createdById: $spraywallProblem->getCreatedBy()->getId(),
            createdByName: $spraywallProblem->getCreatedBy()->getUsername(),
            createdDate: $spraywallProblem->getCreatedDate()->format('Y-m-d\TH:i:s.v\Z'),
            description: $spraywallProblem->getDescription(),
            canEdit: $currentUser !== null && $currentUser->getId() === $spraywallProblem->getCreatedBy()->getId()
        );
    }
}