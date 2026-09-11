-- Migration: 009_AddInvoiceNoToSales
-- Description: Add InvoiceNo column to Sales table to store auto-generated invoice numbers
-- (e.g., INV-2026-000123). The column is nullable to support legacy data migration.

SET @sql := (SELECT IF(
    (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
     WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'Sales' AND COLUMN_NAME = 'InvoiceNo') = 0,
    'ALTER TABLE `Sales` ADD COLUMN `InvoiceNo` VARCHAR(50) NULL AFTER `LocationId`',
    'SELECT 1'));

PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;