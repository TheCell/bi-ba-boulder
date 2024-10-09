-- Doctrine Migration File Generated on 2024-10-08 18:45:50

-- Version DoctrineMigrations\Version20241007181343
CREATE TABLE bloc (id INT AUTO_INCREMENT NOT NULL, name VARCHAR(255) NOT NULL, description LONGTEXT DEFAULT NULL, PRIMARY KEY(id)) DEFAULT CHARACTER SET utf8mb4 COLLATE `utf8mb4_unicode_ci` ENGINE = InnoDB;
CREATE TABLE sector (id INT AUTO_INCREMENT NOT NULL, blocs_id INT DEFAULT NULL, name VARCHAR(255) NOT NULL, description LONGTEXT DEFAULT NULL, INDEX IDX_4BA3D9E87C40FD7C (blocs_id), PRIMARY KEY(id)) DEFAULT CHARACTER SET utf8mb4 COLLATE `utf8mb4_unicode_ci` ENGINE = InnoDB;
ALTER TABLE sector ADD CONSTRAINT FK_4BA3D9E87C40FD7C FOREIGN KEY (blocs_id) REFERENCES bloc (id);
-- Version DoctrineMigrations\Version20241007181343 update table metadata;
INSERT INTO doctrine_migration_versions (version, executed_at, execution_time) VALUES ('DoctrineMigrations\\Version20241007181343', '2024-10-08 18:45:50', 0);

-- Version DoctrineMigrations\Version20241007200906
CREATE TABLE line (id INT AUTO_INCREMENT NOT NULL, bloc_id INT NOT NULL, color VARCHAR(9) DEFAULT NULL, name VARCHAR(255) DEFAULT NULL, identifier VARCHAR(255) NOT NULL, description LONGTEXT DEFAULT NULL, points LONGTEXT DEFAULT NULL COMMENT '(DC2Type:simple_array)', INDEX IDX_D114B4F65582E9C0 (bloc_id), PRIMARY KEY(id)) DEFAULT CHARACTER SET utf8mb4 COLLATE `utf8mb4_unicode_ci` ENGINE = InnoDB;
ALTER TABLE line ADD CONSTRAINT FK_D114B4F65582E9C0 FOREIGN KEY (bloc_id) REFERENCES bloc (id);
ALTER TABLE bloc ADD sector_id INT NOT NULL;
ALTER TABLE bloc ADD CONSTRAINT FK_C778955ADE95C867 FOREIGN KEY (sector_id) REFERENCES sector (id);
CREATE INDEX IDX_C778955ADE95C867 ON bloc (sector_id);
ALTER TABLE sector DROP FOREIGN KEY FK_4BA3D9E87C40FD7C;
DROP INDEX IDX_4BA3D9E87C40FD7C ON sector;
ALTER TABLE sector DROP blocs_id;
-- Version DoctrineMigrations\Version20241007200906 update table metadata;
INSERT INTO doctrine_migration_versions (version, executed_at, execution_time) VALUES ('DoctrineMigrations\\Version20241007200906', '2024-10-08 18:45:50', 0);

-- Version DoctrineMigrations\Version20241007204531
CREATE TABLE point (id INT AUTO_INCREMENT NOT NULL, line_id INT NOT NULL, x DOUBLE PRECISION NOT NULL, y DOUBLE PRECISION NOT NULL, z DOUBLE PRECISION NOT NULL, INDEX IDX_B7A5F3244D7B7542 (line_id), PRIMARY KEY(id)) DEFAULT CHARACTER SET utf8mb4 COLLATE `utf8mb4_unicode_ci` ENGINE = InnoDB;
ALTER TABLE point ADD CONSTRAINT FK_B7A5F3244D7B7542 FOREIGN KEY (line_id) REFERENCES line (id);
ALTER TABLE line DROP points;
-- Version DoctrineMigrations\Version20241007204531 update table metadata;
INSERT INTO doctrine_migration_versions (version, executed_at, execution_time) VALUES ('DoctrineMigrations\\Version20241007204531', '2024-10-08 18:45:50', 0);
