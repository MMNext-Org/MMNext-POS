-- Migration: 013_AddMissingStockMovementColumns
-- Description: Align the StockMovements table with the StockMovement domain model.
-- GenericRepository builds INSERT/UPDATE statements by reflection from model properties,
-- so every model column must exist in the table. The StockMovement model (and EntityBase)
-- declares these columns that 001_InitialSchema.sql does not create:
--   FromLocationId/ToLocationId (transfer source/destination),
--   ProductId (single-product movements),
--   ReferenceId/ReferenceType (source document reference),
--   Quantity (movement quantity),
--   IsActive (active flag alongside Status),
--   CreatedBy/UpdatedBy (EntityBase audit user ids).
-- All added columns are nullable/defaulted to support legacy data.

SET @sql := (SELECT IF(
    (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
     WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'StockMovements' AND COLUMN_NAME = 'FromLocationId') = 0,
    'ALTER TABLE `StockMovements` ADD COLUMN `FromLocationId` INT NULL AFTER `LocationId`',
    'SELECT 1'));
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

SET @sql := (SELECT IF(
    (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
     WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'StockMovements' AND COLUMN_NAME = 'ToLocationId') = 0,
    'ALTER TABLE `StockMovements` ADD COLUMN `ToLocationId` INT NULL AFTER `FromLocationId`',
    'SELECT 1'));
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

SET @sql := (SELECT IF(
    (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
     WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'StockMovements' AND COLUMN_NAME = 'ProductId') = 0,
    'ALTER TABLE `StockMovements` ADD COLUMN `ProductId` INT NULL AFTER `CustomerId`',
    'SELECT 1'));
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

SET @sql := (SELECT IF(
    (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
     WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'StockMovements' AND COLUMN_NAME = 'ReferenceId') = 0,
    'ALTER TABLE `StockMovements` ADD COLUMN `ReferenceId` INT NULL AFTER `ProductId`',
    'SELECT 1'));
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

SET @sql := (SELECT IF(
    (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
     WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'StockMovements' AND COLUMN_NAME = 'ReferenceType') = 0,
    'ALTER TABLE `StockMovements` ADD COLUMN `ReferenceType` VARCHAR(20) NULL AFTER `ReferenceId`',
    'SELECT 1'));
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

SET @sql := (SELECT IF(
    (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
     WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'StockMovements' AND COLUMN_NAME = 'Quantity') = 0,
    'ALTER TABLE `StockMovements` ADD COLUMN `Quantity` INT NOT NULL DEFAULT 0 AFTER `ReferenceType`',
    'SELECT 1'));
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

SET @sql := (SELECT IF(
    (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
     WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'StockMovements' AND COLUMN_NAME = 'IsActive') = 0,
    'ALTER TABLE `StockMovements` ADD COLUMN `IsActive` BOOLEAN NOT NULL DEFAULT 1 AFTER `Status`',
    'SELECT 1'));
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

SET @sql := (SELECT IF(
    (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
     WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'StockMovements' AND COLUMN_NAME = 'CreatedBy') = 0,
    'ALTER TABLE `StockMovements` ADD COLUMN `CreatedBy` INT NULL AFTER `CreatedByUserId`',
    'SELECT 1'));
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

SET @sql := (SELECT IF(
    (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
     WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'StockMovements' AND COLUMN_NAME = 'UpdatedBy') = 0,
    'ALTER TABLE `StockMovements` ADD COLUMN `UpdatedBy` INT NULL AFTER `CreatedBy`',
    'SELECT 1'));
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;