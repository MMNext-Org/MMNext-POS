-- Migration 003: Add Missing Columns to Sales Table
-- Applied by: DatabaseInitializer / MigrationRunner
-- Description: Adds Status and LocationId columns to Sales table for backward compatibility
-- These columns are required by SaleRepository but may be missing in databases created by earlier versions

-- Add Status column to Sales table if not exists
SET @sql = (
    SELECT IF(
        EXISTS (
            SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
            WHERE TABLE_SCHEMA = DATABASE()
            AND TABLE_NAME = 'Sales'
            AND COLUMN_NAME = 'Status'
        ),
        'SELECT ''Column Status already exists'' AS Result;',
        'ALTER TABLE Sales ADD COLUMN Status VARCHAR(50) NULL;'
    )
);
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- Add LocationId column to Sales table if not exists
SET @sql = (
    SELECT IF(
        EXISTS (
            SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
            WHERE TABLE_SCHEMA = DATABASE()
            AND TABLE_NAME = 'Sales'
            AND COLUMN_NAME = 'LocationId'
        ),
        'SELECT ''Column LocationId already exists'' AS Result;',
        'ALTER TABLE Sales ADD COLUMN LocationId INT NULL;'
    )
);
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- Add foreign key for LocationId if not exists
SET @sql = (
    SELECT IF(
        EXISTS (
            SELECT 1 FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE
            WHERE TABLE_SCHEMA = DATABASE()
            AND TABLE_NAME = 'Sales'
            AND COLUMN_NAME = 'LocationId'
            AND REFERENCED_TABLE_NAME = 'Locations'
        ),
        'SELECT ''FK Sales_Location already exists'' AS Result;',
        'ALTER TABLE Sales ADD CONSTRAINT FK_Sales_Location FOREIGN KEY (LocationId) REFERENCES Locations(Id) ON DELETE SET NULL;'
    )
);
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;