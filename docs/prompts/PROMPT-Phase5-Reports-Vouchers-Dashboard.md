# Prompt — Complete Phase 5: Reports, Vouchers, and Dashboard

> **How to use this file:** Copy everything between the `### PROMPT START` and `### PROMPT END` fences and paste it to the AI coding agent. The prompt is self-contained and references the repository as the single source of truth. Do not paste the commentary above the fence.

---

### PROMPT START

You are working in the repository **`J:\Project 1\MMNext POS`** (working dir). This is a .NET 8 WinForms POS application named **MMNext POS** (solution `MMNextPOS.slnx`), being rebuilt to functional parity with a legacy FusionPOS system.

## Your mission
Complete **Phase 5 — Reports, Vouchers, and Dashboard** as defined in `plan.md` (section 4, "Phase 5 — Reports, Vouchers, and Dashboard"). The goal: **critical reports and vouchers are reproducible, printable, exportable, and linked to parity evidence**, with defined parameter contracts, empty-result behavior, date/timezone rules, Myanmar font handling, print settings, and PDF/XLS export behavior.

Read these authoritative documents FIRST:
- `plan.md` — Phase 5 scope (section 4) and engineering rules (section 5).
- `docs/parity/PARITY-MATRIX.md` — status taxonomy; Reports (section 10), Print Vouchers (section 11), Dashboard (section 9). **Note: the matrix is stale for reports** — it says "No XtraReports implemented", but several reports already exist (see baseline below). Audit and correct it rather than trusting it.
- `Legacy-Module-Inventory.md` — the legacy report/voucher reference scope (~96 rpt*.cs, ~100 voucher layouts).
- `docs/parity/Sales-Hardening-Parity.md` — the Phase 2 parity evidence format to replicate.

## Repository architecture (follow these conventions strictly)
- **Layered solution:** `src/MMNextPOS.Domain` → `src/MMNextPOS.Infrastructure` → `src/MMNextPOS.Application` → `src/MMNextPOS.WinForms`; tests under `tests/`.
- Target framework `net8.0-windows`, nullable reference types, warnings-as-errors.
- **DevExpress XtraReports** for report definitions; parameterized queries for data; `IUnitOfWork` transaction boundaries for any write.
- **Myanmar localization is a hard requirement** (risk R9 in plan.md): Unicode/Zawgyi handling, font packaging (e.g., `Myanmar3`), and rendering in grids, print preview, physical print, PDF, and XLS must be verified on clean machines.
- **Tests:** Arrange–Act–Assert, xUnit, Moq; MySQL/Testcontainers for integration tests.

## Current verified baseline (do not regress)
- Release build succeeds; Application unit tests: **264 passed / 0 failed**; full integration suite: **27 passed / 0 failed**.
- Migration chain `000–014` idempotent and version-tracked.
- **Reports already implemented** in `src/MMNextPOS.WinForms/Reports/`: `BaseReport.cs`, `DailySaleSummaryReport.cs`, `SaleInvoiceReport.cs`, `SaleReceiptReport.cs`, `SaleHistoryReport.cs`, `PurchaseInvoiceReport.cs`, `StockListReport.cs`, `StockMovementReport.cs`, `CashFlowReport.cs`, `ProfitLossReport.cs`, `OutstandingReport.cs`, `BarcodeLabelsReport.cs`, `ReportParameterForms.cs`.
- **`IReportService` already exposes:** report-menu CRUD, the five Star reports (CashFlow, ProfitLoss, StockBalance, Reorder, Outstanding — by location + date range), `GenerateReportAsync(reportName, parameters)`, `GenerateSaleReceiptAsync(saleId)`, `GenerateDailySaleSummaryAsync(reportDate)`.
- `ReportServiceTests` exists (unit level).

## Working-environment lessons from Phase 3 (do not repeat the same failures)
1. **Docker/Testcontainers endpoint:** if Docker Desktop is installed but unreachable, set `DOCKER_HOST=npipe://./pipe/dockerDesktopLinuxEngine` (exactly 3 slashes after `npipe:`) before running integration tests.
2. **Format gate:** run `dotnet format MMNextPOS.slnx` (auto-fix) then `--verify-no-changes` before finishing. New files must use CRLF.
3. **Schema drift audit:** run `python tools/schema_drift_audit.py` before finishing; resolve every reported drift.
4. **DevExpress print tests cannot run headless:** implement what is runnable; document the rest with full test case structure (the Phase 2 report precedent). CI will execute them on a Windows runner with the DevExpress feed.
5. **Never log sensitive data** (customer PII, connection strings) — structured logging with redaction (risk R10).

## Phase 5 scope — what to do

### 1. Build the report/voucher inventory (do this FIRST)
From `Legacy-Module-Inventory.md`, inventory all legacy reports (~96) and voucher layouts (~100), group them by operational priority, and record them in a new **`docs/parity/Report-Voucher-Inventory.md`** with columns: legacy name, new implementation (file or `Missing`), priority (P0–P3), status, owner, notes. Mark the already-implemented reports (see baseline) as `Implemented—needs QA` or `Verified` with evidence. This freezes the scope per risk R1 (scope expansion control).

### 2. Implement/harden the highest-priority outputs
Prioritize the business-critical set (P0 first): sale receipts (A4/A5), sale invoices, purchase invoices, stock list, stock movement, cash flow, profit/loss, outstanding, reorder, barcode labels. For each:
- Define the **parameter contract** (parameter names, types, defaults, validation) — follow the existing `ReportParameterForms.cs` pattern.
- Define **empty-result behavior** (no rows → graceful empty report, no crash) and **date/timezone rules** (UTC vs local, date-inclusive ranges).
- Verify data correctness against the actual schema (JOINs populate display fields; the models' `[NotMapped]` display properties like `CustomerName` are populated via lookup).
- Where a report exists only as a spike (e.g., `DailySaleSummaryReport`), harden it to the same standard.

### 3. Export and print verification
- **PDF export:** text-selectable (not rasterized), correct font embedding. **XLS export:** correct structure.
- **Print:** print preview + physical print settings (paper size A4/A5/slip, margins, printer selection).
- **Myanmar Unicode:** representative Unicode/Zawgyi fixtures; verify rendering in grid, print preview, PDF, and XLS; package the required font (e.g., `Myanmar3`) and document the font installation checklist for target machines.
- Tests that require DevExpress/print environment: implement what runs headless (e.g., report construction, data binding, field population, export-to-stream with a DevExpress license) and **document** the rest with full test case structure.

### 4. Dashboard
- `DashboardWidget` entity exists but **no dashboard UI exists** (`ucQuickSummaryMain`, `ucDataView`, `ucFinancialView` all `Missing`).
- Implement a quick-summary dashboard (today's sales, cash flow, outstanding, low-stock alerts) wired into `MainForm` navigation, following the existing list-page/form patterns. Use the existing services (`GetRecentSalesAsync`, `GenerateDailySaleSummaryAsync`, Star report queries) rather than new queries where possible. If the advanced data/financial views are out of scope this phase, record them as `Not in scope` with an owner-approved reason in the parity matrix.

### 5. Report service tests
- Extend `tests/MMNextPOS.Application.Tests/ReportServiceTests.cs`: menu CRUD success/failure, each Star report query (success, empty result, invalid parameters), `GenerateReportAsync` parameter validation, `GenerateSaleReceiptAsync` for existing/missing sale.
- Add integration tests where a report query hits the DB with representative seeded data.

### 6. Parity evidence
Create **`docs/parity/Phase5-Reports-Vouchers-Dashboard-Parity.md`** with the same structure as prior evidence docs: per-report status, test scenario names, acceptance criteria, and **sample outputs for the highest-volume receipt and invoice formats** (PDF/image snapshots). Update `docs/parity/PARITY-MATRIX.md` sections 9/10/11 (correct the stale "No XtraReports implemented" entries; promote to `Verified` only with scenario tests + parity evidence + accepted-difference notes).

## Exit criteria (Definition of Done)
- Critical reports and vouchers are reproducible, printable, exportable, and linked to parity evidence.
- Parameter contracts, empty-result behavior, date/timezone rules, Myanmar font handling, print settings, and PDF/XLS export behavior are defined and documented for every implemented report.
- The report/voucher inventory is frozen with priorities and owners.
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
- Do not touch files unrelated to Phase 5.
- Before editing any file, identify: affected files, dependencies, risks, acceptance criteria, and the verification command that proves the change works.
- Where a test exposes a real bug in production code, fix the production code so the test passes — do not weaken the test.
- Do not commit unless explicitly asked.

## Reporting
When done, report:
1. Files changed (production / tests / docs).
2. Results from the five commands above (paste counts).
3. The report/voucher inventory summary (implemented vs missing by priority).
4. Which print/export behaviors are verified-with-executed-evidence vs documented-only (DevExpress headless limitation).
5. Per-report status table with test evidence.
6. Any production bugs found and fixed (with test evidence).
7. Link to the new parity docs and updated matrix.
8. Remaining risks/blockers (e.g., Myanmar font packaging, DevExpress feed, deferred dashboard views).

### PROMPT END