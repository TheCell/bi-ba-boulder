<?php

namespace App\Entity;

use ApiPlatform\Metadata\ApiResource;
use App\Repository\TinyGnomeRepository;
use Doctrine\ORM\Mapping as ORM;

#[ORM\Entity(repositoryClass: TinyGnomeRepository::class)]
#[ApiResource]
class TinyGnome
{
    #[ORM\Id]
    #[ORM\GeneratedValue]
    #[ORM\Column]
    private ?int $id = null;

    public function getId(): ?int
    {
        return $this->id;
    }
}
