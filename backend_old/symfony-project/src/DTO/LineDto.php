<?php

namespace App\DTO;

use Symfony\Component\Validator\Constraints as Assert;

class LineDto
{
    public function __construct(
      #[Assert\NotBlank]
      public string $id,

      #[Assert\NotBlank]
      public string $blocId,

      #[Assert\NotBlank]
      public string $identifier,

      public ?string $description,

      public ?string $color,

      public ?string $name)
    { }
}
