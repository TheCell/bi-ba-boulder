<?php

namespace App\Entity;

use App\Repository\BookmarkRepository;
use Doctrine\ORM\Mapping as ORM;
use Symfony\Component\Uid\Uuid;
use Symfony\Bridge\Doctrine\Types\UuidType;

#[ORM\Entity(repositoryClass: BookmarkRepository::class)]
class Bookmark
{
    #[ORM\Id]
    #[ORM\Column(type: UuidType::NAME, unique: true)]
    #[ORM\GeneratedValue(strategy: 'CUSTOM')]
    #[ORM\CustomIdGenerator(class: 'doctrine.uuid_generator')]
    private ?Uuid $id = null;

    #[ORM\Column]
    private ?bool $isProject = null;

    #[ORM\Column]
    private ?bool $isFavourite = null;

    #[ORM\ManyToOne(inversedBy: 'bookmarks')]
    #[ORM\JoinColumn(nullable: false)]
    private ?User $User = null;

    public function getId(): ?Uuid
    {
        return $this->id;
    }

    public function isProject(): ?bool
    {
        return $this->isProject;
    }

    public function setIsProject(bool $isProject): static
    {
        $this->isProject = $isProject;

        return $this;
    }

    public function isFavourite(): ?bool
    {
        return $this->isFavourite;
    }

    public function setIsFavourite(bool $isFavourite): static
    {
        $this->isFavourite = $isFavourite;

        return $this;
    }

    public function getUser(): ?User
    {
        return $this->User;
    }

    public function setUser(?User $User): static
    {
        $this->User = $User;

        return $this;
    }
}
