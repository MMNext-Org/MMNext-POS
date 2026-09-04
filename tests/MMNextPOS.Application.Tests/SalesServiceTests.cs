using System;
using System.Collections.Generic;
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
        private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();

        private ISalesService CreateService()
        {
            return new SalesService(_saleRepoMock.Object, _productRepoMock.Object, _unitOfWorkMock.Object);
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
    }
}
