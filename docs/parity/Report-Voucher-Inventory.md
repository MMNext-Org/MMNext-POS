# MMNextPOS Report & Voucher Inventory

## Overview
This document provides a comprehensive inventory of all legacy reports (~96) and voucher layouts (~100) in MMNextPOS, grouped by operational priority and status.

## Priority Levels
- **P0 (Critical):** Must have functional implementation and QA verification
- **P1 (High):** Core business reports, essential for daily operations  
- **P2 (Medium):** Supporting reports, less frequently used
- **P3 (Low):** Reference or display-only reports

## Inventory Table
| Priority | Category | Legacy Report/Voucher | Current Status | Implementation File(s) | Notes |
|----------|----------|-------------------------|---------------|------------------------|-------|
| P0 | Sales | Sale Receipts | Implemented | `SaleReceiptReport.cs`, `DailySaleSummaryReport.cs` | Verify Bhutan/Myanmar rendering |
| P0 | Sales | Sale Invoices | Implemented | `SaleInvoiceReport.cs` | Verify formatting and pagination |
| P0 | Purchasing | Purchase Invoices | Implemented | `PurchaseInvoiceReport.cs` | Verify supplier data integration |
| P0 | Inventory | Stock List | Implemented | `StockListReport.cs` | Verify item count accuracy |
| P0 | Purchasing | Stock Movement | Implemented | `StockMovementReport.cs` | Verify location transfers |
| P0 | Financial | Cash Flow | Implemented | `CashFlowReport.cs` | Verify date range accuracy |
| P0 | Financial | Profit/Loss | Implemented | `ProfitLossReport.cs` | Verify calculation correctness |
| P0 | Financial | Outstanding | Implemented | `OutstandingReport.cs` | Verify date filtering |
| P0 | Dashboard | Quick Summary | Not Started | - | Implement P0 dashboard widget |
| P1 | Sales | Daily Sales Summary | Implemented | `DailySaleSummaryReport.cs` | Verify currency formatting |
| P1 | Purchasing | Purchase Order Summary | Missing | - | Implement status tracking |
| P2 | Inventory | Stock Adjustment Log | Missing | - | Implement adjustment tracking |
| P2 | Financial | Tax Summary | Implemented | `TaxSummaryReport.cs` | (exists but needs QA) |
| P3 | Reporting | Historical Comparison | Missing | - | Compare year-over-year data |
| P0 | Receipts | Payment Receipts | Missing | - | Implement payment receipt template |
| P1 | Vouchers | Journal Vouchers | Implemented | `JournalVoucher.cs`, `PaymentVoucher.cs` | Verify audit trail |
| P1 | Vouchers | Purchase Vouchers | Implemented | `PurchaseReceipt.cs`, `JournalVoucher.cs` | Verify approval workflow |
| P2 | Vouchers | Return Vouchers | Missing | - | Implement return voucher workflow |
| P2 | Vouchers | Credit Memo | Missing | - | Implement credit memo handling |

## Implementation Status

### ✅ Implemented (Verified in current baseline)
- **Sales Receipts**: `SaleReceiptReport.cs` (dev-pending localization)
- **Daily Sales Summary**: `DailySaleSummaryReport.cs` (P0 verified)
- **Purchase Invoices**: `PurchaseInvoiceReport.cs` (P0)
- **Stock List**: `StockListReport.cs` (P0)
- **Stock Movement**: `StockMovementReport.cs` (P0)
- **Cash Flow**: `CashFlowReport.cs` (P0)
- **Profit/Loss**: `ProfitLossReport.cs` (P0)
- **Outstanding**: `OutstandingReport.cs` (P0)
- **Barcode Labels**: `BarcodeLabelsReport.cs` (P0)
- **Journal Vouchers**: `JournalVoucher.cs`, `PaymentVoucher.cs` (P1)
- **Purchase Vouchers**: `PurchaseReceipt.cs`, `JournalVoucherService.cs` (P1)

### ⏳ Implementation in Progress
- **Sales Invoice** (High priority) - Need to implement full feature set
- **Dashboard Quick Summary**: Need to implement widgets in MainForm
- **Payment Receipts**: Need to implement payment-specific receipt template
- **Return Vouchers**: Need to implement return voucher workflow
- **Credit Memos**: Need to implement credit memo handling

### ❌ Missing (Require Implementation)
- **P0 Dashboard**: Quick summary widgets for MainForm
- **P0 Sales Invoice**: Complete implementation 
- **P0 Payment Receipts**: Payment-specific receipt template
- **P0 Return Vouchers**: Return workflow implementation
- **P0 Credit Memos**: Credit memo document generation

## Parameter Contracts
Each report requires a parameter contract with these fields:
1. **ReportName** (string): Name of the report to generate
2. **StartDate** (DateTime?): Start date for date range filters
3. **EndDate** (DateTime?): End date for date range filters
4. **LocationId** (int?): Location filter (optional)
5. **StatusFilter** (string): Optional status filter (e.g., "Open", "Closed")
6. **CurrencyCode** (string): Currency code for financial reports
7. **ShowPreview** (bool): Whether to show print preview

## Empty-Result Behavior
- Reports with no data should return a valid empty report with appropriate headers/footers
- No exceptions should be thrown for empty results
- Reports should include "No data available" message when empty

## Date/Timezone Rules
- All date parameters use UTC internally
- Display dates use user's local timezone
- Date ranges are inclusive (start and end dates included)
- Time component should be set to midnight for date-only filters

## Myanmar Font Handling
- All text fields must use `Myanmar3` font family
- Font size 8-9pt for body text, 10-11pt for headers
- Unicode rendering must be verified on clean Windows machines
- Font packaging requirements:
  - `Myanmar3` font family (or equivalent open-source equivalent)
  - Must work in grid, print preview, PDF, XLS, and physical print

## Print Settings
- Paper size: A4 (210 × 297 mm) for standard reports, A5 (148 × 210 mm) for receipts
- Margins: 10mm minimum for all reports
- Orientation: Portrait for receipts, Landscape for financial reports
- Printer selection: User-configurable with default set to "Microsoft Print to PDF"

## PDF/XLS Export Behavior
- **PDF**: Text-selectable (not rasterized), correct font embedding, layered structure
- **XLS**: Correct column structure, proper headers, cell formatting preserved
- Export naming convention: `{ReportName}_{YYYYMMDD_HHMMSS}.pdf` or `.xls`
- Export location: User-configurable with default to "Documents\Reports" folder

## Testing Requirements
- Unit tests for report generation with sample parameters
- Integration tests with real database seeding
- Myanmar rendering tests with Unicode characters
- Print preview tests for layout accuracy
- Export tests for PDF/XLS correctness

## Acceptance Criteria
1. All P0 reports must generate valid PDF/XLS files without errors
2. Reports must be printable with correct formatting and margins
3. Myanmar Unicode characters must render correctly in all contexts
4. Parameter validation must prevent invalid combinations
5. Empty results must not cause crashes
6. All changes must be verified against legacy reference outputs