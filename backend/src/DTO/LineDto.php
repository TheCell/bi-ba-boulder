<?php

namespace App\DTO;

use Symfony\Component\Validator\Constraints as Assert;
use Symfony\Component\Uid\Uuid;

class LineDto
{
    public function __construct(
      #[Assert\NotBlank]
      public Uuid $id,

      #[Assert\NotBlank]
      public string $blocId,

      #[Assert\NotBlank]
      public string $identifier,

      public ?string $description,

      public ?string $color,

      public ?string $name)
    { }
}
