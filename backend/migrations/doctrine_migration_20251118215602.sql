-- Doctrine Migration File Generated on 2025-11-18 21:56:02

-- Version DoctrineMigrations\Version20251118205534
ALTER TABLE user CHANGE id id BINARY(16) NOT NULL COMMENT '(DC2Type:uuid)';
