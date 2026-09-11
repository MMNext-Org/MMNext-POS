using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MySqlConnector;
using MMNextPOS.Application.Services;
using MMNextPOS.Domain.Models;
using MMNextPOS.Infrastructure.Repositories;
using Xunit;

namespace MMNextPOS.Infrastructure.Tests
{
    /// <summary>
    /// End-to-end integration tests for <see cref="PurchaseService"/> against a
    /// real MySQL testcontainer. Mirrors the Phase 2b sales-test pattern:
    /// - Reset + re-init migrations per test via <see cref="MySqlContainerFixture"/>.
    /// - Inline SQL <c>INSERT</c> to seed master data (Supplier, Product) using the
    ///   fixture's connection string (avoids sharing a UoW transaction with the test).
    /// - Read-back assertions via fresh Dapper <c>SELECT</c> against the live MySQL.
    /// </summary>
    [Collection(nameof(MySqlContainerCollection))]
    public class PurchaseServiceIntegrationTests
    {
        private readonly MySqlContainerFixture _fixture;

        public PurchaseServiceIntegrationTests(MySqlContainerFixture fixture)
        {
            _fixture = fixture;
        }

        private async Task<(int productId, int supplierId)> SeedProductAndSupplierAsync(
            int initialStock, decimal price, string sku = "PUR-SKU", string supplierCode = "PUR-SUP")
        {
            await using var conn = new MySqlConnection(_fixture.ConnectionString);
            await conn.OpenAsync();

            const string insertSupplier = @"
INSERT INTO Suppliers (Code, Name, Address, Phone, Email, ContactPerson, TaxId, CreditLimit, PaymentTermDays, IsActive, IsDeleted)
VALUES (@Code, @Name, '1 Supplier St', '555-0002', 'sup@test.local', 'Test Supplier', 'TAX-001', 100000, 30, 1, 0);
SELECT LAST_INSERT_ID();";
            var supplierId = (int)await conn.ExecuteScalarAsync<long>(
                insertSupplier, new { Code = supplierCode, Name = "Test Supplier" });

            const string insertProduct = @"
INSERT INTO Products (Sku, Name, Price, StockQuantity, IsActive, IsDeleted)
VALUES (@Sku, @Name, @Price, @Stock, 1, 0);
SELECT LAST_INSERT_ID();";
            var productId = (int)await conn.ExecuteScalarAsync<long>(
                insertProduct, new { Sku = sku, Name = "Test Product", Price = price, Stock = initialStock });

            return (productId, supplierId);
        }

        private async Task<T?> ScalarAsync<T>(string sql, object? parameters = null)
        {
            await using var conn = new MySqlConnection(_fixture.ConnectionString);
            await conn.OpenAsync();
            return await conn.ExecuteScalarAsync<T?>(sql, parameters);
        }

        private async Task<int> CountAsync(string sql, object? parameters = null)
        {
            await using var conn = new MySqlConnection(_fixture.ConnectionString);
            await conn.OpenAsync();
            return await conn.ExecuteScalarAsync<int>(sql, parameters);
        }

        private async Task<IReadOnlyList<T>> QueryAsync<T>(string sql, object? parameters = null)
        {
            await using var conn = new MySqlConnection(_fixture.ConnectionString);
            await conn.OpenAsync();
            var rows = await conn.QueryAsync<T>(sql, parameters);
            return rows.ToList();
        }

        [Fact]
        public async Task CreatePurchaseAsync_HappyPath_WritesAllSideEffects()
        {
            // Arrange — fresh DB, seeded product (initial stock 50) and supplier
            await _fixture.ResetDatabaseAsync();
            await _fixture.ServiceProvider.GetRequiredService<DatabaseInitializer>().InitializeAsync();
            var (productId, supplierId) = await SeedProductAndSupplierAsync(initialStock: 50, price: 10.00m);

            using var scope = _fixture.ServiceProvider.CreateScope();
            var purchaseService = scope.ServiceProvider.GetRequiredService<IPurchaseService>();
            var purchase = new Purchase { SupplierId = supplierId, PurchaseDate = DateTime.UtcNow, TotalAmount = 0m };
            var details = new List<PurchaseDetail>
            {
                new PurchaseDetail { ProductId = productId, Quantity = 5, UnitPrice = 10.00m }
            };

            // Act
            var created = await purchaseService.CreatePurchaseWithDetailsAsync(purchase, details);

            // Assert — purchase row
            Assert.NotEqual(0, created.Id);
            var purchaseRows = await CountAsync("SELECT COUNT(*) FROM Purchases WHERE Id = @Id", new { created.Id });
            Assert.Equal(1, purchaseRows);

            // Assert — purchase details
            var detailCount = await CountAsync(
                "SELECT COUNT(*) FROM PurchaseDetails WHERE PurchaseId = @Id", new { created.Id });
            Assert.Equal(1, detailCount);

            // Assert — products.StockQuantity incremented (50 + 5 = 55)
            var newStock = await ScalarAsync<int>(
                "SELECT StockQuantity FROM Products WHERE Id = @Id", new { Id = productId });
            Assert.Equal(55, newStock);

            // Assert — stock movement header (MovementType = 'Purchase')
            // MySQL 8.0 forbids LIMIT inside IN subquery, so JOIN explicitly.
            var movementType = await ScalarAsync<string?>(@"
SELECT sm.MovementType
FROM StockMovements sm
INNER JOIN StockMovementDetails smd ON smd.StockMovementId = sm.Id
WHERE smd.ProductId = @Id
LIMIT 1", new { Id = productId });
            Assert.Equal("Purchase", movementType);

            // Assert — exactly one StockMovementDetails row for this purchase/product, quantity = 5
            var movementDetails = await QueryAsync<(int ProductId, int Quantity)>(
                "SELECT ProductId, Quantity FROM StockMovementDetails WHERE ProductId = @Id",
                new { Id = productId });
            Assert.Single(movementDetails);
            Assert.Equal((productId, 5), movementDetails[0]);

            // Assert — auto-generated purchase-order number with PUR-YYYY-NNNNNN format
            var poNo = await ScalarAsync<string?>(
                "SELECT InvoiceNo FROM Purchases WHERE Id = @Id", new { created.Id });
            Assert.StartsWith($"PUR-{DateTime.UtcNow.Year}-", poNo);
            Assert.Equal("000001", poNo!.Substring(poNo.Length - 6));

            // Assert — InvoiceSequences row exists for the PUR prefix with LastValue=1
            var seqLast = await ScalarAsync<long>(
                "SELECT LastValue FROM InvoiceSequences WHERE Year = @Year AND Prefix = 'PUR'",
                new { Year = DateTime.UtcNow.Year });
            Assert.Equal(1L, seqLast);

            // Assert — supplier outstanding row created for credit purchase (NetAmount=50, PaidAmount=0)
            var outstanding = await QueryAsync<(int SupplierId, int PurchaseId, decimal DebitAmount, decimal CreditAmount, decimal Balance, string Status)>(
                "SELECT SupplierId, PurchaseId, DebitAmount, CreditAmount, Balance, Status FROM SupplierOutstandings WHERE PurchaseId = @Id",
                new { created.Id });
            Assert.Single(outstanding);
            Assert.Equal((supplierId, created.Id, 0m, 50m, 50m, "Open"), outstanding[0]);

            // Assert — purchase audit row in ChangeDateLogs
            var purchaseAudit = await QueryAsync<(string EntityName, int EntityId, string Action)>(
                "SELECT EntityName, EntityId, Action FROM ChangeDateLogs WHERE EntityName = 'Purchase' AND EntityId = @Id",
                new { created.Id });
            Assert.Single(purchaseAudit);
            Assert.Equal(("Purchase", created.Id, "Create"), purchaseAudit[0]);
        }

        [Fact]
        public async Task CreatePurchaseAsync_ProductNotFound_RollsBackAllWrites()
        {
            // Arrange
            await _fixture.ResetDatabaseAsync();
            await _fixture.ServiceProvider.GetRequiredService<DatabaseInitializer>().InitializeAsync();
            var (_, supplierId) = await SeedProductAndSupplierAsync(initialStock: 10, price: 5.00m);

            using var scope = _fixture.ServiceProvider.CreateScope();
            var purchaseService = scope.ServiceProvider.GetRequiredService<IPurchaseService>();
            var purchase = new Purchase { SupplierId = supplierId, PurchaseDate = DateTime.UtcNow };
            var details = new List<PurchaseDetail>
            {
                // ProductId 9999 does not exist in the seeded Products table.
                new PurchaseDetail { ProductId = 9999, Quantity = 1, UnitPrice = 5.00m }
            };

            // Act + Assert exception
            await Assert.ThrowsAsync<ValidationException>(
                () => purchaseService.CreatePurchaseWithDetailsAsync(purchase, details));

            // Assert — no side effects: no purchase, no details, no movement, no invoice sequence bump, no outstanding, no audit
            var purchaseCount = await CountAsync("SELECT COUNT(*) FROM Purchases");
            Assert.Equal(0, purchaseCount);
            var purchaseDetailCount = await CountAsync("SELECT COUNT(*) FROM PurchaseDetails");
            Assert.Equal(0, purchaseDetailCount);
            var movementCount = await CountAsync("SELECT COUNT(*) FROM StockMovements");
            Assert.Equal(0, movementCount);
            var supplierOutstandingCount = await CountAsync("SELECT COUNT(*) FROM SupplierOutstandings");
            Assert.Equal(0, supplierOutstandingCount);
            var purchaseAuditCount = await CountAsync(
                "SELECT COUNT(*) FROM ChangeDateLogs WHERE EntityName = 'Purchase' AND Action = 'Create'");
            Assert.Equal(0, purchaseAuditCount);

            // The InvoiceSequences row for PUR was never created (the transaction rolled back before PUR-…-000001 was issued)
            var purSeqCount = await CountAsync(
                "SELECT COUNT(*) FROM InvoiceSequences WHERE Prefix = 'PUR'");
            Assert.Equal(0, purSeqCount);
        }

        [Fact]
        public async Task CreatePurchaseAsync_FullyPaid_NoSupplierOutstanding()
        {
            // Arrange
            await _fixture.ResetDatabaseAsync();
            await _fixture.ServiceProvider.GetRequiredService<DatabaseInitializer>().InitializeAsync();
            var (productId, supplierId) = await SeedProductAndSupplierAsync(initialStock: 50, price: 5.00m);

            using var scope = _fixture.ServiceProvider.CreateScope();
            var purchaseService = scope.ServiceProvider.GetRequiredService<IPurchaseService>();
            // NetAmount = 25 (5 qty * 5 price), PaidAmount = 25 (fully paid)
            var purchase = new Purchase { SupplierId = supplierId, PurchaseDate = DateTime.UtcNow, PaidAmount = 25m };
            var details = new List<PurchaseDetail>
            {
                new PurchaseDetail { ProductId = productId, Quantity = 5, UnitPrice = 5.00m }
            };

            // Act
            var created = await purchaseService.CreatePurchaseWithDetailsAsync(purchase, details);

            // Assert — purchase created, no supplier outstanding
            Assert.NotEqual(0, created.Id);
            var supplierOutstanding = await CountAsync(
                "SELECT COUNT(*) FROM SupplierOutstandings WHERE SupplierId = @Id",
                new { Id = supplierId });
            Assert.Equal(0, supplierOutstanding);
        }

        [Fact]
        public async Task ReceivePurchaseAsync_IncrementsStockAndWritesMovement()
        {
            // Arrange
            await _fixture.ResetDatabaseAsync();
            await _fixture.ServiceProvider.GetRequiredService<DatabaseInitializer>().InitializeAsync();
            var (productId, supplierId) = await SeedProductAndSupplierAsync(initialStock: 50, price: 10.00m);

            // Create the purchase first (so we have an Id to receive against).
            int purchaseId;
            int detailId1;
            int detailId2;
            using (var scope = _fixture.ServiceProvider.CreateScope())
            {
                var purchaseService = scope.ServiceProvider.GetRequiredService<IPurchaseService>();
                var purchase = new Purchase { SupplierId = supplierId, PurchaseDate = DateTime.UtcNow };
                var details = new List<PurchaseDetail>
                {
                    new PurchaseDetail { ProductId = productId, Quantity = 5, UnitPrice = 10.00m }
                };
                var created = await purchaseService.CreatePurchaseWithDetailsAsync(purchase, details);
                purchaseId = created.Id;
                detailId1 = (await QueryAsync<int>(
                    "SELECT Id FROM PurchaseDetails WHERE PurchaseId = @Id ORDER BY Id LIMIT 1", new { Id = purchaseId })).Single();
                detailId2 = detailId1; // only one detail row
            }

            // Reset the stock to a known state (the create flow already incremented it 50→55)
            await ScalarAsync<int>("UPDATE Products SET StockQuantity = 50 WHERE Id = @Id", new { Id = productId });
            var movementCountBefore = await CountAsync("SELECT COUNT(*) FROM StockMovements");

            // Act — receive 3 of the 5 units ordered
            using (var scope = _fixture.ServiceProvider.CreateScope())
            {
                var purchaseService = scope.ServiceProvider.GetRequiredService<IPurchaseService>();
                await purchaseService.ReceivePurchaseAsync(purchaseId, 1, new List<(int, int)>
                {
                    (detailId1, 3)
                });
            }

            // Assert — stock incremented by 3 (50 → 53)
            var newStock = await ScalarAsync<int>(
                "SELECT StockQuantity FROM Products WHERE Id = @Id", new { Id = productId });
            Assert.Equal(53, newStock);

            // Assert — one new StockMovement row was created (the receive's movement)
            var movementCountAfter = await CountAsync("SELECT COUNT(*) FROM StockMovements");
            Assert.Equal(movementCountBefore + 1, movementCountAfter);

            // Assert — purchase status updated to "PartiallyReceived"
            var status = await ScalarAsync<string?>(
                "SELECT Status FROM Purchases WHERE Id = @Id", new { Id = purchaseId });
            Assert.Equal("PartiallyReceived", status);

            // Assert — audit row written
            var auditCount = await CountAsync(
                "SELECT COUNT(*) FROM ChangeDateLogs WHERE EntityName = 'Purchase' AND Action = 'Receive' AND EntityId = @Id",
                new { Id = purchaseId });
            Assert.Equal(1, auditCount);
        }
    }
}
