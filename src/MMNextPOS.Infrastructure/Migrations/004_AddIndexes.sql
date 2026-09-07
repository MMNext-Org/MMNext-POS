-- Migration 004: Add Performance Indexes
-- Applied by: DatabaseInitializer / MigrationRunner
-- Description: Adds missing indexes for query performance on frequently filtered/joined columns
-- Idempotent: each index is created only if INFORMATION_SCHEMA.STATISTICS reports it missing.
-- (MySQL 8 has no CREATE INDEX IF NOT EXISTS; guarded via user variables + PREPARE. No trailing ; inside prepared strings.)

-- IX_SaleTemps_Status on SaleTemps(Status)
SET @ddl := (SELECT IF((SELECT COUNT(*) FROM INFORMATION_SCHEMA.STATISTICS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'SaleTemps' AND INDEX_NAME = 'IX_SaleTemps_Status') > 0, 'SELECT 1', 'CREATE INDEX IX_SaleTemps_Status ON SaleTemps (Status)'));
PREPARE stmt FROM @ddl; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- IX_SaleTemps_LocationId on SaleTemps(LocationId)
SET @ddl := (SELECT IF((SELECT COUNT(*) FROM INFORMATION_SCHEMA.STATISTICS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'SaleTemps' AND INDEX_NAME = 'IX_SaleTemps_LocationId') > 0, 'SELECT 1', 'CREATE INDEX IX_SaleTemps_LocationId ON SaleTemps (LocationId)'));
PREPARE stmt FROM @ddl; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- IX_SaleTemps_SaleDate on SaleTemps(SaleDate)
SET @ddl := (SELECT IF((SELECT COUNT(*) FROM INFORMATION_SCHEMA.STATISTICS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'SaleTemps' AND INDEX_NAME = 'IX_SaleTemps_SaleDate') > 0, 'SELECT 1', 'CREATE INDEX IX_SaleTemps_SaleDate ON SaleTemps (SaleDate)'));
PREPARE stmt FROM @ddl; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- IX_SaleTemps_CreatedByUserId on SaleTemps(CreatedByUserId)
SET @ddl := (SELECT IF((SELECT COUNT(*) FROM INFORMATION_SCHEMA.STATISTICS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'SaleTemps' AND INDEX_NAME = 'IX_SaleTemps_CreatedByUserId') > 0, 'SELECT 1', 'CREATE INDEX IX_SaleTemps_CreatedByUserId ON SaleTemps (CreatedByUserId)'));
PREPARE stmt FROM @ddl; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- IX_SaleTemps_CustomerId on SaleTemps(CustomerId)
SET @ddl := (SELECT IF((SELECT COUNT(*) FROM INFORMATION_SCHEMA.STATISTICS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'SaleTemps' AND INDEX_NAME = 'IX_SaleTemps_CustomerId') > 0, 'SELECT 1', 'CREATE INDEX IX_SaleTemps_CustomerId ON SaleTemps (CustomerId)'));
PREPARE stmt FROM @ddl; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- IX_Sales_LocationId on Sales(LocationId)
SET @ddl := (SELECT IF((SELECT COUNT(*) FROM INFORMATION_SCHEMA.STATISTICS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'Sales' AND INDEX_NAME = 'IX_Sales_LocationId') > 0, 'SELECT 1', 'CREATE INDEX IX_Sales_LocationId ON Sales (LocationId)'));
PREPARE stmt FROM @ddl; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- IX_Sales_SaleDate on Sales(SaleDate)
SET @ddl := (SELECT IF((SELECT COUNT(*) FROM INFORMATION_SCHEMA.STATISTICS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'Sales' AND INDEX_NAME = 'IX_Sales_SaleDate') > 0, 'SELECT 1', 'CREATE INDEX IX_Sales_SaleDate ON Sales (SaleDate)'));
PREPARE stmt FROM @ddl; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- IX_Sales_Status on Sales(Status)
SET @ddl := (SELECT IF((SELECT COUNT(*) FROM INFORMATION_SCHEMA.STATISTICS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'Sales' AND INDEX_NAME = 'IX_Sales_Status') > 0, 'SELECT 1', 'CREATE INDEX IX_Sales_Status ON Sales (Status)'));
PREPARE stmt FROM @ddl; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- IX_Sales_CustomerId_SaleDate on Sales(CustomerId, SaleDate)
SET @ddl := (SELECT IF((SELECT COUNT(*) FROM INFORMATION_SCHEMA.STATISTICS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'Sales' AND INDEX_NAME = 'IX_Sales_CustomerId_SaleDate') > 0, 'SELECT 1', 'CREATE INDEX IX_Sales_CustomerId_SaleDate ON Sales (CustomerId, SaleDate)'));
PREPARE stmt FROM @ddl; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- IX_SaleDetails_ProductId on SaleDetails(ProductId)
SET @ddl := (SELECT IF((SELECT COUNT(*) FROM INFORMATION_SCHEMA.STATISTICS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'SaleDetails' AND INDEX_NAME = 'IX_SaleDetails_ProductId') > 0, 'SELECT 1', 'CREATE INDEX IX_SaleDetails_ProductId ON SaleDetails (ProductId)'));
PREPARE stmt FROM @ddl; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- IX_StockMovements_MovementDate on StockMovements(MovementDate)
SET @ddl := (SELECT IF((SELECT COUNT(*) FROM INFORMATION_SCHEMA.STATISTICS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'StockMovements' AND INDEX_NAME = 'IX_StockMovements_MovementDate') > 0, 'SELECT 1', 'CREATE INDEX IX_StockMovements_MovementDate ON StockMovements (MovementDate)'));
PREPARE stmt FROM @ddl; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- IX_StockMovements_MovementType on StockMovements(MovementType)
SET @ddl := (SELECT IF((SELECT COUNT(*) FROM INFORMATION_SCHEMA.STATISTICS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'StockMovements' AND INDEX_NAME = 'IX_StockMovements_MovementType') > 0, 'SELECT 1', 'CREATE INDEX IX_StockMovements_MovementType ON StockMovements (MovementType)'));
PREPARE stmt FROM @ddl; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- IX_StockMovements_LocationId on StockMovements(LocationId)
SET @ddl := (SELECT IF((SELECT COUNT(*) FROM INFORMATION_SCHEMA.STATISTICS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'StockMovements' AND INDEX_NAME = 'IX_StockMovements_LocationId') > 0, 'SELECT 1', 'CREATE INDEX IX_StockMovements_LocationId ON StockMovements (LocationId)'));
PREPARE stmt FROM @ddl; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- IX_StockMovements_SupplierId on StockMovements(SupplierId)
SET @ddl := (SELECT IF((SELECT COUNT(*) FROM INFORMATION_SCHEMA.STATISTICS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'StockMovements' AND INDEX_NAME = 'IX_StockMovements_SupplierId') > 0, 'SELECT 1', 'CREATE INDEX IX_StockMovements_SupplierId ON StockMovements (SupplierId)'));
PREPARE stmt FROM @ddl; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- IX_StockMovements_CustomerId on StockMovements(CustomerId)
SET @ddl := (SELECT IF((SELECT COUNT(*) FROM INFORMATION_SCHEMA.STATISTICS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'StockMovements' AND INDEX_NAME = 'IX_StockMovements_CustomerId') > 0, 'SELECT 1', 'CREATE INDEX IX_StockMovements_CustomerId ON StockMovements (CustomerId)'));
PREPARE stmt FROM @ddl; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- IX_StockMovements_Status on StockMovements(Status)
SET @ddl := (SELECT IF((SELECT COUNT(*) FROM INFORMATION_SCHEMA.STATISTICS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'StockMovements' AND INDEX_NAME = 'IX_StockMovements_Status') > 0, 'SELECT 1', 'CREATE INDEX IX_StockMovements_Status ON StockMovements (Status)'));
PREPARE stmt FROM @ddl; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- IX_StockMovements_MovementNo on StockMovements(MovementNo)
SET @ddl := (SELECT IF((SELECT COUNT(*) FROM INFORMATION_SCHEMA.STATISTICS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'StockMovements' AND INDEX_NAME = 'IX_StockMovements_MovementNo') > 0, 'SELECT 1', 'CREATE INDEX IX_StockMovements_MovementNo ON StockMovements (MovementNo)'));
PREPARE stmt FROM @ddl; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- IX_StockMovementDetails_ProductId on StockMovementDetails(ProductId)
SET @ddl := (SELECT IF((SELECT COUNT(*) FROM INFORMATION_SCHEMA.STATISTICS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'StockMovementDetails' AND INDEX_NAME = 'IX_StockMovementDetails_ProductId') > 0, 'SELECT 1', 'CREATE INDEX IX_StockMovementDetails_ProductId ON StockMovementDetails (ProductId)'));
PREPARE stmt FROM @ddl; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- IX_Purchases_SupplierId on Purchases(SupplierId)
SET @ddl := (SELECT IF((SELECT COUNT(*) FROM INFORMATION_SCHEMA.STATISTICS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'Purchases' AND INDEX_NAME = 'IX_Purchases_SupplierId') > 0, 'SELECT 1', 'CREATE INDEX IX_Purchases_SupplierId ON Purchases (SupplierId)'));
PREPARE stmt FROM @ddl; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- IX_Purchases_LocationId on Purchases(LocationId)
SET @ddl := (SELECT IF((SELECT COUNT(*) FROM INFORMATION_SCHEMA.STATISTICS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'Purchases' AND INDEX_NAME = 'IX_Purchases_LocationId') > 0, 'SELECT 1', 'CREATE INDEX IX_Purchases_LocationId ON Purchases (LocationId)'));
PREPARE stmt FROM @ddl; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- IX_Purchases_PurchaseDate on Purchases(PurchaseDate)
SET @ddl := (SELECT IF((SELECT COUNT(*) FROM INFORMATION_SCHEMA.STATISTICS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'Purchases' AND INDEX_NAME = 'IX_Purchases_PurchaseDate') > 0, 'SELECT 1', 'CREATE INDEX IX_Purchases_PurchaseDate ON Purchases (PurchaseDate)'));
PREPARE stmt FROM @ddl; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- IX_Purchases_Status on Purchases(Status)
SET @ddl := (SELECT IF((SELECT COUNT(*) FROM INFORMATION_SCHEMA.STATISTICS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'Purchases' AND INDEX_NAME = 'IX_Purchases_Status') > 0, 'SELECT 1', 'CREATE INDEX IX_Purchases_Status ON Purchases (Status)'));
PREPARE stmt FROM @ddl; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- IX_Purchases_InvoiceNo on Purchases(InvoiceNo)
SET @ddl := (SELECT IF((SELECT COUNT(*) FROM INFORMATION_SCHEMA.STATISTICS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'Purchases' AND INDEX_NAME = 'IX_Purchases_InvoiceNo') > 0, 'SELECT 1', 'CREATE INDEX IX_Purchases_InvoiceNo ON Purchases (InvoiceNo)'));
PREPARE stmt FROM @ddl; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- IX_PurchaseDetails_ProductId on PurchaseDetails(ProductId)
SET @ddl := (SELECT IF((SELECT COUNT(*) FROM INFORMATION_SCHEMA.STATISTICS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'PurchaseDetails' AND INDEX_NAME = 'IX_PurchaseDetails_ProductId') > 0, 'SELECT 1', 'CREATE INDEX IX_PurchaseDetails_ProductId ON PurchaseDetails (ProductId)'));
PREPARE stmt FROM @ddl; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- IX_Payments_PaymentDate on Payments(PaymentDate)
SET @ddl := (SELECT IF((SELECT COUNT(*) FROM INFORMATION_SCHEMA.STATISTICS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'Payments' AND INDEX_NAME = 'IX_Payments_PaymentDate') > 0, 'SELECT 1', 'CREATE INDEX IX_Payments_PaymentDate ON Payments (PaymentDate)'));
PREPARE stmt FROM @ddl; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- IX_Payments_CustomerId on Payments(CustomerId)
SET @ddl := (SELECT IF((SELECT COUNT(*) FROM INFORMATION_SCHEMA.STATISTICS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'Payments' AND INDEX_NAME = 'IX_Payments_CustomerId') > 0, 'SELECT 1', 'CREATE INDEX IX_Payments_CustomerId ON Payments (CustomerId)'));
PREPARE stmt FROM @ddl; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- IX_Payments_SupplierId on Payments(SupplierId)
SET @ddl := (SELECT IF((SELECT COUNT(*) FROM INFORMATION_SCHEMA.STATISTICS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'Payments' AND INDEX_NAME = 'IX_Payments_SupplierId') > 0, 'SELECT 1', 'CREATE INDEX IX_Payments_SupplierId ON Payments (SupplierId)'));
PREPARE stmt FROM @ddl; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- IX_Payments_SaleId on Payments(SaleId)
SET @ddl := (SELECT IF((SELECT COUNT(*) FROM INFORMATION_SCHEMA.STATISTICS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'Payments' AND INDEX_NAME = 'IX_Payments_SaleId') > 0, 'SELECT 1', 'CREATE INDEX IX_Payments_SaleId ON Payments (SaleId)'));
PREPARE stmt FROM @ddl; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- IX_Payments_PurchaseId on Payments(PurchaseId)
SET @ddl := (SELECT IF((SELECT COUNT(*) FROM INFORMATION_SCHEMA.STATISTICS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'Payments' AND INDEX_NAME = 'IX_Payments_PurchaseId') > 0, 'SELECT 1', 'CREATE INDEX IX_Payments_PurchaseId ON Payments (PurchaseId)'));
PREPARE stmt FROM @ddl; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- IX_Payments_PaymentNo on Payments(PaymentNo)
SET @ddl := (SELECT IF((SELECT COUNT(*) FROM INFORMATION_SCHEMA.STATISTICS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'Payments' AND INDEX_NAME = 'IX_Payments_PaymentNo') > 0, 'SELECT 1', 'CREATE INDEX IX_Payments_PaymentNo ON Payments (PaymentNo)'));
PREPARE stmt FROM @ddl; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- IX_Payments_Status on Payments(Status)
SET @ddl := (SELECT IF((SELECT COUNT(*) FROM INFORMATION_SCHEMA.STATISTICS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'Payments' AND INDEX_NAME = 'IX_Payments_Status') > 0, 'SELECT 1', 'CREATE INDEX IX_Payments_Status ON Payments (Status)'));
PREPARE stmt FROM @ddl; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- IX_CustomerOutstandings_CustomerId_Status on CustomerOutstandings(CustomerId, Status)
SET @ddl := (SELECT IF((SELECT COUNT(*) FROM INFORMATION_SCHEMA.STATISTICS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'CustomerOutstandings' AND INDEX_NAME = 'IX_CustomerOutstandings_CustomerId_Status') > 0, 'SELECT 1', 'CREATE INDEX IX_CustomerOutstandings_CustomerId_Status ON CustomerOutstandings (CustomerId, Status)'));
PREPARE stmt FROM @ddl; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- IX_CustomerOutstandings_TransactionDate on CustomerOutstandings(TransactionDate)
SET @ddl := (SELECT IF((SELECT COUNT(*) FROM INFORMATION_SCHEMA.STATISTICS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'CustomerOutstandings' AND INDEX_NAME = 'IX_CustomerOutstandings_TransactionDate') > 0, 'SELECT 1', 'CREATE INDEX IX_CustomerOutstandings_TransactionDate ON CustomerOutstandings (TransactionDate)'));
PREPARE stmt FROM @ddl; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- IX_CustomerOutstandings_SaleId on CustomerOutstandings(SaleId)
SET @ddl := (SELECT IF((SELECT COUNT(*) FROM INFORMATION_SCHEMA.STATISTICS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'CustomerOutstandings' AND INDEX_NAME = 'IX_CustomerOutstandings_SaleId') > 0, 'SELECT 1', 'CREATE INDEX IX_CustomerOutstandings_SaleId ON CustomerOutstandings (SaleId)'));
PREPARE stmt FROM @ddl; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- IX_SupplierOutstandings_SupplierId_Status on SupplierOutstandings(SupplierId, Status)
SET @ddl := (SELECT IF((SELECT COUNT(*) FROM INFORMATION_SCHEMA.STATISTICS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'SupplierOutstandings' AND INDEX_NAME = 'IX_SupplierOutstandings_SupplierId_Status') > 0, 'SELECT 1', 'CREATE INDEX IX_SupplierOutstandings_SupplierId_Status ON SupplierOutstandings (SupplierId, Status)'));
PREPARE stmt FROM @ddl; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- IX_SupplierOutstandings_TransactionDate on SupplierOutstandings(TransactionDate)
SET @ddl := (SELECT IF((SELECT COUNT(*) FROM INFORMATION_SCHEMA.STATISTICS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'SupplierOutstandings' AND INDEX_NAME = 'IX_SupplierOutstandings_TransactionDate') > 0, 'SELECT 1', 'CREATE INDEX IX_SupplierOutstandings_TransactionDate ON SupplierOutstandings (TransactionDate)'));
PREPARE stmt FROM @ddl; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- IX_SupplierOutstandings_PurchaseId on SupplierOutstandings(PurchaseId)
SET @ddl := (SELECT IF((SELECT COUNT(*) FROM INFORMATION_SCHEMA.STATISTICS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'SupplierOutstandings' AND INDEX_NAME = 'IX_SupplierOutstandings_PurchaseId') > 0, 'SELECT 1', 'CREATE INDEX IX_SupplierOutstandings_PurchaseId ON SupplierOutstandings (PurchaseId)'));
PREPARE stmt FROM @ddl; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- IX_Expenses_ExpenseDate on Expenses(ExpenseDate)
SET @ddl := (SELECT IF((SELECT COUNT(*) FROM INFORMATION_SCHEMA.STATISTICS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'Expenses' AND INDEX_NAME = 'IX_Expenses_ExpenseDate') > 0, 'SELECT 1', 'CREATE INDEX IX_Expenses_ExpenseDate ON Expenses (ExpenseDate)'));
PREPARE stmt FROM @ddl; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- IX_Expenses_ExpenseTypeId on Expenses(ExpenseTypeId)
SET @ddl := (SELECT IF((SELECT COUNT(*) FROM INFORMATION_SCHEMA.STATISTICS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'Expenses' AND INDEX_NAME = 'IX_Expenses_ExpenseTypeId') > 0, 'SELECT 1', 'CREATE INDEX IX_Expenses_ExpenseTypeId ON Expenses (ExpenseTypeId)'));
PREPARE stmt FROM @ddl; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- IX_Expenses_LocationId on Expenses(LocationId)
SET @ddl := (SELECT IF((SELECT COUNT(*) FROM INFORMATION_SCHEMA.STATISTICS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'Expenses' AND INDEX_NAME = 'IX_Expenses_LocationId') > 0, 'SELECT 1', 'CREATE INDEX IX_Expenses_LocationId ON Expenses (LocationId)'));
PREPARE stmt FROM @ddl; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- IX_Expenses_VendorId on Expenses(VendorId)
SET @ddl := (SELECT IF((SELECT COUNT(*) FROM INFORMATION_SCHEMA.STATISTICS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'Expenses' AND INDEX_NAME = 'IX_Expenses_VendorId') > 0, 'SELECT 1', 'CREATE INDEX IX_Expenses_VendorId ON Expenses (VendorId)'));
PREPARE stmt FROM @ddl; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- IX_Expenses_Status on Expenses(Status)
SET @ddl := (SELECT IF((SELECT COUNT(*) FROM INFORMATION_SCHEMA.STATISTICS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'Expenses' AND INDEX_NAME = 'IX_Expenses_Status') > 0, 'SELECT 1', 'CREATE INDEX IX_Expenses_Status ON Expenses (Status)'));
PREPARE stmt FROM @ddl; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- IX_Products_Sku on Products(Sku)
SET @ddl := (SELECT IF((SELECT COUNT(*) FROM INFORMATION_SCHEMA.STATISTICS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'Products' AND INDEX_NAME = 'IX_Products_Sku') > 0, 'SELECT 1', 'CREATE INDEX IX_Products_Sku ON Products (Sku)'));
PREPARE stmt FROM @ddl; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- IX_Products_IsActive_IsDeleted on Products(IsActive, IsDeleted)
SET @ddl := (SELECT IF((SELECT COUNT(*) FROM INFORMATION_SCHEMA.STATISTICS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'Products' AND INDEX_NAME = 'IX_Products_IsActive_IsDeleted') > 0, 'SELECT 1', 'CREATE INDEX IX_Products_IsActive_IsDeleted ON Products (IsActive, IsDeleted)'));
PREPARE stmt FROM @ddl; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- IX_Customers_Code on Customers(Code)
SET @ddl := (SELECT IF((SELECT COUNT(*) FROM INFORMATION_SCHEMA.STATISTICS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'Customers' AND INDEX_NAME = 'IX_Customers_Code') > 0, 'SELECT 1', 'CREATE INDEX IX_Customers_Code ON Customers (Code)'));
PREPARE stmt FROM @ddl; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- IX_Customers_IsActive_IsDeleted on Customers(IsActive, IsDeleted)
SET @ddl := (SELECT IF((SELECT COUNT(*) FROM INFORMATION_SCHEMA.STATISTICS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'Customers' AND INDEX_NAME = 'IX_Customers_IsActive_IsDeleted') > 0, 'SELECT 1', 'CREATE INDEX IX_Customers_IsActive_IsDeleted ON Customers (IsActive, IsDeleted)'));
PREPARE stmt FROM @ddl; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- IX_Suppliers_Code on Suppliers(Code)
SET @ddl := (SELECT IF((SELECT COUNT(*) FROM INFORMATION_SCHEMA.STATISTICS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'Suppliers' AND INDEX_NAME = 'IX_Suppliers_Code') > 0, 'SELECT 1', 'CREATE INDEX IX_Suppliers_Code ON Suppliers (Code)'));
PREPARE stmt FROM @ddl; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- IX_Suppliers_IsActive_IsDeleted on Suppliers(IsActive, IsDeleted)
SET @ddl := (SELECT IF((SELECT COUNT(*) FROM INFORMATION_SCHEMA.STATISTICS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'Suppliers' AND INDEX_NAME = 'IX_Suppliers_IsActive_IsDeleted') > 0, 'SELECT 1', 'CREATE INDEX IX_Suppliers_IsActive_IsDeleted ON Suppliers (IsActive, IsDeleted)'));
PREPARE stmt FROM @ddl; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- IX_Users_Username on Users(Username)
SET @ddl := (SELECT IF((SELECT COUNT(*) FROM INFORMATION_SCHEMA.STATISTICS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'Users' AND INDEX_NAME = 'IX_Users_Username') > 0, 'SELECT 1', 'CREATE INDEX IX_Users_Username ON Users (Username)'));
PREPARE stmt FROM @ddl; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- IX_Users_Email on Users(Email)
SET @ddl := (SELECT IF((SELECT COUNT(*) FROM INFORMATION_SCHEMA.STATISTICS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'Users' AND INDEX_NAME = 'IX_Users_Email') > 0, 'SELECT 1', 'CREATE INDEX IX_Users_Email ON Users (Email)'));
PREPARE stmt FROM @ddl; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- IX_Users_IsActive_IsDeleted on Users(IsActive, IsDeleted)
SET @ddl := (SELECT IF((SELECT COUNT(*) FROM INFORMATION_SCHEMA.STATISTICS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'Users' AND INDEX_NAME = 'IX_Users_IsActive_IsDeleted') > 0, 'SELECT 1', 'CREATE INDEX IX_Users_IsActive_IsDeleted ON Users (IsActive, IsDeleted)'));
PREPARE stmt FROM @ddl; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- IX_Users_LocationId on Users(LocationId)
SET @ddl := (SELECT IF((SELECT COUNT(*) FROM INFORMATION_SCHEMA.STATISTICS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'Users' AND INDEX_NAME = 'IX_Users_LocationId') > 0, 'SELECT 1', 'CREATE INDEX IX_Users_LocationId ON Users (LocationId)'));
PREPARE stmt FROM @ddl; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- IX_Assemblies_AssemblyDate on Assemblies(AssemblyDate)
SET @ddl := (SELECT IF((SELECT COUNT(*) FROM INFORMATION_SCHEMA.STATISTICS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'Assemblies' AND INDEX_NAME = 'IX_Assemblies_AssemblyDate') > 0, 'SELECT 1', 'CREATE INDEX IX_Assemblies_AssemblyDate ON Assemblies (AssemblyDate)'));
PREPARE stmt FROM @ddl; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- IX_Assemblies_LocationId on Assemblies(LocationId)
SET @ddl := (SELECT IF((SELECT COUNT(*) FROM INFORMATION_SCHEMA.STATISTICS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'Assemblies' AND INDEX_NAME = 'IX_Assemblies_LocationId') > 0, 'SELECT 1', 'CREATE INDEX IX_Assemblies_LocationId ON Assemblies (LocationId)'));
PREPARE stmt FROM @ddl; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- IX_Assemblies_OutputProductId on Assemblies(OutputProductId)
SET @ddl := (SELECT IF((SELECT COUNT(*) FROM INFORMATION_SCHEMA.STATISTICS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'Assemblies' AND INDEX_NAME = 'IX_Assemblies_OutputProductId') > 0, 'SELECT 1', 'CREATE INDEX IX_Assemblies_OutputProductId ON Assemblies (OutputProductId)'));
PREPARE stmt FROM @ddl; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- IX_AssemblyDetails_ComponentProductId on AssemblyDetails(ComponentProductId)
SET @ddl := (SELECT IF((SELECT COUNT(*) FROM INFORMATION_SCHEMA.STATISTICS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'AssemblyDetails' AND INDEX_NAME = 'IX_AssemblyDetails_ComponentProductId') > 0, 'SELECT 1', 'CREATE INDEX IX_AssemblyDetails_ComponentProductId ON AssemblyDetails (ComponentProductId)'));
PREPARE stmt FROM @ddl; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- IX_SalePriceHistories_ProductId_EffectiveDate on SalePriceHistories(ProductId, EffectiveDate)
SET @ddl := (SELECT IF((SELECT COUNT(*) FROM INFORMATION_SCHEMA.STATISTICS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'SalePriceHistories' AND INDEX_NAME = 'IX_SalePriceHistories_ProductId_EffectiveDate') > 0, 'SELECT 1', 'CREATE INDEX IX_SalePriceHistories_ProductId_EffectiveDate ON SalePriceHistories (ProductId, EffectiveDate)'));
PREPARE stmt FROM @ddl; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- IX_Invoices_CustomerId on Invoices(CustomerId)
SET @ddl := (SELECT IF((SELECT COUNT(*) FROM INFORMATION_SCHEMA.STATISTICS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'Invoices' AND INDEX_NAME = 'IX_Invoices_CustomerId') > 0, 'SELECT 1', 'CREATE INDEX IX_Invoices_CustomerId ON Invoices (CustomerId)'));
PREPARE stmt FROM @ddl; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- IX_Invoices_InvoiceDate on Invoices(InvoiceDate)
SET @ddl := (SELECT IF((SELECT COUNT(*) FROM INFORMATION_SCHEMA.STATISTICS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'Invoices' AND INDEX_NAME = 'IX_Invoices_InvoiceDate') > 0, 'SELECT 1', 'CREATE INDEX IX_Invoices_InvoiceDate ON Invoices (InvoiceDate)'));
PREPARE stmt FROM @ddl; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- IX_Invoices_Status on Invoices(Status)
SET @ddl := (SELECT IF((SELECT COUNT(*) FROM INFORMATION_SCHEMA.STATISTICS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'Invoices' AND INDEX_NAME = 'IX_Invoices_Status') > 0, 'SELECT 1', 'CREATE INDEX IX_Invoices_Status ON Invoices (Status)'));
PREPARE stmt FROM @ddl; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- IX_SalesReturns_SaleId on SalesReturns(SaleId)
SET @ddl := (SELECT IF((SELECT COUNT(*) FROM INFORMATION_SCHEMA.STATISTICS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'SalesReturns' AND INDEX_NAME = 'IX_SalesReturns_SaleId') > 0, 'SELECT 1', 'CREATE INDEX IX_SalesReturns_SaleId ON SalesReturns (SaleId)'));
PREPARE stmt FROM @ddl; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- IX_SalesReturns_CustomerId on SalesReturns(CustomerId)
SET @ddl := (SELECT IF((SELECT COUNT(*) FROM INFORMATION_SCHEMA.STATISTICS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'SalesReturns' AND INDEX_NAME = 'IX_SalesReturns_CustomerId') > 0, 'SELECT 1', 'CREATE INDEX IX_SalesReturns_CustomerId ON SalesReturns (CustomerId)'));
PREPARE stmt FROM @ddl; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- IX_SalesReturns_ReturnDate on SalesReturns(ReturnDate)
SET @ddl := (SELECT IF((SELECT COUNT(*) FROM INFORMATION_SCHEMA.STATISTICS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'SalesReturns' AND INDEX_NAME = 'IX_SalesReturns_ReturnDate') > 0, 'SELECT 1', 'CREATE INDEX IX_SalesReturns_ReturnDate ON SalesReturns (ReturnDate)'));
PREPARE stmt FROM @ddl; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- IX_SalesReturnDetails_ProductId on SalesReturnDetails(ProductId)
SET @ddl := (SELECT IF((SELECT COUNT(*) FROM INFORMATION_SCHEMA.STATISTICS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'SalesReturnDetails' AND INDEX_NAME = 'IX_SalesReturnDetails_ProductId') > 0, 'SELECT 1', 'CREATE INDEX IX_SalesReturnDetails_ProductId ON SalesReturnDetails (ProductId)'));
PREPARE stmt FROM @ddl; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- IX_SalesReturnDetails_SaleDetailId on SalesReturnDetails(SaleDetailId)
SET @ddl := (SELECT IF((SELECT COUNT(*) FROM INFORMATION_SCHEMA.STATISTICS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'SalesReturnDetails' AND INDEX_NAME = 'IX_SalesReturnDetails_SaleDetailId') > 0, 'SELECT 1', 'CREATE INDEX IX_SalesReturnDetails_SaleDetailId ON SalesReturnDetails (SaleDetailId)'));
PREPARE stmt FROM @ddl; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- IX_StockTransfers_FromLocationId on StockTransfers(FromLocationId)
SET @ddl := (SELECT IF((SELECT COUNT(*) FROM INFORMATION_SCHEMA.STATISTICS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'StockTransfers' AND INDEX_NAME = 'IX_StockTransfers_FromLocationId') > 0, 'SELECT 1', 'CREATE INDEX IX_StockTransfers_FromLocationId ON StockTransfers (FromLocationId)'));
PREPARE stmt FROM @ddl; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- IX_StockTransfers_ToLocationId on StockTransfers(ToLocationId)
SET @ddl := (SELECT IF((SELECT COUNT(*) FROM INFORMATION_SCHEMA.STATISTICS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'StockTransfers' AND INDEX_NAME = 'IX_StockTransfers_ToLocationId') > 0, 'SELECT 1', 'CREATE INDEX IX_StockTransfers_ToLocationId ON StockTransfers (ToLocationId)'));
PREPARE stmt FROM @ddl; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- IX_StockTransfers_TransferDate on StockTransfers(TransferDate)
SET @ddl := (SELECT IF((SELECT COUNT(*) FROM INFORMATION_SCHEMA.STATISTICS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'StockTransfers' AND INDEX_NAME = 'IX_StockTransfers_TransferDate') > 0, 'SELECT 1', 'CREATE INDEX IX_StockTransfers_TransferDate ON StockTransfers (TransferDate)'));
PREPARE stmt FROM @ddl; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- IX_StockTransfers_Status on StockTransfers(Status)
SET @ddl := (SELECT IF((SELECT COUNT(*) FROM INFORMATION_SCHEMA.STATISTICS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'StockTransfers' AND INDEX_NAME = 'IX_StockTransfers_Status') > 0, 'SELECT 1', 'CREATE INDEX IX_StockTransfers_Status ON StockTransfers (Status)'));
PREPARE stmt FROM @ddl; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- IX_StockTransferDetails_ProductId on StockTransferDetails(ProductId)
SET @ddl := (SELECT IF((SELECT COUNT(*) FROM INFORMATION_SCHEMA.STATISTICS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'StockTransferDetails' AND INDEX_NAME = 'IX_StockTransferDetails_ProductId') > 0, 'SELECT 1', 'CREATE INDEX IX_StockTransferDetails_ProductId ON StockTransferDetails (ProductId)'));
PREPARE stmt FROM @ddl; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- IX_StarStockTransferReceiveds_FromLocationId on StarStockTransferReceiveds(FromLocationId)
SET @ddl := (SELECT IF((SELECT COUNT(*) FROM INFORMATION_SCHEMA.STATISTICS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'StarStockTransferReceiveds' AND INDEX_NAME = 'IX_StarStockTransferReceiveds_FromLocationId') > 0, 'SELECT 1', 'CREATE INDEX IX_StarStockTransferReceiveds_FromLocationId ON StarStockTransferReceiveds (FromLocationId)'));
PREPARE stmt FROM @ddl; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- IX_StarStockTransferReceiveds_ToLocationId on StarStockTransferReceiveds(ToLocationId)
SET @ddl := (SELECT IF((SELECT COUNT(*) FROM INFORMATION_SCHEMA.STATISTICS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'StarStockTransferReceiveds' AND INDEX_NAME = 'IX_StarStockTransferReceiveds_ToLocationId') > 0, 'SELECT 1', 'CREATE INDEX IX_StarStockTransferReceiveds_ToLocationId ON StarStockTransferReceiveds (ToLocationId)'));
PREPARE stmt FROM @ddl; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- IX_StarSalePriceTransfers_FromLocationId on StarSalePriceTransfers(FromLocationId)
SET @ddl := (SELECT IF((SELECT COUNT(*) FROM INFORMATION_SCHEMA.STATISTICS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'StarSalePriceTransfers' AND INDEX_NAME = 'IX_StarSalePriceTransfers_FromLocationId') > 0, 'SELECT 1', 'CREATE INDEX IX_StarSalePriceTransfers_FromLocationId ON StarSalePriceTransfers (FromLocationId)'));
PREPARE stmt FROM @ddl; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- IX_StarSalePriceTransfers_ToLocationId on StarSalePriceTransfers(ToLocationId)
SET @ddl := (SELECT IF((SELECT COUNT(*) FROM INFORMATION_SCHEMA.STATISTICS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'StarSalePriceTransfers' AND INDEX_NAME = 'IX_StarSalePriceTransfers_ToLocationId') > 0, 'SELECT 1', 'CREATE INDEX IX_StarSalePriceTransfers_ToLocationId ON StarSalePriceTransfers (ToLocationId)'));
PREPARE stmt FROM @ddl; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- IX_StarCashFlowReports_LocationId_ReportDate on StarCashFlowReports(LocationId, ReportDate)
SET @ddl := (SELECT IF((SELECT COUNT(*) FROM INFORMATION_SCHEMA.STATISTICS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'StarCashFlowReports' AND INDEX_NAME = 'IX_StarCashFlowReports_LocationId_ReportDate') > 0, 'SELECT 1', 'CREATE INDEX IX_StarCashFlowReports_LocationId_ReportDate ON StarCashFlowReports (LocationId, ReportDate)'));
PREPARE stmt FROM @ddl; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- IX_StarProfitLossReports_LocationId_FromDate_ToDate on StarProfitLossReports(LocationId, FromDate, ToDate)
SET @ddl := (SELECT IF((SELECT COUNT(*) FROM INFORMATION_SCHEMA.STATISTICS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'StarProfitLossReports' AND INDEX_NAME = 'IX_StarProfitLossReports_LocationId_FromDate_ToDate') > 0, 'SELECT 1', 'CREATE INDEX IX_StarProfitLossReports_LocationId_FromDate_ToDate ON StarProfitLossReports (LocationId, FromDate, ToDate)'));
PREPARE stmt FROM @ddl; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- IX_StarStockBalanceReports_LocationId_ProductId on StarStockBalanceReports(LocationId, ProductId)
SET @ddl := (SELECT IF((SELECT COUNT(*) FROM INFORMATION_SCHEMA.STATISTICS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'StarStockBalanceReports' AND INDEX_NAME = 'IX_StarStockBalanceReports_LocationId_ProductId') > 0, 'SELECT 1', 'CREATE INDEX IX_StarStockBalanceReports_LocationId_ProductId ON StarStockBalanceReports (LocationId, ProductId)'));
PREPARE stmt FROM @ddl; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- IX_StarReorderReports_LocationId_ProductId on StarReorderReports(LocationId, ProductId)
SET @ddl := (SELECT IF((SELECT COUNT(*) FROM INFORMATION_SCHEMA.STATISTICS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'StarReorderReports' AND INDEX_NAME = 'IX_StarReorderReports_LocationId_ProductId') > 0, 'SELECT 1', 'CREATE INDEX IX_StarReorderReports_LocationId_ProductId ON StarReorderReports (LocationId, ProductId)'));
PREPARE stmt FROM @ddl; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- IX_StarOutstandingReports_LocationId_PartyType_PartyId on StarOutstandingReports(LocationId, PartyType, PartyId)
SET @ddl := (SELECT IF((SELECT COUNT(*) FROM INFORMATION_SCHEMA.STATISTICS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'StarOutstandingReports' AND INDEX_NAME = 'IX_StarOutstandingReports_LocationId_PartyType_PartyId') > 0, 'SELECT 1', 'CREATE INDEX IX_StarOutstandingReports_LocationId_PartyType_PartyId ON StarOutstandingReports (LocationId, PartyType, PartyId)'));
PREPARE stmt FROM @ddl; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- IX_IssueHeaders_FromLocationId on IssueHeaders(FromLocationId)
SET @ddl := (SELECT IF((SELECT COUNT(*) FROM INFORMATION_SCHEMA.STATISTICS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'IssueHeaders' AND INDEX_NAME = 'IX_IssueHeaders_FromLocationId') > 0, 'SELECT 1', 'CREATE INDEX IX_IssueHeaders_FromLocationId ON IssueHeaders (FromLocationId)'));
PREPARE stmt FROM @ddl; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- IX_IssueHeaders_ToLocationId on IssueHeaders(ToLocationId)
SET @ddl := (SELECT IF((SELECT COUNT(*) FROM INFORMATION_SCHEMA.STATISTICS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'IssueHeaders' AND INDEX_NAME = 'IX_IssueHeaders_ToLocationId') > 0, 'SELECT 1', 'CREATE INDEX IX_IssueHeaders_ToLocationId ON IssueHeaders (ToLocationId)'));
PREPARE stmt FROM @ddl; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- IX_IssueHeaders_IssueDate on IssueHeaders(IssueDate)
SET @ddl := (SELECT IF((SELECT COUNT(*) FROM INFORMATION_SCHEMA.STATISTICS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'IssueHeaders' AND INDEX_NAME = 'IX_IssueHeaders_IssueDate') > 0, 'SELECT 1', 'CREATE INDEX IX_IssueHeaders_IssueDate ON IssueHeaders (IssueDate)'));
PREPARE stmt FROM @ddl; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- IX_DataMigrations_Status on DataMigrations(Status)
SET @ddl := (SELECT IF((SELECT COUNT(*) FROM INFORMATION_SCHEMA.STATISTICS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'DataMigrations' AND INDEX_NAME = 'IX_DataMigrations_Status') > 0, 'SELECT 1', 'CREATE INDEX IX_DataMigrations_Status ON DataMigrations (Status)'));
PREPARE stmt FROM @ddl; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- IX_DataMigrations_IsScheduled_ScheduledAt on DataMigrations(IsScheduled, ScheduledAt)
SET @ddl := (SELECT IF((SELECT COUNT(*) FROM INFORMATION_SCHEMA.STATISTICS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'DataMigrations' AND INDEX_NAME = 'IX_DataMigrations_IsScheduled_ScheduledAt') > 0, 'SELECT 1', 'CREATE INDEX IX_DataMigrations_IsScheduled_ScheduledAt ON DataMigrations (IsScheduled, ScheduledAt)'));
PREPARE stmt FROM @ddl; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- IX_DeviceInfos_RegistrationId on DeviceInfos(RegistrationId)
SET @ddl := (SELECT IF((SELECT COUNT(*) FROM INFORMATION_SCHEMA.STATISTICS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'DeviceInfos' AND INDEX_NAME = 'IX_DeviceInfos_RegistrationId') > 0, 'SELECT 1', 'CREATE INDEX IX_DeviceInfos_RegistrationId ON DeviceInfos (RegistrationId)'));
PREPARE stmt FROM @ddl; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- IX_DeviceRequests_DeviceId on DeviceRequests(DeviceId)
SET @ddl := (SELECT IF((SELECT COUNT(*) FROM INFORMATION_SCHEMA.STATISTICS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'DeviceRequests' AND INDEX_NAME = 'IX_DeviceRequests_DeviceId') > 0, 'SELECT 1', 'CREATE INDEX IX_DeviceRequests_DeviceId ON DeviceRequests (DeviceId)'));
PREPARE stmt FROM @ddl; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- IX_DeviceRequests_Status on DeviceRequests(Status)
SET @ddl := (SELECT IF((SELECT COUNT(*) FROM INFORMATION_SCHEMA.STATISTICS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'DeviceRequests' AND INDEX_NAME = 'IX_DeviceRequests_Status') > 0, 'SELECT 1', 'CREATE INDEX IX_DeviceRequests_Status ON DeviceRequests (Status)'));
PREPARE stmt FROM @ddl; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- IX_PcClients_DeviceId on PcClients(DeviceId)
SET @ddl := (SELECT IF((SELECT COUNT(*) FROM INFORMATION_SCHEMA.STATISTICS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'PcClients' AND INDEX_NAME = 'IX_PcClients_DeviceId') > 0, 'SELECT 1', 'CREATE INDEX IX_PcClients_DeviceId ON PcClients (DeviceId)'));
PREPARE stmt FROM @ddl; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- IX_PcClients_UserId on PcClients(UserId)
SET @ddl := (SELECT IF((SELECT COUNT(*) FROM INFORMATION_SCHEMA.STATISTICS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'PcClients' AND INDEX_NAME = 'IX_PcClients_UserId') > 0, 'SELECT 1', 'CREATE INDEX IX_PcClients_UserId ON PcClients (UserId)'));
PREPARE stmt FROM @ddl; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- IX_MobileClients_DeviceId on MobileClients(DeviceId)
SET @ddl := (SELECT IF((SELECT COUNT(*) FROM INFORMATION_SCHEMA.STATISTICS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'MobileClients' AND INDEX_NAME = 'IX_MobileClients_DeviceId') > 0, 'SELECT 1', 'CREATE INDEX IX_MobileClients_DeviceId ON MobileClients (DeviceId)'));
PREPARE stmt FROM @ddl; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- IX_MobileClients_UserId on MobileClients(UserId)
SET @ddl := (SELECT IF((SELECT COUNT(*) FROM INFORMATION_SCHEMA.STATISTICS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'MobileClients' AND INDEX_NAME = 'IX_MobileClients_UserId') > 0, 'SELECT 1', 'CREATE INDEX IX_MobileClients_UserId ON MobileClients (UserId)'));
PREPARE stmt FROM @ddl; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- IX_PCUpdates_Version on PCUpdates(Version)
SET @ddl := (SELECT IF((SELECT COUNT(*) FROM INFORMATION_SCHEMA.STATISTICS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'PCUpdates' AND INDEX_NAME = 'IX_PCUpdates_Version') > 0, 'SELECT 1', 'CREATE INDEX IX_PCUpdates_Version ON PCUpdates (Version)'));
PREPARE stmt FROM @ddl; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- IX_ClientUpdateRequests_DeviceId on ClientUpdateRequests(DeviceId)
SET @ddl := (SELECT IF((SELECT COUNT(*) FROM INFORMATION_SCHEMA.STATISTICS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'ClientUpdateRequests' AND INDEX_NAME = 'IX_ClientUpdateRequests_DeviceId') > 0, 'SELECT 1', 'CREATE INDEX IX_ClientUpdateRequests_DeviceId ON ClientUpdateRequests (DeviceId)'));
PREPARE stmt FROM @ddl; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- IX_ClientUpdateRequests_Status on ClientUpdateRequests(Status)
SET @ddl := (SELECT IF((SELECT COUNT(*) FROM INFORMATION_SCHEMA.STATISTICS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'ClientUpdateRequests' AND INDEX_NAME = 'IX_ClientUpdateRequests_Status') > 0, 'SELECT 1', 'CREATE INDEX IX_ClientUpdateRequests_Status ON ClientUpdateRequests (Status)'));
PREPARE stmt FROM @ddl; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- IX_SaleReceipts_SaleId on SaleReceipts(SaleId)
SET @ddl := (SELECT IF((SELECT COUNT(*) FROM INFORMATION_SCHEMA.STATISTICS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'SaleReceipts' AND INDEX_NAME = 'IX_SaleReceipts_SaleId') > 0, 'SELECT 1', 'CREATE INDEX IX_SaleReceipts_SaleId ON SaleReceipts (SaleId)'));
PREPARE stmt FROM @ddl; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- IX_SaleReceipts_CustomerId on SaleReceipts(CustomerId)
SET @ddl := (SELECT IF((SELECT COUNT(*) FROM INFORMATION_SCHEMA.STATISTICS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'SaleReceipts' AND INDEX_NAME = 'IX_SaleReceipts_CustomerId') > 0, 'SELECT 1', 'CREATE INDEX IX_SaleReceipts_CustomerId ON SaleReceipts (CustomerId)'));
PREPARE stmt FROM @ddl; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- IX_SaleReceipts_ReceiptDate on SaleReceipts(ReceiptDate)
SET @ddl := (SELECT IF((SELECT COUNT(*) FROM INFORMATION_SCHEMA.STATISTICS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'SaleReceipts' AND INDEX_NAME = 'IX_SaleReceipts_ReceiptDate') > 0, 'SELECT 1', 'CREATE INDEX IX_SaleReceipts_ReceiptDate ON SaleReceipts (ReceiptDate)'));
PREPARE stmt FROM @ddl; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- IX_PurchaseReceipts_PurchaseId on PurchaseReceipts(PurchaseId)
SET @ddl := (SELECT IF((SELECT COUNT(*) FROM INFORMATION_SCHEMA.STATISTICS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'PurchaseReceipts' AND INDEX_NAME = 'IX_PurchaseReceipts_PurchaseId') > 0, 'SELECT 1', 'CREATE INDEX IX_PurchaseReceipts_PurchaseId ON PurchaseReceipts (PurchaseId)'));
PREPARE stmt FROM @ddl; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- IX_PurchaseReceipts_SupplierId on PurchaseReceipts(SupplierId)
SET @ddl := (SELECT IF((SELECT COUNT(*) FROM INFORMATION_SCHEMA.STATISTICS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'PurchaseReceipts' AND INDEX_NAME = 'IX_PurchaseReceipts_SupplierId') > 0, 'SELECT 1', 'CREATE INDEX IX_PurchaseReceipts_SupplierId ON PurchaseReceipts (SupplierId)'));
PREPARE stmt FROM @ddl; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- IX_PurchaseReceipts_ReceiptDate on PurchaseReceipts(ReceiptDate)
SET @ddl := (SELECT IF((SELECT COUNT(*) FROM INFORMATION_SCHEMA.STATISTICS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'PurchaseReceipts' AND INDEX_NAME = 'IX_PurchaseReceipts_ReceiptDate') > 0, 'SELECT 1', 'CREATE INDEX IX_PurchaseReceipts_ReceiptDate ON PurchaseReceipts (ReceiptDate)'));
PREPARE stmt FROM @ddl; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- IX_PaymentVouchers_PartyType_PartyId on PaymentVouchers(PartyType, PartyId)
SET @ddl := (SELECT IF((SELECT COUNT(*) FROM INFORMATION_SCHEMA.STATISTICS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'PaymentVouchers' AND INDEX_NAME = 'IX_PaymentVouchers_PartyType_PartyId') > 0, 'SELECT 1', 'CREATE INDEX IX_PaymentVouchers_PartyType_PartyId ON PaymentVouchers (PartyType, PartyId)'));
PREPARE stmt FROM @ddl; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- IX_PaymentVouchers_VoucherDate on PaymentVouchers(VoucherDate)
SET @ddl := (SELECT IF((SELECT COUNT(*) FROM INFORMATION_SCHEMA.STATISTICS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'PaymentVouchers' AND INDEX_NAME = 'IX_PaymentVouchers_VoucherDate') > 0, 'SELECT 1', 'CREATE INDEX IX_PaymentVouchers_VoucherDate ON PaymentVouchers (VoucherDate)'));
PREPARE stmt FROM @ddl; EXECUTE stmt; DEALLOCATE PREPARE stmt;


