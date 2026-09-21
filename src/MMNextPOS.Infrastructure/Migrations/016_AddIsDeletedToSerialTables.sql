-- Migration: 016_AddIsDeletedToSerialTables
-- Description: Add IsDeleted column to SerialNumbers and SerialTrackings tables
-- to match EntityBase.IsDeleted on the SerialNumber/SerialTracking domain models
-- (SerialBatches already has IsDeleted from migration 015).

SET @sql := (SELECT IF(
    (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
     WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'SerialNumbers' AND COLUMN_NAME = 'IsDeleted') = 0,
    'ALTER TABLE `SerialNumbers` ADD COLUMN `IsDeleted` TINYINT(1) NOT NULL DEFAULT 0 AFTER `IsActive`',
    'SELECT 1'));

PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

SET @sql := (SELECT IF(
    (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
     WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'SerialTrackings' AND COLUMN_NAME = 'IsDeleted') = 0,
    'ALTER TABLE `SerialTrackings` ADD COLUMN `IsDeleted` TINYINT(1) NOT NULL DEFAULT 0 AFTER `Notes`',
    'SELECT 1'));

PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;
