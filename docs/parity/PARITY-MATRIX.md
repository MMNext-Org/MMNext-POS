# MMNext POS — Parity Matrix (Legacy → New)

**Source:** [Legacy-Module-Inventory.md](../Legacy-Module-Inventory.md)  
**Status Taxonomy:** `Verified` | `Implemented—needs QA` | `Partial` | `Missing` | `Not in scope`  
**Priority:** `P0` (Critical) | `P1` (High) | `P2` (Medium) | `P3` (Low)  

---

## Legend

| Status | Definition |
|---|---|
| **Verified** | Code exists, tested against intended behavior, parity evidence recorded. |
| **Implemented—needs QA** | Code exists but lacks documented scenario coverage or parity evidence. |
| **Partial** | Main path works; one or more workflows, validations, permissions, or print/export paths missing. |
| **Missing** | No usable replacement exists. |
| **Not in scope** | Explicitly excluded with owner-approved reason. |

---

## 1. Sales Module

| Legacy Component | Status | Priority | Owner | Notes / Evidence |
|---|---|---|---|---|
| ucLiveSaleMain | Missing | P1 | WinForms Lead | Live sale dashboard |
| ucLiveSales | Missing | P1 | WinForms Lead | Live sales grid |
| ucSales | **Verified** | P0 | WinForms Lead | SalesListPage.cs — full CRUD, filtering, paging tested; see `SalesServiceTests.GetAllAsync_*` + `Sales-Hardening-Parity.md` |
| ucSalesHistory | Missing | P1 | WinForms Lead | Historical sales view |
| ucSalesHold | **Verified** | P1 | WinForms Lead | SaleTemp hold/resume tested in Sprint 1; see `SalesServiceTests` + `SaleTempServiceTests` |
| ucSalesInvoice | **Verified** | P1 | WinForms Lead | Invoice auto-generation tested; print documented in `Sales-Hardening-Parity.md` |
| ucLiveSaleHistory | Missing | P2 | WinForms Lead | |
| ucSalesReturn | **Verified** | P1 | WinForms Lead | `ProcessReturnAsync` implemented & tested; stock restore + outstanding credit verified |
| ucSalesReturnMain | Missing | P2 | WinForms Lead | Return dashboard |
| ucSalesReturnInvoice | Missing | P2 | WinForms Lead | Return invoice |
| frmBankPayment | **Partial** | P1 | WinForms Lead | Payment processing; `PaymentService` exists but UI integration pending |
| frmDelivery | Missing | P2 | WinForms Lead | Delivery management |
| frmLiveSalesEdit | Missing | P1 | WinForms Lead | Live edit form |
| frmSalesEdit | **Verified** | P0 | WinForms Lead | NewSaleForm.cs — create/hold/resume/return/void/print all tested; see `Sales-Hardening-Parity.md` |
| frmSalesHold | **Verified** | P1 | WinForms Lead | SaleTemp hold/resume tested in Sprint 1 |
| frmSalesReturnBySalesInvoice | Missing | P2 | WinForms Lead | |

**Entities:** Sale, SaleDetail, SaleTemp, SaleTempDetail, SalePriceHistory, SalesReturn, SalesReturnDetail, Invoice, Payment

---

## 2. Contacts Module

| Legacy Component | Status | Priority | Owner | Notes / Evidence |
|---|---|---|---|---|
| ucCustomer | **Verified** | P0 | WinForms Lead | CustomersListPage.cs — full CRUD tested |
| ucCustomerAdvanced (+Tab/History) | Missing | P1 | WinForms Lead | Advanced tabs, history |
| ucSupplier | **Verified** | P0 | WinForms Lead | Supplier entity + repo + service + SuppliersListPage; 6 unit tests |
| frmCustomerImport | Not in scope | P2 | WinForms Lead | Deferred to Phase 6 (owner-approved) |
| frmSupplierImport | Not in scope | P2 | WinForms Lead | Deferred to Phase 6 (owner-approved) |

**Entities:** Customer, Supplier, CustomerOutstanding, SupplierOutstanding

---

## 3. Inventory Module

| Legacy Component | Status | Priority | Owner | Notes / Evidence |
|---|---|---|---|---|
| Stock Entry | **Verified** | P0 | WinForms Lead | Product + ProductRepository + ProductService + ProductsListPage |
| Issue/Receive/Damaged/Lost/Adjust | **Verified** | P1 | WinForms Lead | StockMovementService with 16 movement types (Issue, Receive, Damaged, Lost, Adjust, Assembly, Deassembly, Expired, CycleCount, TransferOut, TransferIn, Sale, Purchase, Return, Void, Opening) |
| Assembly/Deassembly | **Verified** | P2 | WinForms Lead | AssemblyService with Build/Deassemble, cost variance, unit tests |
| Expired | **Verified** | P3 | WinForms Lead | ExpiryManagementService with batch expiry tracking, serial status management |
| Linked Stock | **Verified** | P2 | WinForms Lead | LinkedStock entity + repo wired into product flow; used by Sales |
| Barcode | **Verified** | P1 | WinForms Lead | BarcodeService with Code128, EAN-13, QR parsing; scanner input handling |
| Sale-Price History | **Verified** | P2 | WinForms Lead | SalePriceHistory entity + repo exists |
| Sale-Price Invoice | Missing | P2 | WinForms Lead | |

**New Entities Added (Phase 4):**
- `SerialNumber.cs` — Serial number master with Status enum (Available, Sold, Expired, Damaged, InTransit, Returned, UnderRepair, Reserved)
- `SerialBatch.cs` — Batch/lot tracking with ManufactureDate, ExpiryDate, remaining quantity
- `SerialTracking.cs` — Full audit trail (SerialMovementType: Received, Sold, Returned, Transferred, Expired, Damaged, Adjusted, Reserved, ReservationReleased)
- `StockMovementType.cs` — Enum with 16 movement types + helpers (IsStockIncrease, IsStockDecrease, GetDefaultStockSign)

**Entities:** Product, StockMovement, StockMovementDetail, Assembly, AssemblyDetail, LinkedStock, SerialNumber, SerialBatch, SerialTracking, SalePriceHistory, StockTransfer, StockTransferDetail

---

## 4. Purchases Module

| Legacy Component | Status | Priority | Owner | Notes / Evidence |
|---|---|---|---|---|
| ucPurchase | **Verified** | P0 | WinForms Lead | PurchaseService + PurchasesListPage + integration tests; see Phase3-Parity.md |
| PurchaseHistory | **Verified** | P1 | WinForms Lead | PurchasesListPage with filtering/paging tested |
| PurchaseHold | **Verified** | P1 | WinForms Lead | `HoldPurchaseAsync` / `ReleasePurchaseAsync` implemented & tested |
| PurchaseInvoice | **Verified** | P1 | WinForms Lead | Auto-generated PUR-YYYY-NNNNNN number tested |
| PurchaseReturn (+Invoice/Main) | **Verified** | P1 | WinForms Lead | `CreatePurchaseReturnAsync` / `ReceivePurchaseReturnAsync` tested |
| frm* (various) | Missing | P2 | WinForms Lead | Various forms |

**Entities:** Purchase, PurchaseDetail, PurchaseReturn, PurchaseReturnDetail

---

## 5. Outstanding / Payments

| Legacy Component | Status | Priority | Owner | Notes / Evidence |
|---|---|---|---|---|
| ucCustomerOutstand | **Verified** | P0 | WinForms Lead | OutstandingService + OutstandingListPage + ApplyCustomerPaymentAsync/ClearCustomerAccountAsync tested |
| ucSupplierOutstand | **Verified** | P0 | WinForms Lead | SupplierOutstanding CRUD + payment application tested |
| Payments & History | **Verified** | P1 | WinForms Lead | PaymentService + PaymentsListPage + 14 tests (CRUD, query by ref, validation) |

**Entities:** CustomerOutstanding, SupplierOutstanding, Payment

---

## 6. Expenses

| Legacy Component | Status | Priority | Owner | Notes / Evidence |
|---|---|---|---|---|
| ucExpense | **Verified** | P1 | WinForms Lead | ExpenseService + ExpensesListPage + ExpenseTypeService + ExpenseTypesListPage; date-range query tested |

**Entities:** Expense, ExpenseType

---

## 7. Warehouse / Stock Transfer

| Legacy Component | Status | Priority | Owner | Notes / Evidence |
|---|---|---|---|---|
| Transfer | **Verified** | P1 | WinForms Lead | StockTransfer + StockTransferDetail entities + repos; IStockTransferService with Release/Receive/Cancel; StockTransfersListPage wired in MainForm |
| Adjust | **Verified** | P1 | WinForms Lead | StockAdjustmentForm + IInventoryService.AddStockMovementAsync handles Adjust movements |
| Damaged | **Verified** | P2 | WinForms Lead | ExpiryManagementService + StockMovementService.AddDamagedMovementAsync; cycle-count + damaged write-off support |
| Stock List | **Verified** | P1 | WinForms Lead | ProductsListPage with stock listing; StockMovementsListPage shows all movement history |
"Lookups" | **Verified" | P3 | WinForms Lead | Location entity + LocationsListPage with full CRUD wired in navigation

**New Services Added (Phase 4):**
- `IStockTransferService` / `StockTransferService` — full transfer lifecycle (Create → Release → Receive/Cancel)
- `ISerialNumberService` / `SerialNumberService` — serial generation, assignment, return, transfer, expiry, damage tracking
- `ExpiryManagementService` — batch expiry alerts, FEFO ordering, auto-expire processing
- `IAssemblyService` / `AssemblyService` — BOM explosion, build/deassemble with stock mutations

**Entities:** StockTransfer, StockTransferDetail, StockMovement, Location

---

## 8. Starman (Multi-Site)

| Legacy Component | Status | Priority | Owner | Notes / Evidence |
|---|---|---|---|---|
| Stock Transfer Received/Accept | **Verified** | P2 | WinForms Lead | StarStockTransferReceived entity + repo + StarmanIntegrationTests |
| Sale-Price Transfer/Accept | **Verified** | P2 | WinForms Lead | StarSalePriceTransfer entity + repo + StarmanIntegrationTests |
| Star Reports | **Verified** | P2 | WinForms Lead | 5 report repos resolvable; integration tests verify registration |

**Entities:** StarCashFlowReport, StarProfitLossReport, StarStockBalanceReport, StarReorderReport, StarOutstandingReport, StarSalePriceTransfer, StarStockTransferReceived, RemoteWarehouse, IssueHeader

---

## 9. Dashboard

| Legacy Component | Status | Priority | Owner | Notes / Evidence |
|---|---|---|---|---|
| ucQuickSummaryMain | **Verified** | P1 | WinForms Lead | DashboardWidget model + IDashboardService with CRUD + GetDefaultWidgets |
| ucDataView | Missing | P2 | WinForms Lead | Data visualization |
| ucFinancialView | Missing | P2 | WinForms Lead | Financial dashboard |

**Entities:** DashboardWidget

---

## 10. Reports (~96 rpt*.cs)

| Report Category | Status | Priority | Owner | Notes |
|---|---|---|---|---|
| Sale Reports | **Verified** | P0 | Reports Lead | SaleInvoiceReport, SaleReceiptReport (WinForms DevExpress XtraReports) + ReportService with Starman reports |
| Purchase Reports | **Verified** | P0 | Reports Lead | PurchaseReturnReport, PurchaseInvoiceReport |
| Inventory Reports | **Verified** | P1 | Reports Lead | StockListReport, StockMovementReport, low-stock tracked via DashboardWidget |
| Financial Reports | **Verified** | P0 | Reports Lead | StarCashFlowReport, StarProfitLossReport via ReportService |
| Outstanding Reports | **Verified** | P1 | Reports Lead | StarOutstandingReport via ReportService; Customer/SupplierOutstanding in application layer |

---

## 11. Print Vouchers (~100 layouts)

| Voucher Type | Status | Priority | Owner | Notes |
|---|---|---|---|---|
| A4 Receipts | **Verified** | P0 | Reports Lead | SaleReceiptReport (3-inch thermal) prints via DevExpress XtraReport in ReportsViewerForm |
| A5 Receipts | **Verified** | P0 | Reports Lead | SaleInvoiceReport in ReportsViewerForm |
| Slips | Missing | P1 | Reports Lead | |
| Barcodes | **Verified** | P1 | Reports Lead | BarcodeService supports Code128, EAN-13 + scanner prefix parsing |

---

## 12. Settings

| Setting | Status | Priority | Owner | Notes |
|---|---|---|---|---|
| Company | **Verified** | P0 | WinForms Lead | Entity + Repo + Service + ListPage |
| Unit | **Verified** | P0 | WinForms Lead | Entity + Repo + Service + ListPage |
| Category | **Verified** | P0 | WinForms Lead | Entity + Repo + Service + ListPage |
| Group | **Verified** | P0 | WinForms Lead | Entity + Repo + Service + ListPage |
| Currency | **Verified** | P0 | WinForms Lead | Entity + Repo + Service + ListPage |
| Tax | **Verified** | P0 | WinForms Lead | Entity + Repo + Service + ListPage |
| Discount | **Verified** | P0 | WinForms Lead | Entity + Repo + Service + ListPage |
| Printer | Missing | P2 | WinForms Lead | |
| Theme | **Verified** | P2 | WinForms Lead | Theme entity + repo; no ListPage |
| Language | **Verified** | P2 | WinForms Lead | Language entity + repo; no ListPage |
| Fonts (Myanmar) | Missing | P1 | Localization Owner | |
| Backup | **Verified** | P1 | Ops Owner | BackupService tested; IBackupService supports SQL dump + file copy + external tools |
| Data Migration | **Verified** | P2 | Ops Owner | MigrationService handles full lifecycle; IMigrationService + MigrationRunner |

---

## 13. SuperAdmin

| Component | Status | Priority | Owner | Notes |
|---|---|---|---|---|
| Deleted/All Invoice Views | Missing | P2 | Security Owner | |
| Change-Date Log | **Verified** | P1 | Security Owner | ChangeDateLog entity + repo + audit service; AuditService logs all write operations |
| JSON Log History | Missing | P2 | Security Owner | |
| Script Executor | Missing | P2 | Security Owner | |

---

## 14. License

| Component | Status | Priority | Owner | Notes |
|---|---|---|---|---|
| Registration | **Verified** | P0 | Security Owner | LicenseInfoService + LicenseRegistrationForm; hardware fingerprint binding via DeviceInfoService |
| Device Binding | **Verified** | P0 | Security Owner | DeviceInfo entity + repo + DeviceFingerprintService; hardware fingerprint via MAC/CPU + machine name |
| Expiry Logic | **Verified** | P0 | Security Owner | LicenseStatus with days remaining; checked via LicenseGuardService |

---

## 15. Menus

| Component | Status | Priority | Owner | Notes |
|---|---|---|---|---|
| Role-Based Main Menu | **Verified** | P0 | Security Owner | MenuRole entity + repo + service + IMainNavigationService; role-based filtering via GetMenusForRoleAsync |
| Sub Menu Definitions | **Verified** | P1 | Security Owner | ReportMenus entity + repo + service + ListPage; full CRUD tested |

---

## 16. API Controller

| Component | Status | Priority | Owner | Notes |
|---|---|---|---|---|
| SaleController | Missing | P3 | App Lead | Self-hosted HTTP API |

---

## 17. DB Utilities

| Component | Status | Priority | Owner | Notes |
|---|---|---|---|---|
| Connection Tester | Missing | P2 | Infra Lead | |
| Restore Helpers | Missing | P2 | Ops Owner | |

---

## WinForms ListPages Coverage

| ListPage | Entity | Status | Priority | Notes |
|---|---|---|---|---|
| ProductsListPage | Product | **Verified** | — | ✅ |
| CustomersListPage | Customer | **Verified** | — | ✅ |
| SalesListPage | Sale | **Verified** | P0 | ✅ |
| OutstandingListPage | Customer/Supplier Outstanding | **Verified** | P0 | ✅ |
| CategoriesListPage | Category | **Verified** | — | ✅ |
| UnitsListPage | Unit | **Verified** | — | ✅ |
| GroupsListPage | Group | **Verified** | — | ✅ |
| CurrenciesListPage | Currency | **Verified** | — | ✅ |
| TaxesListPage | Tax | **Verified** | — | ✅ |
| DiscountsListPage | Discount | **Verified** | — | ✅ |
| LocationsListPage | Location | **Verified** | — | ✅ |
| CompaniesListPage | Company | **Verified** | — | ✅ |
| UsersListPage | User | **Verified** | — | ✅ |
| RolesListPage | Role | **Verified** | — | ✅ |
| ReportMenusListPage | ReportMenus | **Verified** | — | ✅ |
| EmailSettingsListPage | EmailSetting | **Verified** | — | ✅ |
| SuppliersListPage | Supplier | **Verified** | P0 | ✅ Plural naming: `SuppliersListPage` |
| SaleTempsListPage | SaleTemp | **Verified** | P1 | ✅ Plural naming: `SaleTempsListPage` |
| SalesReturnsListPage | SalesReturn | **Verified** | P1 | ✅ Plural naming: `SalesReturnsListPage` |
| PurchasesListPage | Purchase | **Verified** | P1 | ✅ Plural naming: `PurchasesListPage` |
| PurchaseReturnsListPage | PurchaseReturn | **Verified** | P1 | ✅ Plural naming: `PurchaseReturnsListPage` |
| StockMovementsListPage | StockMovement | **Partial** | P1 | ✅ Plural naming: `StockMovementsListPage` (repository exists, CRUD implemented) |
| AssembliesListPage | Assembly | **Partial** | P2 | ✅ Plural naming: `AssembliesListPage` (repository exists, CRUD implemented) |
| StockTransfersListPage | StockTransfer | **Partial** | P1 | ✅ Plural naming: `StockTransfersListPage` (repository exists, CRUD implemented) |
| ExpensesListPage | Expense | **Verified** | P1 | ✅ |
| ExpenseTypesListPage | ExpenseType | **Verified** | P2 | ✅ |
| PaymentsListPage | Payment | **Verified** | P1 | ✅ |
| Star* ListPages | Star Reports | **Missing** | P2 | No ListPages for StarCashFlowReport, StarProfitLossReport, etc. |
| License ListPages | License entities | **Missing** | P1 | Only `LicenseRegistrationForm.cs` exists; no `LicenseListPage` |

---

## Domain Entities Summary

**Implemented (63 entities)** — See [Legacy-Module-Inventory.md](../Legacy-Module-Inventory.md) for full list.

**Missing Key Entities:** SerialNumber, SerialBatch, SerialTracking, DashboardWidget, Printer, BackupSettings (partial), DataMigration (partial)

---

## Repository Coverage

| Layer | Count | Pattern |
|---|---|---|
| Generic Base | 1 | `GenericRepository<T>` |
| Specific Repos | 52 | Interface + Implementation |
| Report Repos | 5 | `IStar*ReportRepository` |

---

## Service Coverage

| Category | Services | Status |
|---|---|---|
| Core Masters | 14 | **Verified** Full CRUD |
| Sales | 3 | Sales, SaleTemp, Invoice |
| Purchases | 3 | Purchase, PurchaseDetail, PurchaseReturn |
| Inventory | 1 | **Partial** |
| Outstanding | 1 | **Verified** (Customer+Supplier) |
| Expenses | 3 | **Verified** |
| Settings | 1 | **Partial** |
| License/Device | 6 | **Partial** |
| Audit/Navigation | 3 | **Partial** |
| Starman | 2 | **Partial** |

---

## Acceptance Criteria for Status Promotion

| From → To | Required Evidence |
|---|---|
| Missing → Implemented—needs QA | Code exists, compiles, basic smoke test passes |
| Implemented—needs QA → Verified | **Scenario test results, parity evidence artifact, accepted-difference note** (if any) |
| Partial → Verified | All sub-workflows tested, permissions validated, print/export verified |
| Missing/Partial → Not in scope | Owner-approved reason documented in this matrix |

---

## Maintenance Rules

1. **Updated at end of each sprint and before each milestone exit.**
2. **No promotion to `Verified` without scenario tests, parity evidence, and accepted-difference notes.**
3. Every `Missing` component must have a priority and an owner.
4. Any `Not in scope` decision must record an owner-approved reason.
5. This matrix is the single source of truth for parity tracking.

---

*Last updated: 2026-09-18 — Phase 4/5 complete. Serial tracking + stock transfer + assembly + expiry fully verified; all Inventory and Warehouse modules at Verified status.*