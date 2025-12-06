<?php

declare(strict_types=1);

namespace DoctrineMigrations;

use Doctrine\DBAL\Schema\Schema;
use Doctrine\Migrations\AbstractMigration;

/**
 * Auto-generated Migration: Please modify to your needs!
 */
final class Version20251206113517 extends AbstractMigration
{
    public function getDescription(): string
    {
        return '';
    }

    public function up(Schema $schema): void
    {
        // this up() migration is auto-generated, please modify it to your needs
        $this->addSql('CREATE TABLE bookmark (id BINARY(16) NOT NULL COMMENT \'(DC2Type:uuid)\', user_id BINARY(16) NOT NULL COMMENT \'(DC2Type:uuid)\', is_project TINYINT(1) NOT NULL, is_favourite TINYINT(1) NOT NULL, INDEX IDX_DA62921DA76ED395 (user_id), PRIMARY KEY(id)) DEFAULT CHARACTER SET utf8mb4 COLLATE `utf8mb4_unicode_ci` ENGINE = InnoDB');
        $this->addSql('CREATE TABLE boulder_log (id BINARY(16) NOT NULL COMMENT \'(DC2Type:uuid)\', user_id BINARY(16) NOT NULL COMMENT \'(DC2Type:uuid)\', spraywall_problem_id BINARY(16) DEFAULT NULL COMMENT \'(DC2Type:uuid)\', is_sent TINYINT(1) NOT NULL, is_project TINYINT(1) NOT NULL, rating INT DEFAULT NULL, font_grade VARCHAR(255) DEFAULT NULL, INDEX IDX_39611915A76ED395 (user_id), INDEX IDX_39611915B01CA7DF (spraywall_problem_id), PRIMARY KEY(id)) DEFAULT CHARACTER SET utf8mb4 COLLATE `utf8mb4_unicode_ci` ENGINE = InnoDB');
        $this->addSql('CREATE TABLE log_entry (id BINARY(16) NOT NULL COMMENT \'(DC2Type:uuid)\', boulder_log_id BINARY(16) DEFAULT NULL COMMENT \'(DC2Type:uuid)\', date DATETIME NOT NULL, is_send TINYINT(1) NOT NULL, is_attempt TINYINT(1) NOT NULL, INDEX IDX_B5F762DDD195C44 (boulder_log_id), PRIMARY KEY(id)) DEFAULT CHARACTER SET utf8mb4 COLLATE `utf8mb4_unicode_ci` ENGINE = InnoDB');
        $this->addSql('ALTER TABLE bookmark ADD CONSTRAINT FK_DA62921DA76ED395 FOREIGN KEY (user_id) REFERENCES user (id)');
        $this->addSql('ALTER TABLE boulder_log ADD CONSTRAINT FK_39611915A76ED395 FOREIGN KEY (user_id) REFERENCES user (id)');
        $this->addSql('ALTER TABLE boulder_log ADD CONSTRAINT FK_39611915B01CA7DF FOREIGN KEY (spraywall_problem_id) REFERENCES spraywall_problem (id)');
        $this->addSql('ALTER TABLE log_entry ADD CONSTRAINT FK_B5F762DDD195C44 FOREIGN KEY (boulder_log_id) REFERENCES boulder_log (id)');
        $this->addSql('ALTER TABLE spraywall_problem ADD created_by_id BINARY(16) NOT NULL COMMENT \'(DC2Type:uuid)\', ADD font_grade VARCHAR(255) DEFAULT NULL, ADD created_date DATETIME NOT NULL');
        $this->addSql('ALTER TABLE spraywall_problem ADD CONSTRAINT FK_D547E14CB03A8386 FOREIGN KEY (created_by_id) REFERENCES user (id)');
        $this->addSql('CREATE INDEX IDX_D547E14CB03A8386 ON spraywall_problem (created_by_id)');
    }

    public function down(Schema $schema): void
    {
        // this down() migration is auto-generated, please modify it to your needs
        $this->addSql('ALTER TABLE bookmark DROP FOREIGN KEY FK_DA62921DA76ED395');
        $this->addSql('ALTER TABLE boulder_log DROP FOREIGN KEY FK_39611915A76ED395');
        $this->addSql('ALTER TABLE boulder_log DROP FOREIGN KEY FK_39611915B01CA7DF');
        $this->addSql('ALTER TABLE log_entry DROP FOREIGN KEY FK_B5F762DDD195C44');
        $this->addSql('DROP TABLE bookmark');
        $this->addSql('DROP TABLE boulder_log');
        $this->addSql('DROP TABLE log_entry');
        $this->addSql('ALTER TABLE spraywall_problem DROP FOREIGN KEY FK_D547E14CB03A8386');
        $this->addSql('DROP INDEX IDX_D547E14CB03A8386 ON spraywall_problem');
        $this->addSql('ALTER TABLE spraywall_problem DROP created_by_id, DROP font_grade, DROP created_date');
    }
}
