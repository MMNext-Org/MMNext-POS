# R11 Recovery Incident Record

**Incident ID:** `R11-YYYY-MM-NNN`  
**Date / Time (UTC):**  
**Incident Lead:**  
**Note-taker:**  
**Risk Classification:** Backup, restore, or migration recovery fails (R11)

---

## 1. Trigger and Detection

- [ ] **What triggered the STOP?** (Backup cannot be verified, checksum fails, restore cannot start/complete, required tables/files missing, recovery time exceeds RTO, migration rehearsal cannot be rolled back)
- [ ] **Affected system(s):** (Production, staging, CI/CD pipeline)
- [ ] **Last known-good backup:** (Identifier, timestamp, location)
- [ ] **Last accepted write timestamp:**
- [ ] **RPO target:** (Recovery Point Objective)
- [ ] **RTO target:** (Recovery Time Objective)
- [ ] **Incident start timestamp:**

---

## 2. Evidence Preservation (do not overwrite the only copy)

- [ ] Failed backup preserved (do not delete)
- [ ] Backup logs and checksums captured
- [ ] Source database snapshot preserved
- [ ] Configuration snapshot preserved
- [ ] Published artifact preserved
- [ ] All retained backup copies protected from overwrite

---

## 3. Recovery Point Selection

- [ ] Newest retained backup selected
- [ ] Backup identifier:
- [ ] Checksum verification: PASS / FAIL
- [ ] Isolated restore test: PASS / FAIL
- [ ] If FAIL: moved to next retained recovery point (repeat until PASS)
- [ ] Recovery point selected: (Backup identifier, timestamp)

---

## 4. Isolated Restore Verification

Restore into **isolated environment only** — do not touch production until verified.

- [ ] Schema checks pass
- [ ] Application startup checks pass
- [ ] Representative reads pass (key tables accessible)
- [ ] Stock totals verified
- [ ] Payment totals verified
- [ ] Invoice totals verified
- [ ] Write-then-rollback test passes

---

## 5. Fallback Operating Mode (if RTO will be missed)

- [ ] Read-only mode enabled? (Y/N)
- [ ] Approved manual transaction log activated? (Y/N)
- [ ] Manual transaction log requirements met:
  - [ ] Unique reference per transaction
  - [ ] Operator identified
  - [ ] Timestamp recorded
  - [ ] Customer / product / payment details captured
  - [ ] Dual verification (maker/checker) documented

---

## 6. Production Recovery

- [ ] Production restored / switched to verified recovery environment
- [ ] Only controlled deltas replayed (since backup timestamp)
- [ ] No unverified full dump imported over recovered database

---

## 7. Reconciliation and Communication

| Check | Pre-Recovery | Post-Recovery | Status | Notes |
|---|---|---|---|---|
| Accepted transactions count | | | | |
| Stock balances | | | | |
| Payment totals | | | | |
| Invoice numbers | | | | |
| Outstanding balances | | | | |
| Audit records | | | | |
| Data-loss window | | | | |

- [ ] Reconciliation report attached
- [ ] Business sign-off obtained on limitations / data-loss window
- [ ] User-facing limitations communicated before normal operations resume

---

## 8. Backup Process Repair

- [ ] Failure cause documented
- [ ] Corrective action assigned
- [ ] Next backup verification date scheduled
- [ ] Owner assigned
- [ ] Incident record updated with repair details

> **Rule:** A backup is not considered restored until a real isolated restore succeeds.

---

## 9. Approvals

| Role | Name | Decision | Timestamp | Evidence Link |
|---|---|---|---|---|
| Operations Owner / DBA | | | | |
| Delivery Lead | | | | |
| Business Owner | | | | |

---

## 10. Restart Criteria (all must be met)

- [ ] One backup has passed checksum and isolated restore
- [ ] RPO/RTO impact documented
- [ ] Recovered data has passed reconciliation
- [ ] Manual transactions replayed or explicitly accounted for
- [ ] Runbooks updated
- [ ] Operations Owner authorizes service restoration

---

## 11. Post-Incident

- [ ] Root cause documented
- [ ] Corrective actions assigned with owners and due dates
- [ ] Restore and RPO/RTO measurement repeated if recovery tooling changed
- [ ] Runbook / checklist updated
- [ ] Risk register updated with new score / evidence

---

## 12. Artifacts Attached

- [ ] `docs/risks/R11-recovery-incident.md` (this file)
- [ ] Backup checksums
- [ ] Isolated restore log
- [ ] RPO/RTO measurement
- [ ] Recovered-data reconciliation report
- [ ] Manual transaction log (if used)
- [ ] Service-restoration approval

> **Gap note:** The flowchart `docs/MIGRATION-ROLLBACK-INCIDENT-FLOWCHART.mmd` exists. The rendered PNG `docs/MIGRATION-ROLLBACK-INCIDENT-FLOWCHART.png` is **missing and needs generation**.