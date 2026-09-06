-- Migration 002: Seed Default Data
-- Applied by: DatabaseInitializer / MigrationRunner
-- Description: Inserts default roles, admin user, menu permissions, and report menus

-- Seed Roles
INSERT IGNORE INTO Roles (Code, Name, Description, IsActive, IsDeleted, CreatedAt, UpdatedAt) VALUES
('Admin', 'Administrator', 'Full system access', 1, 0, UTC_TIMESTAMP(), UTC_TIMESTAMP()),
('Manager', 'Manager', 'Manage sales, inventory, and reports', 1, 0, UTC_TIMESTAMP(), UTC_TIMESTAMP()),
('Cashier', 'Cashier', 'Process sales and handle customers', 1, 0, UTC_TIMESTAMP(), UTC_TIMESTAMP()),
('Warehouse', 'Warehouse Staff', 'Manage stock movements and transfers', 1, 0, UTC_TIMESTAMP(), UTC_TIMESTAMP());

-- Seed Default Admin User (password: Admin@123)
-- BCrypt hash with work factor 12
INSERT IGNORE INTO Users (Username, PasswordHash, Email, FullName, IsActive, IsDeleted, CreatedAt, UpdatedAt) VALUES
('admin', '$2a$12$H2IyHt.B8odnd3/3AZW54u3XYGjV/JDCRlKq.iP2HPg1UsDm9w.xe', 'admin@mmnextpos.local', 'System Administrator', 1, 0, UTC_TIMESTAMP(), UTC_TIMESTAMP());

-- Seed UserRole mapping (admin -> Admin role)
INSERT IGNORE INTO UserRoles (UserId, RoleId, CreatedAt, IsDeleted)
SELECT u.Id, r.Id, UTC_TIMESTAMP(), 0
FROM Users u
CROSS JOIN Roles r
WHERE u.Username = 'admin' AND r.Code = 'Admin';

-- Seed MenuRoles for Admin role (full access)
INSERT IGNORE INTO MenuRoles (RoleId, MenuCode, CanView, CanCreate, CanEdit, CanDelete, CanExport, CreatedAt, IsDeleted)
SELECT r.Id, m.MenuCode, 1, 1, 1, 1, 1, UTC_TIMESTAMP(), 0
FROM Roles r
CROSS JOIN (
    SELECT 'SALES' AS MenuCode UNION ALL
    SELECT 'PRODUCT' UNION ALL
    SELECT 'INVENTORY' UNION ALL
    SELECT 'CUSTOMER' UNION ALL
    SELECT 'REPORT' UNION ALL
    SELECT 'PURCHASE' UNION ALL
    SELECT 'OUTSTANDING' UNION ALL
    SELECT 'EXPENSE' UNION ALL
    SELECT 'WAREHOUSE' UNION ALL
    SELECT 'STARMAN' UNION ALL
    SELECT 'SETTINGS' UNION ALL
    SELECT 'LICENSE' UNION ALL
    SELECT 'SUPERADMIN'
) m
WHERE r.Code = 'Admin';

-- Seed MenuRoles for Manager role
INSERT IGNORE INTO MenuRoles (RoleId, MenuCode, CanView, CanCreate, CanEdit, CanDelete, CanExport, CreatedAt, IsDeleted)
SELECT r.Id, m.MenuCode, 1, 1, 1, 1, 1, UTC_TIMESTAMP(), 0
FROM Roles r
CROSS JOIN (
    SELECT 'SALES' AS MenuCode UNION ALL
    SELECT 'PRODUCT' UNION ALL
    SELECT 'INVENTORY' UNION ALL
    SELECT 'CUSTOMER' UNION ALL
    SELECT 'REPORT' UNION ALL
    SELECT 'PURCHASE' UNION ALL
    SELECT 'OUTSTANDING' UNION ALL
    SELECT 'EXPENSE' UNION ALL
    SELECT 'WAREHOUSE' UNION ALL
    SELECT 'SETTINGS'
) m
WHERE r.Code = 'Manager';

-- Seed MenuRoles for Cashier role
INSERT IGNORE INTO MenuRoles (RoleId, MenuCode, CanView, CanCreate, CanEdit, CanDelete, CanExport, CreatedAt, IsDeleted)
SELECT r.Id, m.MenuCode, 1, 1, 1, 0, 0, UTC_TIMESTAMP(), 0
FROM Roles r
CROSS JOIN (
    SELECT 'SALES' AS MenuCode UNION ALL
    SELECT 'PRODUCT' UNION ALL
    SELECT 'CUSTOMER'
) m
WHERE r.Code = 'Cashier';

-- Seed MenuRoles for Warehouse role
INSERT IGNORE INTO MenuRoles (RoleId, MenuCode, CanView, CanCreate, CanEdit, CanDelete, CanExport, CreatedAt, IsDeleted)
SELECT r.Id, m.MenuCode, 1, 1, 1, 0, 0, UTC_TIMESTAMP(), 0
FROM Roles r
CROSS JOIN (
    SELECT 'INVENTORY' AS MenuCode UNION ALL
    SELECT 'WAREHOUSE' UNION ALL
    SELECT 'PRODUCT'
) m
WHERE r.Code = 'Warehouse';

-- Seed ReportMenus (reports)
INSERT IGNORE INTO ReportMenus (Code, Name, ParentCode, FormName, AssemblyName, IconName, DisplayOrder, IsVisible, IsReport, ReportFileName, Description, CreatedAt, UpdatedAt) VALUES
-- Sales Reports
('SALE_RECEIPT', 'Sale Receipt', 'SALES', '', '', 'receipt', 10, 1, 1, '', 'Thermal receipt for sales (3-inch)', UTC_TIMESTAMP(), UTC_TIMESTAMP()),
('SALE_INVOICE', 'Sale Invoice', 'SALES', '', '', 'invoice', 20, 1, 1, '', 'A4 tax invoice for sales', UTC_TIMESTAMP(), UTC_TIMESTAMP()),
('SALE_HISTORY', 'Sale History', 'SALES', '', '', 'history', 30, 1, 1, '', 'Sales transaction history with filters', UTC_TIMESTAMP(), UTC_TIMESTAMP()),
-- Purchase Reports
('PURCHASE_INVOICE', 'Purchase Invoice', 'PURCHASE', '', '', 'invoice', 10, 1, 1, '', 'A4 purchase order/invoice', UTC_TIMESTAMP(), UTC_TIMESTAMP()),
-- Inventory Reports
('STOCK_LIST', 'Stock List', 'INVENTORY', '', '', 'list', 10, 1, 1, '', 'Product stock list with values', UTC_TIMESTAMP(), UTC_TIMESTAMP()),
('STOCK_MOVEMENT', 'Stock Movement Journal', 'INVENTORY', '', '', 'journal', 20, 1, 1, '', 'Stock movement journal (in/out/transfer)', UTC_TIMESTAMP(), UTC_TIMESTAMP()),
('BARCODE_LABELS', 'Barcode Labels', 'INVENTORY', '', '', 'barcode', 30, 1, 1, '', 'Product barcode labels (Avery 5160)', UTC_TIMESTAMP(), UTC_TIMESTAMP()),
-- Financial Reports
('PROFIT_LOSS', 'Profit & Loss Statement', 'REPORT', '', '', 'chart', 10, 1, 1, '', 'Profit & Loss with margins', UTC_TIMESTAMP(), UTC_TIMESTAMP()),
('CASH_FLOW', 'Cash Flow Statement', 'REPORT', '', '', 'chart', 20, 1, 1, '', 'Cash flow statement', UTC_TIMESTAMP(), UTC_TIMESTAMP()),
('OUTSTANDING', 'Outstanding Balances', 'REPORT', '', '', 'balance', 30, 1, 1, '', 'Customer & supplier outstanding balances', UTC_TIMESTAMP(), UTC_TIMESTAMP());

-- Seed MenuRoles for ReportMenus (Admin - full access)
INSERT IGNORE INTO MenuRoles (RoleId, MenuCode, CanView, CanCreate, CanEdit, CanDelete, CanExport, CreatedAt, IsDeleted)
SELECT r.Id, m.MenuCode, 1, 1, 1, 1, 1, UTC_TIMESTAMP(), 0
FROM Roles r
CROSS JOIN (
    SELECT 'SALE_RECEIPT' AS MenuCode UNION ALL
    SELECT 'SALE_INVOICE' UNION ALL
    SELECT 'SALE_HISTORY' UNION ALL
    SELECT 'PURCHASE_INVOICE' UNION ALL
    SELECT 'STOCK_LIST' UNION ALL
    SELECT 'STOCK_MOVEMENT' UNION ALL
    SELECT 'BARCODE_LABELS' UNION ALL
    SELECT 'PROFIT_LOSS' UNION ALL
    SELECT 'CASH_FLOW' UNION ALL
    SELECT 'OUTSTANDING'
) m
WHERE r.Code = 'Admin';

-- Seed MenuRoles for ReportMenus (Manager - view + export)
INSERT IGNORE INTO MenuRoles (RoleId, MenuCode, CanView, CanCreate, CanEdit, CanDelete, CanExport, CreatedAt, IsDeleted)
SELECT r.Id, m.MenuCode, 1, 0, 0, 0, 1, UTC_TIMESTAMP(), 0
FROM Roles r
CROSS JOIN (
    SELECT 'SALE_RECEIPT' AS MenuCode UNION ALL
    SELECT 'SALE_INVOICE' UNION ALL
    SELECT 'SALE_HISTORY' UNION ALL
    SELECT 'PURCHASE_INVOICE' UNION ALL
    SELECT 'STOCK_LIST' UNION ALL
    SELECT 'STOCK_MOVEMENT' UNION ALL
    SELECT 'BARCODE_LABELS' UNION ALL
    SELECT 'PROFIT_LOSS' UNION ALL
    SELECT 'CASH_FLOW' UNION ALL
    SELECT 'OUTSTANDING'
) m
WHERE r.Code = 'Manager';

-- Seed MenuRoles for ReportMenus (Cashier - view sales reports)
INSERT IGNORE INTO MenuRoles (RoleId, MenuCode, CanView, CanCreate, CanEdit, CanDelete, CanExport, CreatedAt, IsDeleted)
SELECT r.Id, m.MenuCode, 1, 0, 0, 0, 0, UTC_TIMESTAMP(), 0
FROM Roles r
CROSS JOIN (
    SELECT 'SALE_RECEIPT' AS MenuCode UNION ALL
    SELECT 'SALE_INVOICE' UNION ALL
    SELECT 'SALE_HISTORY'
) m
WHERE r.Code = 'Cashier';

-- Seed MenuRoles for ReportMenus (Warehouse - view inventory reports)
INSERT IGNORE INTO MenuRoles (RoleId, MenuCode, CanView, CanCreate, CanEdit, CanDelete, CanExport, CreatedAt, IsDeleted)
SELECT r.Id, m.MenuCode, 1, 0, 0, 0, 1, UTC_TIMESTAMP(), 0
FROM Roles r
CROSS JOIN (
    SELECT 'STOCK_LIST' AS MenuCode UNION ALL
    SELECT 'STOCK_MOVEMENT' UNION ALL
    SELECT 'BARCODE_LABELS'
) m
WHERE r.Code = 'Warehouse';