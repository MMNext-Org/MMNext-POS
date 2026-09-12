# R4 Transaction Incident Record

**Incident ID:** `R4-YYYY-MM-NNN`  
**Date / Time (UTC):**  
**Incident Lead:**  
**Note-taker:**  
**Risk Classification:** Transaction rollback leaves stock, payment, or audit data inconsistent (R4)

---

## 1. Trigger and Detection

- [ ] **What triggered the STOP?** (Sale/purchase/return/payment/transfer/adjustment failed after partial write; retry created duplicate reference; stock and payment totals diverged; audit row missing; commit state unknown)
- [ ] **Affected business reference:** (Sale ID, Purchase ID, Invoice No, Payment No, Stock Movement ID)
- [ ] **Affected workflow:** (Sale, Purchase, Return, Payment, Transfer, Adjustment)
- [ ] **Error / exception message:**
- [ ] **Operator / cashier / job:**
- [ ] **Timestamp of failure:**

---

## 2. Evidence Preservation (STOP — do not modify records)

- [ ] User-visible error preserved (screenshot or text)
- [ ] Transaction correlation ID captured
- [ ] Idempotency / reference key frozen — no retry allowed
- [ ] All related IDs captured:
  - Header IDs: Sale / Purchase / Payment / Stock Movement
  - Detail IDs: SaleDetail / PurchaseDetail / PaymentDetail / StockMovementDetail
  - Audit IDs: ChangeDateLog entries
  - Outstanding IDs: CustomerOutstanding / SupplierOutstanding
  - Invoice IDs: Invoice / InvoiceSequences

---

## 3. Read-Only Investigation

### State Classification
Run reconciliation queries **without modifying data** and classify:

| Classification | Criteria | Selected |
|---|---|---|
| `NOT_COMMITTED` | No header row; no details; no stock movement; no audit; no outstanding; no invoice sequence bump | [ ] |
| `COMMITTED` | Header exists with complete details; stock movement recorded; audit present; outstanding updated; invoice sequence incremented | [ ] |
| `PARTIAL/UNKNOWN` | Some writes present but incomplete; commit state cannot be proven | [ ] |

**Classification result:** `NOT_COMMITTED` / `COMMITTED` / `PARTIAL/UNKNOWN`

### Reconciliation Queries (attach output)

- [ ] Business header (Sale/Purchase/Return/Payment/Transfer)
- [ ] Detail rows
- [ ] Stock balances & movements
- [ ] Payment totals
- [ ] Outstanding balances (customer / supplier)
- [ ] Invoice number sequence
- [ ] Audit trail (ChangeDateLog)

---

## 4. Recovery by Branch

### `NOT_COMMITTED`
- [ ] Hold released
- [ ] Exactly one controlled idempotent retry allowed
- [ ] Retry succeeds with one reference and one stock/payment effect

### `COMMITTED`
- [ ] Original treated as authoritative
- [ ] Completed response returned or reprint from stored data
- [ ] Duplicate request rejected
- [ ] Reconciliation clean

### `PARTIAL/UNKNOWN`
- [ ] Reference frozen — no further processing
- [ ] Application Lead + Finance SME approval obtained for compensating transaction
- [ ] Compensating transaction executed via auditable reversal service
- [ ] Original records preserved; correction linked to incident reference

---

## 5. Compensating Transaction (if applicable)

- [ ] Compensating transaction reference / ID:
- [ ] Type: (Stock reversal / Payment reversal / Outstanding adjustment / Invoice void / Audit correction)
- [ ] Executed by:
- [ ] Approved by Application Lead:
- [ ] Approved by Finance SME:
- [ ] Reverses exactly the incorrect effect (no over/under correction)

---

## 6. Re-Verification

- [ ] Stock, payment, outstanding, invoice numbering, audit totals reconciled after recovery
- [ ] Matching failure-injection regression test executed and passed
- [ ] Re-enable sequence: test account → pilot role → general users

---

## 7. Approvals

| Role | Name | Decision | Timestamp | Evidence Link |
|---|---|---|---|---|
| Application Lead | | | | |
| Finance SME / Business Owner | | | | |
| Delivery Lead | | | | |

---

## 8. Restart Criteria (all must be met)

- [ ] Failure-path tests pass (commit/rollback/exception/cancellation/duplicate-reference/idempotency)
- [ ] Duplicate retry rejected (idempotency key enforced)
- [ ] Reconciliation shows one authoritative business result
- [ ] Audit history contains original and any reversal
- [ ] Application Lead + Finance SME approve reopening

---

## 9. Post-Incident

- [ ] Root cause documented
- [ ] Corrective actions assigned with owners and due dates
- [ ] Regression test added / linked to failure-injection suite
- [ ] Runbook / checklist updated
- [ ] Risk register updated with new score / evidence

---

## 10. Artifacts Attached

- [ ] `docs/risks/R4-transaction-incident.md` (this file)
- [ ] Read-only reconciliation output
- [ ] Transaction / error logs
- [ ] Compensating transaction reference (if used)
- [ ] Failure-injection test results
- [ ] Approval records

> **Gap note:** The flowchart `docs/MIGRATION-ROLLBACK-INCIDENT-FLOWCHART.mmd` exists. The rendered PNG `docs/MIGRATION-ROLLBACK-INCIDENT-FLOWCHART.png` is **missing and needs generation**.