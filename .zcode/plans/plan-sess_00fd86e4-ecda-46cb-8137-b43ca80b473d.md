# Phase 3 — Purchasing Hardening (Service-Layer Slice)

## Goal

Mirror the Phase 2 hardening on the purchasing flow: atomic stock increase + stock movement audit + auto-created supplier outstanding + audit-in-tx. Closes the R3 (schema) and R4 (transaction rollback) gaps for purchases.

## Scope

### In scope
- `IStockMovementService.AddPurchaseMovementAsync(Purchase, IReadOnlyList<PurchaseDetail>, int? createdByUserId, CancellationToken)` — new method that creates a `StockMovement` (MovementType="Purchase", SupplierId set) + one `StockMovementDetail` per line, all within the caller's active transaction.
- `IProductRepository.TryIncrementStockAsync(int productId, int quantity, int adjustedBy, string reason, CancellationToken)` — atomic counterpart to the existing `TryDecrementStockAsync` for sales. Single `UPDATE Products SET StockQuantity = StockQuantity + @Quantity WHERE Id = @ProductId`. Returns `true` if the row exists. (For purchases, stock cannot go negative; the only failure mode is product-not-found, so no "insufficient stock" semantics.)
- `PurchaseService` constructor gains `IUnitOfWork`, `IStockMovementService`, `IOutstandingService`, `IInvoiceNumberGenerator`. Two methods are rewritten to use the transaction-wrapped, audit-in-tx pattern:
  - `CreatePurchaseWithDetailsAsync(Purchase, IEnumerable<PurchaseDetail>, CancellationToken)` — wraps insert + atomic stock increase + stock movement + supplier outstanding + audit in one transaction. Mirrors `SalesService.CreateSaleAsync`.
  - `ReceivePurchaseAsync(int, int, IEnumerable<(int,int)>, CancellationToken)` — wraps receive-detail updates + atomic stock increase + stock movement + audit in one transaction. (Today this method calls `AdjustStockAsync` outside any transaction.)
  - `CreatePurchaseReturnAsync` and `ReceivePurchaseReturnAsync` are NOT in scope for this iteration (deferred to a later purchase-return hardening pass).
- `IInvoiceNumberGenerator.NextAsync("PUR", CancellationToken)` reuse — purchases use a separate prefix `PUR-YYYY-NNNNNN` so they don't compete with the `INV-` sequence.
- `ISupplierService` is **not** modified. The test seeder inserts supplier rows directly via SQL.
- `tests\MMNextPOS.Application.Tests\PurchaseServiceTests.cs` — 6 new Moq tests mirroring the Phase 2 sales-tests structure (deduplication, atomicity, supplier-outstanding creation, walk-in-equivalent case, audit-in-tx, `AddPurchaseDetailAsync` delegation).
- `tests\MMNextPOS.Infrastructure.Tests\PurchaseServiceIntegrationTests.cs` — 4 integration tests against a real MySQL testcontainer, mirroring the Phase 2b sales-test pattern.
- `ThrowingAuditService` from Phase 2b is reused for the audit-in-tx test.
- DI updates in `DependencyInjection.cs`: register `IProductRepository` already exists; no new registrations needed. The new `TryIncrementStockAsync` is on the existing `IProductRepository`.

### Out of scope (deferred to later iterations)
- `PurchaseReturnService` and `PurchaseReturnDetailService` hardening (deferred to a later Phase 3 pass; the user did not ask for returns in this round).
- `IPaymentService` and `IPaymentVoucherService` (Phase 3 next slice).
- `IExpenseService` and `IExpenseTypeService` (Phase 3 last slice).
- WinForms UI tests.
- Schema migrations (no new tables needed; the existing migrations cover all required tables).

## Files to create / modify

| File | Action | Notes |
|------|--------|-------|
| `src\MMNextPOS.Application\Services\IStockMovementService.cs` | Edit | Add `AddPurchaseMovementAsync(Purchase, IReadOnlyList<PurchaseDetail>, int?, CancellationToken)` |
| `src\MMNextPOS.Application\Services\StockMovementService.cs` | Edit | Implement the new method; reuses the same `_movementRepo` and `_detailRepo` already injected. |
| `src\MMNextPOS.Infrastructure\Repositories\IProductRepository.cs` | Edit | Add `TryIncrementStockAsync(int productId, int quantity, int adjustedBy, string reason, CancellationToken)` |
| `src\MMNextPOS.Infrastructure\Repositories\ProductRepository.cs` | Edit | Implement the new method as a single `UPDATE Products SET StockQuantity = StockQuantity + @Quantity WHERE Id = @ProductId`. |
| `src\MMNextPOS.Application\Services\PurchaseService.cs` | Edit | Add `IUnitOfWork`, `IStockMovementService`, `IOutstandingService`, `IInvoiceNumberGenerator` to the constructor. Rewrite `CreatePurchaseWithDetailsAsync` and `ReceivePurchaseAsync` to use the transaction pattern. |
| `src\MMNextPOS.Application\DependencyInjection.cs` | (no change) | Existing registrations are sufficient. |
| `tests\MMNextPOS.Application.Tests\PurchaseServiceTests.cs` | Edit | Update existing tests' constructor to provide the new mocks; add 6 new tests mirroring the Phase 2 sales-tests. |
| `tests\MMNextPOS.Infrastructure.Tests\PurchaseServiceIntegrationTests.cs` | Create | 4 integration tests against a real MySQL testcontainer. |
| `memory/phase-3-purchasing-hardening-complete.md` | Create | Memory entry recording the changes, the test pattern, and the difference vs Phase 2 sales. |

## Implementation details

### `ProductRepository.TryIncrementStockAsync`

```sql
UPDATE Products
SET StockQuantity = StockQuantity + @Quantity,
    LastAdjustment = @LastAdjustment,
    AdjustedBy = @AdjustedBy,
    AdjustmentReason = @AdjustmentReason,
    IsActive = 1
WHERE Id = @ProductId
```

Returns `rows == 1`. (No "insufficient stock" failure mode — incrementing is always possible if the product exists.)

### `StockMovementService.AddPurchaseMovementAsync`

Same pattern as `AddSaleMovementAsync`:
- Create `StockMovement` with `MovementType = "Purchase"`, `SupplierId = purchase.SupplierId`, `LocationId = purchase.LocationId`, `MovementDate = purchase.PurchaseDate`, `Reason = "Purchase #..."`.
- Create one `StockMovementDetail` per `PurchaseDetail` row with `UnitCost = UnitPrice`, `LineTotal = UnitPrice * Quantity`, `Notes = PurchaseDetail.Notes`.

### `PurchaseService.CreatePurchaseWithDetailsAsync` (new flow)

```csharp
await _uow.BeginTransactionAsync(ct);
try
{
    // 1) Aggregate duplicate lines (ProductId → sum Quantity, last UnitPrice wins)
    var aggregated = AggregateDuplicateLines(details);

    // 2) Round money per line (banker's half-even)
    var rounded = RoundLines(aggregated);
    purchase.NetAmount = rounded.Sum(l => l.UnitPrice * l.Quantity);
    if (purchase.PurchaseDate == default) purchase.PurchaseDate = DateTime.UtcNow;
    if (string.IsNullOrWhiteSpace(purchase.Status)) purchase.Status = "Active";

    // 3) Atomic stock increase per line
    foreach (var d in rounded)
    {
        var ok = await _productRepo.TryIncrementStockAsync(d.ProductId, d.Quantity, 1, "Purchase", ct);
        if (!ok) throw new ValidationException($"Product {d.ProductId} not found.");
    }

    // 4) Persist purchase header + details
    var created = await _repo.AddAsync(purchase, ct);
    foreach (var d in rounded) { d.PurchaseId = created.Id; await _detailRepo.AddAsync(d, ct); }

    // 5) Stock movement
    await _stockMovementService.AddPurchaseMovementAsync(created, rounded, createdByUserId: null, ct);

    // 6) Auto-generated purchase-order number (PUR-YYYY-NNNNNN)
    var poNo = await _invoiceNumberGenerator.NextAsync("PUR", ct);
    created.InvoiceNo = poNo;
    await _repo.UpdateAsync(created, ct);

    // 7) Supplier outstanding if NetAmount > PaidAmount
    var owed = created.NetAmount - created.PaidAmount;
    if (created.SupplierId > 0 && owed > 0m)
    {
        await _outstandingService.AddSupplierOutstandingAsync(new SupplierOutstanding
        {
            SupplierId = created.SupplierId,
            PurchaseId = created.Id,
            TransactionDate = created.PurchaseDate,
            DebitAmount = 0m,           // we owe them money; this is a payable, not a receivable
            CreditAmount = owed,         // credit here means "amount recorded as owed to supplier"
            Balance = owed,
            Description = $"Auto from purchase {created.InvoiceNo}",
            Status = "Open"
        }, ct);
    }

    // 8) Audit INSIDE the transaction
    await _auditService.LogAsync(nameof(Purchase), created.Id, "Create", null, new { created.Id, poNo, NetAmount = created.NetAmount, LineCount = rounded.Count }, null, null, $"Purchase {poNo} posted: {rounded.Count} lines, total {created.NetAmount:C2}", ct);

    await _uow.CommitAsync(ct);
    return created;
}
catch
{
    await _uow.RollbackAsync(ct);
    throw;
}
```

**Note on outstanding semantics:** `SupplierOutstanding` uses `DebitAmount` for "amount paid to supplier", `CreditAmount` for "amount owed to supplier" — opposite to `CustomerOutstanding`. The test will assert the right column based on the model's existing convention.

### `PurchaseService.ReceivePurchaseAsync` (new flow)

```csharp
await _uow.BeginTransactionAsync(ct);
try
{
    var purchase = await _repo.GetByIdAsync(purchaseId, ct) ?? throw new KeyNotFoundException(...);
    if (purchase.Status == "Received" || purchase.Status == "Cancelled") throw ...;

    var details = (await _detailRepo.GetAllAsync(ct)).Where(d => d.PurchaseId == purchaseId).ToList();

    foreach (var item in receivedItems)
    {
        var detail = details.FirstOrDefault(d => d.Id == item.detailId) ?? continue;
        detail.ReceivedQuantity = item.receivedQuantity;
        await _detailRepo.UpdateAsync(detail, ct);
        await _productRepo.TryIncrementStockAsync(detail.ProductId, item.receivedQuantity, 1, "Purchase Receive", ct);
    }

    purchase.Status = details.All(d => d.ReceivedQuantity >= d.Quantity) ? "Received" : "PartiallyReceived";
    await _repo.UpdateAsync(purchase, ct);
    await _stockMovementService.AddPurchaseMovementAsync(purchase, details, null, ct);
    await _auditService.LogAsync(nameof(Purchase), purchase.Id, "Receive", null, purchase, null, null, $"Received purchase {purchase.InvoiceNo}", ct);

    await _uow.CommitAsync(ct);
    return purchase;
}
catch
{
    await _uow.RollbackAsync(ct);
    throw;
}
```

### `PurchaseService` private helpers (mirror `SalesService`)

- `AggregateDuplicateLines(IList<PurchaseDetail>)` — internal, sum qty + last unit price per ProductId
- `RoundLines(IList<PurchaseDetail>)` — internal, `Math.Round(UnitPrice, 2, ToEven)`

## Test plan

### Unit tests in `PurchaseServiceTests.cs` (Moq, 6 new)

1. `CreatePurchaseWithDetailsAsync_HappyPath_PersistsAndUpdatesStock` — verifies purchase + details are inserted and `TryIncrementStockAsync` is called per line.
2. `CreatePurchaseWithDetailsAsync_ProductNotFound_ThrowsValidationException` — `TryIncrementStockAsync` returns false → exception, transaction rolled back, no purchase persisted.
3. `CreatePurchaseWithDetailsAsync_AggregatesDuplicateLines_ForSameProduct` — same ProductId with different qtys summed.
4. `CreatePurchaseWithDetailsAsync_RoundsLineTotals_HalfEvenToTwoDecimals` — UnitPrice `0.125` rounded to `0.12`.
5. `CreatePurchaseWithDetailsAsync_SupplierOutstanding_CreatedForCreditPurchase` — PaidAmount = 0, SupplierOutstanding row created with Balance = NetAmount.
6. `CreatePurchaseWithDetailsAsync_FullyPaid_DoesNotCreateOutstanding` — PaidAmount = NetAmount, no outstanding row.
7. `CreatePurchaseWithDetailsAsync_AuditFailure_RollsBackEntireTransaction` — uses `ThrowingAuditService` (imported from Phase 2b).
8. `ReceivePurchaseAsync_PartialReceipt_IncrementsStockAndWritesMovement` — verify the receive flow uses `TryIncrementStockAsync` (atomic) and creates a stock movement with `MovementType = "Purchase"`.

### Integration tests in `PurchaseServiceIntegrationTests.cs` (real MySQL, 4 new)

1. `CreatePurchaseAsync_HappyPath_WritesAllSideEffects` — after a valid purchase: 1 Purchases row, 1+ PurchaseDetails rows, Products.StockQuantity incremented, 1 StockMovements header (MovementType=Purchase) + 1 StockMovementDetails per line, 1 SupplierOutstandings row, 1 ChangeDateLogs row, 1 InvoiceSequences row with LastValue=1 for the `PUR` prefix.
2. `CreatePurchaseAsync_ProductNotFound_RollsBackAllWrites` — TryIncrementStockAsync returns false → exception. No rows in any of: Purchases, PurchaseDetails, StockMovements, SupplierOutstandings, ChangeDateLogs.
3. `CreatePurchaseAsync_FullyPaid_NoSupplierOutstanding` — PaidAmount = NetAmount, no outstanding row.
4. `ReceivePurchaseAsync_IncrementsStockAndWritesMovement` — receive goods, stock increases, stock movement created, audit row written.

## Acceptance criteria

1. `dotnet build MMNextPOS.slnx --configuration Release` succeeds with 0 errors, 0 warnings.
2. **160 + 6 = 166** application tests passing (148 originals + 12 Phase 2 sales + 6 new Phase 3 purchase).
3. **13 + 4 = 17** integration tests verified individually (13 Phase 1+2 migration tests + 4 new Phase 3 purchase tests).
4. `PurchaseService.CreatePurchaseWithDetailsAsync` and `ReceivePurchaseAsync` use `IUnitOfWork.BeginTransactionAsync/CommitAsync/RollbackAsync` (verified by the integration tests).
5. The atomic `TryIncrementStockAsync` is the only path that mutates `Products.StockQuantity` for a purchase (a regression test asserts `UpdateAsync` is never called for the happy path).
6. `IStockMovementService.AddPurchaseMovementAsync` is the only path that creates `StockMovement` rows of `MovementType = "Purchase"` from the purchase service.

## Risks and mitigations

- **Testhost memory ceiling (the Phase 1 / Phase 2b WSL2/Docker constraint)**: The 4 new integration tests share the existing `MySqlContainerFixture`, so per-test memory is similar to what we already saw work. If the testhost crashes when running all 4 in a batch, fall back to running them individually.
- **`SupplierOutstanding` semantics (Debit vs Credit are inverted vs `CustomerOutstanding`)**: The current model has `DebitAmount` for "paid to supplier" and `CreditAmount` for "owed to supplier" — opposite to `CustomerOutstanding`. The plan above records an "amount owed" outstanding with `CreditAmount = owed, Balance = owed`. Tests will assert the right values; if a future session wants to align the two models, that's a separate refactor.
- **The existing `PurchaseService.AddAsync` method is unchanged**: It's a "header-only insert" used by UI flows that may not need the new transaction pattern. The plan leaves it alone to avoid scope creep.
- **No schema migrations needed**: All required columns and FKs already exist in the schema from migrations 001-008. The `SupplierOutstanding.PurchaseId` FK to `Purchases(Id)` is already `ON DELETE SET NULL` so a purchase delete doesn't cascade-suppress the outstanding.
- **Per-file commits**: I will commit each change as its own atomic commit, mirroring the Phase 2 pattern that prevented silent reverts.

## Verification commands

```powershell
dotnet build MMNextPOS.slnx --configuration Release
dotnet test tests\MMNextPOS.Application.Tests\MMNextPOS.Application.Tests.csproj --configuration Release --no-build
dotnet test tests\MMNextPOS.Infrastructure.Tests\MMNextPOS.Infrastructure.Tests.csproj --configuration Release --no-build --filter "FullyQualifiedName~PurchaseServiceIntegrationTests.CreatePurchaseAsync_HappyPath_WritesAllSideEffects"
```
