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
    public class PurchaseReturnServiceTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
        private readonly Mock<IPurchaseReturnRepository> _repoMock = new();
        private readonly Mock<IPurchaseReturnDetailRepository> _detailRepoMock = new();
        private readonly Mock<IProductRepository> _productRepoMock = new();
        private readonly Mock<IAuditService> _auditServiceMock = new();

        private IPurchaseReturnService CreateService()
        {
            return new PurchaseReturnService(
                _unitOfWorkMock.Object,
                _repoMock.Object,
                _detailRepoMock.Object,
                _productRepoMock.Object,
                _auditServiceMock.Object);
        }

        private void SetupHappyPath(int productId = 1)
        {
            _unitOfWorkMock.Setup(r => r.BeginTransactionAsync(It.IsAny<CancellationToken>()))
                            .Returns(Task.CompletedTask);
            _unitOfWorkMock.Setup(r => r.CommitAsync(It.IsAny<CancellationToken>()))
                            .Returns(Task.CompletedTask);
            _unitOfWorkMock.Setup(r => r.RollbackAsync(It.IsAny<CancellationToken>()))
                            .Returns(Task.CompletedTask);
            _unitOfWorkMock.Setup(r => r.GetPurchaseReturnByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                            .ReturnsAsync((PurchaseReturn?)null);
            _repoMock.Setup(r => r.AddAsync(It.IsAny<PurchaseReturn>(), It.IsAny<CancellationToken>()))
                     .Callback<PurchaseReturn, CancellationToken>((p, _) => p.Id = 42)
                     .ReturnsAsync((PurchaseReturn p, CancellationToken _) => p);
            _repoMock.Setup(r => r.UpdateAsync(It.IsAny<PurchaseReturn>(), It.IsAny<CancellationToken>()))
                     .Returns(Task.CompletedTask);
            _repoMock.Setup(r => r.DeleteAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                     .Returns(Task.CompletedTask);
            _detailRepoMock.Setup(r => r.AddAsync(It.IsAny<PurchaseReturnDetail>(), It.IsAny<CancellationToken>()))
                           .Callback<PurchaseReturnDetail, CancellationToken>((d, _) => d.Id = 100)
                           .ReturnsAsync((PurchaseReturnDetail d, CancellationToken _) => d);
            _detailRepoMock.Setup(r => r.UpdateAsync(It.IsAny<PurchaseReturnDetail>(), It.IsAny<CancellationToken>()))
                           .Returns(Task.CompletedTask);
            _detailRepoMock.Setup(r => r.DeleteAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                           .Returns(Task.CompletedTask);
            _productRepoMock.Setup(r => r.TryIncrementStockAsync(productId, It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                            .ReturnsAsync(true);
            _productRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                            .ReturnsAsync(new Product { Id = productId, StockQuantity = 100 });
            _auditServiceMock.Setup(a => a.LogAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(),
                It.IsAny<object>(), It.IsAny<object>(), It.IsAny<int?>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task GetByIdAsync_ExistingReturn_ReturnsReturn()
        {
            // Arrange
            var purchaseReturn = new PurchaseReturn { Id = 1, ReturnNo = "RET-001", SupplierId = 5 };
            _repoMock.Setup(r => r.GetByIdAsync(purchaseReturn.Id, It.IsAny<CancellationToken>()))
                     .ReturnsAsync(purchaseReturn);

            var service = CreateService();

            // Act
            var result = await service.GetByIdAsync(purchaseReturn.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(purchaseReturn.Id, result.Id);
            Assert.Equal(purchaseReturn.ReturnNo, result.ReturnNo);
        }

        [Fact]
        public async Task GetByIdAsync_NonExistentReturn_ReturnsNull()
        {
            // Arrange
            _repoMock.Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync((PurchaseReturn?)null!);

            var service = CreateService();

            // Act
            var result = await service.GetByIdAsync(999);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetAllAsync_ReturnsAllReturns()
        {
            // Arrange
            var returns = new List<PurchaseReturn>
            {
                new() { Id = 1, ReturnNo = "RET-001", SupplierId = 5 },
                new() { Id = 2, ReturnNo = "RET-002", SupplierId = 5 },
                new() { Id = 3, ReturnNo = "RET-003", SupplierId = 5 }
            };
            _repoMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                     .ReturnsAsync(returns);

            var service = CreateService();

            // Act
            var result = await service.GetAllAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.Count);
        }

        [Fact]
        public async Task GetPageAsync_ReturnsPagedResult()
        {
            // Arrange
            var pageResult = new PagedResult<PurchaseReturn> { Items = new List<PurchaseReturn> { new PurchaseReturn { Id = 1 } }, TotalCount = 1 };
            _repoMock.Setup(r => r.GetPageAsync(1, 10, It.IsAny<CancellationToken>()))
                     .ReturnsAsync(pageResult);

            var service = CreateService();

            // Act
            var result = await service.GetPageAsync(1, 10);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.TotalCount);
            Assert.Single(result.Items);
        }

        [Fact]
        public async Task AddAsync_HappyPath_BeginsTransactionAndCommits()
        {
            // Arrange
            SetupHappyPath();
            var purchaseReturn = new PurchaseReturn { SupplierId = 5, ReturnNo = "RET-001" };

            var service = CreateService();

            // Act
            var result = await service.AddAsync(purchaseReturn);

            // Assert
            _unitOfWorkMock.Verify(r => r.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWorkMock.Verify(r => r.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWorkMock.Verify(r => r.RollbackAsync(It.IsAny<CancellationToken>()), Times.Never);
            _repoMock.Verify(r => r.AddAsync(purchaseReturn, It.IsAny<CancellationToken>()), Times.Once);
            _auditServiceMock.Verify(a => a.LogAsync(nameof(PurchaseReturn), result.Id, "Create", null, result, 1, "System", It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task AddAsync_ExceptionDuringRepo_RollsBackTransaction()
        {
            // Arrange
            SetupHappyPath();
            _repoMock.Setup(r => r.AddAsync(It.IsAny<PurchaseReturn>(), It.IsAny<CancellationToken>()))
                     .ThrowsAsync(new InvalidOperationException("DB error"));
            _unitOfWorkMock.Setup(r => r.RollbackAsync(It.IsAny<CancellationToken>()))
                            .Returns(Task.CompletedTask);

            var service = CreateService();
            var purchaseReturn = new PurchaseReturn { SupplierId = 5, ReturnNo = "RET-001" };

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => service.AddAsync(purchaseReturn));
            _unitOfWorkMock.Verify(r => r.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWorkMock.Verify(r => r.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task UpdateAsync_ExistingReturn_UpdatesAndCommits()
        {
            // Arrange
            SetupHappyPath();
            var existing = new PurchaseReturn { Id = 1, ReturnNo = "RET-001", SupplierId = 5 };
            var updated = new PurchaseReturn { Id = 1, ReturnNo = "RET-001-UPDATED", SupplierId = 5 };
            _repoMock.Setup(r => r.GetByIdAsync(existing.Id, It.IsAny<CancellationToken>()))
                     .ReturnsAsync(existing);

            var service = CreateService();

            // Act
            await service.UpdateAsync(updated);

            // Assert
            _unitOfWorkMock.Verify(r => r.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWorkMock.Verify(r => r.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWorkMock.Verify(r => r.RollbackAsync(It.IsAny<CancellationToken>()), Times.Never);
            _repoMock.Verify(r => r.UpdateAsync(It.Is<PurchaseReturn>(p => p.ReturnNo == "RET-001-UPDATED"), It.IsAny<CancellationToken>()), Times.Once);
            _auditServiceMock.Verify(a => a.LogAsync(nameof(PurchaseReturn), updated.Id, "Update", existing, updated, It.IsAny<int?>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_ExistingReturn_DeletesAndCommits()
        {
            // Arrange
            SetupHappyPath();
            var existing = new PurchaseReturn { Id = 1, ReturnNo = "RET-001", SupplierId = 5 };
            _repoMock.Setup(r => r.GetByIdAsync(existing.Id, It.IsAny<CancellationToken>()))
                     .ReturnsAsync(existing);

            var service = CreateService();

            // Act
            await service.DeleteAsync(existing.Id);

            // Assert
            _unitOfWorkMock.Verify(r => r.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWorkMock.Verify(r => r.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
            _repoMock.Verify(r => r.DeleteAsync(existing.Id, It.IsAny<CancellationToken>()), Times.Once);
            _auditServiceMock.Verify(a => a.LogAsync(nameof(PurchaseReturn), existing.Id, "Delete", existing, null, It.IsAny<int?>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetPurchaseReturnDetailsAsync_ExistingReturn_ReturnsDetails()
        {
            // Arrange
            SetupHappyPath();
            var details = new List<PurchaseReturnDetail>
            {
                new() { Id = 1, PurchaseReturnId = 1, ProductId = 1, Quantity = 5 },
                new() { Id = 2, PurchaseReturnId = 1, ProductId = 2, Quantity = 3 }
            };
            var purchaseReturn = new PurchaseReturn { Id = 1, ReturnNo = "RET-001", PurchaseReturnDetails = details };
            _unitOfWorkMock.Setup(r => r.GetPurchaseReturnByIdAsync(1, It.IsAny<CancellationToken>()))
                           .ReturnsAsync(purchaseReturn);

            var service = CreateService();

            // Act
            var result = await service.GetPurchaseReturnDetailsAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task GetPurchaseReturnDetailsAsync_NonExistentReturn_ReturnsEmptyList()
        {
            // Arrange
            SetupHappyPath();
            _unitOfWorkMock.Setup(r => r.GetPurchaseReturnByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                           .ReturnsAsync((PurchaseReturn?)null!);

            var service = CreateService();

            // Act
            var result = await service.GetPurchaseReturnDetailsAsync(999);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task AddPurchaseReturnDetailAsync_HappyPath_CommitsTransaction()
        {
            // Arrange
            SetupHappyPath();
            var detail = new PurchaseReturnDetail { PurchaseReturnId = 1, ProductId = 1, Quantity = 5 };

            var service = CreateService();

            // Act
            var result = await service.AddPurchaseReturnDetailAsync(detail);

            // Assert
            _unitOfWorkMock.Verify(r => r.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWorkMock.Verify(r => r.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
            _detailRepoMock.Verify(r => r.AddAsync(detail, It.IsAny<CancellationToken>()), Times.Once);
            _auditServiceMock.Verify(a => a.LogAsync(nameof(PurchaseReturnDetail), result.Id, "Create", null, result, It.IsAny<int?>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task UpdatePurchaseReturnDetailAsync_ExistingDetail_UpdatesAndCommits()
        {
            // Arrange
            SetupHappyPath();
            var existing = new PurchaseReturnDetail { Id = 1, PurchaseReturnId = 1, ProductId = 1, Quantity = 5 };
            var updated = new PurchaseReturnDetail { Id = 1, PurchaseReturnId = 1, ProductId = 1, Quantity = 10 };
            _detailRepoMock.Setup(r => r.GetByIdAsync(existing.Id, It.IsAny<CancellationToken>()))
                           .ReturnsAsync(existing);

            var service = CreateService();

            // Act
            await service.UpdatePurchaseReturnDetailAsync(updated);

            // Assert
            _unitOfWorkMock.Verify(r => r.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWorkMock.Verify(r => r.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
            _detailRepoMock.Verify(r => r.UpdateAsync(It.Is<PurchaseReturnDetail>(d => d.Quantity == 10), It.IsAny<CancellationToken>()), Times.Once);
            _auditServiceMock.Verify(a => a.LogAsync(nameof(PurchaseReturnDetail), updated.Id, "Update", existing, updated, It.IsAny<int?>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task DeletePurchaseReturnDetailAsync_ExistingDetail_DeletesAndCommits()
        {
            // Arrange
            SetupHappyPath();
            var existing = new PurchaseReturnDetail { Id = 1, PurchaseReturnId = 1, ProductId = 1, Quantity = 5 };
            _detailRepoMock.Setup(r => r.GetByIdAsync(existing.Id, It.IsAny<CancellationToken>()))
                           .ReturnsAsync(existing);

            var service = CreateService();

            // Act
            await service.DeletePurchaseReturnDetailAsync(existing.Id);

            // Assert
            _unitOfWorkMock.Verify(r => r.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWorkMock.Verify(r => r.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
            _detailRepoMock.Verify(r => r.DeleteAsync(existing.Id, It.IsAny<CancellationToken>()), Times.Once);
            _auditServiceMock.Verify(a => a.LogAsync(nameof(PurchaseReturnDetail), existing.Id, "Delete", existing, null, It.IsAny<int?>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task CreatePurchaseReturnAsync_HappyPath_StockRestoredAndCommits()
        {
            // Arrange
            SetupHappyPath(productId: 1);
            var returnOrder = new PurchaseReturn { SupplierId = 5, ReturnNo = "RET-001", TotalAmount = 100m };
            var details = new List<PurchaseReturnDetail>
            {
                new PurchaseReturnDetail { ProductId = 1, Quantity = 5, UnitPrice = 10m }
            };

            var service = CreateService();

            // Act
            var result = await service.CreatePurchaseReturnAsync(returnOrder, details);

            // Assert
            _unitOfWorkMock.Verify(r => r.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWorkMock.Verify(r => r.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
            _repoMock.Verify(r => r.AddAsync(returnOrder, It.IsAny<CancellationToken>()), Times.Once);
            _detailRepoMock.Verify(r => r.AddAsync(It.Is<PurchaseReturnDetail>(d => d.ProductId == 1 && d.Quantity == 5), It.IsAny<CancellationToken>()), Times.Once);
            _productRepoMock.Verify(r => r.TryIncrementStockAsync(1, 5, 1, "PurchaseReturn", It.IsAny<CancellationToken>()), Times.Once);
            _auditServiceMock.Verify(a => a.LogAsync(nameof(PurchaseReturn), result.Id, "Create", null, returnOrder, It.IsAny<int?>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task CreatePurchaseReturnAsync_ProductNotFound_ThrowsInvalidOperationException()
        {
            // Arrange
            SetupHappyPath(productId: 99);
            _productRepoMock.Setup(r => r.TryIncrementStockAsync(99, It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                            .ReturnsAsync(false);
            _unitOfWorkMock.Setup(r => r.RollbackAsync(It.IsAny<CancellationToken>()))
                            .Returns(Task.CompletedTask);

            var service = CreateService();
            var returnOrder = new PurchaseReturn { SupplierId = 5, ReturnNo = "RET-001" };
            var details = new List<PurchaseReturnDetail>
            {
                new PurchaseReturnDetail { ProductId = 99, Quantity = 1, UnitPrice = 10m }
            };

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreatePurchaseReturnAsync(returnOrder, details));
            _unitOfWorkMock.Verify(r => r.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWorkMock.Verify(r => r.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task UpdatePurchaseReturnAsync_HappyPath_StockRestoredAndCommits()
        {
            // Arrange
            SetupHappyPath(productId: 1);
            var existing = new PurchaseReturn { Id = 1, ReturnNo = "RET-001", SupplierId = 5, PurchaseReturnDetails = new List<PurchaseReturnDetail>() };
            var returnOrder = new PurchaseReturn { Id = 1, ReturnNo = "RET-001-UPDATED", SupplierId = 5, TotalAmount = 100m };
            var details = new List<PurchaseReturnDetail>
            {
                new PurchaseReturnDetail { ProductId = 1, Quantity = 5, UnitPrice = 10m }
            };
            _repoMock.Setup(r => r.GetByIdAsync(returnOrder.Id, It.IsAny<CancellationToken>()))
                     .ReturnsAsync(existing);
            _unitOfWorkMock.Setup(r => r.GetPurchaseReturnByIdAsync(returnOrder.Id, It.IsAny<CancellationToken>()))
                           .ReturnsAsync(existing);

            var service = CreateService();

            // Act
            var result = await service.UpdatePurchaseReturnAsync(returnOrder, details);

            // Assert
            _unitOfWorkMock.Verify(r => r.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWorkMock.Verify(r => r.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
            _repoMock.Verify(r => r.UpdateAsync(returnOrder, It.IsAny<CancellationToken>()), Times.Once);
            _productRepoMock.Verify(r => r.TryIncrementStockAsync(1, 5, 1, "PurchaseReturnUpdate", It.IsAny<CancellationToken>()), Times.Once);
            Assert.Equal(returnOrder.ReturnNo, result.ReturnNo);
        }

        [Fact]
        public async Task UpdatePurchaseReturnAsync_ProductNotFound_ThrowsInvalidOperationException()
        {
            // Arrange
            SetupHappyPath(productId: 99);
            _productRepoMock.Setup(r => r.TryIncrementStockAsync(99, It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                            .ReturnsAsync(false);
            _unitOfWorkMock.Setup(r => r.RollbackAsync(It.IsAny<CancellationToken>()))
                            .Returns(Task.CompletedTask);
            var existing = new PurchaseReturn { Id = 1, ReturnNo = "RET-001", SupplierId = 5, PurchaseReturnDetails = new List<PurchaseReturnDetail>() };
            var returnOrder = new PurchaseReturn { Id = 1, ReturnNo = "RET-001-UPDATED", SupplierId = 5 };
            var details = new List<PurchaseReturnDetail>
            {
                new PurchaseReturnDetail { ProductId = 99, Quantity = 1, UnitPrice = 10m }
            };
            _repoMock.Setup(r => r.GetByIdAsync(returnOrder.Id, It.IsAny<CancellationToken>()))
                     .ReturnsAsync(existing);
            _unitOfWorkMock.Setup(r => r.GetPurchaseReturnByIdAsync(returnOrder.Id, It.IsAny<CancellationToken>()))
                           .ReturnsAsync(existing);

            var service = CreateService();

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => service.UpdatePurchaseReturnAsync(returnOrder, details));
            _unitOfWorkMock.Verify(r => r.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWorkMock.Verify(r => r.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task GetAvailableStockAsync_ExistingProduct_ReturnsStockQuantity()
        {
            // Arrange
            SetupHappyPath(productId: 1);
            var product = new Product { Id = 1, StockQuantity = 50 };
            _productRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                            .ReturnsAsync(product);

            var service = CreateService();

            // Act
            var result = await service.GetAvailableStockAsync(1);

            // Assert
            Assert.Equal(50, result);
        }

        [Fact]
        public async Task GetAvailableStockAsync_NonExistentProduct_ReturnsZero()
        {
            // Arrange
            SetupHappyPath(productId: 99);
            _productRepoMock.Setup(r => r.GetByIdAsync(99, It.IsAny<CancellationToken>()))
                            .ReturnsAsync((Product?)null!);

            var service = CreateService();

            // Act
            var result = await service.GetAvailableStockAsync(99);

            // Assert
            Assert.Equal(0, result);
        }

        [Fact]
        public async Task HasSufficientStockAsync_EnoughStock_ReturnsTrue()
        {
            // Arrange
            SetupHappyPath(productId: 1);
            var product = new Product { Id = 1, StockQuantity = 100 };
            _productRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                            .ReturnsAsync(product);

            var service = CreateService();

            // Act
            var result = await service.HasSufficientStockAsync(1, 50);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task HasSufficientStockAsync_NotEnoughStock_ReturnsFalse()
        {
            // Arrange
            SetupHappyPath(productId: 1);
            var product = new Product { Id = 1, StockQuantity = 10 };
            _productRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                            .ReturnsAsync(product);

            var service = CreateService();

            // Act
            var result = await service.HasSufficientStockAsync(1, 50);

            // Assert
            Assert.False(result);
        }
    }
}
