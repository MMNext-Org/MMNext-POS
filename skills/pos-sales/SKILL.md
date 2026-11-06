---
name: pos-sales
description: POS sales workflow — create, hold, resume, return, void, and process payments following MMNext POS conventions.
---

# POS Sales Workflow

This skill covers the sales domain of MMNext POS, a .NET 8 WinForms point-of-sale application. Use it when implementing or reviewing sales-related features.

## Key Files and Services

- `src/MMNextPOS.Application/Services/SalesService.cs` — Core sale creation, returns, voids, line aggregation, rounding.
- `src/MMNextPOS.Application/Services/SalesReturnService.cs` — Sales return processing.
- `src/MMNextPOS.Application/Services/SaleReceiptService.cs` — Receipt generation.
- `src/MMNextPOS.WinForms/NewSaleForm.cs` — Sales entry UI (customer lookup, product search, line items, totals, save/hold/print, draft resume).
- `src/MMNextPOS.WinForms/SalesListPage.cs` — Sales listing UI.
- `src/MMNextPOS.WinForms/SalesHoldForm.cs` — Hold/unhold draft sales.

## Sales Workflow Steps

1. **Create Sale** — Customer lookup → product search → add line items → apply discounts/tax → compute totals → save.
2. **Hold/Resume** — Save draft sale → resume later → complete payment.
3. **Payment** — Process cash/card/credit → update customer outstanding → print receipt.
4. **Return** — Process sales return → reverse stock → issue credit/refund.
5. **Void** — Cancel sale → reverse stock movements → audit record.

## Key Rules

- Stock reservation uses `IProductRepository.TryDecrementStockAsync` (atomic `UPDATE ... WHERE StockQuantity >= @Quantity`).
- Duplicate lines are aggregated (sum Quantity, DiscountAmount, TaxAmount).
- Rounding uses `MidpointRounding.ToEven`.
- Invoice numbers are generated atomically via `IInvoiceNumberGenerator` (per-year `InvoiceSequences` table).
- Every write produces an audit record inside the `IUnitOfWork` transaction.
- Sensitive data must never be written to logs.

## When to Use

- Implementing new sales features or modifying existing ones.
- Reviewing sales-related code for correctness, stock safety, and audit compliance.
- Troubleshooting stock, payment, or invoice numbering issues.

## Verification

- `dotnet test` — run `SalesServiceTests` and related integration tests.
- `dotnet build MMNextPOS.slnx --configuration Release` — build gate.