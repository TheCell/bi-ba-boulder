<?php

namespace App\Entity;

use App\Repository\UserRepository;
use Doctrine\Common\Collections\ArrayCollection;
use Doctrine\Common\Collections\Collection;
use Doctrine\ORM\Mapping as ORM;
use Symfony\Bridge\Doctrine\Types\UuidType;
use Symfony\Component\Security\Core\User\PasswordAuthenticatedUserInterface;
use Symfony\Component\Security\Core\User\UserInterface;
use Symfony\Component\Uid\Uuid;

#[ORM\Entity(repositoryClass: UserRepository::class)]
#[ORM\UniqueConstraint(name: 'UNIQ_IDENTIFIER_EMAIL', fields: ['email'])]
class User implements UserInterface, PasswordAuthenticatedUserInterface
{
    #[ORM\Id]
    #[ORM\Column(type: UuidType::NAME, unique: true)]
    #[ORM\GeneratedValue(strategy: 'CUSTOM')]
    #[ORM\CustomIdGenerator(class: 'doctrine.uuid_generator')]
    private ?Uuid $id = null;

    #[ORM\Column(length: 180)]
    private ?string $email = null;

    /**
     * @var list<string> The user roles
     */
    #[ORM\Column]
    private array $roles = [];

    /**
     * @var string The hashed password
     */
    #[ORM\Column]
    private ?string $password = null;

    #[ORM\Column]
    private bool $isVerified = false;

    #[ORM\Column(nullable: true)]
    private ?\DateTime $VerifyMailSentTime = null;

    #[ORM\Column(length: 255, unique: true)]
    private ?string $username = null;

    /**
     * @var Collection<int, SpraywallProblem>
     */
    #[ORM\OneToMany(targetEntity: SpraywallProblem::class, mappedBy: 'CreatedBy')]
    private Collection $spraywallProblems;

    /**
     * @var Collection<int, BoulderLog>
     */
    #[ORM\OneToMany(targetEntity: BoulderLog::class, mappedBy: 'User')]
    private Collection $boulderLogs;

    /**
     * @var Collection<int, Bookmark>
     */
    #[ORM\OneToMany(targetEntity: Bookmark::class, mappedBy: 'User', orphanRemoval: true)]
    private Collection $bookmarks;

    public function __construct()
    {
        $this->spraywallProblems = new ArrayCollection();
        $this->boulderLogs = new ArrayCollection();
        $this->bookmarks = new ArrayCollection();
    }

    public function getId(): ?Uuid
    {
        return $this->id;
    }

    public function getEmail(): ?string
    {
        return $this->email;
    }

    public function setEmail(string $email): static
    {
        $this->email = $email;

        return $this;
    }

    /**
     * A visual identifier that represents this user.
     *
     * @see UserInterface
     */
    public function getUserIdentifier(): string
    {
        return (string) $this->email;
    }

    /**
     * @see UserInterface
     */
    public function getRoles(): array
    {
        $roles = $this->roles;

        return array_unique($roles);
    }

    /**
     * @param list<string> $roles
     */
    public function setRoles(array $roles): static
    {
        $this->roles = $roles;

        return $this;
    }

    /**
     * @see PasswordAuthenticatedUserInterface
     */
    public function getPassword(): ?string
    {
        return $this->password;
    }

    public function setPassword(string $password): static
    {
        $this->password = $password;

        return $this;
    }

    /**
     * Ensure the session doesn't contain actual password hashes by CRC32C-hashing them, as supported since Symfony 7.3.
     */
    public function __serialize(): array
    {
        $data = (array) $this;
        $data["\0" . self::class . "\0password"] = hash('crc32c', $this->password);
        
        return $data;
    }

    #[\Deprecated]
    public function eraseCredentials(): void
    {
        // @deprecated, to be removed when upgrading to Symfony 8
    }

    public function isVerified(): ?bool
    {
        return $this->isVerified;
    }

    public function setIsVerified(bool $isVerified): static
    {
        $this->isVerified = $isVerified;

        return $this;
    }

    public function getVerifyMailSentTime(): ?\DateTime
    {
        return $this->VerifyMailSentTime;
    }

    public function setVerifyMailSentTime(?\DateTime $VerifyMailSentTime): static
    {
        $this->VerifyMailSentTime = $VerifyMailSentTime;

        return $this;
    }

    public function getUsername(): ?string
    {
        return $this->username;
    }

    public function setUsername(string $username): static
    {
        $this->username = $username;

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
            $spraywallProblem->setCreatedBy($this);
        }

        return $this;
    }

    public function removeSpraywallProblem(SpraywallProblem $spraywallProblem): static
    {
        if ($this->spraywallProblems->removeElement($spraywallProblem)) {
            // set the owning side to null (unless already changed)
            if ($spraywallProblem->getCreatedBy() === $this) {
                $spraywallProblem->setCreatedBy(null);
            }
        }

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
            $boulderLog->setUser($this);
        }

        return $this;
    }

    public function removeBoulderLog(BoulderLog $boulderLog): static
    {
        if ($this->boulderLogs->removeElement($boulderLog)) {
            // set the owning side to null (unless already changed)
            if ($boulderLog->getUser() === $this) {
                $boulderLog->setUser(null);
            }
        }

        return $this;
    }

    /**
     * @return Collection<int, Bookmark>
     */
    public function getBookmarks(): Collection
    {
        return $this->bookmarks;
    }

    public function addBookmark(Bookmark $bookmark): static
    {
        if (!$this->bookmarks->contains($bookmark)) {
            $this->bookmarks->add($bookmark);
            $bookmark->setUser($this);
        }

        return $this;
    }

    public function removeBookmark(Bookmark $bookmark): static
    {
        if ($this->bookmarks->removeElement($bookmark)) {
            // set the owning side to null (unless already changed)
            if ($bookmark->getUser() === $this) {
                $bookmark->setUser(null);
            }
        }

        return $this;
    }
}
