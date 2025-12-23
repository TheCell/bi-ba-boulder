<?php

namespace App\Entity;

use App\Entity\Enum\FontGrade;
use App\Repository\SpraywallProblemRepository;
use Doctrine\Common\Collections\ArrayCollection;
use Doctrine\Common\Collections\Collection;
use Doctrine\DBAL\Types\Types;
use Symfony\Bridge\Doctrine\Types\UuidType;
use Doctrine\ORM\Mapping as ORM;
use Symfony\Component\Uid\Uuid;

#[ORM\Entity(repositoryClass: SpraywallProblemRepository::class)]
class SpraywallProblem
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

    #[ORM\ManyToOne(inversedBy: 'spraywallProblems')]
    #[ORM\JoinColumn(nullable: false)]
    private ?Spraywall $spraywall = null;

    #[ORM\Column(enumType: FontGrade::class, nullable: true)]
    private ?FontGrade $FontGrade = null;

    #[ORM\ManyToOne(inversedBy: 'spraywallProblems')]
    #[ORM\JoinColumn(nullable: false)]
    private ?User $CreatedBy = null;

    /**
     * @var Collection<int, BoulderLog>
     */
    #[ORM\OneToMany(targetEntity: BoulderLog::class, mappedBy: 'SpraywallProblem')]
    private Collection $boulderLogs;

    #[ORM\Column]
    private ?\DateTime $CreatedDate = null;

    public function __construct()
    {
        $this->boulderLogs = new ArrayCollection();
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

    public function getSpraywall(): ?Spraywall
    {
        return $this->spraywall;
    }

    public function setSpraywall(?Spraywall $spraywall): static
    {
        $this->spraywall = $spraywall;

        return $this;
    }

    public function getFontGrade(): ?FontGrade
    {
        return $this->FontGrade;
    }

    public function setFontGrade(?FontGrade $FontGrade): static
    {
        $this->FontGrade = $FontGrade;

        return $this;
    }

    public function getCreatedBy(): ?User
    {
        return $this->CreatedBy;
    }

    public function setCreatedBy(?User $CreatedBy): static
    {
        $this->CreatedBy = $CreatedBy;

        return $this;
    }

    /**
     * @return Collection<int, BoulderLog>
     */
    public function getBoulderLogs(): Collection
    {
        return $this->boulderLogs;
    }

    public function addBoulderLog(BoulderLog $boulderLog): static
    {
        if (!$this->boulderLogs->contains($boulderLog)) {
            $this->boulderLogs->add($boulderLog);
            $boulderLog->setSpraywallProblem($this);
        }

        return $this;
    }

    public function removeBoulderLog(BoulderLog $boulderLog): static
    {
        if ($this->boulderLogs->removeElement($boulderLog)) {
            // set the owning side to null (unless already changed)
            if ($boulderLog->getSpraywallProblem() === $this) {
                $boulderLog->setSpraywallProblem(null);
            }
        }

        return $this;
    }

    public function getCreatedDate(): ?\DateTime
    {
        return $this->CreatedDate;
    }

    public function setCreatedDate(\DateTime $CreatedDate): static
    {
        $this->CreatedDate = $CreatedDate;

        return $this;
    }
}
