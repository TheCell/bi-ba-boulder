-- Doctrine Migration File Generated on 2024-10-08 21:18:16

-- Version DoctrineMigrations\Version20241008191800
ALTER TABLE bloc ADD bloc_low_res VARCHAR(2048) DEFAULT NULL, ADD bloc_med_res VARCHAR(2048) DEFAULT NULL, ADD bloc_high_res VARCHAR(2048) DEFAULT NULL;
-- Version DoctrineMigrations\Version20241008191800 update table metadata;
INSERT INTO doctrine_migration_versions (version, executed_at, execution_time) VALUES ('DoctrineMigrations\\Version20241008191800', '2024-10-08 21:18:16', 0);
