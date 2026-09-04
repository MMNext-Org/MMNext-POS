# Legacy Module Inventory Report - MMNext POS vs FusionPOS

**Generated**: 2026-09-04  
**Source**: Plan.md (Full Module Inventory) + Current Codebase Analysis

---

## Executive Summary

| Metric | Status |
|--------|--------|
| **Domain POCOs** | 63/150 (~42%) |
| **Repositories** | 52/150 (~35%) |
| **Application Services** | 45/150 (~30%) |
| **WinForms UI ListPages** | 19/150 (~13%) |
| **Reports/Printing** | 0/196 (0%) |

---

## Module-by-Module Analysis

### 1. SALES MODULE (`Sales/`)

| Legacy Component | Status | Implementation Notes |
|-----------------|--------|---------------------|
| **ucLiveSaleMain** | ❌ | Live sale dashboard |
| **ucLiveSales** | ❌ | Live sales grid |
| **ucSales** | ✅ **Partial** | `SalesListPage.cs` - basic list with CRUD |
| **ucSalesHistory** | ❌ | Historical sales view |
| **ucSalesHold** | ❌ | Hold/retrieve sales |
| **ucSalesInvoice** | ❌ | Invoice view/print |
| **ucLiveSaleHistory** | ❌ | |
| **ucSalesReturn** | ❌ | Return entry |
| **ucSalesReturnMain** | ❌ | Return dashboard |
| **ucSalesReturnInvoice** | ❌ | Return invoice |
| **frmBankPayment** | ❌ | Payment processing |
| **frmDelivery** | ❌ | Delivery management |
| **frmLiveSalesEdit** | ❌ | Live edit form |
| **frmSalesEdit** | ✅ **Partial** | `NewSaleForm.cs` - create new sale |
| **frmSalesHold** | ❌ | |
| **frmSalesReturnBySalesInvoice** | ❌ | |

**Entities**: `Sale`, `SaleDetail`, `SaleTemp`, `SaleTempDetail`, `SalePriceHistory`, `SalesReturn`, `SalesReturnDetail`, `Invoice`, `Payment`

---

### 2. CONTACTS MODULE (`Contacts/`)

| Legacy Component | Status | Implementation Notes |
|-----------------|--------|---------------------|
| **ucCustomer** | ✅ **Partial** | `CustomersListPage.cs` - list with CRUD |
| **ucCustomerAdvanced(+Tab/History)** | ❌ | Advanced tabs, history |
| **ucSupplier** | ✅ **Partial** | `Supplier` entity + repo + service, no ListPage yet |
| **frmCustomerImport** | ❌ | Import dialog |
| **frmSupplierImport** | ❌ | Import dialog |

**Entities**: `Customer`, `Supplier`, `CustomerOutstanding`, `SupplierOutstanding`

---

### 3. INVENTORY MODULE (`Inventory/`)

| Legacy Component | Status | Implementation Notes |
|-----------------|--------|---------------------|
| **Stock Entry** | ✅ **Partial** | `Product` + `ProductRepository` + `ProductService` + `ProductsListPage` |
| **Issue/Receive/Damaged/Lost/Adjust** | ✅ **Partial** | `StockMovement`, `StockMovementDetail` entities + repos |
| **Assembly/Deassembly** | ✅ **Partial** | `Assembly`, `AssemblyDetail` entities + repos |
| **Expired** | ❌ | |
| **Linked Stock** | ✅ **Partial** | `LinkedStock` entity + repo |
| **Barcode** | ❌ | Barcode scanning/generation |
| **Sale-Price History** | ✅ **Partial** | `SalePriceHistory` entity + repo |
| **Sale-Price Invoice** | ❌ | |

**Entities**: `Product`, `StockMovement`, `StockMovementDetail`, `Assembly`, `AssemblyDetail`, `LinkedStock`, `SerialNumber`, `SerialBatch`, `SerialTracking`, `SalePriceHistory`, `StockTransfer`, `StockTransferDetail`

---

### 4. PURCHASES MODULE (`Purchases/`)

| Legacy Component | Status | Implementation Notes |
|-----------------|--------|---------------------|
| **ucPurchase** | ✅ **Partial** | `Purchase`, `PurchaseDetail` entities + repos + service |
| **PurchaseHistory** | ❌ | History list |
| **PurchaseHold** | ❌ | Hold/retrieve |
| **PurchaseInvoice** | ❌ | Invoice view |
| **PurchaseReturn(+Invoice/Main)** | ✅ **Partial** | `PurchaseReturn`, `PurchaseReturnDetail` entities + repos + service |
| **frm*** | ❌ | Various forms |

**Entities**: `Purchase`, `PurchaseDetail`, `PurchaseReturn`, `PurchaseReturnDetail`

---

### 5. OUTSTANDING / PAYMENTS (`Outstanding/`)

| Legacy Component | Status | Implementation Notes |
|-----------------|--------|---------------------|
| **ucCustomerOutstand** | ✅ **Partial** | `CustomerOutstanding` entity + repo + service + `OutstandingListPage` |
| **ucSupplierOutstand** | ✅ **Partial** | `SupplierOutstanding` entity + repo + service + `OutstandingListPage` |
| **Payments & History** | ❌ | Payment processing, history view |

**Entities**: `CustomerOutstanding`, `SupplierOutstanding`, `Payment`

---

### 6. EXPENSES (`Expenses/`)

| Legacy Component | Status | Implementation Notes |
|-----------------|--------|---------------------|
| **ucExpense** | ✅ **Partial** | `Expense`, `ExpenseType` entities + repos + service + `ExpenseSummaryForm` |

**Entities**: `Expense`, `ExpenseType`

---

### 7. WAREHOUSE / STOCK TRANSFER (`Warehouse/`)

| Legacy Component | Status | Implementation Notes |
|-----------------|--------|---------------------|
| **Transfer** | ✅ **Partial** | `StockTransfer`, `StockTransferDetail` entities + repos |
| **Adjust** | ✅ **Partial** | `StockMovement` covers adjustments |
| **Damaged** | ❌ | |
| **Stock List** | ✅ **Partial** | `ProductsListPage` covers stock listing |
| **Lookups** | ❌ | |

**Entities**: `StockTransfer`, `StockTransferDetail`, `StockMovement`, `Location`

---

### 8. STARMAN - Multi-Site (`Starman/`)

| Legacy Component | Status | Implementation Notes |
|-----------------|--------|---------------------|
| **Stock Transfer Received/Accept** | ✅ **Partial** | `StarStockTransferReceived` entity + repo |
| **Sale-Price Transfer/Accept** | ✅ **Partial** | `StarSalePriceTransfer` entity + repo |
| **Star Reports** | ✅ **Partial** | `StarCashFlowReport`, `StarProfitLossReport`, `StarStockBalanceReport`, `StarReorderReport`, `StarOutstandingReport` entities + repos |

**Entities**: `StarCashFlowReport`, `StarProfitLossReport`, `StarStockBalanceReport`, `StarReorderReport`, `StarOutstandingReport`, `StarSalePriceTransfer`, `StarStockTransferReceived`, `RemoteWarehouse`, `IssueHeader`

---

### 9. DASHBOARD (`Dashboard/`)

| Legacy Component | Status | Implementation Notes |
|-----------------|--------|---------------------|
| **ucQuickSummaryMain** | ❌ | Quick summary dashboard |
| **ucDataView** | ❌ | Data visualization |
| **ucFinancialView** | ❌ | Financial dashboard |

**Entities**: `DashboardWidget`

---

### 10. REPORTS (`Reports/`) - ~96 `rpt*.cs`

| Report Category | Status |
|----------------|--------|
| Sale Reports | ❌ |
| Purchase Reports | ❌ |
| Inventory Reports | ❌ |
| Financial Reports | ❌ |
| Outstanding Reports | ❌ |

**No XtraReports implemented yet** - need `IReportService` + DevExpress XtraReport definitions

---

### 11. PRINT VOUCHERS (`PrintVoucher/`) - ~100 layouts

| Voucher Type | Status |
|-------------|--------|
| A4 Receipts | ❌ |
| A5 Receipts | ❌ |
| Slips | ❌ |
| Barcodes | ❌ |

**No printing/voucher infrastructure yet**

---

### 12. SETTINGS (`Settings/`)

| Setting | Status | Implementation Notes |
|---------|--------|---------------------|
| **Company** | ✅ **Full** | Entity + Repo + Service + ListPage |
| **Unit** | ✅ **Full** | Entity + Repo + Service + ListPage |
| **Category** | ✅ **Full** | Entity + Repo + Service + ListPage |
| **Group** | ✅ **Full** | Entity + Repo + Service + ListPage |
| **Currency** | ✅ **Full** | Entity + Repo + Service + ListPage |
| **Tax** | ✅ **Full** | Entity + Repo + Service + ListPage |
| **Discount** | ✅ **Full** | Entity + Repo + Service + ListPage |
| **Printer** | ❌ | |
| **Theme** | ✅ **Partial** | `Theme` entity + repo |
| **Language** | ✅ **Partial** | `Language` entity + repo |
| **Fonts (Myanmar)** | ❌ | |
| **Backup** | ❌ | |
| **Data Migration** | ❌ | |

---

### 13. SUPERADMIN (`SuperAdmin/`)

| Component | Status |
|----------|--------|
| Deleted/All Invoice Views | ❌ |
| Change-Date Log | ✅ **Partial** | `ChangeDateLog` entity + repo + audit service |
| JSON Log History | ❌ |
| Script Executor | ❌ |

---

### 14. LICENSE (`License/`, `Program.cs`)

| Component | Status |
|----------|--------|
| Registration | ❌ |
| Device Binding | ✅ **Partial** | `PcClient`, `MobileClient`, `DeviceRequest`, `PCUpdate`, `ClientUpdateRequest`, `AppInfo`, `LicenseInfo`, `Subscription`, `Registration` entities + repos |
| Expiry Logic | ❌ |

---

### 15. MENUS (`Menus/`)

| Component | Status |
|----------|--------|
| Role-Based Main Menu | ✅ **Partial** | `MenuRole` entity + repo + service + `IMainNavigationService` |
| Sub Menu Definitions | ✅ **Partial** | `ReportMenus` entity + repo + service + ListPage |

---

### 16. API CONTROLLER (`Controller/`)

| Component | Status |
|----------|--------|
| SaleController | ❌ | Self-hosted HTTP API |

---

### 17. DB UTILITIES (`Utilities/`, `DBConfig/`)

| Component | Status |
|----------|--------|
| Connection Tester | ❌ |
| Restore Helpers | ❌ |

---

## Domain Entities Inventory

### Implemented (63 entities)

**Core Masters (14)**: Category, Unit, Group, Currency, Tax, Discount, Location, Company, User, Role, UserRole, MenuRole, ReportMenus, EmailSetting

**Sales (8)**: Sale, SaleDetail, SaleTemp, SaleTempDetail, SalePriceHistory, SalesReturn, SalesReturnDetail, Invoice

**Purchases (4)**: Purchase, PurchaseDetail, PurchaseReturn, PurchaseReturnDetail

**Inventory (10)**: Product, StockMovement, StockMovementDetail, Assembly, AssemblyDetail, LinkedStock, SerialNumber, SerialBatch, SerialTracking, StockTransfer

**Outstanding/Payments (3)**: CustomerOutstanding, SupplierOutstanding, Payment

**Expenses (2)**: Expense, ExpenseType

**Warehouse (2)**: StockTransfer, StockTransferDetail

**Starman (8)**: StarCashFlowReport, StarProfitLossReport, StarStockBalanceReport, StarReorderReport, StarOutstandingReport, StarSalePriceTransfer, StarStockTransferReceived, RemoteWarehouse

**License/Device (9)**: LicenseInfo, Registration, Subscription, DeviceInfo, PcClient, MobileClient, DeviceRequest, PCUpdate, ClientUpdateRequest

**System (5)**: AppInfo, IssueHeader, Payment, Theme, Language

**Audit/Config (5)**: ChangeDateLog, DashboardWidget, ValueObject, EntityBase, Exceptions

---

## Repository Coverage

| Layer | Count | Pattern |
|-------|-------|---------|
| Generic Base | 1 | `GenericRepository<T>` |
| Specific Repos | 52 | Interface + Implementation |
| Report Repos | 5 | `IStar*ReportRepository` |

---

## Service Coverage

| Category | Services | Status |
|----------|----------|--------|
| Core Masters | 14 | ✅ Full CRUD |
| Sales | 3 | Sales, SaleTemp, Invoice |
| Purchases | 2 | Purchase, PurchaseDetail |
| Inventory | 1 | Inventory (partial) |
| Outstanding | 1 | Outstanding (Customer+Supplier) |
| Expenses | 3 | Expense, ExpenseType, ExpenseSummary |
| Settings | 1 | Setting (partial) |
| License/Device | 6 | Partial |
| Audit/Navigation | 3 | Audit, MainNavigation, MenuRole |
| Starman | 2 | StarSalePriceTransfer, StarStockTransferReceived |

---

## WinForms UI Coverage

| ListPage | Entity | Status |
|----------|--------|--------|
| ProductsListPage | Product | ✅ |
| CustomersListPage | Customer | ✅ |
| SalesListPage | Sale | ✅ |
| OutstandingListPage | Customer/Supplier Outstanding | ✅ |
| CategoriesListPage | Category | ✅ |
| UnitsListPage | Unit | ✅ |
| GroupsListPage | Group | ✅ |
| CurrenciesListPage | Currency | ✅ |
| TaxesListPage | Tax | ✅ |
| DiscountsListPage | Discount | ✅ |
| LocationsListPage | Location | ✅ |
| CompaniesListPage | Company | ✅ |
| UsersListPage | User | ✅ |
| RolesListPage | Role | ✅ |
| ReportMenusListPage | ReportMenus | ✅ |
| EmailSettingsListPage | EmailSetting | ✅ |
| **Missing ListPages** | | |
| SupplierListPage | Supplier | ❌ |
| SaleTempListPage | SaleTemp | ❌ |
| SalesReturnListPage | SalesReturn | ❌ |
| PurchaseListPage | Purchase | ❌ |
| PurchaseReturnListPage | PurchaseReturn | ❌ |
| StockMovementListPage | StockMovement | ❌ |
| AssemblyListPage | Assembly | ❌ |
| StockTransferListPage | StockTransfer | ❌ |
| ExpenseListPage | Expense | ❌ |
| ExpenseTypeListPage | ExpenseType | ❌ |
| PaymentListPage | Payment | ❌ |
| Star* ListPages | Star Reports | ❌ |
| License ListPages | License entities | ❌ |

---

## Next Implementation Priority (per plan.md Phase C-D)

### Phase C: Generic Repository Layer (Week 2)
1. Migrate all specific repos to inherit `GenericRepository<T>`
2. Standardize paging: `PageSize` + `PageIndex`
3. Add soft-delete support to all repos

### Phase D: Application Layer Expansion (Weeks 3-4)
1. `IInventoryService` - stock movements, serials, assemblies
2. `IPurchaseService` - full purchase cycle
3. `IOutstandingService` - complete (✅)
4. `IExpenseService` - complete (✅)
5. `ISettingService` - all master data
6. `IUserService`, `IRoleService`, `ILicenseService`, `IBackupService`
7. `IDocumentService` - invoice numbering
8. `IReportService` - parameterized reporting

### Phase E: Presentation Foundation (Week 5) - **COMPLETED**
- AsyncFormBase extensions ✅
- ListPage base class ✅
- IMainNavigationService ✅
- MainForm on ListPage ✅

### Phase F: Sales Module (Week 6)
- Sales History, Returns, Hold, Live Sale edit
- NewSaleForm hardening (barcode, currency, discount, tax)

---

## Coverage Progress

```
Phase A (Foundation)     ████████████ 100%  ✅ Complete
Phase B (Data Model)     ████████████ 100%  ✅ Complete  
Phase C (Repo Layer)     ████████     80%   In Progress
Phase D (App Layer)      ████         40%   In Progress
Phase E (Presentation)   ████████████ 100%  ✅ Complete
Phase F (Sales)          ██           15%   Not Started
Phase G (Contacts/Purch) ██           10%   Not Started
Phase H (Inventory/Wh)   ██           10%   Not Started
Phase I (Reports/Print)  ██           0%    Not Started
Phase J (Admin)          ██           5%    Not Started
Phase K (QA)             ██           0%    Not Started
Phase L (Release)        ██           0%    Not Started

Overall: ~35% to full FusionPOS parity
```

---

## Action Items

1. **Immediate**: Add missing ListPages for Supplier, SaleTemp, SalesReturn, Purchase, PurchaseReturn
2. **Phase C**: Migrate all repos to `GenericRepository<T>` base
3. **Phase D**: Implement `IInventoryService`, `IPurchaseService`, `IReportService`
4. **Phase F**: Complete Sales module (History, Returns, Hold, Live Edit)
5. **Phase I**: Implement XtraReport definitions for ~96 reports
6. **Phase L**: Create WiX MSI installer