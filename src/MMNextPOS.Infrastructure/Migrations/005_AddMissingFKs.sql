-- Migration 005: Add Missing Foreign Keys for Audit Fields
-- Applied by: DatabaseInitializer / MigrationRunner
-- Description: Adds foreign key constraints for CreatedBy/UpdatedBy fields referencing Users table
-- Also adds missing FK constraints on several tables

DELIMITER //

DROP PROCEDURE IF EXISTS AddMissingAuditFKs //
CREATE PROCEDURE AddMissingAuditFKs()
BEGIN
    DECLARE done INT DEFAULT FALSE;
    DECLARE fk_name VARCHAR(100);
    DECLARE table_name VARCHAR(100);
    DECLARE column_name VARCHAR(100);
    DECLARE ref_table VARCHAR(100);
    DECLARE ref_column VARCHAR(100);
    DECLARE on_delete VARCHAR(50);
    DECLARE cur CURSOR FOR
        SELECT 'FK_Products_CreatedBy' AS fk_name, 'Products' AS table_name, 'CreatedBy' AS column_name, 'Users' AS ref_table, 'Id' AS ref_column, 'SET NULL' AS on_delete
        UNION ALL SELECT 'FK_Products_UpdatedBy', 'Products', 'UpdatedBy', 'Users', 'Id', 'SET NULL'
        UNION ALL SELECT 'FK_Customers_CreatedBy', 'Customers', 'CreatedBy', 'Users', 'Id', 'SET NULL'
        UNION ALL SELECT 'FK_Customers_UpdatedBy', 'Customers', 'UpdatedBy', 'Users', 'Id', 'SET NULL'
        UNION ALL SELECT 'FK_StockMovements_CreatedByUserId', 'StockMovements', 'CreatedByUserId', 'Users', 'Id', 'SET NULL'
        UNION ALL SELECT 'FK_Assemblies_CreatedByUserId', 'Assemblies', 'CreatedByUserId', 'Users', 'Id', 'SET NULL'
        UNION ALL SELECT 'FK_Purchases_CreatedByUserId', 'Purchases', 'CreatedByUserId', 'Users', 'Id', 'SET NULL'
        UNION ALL SELECT 'FK_PurchaseReturns_CreatedByUserId', 'PurchaseReturns', 'CreatedByUserId', 'Users', 'Id', 'SET NULL'
        UNION ALL SELECT 'FK_Expenses_CreatedByUserId', 'Expenses', 'CreatedByUserId', 'Users', 'Id', 'SET NULL'
        UNION ALL SELECT 'FK_Invoices_CreatedByUserId', 'Invoices', 'CreatedByUserId', 'Users', 'Id', 'SET NULL'
        UNION ALL SELECT 'FK_SalesReturns_CreatedByUserId', 'SalesReturns', 'CreatedByUserId', 'Users', 'Id', 'SET NULL'
        UNION ALL SELECT 'FK_SaleReceipts_CreatedByUserId', 'SaleReceipts', 'CreatedByUserId', 'Users', 'Id', 'SET NULL'
        UNION ALL SELECT 'FK_PurchaseReceipts_CreatedByUserId', 'PurchaseReceipts', 'CreatedByUserId', 'Users', 'Id', 'SET NULL'
        UNION ALL SELECT 'FK_StockTransfers_CreatedByUserId', 'StockTransfers', 'CreatedByUserId', 'Users', 'Id', 'SET NULL'
        UNION ALL SELECT 'FK_StockTransfers_ReceivedByUserId', 'StockTransfers', 'ReceivedByUserId', 'Users', 'Id', 'SET NULL'
        UNION ALL SELECT 'FK_Payments_ReceivedByUserId', 'Payments', 'ReceivedByUserId', 'Users', 'Id', 'SET NULL'
        UNION ALL SELECT 'FK_SalePriceHistories_ChangedByUserId', 'SalePriceHistories', 'ChangedByUserId', 'Users', 'Id', 'SET NULL'
        UNION ALL SELECT 'FK_StarStockTransferReceiveds_ReceivedByUserId', 'StarStockTransferReceiveds', 'ReceivedByUserId', 'Users', 'Id', 'SET NULL'
        UNION ALL SELECT 'FK_StarSalePriceTransfers_AcceptedByUserId', 'StarSalePriceTransfers', 'AcceptedByUserId', 'Users', 'Id', 'SET NULL'
        UNION ALL SELECT 'FK_IssueHeaders_IssuedByUserId', 'IssueHeaders', 'IssuedByUserId', 'Users', 'Id', 'SET NULL'
        UNION ALL SELECT 'FK_IssueHeaders_ReceivedByUserId', 'IssueHeaders', 'ReceivedByUserId', 'Users', 'Id', 'SET NULL'
        UNION ALL SELECT 'FK_PaymentVouchers_CreatedByUserId', 'PaymentVouchers', 'CreatedByUserId', 'Users', 'Id', 'SET NULL'
        UNION ALL SELECT 'FK_DeviceRequests_ApprovedByUserId', 'DeviceRequests', 'ApprovedByUserId', 'Users', 'Id', 'SET NULL'
        UNION ALL SELECT 'FK_ClientUpdateRequests_ApprovedByUserId', 'ClientUpdateRequests', 'ApprovedByUserId', 'Users', 'Id', 'SET NULL'
        UNION ALL SELECT 'FK_SuperAdminLogs_ExecutedByUserId', 'SuperAdminLogs', 'ExecutedByUserId', 'Users', 'Id', 'SET NULL'
        UNION ALL SELECT 'FK_ChangeDateLogs_ChangedByUserId', 'ChangeDateLogs', 'ChangedByUserId', 'Users', 'Id', 'SET NULL';
    DECLARE CONTINUE HANDLER FOR NOT FOUND SET done = TRUE;

    OPEN cur;
    read_loop: LOOP
        FETCH cur INTO fk_name, table_name, column_name, ref_table, ref_column, on_delete;
        IF done THEN
            LEAVE read_loop;
        END IF;

        IF NOT EXISTS (
            SELECT 1 FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE
            WHERE TABLE_SCHEMA = DATABASE()
            AND TABLE_NAME = table_name
            AND CONSTRAINT_NAME = fk_name
        ) THEN
            -- Build ALTER TABLE statement directly using CONCAT
            SET @alter_stmt = CONCAT('ALTER TABLE ', table_name, ' ADD CONSTRAINT ', fk_name, ' FOREIGN KEY (', column_name, ') REFERENCES ', ref_table, '(', ref_column, ') ON DELETE ', on_delete);
            PREPARE stmt FROM @alter_stmt;
            EXECUTE stmt;
            DEALLOCATE PREPARE stmt;
        END IF;
    END LOOP;
    CLOSE cur;
END //

DELIMITER ;

-- Execute the procedure
CALL AddMissingAuditFKs();

-- Clean up
DROP PROCEDURE IF EXISTS AddMissingAuditFKs;