using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MySqlConnector;
using MMNextPOS.Application;
using MMNextPOS.Application.Services;
using MMNextPOS.Domain.Models;
using MMNextPOS.Infrastructure.Repositories;
using Xunit;

namespace MMNextPOS.Infrastructure.Tests
{
    /// <summary>
    /// End-to-end integration tests for <see cref="SalesService.CreateSaleAsync"/> against a
    /// real MySQL testcontainer. These tests close the R3 (schema/migration) and R4
    /// (transaction rollback) risk-register evidence gaps by asserting what the
    /// Phase 2 service-layer changes actually do in MySQL — atomic stock decrement,
    /// stock-movement audit row, auto-generated invoice number, customer outstanding
    /// for credit sales, audit log inside the transaction, and full rollback on failure.
    /// </summary>
    [Collection(nameof(MySqlContainerCollection))]
    public class SalesServiceIntegrationTests
    {
        private readonly MySqlContainerFixture _fixture;

        public SalesServiceIntegrationTests(MySqlContainerFixture fixture)
        {
            _fixture = fixture;
        }

        private async Task<(int productId, int customerId)> SeedProductAndCustomerAsync(
            int initialStock, decimal price, string sku = "TEST-SKU", string customerCode = "TEST-CUST")
        {
            await using var conn = new MySqlConnection(_fixture.ConnectionString);
            await conn.OpenAsync();

            // Seed a customer (no FK dependencies required for a customer).
            const string insertCustomer = @"
INSERT INTO Customers (Code, Name, Address, Phone, Email, IsActive, IsDeleted)
VALUES (@Code, @Name, '1 Test St', '555-0001', 'cust@test.local', 1, 0);
SELECT LAST_INSERT_ID();";
            var customerId = (int)await conn.ExecuteScalarAsync<long>(
                insertCustomer, new { Code = customerCode, Name = "Test Customer" });

            // Seed a product. The Sales table doesn't FK to Products, so we can skip Category.
            const string insertProduct = @"
INSERT INTO Products (Sku, Name, Price, StockQuantity, IsActive, IsDeleted)
VALUES (@Sku, @Name, @Price, @Stock, 1, 0);
SELECT LAST_INSERT_ID();";
            var productId = (int)await conn.ExecuteScalarAsync<long>(
                insertProduct, new { Sku = sku, Name = "Test Product", Price = price, Stock = initialStock });

            return (productId, customerId);
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
        public async Task CreateSaleAsync_HappyPath_WritesAllSideEffects()
        {
            // Arrange — fresh DB, seeded product with 100 units and a customer
            await _fixture.ResetDatabaseAsync();
            await _fixture.ServiceProvider.GetRequiredService<DatabaseInitializer>().InitializeAsync();
            var (productId, customerId) = await SeedProductAndCustomerAsync(initialStock: 100, price: 10.00m);

            using var scope = _fixture.ServiceProvider.CreateScope();
            var salesService = scope.ServiceProvider.GetRequiredService<ISalesService>();
            var sale = new Sale { CustomerId = customerId, SaleDate = DateTime.UtcNow, TotalAmount = 30m };
            var details = new List<SaleDetail>
            {
                new SaleDetail { ProductId = productId, Quantity = 3, UnitPrice = 10.00m }
            };

            // Act
            var created = await salesService.CreateSaleAsync(sale, details);

            // Assert — sale row
            Assert.NotEqual(0, created.Id);
            var salesRows = await CountAsync("SELECT COUNT(*) FROM Sales WHERE Id = @Id", new { created.Id });
            Assert.Equal(1, salesRows);

            // Assert — sale details
            var saleDetailCount = await CountAsync(
                "SELECT COUNT(*) FROM SaleDetails WHERE SaleId = @Id", new { created.Id });
            Assert.Equal(1, saleDetailCount);

            // Assert — products.StockQuantity decremented (100 - 3 = 97)
            var newStock = await ScalarAsync<int>(
                "SELECT StockQuantity FROM Products WHERE Id = @Id", new { Id = productId });
            Assert.Equal(97, newStock);

            // Assert — stock movement header (MovementType = 'Sale')
            // MySQL 8.0 forbids LIMIT inside an IN subquery, so look up the movement id
            // by joining on StockMovementDetails.
            var movementType = await ScalarAsync<string?>(@"
SELECT sm.MovementType
FROM StockMovements sm
INNER JOIN StockMovementDetails smd ON smd.StockMovementId = sm.Id
WHERE smd.ProductId = @Id
LIMIT 1", new { Id = productId });
            Assert.Equal("Sale", movementType);

            // Assert — exactly one StockMovementDetails row for this sale/product, quantity = 3
            var movementDetails = await QueryAsync<(int ProductId, int Quantity)>(
                "SELECT ProductId, Quantity FROM StockMovementDetails WHERE ProductId = @Id",
                new { Id = productId });
            Assert.Single(movementDetails);
            Assert.Equal((productId, 3), movementDetails[0]);

            // Assert — invoice auto-generated with INV-YYYY-NNNNNN format
            var invoice = await QueryAsync<(string InvoiceNo, int SaleId, int CustomerId, decimal AmountDue, string Status)>(
                "SELECT InvoiceNo, SaleId, CustomerId, AmountDue, Status FROM Invoices WHERE SaleId = @Id",
                new { created.Id });
            Assert.Single(invoice);
            var (invoiceNo, saleId, custId, amountDue, status) = invoice[0];
            Assert.Equal(created.Id, saleId);
            Assert.Equal(customerId, custId);
            Assert.Equal(30m, amountDue);
            Assert.Equal("Active", status);
            Assert.StartsWith($"INV-{DateTime.UtcNow.Year}-", invoiceNo);
            Assert.Equal("000001", invoiceNo.Substring(invoiceNo.Length - 6));

            // Assert — InvoiceSequences row exists and LastValue = 1
            var seqLast = await ScalarAsync<long>(
                "SELECT LastValue FROM InvoiceSequences WHERE Year = @Year AND Prefix = 'INV'",
                new { Year = DateTime.UtcNow.Year });
            Assert.Equal(1L, seqLast);

            // Assert — customer outstanding row created for credit sale
            var outstanding = await QueryAsync<(int CustomerId, int SaleId, decimal DebitAmount, decimal CreditAmount, decimal Balance, string Status)>(
                "SELECT CustomerId, SaleId, DebitAmount, CreditAmount, Balance, Status FROM CustomerOutstandings WHERE SaleId = @Id",
                new { created.Id });
            Assert.Single(outstanding);
            Assert.Equal((customerId, created.Id, 30m, 0m, 30m, "Open"), outstanding[0]);

            // Assert — sale audit row in ChangeDateLogs, written BEFORE commit
            var saleAudit = await QueryAsync<(string EntityName, int EntityId, string Action)>(
                "SELECT EntityName, EntityId, Action FROM ChangeDateLogs WHERE EntityName = 'Sale' AND EntityId = @Id",
                new { created.Id });
            Assert.Single(saleAudit);
            Assert.Equal(("Sale", created.Id, "Create"), saleAudit[0]);
        }

        [Fact]
        public async Task CreateSaleAsync_DuplicateLines_AggregatesAndWritesOneMovement()
        {
            // Arrange
            await _fixture.ResetDatabaseAsync();
            await _fixture.ServiceProvider.GetRequiredService<DatabaseInitializer>().InitializeAsync();
            var (productId, customerId) = await SeedProductAndCustomerAsync(initialStock: 100, price: 5.00m);

            using var scope = _fixture.ServiceProvider.CreateScope();
            var salesService = scope.ServiceProvider.GetRequiredService<ISalesService>();

            var sale = new Sale { CustomerId = customerId, SaleDate = DateTime.UtcNow, TotalAmount = 0m };
            // Two lines for the same product: qty 2 + qty 3 = 5.
            var details = new List<SaleDetail>
            {
                new SaleDetail { ProductId = productId, Quantity = 2, UnitPrice = 5.00m },
                new SaleDetail { ProductId = productId, Quantity = 3, UnitPrice = 5.00m }
            };

            // Act
            var created = await salesService.CreateSaleAsync(sale, details);

            // Assert — products.StockQuantity decremented by aggregated 5
            var newStock = await ScalarAsync<int>(
                "SELECT StockQuantity FROM Products WHERE Id = @Id", new { Id = productId });
            Assert.Equal(95, newStock);

            // Assert — only one StockMovementDetails row for the product
            var movementDetails = await CountAsync(
                "SELECT COUNT(*) FROM StockMovementDetails WHERE ProductId = @Id", new { Id = productId });
            Assert.Equal(1, movementDetails);
        }

        [Fact]
        public async Task CreateSaleAsync_InsufficientStock_RollsBackAllWrites()
        {
            // Arrange
            await _fixture.ResetDatabaseAsync();
            await _fixture.ServiceProvider.GetRequiredService<DatabaseInitializer>().InitializeAsync();
            var (productId, customerId) = await SeedProductAndCustomerAsync(initialStock: 1, price: 5.00m);

            using var scope = _fixture.ServiceProvider.CreateScope();
            var salesService = scope.ServiceProvider.GetRequiredService<ISalesService>();
            var sale = new Sale { CustomerId = customerId, SaleDate = DateTime.UtcNow, TotalAmount = 10m };
            var details = new List<SaleDetail>
            {
                new SaleDetail { ProductId = productId, Quantity = 2, UnitPrice = 5.00m }
            };

            // Act + Assert exception
            await Assert.ThrowsAsync<InsufficientStockException>(
                () => salesService.CreateSaleAsync(sale, details));

            // Assert — no side effects: stock unchanged
            var newStock = await ScalarAsync<int>(
                "SELECT StockQuantity FROM Products WHERE Id = @Id", new { Id = productId });
            Assert.Equal(1, newStock);

            // No sale written
            var saleCount = await CountAsync("SELECT COUNT(*) FROM Sales");
            Assert.Equal(0, saleCount);

            // No sale details
            var saleDetailCount = await CountAsync("SELECT COUNT(*) FROM SaleDetails");
            Assert.Equal(0, saleDetailCount);

            // No stock movement
            var movementCount = await CountAsync("SELECT COUNT(*) FROM StockMovements");
            Assert.Equal(0, movementCount);

            // No invoice
            var invoiceCount = await CountAsync("SELECT COUNT(*) FROM Invoices");
            Assert.Equal(0, invoiceCount);

            // No customer outstanding
            var outstandingCount = await CountAsync("SELECT COUNT(*) FROM CustomerOutstandings");
            Assert.Equal(0, outstandingCount);

            // No sale audit row
            var auditCount = await CountAsync(
                "SELECT COUNT(*) FROM ChangeDateLogs WHERE EntityName = 'Sale' AND Action = 'Create'");
            Assert.Equal(0, auditCount);
        }

        [Fact]
        public async Task CreateSaleAsync_AuditFailure_RollsBackAllWrites()
        {
            // Arrange — build a fresh service provider that replaces IAuditService
            // with a throwing decorator. The throw happens inside InvoiceService.AddAsync's
            // audit step, after the INSERT but inside the active transaction.
            await _fixture.ResetDatabaseAsync();
            await _fixture.ServiceProvider.GetRequiredService<DatabaseInitializer>().InitializeAsync();
            var (productId, customerId) = await SeedProductAndCustomerAsync(initialStock: 50, price: 10.00m);

            // Build a new service provider that registers IAuditService as a throwing
            // decorator. The wrapping is needed because InvoiceService.AddAsync (called
            // inside CreateSaleAsync) calls _auditService.LogAsync, and we want the
            // exception to fire inside the active transaction so we can prove the
            // whole sale is rolled back.
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddApplication(_fixture.Configuration);
            // Remove the production IAuditService registration (if any) and add the
            // throwing decorator in its place.
            var existing = services.Where(s => s.ServiceType == typeof(IAuditService)).ToList();
            foreach (var e in existing) services.Remove(e);
            services.AddScoped<IAuditService>(sp => new ThrowingAuditService(
                new AuditService(sp.GetRequiredService<IChangeDateLogRepository>()),
                new InvalidOperationException("audit DB down (forced)")));
            using var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var salesService = scope.ServiceProvider.GetRequiredService<ISalesService>();

            var sale = new Sale { CustomerId = customerId, SaleDate = DateTime.UtcNow, TotalAmount = 30m };
            var details = new List<SaleDetail>
            {
                new SaleDetail { ProductId = productId, Quantity = 3, UnitPrice = 10.00m }
            };

            // Act + Assert — exception propagates out of CreateSaleAsync
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => salesService.CreateSaleAsync(sale, details));

            // Assert — every side effect rolled back
            var newStock = await ScalarAsync<int>(
                "SELECT StockQuantity FROM Products WHERE Id = @Id", new { Id = productId });
            Assert.Equal(50, newStock);

            var saleCount = await CountAsync("SELECT COUNT(*) FROM Sales");
            Assert.Equal(0, saleCount);
            var saleDetailCount = await CountAsync("SELECT COUNT(*) FROM SaleDetails");
            Assert.Equal(0, saleDetailCount);
            var movementCount = await CountAsync("SELECT COUNT(*) FROM StockMovements");
            Assert.Equal(0, movementCount);
            var invoiceCount = await CountAsync("SELECT COUNT(*) FROM Invoices");
            Assert.Equal(0, invoiceCount);
            var outstandingCount = await CountAsync("SELECT COUNT(*) FROM CustomerOutstandings");
            Assert.Equal(0, outstandingCount);

            // The throwing decorator never delegated, so the ChangeDateLogs has zero
            // rows for this sale attempt. (Other change-logs from migration
            // bootstrap or unrelated inserts should not exist either since we
            // reset the database.)
            var auditCount = await CountAsync("SELECT COUNT(*) FROM ChangeDateLogs");
            Assert.Equal(0, auditCount);
        }

        [Fact]
        public async Task CreateSaleAsync_NoOutstandingForZeroTotalSale()
        {
            // Arrange — sale with TotalAmount = 0 (e.g. free issue, fully-discounted
            // promotion). The Phase 2 rule in SalesService.CreateSaleAsync is:
            //   if (CustomerId > 0 && TotalAmount > 0m) → add CustomerOutstanding
            // A zero-total sale skips the outstanding row even when CustomerId is set.
            await _fixture.ResetDatabaseAsync();
            await _fixture.ServiceProvider.GetRequiredService<DatabaseInitializer>().InitializeAsync();
            var (productId, customerId) = await SeedProductAndCustomerAsync(initialStock: 50, price: 5.00m);

            using var scope = _fixture.ServiceProvider.CreateScope();
            var salesService = scope.ServiceProvider.GetRequiredService<ISalesService>();
            var sale = new Sale { CustomerId = customerId, SaleDate = DateTime.UtcNow, TotalAmount = 0m };
            var details = new List<SaleDetail>
            {
                new SaleDetail { ProductId = productId, Quantity = 2, UnitPrice = 0m }
            };

            // Act
            var created = await salesService.CreateSaleAsync(sale, details);

            // Assert — sale was created, no customer outstanding
            Assert.NotEqual(0, created.Id);
            var saleCount = await CountAsync("SELECT COUNT(*) FROM Sales WHERE Id = @Id", new { created.Id });
            Assert.Equal(1, saleCount);
            var outstandingForCustomer = await CountAsync(
                "SELECT COUNT(*) FROM CustomerOutstandings WHERE CustomerId = @Id",
                new { Id = customerId });
            Assert.Equal(0, outstandingForCustomer);
        }

        [Fact]
        public async Task CreateSaleAsync_TwoConcurrentSalesForLastUnit_OnlyOneSucceeds()
        {
            // Arrange
            await _fixture.ResetDatabaseAsync();
            await _fixture.ServiceProvider.GetRequiredService<DatabaseInitializer>().InitializeAsync();
            var (productId, customerId) = await SeedProductAndCustomerAsync(initialStock: 1, price: 5.00m);

            // Act — two parallel sales, each requesting the only unit.
            // The UoW lifetime is scoped, so we create two independent scopes and
            // resolve a fresh ISalesService from each.
            var (s1, s2) = await RunTwoConcurrentAsync(productId, customerId);

            // Assert — exactly one succeeded and one threw InsufficientStockException
            var successes = new[] { s1, s2 }.Count(x => x.success);
            var failures = new[] { s1, s2 }.Count(x => !x.success);
            Assert.Equal(1, successes);
            Assert.Equal(1, failures);

            // Final stock is 0 (the loser never decremented it)
            var finalStock = await ScalarAsync<int>(
                "SELECT StockQuantity FROM Products WHERE Id = @Id", new { Id = productId });
            Assert.Equal(0, finalStock);
        }

        [Fact]
        public async Task CreateSaleAsync_InvoiceNumbers_AreMonotonicAcrossConcurrentSales()
        {
            // Arrange
            await _fixture.ResetDatabaseAsync();
            await _fixture.ServiceProvider.GetRequiredService<DatabaseInitializer>().InitializeAsync();
            var (productId, customerId) = await SeedProductAndCustomerAsync(initialStock: 100, price: 5.00m);

            // Act — 5 parallel sales, one unit each.
            var outcomes = await RunFiveConcurrentAsync(productId, customerId, quantityPerSale: 1);

            // Assert — all 5 succeeded
            Assert.All(outcomes, o => Assert.True(o.success, "expected all 5 concurrent sales to succeed"));

            // Assert — 5 distinct invoice numbers, all in the current year, all monotonic
            var invoices = await QueryAsync<string>(
                "SELECT InvoiceNo FROM Invoices ORDER BY InvoiceNo");
            Assert.Equal(5, invoices.Count);
            var distinct = new HashSet<string>(invoices);
            Assert.Equal(5, distinct.Count);
            var yearPrefix = $"INV-{DateTime.UtcNow.Year}-";
            Assert.All(invoices, no => Assert.StartsWith(yearPrefix, no));

            // Parse the 6-digit sequence and verify it is exactly 1..5
            var seqs = invoices
                .Select(no => int.Parse(no.Substring(no.Length - 6)))
                .OrderBy(x => x)
                .ToList();
            Assert.Equal(new[] { 1, 2, 3, 4, 5 }, seqs);
        }

        // ─── helpers for concurrent-sale tests ───

        private async Task<(bool success, int saleId, Exception? error)> RunOneAsync(
            int productId, int customerId, int quantity)
        {
            try
            {
                using var scope = _fixture.ServiceProvider.CreateScope();
                var salesService = scope.ServiceProvider.GetRequiredService<ISalesService>();
                var sale = new Sale { CustomerId = customerId, SaleDate = DateTime.UtcNow, TotalAmount = 5m * quantity };
                var details = new List<SaleDetail>
                {
                    new SaleDetail { ProductId = productId, Quantity = quantity, UnitPrice = 5.00m }
                };
                var created = await salesService.CreateSaleAsync(sale, details);
                return (true, created.Id, null);
            }
            catch (Exception ex)
            {
                return (false, 0, ex);
            }
        }

        private async Task<((bool success, int saleId, Exception? error) first, (bool success, int saleId, Exception? error) second)>
            RunTwoConcurrentAsync(int productId, int customerId)
        {
            var t1 = Task.Run(() => RunOneAsync(productId, customerId, 1));
            var t2 = Task.Run(() => RunOneAsync(productId, customerId, 1));
            await Task.WhenAll(t1, t2);
            return (t1.Result, t2.Result);
        }

        private async Task<List<(bool success, int saleId, Exception? error)>> RunFiveConcurrentAsync(
            int productId, int customerId, int quantityPerSale)
        {
            var tasks = Enumerable.Range(0, 5)
                .Select(_ => Task.Run(() => RunOneAsync(productId, customerId, quantityPerSale)))
                .ToArray();
            await Task.WhenAll(tasks);
            return tasks.Select(t => t.Result).ToList();
        }
    }
}
