## Task 1: Implement Fixed-Amount Discount logic for a single line item.

**Description:** Implement and test the functionality where a fixed monetary amount is subtracted from a single line item's total within a sale.

**Acceptance criteria:**
- [x] A fixed discount value is correctly subtracted from the line total.
- [x] The final line total reflects the fixed discount.

**Verification:**
- [ ] Tests pass: dotnet test tests/MMNextPOS.Application.Tests/MMNextPOS.Application.Tests.csproj --configuration Release --filter "FullyQualifiedName~SalesServiceTests"
- [ ] Build succeeds: dotnet build MMNextPOS.slnx --configuration Release
- [ ] Manual check: Verify calculation in a test setup with a fixed discount.

**Dependencies:** None

**Files likely touched:**
- `src/MMNextPOS.Domain/Models/SaleDetail.cs`
- `src/MMNextPOS.Application/SalesService.cs`
- `tests/MMNextPOS.Application.Tests/SalesServiceTests.cs`

**Estimated scope:** S
## Task 5: Implement Print Output generation for a completed sale.

**Description:** Implement a service and presentation logic to generate a structured, human-readable print/receipt output for a completed `Sale` entity, including line items, totals, tax breakdown, and customer info.

**Acceptance criteria:**
- [ ] A `SalePrintService` can generate a printable string/document from a `Sale` entity.
- [ ] The output accurately reflects final calculated totals, tax amounts, and applied discounts (including scanner overrides).
- [ ] The print output format meets basic POS requirements (e.g., line items clear, totals visible).

**Verification:**
- [ ] Tests pass: dotnet test tests/MMNextPOS.Application.Tests/MMNextPOS.Application.Tests.csproj --configuration Release --filter "FullyQualifiedName~SalesServiceTests"
- [ ] Build succeeds: dotnet build MMNextPOS.slnx --configuration Release
- [ ] Manual check: Print output generated from a mock completed sale object matches expected receipt format.

**Dependencies:** Task 1 (Discount logic)

**Files likely touched:**
- `src/MMNextPOS.Application/Services/ISalePrintService.cs`
- `src/MMNextPOS.Application/Services/SalePrintService.cs`
- `tests/MMNextPOS.Application.Tests/SalesServiceTests.cs`

**Estimated scope:** M