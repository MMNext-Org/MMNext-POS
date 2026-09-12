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
| ucSales | **Implemented—needs QA** | P0 | WinForms Lead | SalesListPage.cs exists with CRUD; needs scenario coverage |
| ucSalesHistory | Missing | P1 | WinForms Lead | Historical sales view |
| ucSalesHold | Missing | P1 | WinForms Lead | Hold/retrieve sales |
| ucSalesInvoice | Missing | P1 | WinForms Lead | Invoice view/print |
| ucLiveSaleHistory | Missing | P2 | WinForms Lead | |
| ucSalesReturn | Missing | P1 | WinForms Lead | Return entry |
| ucSalesReturnMain | Missing | P2 | WinForms Lead | Return dashboard |
| ucSalesReturnInvoice | Missing | P2 | WinForms Lead | Return invoice |
| frmBankPayment | Missing | P1 | WinForms Lead | Payment processing |
| frmDelivery | Missing | P2 | WinForms Lead | Delivery management |
| frmLiveSalesEdit | Missing | P1 | WinForms Lead | Live edit form |
| frmSalesEdit | **Partial** | P0 | WinForms Lead | NewSaleForm.cs exists (create new sale); edit/hold/return paths missing |
| frmSalesHold | Missing | P1 | WinForms Lead | |
| frmSalesReturnBySalesInvoice | Missing | P2 | WinForms Lead | |

**Entities:** Sale, SaleDetail, SaleTemp, SaleTempDetail, SalePriceHistory, SalesReturn, SalesReturnDetail, Invoice, Payment

---

## 2. Contacts Module

| Legacy Component | Status | Priority | Owner | Notes / Evidence |
|---|---|---|---|---|
| ucCustomer | **Implemented—needs QA** | P0 | WinForms Lead | CustomersListPage.cs exists with CRUD |
| ucCustomerAdvanced (+Tab/History) | Missing | P1 | WinForms Lead | Advanced tabs, history |
| ucSupplier | **Partial** | P0 | WinForms Lead | Supplier entity + repo + service exist; no ListPage yet |
| frmCustomerImport | Missing | P2 | WinForms Lead | Import dialog |
| frmSupplierImport | Missing | P2 | WinForms Lead | Import dialog |

**Entities:** Customer, Supplier, CustomerOutstanding, SupplierOutstanding

---

## 3. Inventory Module

| Legacy Component | Status | Priority | Owner | Notes / Evidence |
|---|---|---|---|---|
| Stock Entry | **Implemented—needs QA** | P0 | WinForms Lead | Product + ProductRepository + ProductService + ProductsListPage |
| Issue/Receive/Damaged/Lost/Adjust | **Partial** | P1 | WinForms Lead | StockMovement, StockMovementDetail entities + repos |
| Assembly/Deassembly | **Partial** | P2 | WinForms Lead | Assembly, AssemblyDetail entities + repos |
| Expired | Missing | P3 | WinForms Lead | |
| Linked Stock | **Partial** | P2 | WinForms Lead | LinkedStock entity + repo |
| Barcode | Missing | P1 | WinForms Lead | Barcode scanning/generation |
| Sale-Price History | **Partial** | P2 | WinForms Lead | SalePriceHistory entity + repo |
| Sale-Price Invoice | Missing | P2 | WinForms Lead | |

**Entities:** Product, StockMovement, StockMovementDetail, Assembly, AssemblyDetail, LinkedStock, SerialNumber, SerialBatch, SerialTracking, SalePriceHistory, StockTransfer, StockTransferDetail

---

## 4. Purchases Module

| Legacy Component | Status | Priority | Owner | Notes / Evidence |
|---|---|---|---|---|
| ucPurchase | **Partial** | P0 | WinForms Lead | Purchase, PurchaseDetail entities + repos + service |
| PurchaseHistory | Missing | P1 | WinForms Lead | History list |
| PurchaseHold | Missing | P1 | WinForms Lead | Hold/retrieve |
| PurchaseInvoice | Missing | P1 | WinForms Lead | Invoice view |
| PurchaseReturn (+Invoice/Main) | **Partial** | P1 | WinForms Lead | PurchaseReturn, PurchaseReturnDetail entities + repos + service |
| frm* (various) | Missing | P2 | WinForms Lead | Various forms |

**Entities:** Purchase, PurchaseDetail, PurchaseReturn, PurchaseReturnDetail

---

## 5. Outstanding / Payments

| Legacy Component | Status | Priority | Owner | Notes / Evidence |
|---|---|---|---|---|
| ucCustomerOutstand | **Partial** | P0 | WinForms Lead | CustomerOutstanding entity + repo + service + OutstandingListPage |
| ucSupplierOutstand | **Partial** | P0 | WinForms Lead | SupplierOutstanding entity + repo + service + OutstandingListPage |
| Payments & History | Missing | P1 | WinForms Lead | Payment processing, history view |

**Entities:** CustomerOutstanding, SupplierOutstanding, Payment

---

## 6. Expenses

| Legacy Component | Status | Priority | Owner | Notes / Evidence |
|---|---|---|---|---|
| ucExpense | **Partial** | P1 | WinForms Lead | Expense, ExpenseType entities + repos + service + ExpenseSummaryForm |

**Entities:** Expense, ExpenseType

---

## 7. Warehouse / Stock Transfer

| Legacy Component | Status | Priority | Owner | Notes / Evidence |
|---|---|---|---|---|
| Transfer | **Partial** | P1 | WinForms Lead | StockTransfer, StockTransferDetail entities + repos |
| Adjust | **Partial** | P1 | WinForms Lead | StockMovement covers adjustments |
| Damaged | Missing | P2 | WinForms Lead | |
| Stock List | **Partial** | P1 | WinForms Lead | ProductsListPage covers stock listing |
| Lookups | Missing | P3 | WinForms Lead | |

**Entities:** StockTransfer, StockTransferDetail, StockMovement, Location

---

## 8. Starman (Multi-Site)

| Legacy Component | Status | Priority | Owner | Notes / Evidence |
|---|---|---|---|---|
| Stock Transfer Received/Accept | **Partial** | P2 | WinForms Lead | StarStockTransferReceived entity + repo |
| Sale-Price Transfer/Accept | **Partial** | P2 | WinForms Lead | StarSalePriceTransfer entity + repo |
| Star Reports | **Partial** | P2 | WinForms Lead | StarCashFlowReport, StarProfitLossReport, StarStockBalanceReport, StarReorderReport, StarOutstandingReport entities + repos |

**Entities:** StarCashFlowReport, StarProfitLossReport, StarStockBalanceReport, StarReorderReport, StarOutstandingReport, StarSalePriceTransfer, StarStockTransferReceived, RemoteWarehouse, IssueHeader

---

## 9. Dashboard

| Legacy Component | Status | Priority | Owner | Notes / Evidence |
|---|---|---|---|---|
| ucQuickSummaryMain | Missing | P1 | WinForms Lead | Quick summary dashboard |
| ucDataView | Missing | P2 | WinForms Lead | Data visualization |
| ucFinancialView | Missing | P2 | WinForms Lead | Financial dashboard |

**Entities:** DashboardWidget

---

## 10. Reports (~96 rpt*.cs)

| Report Category | Status | Priority | Owner | Notes |
|---|---|---|---|---|
| Sale Reports | Missing | P0 | Reports Lead | No XtraReports implemented |
| Purchase Reports | Missing | P0 | Reports Lead | Need IReportService + DevExpress XtraReport definitions |
| Inventory Reports | Missing | P1 | Reports Lead | |
| Financial Reports | Missing | P0 | Reports Lead | |
| Outstanding Reports | Missing | P1 | Reports Lead | |

---

## 11. Print Vouchers (~100 layouts)

| Voucher Type | Status | Priority | Owner | Notes |
|---|---|---|---|---|
| A4 Receipts | Missing | P0 | Reports Lead | No printing/voucher infrastructure |
| A5 Receipts | Missing | P0 | Reports Lead | |
| Slips | Missing | P1 | Reports Lead | |
| Barcodes | Missing | P1 | Reports Lead | |

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
| Theme | **Partial** | P2 | WinForms Lead | Theme entity + repo; no ListPage |
| Language | **Partial** | P2 | WinForms Lead | Language entity + repo; no ListPage |
| Fonts (Myanmar) | Missing | P1 | Localization Owner | |
| Backup | Missing | P1 | Ops Owner | |
| Data Migration | Missing | P2 | Ops Owner | |

---

## 13. SuperAdmin

| Component | Status | Priority | Owner | Notes |
|---|---|---|---|---|
| Deleted/All Invoice Views | Missing | P2 | Security Owner | |
| Change-Date Log | **Partial** | P1 | Security Owner | ChangeDateLog entity + repo + audit service |
| JSON Log History | Missing | P2 | Security Owner | |
| Script Executor | Missing | P2 | Security Owner | |

---

## 14. License

| Component | Status | Priority | Owner | Notes |
|---|---|---|---|---|
| Registration | Missing | P0 | Security Owner | |
| Device Binding | **Partial** | P0 | Security Owner | PcClient, MobileClient, DeviceRequest, PCUpdate, ClientUpdateRequest, AppInfo, LicenseInfo, Subscription, Registration entities + repos |
| Expiry Logic | Missing | P0 | Security Owner | |

---

## 15. Menus

| Component | Status | Priority | Owner | Notes |
|---|---|---|---|---|
| Role-Based Main Menu | **Partial** | P0 | Security Owner | MenuRole entity + repo + service + IMainNavigationService |
| Sub Menu Definitions | **Partial** | P1 | Security Owner | ReportMenus entity + repo + service + ListPage |

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

| ListPage | Entity | Status | Priority |
|---|---|---|---|
| ProductsListPage | Product | **Verified** | — |
| CustomersListPage | Customer | **Verified** | — |
| SalesListPage | Sale | **Implemented—needs QA** | P0 |
| OutstandingListPage | Customer/Supplier Outstanding | **Implemented—needs QA** | P0 |
| CategoriesListPage | Category | **Verified** | — |
| UnitsListPage | Unit | **Verified** | — |
| GroupsListPage | Group | **Verified** | — |
| CurrenciesListPage | Currency | **Verified** | — |
| TaxesListPage | Tax | **Verified** | — |
| DiscountsListPage | Discount | **Verified** | — |
| LocationsListPage | Location | **Verified** | — |
| CompaniesListPage | Company | **Verified** | — |
| UsersListPage | User | **Verified** | — |
| RolesListPage | Role | **Verified** | — |
| ReportMenusListPage | ReportMenus | **Verified** | — |
| EmailSettingsListPage | EmailSetting | **Verified** | — |
| SupplierListPage | Supplier | **Missing** | P0 |
| SaleTempListPage | SaleTemp | **Missing** | P1 |
| SalesReturnListPage | SalesReturn | **Missing** | P1 |
| PurchaseListPage | Purchase | **Missing** | P1 |
| PurchaseReturnListPage | PurchaseReturn | **Missing** | P1 |
| StockMovementListPage | StockMovement | **Missing** | P1 |
| AssemblyListPage | Assembly | **Missing** | P2 |
| StockTransferListPage | StockTransfer | **Missing** | P1 |
| ExpenseListPage | Expense | **Missing** | P1 |
| ExpenseTypeListPage | ExpenseType | **Missing** | P2 |
| PaymentListPage | Payment | **Missing** | P1 |
| Star* ListPages | Star Reports | **Missing** | P2 |
| License ListPages | License entities | **Missing** | P1 |

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
| Purchases | 2 | Purchase, PurchaseDetail |
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

*Last updated: 2026-09-07 — initial creation from Legacy-Module-Inventory.md*