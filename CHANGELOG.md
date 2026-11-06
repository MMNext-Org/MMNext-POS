# Changelog

All notable changes to MMNext POS will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added
- **Parity gap closures (2026-10-04)**
  - AR Ageing report: `IOutstandingService.GetCustomerAgeingAsync` (0-30 / 31-60 / 61-90 / 90+ buckets), `CustomerAgeingReport` (DevExpress), "AR Ageing" button on the Outstanding page; 5 new unit tests
  - Connection Tester UI: `ConnectionTestPage` Admin navigation page surfacing `ConnectionTestService` (latency, server version, table count, DB size)
  - Customer/Supplier CSV import wired into UI: "Import CSV" toolbar buttons on Customers/Suppliers pages (Admin/Manager roles only)
  - `ListPage<T>` base: `AppendToolbarButton` helper for module-specific toolbar actions

### Fixed
- appsettings/README alignment: `MMNEXTPOS_CONNECTION_STRING` environment variable now actually overrides `ConnectionStrings:Default` at startup (main app + health-check scope)
- `docs/PARITY-FINAL-CHECKLIST.md` re-verified against codebase: corrected false "JournalVoucher repository exists" note, test counts (StockTransfer/Serial 16, Expiry 7, Dashboard 9), installer/connection-tester/import statuses, P0/P1 totals
- `docs/parity/PARITY-MATRIX.md`: LiveSale → Partial (dormant form), imports → Verified, Connection Tester → Verified, Restore Helpers → Implemented-needs-QA, Myanmar fonts → Partial (Pyidaungsu bundled; Zawgyi untested)

### Testing
- Build: 0 errors, Release configuration
- Format: `dotnet format --verify-no-changes` clean
- Unit tests: 327/327 passed (Application layer)
- Integration tests: require Docker/MySQL testcontainer (CI Ubuntu job); 017 migration + Journal Voucher repo tests written
- Obfuscation: end-to-end test with 4 assemblies verified — anonymous type JSON serialization, `ConnectionStringOptions` reflection binding, mapping file not shipped

### Added (previous sessions)
- **Expense Management Module**
  - `IExpenseService` / `ExpenseService` - Full CRUD for expense entries
  - `IExpenseTypeService` / `ExpenseTypeService` - Expense categories management
  - `ExpenseForm` - Add/Edit expense entries with validation
  - `ExpenseListForm` - Grid view with Refresh, Add, Edit, Delete actions
  - Auto-generated expense numbers (format: EXP-YYYYMMDD-XXXX)

- **Outstanding Management Module**
  - `IOutstandingService` / `OutstandingService` - Customer & Supplier outstanding balances
  - `OutstandingForm` - Add/Edit outstanding entries
  - Outstanding tab in `SupplierAdvancedForm` with grid, add/edit/delete
  - Proper Debit/Credit semantics for Customer (AR) and Supplier (AP)

- **Audit Logging**
  - `ChangeDateLog` entity - Tracks Create/Update/Delete on all entities
  - `IAuditService` / `AuditService` - Centralized audit logging
  - Audit entries automatically written on all write operations

- **Expense Summary & Reporting**
  - `IExpenseSummaryService` / `ExpenseSummaryService` - Monthly/yearly summaries
  - `ExpenseSummaryForm` - Dashboard with charts and category breakdown
  - Bar chart (monthly expense amounts) + Line chart (transaction count)
  - Export to CSV functionality
  - Added to Main Navigation menu

- **Database Schema**
  - `Expenses`, `ExpenseTypes`, `CustomerOutstandings`, `SupplierOutstandings` tables
  - `ChangeDateLogs` audit table with indexes
  - Idempotent `CREATE TABLE IF NOT EXISTS` in `DatabaseInitializer`

- **Dependency Injection**
  - All new services registered in `DependencyInjection.cs`
  - Scoped repositories and services per request/operation

### Changed
- Fixed WinForms stub compilation errors:
  - `CustomerAdvancedForm` - BorderStyles namespace fix
  - `ImportDialog` - BorderStyles & TextEditStyles namespace fix
  - `SalesReturnInvoiceForm` - BorderStyles + TextAlign → Appearance.TextOptions
  - `SupplierAdvancedForm` - Complete rewrite with functional Outstanding tab
  - `ExpenseSummaryForm` - LookUpEdit SelectedItem → EditValue fix

### Fixed
- Unit test compilation after AuditService integration
- ExpenseSummaryForm month/year selector binding
- SupplierAdvancedForm Outstanding tab button state management

### Testing
- 47 unit tests passing (Expense, ExpenseType, Outstanding services)
- Audit logging verified in all CRUD operations
- Application & Infrastructure layers build clean

---

## [1.0.0] - 2026-09-02 (Planned Release)

### Added
- Complete Expense Management (Entry, List, Summary, Charts)
- Complete Outstanding Management (Customer AR, Supplier AP)
- Audit Logging infrastructure
- Expense Summary Dashboard with DevExpress Charts
- Main Navigation integration

### Technical Debt Addressed
- WinForms stub compilation errors resolved for core new forms
- Dependency Injection properly configured for all new services
- Database initialization idempotent and production-ready

---

## [0.9.0] - 2026-08-31

### Added
- Core Application Services: Sales, Customer, Product, Supplier, Category, Unit, Group, Currency, Tax, Discount, Location, Company, User, Role
- Infrastructure Repositories with Generic Repository pattern
- Database Initializer with MySQL schema
- AsyncFormBase for non-blocking UI operations
- MainForm with DevExpress FluentDesign navigation
- NewSaleForm for point-of-sale operations
- ListPage<T> base class for master-detail grids
- FusionCsvExporter for FusionPOS-compatible CSV export

### Changed
- Migration from legacy FusionPOS to modern .NET 8 architecture
- DevExpress WinForms as UI framework
- MySQL as primary database (SQLite fallback available)

---

## [0.1.0] - 2026-08-30

### Added
- Initial solution structure (Domain, Application, Infrastructure, WinForms projects)
- EntityBase with CreatedAt/UpdatedAt tracking
- Basic Domain Models: Product, Customer, Sale, SaleDetail, Invoice
- Basic Repository pattern implementation
- Docker/Development environment setup