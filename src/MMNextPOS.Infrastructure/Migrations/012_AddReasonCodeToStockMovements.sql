-- Migration: 012_AddReasonCodeToStockMovements
-- Description: Add ReasonCode column to StockMovements table. The StockMovement model
-- includes a ReasonCode property (e.g., DMG, LST, EXP, ADJ codes used by Phase 4
-- inventory workflows), and GenericRepository's reflection-based INSERT includes it,
-- so the column must exist to avoid "Unknown column" errors on movement writes.
-- The column is nullable to support legacy data.

SET @sql := (SELECT IF(
    (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
     WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'StockMovements' AND COLUMN_NAME = 'ReasonCode') = 0,
    'ALTER TABLE `StockMovements` ADD COLUMN `ReasonCode` VARCHAR(50) NULL AFTER `Reason`',
    'SELECT 1'));

PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;