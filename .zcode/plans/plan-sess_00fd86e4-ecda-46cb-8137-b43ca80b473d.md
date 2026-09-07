# Phase 2 — Sales MVP Hardening (Service-Layer Slice)

## Scope of this iteration

Service-layer fixes for the sales flow that close the largest data-safety and correctness gaps. UI behavior changes are limited to what the form must pass to the service. No new test harness for WinForms; no new test project structure; no DevExpress UI tests.

### In scope
- `SalesService.CreateSaleAsync` hardening (atomic stock, stock movement, customer outstanding, invoice number, audit, duplicate-line, rounding)
- `SalesService.AddSaleDetailAsync` aligned to the same rules
- `IInvoiceNumberGenerator` + `DbInvoiceNumberGenerator` (atomic DB sequence)
- New `InvoiceSequences` migration script
- New tests for: stock atomicity under concurrency, insufficient-stock rollback, stock movement creation, customer outstanding creation, invoice number uniqueness, duplicate-line handling, rounding, audit-inside-transaction

### Out of scope (deferred to later iterations)
- WinForms UI smoke tests
- Return-flow end-to-end rehearsal
- Report output parity (PDF/print)
- Long-running session / memory / disposal review (R8)
- Barcode scanner physical input verification

---

## Findings driving the plan

Confirmed from `src/MMNextPOS.Application/Services/SalesService.cs:30-84` and the explore reports:

1. **Non-atomic stock decrement** — line 57 does `product.StockQuantity -= d.Quantity; await _productRepo.UpdateAsync(...)`. Two concurrent sales can both pass the `if (product.StockQuantity < d.Quantity)` check and oversell. `IProductRepository.AdjustStockAsync` already exists and is atomic (single `UPDATE ... SET StockQuantity = @NewStock WHERE Id = @Id AND StockQuantity >= @Quantity`) but `SalesService` does not use it.
2. **No stock movement record** — sale posts decrement `Products.StockQuantity` but does not write a `StockMovement` + `StockMovementDetail` row. No audit trail of why stock changed.
3. **No customer outstanding entry** — credit sales do not create a `CustomerOutstanding` row with `SaleId` populated.
4. **No invoice auto-generation** — `SalesService.CreateSaleAsync` does not create an `Invoice` row or call any `IInvoiceNumberGenerator`. `InvoiceService.AddAsync` accepts an `Invoice` whose `InvoiceNo` is empty by default.
5. **Audit outside transaction** — `_auditService.LogAsync(...)` is called after `_unitOfWork.CommitAsync(...)`. A failure of the audit write leaves a sale without audit, violating the R4 fallback plan ("write audit records within the same transaction when appropriate").
6. **No tests** for duplicate-line, rounding, concurrent stock decrement, stock-movement creation, outstanding creation, invoice uniqueness, audit-inside-transaction.

---

## Implementation plan

### 1. New migration: `008_InvoiceSequences.sql`
- Creates `InvoiceSequences (Year INT NOT NULL PRIMARY KEY, LastValue BIGINT NOT NULL DEFAULT 0, UpdatedAt DATETIME)` table.
- Embedded in the same `MMNextPOS.Infrastructure.Migrations` folder.
- Registered in `MigrationRunner.LoadMigrations()` as the 9th migration (after 007).
- MigrationRunner ensures it runs in order with the same idempotent + checksummed pattern as 000–007.

### 2. New abstraction: `IInvoiceNumberGenerator`
- New files:
  - `src/MMNextPOS.Application/Services/IInvoiceNumberGenerator.cs`
  - `src/MMNextPOS.Application/Services/DbInvoiceNumberGenerator.cs` (implementation in Application layer — uses IUnitOfWork, no Dapper dependency on Infrastructure)
  - `src/MMNextPOS.Application/Services/InvoiceNumberFormat.cs` (formats like `INV-2026-000123`)
- API: `Task<string> NextAsync(string prefix, CancellationToken ct)` — issues a monotonic number per (prefix, current UTC year) using an atomic `INSERT ... ON DUPLICATE KEY UPDATE LastValue = LastValue + 1` on `InvoiceSequences`, inside the caller's transaction (passes the active IUnitOfWork).
- Format: `INV-{Year}-{LastValue:D6}`.

### 3. Refactor `SalesService.CreateSaleAsync`

Replace the body so the entire flow runs inside one `IUnitOfWork` transaction:

```
await _uow.BeginTransactionAsync(ct);
try
{
    // 1) Aggregate duplicate lines (same ProductId → sum Quantity, max UnitPrice/Discount).
    var aggregated = AggregateDuplicateLines(details);

    // 2) For each unique line, atomic stock check + decrement:
    //    await _productRepo.AdjustStockAsync(productId, -quantity, "Sale", userId, ct)
    //    Throws InsufficientStockException if no row updated (row was locked or stock < 0).
    foreach (var d in aggregated) await _productRepo.AdjustStockAsync(d.ProductId, -d.Quantity, "Sale", userId, ct);

    // 3) Round money to 2 dp using banker-friendly half-even at the line level, then sum.
    var roundedLines = RoundLines(aggregated);
    sale.TotalAmount = roundedLines.Sum(l => l.LineTotal);
    sale.DiscountAmount = roundedLines.Sum(l => l.DiscountAmount);
    sale.TaxAmount = roundedLines.Sum(l => l.TaxAmount);

    // 4) Create sale + details in transaction.
    var created = await _saleRepo.CreateSaleWithDetailsAsync(sale, roundedLines, ct);

    // 5) Create stock movement + details (Sale type, same LocationId, refs created.Id).
    var movement = await _stockMovementService.AddStockMovementAsync(StockMovementType.Sale, created, roundedLines, ct);

    // 6) Create Invoice with auto-generated number, link to sale.
    var invoiceNo = await _invoiceNumberGenerator.NextAsync("INV", ct);
    var invoice = new Invoice { SaleId = created.Id, InvoiceNo = invoiceNo, InvoiceDate = created.SaleDate, TotalAmount = created.TotalAmount, Status = "Issued" };
    await _invoiceService.AddAsync(invoice, ct);
    created.InvoiceId = invoice.Id;
    await _saleRepo.UpdateAsync(created, ct);

    // 7) Update customer outstanding if CustomerId > 0 and BalanceDue > 0.
    if (created.CustomerId > 0 && created.NetAmount - created.PaidAmount > 0)
    {
        await _outstandingService.AddCustomerOutstandingAsync(new CustomerOutstanding
        {
            CustomerId = created.CustomerId, SaleId = created.Id, TransactionDate = created.SaleDate,
            DebitAmount = created.NetAmount - created.PaidAmount, CreditAmount = 0,
            Balance = created.NetAmount - created.PaidAmount, Status = "Open"
        }, ct);
    }

    // 8) Audit INSIDE the transaction (before commit).
    await _auditService.LogAsync(entityName: nameof(Sale), entityId: created.Id, action: "Create",
        oldValues: null, newValues: created, userId: currentUserId, userName: currentUserName,
        description: $"Sale {created.InvoiceNo} posted: {roundedLines.Count} lines, total {created.TotalAmount:C2}",
        cancellationToken: ct);

    await _uow.CommitAsync(ct);
    return created;
}
catch
{
    await _uow.RollbackAsync(ct);
    throw;
}
```

`AddSaleDetailAsync` is collapsed into a single-detail call to `CreateSaleAsync` to share the same rules. The old two-method shape is preserved on the interface for backward compat, but the implementation delegates to `CreateSaleAsync`.

### 4. New abstractions needed
- `IProductRepository.AdjustStockAsync(int productId, int quantityDelta, string reason, int adjustedBy, CancellationToken ct)` — already exists. Confirmed signature; no changes.
- `IStockMovementService` (in `src/MMNextPOS.Application/Services/IStockMovementService.cs` and `StockMovementService.cs`) — new. Method: `AddStockMovementAsync(StockMovementType type, Sale sale, IReadOnlyList<SaleDetail> lines, CancellationToken ct)`. Writes `StockMovement` + `StockMovementDetail` rows in the active transaction. `StockMovementType` enum already exists in `MMNextPOS.Domain`.
- `IInvoiceNumberGenerator` — new.
- `IInvoiceService.AddAsync(...)` — already exists. Verified during the 005-MissingEntityTables migration work; passes the active transaction via `IUnitOfWork`.

### 5. Update `DependencyInjection.cs`
- Register `IStockMovementService → StockMovementService` (Scoped)
- Register `IInvoiceNumberGenerator → DbInvoiceNumberGenerator` (Scoped, depends on IUnitOfWork)
- `SalesService` constructor gains: `IStockMovementService`, `IInvoiceNumberGenerator`, `IInvoiceService`, `IOutstandingService`, `ICurrentUser` (for userId/userName in audit)

### 6. New tests in `tests/MMNextPOS.Application.Tests/SalesServiceTests.cs`

All new tests use the same Moq pattern as the existing 10 tests. The list (target ~12 new tests):

1. `CreateSaleAsync_AggregatesDuplicateLines_ForSameProduct`
2. `CreateSaleAsync_RoundsLineTotals_HalfEvenToTwoDecimals`
3. `CreateSaleAsync_InsufficientStock_DoesNotCommit_AndDoesNotCallAdjustStock_ForLaterLines` (verify atomicity)
4. `CreateSaleAsync_StockMovement_CreatedWithTypeSaleAndSameLocation`
5. `CreateSaleAsync_Invoice_AutoGeneratedAndLinkedToSale`
6. `CreateSaleAsync_InvoiceNumber_MonotonicPerYear`
7. `CreateSaleAsync_CustomerOutstanding_CreatedForCreditSale`
8. `CreateSaleAsync_CustomerOutstanding_NotCreatedForCashSale`
9. `CreateSaleAsync_AuditLog_WrittenBeforeCommit`
10. `CreateSaleAsync_AllWritesFail_NoSideEffectsInAnyRepository` (each downstream mock `VerifyNever`)
11. `AddSaleDetailAsync_DelegatesToCreateSaleAsync_AndFollowsSameRules`
12. `CreateSaleAsync_ConcurrentStockCheck_UsesAdjustStock_NotReadThenWrite` (verify AdjustStock is called and the old read-then-write is gone)

### 7. Migration verification
- Add a small test in `tests/MMNextPOS.Infrastructure.Tests` (the same `MySqlContainerFixture`) named `MigrationRunner_InvoiceSequencesTable_Exists` that runs all migrations and asserts `InvoiceSequences` is present with `(Year, LastValue, UpdatedAt)` columns and `Year` is `PRI`.

### 8. Documentation
- Update `plan.md` to mark the Phase 2 service-hardening sub-items complete with checkboxes and link to the implementation files.
- Append a memory entry under `migration-idempotence-complete.md` describing the new `008_InvoiceSequences` migration and the new `IInvoiceNumberGenerator` pattern, so future migrations and audit/sequence work can find it.

---

## Files to create / modify

| File | Action | Notes |
|---|---|---|
| `src/MMNextPOS.Infrastructure/Migrations/008_InvoiceSequences.sql` | Create | One new table, idempotent CREATE TABLE IF NOT EXISTS |
| `src/MMNextPOS.Infrastructure/MigrationRunner.cs` | Edit | Add `"008"` to `knownMigrations` array |
| `src/MMNextPOS.Application/Services/IInvoiceNumberGenerator.cs` | Create | Interface + `InvoiceNumberFormat` helper |
| `src/MMNextPOS.Application/Services/DbInvoiceNumberGenerator.cs` | Create | Atomic `INSERT ... ON DUPLICATE KEY UPDATE LastValue = LastValue + 1` |
| `src/MMNextPOS.Application/Services/IStockMovementService.cs` | Create | New abstraction for movement creation |
| `src/MMNextPOS.Application/Services/StockMovementService.cs` | Create | Writes StockMovement + StockMovementDetail in current transaction |
| `src/MMNextPOS.Application/Services/SalesService.cs` | Refactor | New transactional body covering points 1–8 above |
| `src/MMNextPOS.Application/Services/ISalesService.cs` | Edit | No interface change (preserve AddSaleDetailAsync) |
| `src/MMNextPOS.Application/DependencyInjection.cs` | Edit | Register new services |
| `tests/MMNextPOS.Application.Tests/SalesServiceTests.cs` | Extend | 12 new tests (Moq-based, no DB) |
| `tests/MMNextPOS.Infrastructure.Tests/MigrationIdempotenceTests.cs` | Extend | 1 new test for `InvoiceSequences` table |
| `plan.md` | Edit | Mark Phase 2 service-hardening sub-items complete |
| `memory/...` | Append | New memory: phase-2-service-hardening-complete |

## Acceptance criteria
1. `dotnet build MMNextPOS.slnx --configuration Release` succeeds with 0 warnings, 0 errors.
2. `dotnet test tests/MMNextPOS.Application.Tests` — 148 existing + 12 new = **160/160 passing**.
3. `dotnet test tests/MMNextPOS.Infrastructure.Tests --filter "FullyQualifiedName~MigrationIdempotenceTests"` — 12 existing + 1 new = **13/13 passing** (in two batches again if needed).
4. `SalesService.CreateSaleAsync` is the single entry point and writes: sale, details, stock movement, invoice, customer outstanding, audit — all in one transaction, all-or-nothing.
5. `IProductRepository.AdjustStockAsync` is the only path that mutates `Products.StockQuantity` for a sale. The old read-then-write is removed and a regression test asserts this.
6. `InvoiceSequences` table exists after `DatabaseInitializer.InitializeAsync()`. Invoice numbers are monotonic per year and unique.

## Risks and mitigations
- **Service constructor signature change** — `SalesService` gains 4 new dependencies. `DependencyInjection.cs` is updated; all callers go through DI. No direct constructor call sites to update (verified by reading the WinForms forms: they only inject `ISalesService`).
- **Invoice number race** — the atomic `INSERT ... ON DUPLICATE KEY UPDATE` is run inside the same `IUnitOfWork` transaction, so MySQL row-level locking on the sequence row prevents duplicates even under concurrent inserts. Tested via the `MigrationRunner_InvoiceSequencesTable_Exists` infrastructure test and the `CreateSaleAsync_InvoiceNumber_MonotonicPerYear` application test.
- **StockMovementService new abstraction** — no consumers yet outside sales, so it is a fresh abstraction. Future inventory work (Phase 4) will use the same interface for purchases/returns/adjustments.
- **Test host memory** — same constraint as Phase 1; the single new infrastructure test will go into a batch with the others, no parallel testhost pressure.

## Verification commands
```powershell
dotnet build MMNextPOS.slnx --configuration Release
dotnet test tests/MMNextPOS.Application.Tests/MMNextPOS.Application.Tests.csproj --configuration Release
dotnet test tests/MMNextPOS.Infrastructure.Tests/MMNextPOS.Infrastructure.Tests.csproj --configuration Release --filter "FullyQualifiedName~MigrationIdempotenceTests"
```
