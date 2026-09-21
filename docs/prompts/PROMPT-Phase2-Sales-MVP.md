# Prompt — Complete Phase 2: Sales MVP Hardening

> **How to use this file:** Copy everything between the `### PROMPT START` and `### PROMPT END` fences and paste it to the AI coding agent (Claude/opencode/other). The prompt is self-contained and references the repository as the single source of truth. Do not paste the commentary above the fence.

---

### PROMPT START

You are working in the repository **`J:\Project 1\MMNext POS`** (working dir). This is a .NET 8 WinForms POS application named **MMNext POS** (solution `MMNextPOS.slnx`), being rebuilt to functional parity with a legacy FusionPOS system.

## Your mission
Complete **Phase 2 — Sales MVP Hardening** as defined in `plan.md` (section 4, "Phase 2 — Sales MVP Hardening") and the sprint plan `Sprint2_P1_Plan.md`. The goal is that **a cashier can complete, hold, resume, print, return, and void a sale through the UI with correct stock, payment, audit, and rollback behavior.**

Read `plan.md`, `Sprint2_P1_Plan.md`, `docs/sprints/SPRINT-1-SALES-HARDENING.md`, and `docs/parity/PARITY-MATRIX.md` first. They are the authoritative requirements and status sources.

## Repository architecture (follow these conventions strictly)
- **Layered solution:** `src/MMNextPOS.Domain` → `src/MMNextPOS.Infrastructure` → `src/MMNextPOS.Application` → `src/MMNextPOS.WinForms`; tests under `tests/`.
- Target framework `net8.0-windows` (WinForms), nullable reference types enabled, warnings-as-errors.
- Data access: **parameterized Dapper queries** only; repositories implement `I*Repository` + `GenericRepository<T>`; async database access.
- Business logic lives in Application-layer services using constructor-injected repositories (registered in `src/MMNextPOS.Application/DependencyInjection.cs`).
- **Transactions:** every multi-write operation (sale, return, void, payment) must run inside a single `IUnitOfWork` transaction so stock/payment/audit/outstanding updates commit or roll back together.
- **Audit:** every write must produce an audit record (see `ChangeDateLog` / `IAuditService`).
- **UI:** WinForms with DevExpress; follow the existing list-page/form patterns (`AsyncFormBase`, cancellation, progress, confirmation, error handling, proper `Dispose`). Do not create a parallel architecture.
- **Tests:** Arrange–Act–Assert, xUnit, Moq for unit tests; MySQL/Testcontainers for integration tests.

## Current verified baseline (do not regress)
- Release build succeeds; Application unit tests currently pass: **167 passed / 3 skipped / 0 failed** (`MMNextPOS.Application.Tests`).
- Infrastructure migration tests pass against the `000–009` (10-migration) chain.
- Already implemented in `ISalesService`/`SalesService.cs`: `CreateSaleAsync` (atomic stock reservation via `IProductRepository.TryDecrementStockAsync`, duplicate-line aggregation, `MidpointRounding.ToEven` rounding, per-year invoice numbering via `IInvoiceNumberGenerator`), `GetAllAsync` (filtered), `GetSaleDetailsAsync`, `ProcessReturnAsync`, `VoidSaleAsync`, `AggregateDuplicateLines`, `RoundLines`. Stock movements for Purchase/Return/Void are emitted via `IStockMovementService`. Credit sales record `CustomerOutstanding`.
- `NewSaleForm` supports customer lookup, product search, editable line items, totals, save/hold/print, and draft-resume. `SalesListPage` exists (status: Implemented—needs QA).

## Phase 2 scope — what to do

### 1. Close the P1 test gap (highest priority — this is the bulk of the work)
Follow `Sprint2_P1_Plan.md`. The sprint defines **28 P1 test cases** in 4 categories. Several are already added and passing (BOGO, 0.005→0.00, 0.015→0.02 rounding, Outstanding-created, Payment-reduces-balance). Implement **all remaining P1 cases** in `tests/MMNextPOS.Application.Tests/SalesServiceTests.cs` (and supporting service/integration tests where needed):

- **P1-6 Price/Tax/Discount precedence:** percentage discount, fixed-amount discount, tax jurisdiction per customer type, price override (scanner price wins over catalog), boundary fixtures (0.005→0.00, 0.015→0.02, negative prices, >100% discount rejection).
- **P1-7 Currency rounding:** repeated rounding doesn't skew (0.15+0.15+0.15 ≈ 0.45), line-items-then-total (0.33×3 → 0.99 not 1.00), cash-change calculation (15 − 10.99 = 4.01), no floating-point drift.
- **P1-8 Customer Outstanding/AR:** part payment → remaining balance due, overpayment → credit for future sale, customer clearance → zero balance + "Cleared" status + audit record.
- **P1-9 Print receipt/voucher:** DevExpress-dependent. Implement what is runnable in the current environment and **document** the rest with a full test case structure (these may not execute in a headless CI). Cover: receipt all-fields-populated, invoice header/lines/footers, PDF text-selectable export, Myanmar Unicode display, voucher format `EXP-YYYYMMDD-XXXX`, empty-result graceful handling.

Where a test case exposes a real bug in `SalesService` (e.g., rounding rule not followed, outstanding not updated, clearance not implemented), **fix the production code** so the test passes — do not weaken the test.

### 2. Verify the end-to-end cashier workflows
Confirm through unit + integration tests that: new sale, draft/hold/unhold, payment, invoice numbering, print, void/delete, return, and **insufficient-stock** behavior all work with correct stock, payment, audit, and rollback semantics. Add any missing integration coverage under `tests/MMNextPOS.Infrastructure.Tests/SalesServiceIntegrationTests.cs`.

### 3. Verify barcode, duplicate lines, rule precedence, rounding, and credit updates
Add/extend tests for barcode scanner input handling, duplicate-line handling, price/tax/discount precedence, currency rounding, customer credit/outstanding updates, and cancellation-token propagation.

### 4. Permissions and navigation
Verify all sales-related permissions and role-based navigation paths (see `IMainNavigationService`, `MenuRole`). Add unit tests for permission-denied paths.

### 5. Audit and logging safety
Confirm every sale/return/void/payment write produces the required audit record **inside the transaction**, and that sensitive data is never written to logs. Add a redaction test if none exists.

### 6. Parity evidence
Create **`docs/parity/Sales-Hardening-Parity.md`** (referenced in `plan.md` but not yet created). For each completed sales feature record: status (per the taxonomy in `PARITY-MATRIX.md`), test scenario names, acceptance criteria, and sample outputs for the highest-volume receipt and invoice formats. Update `docs/parity/PARITY-MATRIX.md` accordingly (promote `SalesListPage` and `NewSaleForm` entries where evidence exists).

## Exit criteria (Definition of Done)
- A cashier can complete, hold, resume, print, return, and void a sale through the UI with correct stock, payment, audit, and rollback behavior.
- All 28 P1 test cases are either implemented-and-passing OR documented-with-a-runnable-test-structure (DevExpress print cases may be documented-only).
- No regression: existing 167 passing Application tests still pass.
- New tests cover success, validation-failure, rollback, and exception paths.

## Quality gates — you must run all of these and report results before claiming completion
```
dotnet build MMNextPOS.slnx --configuration Release
dotnet format MMNextPOS.slnx --verify-no-changes
dotnet test tests/MMNextPOS.Application.Tests/MMNextPOS.Application.Tests.csproj --configuration Release
dotnet test tests/MMNextPOS.Infrastructure.Tests/MMNextPOS.Infrastructure.Tests.csproj --configuration Release
```
(Integration tests use MySQL/Testcontainers and may need Docker.)

## Engineering rules
- Minimal, focused diffs; follow existing repo conventions over any generic defaults.
- Do not touch files unrelated to Phase 2.
- Before editing any file, identify: affected files, dependencies, risks, acceptance criteria, and the verification command that will prove the change works.
- Do not commit unless explicitly asked.

## Reporting
When done, report:
1. Files changed (grouped: production / tests / docs).
2. Test results from the four commands above (paste the pass/fail counts).
3. The P1 test case status table (implemented-passing vs documented-only).
4. Any production bugs you found and fixed (with test evidence).
5. Link to `docs/parity/Sales-Hardening-Parity.md`.
6. Any remaining risks/blockers (e.g., DevExpress-dependent tests that cannot run headless).

### PROMPT END