<?php

namespace App\Entity;

use App\Repository\SpraywallRepository;
use Doctrine\Common\Collections\ArrayCollection;
use Doctrine\Common\Collections\Collection;
use Doctrine\DBAL\Types\Types;
use Symfony\Bridge\Doctrine\Types\UuidType;
use Doctrine\ORM\Mapping as ORM;
use Symfony\Component\Uid\Uuid;

#[ORM\Entity(repositoryClass: SpraywallRepository::class)]
class Spraywall
{
    #[ORM\Id]
    #[ORM\Column(type: UuidType::NAME, unique: true)]
    #[ORM\GeneratedValue(strategy: 'CUSTOM')]
    #[ORM\CustomIdGenerator(class: 'doctrine.uuid_generator')]
    private ?Uuid $id = null;

    #[ORM\Column(length: 512)]
    private ?string $name = null;

    #[ORM\Column(type: Types::TEXT, nullable: true)]
    private ?string $description = null;

    /**
     * @var Collection<Uuid, SpraywallProblem>
     */
    #[ORM\OneToMany(targetEntity: SpraywallProblem::class, mappedBy: 'spraywall')]
    private Collection $spraywallProblems;

    public function __construct()
    {
        $this->spraywallProblems = new ArrayCollection();
    }

    public function getId(): ?Uuid
    {
        return $this->id;
    }

    public function getName(): ?string
    {
        return $this->name;
    }

    public function setName(string $name): static
    {
        $this->name = $name;

        return $this;
    }

    public function getDescription(): ?string
    {
        return $this->description;
    }

    public function setDescription(?string $description): static
    {
        $this->description = $description;

        return $this;
    }

    /**
     * @return Collection<int, SpraywallProblem>
     */
    public function getSpraywallProblems(): Collection
    {
        return $this->spraywallProblems;
    }

    public function addSpraywallProblem(SpraywallProblem $spraywallProblem): static
    {
        if (!$this->spraywallProblems->contains($spraywallProblem)) {
            $this->spraywallProblems->add($spraywallProblem);
            $spraywallProblem->setSpraywall($this);
        }

        return $this;
    }

    public function removeSpraywallProblem(SpraywallProblem $spraywallProblem): static
    {
        if ($this->spraywallProblems->removeElement($spraywallProblem)) {
            // set the owning side to null (unless already changed)
            if ($spraywallProblem->getSpraywall() === $this) {
                $spraywallProblem->setSpraywall(null);
            }
        }

        return $this;
    }
}
