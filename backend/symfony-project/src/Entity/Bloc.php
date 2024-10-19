<?php

namespace App\Entity;

use App\Repository\BlocRepository;
use Doctrine\Common\Collections\ArrayCollection;
use Doctrine\Common\Collections\Collection;
use Doctrine\DBAL\Types\Types;
use Doctrine\ORM\Mapping as ORM;
use ApiPlatform\Metadata\ApiResource;
use ApiPlatform\Metadata\Get;
use App\DTO\BlocDto;

// #[ORM\Entity(repositoryClass: BlocRepository::class)]
#[ORM\Entity()]
#[ApiResource(
    operations: [
        new Get(uriTemplate: 'bloc/{id}', routeName: 'get-bloc', output: BlocDto::class)
    ]
)]
class Bloc
{
    #[ORM\Id]
    #[ORM\GeneratedValue]
    #[ORM\Column]
    private ?int $id = null;

    #[ORM\Column(length: 255)]
    private ?string $name = null;

    #[ORM\Column(type: Types::TEXT, nullable: true)]
    private ?string $description = null;

    /**
     * @var Collection<int, Line>
     */
    #[ORM\OneToMany(targetEntity: Line::class, mappedBy: 'bloc', orphanRemoval: true)]
    private Collection $boulderLines;

    #[ORM\ManyToOne(inversedBy: 'blocs')]
    #[ORM\JoinColumn(nullable: false)]
    private ?Sector $sector = null;

    #[ORM\Column(length: 2048, nullable: true)]
    private ?string $blocLowRes = null;

    #[ORM\Column(length: 2048, nullable: true)]
    private ?string $blocMedRes = null;

    #[ORM\Column(length: 2048, nullable: true)]
    private ?string $blocHighRes = null;

    public function __construct()
    {
        $this->boulderLines = new ArrayCollection();
    }

    public function getId(): ?int
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
     * @return Collection<int, Line>
     */
    public function getBoulderLines(): Collection
    {
        return $this->boulderLines;
    }

    public function addBoulderLine(Line $boulderLine): static
    {
        if (!$this->boulderLines->contains($boulderLine)) {
            $this->boulderLines->add($boulderLine);
            $boulderLine->setBloc($this);
        }

        return $this;
    }

    public function removeBoulderLine(Line $boulderLine): static
    {
        if ($this->boulderLines->removeElement($boulderLine)) {
            // set the owning side to null (unless already changed)
            if ($boulderLine->getBloc() === $this) {
                $boulderLine->setBloc(null);
            }
        }

        return $this;
    }

    public function getSector(): ?Sector
    {
        return $this->sector;
    }

    public function setSector(?Sector $sector): static
    {
        $this->sector = $sector;

        return $this;
    }

    public function getBlocLowRes(): ?string
    {
        return $this->blocLowRes;
    }

    public function setBlocLowRes(?string $blocLowRes): static
    {
        $this->blocLowRes = $blocLowRes;

        return $this;
    }

    public function getBlocMedRes(): ?string
    {
        return $this->blocMedRes;
    }

    public function setBlocMedRes(?string $blocMedRes): static
    {
        $this->blocMedRes = $blocMedRes;

        return $this;
    }

    public function getBlocHighRes(): ?string
    {
        return $this->blocHighRes;
    }

    public function setBlocHighRes(?string $blocHighRes): static
    {
        $this->blocHighRes = $blocHighRes;

        return $this;
    }
}
