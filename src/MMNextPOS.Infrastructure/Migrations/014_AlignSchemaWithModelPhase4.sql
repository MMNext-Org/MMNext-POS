-- Migration: 014_AlignSchemaWithModelPhase4
-- Description: Align remaining tables with their domain models after the Phase 3/4
-- hardening additions. GenericRepository builds INSERT/UPDATE statements by reflection
-- from model properties, so every model column must exist in the table. This migration
-- adds, guarded (idempotent):
--   StockMovementDetails: BatchNumber, ExpiryDate, LocationId, ReferenceDetailId
--   Products:             Barcode, ReorderPoint, MaxStockLevel, SerialTracked, BatchTracked, ShelfLifeDays
--   Payments:             Description
--   Expenses:             Notes
--   StockTransfers:       CancelReason, CancelledByUserId, CancelledDate, ReleasedByUserId, ReleasedDate
--   StockTransferDetails: UnitPrice
-- All added columns are nullable/defaulted to support legacy data.

-- ── StockMovementDetails ─────────────────────────────────────────────────
SET @sql := (SELECT IF(
    (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
     WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'StockMovementDetails' AND COLUMN_NAME = 'BatchNumber') = 0,
    'ALTER TABLE `StockMovementDetails` ADD COLUMN `BatchNumber` VARCHAR(50) NULL AFTER `SerialNumber`',
    'SELECT 1'));
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

SET @sql := (SELECT IF(
    (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
     WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'StockMovementDetails' AND COLUMN_NAME = 'ExpiryDate') = 0,
    'ALTER TABLE `StockMovementDetails` ADD COLUMN `ExpiryDate` DATETIME NULL AFTER `BatchNumber`',
    'SELECT 1'));
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

SET @sql := (SELECT IF(
    (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
     WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'StockMovementDetails' AND COLUMN_NAME = 'LocationId') = 0,
    'ALTER TABLE `StockMovementDetails` ADD COLUMN `LocationId` INT NULL AFTER `ExpiryDate`',
    'SELECT 1'));
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

SET @sql := (SELECT IF(
    (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
     WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'StockMovementDetails' AND COLUMN_NAME = 'ReferenceDetailId') = 0,
    'ALTER TABLE `StockMovementDetails` ADD COLUMN `ReferenceDetailId` INT NULL AFTER `LocationId`',
    'SELECT 1'));
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- ── Products ─────────────────────────────────────────────────────────────
SET @sql := (SELECT IF(
    (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
     WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'Products' AND COLUMN_NAME = 'Barcode') = 0,
    'ALTER TABLE `Products` ADD COLUMN `Barcode` VARCHAR(100) NULL AFTER `Name`',
    'SELECT 1'));
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

SET @sql := (SELECT IF(
    (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
     WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'Products' AND COLUMN_NAME = 'ReorderPoint') = 0,
    'ALTER TABLE `Products` ADD COLUMN `ReorderPoint` INT NULL AFTER `StockQuantity`',
    'SELECT 1'));
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

SET @sql := (SELECT IF(
    (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
     WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'Products' AND COLUMN_NAME = 'MaxStockLevel') = 0,
    'ALTER TABLE `Products` ADD COLUMN `MaxStockLevel` INT NULL AFTER `ReorderPoint`',
    'SELECT 1'));
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

SET @sql := (SELECT IF(
    (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
     WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'Products' AND COLUMN_NAME = 'SerialTracked') = 0,
    'ALTER TABLE `Products` ADD COLUMN `SerialTracked` BOOLEAN NOT NULL DEFAULT 0 AFTER `MaxStockLevel`',
    'SELECT 1'));
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

SET @sql := (SELECT IF(
    (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
     WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'Products' AND COLUMN_NAME = 'BatchTracked') = 0,
    'ALTER TABLE `Products` ADD COLUMN `BatchTracked` BOOLEAN NOT NULL DEFAULT 0 AFTER `SerialTracked`',
    'SELECT 1'));
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

SET @sql := (SELECT IF(
    (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
     WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'Products' AND COLUMN_NAME = 'ShelfLifeDays') = 0,
    'ALTER TABLE `Products` ADD COLUMN `ShelfLifeDays` INT NULL AFTER `BatchTracked`',
    'SELECT 1'));
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- ── Payments ─────────────────────────────────────────────────────────────
SET @sql := (SELECT IF(
    (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
     WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'Payments' AND COLUMN_NAME = 'Description') = 0,
    'ALTER TABLE `Payments` ADD COLUMN `Description` VARCHAR(500) NULL AFTER `Notes`',
    'SELECT 1'));
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- ── Expenses ─────────────────────────────────────────────────────────────
SET @sql := (SELECT IF(
    (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
     WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'Expenses' AND COLUMN_NAME = 'Notes') = 0,
    'ALTER TABLE `Expenses` ADD COLUMN `Notes` VARCHAR(500) NULL AFTER `Description`',
    'SELECT 1'));
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- ── StockTransfers ───────────────────────────────────────────────────────
SET @sql := (SELECT IF(
    (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
     WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'StockTransfers' AND COLUMN_NAME = 'ReleasedByUserId') = 0,
    'ALTER TABLE `StockTransfers` ADD COLUMN `ReleasedByUserId` INT NULL AFTER `Status`',
    'SELECT 1'));
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

SET @sql := (SELECT IF(
    (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
     WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'StockTransfers' AND COLUMN_NAME = 'ReleasedDate') = 0,
    'ALTER TABLE `StockTransfers` ADD COLUMN `ReleasedDate` DATETIME NULL AFTER `ReleasedByUserId`',
    'SELECT 1'));
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

SET @sql := (SELECT IF(
    (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
     WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'StockTransfers' AND COLUMN_NAME = 'CancelledByUserId') = 0,
    'ALTER TABLE `StockTransfers` ADD COLUMN `CancelledByUserId` INT NULL AFTER `ReleasedDate`',
    'SELECT 1'));
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

SET @sql := (SELECT IF(
    (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
     WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'StockTransfers' AND COLUMN_NAME = 'CancelledDate') = 0,
    'ALTER TABLE `StockTransfers` ADD COLUMN `CancelledDate` DATETIME NULL AFTER `CancelledByUserId`',
    'SELECT 1'));
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

SET @sql := (SELECT IF(
    (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
     WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'StockTransfers' AND COLUMN_NAME = 'CancelReason') = 0,
    'ALTER TABLE `StockTransfers` ADD COLUMN `CancelReason` VARCHAR(500) NULL AFTER `CancelledDate`',
    'SELECT 1'));
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- ── StockTransferDetails ─────────────────────────────────────────────────
SET @sql := (SELECT IF(
    (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
     WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'StockTransferDetails' AND COLUMN_NAME = 'UnitPrice') = 0,
    'ALTER TABLE `StockTransferDetails` ADD COLUMN `UnitPrice` DECIMAL(18,2) NOT NULL DEFAULT 0 AFTER `Quantity`',
    'SELECT 1'));
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;