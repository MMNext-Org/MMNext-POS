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
        private readonly Mock<IPurchaseReturnRepository> _returnRepoMock = new();
        private readonly Mock<IPurchaseReturnDetailRepository> _detailRepoMock = new();
        private readonly Mock<IProductRepository> _productRepoMock = new();
        private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
        private readonly Mock<IStockMovementService> _stockMovementMock = new();
        private readonly Mock<IAuditService> _auditServiceMock = new();

        private IPurchaseReturnService CreateService() => new PurchaseReturnService(
            _returnRepoMock.Object,
            _detailRepoMock.Object,
            _productRepoMock.Object,
            _unitOfWorkMock.Object,
            _stockMovementMock.Object,
            _auditServiceMock.Object);

        private void SetupTransaction()
        {
            _unitOfWorkMock.Setup(r => r.BeginTransactionAsync(It.IsAny<CancellationToken>()))
                           .Returns(Task.CompletedTask);
            _unitOfWorkMock.Setup(r => r.CommitAsync(It.IsAny<CancellationToken>()))
                           .Returns(Task.CompletedTask);
            _unitOfWorkMock.Setup(r => r.RollbackAsync(It.IsAny<CancellationToken>()))
                           .Returns(Task.CompletedTask);
        }

        private static PurchaseReturn NewReturn(int id = 1, string returnNo = "RET-001") => new()
        {
            Id = id,
            ReturnNo = returnNo,
            PurchaseId = 10,
            SupplierId = 5,
            ReturnDate = DateTime.UtcNow,
            TotalAmount = 100m,
            Status = "Active"
        };

        private static PurchaseReturnDetail NewDetail(int id, int purchaseReturnId, int productId, int quantity) => new()
        {
            Id = id,
            PurchaseReturnId = purchaseReturnId,
            ProductId = productId,
            Quantity = quantity,
            UnitPrice = 10m,
            LineTotal = quantity * 10m
        };

        // ───────────────────────── Argument validation ─────────────────────────

        [Fact]
        public void Constructor_NullRepo_ThrowsArgumentNullException() =>
            Assert.Throws<ArgumentNullException>(() => new PurchaseReturnService(
                null!, _detailRepoMock.Object, _productRepoMock.Object, _unitOfWorkMock.Object,
                _stockMovementMock.Object, _auditServiceMock.Object));

        [Fact]
        public async Task CreatePurchaseReturnAsync_NullReturnOrder_ThrowsArgumentNullException()
        {
            var service = CreateService();
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                service.CreatePurchaseReturnAsync(null!, new List<PurchaseReturnDetail>()));
        }

        [Fact]
        public async Task CreatePurchaseReturnAsync_NullDetails_ThrowsArgumentNullException()
        {
            var service = CreateService();
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                service.CreatePurchaseReturnAsync(NewReturn(), null!));
        }

        [Fact]
        public async Task CreatePurchaseReturnAsync_EmptyDetails_ThrowsValidationException()
        {
            var service = CreateService();
            await Assert.ThrowsAsync<ValidationException>(() =>
                service.CreatePurchaseReturnAsync(NewReturn(), Array.Empty<PurchaseReturnDetail>()));
        }

        // ───────────────────────── Create happy path ─────────────────────────

        [Fact]
        public async Task CreatePurchaseReturnAsync_HappyPath_DecrementsStock_Commits_Audits()
        {
            // Arrange
            SetupTransaction();
            _productRepoMock.Setup(r => r.TryDecrementStockAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                            .ReturnsAsync(true);
            _returnRepoMock.Setup(r => r.AddAsync(It.IsAny<PurchaseReturn>(), It.IsAny<CancellationToken>()))
                           .Callback<PurchaseReturn, CancellationToken>((p, _) => p.Id = 7)
                           .ReturnsAsync((PurchaseReturn p, CancellationToken _) => p);
            _detailRepoMock.Setup(r => r.AddAsync(It.IsAny<PurchaseReturnDetail>(), It.IsAny<CancellationToken>()))
                           .ReturnsAsync((PurchaseReturnDetail d, CancellationToken _) => d);
            _stockMovementMock.Setup(s => s.AddReturnMovementAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<decimal>(), It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                                          .ReturnsAsync(new StockMovement());

            var service = CreateService();
            var returnOrder = NewReturn();
            var details = new List<PurchaseReturnDetail> { NewDetail(1, 1, 100, 3), NewDetail(2, 1, 101, 2) };

            // Act
            var result = await service.CreatePurchaseReturnAsync(returnOrder, details);

            // Assert
            Assert.Equal(7, result.Id);
            _unitOfWorkMock.Verify(r => r.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWorkMock.Verify(r => r.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWorkMock.Verify(r => r.RollbackAsync(It.IsAny<CancellationToken>()), Times.Never);
            _productRepoMock.Verify(r => r.TryDecrementStockAsync(100, 3, It.IsAny<int>(), "PurchaseReturn", It.IsAny<CancellationToken>()), Times.Once);
            _productRepoMock.Verify(r => r.TryDecrementStockAsync(101, 2, It.IsAny<int>(), "PurchaseReturn", It.IsAny<CancellationToken>()), Times.Once);
            _returnRepoMock.Verify(r => r.AddAsync(returnOrder, It.IsAny<CancellationToken>()), Times.Once);
            _detailRepoMock.Verify(r => r.AddAsync(It.Is<PurchaseReturnDetail>(d => d.PurchaseReturnId == 7), It.IsAny<CancellationToken>()), Times.Exactly(2));
            _stockMovementMock.Verify(s => s.AddReturnMovementAsync(7, It.IsAny<int>(), It.IsAny<int>(), It.IsAny<decimal>(), It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
            _auditServiceMock.Verify(a => a.LogAsync(nameof(PurchaseReturn), 7, "Create", null, It.IsAny<object>(), It.IsAny<int?>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task CreatePurchaseReturnAsync_InsufficientStock_RollsBack_NoWrites()
        {
            // Arrange
            SetupTransaction();
            _productRepoMock.Setup(r => r.TryDecrementStockAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                            .ReturnsAsync(false);
            _unitOfWorkMock.Setup(r => r.RollbackAsync(It.IsAny<CancellationToken>()))
                           .Returns(Task.CompletedTask);

            var service = CreateService();
            var details = new List<PurchaseReturnDetail> { NewDetail(1, 1, 999, 10) };

            // Act & Assert
            await Assert.ThrowsAsync<ValidationException>(() =>
                service.CreatePurchaseReturnAsync(NewReturn(), details));

            _unitOfWorkMock.Verify(r => r.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWorkMock.Verify(r => r.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
            _returnRepoMock.Verify(r => r.AddAsync(It.IsAny<PurchaseReturn>(), It.IsAny<CancellationToken>()), Times.Never);
            _detailRepoMock.Verify(r => r.AddAsync(It.IsAny<PurchaseReturnDetail>(), It.IsAny<CancellationToken>()), Times.Never);
            _stockMovementMock.Verify(s => s.AddReturnMovementAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<decimal>(), It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        // ───────────────────────── Update happy path ─────────────────────────

        [Fact]
        public async Task UpdatePurchaseReturnAsync_HappyPath_ReversesOldStock_AppliesNewStock_Commits()
        {
            // Arrange
            SetupTransaction();
            _returnRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                           .ReturnsAsync(NewReturn(1));
            _productRepoMock.Setup(r => r.TryIncrementStockAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                            .ReturnsAsync(true);
            _productRepoMock.Setup(r => r.TryDecrementStockAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                            .ReturnsAsync(true);
            _detailRepoMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                                       .ReturnsAsync(new List<PurchaseReturnDetail> { NewDetail(1, 1, 200, 4) });
            _detailRepoMock.Setup(r => r.DeleteAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                           .Returns(Task.CompletedTask);
            _detailRepoMock.Setup(r => r.AddAsync(It.IsAny<PurchaseReturnDetail>(), It.IsAny<CancellationToken>()))
                           .ReturnsAsync((PurchaseReturnDetail d, CancellationToken _) => d);
            _returnRepoMock.Setup(r => r.UpdateAsync(It.IsAny<PurchaseReturn>(), It.IsAny<CancellationToken>()))
                           .Returns(Task.CompletedTask);
            _auditServiceMock.Setup(a => a.LogAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<object>(), It.IsAny<object>(), It.IsAny<int?>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                             .Returns(Task.CompletedTask);

            var service = CreateService();
            var updated = NewReturn(1, "RET-001-UPDATED");
            var newDetails = new List<PurchaseReturnDetail> { NewDetail(10, 1, 300, 5) };

            // Act
            await service.UpdatePurchaseReturnAsync(updated, newDetails);

            // Assert
            _unitOfWorkMock.Verify(r => r.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWorkMock.Verify(r => r.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWorkMock.Verify(r => r.RollbackAsync(It.IsAny<CancellationToken>()), Times.Never);
            // Reversal of old line (product 200, qty 4)
            _productRepoMock.Verify(r => r.TryIncrementStockAsync(200, 4, It.IsAny<int>(), "PurchaseReturnReversal", It.IsAny<CancellationToken>()), Times.Once);
            // Decrement of new line (product 300, qty 5)
            _productRepoMock.Verify(r => r.TryDecrementStockAsync(300, 5, It.IsAny<int>(), "PurchaseReturn", It.IsAny<CancellationToken>()), Times.Once);
            _returnRepoMock.Verify(r => r.UpdateAsync(updated, It.IsAny<CancellationToken>()), Times.Once);
            _auditServiceMock.Verify(a => a.LogAsync(nameof(PurchaseReturn), 1, "Update", It.IsAny<object>(), updated, It.IsAny<int?>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task UpdatePurchaseReturnAsync_NotFound_ThrowsValidationException()
        {
            // Arrange
            _returnRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync((PurchaseReturn?)null);

            var service = CreateService();
            await Assert.ThrowsAsync<ValidationException>(() =>
                service.UpdatePurchaseReturnAsync(NewReturn(1), new List<PurchaseReturnDetail>()));
        }

        // ───────────────────────── Delete happy path ─────────────────────────

        [Fact]
        public async Task DeleteAsync_HappyPath_RestoresStock_DeletesDetails_Commits()
        {
            // Arrange
            SetupTransaction();
            var existing = NewReturn(5, "RET-005");
            _returnRepoMock.Setup(r => r.GetByIdAsync(5, It.IsAny<CancellationToken>())).ReturnsAsync(existing);
            _detailRepoMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                                       .ReturnsAsync(new List<PurchaseReturnDetail> { NewDetail(1, 5, 400, 2), NewDetail(2, 5, 401, 3) });
            _productRepoMock.Setup(r => r.TryIncrementStockAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                            .ReturnsAsync(true);
            _detailRepoMock.Setup(r => r.DeleteAsync(It.IsAny<int>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
            _returnRepoMock.Setup(r => r.DeleteAsync(5, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
            _auditServiceMock.Setup(a => a.LogAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<object>(), It.IsAny<object>(), It.IsAny<int?>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                             .Returns(Task.CompletedTask);

            var service = CreateService();
            await service.DeleteAsync(5);

            // Assert
            _unitOfWorkMock.Verify(r => r.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWorkMock.Verify(r => r.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWorkMock.Verify(r => r.RollbackAsync(It.IsAny<CancellationToken>()), Times.Never);
            _productRepoMock.Verify(r => r.TryIncrementStockAsync(400, 2, It.IsAny<int>(), "PurchaseReturnDelete", It.IsAny<CancellationToken>()), Times.Once);
            _productRepoMock.Verify(r => r.TryIncrementStockAsync(401, 3, It.IsAny<int>(), "PurchaseReturnDelete", It.IsAny<CancellationToken>()), Times.Once);
            _detailRepoMock.Verify(r => r.DeleteAsync(1, It.IsAny<CancellationToken>()), Times.Once);
            _detailRepoMock.Verify(r => r.DeleteAsync(2, It.IsAny<CancellationToken>()), Times.Once);
            _returnRepoMock.Verify(r => r.DeleteAsync(5, It.IsAny<CancellationToken>()), Times.Once);
            _auditServiceMock.Verify(a => a.LogAsync(nameof(PurchaseReturn), 5, "Delete", existing, null, It.IsAny<int?>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_NotFound_ThrowsValidationException_NoTransaction()
        {
            // Arrange
            _returnRepoMock.Setup(r => r.GetByIdAsync(99, It.IsAny<CancellationToken>())).ReturnsAsync((PurchaseReturn?)null);

            var service = CreateService();
            await Assert.ThrowsAsync<ValidationException>(() => service.DeleteAsync(99));

            _unitOfWorkMock.Verify(r => r.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Never);
            _unitOfWorkMock.Verify(r => r.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        // ───────────────────────── Stock validation ─────────────────────────

        [Fact]
        public async Task HasSufficientStockAsync_ReturnsTrue_WhenStockEqualsQuantity()
        {
            // Arrange
            _productRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(new Product { StockQuantity = 5 });
            var service = CreateService();

            // Act
            var result = await service.HasSufficientStockAsync(1, 5);

            Assert.True(result);
        }

        [Fact]
        public async Task HasSufficientStockAsync_ReturnsFalse_WhenStockLessThanQuantity()
        {
            // Arrange
            _productRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(new Product { StockQuantity = 2 });
            var service = CreateService();

            // Act
            var result = await service.HasSufficientStockAsync(1, 5);

            Assert.False(result);
        }

        [Fact]
        public async Task GetAvailableStockAsync_ReturnsZero_WhenProductNotFound()
        {
            // Arrange
            _productRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync((Product?)null);
            var service = CreateService();

            // Act
            var result = await service.GetAvailableStockAsync(1);

            Assert.Equal(0, result);
        }
    }
}
