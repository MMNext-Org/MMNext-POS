# MMNext POS Critical-Risk Implementation Guide

## 1. Purpose and operating principle

This guide turns the critical-risk controls in [Plan.md](../Plan.md) into a repeatable operating procedure for engineering, QA, migration, operations, and business teams. Use it for normal mitigation work, scheduled tests, dry runs, production changes, and real incidents involving schema migration, transaction consistency, backup, or rollback.

> **Operating principle:** Stop the affected activity, preserve evidence, isolate the environment, reconcile the data, obtain independent approval, and only then resume.

No team member should improvise a data correction, delete a financial record to hide a mismatch, overwrite the only backup, or retry an operation whose commit state is unknown.

## 2. Roles and separation of duties

| Role | Responsibilities |
|---|---|
| Delivery Lead | Owns risk acceptance, release gates, escalation, and final restart approval. |
| QA Lead | Maintains the testing/dry-run calendar, evidence index, regression coverage, and defect follow-up. |
| Data Migration Owner / DBA | Owns schema inventory, migration scripts, backups, reconciliation, and R3 recovery. |
| Application Lead | Owns Unit of Work, idempotency, transaction behavior, failure injection, and R4 recovery. |
| Operations Owner | Owns backup jobs, restore environments, RPO/RTO measurement, runbooks, and R11 recovery. |
| Business Owner / Finance SME | Confirms stock, payment, invoice, outstanding, and financial correctness. |
| Incident Lead | Coordinates a live event, assigns actions, controls communication, and maintains the timeline. |
| Note-taker | Records timestamps, commands, decisions, evidence links, approvals, and unresolved actions. |

The person performing an emergency data correction must not be the sole person approving that the correction is safe.

## 3. Required artifacts and naming

Create or update the following artifacts for every test, dry run, change, or incident:

| Artifact | Required contents |
|---|---|
| `docs/risks/RISK-REGISTER.md` | Risk score, trigger, owner, due date, status, next action, and evidence links. |
| `docs/baseline/BUILD-TEST-BASELINE.md` | Environment, commands, build/test result, failures, root cause, and owner. |
| `docs/risks/R3-migration-incident.md` | Migration version, failed step, schema snapshots, backup, reconciliation, and decision record. |
| `docs/risks/R4-transaction-incident.md` | Business reference, state classification, read-only investigation, compensating action, and approvals. |
| `docs/risks/R11-recovery-incident.md` | Backup/checksum, restore point, RPO/RTO, recovered data, manual transactions, and restoration approval. |
| `docs/parity/` | Feature/report/voucher status and evidence for functional parity. |

Use a unique incident or dry-run identifier such as `R3-2026-09-001`. Never place passwords, connection strings, tokens, or unmasked customer data in these files.

## 4. Standard operating cycle

### 4.1 Before work starts

- [ ] Confirm the risk ID, scenario, objective, owner, facilitator, observer, business approver, and escalation contact.
- [ ] Confirm whether the work is a PR test, weekly test, sprint dry run, release rehearsal, or live incident.
- [ ] Confirm the environment and ensure it is isolated or explicitly approved.
- [ ] Use synthetic or approved masked data.
- [ ] Record application, database, schema, migration, configuration, and backup versions.
- [ ] Define expected results, stop conditions, recovery point, RPO/RTO, and required evidence.
- [ ] Confirm the rollback artifact, restore location, runbook version, and communication channel.

### 4.2 During work

- [ ] The facilitator announces each step before execution.
- [ ] The note-taker records start/end timestamps, command or action, result, and evidence path.
- [ ] The observer checks that no step bypasses the checklist.
- [ ] Stop immediately on a failed condition, unexpected write, unexplained mismatch, or uncertain commit state.
- [ ] Do not perform repairs in production or directly edit business history.

### 4.3 After work

- [ ] Run the required reconciliation checks.
- [ ] Attach logs, screenshots, reports, checksums, approvals, and test results.
- [ ] Record every failed step with an owner and due date.
- [ ] Update the runbook, test, risk register, or parity matrix before closing the activity.
- [ ] Obtain independent technical and business approval.
- [ ] Mark the activity `PASS`, `PASS WITH ACTIONS`, `BLOCKED`, or `FAILED`; do not use “done” without evidence.

## 5. Testing and dry-run schedule

| Cadence | R3 Schema/Migration | R4 Transaction | R11 Backup/Recovery | Completion evidence |
|---|---|---|---|---|
| Every relevant PR | Migration unit tests, repository integration tests, schema build, and idempotence checks. | Commit/rollback, exception, cancellation, duplicate-reference, and idempotency tests. | Backup/deployment configuration checks when relevant. | CI result and linked defects. |
| Weekly | Initializer twice on a clean DB; compare schema/version snapshots. | Failure injection for sale, purchase, return, payment, and stock movement. | Latest checksum plus sample isolated restore. | Weekly risk entry and open actions. |
| End of sprint | Staging migration with row-count, totals, stock, payment, invoice, and FK checks. | Partial-failure rehearsal and compensating transaction/audit verification. | Full isolated restore and RPO/RTO measurement. | Sprint review and signed reconciliation. |
| Before M1 | Two idempotent migration runs plus rollback/rebuild exercise. | All failure-path tests with no unexplained differences. | Backup/restore and migration rollback rehearsal. | M1 evidence pack and approval. |
| Before production change | Go/no-go review, fresh verified backup, rollback artifact, and no unresolved `UNKNOWN` transactions. | Confirm no unresolved compensating actions. | Confirm operator availability and communication plan. | Signed change record. |
| After incident/tooling change | Repeat affected dry run and add regression test. | Repeat exact failure scenario. | Repeat restore and RPO/RTO measurement. | Postmortem and updated procedure. |

A missed Critical-risk exercise is itself a risk event and must be recorded rather than silently rescheduled.

## 6. R3 implementation: schema drift or partial migration

### Normal mitigation

1. Maintain a model/query/table schema inventory.
2. Use explicit, ordered, versioned migrations; do not rely only on `CREATE TABLE IF NOT EXISTS`.
3. Test migrations against a disposable database and run the same migration twice.
4. Reconcile rows, totals, stock, payments, invoices, foreign keys, and indexes.
5. Keep an immutable source backup and a fresh target backup before migration.

### Fallback execution

- [ ] `STOP`: Block migration and switch the affected system to maintenance/read-only mode.
- [ ] Record the last successful version, error, source/target identifiers, and operator.
- [ ] Preserve logs, schema snapshots, checksums, failed SQL, and build version.
- [ ] Confirm whether the target contains accepted business writes.
- [ ] Rebuild only when there are no accepted writes and the source backup is verified.
- [ ] Restore and produce a delta report when accepted writes exist.
- [ ] Repair only in disposable staging; prove idempotence and reconciliation.
- [ ] Obtain DBA, Delivery Lead, and business-owner approval.
- [ ] Retry on a fresh target and retain all logs.
- [ ] Resume only after schema version, reconciliation, rollback artifact, and approvals are complete.

## 7. R4 implementation: transaction inconsistency

### Normal mitigation

1. Use a single Unit of Work and transaction boundary for each business operation.
2. Enforce unique references and idempotency keys.
3. Test commit, rollback, timeout, cancellation, exception, and duplicate retry paths.
4. Keep financial and stock history auditable; use compensating transactions instead of deletion.

### Fallback execution

- [ ] `STOP`: Prevent retry of the affected reference/invoice/payment/stock movement.
- [ ] Mark state `UNKNOWN` when commit status cannot be proven.
- [ ] Capture all header, detail, stock, payment, outstanding, invoice, and audit IDs.
- [ ] Run read-only reconciliation and classify `NOT_COMMITTED`, `COMMITTED`, or `PARTIAL/UNKNOWN`.
- [ ] For `NOT_COMMITTED`, allow exactly one controlled idempotent retry.
- [ ] For `COMMITTED`, treat the original as authoritative and reject duplicate replay.
- [ ] For `PARTIAL/UNKNOWN`, freeze the reference and obtain Application Lead plus Finance SME approval.
- [ ] Execute any correction through an auditable compensating service.
- [ ] Reconcile again and run the matching failure-injection regression test.
- [ ] Re-enable through test account, pilot role, then general users.

## 8. R11 implementation: backup, restore, and recovery

### Normal mitigation

1. Define RPO/RTO with stakeholders.
2. Verify backup checksums and perform isolated restores on schedule.
3. Keep multiple retained recovery points and protect them from overwrite.
4. Rehearse migration rollback and maintain a manual/read-only fallback mode.
5. Test the restored application, not only the database files.

### Fallback execution

- [ ] `STOP`: Block destructive migration, release promotion, and unverified writes.
- [ ] Record last known-good backup, last accepted write time, RPO/RTO, and incident start.
- [ ] Preserve failed backup, logs, checksums, source, configuration, and artifact.
- [ ] Move backward through retained backups until checksum and isolated restore pass.
- [ ] Restore in isolation and verify schema, startup, reads, stock/payment/invoice totals, and write-then-rollback.
- [ ] If RTO will be missed, enable read-only mode or the approved manual transaction log.
- [ ] Give each manual transaction a unique reference, operator, timestamp, details, and dual verification.
- [ ] Restore production only after isolated verification.
- [ ] Replay only controlled deltas after the backup timestamp.
- [ ] Reconcile before/after data and document any loss window.
- [ ] Operations Owner authorizes service restoration only after evidence is complete.

## 9. Incident communication protocol

The Incident Lead sends an initial notice containing the risk ID, impact, affected workflows, current mode, next decision time, and user instructions. Updates are sent at the agreed cadence or whenever the recovery branch changes. Before normal service resumes, the Incident Lead confirms the recovery result, outstanding limitations, and user-facing instructions. A post-incident summary must record root cause, data impact, recovery duration, corrective actions, and regression tests.

## 10. Training and readiness

Run a 30-minute checklist walkthrough for each new team member. Run a tabletop exercise before the first real migration, then a technical dry run before every milestone that changes schema, transactions, or recovery tooling. The QA Lead maintains attendance and competency evidence. A team is not considered ready until each required role can identify the STOP condition, locate the correct runbook, preserve evidence, and state who approves restart.

## 11. Definition of ready and definition of recovered

A change is **ready** only when the test result, rollback artifact, backup/recovery point, owner, approver, communication plan, and evidence location are known. An incident is **recovered** only when the affected data reconciles, the service operates in the approved mode, evidence is complete, corrective tests pass, and the responsible technical and business owners approve restart.
