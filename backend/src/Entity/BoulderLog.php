<?php

namespace App\Entity;

use App\Entity\Enum\FontGrade;
use App\Entity\Enum\Rating;
use App\Repository\BoulderLogRepository;
use Doctrine\Common\Collections\ArrayCollection;
use Doctrine\Common\Collections\Collection;
use Doctrine\ORM\Mapping as ORM;
use Symfony\Component\Uid\Uuid;
use Symfony\Bridge\Doctrine\Types\UuidType;

#[ORM\Entity(repositoryClass: BoulderLogRepository::class)]
class BoulderLog
{
    #[ORM\Id]
    #[ORM\Column(type: UuidType::NAME, unique: true)]
    #[ORM\GeneratedValue(strategy: 'CUSTOM')]
    #[ORM\CustomIdGenerator(class: 'doctrine.uuid_generator')]
    private ?Uuid $id = null;

    #[ORM\ManyToOne(inversedBy: 'boulderLogs')]
    #[ORM\JoinColumn(nullable: false)]
    private ?User $User = null;

    #[ORM\ManyToOne(inversedBy: 'boulderLogs')]
    private ?SpraywallProblem $SpraywallProblem = null;

    #[ORM\Column]
    private ?bool $isSent = null;

    #[ORM\Column]
    private ?bool $isProject = null;

    #[ORM\Column(nullable: true, enumType: Rating::class)]
    private ?Rating $Rating = null;

    #[ORM\Column(nullable: true, enumType: FontGrade::class)]
    private ?FontGrade $FontGrade = null;

    /**
     * @var Collection<int, LogEntry>
     */
    #[ORM\OneToMany(targetEntity: LogEntry::class, mappedBy: 'BoulderLog')]
    private Collection $logEntries;

    public function __construct()
    {
        $this->logEntries = new ArrayCollection();
    }

    public function getId(): ?Uuid
    {
        return $this->id;
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

    public function getSpraywallProblem(): ?SpraywallProblem
    {
        return $this->SpraywallProblem;
    }

    public function setSpraywallProblem(?SpraywallProblem $SpraywallProblem): static
    {
        $this->SpraywallProblem = $SpraywallProblem;

        return $this;
    }

    public function isSent(): ?bool
    {
        return $this->isSent;
    }

    public function setIsSent(bool $isSent): static
    {
        $this->isSent = $isSent;

        return $this;
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

    public function getRating(): ?Rating
    {
        return $this->Rating;
    }

    public function setRating(?Rating $Rating): static
    {
        $this->Rating = $Rating;

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

    /**
     * @return Collection<int, LogEntry>
     */
    public function getLogEntries(): Collection
    {
        return $this->logEntries;
    }

    public function addLogEntry(LogEntry $logEntry): static
    {
        if (!$this->logEntries->contains($logEntry)) {
            $this->logEntries->add($logEntry);
            $logEntry->setBoulderLog($this);
        }

        return $this;
    }

    public function removeLogEntry(LogEntry $logEntry): static
    {
        if ($this->logEntries->removeElement($logEntry)) {
            // set the owning side to null (unless already changed)
            if ($logEntry->getBoulderLog() === $this) {
                $logEntry->setBoulderLog(null);
            }
        }

        return $this;
    }
}
