# Prompt — Complete Phase 3: Purchasing, Contacts, Payments, and Expenses

> **How to use this file:** Copy everything between the `### PROMPT START` and `### PROMPT END` fences and paste it to the AI coding agent. The prompt is self-contained and references the repository as the single source of truth. Do not paste the commentary above the fence.

---

### PROMPT START

You are working in the repository **`J:\Project 1\MMNext POS`** (working dir). This is a .NET 8 WinForms POS application named **MMNext POS** (solution `MMNextPOS.slnx`), being rebuilt to functional parity with a legacy FusionPOS system.

## Your mission
Complete **Phase 3 — Purchasing, Contacts, Payments, and Expenses** as defined in `plan.md` (section 4, "Phase 3 — Purchasing, Contacts, Payments, and Expenses"). 

Read these authoritative documents FIRST:
- `plan.md` — Phase 3 scope (section 4) and engineering rules (section 5).
- `docs/parity/PARITY-MATRIX.md` — the status taxonomy and current module statuses.
- `docs/prompts/PROMPT-Phase2-Sales-MVP.md` — the Phase 2 prompt (already executed) as a structural and quality template for how Phase 3 work should be carried out and reported.
- `docs/parity/Sales-Hardening-Parity.md` — the Phase 2 parity evidence format to replicate.

## Repository architecture (follow these conventions strictly)
- **Layered solution:** `src/MMNextPOS.Domain` → `src/MMNextPOS.Infrastructure` → `src/MMNextPOS.Application` → `src/MMNextPOS.WinForms`; tests under `tests/`.
- Target framework `net8.0-windows` (WinForms), nullable reference types enabled, warnings-as-errors.
- Data access: **parameterized Dapper queries** only; repositories implement `I*Repository` + `GenericRepository<T>`; async database access.
- Business logic lives in Application-layer services using constructor-injected repositories (registered in `src/MMNextPOS.Application/DependencyInjection.cs`).
- **Transactions:** every multi-write operation must run inside a single `IUnitOfWork` transaction so header/detail/stock/payment/audit/outstanding updates commit or roll back together.
- **Audit:** every write must produce an audit record (see `ChangeDateLog` / `IAuditService`).
- **UI:** WinForms with DevExpress; follow the existing list-page/form patterns (`AsyncFormBase`, cancellation, progress, confirmation, error handling, proper `Dispose`). **Reuse the existing services and list-page patterns — do not create a parallel architecture.**
- **Tests:** Arrange–Act–Assert, xUnit, Moq for unit tests; MySQL/Testcontainers for integration tests.

## Current verified baseline (do not regress)
- Release build succeeds; Application unit tests currently pass: **175 passed / 0 failed / 0 skipped**.
- Infrastructure migration tests pass against the `000–010` (11-migration) chain.
- Phase 2 (Sales MVP) is complete: `CreateSaleAsync`/`VoidSaleAsync`/`ProcessReturnAsync`/`RoundLines`/`AggregateDuplicateLines`, atomic stock reservation, per-year invoice & return numbering, `CustomerOutstanding` for credit sales, tax-rate lookup by customer type, overpayment credit, and customer clearance are implemented and tested.

## Phase 3 scope — what to do

### 0. Prerequisite — close the Phase 2 schema-drift gap (do this FIRST)
`Customer.CustomerType` exists in `src/MMNextPOS.Domain/Models/Customer.cs` and is consumed by `TaxRateService`, but the column is **not** in the database. Add migration:
- Create `src/MMNextPOS.Infrastructure/Migrations/011_AddCustomerTypeToCustomer.sql` (idempotent `ADD COLUMN IF NOT EXISTS`-style guarded SQL, matching the pattern of `009_AddInvoiceNoToSales.sql`).
- Register it in `src/MMNextPOS.Infrastructure/MigrationRunner.cs` (`LoadMigrations()` list, version `"011"`).
- Update `tests/MMNextPOS.Infrastructure.Tests/MigrationIdempotenceTests.cs` to expect version `"011"` and 12 migrations (000–011) if the assertions hard-code the count.
- Verify with the migration idempotence test suite (Docker/Testcontainers).

### 1. Purchasing workflows (highest priority)
Complete and harden supplier/purchase workflows end-to-end. `IPurchaseService` already exposes many methods (`CreatePurchaseWithDetailsAsync`, `UpdatePurchaseWithDetailsAsync`, `ReceivePurchaseAsync`, `HoldPurchaseAsync`, `ReleasePurchaseAsync`, `CancelPurchaseAsync`, `CreatePurchaseReturnAsync`, `ReceivePurchaseReturnAsync`, stock checks). For each workflow ensure it is **implemented, transactional, audited, and tested**:
- Create purchase (with details, atomic stock increase on receipt).
- Receive purchase (partial receipt allowed, stock updated only for received quantities).
- Hold / release / cancel purchase.
- Purchase return (with `PurchaseReturnDetail`, stock decrement, `SupplierOutstanding` update, return numbering via `IReturnNumberGenerator`).
- Insufficient-supplier-credit and partial-receive edge cases.
- Add unit tests in `tests/MMNextPOS.Application.Tests/PurchaseServiceTests.cs` and integration tests in `tests/MMNextPOS.Infrastructure.Tests/PurchaseServiceIntegrationTests.cs` covering success, validation-failure, rollback, and exception paths.

### 2. Contacts: supplier and customer management
- **Supplier:** `SupplierListPage` is `Missing` per the parity matrix — create it following the existing list-page pattern (e.g., `CustomersListPage`). Supplier entity + repo + service exist; wire the UI. Add supplier CRUD tests.
- **Customer:** `CustomersListPage` is `Verified`; ensure the advanced tabs/history and import dialog gaps are at least documented in the parity matrix. If `frmCustomerImport`/`frmSupplierImport` are out of scope this phase, record them as `Not in scope` with an owner-approved reason.
- Wire navigation/permissions for new pages via `IMainNavigationService`.

### 3. Customer / supplier outstanding and payments
- **Outstanding:** `OutstandingListPage` exists (Customer/Supplier). Verify and test: applying a payment reduces the balance; part payment leaves a remaining balance; overpayment creates a future credit; clearing brings the balance to zero with `"Cleared"` status + audit record. Extend `tests/MMNextPOS.Application.Tests/OutstandingServiceTests.cs`.
- **Payments:** `IPaymentService` exists (`ProcessPaymentAsync`, `GetBySaleAsync`, `GetByCustomerAsync`, `GetBySupplierAsync`, `GetByDateRangeAsync`, etc.). Ensure `ProcessPaymentAsync` is transactional and updates the correct outstanding balance (customer or supplier), records audit, and supports the payment methods. `PaymentsListPage` is `Missing` — create it. Add payment workflow unit + integration tests including payment against outstanding, refund, and validation-failure paths.

### 4. Expenses
- `IExpenseService` exists (CRUD + date-range). `ExpenseEditForm` and `ExpenseMonthlySummaryForm` exist in the working tree (uncommitted). Ensure:
  - Expense types CRUD is complete and tested.
  - Expense entry is transactional + audited.
  - Monthly summary aggregation is correct and tested.
- Create `ExpenseListPage` and `ExpenseTypeListPage` (both `Missing` per parity matrix) following existing list-page patterns. Add expense unit tests in `tests/MMNextPOS.Application.Tests/ExpenseServiceTests.cs` / `ExpenseTypeServiceTests.cs`.

### 5. Parity evidence
Update **`docs/parity/PARITY-MATRIX.md`** for every Phase 3 area (Sales → Purchasing, Contacts, Outstanding/Payments, Expenses) following the status taxonomy. Create **`docs/parity/Phase3-Purchasing-Contacts-Payments-Expenses-Parity.md`** with the same structure as `docs/parity/Sales-Hardening-Parity.md`: per-workflow status, test scenario names, acceptance criteria, and sample outputs. Every promotion to `Verified` requires scenario tests + parity evidence + accepted-difference notes.

## Exit criteria (Definition of Done)
- Supplier/customer management, imports (or explicit deferral), purchases, purchase returns, customer/supplier outstanding, payments, expense types, expense entry, and monthly summaries all have:
  - service unit tests (success, validation-failure, rollback, authorization where applicable),
  - repository/integration tests,
  - UI smoke scenarios (or a documented reason),
  - permission checks,
  - documented validation behavior.
- No regression: existing 175 passing Application tests still pass.

## Quality gates — you must run all of these and report results before claiming completion
```
dotnet build MMNextPOS.slnx --configuration Release
dotnet format MMNextPOS.slnx --verify-no-changes
dotnet test tests/MMNextPOS.Application.Tests/MMNextPOS.Application.Tests.csproj --configuration Release
dotnet test tests/MMNextPOS.Infrastructure.Tests/MMNextPOS.Infrastructure.Tests.csproj --configuration Release
```
(Integration tests use MySQL/Testcontainers and need Docker. If Docker is unavailable, state it explicitly as a blocker, exactly as the Phase 2 report did — do not claim success you did not execute.)

## Engineering rules
- Minimal, focused diffs; follow existing repo conventions over any generic defaults.
- Do not touch files unrelated to Phase 3.
- Before editing any file, identify: affected files, dependencies, risks, acceptance criteria, and the verification command that will prove the change works.
- Do not commit unless explicitly asked.

## Reporting
When done, report:
1. Files changed (grouped: production / tests / docs).
2. Test results from the four commands above (paste the pass/fail counts).
3. A per-workflow status table (Purchasing, Contacts, Outstanding/Payments, Expenses) with test evidence.
4. The migration result for `011_AddCustomerTypeToCustomer` and the updated migration-test outcome.
5. Any production bugs found and fixed (with test evidence).
6. Link to the new parity doc and updated parity matrix.
7. Any remaining risks/blockers (e.g., DevExpress/print, Docker-unavailable integration tests, deferred import dialogs).

### PROMPT END