<?php

namespace App\DTO;

use Symfony\Component\Validator\Constraints as Assert;

class SpraywallProblemSearchDto
{
  public function __construct(
    #[Assert\NotBlank]
    public int $totalCount,

    #[Assert\NotBlank]
    public int $currentPage,

    /**
     * @var SpraywallProblemDto[]
     */
    #[Assert\NotBlank]
    public array $problems // array of SpraywallProblemDto
    )
  { }
}
