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