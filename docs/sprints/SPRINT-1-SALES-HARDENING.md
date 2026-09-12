# MMNext POS — Sprint 1: Sales Hardening

**Goal:** Complete the core cashier workflow (hold/unhold, return/refund, void/cancel) and harden the NewSaleForm.

**Related Issues:** P0 gaps in SalesListPage and NewSaleForm from parity matrix  
**Milestone:** M2 — Sales ready

---

## Tasks

### 1. Review Current State

| Task | Description | Estimate |
|---|---|---|
| 1.1 Review NewSaleForm.cs | Understand current sale-temp integration and UI wiring | 15 min |
| 1.2 Review SalesListPage.cs | Check current column config and edit/delete handling | 10 min |
| 1.3 Review SalesService.cs | Audit CreateSaleWithDetailsAsync, look for hold/return/void | 15 min |
| 1.4 Review SaleTempService | Check SaleTemp CRUD methods and draft/resume logic | 10 min |

### 2. Implement Hold/Unhold Workflow

| Task | Description | Estimate |
|---|---|---|
| 2.1 Add Hold button handler | Call SaleTempService.CreateAsync with current sale data | 30 min |
| 2.2 Add Unhold (Resume) button | Load SaleTemp details into form for editing | 30 min |
| 2.3 Add SaleTemp validation | Check for expired/invalid drafts | 15 min |
| 2.4 Add SaleTemp cleanup | Delete temp on successful sale completion | 15 min |
| 2.5 Unit tests | Mock SaleTempService for hold/resume paths | 60 min |

### 3. Implement Return/Refund Path

| Task | Description | Estimate |
|---|---|---|
| 3.1 Add Return button | Open return dialog with sale line items | 30 min |
| 3.2 Create return validation | Check sale is completed, not already returned | 15 min |
| 3.3 Create return service method | In SalesService: CreateReturnAsync | 45 min |
| 3.4 Update stock | Increment product quantities on return | 30 min |
| 3.5 Handle tenders/refunds | Cash/credit card refund logic | 30 min |
| 3.6 Audit trail | Log return with reason and original sale ID | 15 min |
| 3.7 Unit tests | Test return validation and stock increment | 60 min |

### 4. Implement Void/Cancel with Audit

| Task | Description | Estimate |
|---|---|---|
| 4.1 Add Void button | Mark sale as Voided, reverse stock | 30 min |
| 4.2 Permission check | Require Manager approval for void after payment | 20 min |
| 4.3 Stock reversal | Increment quantities for all sale lines | 20 min |
| 4.4 Audit requirement | Void reason required, log to ChangeDateLog | 20 min |
| 4.5 Payment reversal | Integration with payment service | 30 min |
| 4.6 Unit tests | Test void validation and audit logging | 45 min |

### 5. Enhance NewSaleForm UI/UX

| Task | Description | Estimate |
|---|---|---|
| 5.1 Draft indicator | Visual indicator when resuming a draft | 15 min |
| 5.2 Auto-save draft | Periodically save current state as SaleTemp | 30 min |
| 5.3 Line item validation | Prevent zero/negative quantities, duplicate products | 20 min |
| 5.4 Keyboard shortcuts | F2-add line, F3-remove, F5-hold, F6-void | 20 min |
| 5.5 Error handling | Display validation errors in status bar | 15 min |

### 6. SalesListPage QA Enhancements

| Task | Description | Estimate |
|---|---|---|
| 6.1 Add Edit button | Open NewSaleForm in edit mode for existing sale | 30 min |
| 6.2 Add Delete/Void buttons | Void or delete sale from list (with permissions) | 30 min |
| 6.3 Add filters | Date range, customer, status filters | 45 min |
| 6.4 Add export | Export to CSV/Excel | 30 min |
| 6.5 Unit tests | Test filters, sorting, paging | 30 min |

### 7. Scenario Test Templates

| Task | Description | Estimate |
|---|---|---|
| 7.1 Hold/Resume scenario | Draft → Hold → Resume → Complete | 30 min |
| 7.2 Return scenario | Sale → Return → Verify stock increment | 30 min |
| 7.3 Void scenario | Sale → Void (with/without payment) | 30 min |
| 7.4 Insufficient stock | Try to sell more than available | 15 min |
| 7.5 Invalid draft | Try to resume expired/invalid SaleTemp | 15 min |

---

## Total Estimated Effort: ~20 hours (5 days @ 4h/day)

**Acceptance Criteria for Sprint Completion:**
- NewSaleForm can: create sale, hold draft, resume draft, complete sale, return sale, void sale
- SalesListPage can: view, edit (via NewSaleForm), void, filter sales
- All workflows have unit tests covering success/failure paths
- Stock and payment quantities correctly update/reverse in all scenarios
- Audit trail records all mutations (create, hold, resume, return, void)

**Exit Criteria for M2 (Sales ready):**
- Sprint 1 tasks completed and verified
- Sales hardening scenarios tested with stock/payment reconciliation
- Parity matrix updated: SalesListPage → Implemented—needs QA → Verified; NewSaleForm → Partial → Verified