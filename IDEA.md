# 🚀 MMNextPOS – Idea & Roadmap Documentation

> **Status**: Living document reflecting the N-Tier architecture implementation for the modern Windows POS system. Updated to include Cloudflare SaaS integration capabilities.

## 1. Project Vision & Goals

MMNextPOS is a **complete modernisation** of a legacy Windows POS system, built with **.NET 8, WinForms, and DevExpress**. It replaces a monolithic FusionPOS codebase with a clean layered architecture (Domain → Infrastructure → Application → Presentation) and asynchronous, test-driven development.

### Core Goals
- ✅ **Eliminate UI-freezing** and memory leak issues from the legacy system via async/await patterns
- ✅ **Decouple the UI from the database** using a clean N-Tier architecture with Dependency Injection
- ✅ **Utilise modern DevExpress UI controls** for a premium, fast, and user-friendly experience
- ✅ **Make the MySQL database scalable, reliable, and testable** with Dapper async queries and Testcontainers
- ✅ **Integrate Cloudflare SaaS services** for distributed settings, bot protection, background processing, and object storage
- ✅ **Achieve 100% FusionPOS parity** across 14 phases (A–N) with defined acceptance criteria

### Success Metrics
- Build green on CI (Windows + Ubuntu matrix)
- ≥80% unit test coverage across services and repositories
- All 96 legacy reports available as DevExpress XtraReport templates
- Role-based navigation enforced, passwords hashed (BCrypt)
- Self-contained EXE + WiX MSI build with Squirrel auto-update

---

## 2. Technology Stack & Architecture

### Presentation Layer
- **C# WinForms** with **DevExpress 26.1.4** (Fluent Design, DocumentManager, XtraGrid)
- **Async-first UI patterns** using `RunAsync` and `SplashScreenManager` overlay
- **Dark/Light mode toggle** via settings, stored in Cloudflare KV or local JSON
- **High-DPI support** with `SetHighDpiMode(SystemAware)`

### Business Logic Layer (Application)
- **Service classes** containing core POS rules (stock validation, audit logging, expense summarisation)
- **MediatR/CQRS pattern** optional for command handling
- **IUnitOfWork** per operation with `BeginTransactionAsync`/`CommitAsync`
- **Moq-in-memory unit tests** for every public service method

### Data Access Layer (Infrastructure)
- **Dapper** (not EF Core) for async MySQL queries via MySqlConnector
- **Generic Repository** pattern with `GetPageAsync` paging and `IsDeleted` soft-delete filtering
- **DatabaseInitializer** with idempotent `CREATE TABLE IF NOT EXISTS` statements
- **Testcontainers** MySQL for integration test suite

### Cross-Cutting Concerns
- **Serilog JSON logging** with optional Seq/ELK sink
- **Audit logging** automatic on all Create/Update/Delete operations (ChangeDateLog table)
- **Windows Credential Manager** via `ISecretProvider` abstraction for DB credentials
- **Cloudflare SaaS integration** (see Section 4)

### Architecture Diagram
```mermaid
flowchart TB
    subgraph Domain
        direction LR
        Product[Product POCO]
        Customer[Customer POCO]
        Sale[Sale POCO]
        SaleDetail[SaleDetail POCO]
        Expense[Expense POCO]
    end

    subgraph Infrastructure
        direction LR
        RepoBase[RepositoryBase]
        ProductRepo[ProductRepository]
        SaleRepo[SaleRepository]
        DBInit[DatabaseInitializer]
    end

    subgraph Application
        direction LR
        SalesService[SalesService]
        CustomerService[CustomerService]
        DI[DependencyInjection]
        TurnstileSvc[Cloudflare Turnstile]
        KvStore[Cloudflare KV]
        QueueSvc[Cloudflare Queues]
        R2Store[Cloudflare R2]
    end

    subgraph Presentation
        direction LR
        MainForm[MainForm (WinForms)]
        Program[Program.cs – DI bootstrap]
    end

    Domain --> Infrastructure
    Infrastructure --> Application
    Application --> Presentation
    Presentation --> Program
    Cloudflare -->|API Calls| Application
```

---

## 3. Core Modules & Feature Roadmap

The project is organised into **14 phases (Weeks A–N)**, each with defined deliverables and acceptance criteria.

### 🟢 Phase A – Foundation Harden (Weeks 1–2)
- Fix all compile errors (BorderStyles, LookUpColumnInfo, duplicate locals)
- Add missing DevExpress `using` directives
- Refactor `MainForm` protected-member access
- Run `dotnet build` until **green**
- Update CI to use .NET 10 SDK
- Verify unit-tests still pass

### 🟡 Phase B – Data-Model Completion (Weeks 3–4)
- Generate missing POCOs from FusionPOS Discovery Report
- Create entities: `Invoice`, `PurchaseReturn`, `StockMovement`, `LicenseInfo`, `DashboardWidget`
- Update `DatabaseInitializer` with `IF NOT EXISTS` statements
- Add repository interfaces and concrete classes for new entities
- Add service interfaces and implementations

### 🟠 Phase C – Generic Repository Layer (Weeks 5–6)
- Refactor all repos to inherit from `GenericRepository<T>`
- Implement `GetPageAsync(int page, int pageSize)` for paging
- Add `IsDeleted` column to all tables, update `EntityBase`
- Amend `GenericRepository<T>.GetAllAsync` to filter `IsDeleted = 0`
- Write unit-tests for paging and soft-delete behavior

### 🟡 Phase D – Application Layer Expansion (Weeks 7–10)
- **Purchase Service** (`IPurchaseService`, `PurchaseService`)
- **Inventory Service** – stock validation, movement, assembly, barcode
- **Return Service** (`ISalesReturnService`)
- **Settings Service** (`ISettingService`) – CRUD for master data
- **License Service** (`ILicenseService`)
- **Report Service** (`IReportService`)
- All services use `IUnitOfWork.BeginTransactionAsync`/`CommitAsync`
- Add unit-tests for each new service (Moq + in-memory DB)

### 🟠 Phase E – Presentation Foundation (Weeks 11–12)
- Convert every list form to inherit from `ListPage<T>`
- Implement standard toolbar (`Refresh`, `New`, `Edit`, `Delete`, `Export CSV/Excel`)
- **DevExpress skin manager** with light/dark toggle
- **Global search box** on main form
- **Context menu** (`Edit`, `Delete`, `Copy`, `Export Row`)
- **Keyboard shortcuts** (`F5`, `Ctrl+N`, `Enter`, `Del`)
- All forms use `RunAsync` for service calls + `SplashScreenManager`

### 🔴 Phase F – Sales Module (Weeks 13–14)
- Finish `SalesHistoryForm`, `SalesHoldForm`, `SalesReturnForm`, `LiveSaleForm`
- **Receipt printing** using DevExpress `XtraReport`
- **Status workflow** (Active → Hold → Voided → Completed)
- **Audit-log entries** for each sale mutation

### 🟡 Phase G – Purchases & Inventory (Weeks 15–18)
- UI and services for **Purchases** (list, invoice, return, hold)
- **Inventory screens**: Stock Entry, Issue, Receive, Adjust, Damaged, Lost, Assembly/De-assembly, Barcode scan, Sale-price history
- **Stock movement journal** + `StockMovementForm`
- All inventory actions wrapped in **single transaction**

### 🟠 Phase H – Returns, Holds, Delivery (Weeks 19–20)
- **Delivery** forms (`DeliveryForm`, `PickupForm`)
- **Bank Payment** integration (simple stub)
- Connect returns/holds to sales service (inventory rollback on void)

### 🟡 Phase I – Reporting & Vouchers (Weeks 21–23)
- Convert 96 legacy `rpt*` files to DevExpress `XtraReport` templates
- **ReportViewerForm** – list reports, parameter selection, preview, export to PDF/Excel
- **Voucher printing** (receipt, invoice, purchase order)

### 🟠 Phase J – Settings, License & Backup (Weeks 24–25)
- Complete **SettingsForm** with tabs for all master data + app preferences
- **Licence registration UI** (`LicenceForm`) – reads licence file, validates expiry, stores device binding
- **BackupService** runs `mysqldump` and restores from selected file; exposed via UI
- **👉 Cloudflare R2 integration** for offloading backup files and generated reports

### 🔴 Phase K – Security, Auth & Auditing (Weeks 26–27)
- **LoginForm** with `IUserService.AuthenticateAsync` (BCrypt hashed passwords)
- **Windows Credential Manager** via `ISecretProvider` abstraction for DB credentials
- **Role-based navigation** (`IMainNavigationService`) – menu items filtered by user roles
- **AuditLogService** – writes to `AuditLogs` table; UI audit-log viewer
- **👉 Cloudflare Turnstile** integration to protect admin web interface or API endpoints

### 🟡 Phase L – Performance, Testing & CI (Weeks 28–30)
- Add **paging** to all list pages (`GetPageAsync`)
- **Cache lookup tables** (`ExpenseType`, `Supplier`, `Product`) with `IMemoryCache`
- Switch Serilog to **Async** sink (`WriteTo.Async`)
- **SQL profiling** (log queries >200 ms)
- Increase test coverage to **≥80%** across services and repositories
- Enable **CodeQL** and **Dependabot** in GitHub Actions
- Add **GitVersion** for semantic versioning
- **👉 Cloudflare Queues** for offloading background job processing

### 🟠 Phase M – Migration Tool & Documentation (Weeks 31–32)
- Create `MigrationTool` console app using AutoMapper and batch transactions
- **Batch-mode** (transaction per 1,000 rows) and **validation** mode
- Write **on-boarding docs**, **migration guide**, **ER diagram**, **contribution guide**, **`.editorconfig`**
- **👉 Cloudflare KV** for distributed feature flags across POS installations

### 🟡 Phase N – Release & Deploy (Week 33)
- CI builds **self-contained EXE** (`dotnet publish -r win-x64 --self-contained`) and **WiX MSI**
- Publish artifacts to **GitHub Release** (tag `v1.0.0`)
- **Squirrel** auto-update integration (check GitHub Releases on start-up)
- **Installer documentation** (MSI run, DB config, licence activation)
- **👉 Cloudflare Workers** as lightweight API layer for mobile/3rd-party integrations

---

## 4. Cloudflare SaaS Integration

| Service | Purpose | Implementation Status |
|---------|---------|----------------------|
| **Turnstile** | Bot protection for admin forms/APIs | Planned for Phase K |
| **KV** | Distributed feature flags/settings across installations | Planned for Phase M |
| **Queues** | Background job processing (report generation, exports) | Planned for Phase L |
| **Workers** | Lightweight API layer for mobile/3rd-party | Planned for Phase N |
| **R2** | Object storage for reports, receipts, backups | Planned for Phase J |

### 4.1 Cloudflare Turnstile (Phase K)
- Replaces reCAPTCHA on any web-facing forms
- Verification via `/turnstile/verify` HTTP endpoint
- Stores site key + secret in `appsettings.json` or environment variables
- Verification service injected via DI: `ITurnstileVerificationService`

### 4.2 Cloudflare KV (Phase M)
- Global key-value store for feature flags, configuration
- Survives across multiple POS installations without central DB
- `GetAsync<T>(key)`, `SetAsync<T>(key, value)`, `DeleteAsync(key)`
- Used for dark-mode toggle, feature flags, distributed settings

### 4.3 Cloudflare Queues (Phase L)
- Reliable background job processing without managing infrastructure
- Enqueue from WinForms: `_queueService.EnqueueAsync("GenerateReport", payload)`
- Cloudflare Worker processes jobs asynchronously
- Report generation, CSV exports, backup processing offloaded

### 4.4 Cloudflare Workers (Phase N)
- Edge-run JavaScript/TypeScript workers for API endpoints
- Exposes REST APIs: `/api/health`, `/api/sales`, etc.
- WinForms app calls via HTTP client
- Can serve as reverse proxy or microservice gateway

### 4.5 Cloudflare R2 (Phase J)
- S3-compatible object storage for reports, receipts, exported files
- Store PDF invoices, CSV exports, backup dumps instead of local filesystem
- Presigned URLs with expiration for download links
- Reduces MySQL storage bloat, offloads file serving

---

## 5. Key Business Rules (Do Not Violate)

1. **Soft Deletes:** Products and Categories must NEVER be permanently deleted (to preserve old sale receipts). Use `IsActive` or `IsDeleted` flags — applies to all tables including Cloudflare KV stored data.

2. **Transactional Integrity:** A Checkout operation (deducting stock + saving sale) must be executed in a single DB Transaction. If one fails, everything rolls back — Cloudflare services are best-effort and should not block business transactions.

3. **Data Binding:** Always use `BindingList<T>` or DevExpress-compatible data sources for UI binding to ensure two-way real-time updates.

4. **Cloudflare SaaS Best Practices:**
   - **Never store secrets** (passwords, connection strings) in Cloudflare KV or R2
   - **Always verify Turnstile tokens server-side** — never trust client-side verification
   - **Use least-privilege API tokens** — limit permissions to specific namespaces/queues/buckets
   - **Monitor Cloudflare dashboard** for usage, errors, and rate limiting
   - **Graceful degradation** — if Cloudflare API fails, fall back to local behavior (don't block UI)

5. **Observability:** Structure all Cloudflare API calls with Serilog context, log success/failure, error codes, and response times.

---

## 6. Dependencies & Project Structure

### NuGet Packages (key additions)
- `Serilog` + `Serilog.Sinks.Console` + `Serilog.Sinks.File` (already included)
- `MySqlConnector` + `Dapper` (already included)
- `MediatR` (optional, for CQRS pattern)
- Cloudflare SDK wrappers (custom HTTP client implementations as described in `Cloudflare SaaS Setup.md`)

### Key Project Files Updated
- `src/MMNextPOS.Application/DependencyInjection.cs` — Register all Cloudflare services
- `Cloudflare SaaS Setup.md` — Complete integration guide (newly created)
- `src/MMNextPOS.WinForms/Program.cs` — DI bootstrap, DatabaseInitializer call
- `src/MMNextPOS.Application/Services/*.cs` — New service implementations

### Git Integration
- Remote: `https://github.com/MMNext-Org/MMNext-POS.git`
- Branch: `main`
- CI: GitHub Actions with Windows + Ubuntu matrix
- Code Quality: `dotnet format`, dependency review, CodeQL

---

## 7. Next Immediate Step (Week 1 – Phase A)

1. Run `dotnet build` locally, collect every compilation error
2. Add missing DevExpress `using` statements (`DevExpress.XtraEditors.Controls`, `DevExpress.XtraGrid.Views.Grid`)
3. Resolve duplicate local variables (`buttonPanel`, `mainLayout`, etc.)
4. Add the missing `_serviceProvider` field to `NewSaleForm` (or refactor to use DI)
5. Fix all `BorderStyles`, `LookUpColumnInfo`, `SearchMode`, `TextEditStyles` references
6. Commit a new `.gitignore`, update GitHub Actions to use .NET 10 SDK, and push a **green build**
7. Once the build is green, open a PR and have the team review the changes

---

*Last updated: 2026-09-04*  
*Project: MMNextPOS – Modern Windows POS Application*  
*Architecture: N-Tier (Domain → Infrastructure → Application → Presentation)*  
*Database: MySQL 8.0 with Dapper async*  
*UI: DevExpress WinForms 26.1.4*  
*Cloudflare SaaS: Turnstile + KV + Queues + Workers + R2 (planned phases K, M, L, N, J)*