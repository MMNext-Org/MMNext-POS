-- Migration 004: Add Performance Indexes
-- Applied by: DatabaseInitializer / MigrationRunner
-- Description: Adds missing indexes for query performance on frequently filtered/joined columns

-- SaleTemps indexes
CREATE INDEX IF NOT EXISTS IX_SaleTemps_Status ON SaleTemps (Status);
CREATE INDEX IF NOT EXISTS IX_SaleTemps_LocationId ON SaleTemps (LocationId);
CREATE INDEX IF NOT EXISTS IX_SaleTemps_SaleDate ON SaleTemps (SaleDate);
CREATE INDEX IF NOT EXISTS IX_SaleTemps_CreatedByUserId ON SaleTemps (CreatedByUserId);
CREATE INDEX IF NOT EXISTS IX_SaleTemps_CustomerId ON SaleTemps (CustomerId);

-- Sales indexes
CREATE INDEX IF NOT EXISTS IX_Sales_LocationId ON Sales (LocationId);
CREATE INDEX IF NOT EXISTS IX_Sales_SaleDate ON Sales (SaleDate);
CREATE INDEX IF NOT EXISTS IX_Sales_Status ON Sales (Status);
CREATE INDEX IF NOT EXISTS IX_Sales_CustomerId_SaleDate ON Sales (CustomerId, SaleDate);

-- SaleDetails indexes
CREATE INDEX IF NOT EXISTS IX_SaleDetails_ProductId ON SaleDetails (ProductId);

-- StockMovements indexes
CREATE INDEX IF NOT EXISTS IX_StockMovements_MovementDate ON StockMovements (MovementDate);
CREATE INDEX IF NOT EXISTS IX_StockMovements_MovementType ON StockMovements (MovementType);
CREATE INDEX IF NOT EXISTS IX_StockMovements_LocationId ON StockMovements (LocationId);
CREATE INDEX IF NOT EXISTS IX_StockMovements_SupplierId ON StockMovements (SupplierId);
CREATE INDEX IF NOT EXISTS IX_StockMovements_CustomerId ON StockMovements (CustomerId);
CREATE INDEX IF NOT EXISTS IX_StockMovements_Status ON StockMovements (Status);
CREATE INDEX IF NOT EXISTS IX_StockMovements_MovementNo ON StockMovements (MovementNo);

-- StockMovementDetails indexes
CREATE INDEX IF NOT EXISTS IX_StockMovementDetails_ProductId ON StockMovementDetails (ProductId);

-- Purchases indexes
CREATE INDEX IF NOT EXISTS IX_Purchases_SupplierId ON Purchases (SupplierId);
CREATE INDEX IF NOT EXISTS IX_Purchases_LocationId ON Purchases (LocationId);
CREATE INDEX IF NOT EXISTS IX_Purchases_PurchaseDate ON Purchases (PurchaseDate);
CREATE INDEX IF NOT EXISTS IX_Purchases_Status ON Purchases (Status);
CREATE INDEX IF NOT EXISTS IX_Purchases_InvoiceNo ON Purchases (InvoiceNo);

-- PurchaseDetails indexes
CREATE INDEX IF NOT EXISTS IX_PurchaseDetails_ProductId ON PurchaseDetails (ProductId);

-- Payments indexes
CREATE INDEX IF NOT EXISTS IX_Payments_PaymentDate ON Payments (PaymentDate);
CREATE INDEX IF NOT EXISTS IX_Payments_CustomerId ON Payments (CustomerId);
CREATE INDEX IF NOT EXISTS IX_Payments_SupplierId ON Payments (SupplierId);
CREATE INDEX IF NOT EXISTS IX_Payments_SaleId ON Payments (SaleId);
CREATE INDEX IF NOT EXISTS IX_Payments_PurchaseId ON Payments (PurchaseId);
CREATE INDEX IF NOT EXISTS IX_Payments_PaymentNo ON Payments (PaymentNo);
CREATE INDEX IF NOT EXISTS IX_Payments_Status ON Payments (Status);

-- CustomerOutstandings indexes
CREATE INDEX IF NOT EXISTS IX_CustomerOutstandings_CustomerId_Status ON CustomerOutstandings (CustomerId, Status);
CREATE INDEX IF NOT EXISTS IX_CustomerOutstandings_TransactionDate ON CustomerOutstandings (TransactionDate);
CREATE INDEX IF NOT EXISTS IX_CustomerOutstandings_SaleId ON CustomerOutstandings (SaleId);

-- SupplierOutstandings indexes
CREATE INDEX IF NOT EXISTS IX_SupplierOutstandings_SupplierId_Status ON SupplierOutstandings (SupplierId, Status);
CREATE INDEX IF NOT EXISTS IX_SupplierOutstandings_TransactionDate ON SupplierOutstandings (TransactionDate);
CREATE INDEX IF NOT EXISTS IX_SupplierOutstandings_PurchaseId ON SupplierOutstandings (PurchaseId);

-- Expenses indexes
CREATE INDEX IF NOT EXISTS IX_Expenses_ExpenseDate ON Expenses (ExpenseDate);
CREATE INDEX IF NOT EXISTS IX_Expenses_ExpenseTypeId ON Expenses (ExpenseTypeId);
CREATE INDEX IF NOT EXISTS IX_Expenses_LocationId ON Expenses (LocationId);
CREATE INDEX IF NOT EXISTS IX_Expenses_VendorId ON Expenses (VendorId);
CREATE INDEX IF NOT EXISTS IX_Expenses_Status ON Expenses (Status);

-- Products indexes
CREATE INDEX IF NOT EXISTS IX_Products_Sku ON Products (Sku);
CREATE INDEX IF NOT EXISTS IX_Products_IsActive_IsDeleted ON Products (IsActive, IsDeleted);
CREATE INDEX IF NOT EXISTS IX_Products_CategoryId ON Products (CategoryId);

-- Customers indexes
CREATE INDEX IF NOT EXISTS IX_Customers_Code ON Customers (Code);
CREATE INDEX IF NOT EXISTS IX_Customers_IsActive_IsDeleted ON Customers (IsActive, IsDeleted);

-- Suppliers indexes
CREATE INDEX IF NOT EXISTS IX_Suppliers_Code ON Suppliers (Code);
CREATE INDEX IF NOT EXISTS IX_Suppliers_IsActive_IsDeleted ON Suppliers (IsActive, IsDeleted);

-- Users indexes
CREATE INDEX IF NOT EXISTS IX_Users_Username ON Users (Username);
CREATE INDEX IF NOT EXISTS IX_Users_Email ON Users (Email);
CREATE INDEX IF NOT EXISTS IX_Users_IsActive_IsDeleted ON Users (IsActive, IsDeleted);
CREATE INDEX IF NOT EXISTS IX_Users_LocationId ON Users (LocationId);

-- Assemblies indexes
CREATE INDEX IF NOT EXISTS IX_Assemblies_AssemblyDate ON Assemblies (AssemblyDate);
CREATE INDEX IF NOT EXISTS IX_Assemblies_LocationId ON Assemblies (LocationId);
CREATE INDEX IF NOT EXISTS IX_Assemblies_OutputProductId ON Assemblies (OutputProductId);

-- AssemblyDetails indexes
CREATE INDEX IF NOT EXISTS IX_AssemblyDetails_ComponentProductId ON AssemblyDetails (ComponentProductId);

-- SalePriceHistories indexes
CREATE INDEX IF NOT EXISTS IX_SalePriceHistories_ProductId_EffectiveDate ON SalePriceHistories (ProductId, EffectiveDate);

-- Invoices indexes
CREATE INDEX IF NOT EXISTS IX_Invoices_CustomerId ON Invoices (CustomerId);
CREATE INDEX IF NOT EXISTS IX_Invoices_InvoiceDate ON Invoices (InvoiceDate);
CREATE INDEX IF NOT EXISTS IX_Invoices_Status ON Invoices (Status);

-- SalesReturns indexes
CREATE INDEX IF NOT EXISTS IX_SalesReturns_SaleId ON SalesReturns (SaleId);
CREATE INDEX IF NOT EXISTS IX_SalesReturns_CustomerId ON SalesReturns (CustomerId);
CREATE INDEX IF NOT EXISTS IX_SalesReturns_ReturnDate ON SalesReturns (ReturnDate);

-- SalesReturnDetails indexes
CREATE INDEX IF NOT EXISTS IX_SalesReturnDetails_ProductId ON SalesReturnDetails (ProductId);
CREATE INDEX IF NOT EXISTS IX_SalesReturnDetails_SaleDetailId ON SalesReturnDetails (SaleDetailId);

-- StockTransfers indexes
CREATE INDEX IF NOT EXISTS IX_StockTransfers_FromLocationId ON StockTransfers (FromLocationId);
CREATE INDEX IF NOT EXISTS IX_StockTransfers_ToLocationId ON StockTransfers (ToLocationId);
CREATE INDEX IF NOT EXISTS IX_StockTransfers_TransferDate ON StockTransfers (TransferDate);
CREATE INDEX IF NOT EXISTS IX_StockTransfers_Status ON StockTransfers (Status);

-- StockTransferDetails indexes
CREATE INDEX IF NOT EXISTS IX_StockTransferDetails_ProductId ON StockTransferDetails (ProductId);

-- Starman tables indexes
CREATE INDEX IF NOT EXISTS IX_StarStockTransferReceiveds_FromLocationId ON StarStockTransferReceiveds (FromLocationId);
CREATE INDEX IF NOT EXISTS IX_StarStockTransferReceiveds_ToLocationId ON StarStockTransferReceiveds (ToLocationId);
CREATE INDEX IF NOT EXISTS IX_StarSalePriceTransfers_FromLocationId ON StarSalePriceTransfers (FromLocationId);
CREATE INDEX IF NOT EXISTS IX_StarSalePriceTransfers_ToLocationId ON StarSalePriceTransfers (ToLocationId);
CREATE INDEX IF NOT EXISTS IX_StarCashFlowReports_LocationId_ReportDate ON StarCashFlowReports (LocationId, ReportDate);
CREATE INDEX IF NOT EXISTS IX_StarProfitLossReports_LocationId_FromDate_ToDate ON StarProfitLossReports (LocationId, FromDate, ToDate);
CREATE INDEX IF NOT EXISTS IX_StarStockBalanceReports_LocationId_ProductId ON StarStockBalanceReports (LocationId, ProductId);
CREATE INDEX IF NOT EXISTS IX_StarReorderReports_LocationId_ProductId ON StarReorderReports (LocationId, ProductId);
CREATE INDEX IF NOT EXISTS IX_StarOutstandingReports_LocationId_PartyType_PartyId ON StarOutstandingReports (LocationId, PartyType, PartyId);
CREATE INDEX IF NOT EXISTS IX_IssueHeaders_FromLocationId ON IssueHeaders (FromLocationId);
CREATE INDEX IF NOT EXISTS IX_IssueHeaders_ToLocationId ON IssueHeaders (ToLocationId);
CREATE INDEX IF NOT EXISTS IX_IssueHeaders_IssueDate ON IssueHeaders (IssueDate);

-- ChangeDateLogs indexes (already defined in table, but ensuring)
-- Already has: IX_ChangeDateLogs_Entity, IX_ChangeDateLogs_User, IX_ChangeDateLogs_Date

-- SuperAdminLogs indexes (already defined in table)
-- Already has: IX_SuperAdminLogs_User, IX_SuperAdminLogs_Date

-- DataMigrations indexes
CREATE INDEX IF NOT EXISTS IX_DataMigrations_Status ON DataMigrations (Status);
CREATE INDEX IF NOT EXISTS IX_DataMigrations_IsScheduled_ScheduledAt ON DataMigrations (IsScheduled, ScheduledAt);

-- DeviceInfos indexes (already has IX_DeviceInfos_Fingerprint)
CREATE INDEX IF NOT EXISTS IX_DeviceInfos_RegistrationId ON DeviceInfos (RegistrationId);

-- DeviceRequests indexes
CREATE INDEX IF NOT EXISTS IX_DeviceRequests_DeviceId ON DeviceRequests (DeviceId);
CREATE INDEX IF NOT EXISTS IX_DeviceRequests_Status ON DeviceRequests (Status);

-- PcClients indexes
CREATE INDEX IF NOT EXISTS IX_PcClients_DeviceId ON PcClients (DeviceId);
CREATE INDEX IF NOT EXISTS IX_PcClients_UserId ON PcClients (UserId);

-- MobileClients indexes
CREATE INDEX IF NOT EXISTS IX_MobileClients_DeviceId ON MobileClients (DeviceId);
CREATE INDEX IF NOT EXISTS IX_MobileClients_UserId ON MobileClients (UserId);

-- PCUpdates indexes
CREATE INDEX IF NOT EXISTS IX_PCUpdates_Version ON PCUpdates (Version);

-- ClientUpdateRequests indexes
CREATE INDEX IF NOT EXISTS IX_ClientUpdateRequests_DeviceId ON ClientUpdateRequests (DeviceId);
CREATE INDEX IF NOT EXISTS IX_ClientUpdateRequests_Status ON ClientUpdateRequests (Status);

-- SaleReceipts indexes
CREATE INDEX IF NOT EXISTS IX_SaleReceipts_SaleId ON SaleReceipts (SaleId);
CREATE INDEX IF NOT EXISTS IX_SaleReceipts_CustomerId ON SaleReceipts (CustomerId);
CREATE INDEX IF NOT EXISTS IX_SaleReceipts_ReceiptDate ON SaleReceipts (ReceiptDate);

-- PurchaseReceipts indexes
CREATE INDEX IF NOT EXISTS IX_PurchaseReceipts_PurchaseId ON PurchaseReceipts (PurchaseId);
CREATE INDEX IF NOT EXISTS IX_PurchaseReceipts_SupplierId ON PurchaseReceipts (SupplierId);
CREATE INDEX IF NOT EXISTS IX_PurchaseReceipts_ReceiptDate ON PurchaseReceipts (ReceiptDate);

-- PaymentVouchers indexes
CREATE INDEX IF NOT EXISTS IX_PaymentVouchers_PartyType_PartyId ON PaymentVouchers (PartyType, PartyId);
CREATE INDEX IF NOT EXISTS IX_PaymentVouchers_VoucherDate ON PaymentVouchers (VoucherDate);