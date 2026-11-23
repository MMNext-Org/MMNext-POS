# Report-Voucher Inventory

**Status**: Initial draft - needs review and validation against legacy inventory

## Legend
- **Status**: 
  - `Implemented` - Report/voucher already exists and is functional
  - `Missing` - No implementation found
  - `Partial` - Implementation exists but needs hardening/validation
- **Priority**:
  - `P0` = Critical (must have)
  - `P1` = High
  - `P2` = Medium
  - `P3` = Low
- **Notes**: Additional context, verification status, parity evidence links, etc.

## Sales Reports (P0)
| Legacy Report | Status | Priority | Notes |
|---------------|--------|----------|-------|
| Sale Receipt | Implemented (SaleReceiptReport.cs) | P0 | A4/A5 printing, PDF/XLS export, Myanmar font |
| Sale Invoice | Missing | P0 | |
| Purchase Invoice | Implemented (PurchaseInvoiceReport.cs) | P0 | |
| Daily Sale Summary | Implemented (DailySaleSummaryReport.cs) | P0 | Must verify empty-result handling |
| Sales History | Implemented (SaleHistoryReport.cs) | P1 | |
| Sales Return | Missing | P0 | |
| Sales Credit Memo | Missing | P2 | |
| Sales Credit Note | Missing | P2 | |
| Sales Void | Missing | P0 | |

## Purchase Reports (P0)
| Legacy Report | Status | Priority | Notes |
|---------------|--------|----------|-------|
| Purchase Invoice | Implemented (PurchaseInvoiceReport.cs) | P0 | |
| Purchase History | Missing | P1 | |
| Purchase Hold | Missing | P1 | |
| Purchase Return | Implemented (PurchaseReturnReport.cs) | P0 | |
| Purchase Return Invoice | Missing | P1 | |
| Purchase Credit Note | Missing | P2 | |

## Inventory Reports (P0)
| Legacy Report | Status | Priority | Notes |
|---------------|--------|----------|-------|
| Product List | Implemented (ProductsListPage.cs) | P0 | |
| Stock Movement List | Implemented (StockMovementReport.cs) | P0 | |
| Stock Transfer List | Implemented (StockTransferReport.cs) | P0 | |
| Stock Adjustment List | Implemented (StockMovementReport.cs) | P0 | |
| Assembly List | Implemented (AssemblyReport.cs) | P1 | |
| Assembly Detail | Implemented (AssemblyReport.cs) | P1 | |
| Assembly Detail by Location | Missing | P2 | |
| Inventory Value Summary | Missing | P1 | |
| Product Availability | Missing | P1 | |
| Stock by Location | Missing | P2 | |
| Expired Items Report | Missing | P2 | |
| Reorder Point Analysis | Missing | P1 | |
| Stock Ageing Report | Missing | P1 | |
| Serial Number Tracking | Missing | P2 | |
| Barcode Scan History | Missing | P2 | |
| Barcode Print Report | Missing | P2 | |

## Financial Reports (P0)
| Legacy Report | Status | Priority | Notes |
|---------------|--------|----------|-------|
| Cash Flow Report | Implemented (CashFlowReport.cs) | P0 | |
| Profit & Loss Report | Implemented (ProfitLossReport.cs) | P0 | |
| Outstanding Report | Implemented (OutstandingReport.cs) | P0 | |
| Reorder Report | Missing | P1 | |
| Low Stock Alert Report | Missing | P1 | |
| Customer Credit Balance | Missing | P2 | |
| Supplier Credit Balance | Missing | P2 | |
| Tax Summary Report | Missing | P1 | |
| Payment Summary Report | Missing | P1 | |
| Sales by Product Summary | Missing | P1 | |
| Sales by Customer Summary | Missing | P1 | |
| Purchases by Supplier Summary | Missing | P1 | |
| Sales by Date Summary | Missing | P1 | |
| Purchases by Date Summary | Missing | P1 | |

## Outstanding/Payments (P0)
| Legacy Report | Status | Priority | Notes |
|---------------|--------|----------|-------|
| Customer Outstanding List | Implemented (OutstandingListPage.cs) | P0 | |
| Supplier Outstanding List | Implemented (OutstandingListPage.cs) | P0 | |
| Customer Payment History | Missing | P1 | |
| Supplier Payment History | Missing | P1 | |
| Overdue Accounts Report | Missing | P1 | |
| Aging Report | Missing | P1 | |
| Write-off History | Missing | P2 | |

## Starman Reports (P0)
| Legacy Report | Status | Priority | Notes |
|---------------|--------|----------|-------|
| Star Cash Flow Report | Implemented (StarCashFlowReport.cs) | P0 | |
| Star Profit & Loss Report | Implemented (StarProfitLossReport.cs) | P0 | |
| Star Stock Balance Report | Implemented (StarStockBalanceReport.cs) | P0 | |
| Star Reorder Report | Implemented (StarReorderReport.cs) | P0 | |
| Star Outstanding Report | Implemented (StarOutstandingReport.cs) | P0 | |
| Star Sales Transfer Report | Implemented (StarSalePriceTransfer.cs) | P1 | |
| Star Stock Transfer Received | Implemented (StarStockTransferReceived.cs) | P1 | |

## Print Vouchers (P0)
| Voucher Type | Status | Priority | Notes |
|---------------|--------|----------|-------|
| A4 Receipt (Standard) | Missing | P0 | |
| A5 Receipt (Short) | Missing | P0 | |
| Slip Receipt | Missing | P0 | |
| Barcode Label Print | Missing | P0 | |
| Barcode Print Preview | Missing | P1 | |
| Receipt PDF Export | Missing | P0 | |
| Receipt XLS Export | Missing | P0 | |
| Voucher Barcode Integration | Missing | P1 | |

## Dashboard Widgets (P0)
| Widget | Status | Priority | Notes |
|--------|--------|----------|-------|
| Quick Summary (Today's Sales) | Missing | P0 | |
| Cash Flow Summary | Missing | P0 | |
| Outstanding Summary | Missing | P0 | |
| Low Stock Alerts | Missing | P0 | |
| Daily/Weekly/Monthly Trend | Missing | P1 | |
| Financial View (Balance Sheet) | Missing | P1 | |
| Inventory View | Missing | P1 | |
| Sales by Product | Missing | P1 | |
| Purchases by Supplier | Missing | P1 | |

## Print Settings (P1)
| Setting | Status | Priority | Notes |
|---------|--------|----------|-------|
| Paper Size (A4, A5, Slip) | Missing | P1 | |
| Printer Selection | Missing | P1 | |
| Margins (Min 10mm) | Missing | P1 | |
| Orientation (Portrait/Landscape) | Missing | P1 | |
| PDF Export Settings (Text-selectable) | Missing | P1 | |
| XLS Export Settings (Column widths, formatting) | Missing | P1 | |

## Myanmar Localization (P1)
| Item | Status | Priority | Notes |
|------|--------|----------|-------|
| Zawgyi Font (Myanmar3) | Missing | P1 | Must verify rendering in grids, print preview, PDF, XLS |
| Unicode/Zawgyi Support in UI Controls | Missing | P1 | Ensure all text fields support Zawgyi |
| PDF Font Embedding for Myanmar | Missing | P1 | Ensure PDFs embed Zawgyi font correctly |
| XLS Myanmar Font Support | Missing | P1 | Ensure XLS reports display Zawgyi correctly |

## Integration Points (P2)
| Integration | Status | Priority | Notes |
|-----------|--------|----------|-------|
| Report Service Parameter Contracts | Implemented | P2 | Verify all existing report services follow contract pattern |
| Report Service Empty-Result Handling | Implemented (partial) | P2 | Must verify all reports handle empty results gracefully |
| Dashboard Widget Data Sources | Implemented (UI) | P2 | Must verify data sources are correct and live |
| Report Service Test Coverage | Partial | P2 | Need more unit/integration tests for edge cases |

## Verification Checklist (Run before marking as Done)
- [ ] All P0 reports have parameter contracts defined
- [ ] All P0 reports have empty-result behavior verified
- [ ] All P0 reports pass PDF/XLS export verification
- [ ] All P0 reports verified for print settings (paper size, margins)
- [ ] Myanmar localization verified for all P0 reports and dashboard
- [ ] All P0 reports have test coverage (unit + integration)
- [ ] Report inventory document is complete and reviewed

## Next Steps
1. Review this inventory with stakeholders
2. Prioritize remaining missing reports by business impact
3. Implement highest-priority missing reports
4. Verify all export/print behaviors
5. Update parity evidence docs
6. Finalize QA sign-off

*Document maintained by: [Your Name] - Last updated: 2026-09-04*