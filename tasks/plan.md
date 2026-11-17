# Implementation Plan: Sprint 2 P1 Feature Completion

## Overview
This plan decomposes the remaining critical and medium-priority P1 items from Sprint 2, focusing on completing Price/Tax/Discount precedence, Currency Rounding edge cases, Customer Outstanding/AR logic, and documenting Print/Voucher output for the POS application. Implementation will proceed via vertical slicing, delivering small, verifiable feature paths first.

## Architecture Decisions
- **Layering**: Strict Domain → Infrastructure → Application → WinForms layering is maintained.
- **Data Access**: Dapper/MySQL via `IUnitOfWork` transaction boundary, ensuring transactional integrity for all write actions (e.g., creating sales/updates).
- **UI Threading**: All long-running/async operations in WinForms use `await` and `AsyncFormBase.RunAsync` to prevent UI blocking.
- **Auditing**: Audit-log rows are written per write action (e.g., sale creation, payment application).

## Task List

### Phase 1: Price/Discount Precedence (P1-6)
- [ ] Task 1: Implement Fixed-Amount Discount logic for a single line item.
- [ ] Task 2: Implement Percentage Discount logic (applied to unit price or line total).
- [ ] Task 3: Implement Tax Jurisdiction logic based on customer type.
- [ ] Task 4: Implement Scanner Price Override functionality.

### Checkpoint: Price/Discount Complete
- [ ] All P1-6 test cases pass: TC3.6.1–TC3.6.5
- [ ] Application builds clean and `dotnet format --verify-no-changes` passes.

### Phase 2: Currency Rounding (P1-7)
- [ ] Task 5: Implement repeated rounding test case (0.15+0.15+0.15) without skew.
- [ ] Task 6: Implement cash change calculation rounding (e.g., 15m - 10.99m = 4.01m).

### Checkpoint: Rounding Logic Verified
- [ ] Tasks 5 and 6 pass, verifying correct rounding behavior under summation and calculation.

### Phase 3: Customer Outstanding/AR (P1-8)
- [ ] Task 7: Implement logic for payment reducing existing outstanding balance.
- [ ] Task 8: Implement logic for overpayment creating a credit for future use.
- [ ] Task 9: Implement logic for customer account clearance (zero balance status).

### Checkpoint: AR Logic Complete
- [ ] Tasks 7-9 pass, verifying AR status transitions (Open, Cleared) and credit tracking.

### Phase 4: Print Output (P1-9)
- [ ] Task 10: Implement receipt generation/printing, verifying all critical fields (date, totals, items) are present via DevExpress XtraReport.
- [ ] Task 11: Implement Invoice printing format validation.
- [ ] Task 12: Implement PDF export, ensuring text is selectable, not rasterized.
- [ ] Task 13: Verify Myanmar Unicode displays correctly in print output.
- [ ] Task 14: Handle empty sale results gracefully during receipt generation.

### Checkpoint: P1 Complete
- [ ] All 28 P1 test cases are documented and verified.
- [ ] All acceptance criteria are met.
- [ ] Ready for full Sprint 2 review.

## Risks and Mitigations
| Risk | Impact | Mitigation |
|------|--------|------------|
| DevExpress dependency for P1-9 | Blocks completion of 5 test cases. | Document test structure clearly even if not runnable in current environment; focus on logic implementation first. |

## Open Questions
- [ ] Confirm which specific test suite/class (`SalesServiceTests.cs`) should receive these new tests.