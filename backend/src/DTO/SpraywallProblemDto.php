<?php

namespace App\DTO;

use Symfony\Component\Validator\Constraints as Assert;
use Symfony\Component\Uid\Uuid;

class SpraywallProblemDto
{
  public function __construct(
    #[Assert\NotBlank]
    public Uuid $id,

    #[Assert\NotBlank]
    public string $name,
    
    #[Assert\NotBlank]
    public string $image,

    public ?int $fontGrade,
    
    #[Assert\NotBlank]
    public Uuid $createdById,

    #[Assert\NotBlank]
    public string $createdByName,

    #[Assert\NotBlank]
    public string $createdDate,

    public ?string $description)
  { }
}
