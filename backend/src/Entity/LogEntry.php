<?php

namespace App\Entity;

use App\Repository\LogEntryRepository;
use Doctrine\ORM\Mapping as ORM;
use Symfony\Component\Uid\Uuid;
use Symfony\Bridge\Doctrine\Types\UuidType;

#[ORM\Entity(repositoryClass: LogEntryRepository::class)]
class LogEntry
{
    #[ORM\Id]
    #[ORM\Column(type: UuidType::NAME, unique: true)]
    #[ORM\GeneratedValue(strategy: 'CUSTOM')]
    #[ORM\CustomIdGenerator(class: 'doctrine.uuid_generator')]
    private ?Uuid $id = null;

    #[ORM\Column]
    private ?\DateTime $Date = null;

    #[ORM\ManyToOne(inversedBy: 'logEntries')]
    private ?BoulderLog $BoulderLog = null;

    #[ORM\Column]
    private ?bool $isSend = null;

    #[ORM\Column]
    private ?bool $isAttempt = null;

    public function getId(): ?Uuid
    {
        return $this->id;
    }

    public function getDate(): ?\DateTime
    {
        return $this->Date;
    }

    public function setDate(\DateTime $Date): static
    {
        $this->Date = $Date;

        return $this;
    }

    public function getBoulderLog(): ?BoulderLog
    {
        return $this->BoulderLog;
    }

    public function setBoulderLog(?BoulderLog $BoulderLog): static
    {
        $this->BoulderLog = $BoulderLog;

        return $this;
    }

    public function isSend(): ?bool
    {
        return $this->isSend;
    }

    public function setIsSend(bool $isSend): static
    {
        $this->isSend = $isSend;

        return $this;
    }

    public function isAttempt(): ?bool
    {
        return $this->isAttempt;
    }

    public function setIsAttempt(bool $isAttempt): static
    {
        $this->isAttempt = $isAttempt;

        return $this;
    }
}
