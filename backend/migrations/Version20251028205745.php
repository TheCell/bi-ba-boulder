<?php

declare(strict_types=1);

namespace DoctrineMigrations;

use Doctrine\DBAL\Schema\Schema;
use Doctrine\Migrations\AbstractMigration;

/**
 * Auto-generated Migration: Please modify to your needs!
 */
final class Version20251028205745 extends AbstractMigration
{
    public function getDescription(): string
    {
        return '';
    }

    public function up(Schema $schema): void
    {
        // this up() migration is auto-generated, please modify it to your needs
        $this->addSql('CREATE TABLE spraywall (id INT AUTO_INCREMENT NOT NULL, name VARCHAR(512) NOT NULL, description LONGTEXT DEFAULT NULL, PRIMARY KEY(id)) DEFAULT CHARACTER SET utf8mb4 COLLATE `utf8mb4_unicode_ci` ENGINE = InnoDB');
        $this->addSql('CREATE TABLE spraywall_problem (id INT AUTO_INCREMENT NOT NULL, spraywall_id INT NOT NULL, name VARCHAR(512) NOT NULL, description LONGTEXT DEFAULT NULL, INDEX IDX_D547E14C82CD71EE (spraywall_id), PRIMARY KEY(id)) DEFAULT CHARACTER SET utf8mb4 COLLATE `utf8mb4_unicode_ci` ENGINE = InnoDB');
        $this->addSql('ALTER TABLE spraywall_problem ADD CONSTRAINT FK_D547E14C82CD71EE FOREIGN KEY (spraywall_id) REFERENCES spraywall (id)');
    }

    public function down(Schema $schema): void
    {
        // this down() migration is auto-generated, please modify it to your needs
        $this->addSql('ALTER TABLE spraywall_problem DROP FOREIGN KEY FK_D547E14C82CD71EE');
        $this->addSql('DROP TABLE spraywall');
        $this->addSql('DROP TABLE spraywall_problem');
    }
}
