# R3 Migration Incident Record

**Incident ID:** `R3-YYYY-MM-NNN`  
**Date / Time (UTC):**  
**Incident Lead:**  
**Note-taker:**  
**Risk Classification:** Schema drift / partial migration (R3)

---

## 1. Trigger and Detection

- [ ] **What triggered the STOP?** (Failed migration step, unexpected schema version, missing object, non-idempotent behavior, reconciliation mismatch)
- [ ] **Affected system(s):** (Application, database, environment identifiers)
- [ ] **Last successful migration step / version:**
- [ ] **Error message / failed SQL:**
- [ ] **Operator / automated job:**
- [ ] **Timestamp of detection:**

---

## 2. Evidence Preservation (do not modify before capturing)

- [ ] Migration logs (full output)
- [ ] Schema snapshots — source (pre-migration) and target (current state)
- [ ] Checksums of source and target databases
- [ ] Failed SQL statement(s)
- [ ] Application build / version / commit SHA
- [ ] Database metadata (table counts, row counts for key tables)
- [ ] Configuration snapshot (connection strings, migration settings)

> **Rule:** Never manually edit or delete target rows while evidence is being collected.

---

## 3. Source Protection

- [ ] Verified immutable backup of legacy source captured? (Y/N)
- [ ] Backup identifier / location:
- [ ] Diagnostic copy created for investigation? (Y/N — must NOT become new source of truth)

---

## 4. Target Write Status

- [ ] Does the target contain accepted business writes? (Y/N)
- [ ] If **Yes**: list affected tables / business domains:
- [ ] If **No**: proceed to Rebuild path

---

## 5. Recovery Decision

### Rebuild Path (no accepted writes on target)
- [ ] Source backup verified (checksum + test restore)
- [ ] Target dropped/recreated in approved isolated environment
- [ ] Migration re-run from last known-good version
- [ ] Idempotence proven (run twice, same result)

### Restore Path (accepted writes exist)
- [ ] Last verified target backup restored
- [ ] Delta report produced (approved changes since backup)
- [ ] Migration repaired only in disposable staging
- [ ] Migration run twice in staging; idempotence + reconciliation verified
- [ ] DBA, Delivery Lead, and business-owner approval obtained

---

## 6. Reconciliation

| Check | Target | Actual | Status | Notes |
|---|---|---|---|---|
| Schema version | | | | |
| Row counts (core tables) | | | | |
| Totals (stock, payments, invoices) | | | | |
| Invoice number sequence | | | | |
| Foreign key integrity | | | | |
| Required indexes present | | | | |

- [ ] Reconciliation report attached
- [ ] Any accepted differences have written business-owner sign-off

---

## 7. Approvals

| Role | Name | Decision | Timestamp | Evidence Link |
|---|---|---|---|---|
| Data Migration Owner / DBA | | | | |
| Delivery Lead | | | | |
| Business Owner / Finance SME | | | | |

---

## 8. Restart Criteria (all must be met)

- [ ] Fresh target reaches expected schema version (`009` or current)
- [ ] Migration is repeatable (idempotent run verified)
- [ ] Reconciliation signed by DBA + Delivery Lead + Business Owner
- [ ] Backup and rollback artifacts stored
- [ ] Delivery Lead explicitly changes risk status from `BLOCKING` to `MITIGATED` or `ACCEPTED`

---

## 9. Post-Incident

- [ ] Root cause documented
- [ ] Corrective actions assigned with owners and due dates
- [ ] Regression test added (or linked to existing failure-injection test)
- [ ] Runbook / checklist updated
- [ ] Risk register updated with new score / evidence

---

## 10. Artifacts Attached

- [ ] `docs/risks/R3-migration-incident.md` (this file)
- [ ] Source / target schema snapshots
- [ ] Migration logs
- [ ] Verified backup identifier
- [ ] Reconciliation report
- [ ] Repaired migration test results
- [ ] Approval records

> **Gap note:** The flowchart `docs/MIGRATION-ROLLBACK-INCIDENT-FLOWCHART.mmd` exists. The rendered PNG `docs/MIGRATION-ROLLBACK-INCIDENT-FLOWCHART.png` is **missing and needs generation**.