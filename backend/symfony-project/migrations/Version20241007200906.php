<?php

declare(strict_types=1);

namespace DoctrineMigrations;

use Doctrine\DBAL\Schema\Schema;
use Doctrine\Migrations\AbstractMigration;

/**
 * Auto-generated Migration: Please modify to your needs!
 */
final class Version20241007200906 extends AbstractMigration
{
    public function getDescription(): string
    {
        return '';
    }

    public function up(Schema $schema): void
    {
        // this up() migration is auto-generated, please modify it to your needs
        $this->addSql('CREATE TABLE line (id INT AUTO_INCREMENT NOT NULL, bloc_id INT NOT NULL, color VARCHAR(9) DEFAULT NULL, name VARCHAR(255) DEFAULT NULL, identifier VARCHAR(255) NOT NULL, description LONGTEXT DEFAULT NULL, points LONGTEXT DEFAULT NULL COMMENT \'(DC2Type:simple_array)\', INDEX IDX_D114B4F65582E9C0 (bloc_id), PRIMARY KEY(id)) DEFAULT CHARACTER SET utf8mb4 COLLATE `utf8mb4_unicode_ci` ENGINE = InnoDB');
        $this->addSql('ALTER TABLE line ADD CONSTRAINT FK_D114B4F65582E9C0 FOREIGN KEY (bloc_id) REFERENCES bloc (id)');
        $this->addSql('ALTER TABLE bloc ADD sector_id INT NOT NULL');
        $this->addSql('ALTER TABLE bloc ADD CONSTRAINT FK_C778955ADE95C867 FOREIGN KEY (sector_id) REFERENCES sector (id)');
        $this->addSql('CREATE INDEX IDX_C778955ADE95C867 ON bloc (sector_id)');
        $this->addSql('ALTER TABLE sector DROP FOREIGN KEY FK_4BA3D9E87C40FD7C');
        $this->addSql('DROP INDEX IDX_4BA3D9E87C40FD7C ON sector');
        $this->addSql('ALTER TABLE sector DROP blocs_id');
    }

    public function down(Schema $schema): void
    {
        // this down() migration is auto-generated, please modify it to your needs
        $this->addSql('ALTER TABLE line DROP FOREIGN KEY FK_D114B4F65582E9C0');
        $this->addSql('DROP TABLE line');
        $this->addSql('ALTER TABLE bloc DROP FOREIGN KEY FK_C778955ADE95C867');
        $this->addSql('DROP INDEX IDX_C778955ADE95C867 ON bloc');
        $this->addSql('ALTER TABLE bloc DROP sector_id');
        $this->addSql('ALTER TABLE sector ADD blocs_id INT DEFAULT NULL');
        $this->addSql('ALTER TABLE sector ADD CONSTRAINT FK_4BA3D9E87C40FD7C FOREIGN KEY (blocs_id) REFERENCES bloc (id)');
        $this->addSql('CREATE INDEX IDX_4BA3D9E87C40FD7C ON sector (blocs_id)');
    }
}
