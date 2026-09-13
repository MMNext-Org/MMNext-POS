-- Migration: 011_AddCustomerTypeToCustomer
-- Description: Add CustomerType column to Customers table for tax jurisdiction lookup.
-- Used by ITaxRateService/TaxRateService to determine the applicable tax rate by customer
-- type (Retail, Wholesale, TaxExempt, Government, Export). The column is nullable to
-- support legacy data and to default existing customers to Retail behavior via code.

SET @sql := (SELECT IF(
    (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
     WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'Customers' AND COLUMN_NAME = 'CustomerType') = 0,
    'ALTER TABLE `Customers` ADD COLUMN `CustomerType` VARCHAR(20) NULL AFTER `Email`',
    'SELECT 1'));

PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;