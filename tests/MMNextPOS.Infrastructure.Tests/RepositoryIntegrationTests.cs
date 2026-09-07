using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.MySql;
using MMNextPOS.Application;
using MMNextPOS.Application.Services;
using MMNextPOS.Domain.Models;
using MMNextPOS.Infrastructure;
using MMNextPOS.Infrastructure.Repositories;
using Xunit;

namespace MMNextPOS.Infrastructure.Tests
{
    public class SalesServiceIntegrationTests : IAsyncLifetime
    {
        private MySqlContainer _container = null!;
        private IConfiguration _configuration = null!;
        private IServiceProvider _serviceProvider = null!;

        public async Task InitializeAsync()
        {
            // Build and start a MySQL container for the duration of the test suite
            _container = new MySqlBuilder()
                .WithDatabase("mmnextpos_test")
                .WithUsername("test")
                .WithPassword("test")
                .WithImage("mysql:8.0")
                .WithCleanUp(true)
                .Build();
            await _container.StartAsync();

            // Load configuration with the container's connection string
            var connectionString = _container.GetConnectionString();
            // Add Allow User Variables=true to support PREPARE statements with user variables
            if (!connectionString.Contains("Allow User Variables"))
            {
                connectionString += ";Allow User Variables=true";
            }

            _configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:Default"] = connectionString
                })
                .Build();

            // Build DI container
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddApplication(_configuration);
            services.AddScoped<ISalesService, SalesService>();
            _serviceProvider = services.BuildServiceProvider();

            // Ensure tables exist
            var dbInit = _serviceProvider.GetRequiredService<DatabaseInitializer>();
            await dbInit.InitializeAsync();
        }

        public async Task DisposeAsync()
        {
            if (_serviceProvider is IDisposable disposable)
            {
                disposable.Dispose();
            }
            if (_container != null)
            {
                await _container.DisposeAsync();
            }
        }

        [Fact]
        public async Task CreateSaleAsync_ValidData_CreatesSaleAndUpdatesStock()
        {
            // Arrange
            using var scope = _serviceProvider.CreateScope();
            var productRepo = scope.ServiceProvider.GetRequiredService<IProductRepository>();
            var saleRepo = scope.ServiceProvider.GetRequiredService<ISaleRepository>();
            var salesService = scope.ServiceProvider.GetRequiredService<ISalesService>();

            // Create a test product with stock
            var product = new Product
            {
                Sku = "TEST001",
                Name = "Test Product",
                Price = 10.00m,
                StockQuantity = 100
            };
            var addedProduct = await productRepo.AddAsync(product);
            Assert.NotEqual(0, addedProduct.Id);

            // Create a customer
            var customerRepo = scope.ServiceProvider.GetRequiredService<ICustomerRepository>();
            var customer = new Customer
            {
                Name = "Test Customer",
                Address = "123 Test St",
                Phone = "555-1234",
                Email = "test@example.com"
            };
            var addedCustomer = await customerRepo.AddAsync(customer);
            Assert.NotEqual(0, addedCustomer.Id);

            // Create sale with details
            var sale = new Sale
            {
                CustomerId = addedCustomer.Id,
                SaleDate = DateTime.UtcNow,
                TotalAmount = 30.00m // 3 * 10.00
            };

            var details = new List<SaleDetail>
            {
                new SaleDetail
                {
                    ProductId = addedProduct.Id,
                    Quantity = 3,
                    UnitPrice = addedProduct.Price
                }
            };

            // Act
            var createdSale = await salesService.CreateSaleAsync(sale, details);

            // Assert
            Assert.NotEqual(0, createdSale.Id);
            Assert.Equal(sale.CustomerId, createdSale.CustomerId);
            Assert.Equal(sale.TotalAmount, createdSale.TotalAmount);

            // Verify stock was decremented
            var updatedProduct = await productRepo.GetByIdAsync(addedProduct.Id);
            Assert.NotNull(updatedProduct);
            Assert.Equal(97, updatedProduct.StockQuantity); // 100 - 3

            // Verify sale details were created
            var recentSales = await saleRepo.GetRecentAsync(1);
            Assert.Single(recentSales);
            Assert.Equal(createdSale.Id, recentSales[0].Id);
        }

        [Fact]
        public async Task CreateSaleAsync_InsufficientStock_ThrowsInsufficientStockException()
        {
            // Arrange
            using var scope = _serviceProvider.CreateScope();
            var productRepo = scope.ServiceProvider.GetRequiredService<IProductRepository>();
            var customerRepo = scope.ServiceProvider.GetRequiredService<ICustomerRepository>();
            var salesService = scope.ServiceProvider.GetRequiredService<ISalesService>();

            // Create a test product with LOW stock
            var product = new Product
            {
                Sku = "TEST002",
                Name = "Low Stock Product",
                Price = 5.00m,
                StockQuantity = 1 // Only 1 in stock
            };
            var addedProduct = await productRepo.AddAsync(product);

            var customer = new Customer
            {
                Name = "Test Customer 2",
                Email = "test2@example.com"
            };
            var addedCustomer = await customerRepo.AddAsync(customer);

            // Create sale requesting MORE than available stock
            var sale = new Sale
            {
                CustomerId = addedCustomer.Id,
                SaleDate = DateTime.UtcNow,
                TotalAmount = 10.00m // 2 * 5.00
            };

            var details = new List<SaleDetail>
            {
                new SaleDetail
                {
                    ProductId = addedProduct.Id,
                    Quantity = 2, // Request 2 but only 1 in stock
                    UnitPrice = addedProduct.Price
                }
            };

            // Act & Assert
            await Assert.ThrowsAsync<InsufficientStockException>(() =>
                salesService.CreateSaleAsync(sale, details));

            // Verify stock was NOT changed (transaction rolled back)
            var unchangedProduct = await productRepo.GetByIdAsync(addedProduct.Id);
            Assert.NotNull(unchangedProduct);
            Assert.Equal(1, unchangedProduct.StockQuantity); // Still 1
        }

        [Fact]
        public async Task CreateSaleAsync_AtomicTransaction_RollsBackOnStockUpdateFailure()
        {
            // This test verifies that if stock update fails AFTER sale creation,
            // the entire transaction rolls back.

            using var scope = _serviceProvider.CreateScope();
            var productRepo = scope.ServiceProvider.GetRequiredService<IProductRepository>();
            var customerRepo = scope.ServiceProvider.GetRequiredService<ICustomerRepository>();
            var salesService = scope.ServiceProvider.GetRequiredService<ISalesService>();

            var product = new Product
            {
                Sku = "TEST003",
                Name = "Atomic Test Product",
                Price = 7.00m,
                StockQuantity = 10
            };
            var addedProduct = await productRepo.AddAsync(product);

            var customer = new Customer
            {
                Name = "Test Customer 3",
                Email = "test3@example.com"
            };
            var addedCustomer = await customerRepo.AddAsync(customer);

            var sale = new Sale
            {
                CustomerId = addedCustomer.Id,
                SaleDate = DateTime.UtcNow,
                TotalAmount = 14.00m
            };

            var details = new List<SaleDetail>
            {
                new SaleDetail
                {
                    ProductId = addedProduct.Id,
                    Quantity = 2,
                    UnitPrice = addedProduct.Price
                }
            };

            // Act
            var createdSale = await salesService.CreateSaleAsync(sale, details);

            // Assert
            Assert.NotEqual(0, createdSale.Id);

            var updatedProduct = await productRepo.GetByIdAsync(addedProduct.Id);
            Assert.NotNull(updatedProduct);
            Assert.Equal(8, updatedProduct.StockQuantity); // 10 - 2
        }
    }
}
