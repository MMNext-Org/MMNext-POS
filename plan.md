# MMNext POS — Verified Implementation Roadmap

> **Planning basis:** This roadmap was revised after comparing the original Discovery Report with the current repository structure and implementation. It replaces assumptions about an empty scaffold with a status-driven plan focused on verification, missing workflows, parity, and release readiness.

**Target:** Deliver a maintainable .NET 8 WinForms POS that reaches the agreed level of functional parity with FusionPOS without sacrificing build reliability, testability, or data safety.

**Current review status:** Architecture and a substantial portion of the application surface already exist. The next sprint must therefore begin with a build/test baseline and a feature-gap audit rather than recreating the foundation.

---

## 1. Verified Current State

| Area | Verified state | Planning consequence |
|---|---|---|
| Solution | `MMNextPOS.slnx` contains four source projects and two test projects. | Do not create a new solution; stabilize and extend the existing one. |
| Target framework | WinForms targets `net8.0-windows`; nullable reference types and warnings-as-errors are enabled. | Keep .NET 8 as the baseline. Do not move CI to .NET 10 preview as an immediate task. |
| Architecture | Domain, Infrastructure, Application, and WinForms layers are present. | Preserve the existing boundaries and remove only confirmed violations. |
| Dependency injection | `DependencyInjection.cs` registers a broad set of repositories and services covering sales, purchasing, inventory, stock transfer, reports, licensing, settings, receipts, backup, migration, and SuperAdmin. | The old “most services still to build” gap is outdated. Audit behavior and completeness instead. |
| Database | `DatabaseInitializer` already creates a substantial set of master, sales, inventory, and warehouse tables. | Focus on schema-to-repository consistency, missing tables/columns, indexes, migrations, and idempotence. |
| Repository layer | `GenericRepository` and many domain repositories exist, with paging and soft-delete support. | Validate query correctness, transaction participation, paging semantics, and SQL safety before adding more abstractions. |
| Application layer | Services exist for core sales, customers, products, masters, purchases, returns, inventory, settings, license, reports, receipts, backup, migration, and SuperAdmin areas. | Prioritize business-rule coverage, error handling, audit logging, and missing edge cases. |
| Presentation layer | `MainForm` already wires many list pages and functional areas, including sales, customers, suppliers, outstanding, purchases, inventory, stock transfers, expenses, payments, settings, reports, backups, migrations, and SuperAdmin. | Replace the old “sales + catalog placeholder” description with a UI-completeness and usability audit. |
| Sales entry | `NewSaleForm` supports customer lookup, product search, editable line items, totals, save/hold/print, and draft-resume behavior. | Harden barcode, pricing, stock, discount/tax, payment, cancellation, and print flows rather than rebuilding the form. |
| Testing | Application unit tests and MySQL/Testcontainers integration tests exist. | Expand coverage and reliability; do not describe testing as unstarted. |
| CI/CD | CI runs on Windows with .NET 8, restore, Release build, format verification, application tests, and MySQL integration tests. | Make the existing pipeline consistently green and add missing quality gates incrementally. |

### Evidence used for this revision

The status above is grounded in the following repository files: [DependencyInjection.cs](src/MMNextPOS.Application/DependencyInjection.cs), [MainForm.cs](src/MMNextPOS.WinForms/MainForm.cs), [NewSaleForm.cs](src/MMNextPOS.WinForms/NewSaleForm.cs), [DatabaseInitializer.cs](src/MMNextPOS.Infrastructure/DatabaseInitializer.cs), [GenericRepository.cs](src/MMNextPOS.Infrastructure/Repositories/GenericRepository.cs), [MMNextPOS.slnx](MMNextPOS.slnx), [ci.yml](.github/workflows/ci.yml), and the test projects under `tests/`.

---

## 2. Scope and Parity Inventory

The legacy inventory remains the reference scope: Sales, Contacts, Inventory, Purchases, Outstanding/Payments, Expenses, Warehouse/Stock Transfer, Starman multi-site workflows, Dashboard, Reports, Print Vouchers, Settings, SuperAdmin, License, Menus, API Controller, and database utilities.

The inventory must now be converted into a **traceable parity matrix**. Each legacy screen, menu item, report, voucher, field, validation rule, and permission should have one of these statuses:

| Status | Meaning |
|---|---|
| `Verified` | New implementation exists and has been tested against the intended behavior. |
| `Implemented—needs QA` | Code exists but lacks documented scenario coverage or parity evidence. |
| `Partial` | Main path exists, but one or more important workflows, validations, permissions, or print/export paths are missing. |
| `Missing` | No usable replacement exists. |
| `Not in scope` | Explicitly excluded and recorded with an owner-approved reason. |

The parity matrix should be maintained under `docs/parity/` and linked from this plan. It must not rely only on file names because a class or form can exist while its business behavior remains incomplete.

---

## 3. Corrected Gap Matrix

| Capability | Current position | Remaining work | Priority |
|---|---|---|---|
| Domain models | Core entities and `EntityBase` are present. | Compare all required legacy fields and value objects against actual schema and workflows; remove ambiguous or duplicate models. | High |
| Database schema | Broad initializer coverage exists. | Produce a schema inventory, add missing indexes/constraints, verify idempotent upgrades, and create a legacy-to-new migration map. | Critical |
| Repositories | Generic and many specialized repositories exist. | Test every registered repository, standardize cancellation/paging/transactions, and review parameterization and soft-delete behavior. | Critical |
| Application services | Broad service registration exists. | Complete business-rule tests for stock, returns, payments, invoice numbering, audit logs, and failure rollback. | Critical |
| WinForms UI | Main shell and numerous list pages/forms exist. | Verify navigation permissions, disposal, loading/error states, edit dialogs, keyboard/barcode flows, and screen-by-screen parity. | High |
| Reports and vouchers | Report service, report viewer, and receipt/voucher repositories exist. | Inventory all legacy reports/vouchers, prioritize the business-critical set, then validate output with PDF/image comparisons. | High |
| Licensing | License repositories/services and startup guard infrastructure exist. | Test registration, device binding, expiry, offline behavior, failure messaging, and upgrade compatibility. | High |
| Observability | Serilog and health-check infrastructure are configured. | Define structured event fields, correlation identifiers, log retention, sensitive-data redaction, and operational runbooks. | Medium |
| Tests | Unit and integration foundations exist. | Add module suites, edge-case tests, migration tests, UI smoke coverage, and a measured coverage threshold. | Critical |
| Delivery | CI and tagged self-contained release workflow exist. | Verify release reproducibility, DevExpress licensing prerequisites, installer strategy, migration packaging, and rollback instructions. | High |

---

## 4. Immediate Execution Plan

### Phase 0 — Baseline and Evidence Lock (first sprint)

This replaces the previous “create the foundation” step because the foundation is already present.

1. Run `dotnet restore MMNextPOS.slnx` and `dotnet build MMNextPOS.slnx --configuration Release` on a clean Windows environment.
2. Run format verification, application unit tests, and infrastructure integration tests using the repository’s CI commands.
3. Record every failure with the project, file, test name, root cause, and proposed owner in `docs/baseline/BUILD-TEST-BASELINE.md`.
4. Confirm the required DevExpress package feed/license setup and document any machine-specific prerequisite.
5. Create or verify `.gitignore`, inspect the working tree, and preserve unrelated user changes. Do not create a first commit automatically.
6. Establish the parity matrix from the legacy module inventory before starting feature work.

**Exit criteria:** Release build succeeds, all currently expected tests pass or have an approved defect record, CI reproduces the local result, and the parity matrix has owners and priorities.

### Phase 1 — Transaction and Data Safety

1. Review `DatabaseInitializer` against every repository query and model field.
2. Add missing foreign keys, uniqueness rules, indexes, timestamps, soft-delete rules, and transaction boundaries where required.
3. Verify `IUnitOfWork` connection and transaction reuse across sale, purchase, return, stock movement, payment, and transfer workflows.
4. Add migration/version tracking so schema upgrades are explicit and repeatable instead of relying only on `CREATE TABLE IF NOT EXISTS`.
5. Build a read-only legacy schema inspection and mapping report before implementing data import.

**Exit criteria:** Schema initialization is idempotent, transactional workflows roll back correctly, and a representative legacy dataset can be mapped without data loss assumptions.

### Phase 2 — Sales MVP Hardening

1. Complete end-to-end tests for new sale, draft/hold/unhold, payment, invoice numbering, print, void/delete, return, and insufficient-stock behavior.
2. Verify barcode scanner input, duplicate-line handling, price/tax/discount precedence, currency rounding, customer credit/outstanding updates, and cancellation tokens.
3. Verify all sales-related permissions and role-based navigation paths.
4. Confirm that every write produces the required audit record and that sensitive data is not written to logs.
5. Produce a parity checklist and sample outputs for the highest-volume receipt and invoice formats.

**Exit criteria:** A cashier can complete, hold, resume, print, return, and void a sale through the UI with correct stock, payment, audit, and rollback behavior.

### Phase 3 — Purchasing, Contacts, Payments, and Expenses

Implement and test supplier/customer management, imports, purchases, purchase returns, customer/supplier outstanding, payments, expense types, expense entry, and monthly summaries. The work must reuse the existing services and list-page patterns rather than creating a parallel architecture.

**Exit criteria:** Each workflow has service tests, repository integration tests, UI smoke scenarios, permission checks, and documented validation behavior.

### Phase 4 — Inventory, Warehouse, and Starman

Complete stock entry, issue, receive, adjust, damaged, lost, expired, serial tracking, assembly/deassembly, linked stock, stock transfers, remote warehouse acceptance, sale-price transfers, and Starman reports. Prioritize movements that affect available stock and enforce atomicity across header, detail, serial, and audit records.

**Exit criteria:** Stock balances reconcile after representative sequences of purchases, sales, returns, adjustments, transfers, assemblies, and reversals.

### Phase 5 — Reports, Vouchers, and Dashboard

1. Build the report/voucher inventory from the legacy source and group it by operational priority.
2. Implement the highest-priority sale, purchase, inventory, financial, outstanding, dashboard, receipt, barcode, and voucher outputs first.
3. Define parameter contracts, empty-result behavior, date/timezone rules, Myanmar font handling, print settings, and PDF/XLS export behavior.
4. Compare representative outputs against legacy PDFs/images and record accepted differences.

**Exit criteria:** Critical reports and vouchers are reproducible, printable, exportable, and linked to parity evidence.

### Phase 6 — Administration, Licensing, Backup, and Migration

Complete settings, company configuration, themes, languages, Myanmar font conversion, user/role/menu management, license/device binding, backup/restore, data migration, SuperAdmin logs, and controlled script execution. Destructive operations require explicit confirmation, authorization, audit logging, and a recovery procedure.

**Exit criteria:** A fresh installation can be configured, licensed, backed up, migrated from an approved legacy sample, and recovered using documented procedures.

### Phase 7 — Parity QA and Release

Run the full menu/field/report/voucher checklist, performance tests with realistic data volumes, memory/disposal review for long-running WinForms sessions, accessibility and keyboard checks, Myanmar Unicode/Zawgyi verification, security review, and release-candidate migration rehearsal.

**Exit criteria:** Release candidate passes build, tests, smoke checks, migration rehearsal, output comparisons, and sign-off against the parity matrix.

---

## 5. Quality Gates and Engineering Rules

| Gate | Required evidence |
|---|---|
| Build gate | `dotnet build MMNextPOS.slnx --configuration Release` succeeds with no unexpected warnings. |
| Format gate | `dotnet format --verify-no-changes` succeeds. |
| Unit-test gate | Application service tests cover success, validation failure, rollback, authorization, and exception paths. |
| Integration-test gate | MySQL/Testcontainers tests verify schema initialization, transactions, repository queries, paging, and representative migrations. |
| UI gate | Core forms do not block the UI thread, support cancellation, show actionable errors, and dispose controls/resources correctly. |
| Data gate | Write workflows are transactional and auditable; SQL is parameterized; sensitive values are redacted from logs. |
| Parity gate | Each completed legacy feature has a status, test scenario, evidence artifact, and accepted-difference note where applicable. |
| Release gate | Self-contained publish, installer/migration artifacts, configuration instructions, rollback steps, and release notes are reproducible. |

All new code must follow the repository’s layered architecture, nullable-reference conventions, async database access, dependency-injection pattern, parameterized Dapper queries, and Arrange–Act–Assert test style. Every implementation task must identify affected files, dependencies, risks, acceptance criteria, and verification commands before editing.

---

## 6. Detailed Risk Register and Mitigation Plans

Risk management is an active delivery process, not a one-time checklist. The project manager should review this register at least once per sprint, update probability and impact after each baseline or release rehearsal, and create a tracked issue whenever a trigger is observed. Risk owners are accountable for prevention and early escalation; the delivery lead is accountable for accepting residual risk.

### 6.1 Risk Scoring and Escalation

| Score | Interpretation | Required action |
|---|---|---|
| 1–3 | Low | Track in the sprint risk log and verify at the next review. |
| 4–6 | Medium | Assign an owner, mitigation due date, and evidence artifact. Review weekly. |
| 7–9 | High | Escalate to the delivery lead within one business day; add a contingency task and gate release if unresolved. |
| 10–16 | Critical | Stop the affected release or migration activity until an approved resolution, rollback, or risk acceptance exists. |

Risk score is calculated as **Probability (1–4) × Impact (1–4)**. The score is a prioritization aid, not a substitute for judgment; data loss, security exposure, and regulatory concerns should be escalated even when their numerical score is low.

### 6.2 Detailed Risk Register

| ID / Risk | Trigger and early warning | Preventive mitigation plan | Contingency / response plan | Owner | Verification evidence |
|---|---|---|---|---|---|
| R1 — Legacy parity scope expands indefinitely | New legacy screens, fields, reports, or exceptions are discovered after a phase has started; “small” requests do not have an owner or acceptance test. | Freeze the approved legacy inventory at the start of each release; maintain `docs/parity/` with status, priority, owner, dependencies, and accepted exclusions. Require change-control approval for additions that affect schedule or architecture. Prioritize cashier, stock, payment, reporting, and migration-critical behavior first. | Place unapproved additions in a deferred backlog; re-estimate the affected phase; obtain explicit stakeholder approval before changing the release target. Do not silently expand an active sprint. | Product Owner / Delivery Lead | Approved parity matrix, change log, phase exit checklist, and signed scope decisions. |
| R2 — Build or CI failure blocks delivery | Clean restore/build differs from developer results; warnings-as-errors fail; DevExpress packages cannot be restored; CI and local test results diverge. | Keep .NET 8 as the single supported baseline; run the exact CI commands locally; pin or document package sources; maintain a clean Windows build runner; treat build and format checks as merge gates. Record environment prerequisites and diagnostic commands in `docs/baseline/`. | Freeze feature merges to the affected projects; reproduce on a clean runner; roll back the last dependency/configuration change; use a short-lived compatibility branch only with an expiry date and owner. | Build/CI Owner | Green `dotnet build`, `dotnet format --verify-no-changes`, CI run, package-feed checklist, and recorded environment versions. |
| R3 — Schema drift or partial migration corrupts data | Model fields do not match initializer/repository queries; `CREATE TABLE IF NOT EXISTS` leaves old columns unchanged; migration produces row-count or balance differences; startup initialization is non-idempotent. | Introduce explicit schema versioning and ordered migrations; compare models, queries, and tables in a schema inventory; add keys, indexes, constraints, and migration tests; run migrations only against a backup or staging copy first. Require reconciliation reports for row counts, totals, stock, payments, and invoice numbers. | Stop the migration; preserve logs and the source database; restore the target from backup; run read-only diagnostics; repair the migration script and repeat in staging before retrying production. Never “fix” mismatches with ad-hoc manual deletes. | Data Migration Owner / DBA | Repeated initializer run without drift, migration test results, backup/restore rehearsal, and signed reconciliation report. |
| R4 — Transaction rollback leaves stock, payment, or audit data inconsistent | A failure occurs between header/detail/stock/payment/audit writes; retry creates duplicate invoice numbers or double stock movement; integration tests pass only on the happy path. | Use one Unit of Work and transaction boundary for each business operation; define idempotency keys for retried operations; test commit, rollback, timeout, cancellation, and repository exceptions; enforce unique invoice/reference constraints; write audit records within the same transaction when appropriate. | Mark the operation as failed and prevent retry until status is known; run a reconciliation job for stock/payment/audit records; reverse through an approved compensating transaction; escalate unreconciled balances before reopening the workflow. | Application Lead / Finance SME | Failure-injection tests, duplicate-retry tests, stock/payment reconciliation, and audit trail review. |
| R5 — Core sales workflow is functionally incomplete | Cashier cannot complete hold/unhold, return, void, payment, barcode, tax/discount, print, or insufficient-stock scenarios; user acceptance finds rules not covered by unit tests. | Define a sales scenario catalog with inputs, expected stock/payment/audit/print results, and roles; cover service, repository, and UI smoke tests; validate rounding and rule precedence with boundary fixtures; test cancellation and duplicate scans. | Keep the release in internal testing; enable only verified sales paths; document known limitations; prioritize defects by financial or stock impact and do not accept manual workarounds for incorrect totals. | Sales Product Owner / Application Lead | End-to-end scenario results, sample receipts/invoices, permission checks, and stock/payment reconciliation. |
| R6 — DevExpress licensing or package-feed issue affects build/release | Local build works only on one workstation; CI cannot resolve packages or license; evaluation notices appear in production artifacts. | Document the approved feed, credentials handling, license files, and CI secret names; validate restore/build/publish on a clean Windows runner; separate development, CI, staging, and production prerequisites; include license verification in release readiness. | Stop publishing; use the last known-good artifact if permitted; contact the licensed vendor/administrator; remove unlicensed artifacts from distribution; record the dependency incident before retrying. | Release Engineer / Platform Owner | Clean-runner restore, Release publish log, license checklist, and artifact inspection. |
| R7 — MainForm and UI composition become difficult to maintain | Constructor dependency count grows; navigation registration and page creation duplicate logic; forms block the UI, fail to dispose, or show inconsistent error states. | Keep navigation composition in a dedicated service/factory; use scoped page creation; standardize `AsyncFormBase`, cancellation, progress, confirmation, and error handling; introduce UI smoke tests for navigation and repeated open/close cycles; refactor only behind tests. | Isolate the affected module behind a feature flag or navigation restriction; stop adding dependencies to `MainForm`; create a focused refactor task with acceptance tests before further expansion. | WinForms Lead | Dependency review, navigation smoke test, UI responsiveness check, and dispose/reopen test evidence. |
| R8 — Long-running sessions leak controls, handlers, connections, or memory | Memory rises after repeated navigation; disposed forms still receive events; MySQL connections remain open; users report degradation after a full shift. | Enforce `Dispose(bool)` and cancellation-source cleanup; open DB connections per operation; unsubscribe handlers; run repeated open/edit/close scenarios; capture baseline memory observations and review high-risk DevExpress controls. | Restart the affected session safely; disable the leaking screen if necessary; capture a diagnostic trace; fix disposal at the smallest owning component; add a regression test before re-enabling broad use. | WinForms Lead / QA Lead | Repeated-session test log, connection-lifetime review, disposal checklist, and before/after memory observations. |
| R9 — Myanmar Unicode/Zawgyi, fonts, printing, or export render incorrectly | Text appears broken in grids, reports, receipts, PDF/XLS export, or installed machines; search/sort behavior differs by encoding. | Define representative Unicode/Zawgyi fixtures; standardize conversion boundaries; package and verify required fonts; test grid, print preview, physical print, PDF, and XLS outputs on clean machines; keep localization acceptance in every report/voucher phase. | Preserve the original value where safe; route affected output to a verified template; block release of the affected report/print path; correct conversion rules and re-run the fixture suite. | Localization Owner / Reports Lead | Fixture comparison pack, font installation checklist, print/PDF/XLS samples, and stakeholder visual sign-off. |
| R10 — Sensitive data or credentials appear in logs or advisory diagnostics | Connection strings, passwords, customer data, tokens, or full database dumps appear in logs, bug reports, prompts, or uploaded artifacts. | Apply structured logging with redaction; prohibit secrets and raw customer data in diagnostics; use synthetic data and minimal excerpts; review Serilog sinks and retention; add a pre-release security checklist and secret scanning. | Revoke/rotate exposed credentials immediately; quarantine and delete the artifact where possible; assess scope; notify the security owner; add a regression test or logging rule before resuming. | Security Owner / Platform Owner | Redaction tests, secret scan result, sink configuration review, retention policy, and incident record if applicable. |
| R11 — Backup, restore, or migration recovery fails when needed | Backup completes without a restorable artifact; restore has missing tables/files; recovery time exceeds the business tolerance; migration rehearsal is skipped. | Define RPO/RTO targets with stakeholders; schedule encrypted backups; verify checksums and restore into an isolated environment; rehearse legacy-to-new migration and rollback; document operator runbooks with named owners. | Stop destructive migration; restore the latest verified backup; switch to the approved fallback process; record data loss window and recovery time; run a post-incident reconciliation before reopening. | Operations Owner / DBA | Successful restore rehearsal, checksum log, RPO/RTO measurement, migration rollback test, and signed runbook. |
| R12 — Test coverage gives false confidence | Only happy paths are covered; integration tests depend on local state; tests are flaky or skipped; UI and report behavior is untested. | Maintain a test pyramid: service unit tests, repository/Testcontainers tests, targeted UI smoke tests, migration tests, and report/voucher comparisons; make CI fail on unexplained test failures; quarantine flaky tests with an owner and deadline. | Block the affected milestone; reproduce in a clean environment; fix the test or implementation rather than increasing retries; add the missing scenario to the parity matrix and regression suite. | QA Lead / Module Owner | CI history, coverage trend, flake register, failure-path tests, and parity evidence artifacts. |
| R13 — Performance degrades with realistic data volumes | Grid paging/filtering slows with 10k+ records; report queries time out; UI thread is blocked; database CPU/lock time rises. | Establish performance budgets for key screens and reports; require server-side paging/filtering; add indexes based on query plans; use cancellation tokens and bounded result sets; test with representative data before release. | Disable or limit expensive filters/reports; use a safe paging default; capture query plans and timings; optimize the highest-impact query before re-opening the gate. | Infrastructure Lead / QA Lead | Timed performance suite, query plans, representative dataset results, and responsiveness measurements. |
| R14 — Role, license, or destructive-operation authorization is bypassed | A user can open a restricted menu or call a service directly; expired licenses still permit work; backup/restore/script execution lacks confirmation or audit. | Enforce authorization in both navigation and application services; centralize license guard checks; require explicit confirmation and audit records for destructive actions; test permitted, denied, expired, and offline states. | Disable the affected operation or role; revoke sessions/credentials if necessary; repair server-side checks; review audit history and notify the security/business owner before re-enabling. | Security Owner / Application Lead | Authorization matrix, negative tests, license-state tests, audit-log samples, and role-based UI smoke tests. |
| R15 — Release artifact or installer is not reproducible | Tagged build differs from local build; self-contained publish omits configuration or migration files; installer works only on developer machines. | Build from clean checkout; pin release inputs; verify self-contained publish and installer in an isolated Windows VM; include configuration, prerequisites, migration CLI, rollback instructions, and release notes; retain checksums. | Do not promote the artifact; revert to the last verified release; fix packaging in a release branch; repeat installation, upgrade, uninstall, and rollback tests. | Release Engineer | Clean-checkout publish log, install/upgrade test, artifact checksum, smoke test, and release checklist. |

### 6.3 Risk Review Cadence and Required Artifacts

At sprint planning, each active risk must have a current score, owner, target mitigation date, and next action. At sprint review, the owner must provide evidence or explicitly report that the risk remains open. Before each milestone exit, the delivery lead must review all High and Critical risks and record whether each is **mitigated**, **accepted**, **transferred**, or **blocking**.

The minimum risk-management artifacts are `docs/risks/RISK-REGISTER.md`, `docs/baseline/BUILD-TEST-BASELINE.md`, `docs/parity/`, migration/reconciliation reports, backup/restore evidence, and release checklists. The team operating procedure is documented in [CRITICAL-RISK-IMPLEMENTATION-GUIDE.md](docs/CRITICAL-RISK-IMPLEMENTATION-GUIDE.md), and the migration/rollback decision flow is available as [MIGRATION-ROLLBACK-INCIDENT-FLOWCHART.mmd](docs/MIGRATION-ROLLBACK-INCIDENT-FLOWCHART.mmd) with a rendered [PNG reference](docs/MIGRATION-ROLLBACK-INCIDENT-FLOWCHART.png). A milestone cannot be marked complete solely because code exists; the corresponding mitigation evidence must also be attached to the milestone record.

### 6.4 Critical-Risk Fallback Playbooks

The following three risks are treated as the highest-priority Critical-risk candidates because they can cause irreversible data loss, incorrect financial results, or an unsafe recovery state. The fallback plans are activated when the relevant trigger occurs, when a reconciliation check fails, or when the delivery lead cannot establish data integrity within the agreed response window. These playbooks are deliberately conservative: **stop, preserve evidence, restore or isolate, reconcile, and only then resume**.

#### Fallback Plan A — R3: Schema Drift or Partial Migration

**Activation conditions:** Stop the migration immediately if any migration step fails, the target schema version is unexpected, a required table/column/index is absent, the initializer is not idempotent, row counts differ beyond the approved tolerance, or stock/payment/invoice reconciliation fails. No further application writes are allowed to the affected target until the incident owner authorizes a controlled recovery.

**Fallback sequence:**

1. **Declare and isolate.** The Data Migration Owner marks the migration as `BLOCKED`, stops the application or places it in read-only/maintenance mode, and records the migration version, timestamp, operator, source database identifier, target database identifier, and last successful step.
2. **Preserve evidence.** Capture migration logs, schema snapshots, checksums, failed SQL statement, application build/version, and database metadata. Do not manually edit or delete target rows while the evidence is being collected.
3. **Protect the source.** Take or confirm a verified backup of the legacy source and preserve it as immutable input. Create a separate diagnostic copy for investigation.
4. **Choose the recovery branch.** If the target contains no accepted business writes, discard the target and rebuild from the last known-good schema. If accepted writes exist, restore the target to the last verified backup and produce a delta report before replaying any approved changes.
5. **Repair in staging only.** Fix the migration script or schema version in a disposable staging database. Re-run the migration twice to prove idempotence, then run row-count, amount, stock, payment, invoice-number, and referential-integrity reconciliation.
6. **Obtain recovery approval.** The DBA and Delivery Lead approve the repaired migration only after the reconciliation report has no unexplained differences. Any accepted difference must have a written business owner sign-off.
7. **Retry and monitor.** Execute the approved migration against a fresh target, keep the source read-only, monitor each version boundary, and retain all logs. Do not combine schema repair with unrelated feature deployment.

**Decision points:**

| Decision | Condition | Action |
|---|---|---|
| Rebuild | No accepted writes exist on the target and source backup is verified. | Drop/recreate the target in the approved isolated environment and restart from the last known-good version. |
| Restore | Target has accepted writes or rebuild would lose approved changes. | Restore the last verified target backup, produce a delta report, and replay only approved deltas. |
| Escalate | Source backup is invalid, reconciliation cannot explain a difference, or data loss is suspected. | Keep the system blocked and escalate to the Delivery Lead, DBA, and business owner; do not proceed by manual correction. |

**Restart criteria:** A fresh target reaches the expected schema version; the migration is repeatable; reconciliation has been signed; backup and rollback artifacts are stored; and the Delivery Lead explicitly changes the risk status from `BLOCKING` to `MITIGATED` or `ACCEPTED`.

**Required evidence:** `docs/risks/R3-migration-incident.md`, source/target schema snapshots, migration logs, verified backup identifier, reconciliation report, repaired migration test, and approval record.

#### Fallback Plan B — R4: Transaction Rollback Leaves Stock, Payment, or Audit Data Inconsistent

**Activation conditions:** Activate when a sale, purchase, return, payment, transfer, or adjustment fails after any partial write; a retry creates a duplicate reference; stock and payment totals diverge; an audit row is missing; or the system cannot determine whether the original transaction committed. The affected operation must be marked `UNKNOWN` rather than retried blindly.

**Fallback sequence:**

1. **Stop duplicate activity.** Disable retry for the affected reference/invoice number, preserve the user-visible error, and prevent a second cashier or job from processing the same idempotency key.
2. **Capture transaction state.** Record the business reference, user, time, database transaction outcome if known, affected header/detail IDs, stock movement IDs, payment IDs, and audit IDs. Do not alter records before the reconciliation query is captured.
3. **Run a read-only reconciliation.** Compare the business header, details, stock balances/movements, payment totals, outstanding balance, invoice number, and audit trail. Classify the incident as `NOT_COMMITTED`, `COMMITTED`, or `PARTIAL/UNKNOWN`.
4. **Recover by branch.** For `NOT_COMMITTED`, release the hold and allow one controlled retry. For `COMMITTED`, mark the client request as completed and block duplicate replay. For `PARTIAL/UNKNOWN`, keep the operation blocked and require a compensating transaction approved by the Application Lead and Finance SME.
5. **Use compensating actions, not ad-hoc edits.** Reverse incorrect stock/payment/audit effects through an auditable reversal service. Never delete financial records to hide a mismatch; preserve the original records and link the correction to the incident reference.
6. **Reconcile again.** Verify stock, payment, outstanding, invoice numbering, and audit totals after the recovery action. Run the same scenario through the failure-injection test suite to prevent recurrence.
7. **Re-enable gradually.** Re-enable the workflow first in a controlled test account, then with a limited pilot role, and finally for general users after the owner signs the recovery evidence.

**Decision points:**

| State | Safe fallback | Resume rule |
|---|---|---|
| `NOT_COMMITTED` | Release the operation and allow one idempotent retry. | Retry succeeds once with one reference and one stock/payment effect. |
| `COMMITTED` | Treat the original as authoritative; return a completed response or reprint from stored data. | Duplicate request is rejected and reconciliation is clean. |
| `PARTIAL/UNKNOWN` | Freeze the reference and execute an approved compensating transaction after investigation. | Stock, payment, outstanding, invoice, and audit totals reconcile with signed evidence. |

**Restart criteria:** Failure-path tests pass; duplicate retry is rejected; reconciliation shows one authoritative business result; audit history contains the original and any reversal; and the Application Lead plus Finance SME approve reopening.

**Required evidence:** `docs/risks/R4-transaction-incident.md`, read-only reconciliation output, transaction/error logs, compensating transaction reference if used, failure-injection test, and approval record.

#### Fallback Plan C — R11: Backup, Restore, or Migration Recovery Fails

**Activation conditions:** Activate when a scheduled backup cannot be verified, checksum fails, restore cannot start or complete, required tables/files are missing, recovery time exceeds the agreed RTO, or a migration rehearsal cannot be rolled back. Destructive migration and release promotion are blocked until a verified recovery point exists.

**Fallback sequence:**

1. **Declare recovery mode.** The Operations Owner announces the incident, records the last known-good backup and last accepted write timestamp, and places migration/release operations on hold.
2. **Protect all available copies.** Preserve the failed backup, logs, checksums, source database, configuration snapshot, and published artifact. Do not overwrite the only copy while troubleshooting.
3. **Select the recovery point.** Choose the newest backup that passes checksum and isolated restore verification. If the newest backup fails, move backward through the retained backup chain rather than attempting uncontrolled repair.
4. **Restore into isolation first.** Restore the selected backup into an isolated environment, run schema checks, application startup checks, representative reads, stock/payment/invoice reconciliation, and a small write-and-rollback test.
5. **Use the fallback operating mode.** If production cannot be restored within the RTO, keep the system read-only or use the approved manual transaction log. Manual transactions must have unique references, operator, timestamp, customer/product/payment details, and dual verification so they can be replayed safely later.
6. **Recover production.** After isolated verification, restore or switch to the approved recovery environment. Replay only the controlled delta since the backup timestamp; do not import an unverified full dump over the recovered database.
7. **Reconcile and communicate.** Compare accepted transactions before and after recovery, quantify any data-loss window, obtain business sign-off, and communicate limitations before returning to normal operations.
8. **Repair the backup process.** Add the failure cause, corrective action, next backup verification date, and owner to the incident record. A backup is not considered restored until a real isolated restore succeeds.

**Decision points:**

| Decision | Condition | Action |
|---|---|---|
| Continue normal service | Latest backup passes checksum and isolated restore within RTO. | Proceed with controlled restore and delta replay. |
| Use manual/read-only mode | Restore is delayed but source data remains protected. | Stop writes or use the approved manual log until a verified environment is available. |
| Roll back release/migration | New release or migration caused the recovery failure. | Return to the last verified artifact/schema and investigate in isolation. |
| Escalate as data-loss incident | No verified backup exists or the loss window cannot be determined. | Escalate to Delivery Lead, DBA, Operations Owner, and business owner; record impact before any further write activity. |

**Restart criteria:** One backup has passed checksum and isolated restore; RPO/RTO impact is documented; recovered data has passed reconciliation; manual transactions have been replayed or explicitly accounted for; runbooks are updated; and the Operations Owner authorizes service restoration.

**Required evidence:** `docs/risks/R11-recovery-incident.md`, backup checksums, isolated restore log, RPO/RTO measurement, recovered-data reconciliation, manual transaction log if used, and service-restoration approval.

### 6.5 Critical-Risk Testing and Dry-Run Schedule

The schedule below uses relative timing so it remains valid when sprint dates change. Every exercise must use synthetic or approved masked data, a named facilitator, an observer, a written result, and an owner for every failed check. A dry run is not complete until the team demonstrates both the procedure and the evidence required to authorize recovery.

| Timing | R3 — Schema / Migration | R4 — Transaction Consistency | R11 — Backup / Recovery | Required output |
|---|---|---|---|---|
| Every pull request affecting schema, repositories, or transactions | Run schema build, migration unit tests, repository integration tests, and transaction rollback tests. | Run commit/rollback, exception, cancellation, duplicate-reference, and idempotency tests for affected workflows. | Run backup configuration/static checks if backup or deployment code changes. | CI result, test logs, and linked defect IDs. |
| Weekly engineering cycle | Re-run initializer twice on a clean database and compare schema/version snapshots. | Run failure injection for sale, purchase, return, payment, and stock movement paths. | Verify the latest backup checksum and perform a sample isolated restore of the smallest representative database. | Weekly risk evidence entry and open-action list. |
| End of each sprint | Perform a staging migration from a representative legacy snapshot; run row-count, totals, stock, payment, invoice, and referential checks. | Execute a controlled partial-failure rehearsal and verify compensating transaction/audit behavior. | Perform a full restore rehearsal into an isolated environment and measure RPO/RTO. | Sprint risk review, reconciliation report, and owner sign-off. |
| Before M1 Data Safety exit | Complete two consecutive idempotent migration runs and one rollback/rebuild exercise. | Complete all failure-path tests with no unexplained stock, payment, or audit differences. | Complete backup/restore and migration rollback rehearsal with documented recovery time. | M1 evidence pack and Delivery Lead approval. |
| Before M2/M3 business release | Rehearse schema upgrade with application version N to N+1 and rollback to N. | Run cashier and operations scenarios with controlled faults and duplicate retry attempts. | Restore the release candidate database, replay controlled deltas, and reconcile business totals. | Release readiness checklist and business-owner sign-off. |
| Before every production migration or release | Conduct a go/no-go review using the latest backup, schema, and reconciliation evidence. | Confirm no `UNKNOWN` transactions or unresolved compensating actions exist. | Verify a fresh restorable backup, operator availability, rollback artifact, and communication plan. | Signed change record; otherwise the change is blocked. |
| After any incident or material procedure change | Repeat the affected dry run and add a regression test before closing the incident. | Repeat the exact failure scenario and confirm duplicate prevention/reconciliation. | Repeat restore and RPO/RTO measurement if recovery tooling changed. | Incident postmortem, updated checklist, and regression evidence. |

**Schedule ownership:** The QA Lead maintains the calendar and evidence index; the Data Migration Owner runs R3 exercises; the Application Lead runs R4 exercises; the Operations Owner/DBA runs R11 exercises; and the Delivery Lead approves milestone or release gates. A missed Critical-risk exercise is recorded as a risk, not silently rescheduled.

### 6.6 Team Operational Checklists

The checklists below are intended to be copied into incident tickets or runbooks. Each checkbox must be marked by the responsible person with a timestamp or linked evidence. `STOP` means no further write, migration, or release activity is permitted until the named decision owner approves continuation.

#### A. Common preparation checklist — before mitigation or dry run

- [ ] Confirm the risk ID, scenario, facilitator, observer, technical owner, business owner, and escalation contact.
- [ ] Confirm the environment is isolated or explicitly approved for controlled testing.
- [ ] Use synthetic or masked data; verify that secrets, real customer data, and production credentials are not present.
- [ ] Record application version, database/schema version, migration version, backup identifier, configuration version, and start time.
- [ ] Confirm rollback artifacts, restore location, runbook version, and communication channel are available.
- [ ] Define expected results, stop conditions, RPO/RTO target where applicable, and evidence files before starting.
- [ ] Notify affected users or testers and confirm who has authority to stop the exercise.

#### B. R3 schema drift / partial migration checklist

**Detection and stop:**

- [ ] **STOP** migration or release activity at the first failed step, unexpected schema version, missing object, non-idempotent behavior, or reconciliation mismatch.
- [ ] Place the affected application/database in maintenance or read-only mode.
- [ ] Record the last successful migration step, target identifier, source identifier, operator, timestamp, and error.

**Evidence and protection:**

- [ ] Export schema/version snapshots before any repair attempt.
- [ ] Preserve migration logs, failed SQL, checksums, build version, and database metadata.
- [ ] Verify and preserve an immutable source backup; never use the diagnostic copy as the new source of truth.
- [ ] Confirm whether the target contains accepted business writes.

**Recovery decision:**

- [ ] If no accepted writes exist, obtain approval to rebuild the target from the last known-good version.
- [ ] If accepted writes exist, restore the last verified target backup and create a delta report.
- [ ] If backup validity or data integrity is uncertain, escalate and keep the system blocked.

**Repair and verification:**

- [ ] Repair migration scripts only in disposable staging.
- [ ] Run the repaired migration twice and compare schema/version snapshots.
- [ ] Reconcile row counts, totals, stock, payments, invoice numbers, foreign keys, and required indexes.
- [ ] Obtain DBA, Delivery Lead, and business-owner approval for any accepted difference.
- [ ] Re-run on a fresh target, retain logs, and attach `R3-migration-incident.md`.

**Restart:**

- [ ] Confirm expected schema version and idempotence.
- [ ] Confirm signed reconciliation and verified rollback artifact.
- [ ] Confirm monitoring and communication are active.
- [ ] Delivery Lead changes status from `BLOCKING` only after all evidence is linked.

#### C. R4 transaction inconsistency checklist

**Detection and containment:**

- [ ] **STOP** retrying the affected reference, invoice, payment, or stock movement.
- [ ] Mark the operation `UNKNOWN` if commit status cannot be proven.
- [ ] Prevent duplicate processing through the idempotency/reference guard.
- [ ] Preserve the user-visible error and transaction correlation ID.

**Read-only investigation:**

- [ ] Capture header/detail IDs, stock movement IDs, payment IDs, audit IDs, user, timestamp, and request/reference number.
- [ ] Compare header, details, stock movement, payment, outstanding, invoice numbering, and audit records without modifying them.
- [ ] Classify the state as `NOT_COMMITTED`, `COMMITTED`, or `PARTIAL/UNKNOWN`.

**Fallback action:**

- [ ] For `NOT_COMMITTED`, release the hold and allow exactly one controlled idempotent retry.
- [ ] For `COMMITTED`, treat the original as authoritative and reject duplicate replay.
- [ ] For `PARTIAL/UNKNOWN`, keep the reference blocked and obtain Application Lead plus Finance SME approval for a compensating transaction.
- [ ] Never delete or manually overwrite financial/stock history to hide the mismatch.

**Verification and controlled reopening:**

- [ ] Verify one authoritative business result and reconcile stock, payment, outstanding, invoice, and audit totals.
- [ ] Link any reversal to the original incident/reference.
- [ ] Run the corresponding failure-injection regression test.
- [ ] Re-enable first for a controlled test account, then a pilot role, then general users.
- [ ] Attach `R4-transaction-incident.md` and obtain Application Lead plus Finance SME approval.

#### D. R11 backup / restore / recovery checklist

**Detection and protection:**

- [ ] **STOP** destructive migration, release promotion, and unverified writes when a backup, checksum, restore, or rollback check fails.
- [ ] Record the last known-good backup, last accepted write time, expected RPO/RTO, and incident start time.
- [ ] Preserve failed backup, logs, checksums, configuration, source database, and release artifact; do not overwrite the only copy.

**Recovery point selection:**

- [ ] Test the newest retained backup checksum.
- [ ] If it fails, move backward through retained backups until one passes checksum and isolated restore.
- [ ] Record the selected recovery point and expected data-loss window.

**Isolated restore and fallback operation:**

- [ ] Restore into an isolated environment before touching production.
- [ ] Run schema checks, startup checks, representative reads, stock/payment/invoice reconciliation, and a write-then-rollback test.
- [ ] If production cannot return within RTO, enable read-only mode or the approved manual transaction log.
- [ ] Ensure every manual transaction has a unique reference, operator, timestamp, customer/product/payment details, and dual verification.

**Production recovery and closeout:**

- [ ] Restore/switch only after isolated verification passes.
- [ ] Replay only controlled deltas after the backup timestamp; do not import an unverified full dump over recovered data.
- [ ] Reconcile accepted transactions before/after recovery and document the data-loss window.
- [ ] Replay or explicitly account for every manual transaction.
- [ ] Update the backup/runbook process and attach `R11-recovery-incident.md`.
- [ ] Operations Owner authorizes service restoration only after RPO/RTO and reconciliation evidence are complete.

#### E. Communication and approval checklist

- [ ] Name one incident lead and one note-taker; do not allow the person performing a data correction to be the sole approver of safety.
- [ ] Send an initial status containing risk ID, impact, affected workflows, current mode, next decision time, and user instructions.
- [ ] Send updates at the agreed incident cadence or immediately when the recovery branch changes.
- [ ] Record every approval, rejection, exception, and accepted difference in the incident ticket.
- [ ] Notify stakeholders before switching from maintenance/read-only/manual mode back to normal service.
- [ ] Publish a short post-incident summary with root cause, data impact, recovery duration, corrective action, and regression test.

#### F. Dry-run completion checklist

- [ ] Scenario, environment, data set, participants, and start/end times are recorded.
- [ ] The team successfully identified the trigger and executed the STOP action.
- [ ] The correct fallback branch was selected without relying on undocumented tribal knowledge.
- [ ] Recovery/rollback completed within the agreed target or the miss was recorded.
- [ ] Reconciliation results match expected totals and no unexplained differences remain.
- [ ] Required logs, screenshots/reports, approvals, and checksums are attached.
- [ ] Every failed step has an owner and due date.
- [ ] Runbook/checklist/test has been updated before the dry run is marked complete.

#### Critical fallback governance

No single individual should approve both an emergency data correction and the evidence that declares it safe. At minimum, the operational owner performs the recovery, the technical owner verifies system/data integrity, and the business owner verifies financial and stock results. If any of the three fallback plans is activated, the affected milestone remains **blocked** until the restart criteria and evidence are complete.

---

## 7. Updated Milestones

| Milestone | Definition of done |
|---|---|
| M0 Baseline locked | Clean Release build, current tests, CI result, and defect register are recorded. |
| M1 Data safety | Schema/versioning, transactions, rollback, audit, and migration mapping are verified. |
| M2 Sales ready | Core cashier workflows pass UI, service, repository, permission, and print checks. |
| M3 Operations ready | Contacts, purchasing, payments, expenses, inventory, warehouse, and transfers reconcile correctly. |
| M4 Reporting ready | Critical reports/vouchers pass parameter, export, print, and parity checks. |
| M5 Administration ready | Settings, licensing, backup/restore, migration, and SuperAdmin controls are verified. |
| M6 Release candidate | Full parity matrix, performance, long-session, localization, security, migration, and installer checks pass. |

---

## 8. Next Immediate Step

Start **Phase 0 — Baseline and Evidence Lock**. Do not generate new POCOs, repositories, or screens until the clean build/test result and parity matrix identify a confirmed gap. The first implementation batch should be the smallest set of fixes required to make the existing .NET 8 solution and CI pipeline reliable, followed by transaction/data-safety validation and sales workflow hardening.

### Verification commands

```powershell
dotnet restore MMNextPOS.slnx
dotnet build MMNextPOS.slnx --configuration Release
dotnet format --verify-no-changes
dotnet test tests/MMNextPOS.Application.Tests/MMNextPOS.Application.Tests.csproj --configuration Release
dotnet test tests/MMNextPOS.Infrastructure.Tests/MMNextPOS.Infrastructure.Tests.csproj --configuration Release
```

*Last updated after repository inspection and comparison with `Discovery-Report.md`.*
