<?php

declare(strict_types=1);

namespace DoctrineMigrations;

use Doctrine\DBAL\Schema\Schema;
use Doctrine\Migrations\AbstractMigration;

/**
 * Auto-generated Migration: Please modify to your needs!
 */
final class Version20241020215213 extends AbstractMigration
{
    public function getDescription(): string
    {
        return '';
    }

    public function up(Schema $schema): void
    {
        // this up() migration is auto-generated, please modify it to your needs
        $this->addSql('CREATE TABLE bloc (id INT AUTO_INCREMENT NOT NULL, sector_id INT NOT NULL, name VARCHAR(255) NOT NULL, description LONGTEXT DEFAULT NULL, bloc_low_res VARCHAR(2048) DEFAULT NULL, bloc_med_res VARCHAR(2048) DEFAULT NULL, bloc_high_res VARCHAR(2048) DEFAULT NULL, INDEX IDX_C778955ADE95C867 (sector_id), PRIMARY KEY(id)) DEFAULT CHARACTER SET utf8mb4 COLLATE `utf8mb4_unicode_ci` ENGINE = InnoDB');
        $this->addSql('CREATE TABLE line (id INT AUTO_INCREMENT NOT NULL, bloc_id INT NOT NULL, color VARCHAR(9) DEFAULT NULL, name VARCHAR(255) DEFAULT NULL, identifier VARCHAR(255) NOT NULL, description LONGTEXT DEFAULT NULL, INDEX IDX_D114B4F65582E9C0 (bloc_id), PRIMARY KEY(id)) DEFAULT CHARACTER SET utf8mb4 COLLATE `utf8mb4_unicode_ci` ENGINE = InnoDB');
        $this->addSql('CREATE TABLE point (id INT AUTO_INCREMENT NOT NULL, line_id INT NOT NULL, x DOUBLE PRECISION NOT NULL, y DOUBLE PRECISION NOT NULL, z DOUBLE PRECISION NOT NULL, INDEX IDX_B7A5F3244D7B7542 (line_id), PRIMARY KEY(id)) DEFAULT CHARACTER SET utf8mb4 COLLATE `utf8mb4_unicode_ci` ENGINE = InnoDB');
        $this->addSql('CREATE TABLE sector (id INT AUTO_INCREMENT NOT NULL, name VARCHAR(255) NOT NULL, description LONGTEXT DEFAULT NULL, PRIMARY KEY(id)) DEFAULT CHARACTER SET utf8mb4 COLLATE `utf8mb4_unicode_ci` ENGINE = InnoDB');
        $this->addSql('ALTER TABLE bloc ADD CONSTRAINT FK_C778955ADE95C867 FOREIGN KEY (sector_id) REFERENCES sector (id)');
        $this->addSql('ALTER TABLE line ADD CONSTRAINT FK_D114B4F65582E9C0 FOREIGN KEY (bloc_id) REFERENCES bloc (id)');
        $this->addSql('ALTER TABLE point ADD CONSTRAINT FK_B7A5F3244D7B7542 FOREIGN KEY (line_id) REFERENCES line (id)');
    }

    public function down(Schema $schema): void
    {
        // this down() migration is auto-generated, please modify it to your needs
        $this->addSql('ALTER TABLE bloc DROP FOREIGN KEY FK_C778955ADE95C867');
        $this->addSql('ALTER TABLE line DROP FOREIGN KEY FK_D114B4F65582E9C0');
        $this->addSql('ALTER TABLE point DROP FOREIGN KEY FK_B7A5F3244D7B7542');
        $this->addSql('DROP TABLE bloc');
        $this->addSql('DROP TABLE line');
        $this->addSql('DROP TABLE point');
        $this->addSql('DROP TABLE sector');
    }
}
