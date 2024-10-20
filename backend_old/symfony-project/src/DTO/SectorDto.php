<?php

namespace App\DTO;

use Symfony\Component\Validator\Constraints as Assert;

class SectorDto
{
  public function __construct(
    #[Assert\NotBlank]
    public string $id,

    #[Assert\NotBlank]
    public string $name,

    public ?string $description)
  { }
}
