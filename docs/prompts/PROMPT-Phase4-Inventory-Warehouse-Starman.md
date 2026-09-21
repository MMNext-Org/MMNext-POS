# Prompt — Complete Phase 4: Inventory, Warehouse, and Starman

> **How to use this file:** Copy everything between the `### PROMPT START` and `### PROMPT END` fences and paste it to the AI coding agent. The prompt is self-contained and references the repository as the single source of truth. Do not paste the commentary above the fence.

---

### PROMPT START

You are working in the repository **`J:\Project 1\MMNext POS`** (working dir). This is a .NET 8 WinForms POS application named **MMNext POS** (solution `MMNextPOS.slnx`), being rebuilt to functional parity with a legacy FusionPOS system.

## Your mission
Complete **Phase 4 — Inventory, Warehouse, and Starman** as defined in `plan.md` (section 4, "Phase 4 — Inventory, Warehouse, and Starman"). The goal: **stock balances reconcile after representative sequences of purchases, sales, returns, adjustments, transfers, assemblies, and reversals**, and movements that affect available stock are atomic across header, detail, serial, and audit records.

Read these authoritative documents FIRST:
- `plan.md` — Phase 4 scope (section 4) and engineering rules (section 5).
- `docs/parity/PARITY-MATRIX.md` — status taxonomy and current module statuses (Inventory, Warehouse/Stock Transfer, Starman sections).
- `docs/parity/Sales-Hardening-Parity.md` — the Phase 2 parity evidence format to replicate.
- `docs/parity/Phase3-Purchasing-Contacts-Payments-Expenses-Parity.md` — the Phase 3 evidence (if present).

## Repository architecture (follow these conventions strictly)
- **Layered solution:** `src/MMNextPOS.Domain` → `src/MMNextPOS.Infrastructure` → `src/MMNextPOS.Application` → `src/MMNextPOS.WinForms`; tests under `tests/`.
- Target framework `net8.0-windows`, nullable reference types, warnings-as-errors.
- Data access: **parameterized Dapper queries** only; async; `IUnitOfWork` transaction boundaries.
- **`GenericRepository<T>` builds INSERT/UPDATE column lists by reflection from model properties** — every model column must exist in the DB table, and computed properties must be `[NotMapped]`. Violations surface as `Unknown column 'X' in 'field list'` MySQL errors.
- **Audit:** every write must produce an audit record inside the transaction.
- **Tests:** Arrange–Act–Assert, xUnit, Moq for unit tests; MySQL/Testcontainers for integration tests.

## Current verified baseline (do not regress)
- Release build succeeds; Application unit tests: **264 passed / 0 failed**; full integration suite: **27 passed / 0 failed** (with Docker configured — see below).
- Migration chain `000–014` (15 migrations) is idempotent and version-tracked; `014_AlignSchemaWithModelPhase4.sql` aligns StockMovements, StockMovementDetails, Products, Payments, Expenses, and StockTransfers with their models.
- `IStockMovementService` already implements **15 movement types**: Sale, Purchase, Return, Void, Issue, Receive, Damaged, Lost, Adjustment, TransferOut, TransferIn, Assembly, Deassembly, Expired, CycleCount. `AssemblyService`, `ExpiryManagementService`, `SerialNumberService` (+ interfaces) exist. `StockMovementsListPage`, `StockTransfersListPage`, `AssembliesListPage` exist. `StockTransferServiceTests` and `SerialNumberServiceTests` exist (unit level).

## Working-environment lessons from Phase 3 (do not repeat the same failures)
1. **Docker/Testcontainers endpoint:** if Docker Desktop is installed but the daemon reports unreachable, set `DOCKER_HOST=npipe://./pipe/dockerDesktopLinuxEngine` (exactly 3 slashes after `npipe:`) before running integration tests. Slash-4 format fails Testcontainers parsing.
2. **Slow integration suite:** each test resets the DB and re-runs all migrations; migration 007 alone runs 282 statements. Budget time, use test filters, and do not abort partial runs that are still making progress.
3. **Format gate:** run `dotnet format MMNextPOS.slnx` (auto-fix) then `dotnet format MMNextPOS.slnx --verify-no-changes` before finishing. New files must use CRLF line endings.
4. **Schema drift audit:** before finishing, run `python tools/schema_drift_audit.py` and resolve every reported drift (it compares model properties against migration-created tables). Model properties missing from tables = future `Unknown column` failures.

## Phase 4 scope — what to do

### 0. Prerequisite — create the missing Serial* tables (do this FIRST)
`SerialNumber`/`SerialBatch`/`SerialTracking` entities and repositories/services are registered in DI, but **no migration creates the `SerialNumbers`, `SerialBatches`, or `SerialTrackings` tables** — any write to them will fail with "Table doesn't exist".
- Add migration `015_AddSerialTrackingTables.sql` creating all three tables (idempotent `CREATE TABLE IF NOT EXISTS`, matching `001_InitialSchema.sql` conventions: InnoDB, FKs to Products/Locations where applicable, EntityBase audit columns `CreatedAt/UpdatedAt/CreatedBy/UpdatedBy/IsDeleted`).
- Register version `"015"` in `src/MMNextPOS.Infrastructure/MigrationRunner.cs` (`LoadMigrations()`).
- Update `tests/MMNextPOS.Infrastructure.Tests/MigrationIdempotenceTests.cs` (version `"015"`, 16 migrations 000–015) — and add a structural test asserting the three tables exist after a full run (follow the `MigrationRunner_InvoiceSequencesTable_Exists_AfterFullRun` pattern).
- Run the migration idempotence suite to verify.

### 1. Movement-type tests (the bulk of the work)
`IStockMovementService` has 15 movement methods but **no dedicated unit test suite**. Create `tests/MMNextPOS.Application.Tests/StockMovementServiceTests.cs` covering, for each movement type that affects available stock (Issue, Receive, Damaged, Lost, Expired, Adjustment, TransferOut, TransferIn, Assembly, Deassembly, CycleCount):
- Success path: movement header + detail + stock effect + audit record written.
- Validation failure: negative/zero quantity, unknown product/location, invalid reason code → `ValidationException` (or the repo's exception type), **nothing written**.
- Rollback: failure after partial write rolls back header/detail/stock/audit together.
- Direction correctness: Issue/Damaged/Lost/Expired/TransferOut **decrement** available stock; Receive/Adjustment(+)/TransferIn **increment** it.

Add integration tests in `tests/MMNextPOS.Infrastructure.Tests/` (e.g., `StockMovementServiceIntegrationTests.cs`) exercising the atomic paths against real MySQL: issue more than available → fails with zero side effects; receive updates stock exactly for received quantities; assembly/deassembly reconcile component ↔ assembled product stock.

### 2. Serial and batch tracking
With the Serial* tables in place: serial number generation/registration, batch tracking, serial status transitions, expiry via `ExpiryManagementService`. Ensure writes to Serial* tables are transactional with the movement that references them, and add unit + integration tests.

### 3. Stock transfers and Starman
- Verify `StockTransferService` workflows: create transfer, transfer out/in (atomic decrement at source, increment at destination or on acceptance), release/cancel with audit (`CancelReason`, `ReleasedByUserId` columns now exist via migration 014).
- Starman: `StarStockTransferReceived` (remote warehouse acceptance), `StarSalePriceTransfer` (sale-price transfers), and the five Star report repos (`GetCashFlowReportAsync`, `GetProfitLossReportAsync`, `GetStockBalanceReportAsync`, `GetReorderReportAsync`, `GetOutstandingReportAsync` in `IReportService`). Verify their query correctness against the actual schema (these Star tables exist in migration 001/005) and add service tests.
- Verify the Star* ListPages/remote acceptance UI paths exist or document them as deferred in the parity matrix.

### 4. Linked stock and stock reconciliation
- Verify `LinkedStock` workflows (linked product stock behavior).
- Build a **stock reconciliation test**: seed a known stock level, run a representative sequence (purchase → sale → return → adjust → transfer → assembly), and assert the final balance equals the expected ledger arithmetic. This is the Phase 4 exit criterion.

### 5. Parity evidence
Create **`docs/parity/Phase4-Inventory-Warehouse-Starman-Parity.md`** with the same structure as the Phase 2/3 evidence docs: per-workflow status (per the taxonomy), test scenario names, acceptance criteria, sample outputs where applicable. Update `docs/parity/PARITY-MATRIX.md` for Inventory, Warehouse/Stock Transfer, and Starman sections (promote entries where evidence exists; the matrix is currently stale — e.g., "Issue/Receive/Damaged/Lost/Adjust Partial" and "Barcode Missing" predate the current implementation).

## Exit criteria (Definition of Done)
- Stock balances reconcile after representative sequences of purchases, sales, returns, adjustments, transfers, assemblies, and reversals.
- Movements that affect available stock are atomic across header, detail, serial, and audit records (failure-injection tests prove rollback).
- Serial* tables exist, are migration-tracked, and their writes are transactional.
- Every Phase 4 workflow has service tests, repository/integration tests, UI smoke scenarios (or documented deferral), permission checks, and documented validation behavior.
- No regression: existing 264 unit tests and 27 integration tests still pass.

## Quality gates — run all of these and report results before claiming completion
```
python tools/schema_drift_audit.py
dotnet build MMNextPOS.slnx --configuration Release
dotnet format MMNextPOS.slnx --verify-no-changes
dotnet test tests/MMNextPOS.Application.Tests/MMNextPOS.Application.Tests.csproj --configuration Release
dotnet test tests/MMNextPOS.Infrastructure.Tests/MMNextPOS.Infrastructure.Tests.csproj --configuration Release
```
(Integration tests need Docker; set `DOCKER_HOST=npipe://./pipe/dockerDesktopLinuxEngine` if the daemon is unreachable. If Docker is truly unavailable, state it explicitly as a blocker — do not claim success you did not execute.)

## Engineering rules
- Minimal, focused diffs; follow existing repo conventions over generic defaults.
- Do not touch files unrelated to Phase 4.
- Before editing any file, identify: affected files, dependencies, risks, acceptance criteria, and the verification command that proves the change works.
- Where a test exposes a real bug in production code, fix the production code so the test passes — do not weaken the test.
- Do not commit unless explicitly asked.

## Reporting
When done, report:
1. Files changed (production / tests / docs).
2. Results from the five commands above (paste counts).
3. The schema-drift audit result (before/after).
4. Per-workflow status table (movements, serial/batch, transfers, Starman, linked stock) with test evidence.
5. Any production bugs found and fixed (with test evidence).
6. Link to the new parity doc and updated matrix.
7. Remaining risks/blockers.

### PROMPT END