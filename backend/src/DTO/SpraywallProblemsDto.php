<?php

namespace App\DTO;

use Symfony\Component\Validator\Constraints as Assert;

class SpraywallProblemsDto
{
  public function __construct(
    #[Assert\NotBlank]
    public string $id,

    #[Assert\NotBlank]
    public string $name,
    
    #[Assert\NotBlank]
    public string $image,
    
    public ?string $description)
  { }
}
