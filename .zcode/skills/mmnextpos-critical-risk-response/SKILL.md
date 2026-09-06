---
name: mmnextpos-critical-risk-response
description: Create and operate reusable critical-risk registers, mitigation plans, testing schedules, dry-run programs, team checklists, and migration/rollback incident-response flowcharts for POS or transactional software projects. Use when a project needs detailed risk controls, backup and restore planning, schema migration safety, transaction rollback procedures, operational runbooks, or stakeholder-ready implementation guidance. Also use for MMNextPOS-specific backup/restore, database migration, stock/payment integrity, and release-safety planning, even if the user just says "risk register", "rollback plan", or "recovery runbook".
---

# MMNextPOS Critical-Risk Response

## Overview

Use this skill to convert a project risk register into an executable operating system: detailed mitigations, fallback playbooks, recurring test/dry-run schedules, role-based checklists, incident flowcharts, evidence requirements, and restart gates. It is designed for POS and other transactional systems where schema, stock, payments, audit history, backups, and releases must remain consistent.

## Workflow

1. **Inspect the project context.** Read the project plan, existing risk register, architecture notes, CI workflow, migration/backup documentation, and applicable repository instructions. Identify existing owners, environments, commands, artifacts, and business invariants.
2. **Select critical risks.** Prioritize risks that can cause irreversible data loss, incorrect financial/stock results, security exposure, or unsafe recovery. Prefer three high-impact risks when stakeholders request a focused playbook. Typical choices are schema/migration drift, transaction inconsistency, and backup/restore failure.
3. **Define each risk completely.** For every risk document triggers, preventive controls, activation conditions, stop conditions, fallback branches, contingency actions, owner, independent approver, verification evidence, restart criteria, and required incident artifacts.
4. **Create the testing program.** Use relative cadence: relevant pull requests, weekly engineering checks, end-of-sprint dry runs, pre-milestone rehearsals, pre-release go/no-go checks, and post-incident regression tests. Every exercise must name a facilitator, observer, owner, expected result, and evidence location.
5. **Create operational checklists.** Write checkboxes for preparation, STOP action, evidence preservation, read-only investigation, recovery decision, verification, controlled reopening, communication, approvals, and closeout. Make checklists copyable into incident tickets.
6. **Create a flowchart.** Model the common path as `detect → stop → declare → isolate → preserve evidence → classify risk → choose recovery branch → reconcile → independent technical verification → business verification → controlled reopening → post-incident review`. Include explicit `NO/ESCALATE/BLOCKED` branches.
7. **Integrate and validate.** Link the guide and flowchart from the project plan (creating the link if the plan already has an index section, noting the artifacts as new otherwise), preserve repository naming conventions, and check that all referenced files and owners are coherent — referenced paths either exist or are listed as artifacts the current task will create.

## Required Safety Rules

- Treat uncertain commit state as `UNKNOWN`; never retry blindly.
- Stop writes, migration, release promotion, and destructive actions when integrity or recovery is uncertain.
- Preserve logs, schema snapshots, checksums, IDs, versions, backups, and incident timeline before repair.
- Repair only in an isolated or disposable environment first.
- Use compensating transactions for financial or stock corrections; never delete history to hide mismatches.
- Restore backups into isolation and verify the application, not only database files.
- Use synthetic or approved masked data in tests, dry runs, diagnostics, and external advisory calls.
- Separate duties: the person performing a correction must not be the sole approver of safety.
- Resume only after reconciliation, evidence completion, independent technical verification, business verification, and explicit restart approval.

## Risk Playbook Template

For each critical risk, use this order:

1. **Activation conditions** - observable triggers and early warnings.
2. **STOP action** - what activity must immediately halt and which mode to use.
3. **Evidence preservation** - logs, versions, IDs, snapshots, checksums, and backups.
4. **Classification** - the state or branch that determines the safe fallback.
5. **Fallback sequence** - numbered low-ambiguity actions.
6. **Decision table** - condition, safe action, and resume rule.
7. **Restart criteria** - objective evidence required before reopening.
8. **Required evidence** - exact incident/runbook/report paths.

## Recommended Critical Playbooks

### Schema or migration drift

Stop migration and writes, preserve source and target evidence, determine whether the target contains accepted writes, rebuild only when safe, restore and create a delta report when necessary, repair in staging, prove idempotence, reconcile rows/totals/stock/payments/invoices/constraints, and approve a fresh-target retry.

### Transaction inconsistency

Freeze the reference, mark uncertain state `UNKNOWN`, prevent duplicate processing, investigate read-only, classify `NOT_COMMITTED`/`COMMITTED`/`PARTIAL-UNKNOWN`, retry only the first case once, accept the original in the second case, and require an approved compensating transaction in the third case.

### Backup or recovery failure

Block destructive work, protect all backup copies, walk backward through retained recovery points until checksum and isolated restore pass, verify schema/startup/reads/business totals/write-rollback, use read-only or an approved manual log if RTO will be missed, replay controlled deltas, reconcile, and authorize restoration only after evidence is complete.

## Deliverable Structure

When creating project artifacts, prefer these paths (create any that do not exist; keep numbering consistent with the project's existing risk register if one is present):

- `docs/CRITICAL-RISK-IMPLEMENTATION-GUIDE.md` for team instructions, schedules, role ownership, and operational checklists.
- `docs/MIGRATION-ROLLBACK-INCIDENT-FLOWCHART.mmd` for Mermaid incident flowcharts.
- `docs/risks/RISK-REGISTER.md` for the active register and evidence links.
- `docs/risks/R3-migration-incident.md`, `R4-transaction-incident.md`, and `R11-recovery-incident.md` for incident records.

## Quality Check

Before delivering, verify that:

- Every critical risk has a trigger, owner, fallback, evidence, and restart gate.
- Schedules include PR, weekly, sprint, milestone, pre-release, and post-incident points where applicable.
- Checklists contain STOP conditions and do not require undocumented tribal knowledge.
- Flowcharts include escalation and blocked paths.
- No procedure encourages destructive manual edits, blind retry, unverified restore, or unapproved restart.
- Project-specific references point to real files or are clearly labeled as artifacts to create.
- The skill directory structure is valid: `SKILL.md` exists with `name` (kebab-case, matching the directory name) and `description` frontmatter, and any referenced `references/`, `scripts/`, or `assets/` files are present.
