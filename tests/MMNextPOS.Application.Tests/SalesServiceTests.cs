using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MMNextPOS.Application.Services;
using MMNextPOS.Domain.Models;
using MMNextPOS.Infrastructure;
using MMNextPOS.Infrastructure.Repositories;
using Moq;
using Xunit;

namespace MMNextPOS.Application.Tests
{
    public class SalesServiceTests
    {
        private readonly Mock<IProductRepository> _productRepoMock = new();
        private readonly Mock<ISaleRepository> _saleRepoMock = new();
        private readonly Mock<ISaleDetailRepository> _saleDetailRepoMock = new();
        private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();

        private ISalesService CreateService()
        {
            return new SalesService(_saleRepoMock.Object, _saleDetailRepoMock.Object, _productRepoMock.Object, _unitOfWorkMock.Object);
        }

        [Fact]
        public async Task CreateSaleAsync_ValidData_CreatesSaleAndUpdatesStock()
        {
            // Arrange
            var product = new Product { Id = 1, StockQuantity = 10, Price = 5m };
            _productRepoMock.Setup(r => r.GetByIdAsync(product.Id, It.IsAny<CancellationToken>()))
                             .ReturnsAsync(product);
            _saleRepoMock.Setup(r => r.CreateSaleWithDetailsAsync(It.IsAny<Sale>(), It.IsAny<IEnumerable<SaleDetail>>(), It.IsAny<CancellationToken>()))
                          .ReturnsAsync((Sale s, IEnumerable<SaleDetail> d, CancellationToken ct) => { s.Id = 42; return s; });
            _productRepoMock.Setup(r => r.UpdateAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
                             .Returns(Task.CompletedTask);
            _unitOfWorkMock.Setup(r => r.BeginTransactionAsync(It.IsAny<CancellationToken>()))
                            .Returns(Task.CompletedTask);
            _unitOfWorkMock.Setup(r => r.CommitAsync(It.IsAny<CancellationToken>()))
                            .Returns(Task.CompletedTask);

            var service = CreateService();
            var sale = new Sale { CustomerId = 1, TotalAmount = 15m };
            var details = new List<SaleDetail>
            {
                new SaleDetail { ProductId = product.Id, Quantity = 2, UnitPrice = product.Price }
            };

            // Act
            var result = await service.CreateSaleAsync(sale, details);

            // Assert
            Assert.Equal(42, result.Id);
            _unitOfWorkMock.Verify(r => r.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWorkMock.Verify(r => r.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
            _saleRepoMock.Verify(r => r.CreateSaleWithDetailsAsync(sale, details, It.IsAny<CancellationToken>()), Times.Once);
            _productRepoMock.Verify(r => r.UpdateAsync(It.Is<Product>(p => p.StockQuantity == 8), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task CreateSaleAsync_InsufficientStock_ThrowsInsufficientStockException()
        {
            // Arrange
            var product = new Product { Id = 1, StockQuantity = 1, Price = 5m };
            _productRepoMock.Setup(r => r.GetByIdAsync(product.Id, It.IsAny<CancellationToken>()))
                             .ReturnsAsync(product);
            _unitOfWorkMock.Setup(r => r.BeginTransactionAsync(It.IsAny<CancellationToken>()))
                            .Returns(Task.CompletedTask);
            _unitOfWorkMock.Setup(r => r.RollbackAsync(It.IsAny<CancellationToken>()))
                            .Returns(Task.CompletedTask);

            var service = CreateService();
            var sale = new Sale { CustomerId = 1, TotalAmount = 15m };
            var details = new List<SaleDetail>
            {
                new SaleDetail { ProductId = product.Id, Quantity = 2, UnitPrice = product.Price }
            };

            // Act & Assert
            await Assert.ThrowsAsync<InsufficientStockException>(() => service.CreateSaleAsync(sale, details));

            // Verify rollback was called
            _unitOfWorkMock.Verify(r => r.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWorkMock.Verify(r => r.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task CreateSaleAsync_ExceptionDuringOperation_RollsBackTransaction()
        {
            // Arrange
            var product = new Product { Id = 1, StockQuantity = 10, Price = 5m };
            _productRepoMock.Setup(r => r.GetByIdAsync(product.Id, It.IsAny<CancellationToken>()))
                             .ReturnsAsync(product);
            _saleRepoMock.Setup(r => r.CreateSaleWithDetailsAsync(It.IsAny<Sale>(), It.IsAny<IEnumerable<SaleDetail>>(), It.IsAny<CancellationToken>()))
                          .ThrowsAsync(new InvalidOperationException("Database error"));
            _unitOfWorkMock.Setup(r => r.BeginTransactionAsync(It.IsAny<CancellationToken>()))
                            .Returns(Task.CompletedTask);
            _unitOfWorkMock.Setup(r => r.RollbackAsync(It.IsAny<CancellationToken>()))
                            .Returns(Task.CompletedTask);

            var service = CreateService();
            var sale = new Sale { CustomerId = 1, TotalAmount = 15m };
            var details = new List<SaleDetail>
            {
                new SaleDetail { ProductId = product.Id, Quantity = 2, UnitPrice = product.Price }
            };

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateSaleAsync(sale, details));

            // Verify rollback was called, commit was not
            _unitOfWorkMock.Verify(r => r.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWorkMock.Verify(r => r.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        private void SetupSales(params Sale[] sales)
        {
            _saleRepoMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                         .ReturnsAsync((IReadOnlyList<Sale>)sales.ToList());
        }

        [Fact]
        public async Task GetAllAsync_NoFilters_ReturnsAllSalesOrderedByDateDescending()
        {
            // Arrange
            SetupSales(
                new Sale { Id = 1, CustomerId = 1, SaleDate = new DateTime(2026, 9, 1), TotalAmount = 10m },
                new Sale { Id = 2, CustomerId = 2, SaleDate = new DateTime(2026, 9, 3), TotalAmount = 20m },
                new Sale { Id = 3, CustomerId = 1, SaleDate = new DateTime(2026, 9, 2), TotalAmount = 30m });

            var service = CreateService();

            // Act
            var result = await service.GetAllAsync();

            // Assert
            Assert.Equal(new[] { 2, 3, 1 }, result.Select(s => s.Id));
        }

        [Fact]
        public async Task GetAllAsync_DateRange_FiltersInclusively()
        {
            // Arrange
            SetupSales(
                new Sale { Id = 1, SaleDate = new DateTime(2026, 8, 31, 23, 59, 59), TotalAmount = 10m },
                new Sale { Id = 2, SaleDate = new DateTime(2026, 9, 1), TotalAmount = 20m },
                new Sale { Id = 3, SaleDate = new DateTime(2026, 9, 5, 12, 0, 0), TotalAmount = 30m },
                new Sale { Id = 4, SaleDate = new DateTime(2026, 9, 10), TotalAmount = 40m });

            var service = CreateService();

            // Act
            var result = await service.GetAllAsync(
                fromDate: new DateTime(2026, 9, 1),
                toDate: new DateTime(2026, 9, 5));

            // Assert
            Assert.Equal(new[] { 3, 2 }, result.Select(s => s.Id));
        }

        [Fact]
        public async Task GetAllAsync_CustomerFilter_ReturnsOnlySalesForThatCustomer()
        {
            // Arrange
            SetupSales(
                new Sale { Id = 1, CustomerId = 1, SaleDate = new DateTime(2026, 9, 1), TotalAmount = 10m },
                new Sale { Id = 2, CustomerId = 2, SaleDate = new DateTime(2026, 9, 2), TotalAmount = 20m },
                new Sale { Id = 3, CustomerId = 1, SaleDate = new DateTime(2026, 9, 3), TotalAmount = 30m });

            var service = CreateService();

            // Act
            var result = await service.GetAllAsync(customerId: 1);

            // Assert
            Assert.Equal(new[] { 3, 1 }, result.Select(s => s.Id));
        }

        [Fact]
        public async Task GetAllAsync_StatusFilter_IsCaseInsensitiveAndExcludesNullStatus()
        {
            // Arrange
            SetupSales(
                new Sale { Id = 1, SaleDate = new DateTime(2026, 9, 1), TotalAmount = 10m, Status = "Completed" },
                new Sale { Id = 2, SaleDate = new DateTime(2026, 9, 2), TotalAmount = 20m, Status = "completed" },
                new Sale { Id = 3, SaleDate = new DateTime(2026, 9, 3), TotalAmount = 30m, Status = "Voided" },
                new Sale { Id = 4, SaleDate = new DateTime(2026, 9, 4), TotalAmount = 40m, Status = null });

            var service = CreateService();

            // Act
            var result = await service.GetAllAsync(status: "COMPLETED");

            // Assert
            Assert.Equal(new[] { 2, 1 }, result.Select(s => s.Id));
        }

        [Fact]
        public async Task GetAllAsync_LocationFilter_ReturnsOnlySalesForThatLocation()
        {
            // Arrange
            SetupSales(
                new Sale { Id = 1, SaleDate = new DateTime(2026, 9, 1), TotalAmount = 10m, LocationId = 1 },
                new Sale { Id = 2, SaleDate = new DateTime(2026, 9, 2), TotalAmount = 20m, LocationId = 2 },
                new Sale { Id = 3, SaleDate = new DateTime(2026, 9, 3), TotalAmount = 30m, LocationId = null });

            var service = CreateService();

            // Act
            var result = await service.GetAllAsync(locationId: 1);

            // Assert
            Assert.Equal(new[] { 1 }, result.Select(s => s.Id));
        }
    }
}
