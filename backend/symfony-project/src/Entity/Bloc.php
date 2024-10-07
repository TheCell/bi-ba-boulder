<?php

namespace App\Entity;

use App\Repository\BlocRepository;
use Doctrine\Common\Collections\ArrayCollection;
use Doctrine\Common\Collections\Collection;
use Doctrine\DBAL\Types\Types;
use Doctrine\ORM\Mapping as ORM;

#[ORM\Entity(repositoryClass: BlocRepository::class)]
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
     * @var Collection<int, Sector>
     */
    #[ORM\OneToMany(targetEntity: Sector::class, mappedBy: 'blocs')]
    private Collection $sector;

    public function __construct()
    {
        $this->sector = new ArrayCollection();
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
     * @return Collection<int, Sector>
     */
    public function getSector(): Collection
    {
        return $this->sector;
    }

    public function addSector(Sector $sector): static
    {
        if (!$this->sector->contains($sector)) {
            $this->sector->add($sector);
            $sector->setBlocs($this);
        }

        return $this;
    }

    public function removeSector(Sector $sector): static
    {
        if ($this->sector->removeElement($sector)) {
            // set the owning side to null (unless already changed)
            if ($sector->getBlocs() === $this) {
                $sector->setBlocs(null);
            }
        }

        return $this;
    }
}
