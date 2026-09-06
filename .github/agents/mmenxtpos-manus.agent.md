# MMNext POS – Incremental Roadmap to Full Parity with FusionPOS
*(Rewrite: target = 100% feature-completeness against `J:\Project 1\POS\FusionPOS`)*

---

## 0. Guiding Principles
1. **Feature discovery first** – each module is fully enumerated from the legacy codebase before coding starts; nothing is skipped.
2. **Layered architecture preserved** – every module gets a Domain POCO, Repository, Service, and WinForms UserControl; no UI-to-DB shortcuts.
3. **DevExpress remains** the UI toolchain (GridControl, XtraReport, BarcodeReport, …) to minimise behavioural risk.
4. **MySQL remains** the production DB; SQLite may remain as an offline fallback.
5. **Each sprint finishes build-green + test-green** before the next module begins.

---

## 1. Full Module Inventory (from FusionPOS)

| Module group | Legacy folder | What it contains |
|---|---|---|
| **Sales** | `Sales/` | ucliveSaleMain, ucLiveSales, ucSales, ucSalesHistory, ucSalesHold, ucSalesInvoice, ucLiveSaleHistory, ucSalesReturn, ucSalesReturnMain, ucSalesReturnInvoice, frmBankPayment, frmDelivery, frmLiveSalesEdit, frmSalesEdit, frmSalesHold, frmSalesReturnBySalesInvoice |
| **Contacts** | `Contacts/` | ucCustomer, ucCustomerAdvanced(+Tab/History), ucSupplier, frmCustomerImport, frmSupplierImport |
| **Inventory** | `Inventory/` | stock entry, issue/receive/damaged/lost/adjust, assembly/deassembly, expired, linked stock, barcode, sale-price history/invoice |
| **Purchases** | `Purchases/` | ucPurchase, PurchaseHistory, PurchaseHold, PurchaseInvoice, PurchaseReturn(+Invoice/Main), frm* |
| **Outstanding / Payments** | `Outstanding/` | ucCustomerOutstand, ucSupplierOutstand, payments & history |
| **Expenses** | `Expenses/` | ucExpense |
| **Warehouse / Stock Transfer** | `Warehouse/` | transfer, adjust, damaged, stock list, lookups |
| **Starman (multi-site)** | `Starman/` | stock transfer received/accept, sale-price transfer/accept, star reports (cash flow, P&L, stock balance, reorder, outstanding) |
| **Dashboard** | `Dashboard/` | ucQuickSummaryMain, ucDataView, ucFinancialView |
| **Reports** | `Reports/` | ~96 `rpt*.cs` report definitions (sale / purchase / inventory / financial / outstanding) |
| **Print vouchers** | `PrintVoucher/` | ~100 receipt/voucher layouts (A4, A5, slips, barcodes) |
| **Settings** | `Settings/` | company, unit, category, group, currency, tax, discount, printer, theme, language, fonts (Myanmar), backup, data migration |
| **SuperAdmin** | `SuperAdmin/` | deleted/all invoice views, change-date log, JSON log history, script executor |
| **License** | `License/`, `Program.cs` | registration, device binding, expiry logic |
| **Menus** | `Menus/` | role-based main & sub menu definitions |
| **API controller** | `Controller/` | SaleController (self-hosted HTTP API) |
| **DB utilities** | `Utilities/`, `DBConfig/` | connection tester, restore helpers |

---

## 2. Gap Matrix

| Layer | Already built | Still to build |
|---|---|---|
| Domain POCOs | Product, Customer, Sale, SaleDetail, Invoice, exceptions | + every remaining VO (~150 entities / value objects in `app.config`) |
| Infrastructure repos | Product, Sale, Customer repos + UnitOfWork | repos for every VO + report query repos |
| Application services | Sales, Customer, Product services | Inventory, Purchase, Outstanding, Expense, Warehouse, Starman, Dashboard, Settings, License, Print, Reporting |
| WinForms UI | MainForm (sales + catalog placeholder), NewSaleForm | ~150 screens rebuilt with async patterns |
| Reports / Printing | – | each rpt* class re-implemented as XtraReport using IReportService |
| License | – | registration, device binding, expiry check |
| Tests | basic unit + integration | per-module suites + coverage target ≥70 % |

---

## 3. Granular Phase Plan

### Phase A — Foundation Hardening (Week 0)
- Fix remaining build errors; turn warnings-as-errors on.
- Introduce `EntityBase` (Id, CreatedAt, UpdatedAt, CreatedBy).
- Full UnitOfWork tests: commit/rollback, connection reuse.
- Add `.gitignore`; initial git commit; CI uses .NET SDK 10 preview.

### Phase B — Data Model Completion (Week 1)
Generate POCOs for every VO listed in legacy `app.config`:
- Masters: `Category`, `Unit`, `Group`, `Currency`, `Tax`, `Discount`, `Location`, `Company`, `User`, `Role`, `UserRole`, `MenuRole`, `ReportMenus`, `EmailSetting`, etc.
- Sales VOs (`SaleTemp`, `SaleDetailTemp`, serials, LiveSale*, LiveName).
- Purchases VOs (`PurchaseTemp`, serials).
- Inventory VOs (opening/issue/receive/damaged/lost/adjust + serial versions, sale-price, assembly/deassembly, linked, code-merge).
- Outstanding/payment VOs (customer & supplier, outstand view VOs).
- Expense VOs.
- Warehouse/Starman VOs (`StockTransfer*`, `RemoteWarehouse`, `IssueHeader`, plus the `Star*` report DTOs).
- License VOs (`Registration`, `Subscription`, `LicenseInfo`, `DeviceInfo`, `PcClient`, `MobileClient`, `AppInfo`, `DeviceRequest`, `PCUpdate`, `ClientUpdateRequest`).
- Extend `DatabaseInitializer` with per-table `CREATE TABLE IF NOT EXISTS` and idempotent ALTERs.

### Phase C — Generic Repository Layer (Week 2)
- Generic `IRepository<T>` CRUD repo; per-domain repo interfaces.
- All repos use `IUnitOfWork.Connection` + current `Transaction`.
- Paging standard (`PageSize` + `PageIndex`).
- Migrate Product/Sale/Customer repos to the generic base.

### Phase D — Application Layer Expansion (Weeks 3-4)
Service interfaces + implementations for:
- `IInventoryService` (stock movements incl. serials, assemblies)
- `IPurchaseService`
- `IOutstandingService`
- `IExpenseService`
- `ISettingService` (unit/category/group/currency/tax/discount/etc.)
- `IUserService`, `IRoleService`, `ILicenseService`, `IBackupService`, `ILanguageService`, `IThemeService`
- `IDocumentService` (invoice numbering)
- `IReportService` (parameterized reporting)
- Audit-log entries written in every transactional method.

### Phase E — Presentation Foundation (Week 5)
- `AsyncFormBase` extensions: Cancel, progress helper, confirm helper.
- Base `ListPage<T>` (search/refresh/new/edit/delete/export CSV/paging).
- `IMainNavigationService` to build FluentDesign side-nav from role menus.
- MainForm rewritten on top of `ListPage` for Products/Customers/Sales.

### Phase F — Sales Module (Week 6)
- Sales List page + detail view, print receipt, delete/void logic, hold/un-hold, live-sale edit, delivery.
- NewSaleForm hardening (barcode scanner, currency, discount, tax, live stock badge).
- Sales History / Return / ReturnInvoice / ReturnMain / SalesHold / LiveSale screens.
- CSV export format parity.

### Phase G — Contacts / Purchases / Outstanding / Expenses (Week 7)
- Contact & supplier lists, import dialogs, customer-advanced tabs, payment & history forms.
- Purchase list + Purchase Return/Hold/Invoice screens.
- Outstanding UC (receivable/payable), auto-close when balance = 0.
- Expense entry + types + monthly summary.

### Phase H — Inventory & Warehouse (Weeks 8-9)
- Stock entry / issue / receive / adjust / damaged / lost / expired forms with serial tracking.
- Assembly/Deassembly (BOM).
- Stock-transfer screens incl. transfer received acceptance + history.
- Remote-warehouse concept for Starman flows.

### Phase I — Reports & Vouchers (Weeks 10-11)
- Implement every legacy `rpt*` as DevExpress XtraReport fed by `IReportService`.
- Top priority: Sale family, Purchase family, Inventory family, Financial (P&L / Cash Flow / IncomeExpense), Outstanding, Dashboard summary, all vouchers & barcodes.
- Report viewer page with parameter form, preview, print, export-pdf/xls.

### Phase J — Admin & Cross-cutting (Week 12)
- Settings screens: company, currency, tax, discount, units, categories, themes, language, font converter, backup/restore, data-migration wizard.
- License screens & device-binding flow (custom .NET 8 HTTP listener).
- SuperAdmin views: invoice history, deleted views, change-date log, JSON log viewer, script executor.

### Phase K — Parity QA (Week 13)
- Side-by-side checklist of every menu item, field, voucher and report.
- Performance tests with 10k+ records for grids, paging, filtering.
- Myanmar fonts/language verification; theme switching verified.
- Memory-leak audit on DevExpress controls (dispose patterns enforced).

### Phase L — Release & Installer (Week 14)
- `dotnet publish -r win-x64 --self-contained` → `publish/` bundle.
- Installer: WiX MSI + silent-install PowerShell (or ClickOnce variant).
- Migration tool: imports legacy MySQL data into the new schema (CLI exe).
- Tag `v1.0.0`, upload artifacts, release notes.

---

## 4. Milestones & Acceptance Criteria

| Milestone | Definition of Done | Verification |
|---|---|---|
| M1 Foundation green | `dotnet build` 0 errors, all unit tests pass | CI badge |
| M2 Business modules complete | Every legacy menu item maps to a working screen | UI review + smoke tests |
| M3 Print/report parity | Top 10 legacy vouchers/reports produce identical output | Side-by-side PDF diff |
| M4 License workflow | Registration + expiry rules enforced on fresh install | license unit tests |
| M5 User-acceptance | Staging migration passes, onboard smoke OK | sign-off checklist |

---

## 5. Engineering Rules (every module)
1. UI pages inherit `AsyncFormBase`; long tasks never block UI (`RunAsync`).
2. Every DB call has `CancellationToken` support and uses `IUnitOfWork`.
3. Every form overrides `Dispose(bool)` and cancels its own `CancellationTokenSource`.
4. Audit-log row written per write-action (`ChangeDateLog`, `CustomerLog`, etc.).
5. Shared styles: fonts, colours, spacing from `UITheme` class derived from legacy look.

---

## 6. Risk & Mitigation
- **DB drift**: migration utility reads the legacy schema live, never hand-edited.
- **DevExpress license**: evaluation version for dev, production requires registered license file.
- **Myanmar font issues**: include `ucMyanmarFontConverter` equivalent early; test on a mix of Zawgyi & Unicode.
- **Scope creep**: each phase modifies only its area; 1-week cap enforced.

---

## 7. Next Immediate Step
Run **Phase A**:
1. Fix compilation errors in WinForms & test projects (known list).
2. Create `.gitignore`, create first commit.
3. Upgrade CI run to dotnet SDK 10.

*This plan now contains a feature-complete, sprint-ready path to bring MMNext POS to full parity with FusionPOS while keeping a modern, test-driven .NET 8 + DevExpress architecture.*