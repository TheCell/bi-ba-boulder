-- Doctrine Migration File Generated on 2025-11-23 12:21:10

-- Version DoctrineMigrations\Version20251123112030
ALTER TABLE user ADD verify_mail_sent_time DATETIME DEFAULT NULL;
