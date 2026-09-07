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
    public class PurchaseServiceTests
    {
        private readonly Mock<IPurchaseRepository> _purchaseRepoMock = new();
        private readonly Mock<IPurchaseDetailRepository> _detailRepoMock = new();
        private readonly Mock<IPurchaseReturnRepository> _returnRepoMock = new();
        private readonly Mock<IPurchaseReturnDetailRepository> _returnDetailRepoMock = new();
        private readonly Mock<IProductRepository> _productRepoMock = new();
        private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
        private readonly Mock<IAuditService> _auditServiceMock = new();
        private readonly Mock<IStockMovementService> _stockMovementMock = new();
        private readonly Mock<IInvoiceNumberGenerator> _invoiceNumberMock = new();
        private readonly Mock<IOutstandingService> _outstandingMock = new();

        private IPurchaseService CreateService()
        {
            return new PurchaseService(
                _purchaseRepoMock.Object,
                _detailRepoMock.Object,
                _returnRepoMock.Object,
                _returnDetailRepoMock.Object,
                _productRepoMock.Object,
                _unitOfWorkMock.Object,
                _auditServiceMock.Object,
                _stockMovementMock.Object,
                _invoiceNumberMock.Object,
                _outstandingMock.Object);
        }

        private void SetupHappyPath(int productId = 1, int initialStock = 0, int supplierId = 5, decimal price = 10m, string invoiceNo = "PUR-2026-000001")
        {
            _productRepoMock.Setup(r => r.TryIncrementStockAsync(productId, It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                            .ReturnsAsync(true);
            _purchaseRepoMock.Setup(r => r.AddAsync(It.IsAny<Purchase>(), It.IsAny<CancellationToken>()))
                            .Callback<Purchase, CancellationToken>((p, _) => p.Id = 42)
                            .ReturnsAsync((Purchase p, CancellationToken _) => p);
            _detailRepoMock.Setup(r => r.AddAsync(It.IsAny<PurchaseDetail>(), It.IsAny<CancellationToken>()))
                           .ReturnsAsync((PurchaseDetail d, CancellationToken _) => { d.Id = 100; return d; });
            _stockMovementMock.Setup(s => s.AddPurchaseMovementAsync(It.IsAny<Purchase>(), It.IsAny<IReadOnlyList<PurchaseDetail>>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()))
                              .ReturnsAsync(new StockMovement { Id = 200 });
            _invoiceNumberMock.Setup(n => n.NextAsync("PUR", It.IsAny<CancellationToken>()))
                              .ReturnsAsync(invoiceNo);
            _outstandingMock.Setup(o => o.AddSupplierOutstandingAsync(It.IsAny<SupplierOutstanding>(), It.IsAny<CancellationToken>()))
                            .ReturnsAsync((SupplierOutstanding s, CancellationToken _) => { s.Id = 7; return s; });
            _unitOfWorkMock.Setup(r => r.BeginTransactionAsync(It.IsAny<CancellationToken>()))
                            .Returns(Task.CompletedTask);
            _unitOfWorkMock.Setup(r => r.CommitAsync(It.IsAny<CancellationToken>()))
                            .Returns(Task.CompletedTask);
        }

        // ───────────────────────── Existing behaviour preserved ─────────────────────────

        [Fact]
        public async Task CreatePurchaseWithDetailsAsync_HappyPath_PersistsAndUpdatesStock()
        {
            // Arrange
            SetupHappyPath(productId: 1, supplierId: 5, price: 10m);

            var service = CreateService();
            var purchase = new Purchase { SupplierId = 5, PurchaseDate = DateTime.UtcNow, TotalAmount = 0m };
            var details = new List<PurchaseDetail>
            {
                new PurchaseDetail { ProductId = 1, Quantity = 5, UnitPrice = 10m }
            };

            // Act
            var result = await service.CreatePurchaseWithDetailsAsync(purchase, details);

            // Assert
            Assert.Equal(42, result.Id);
            _unitOfWorkMock.Verify(r => r.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWorkMock.Verify(r => r.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
            _purchaseRepoMock.Verify(r => r.AddAsync(purchase, It.IsAny<CancellationToken>()), Times.Once);
            // Phase 3 hardening: stock is incremented atomically (not via UpdateAsync).
            _productRepoMock.Verify(r => r.TryIncrementStockAsync(1, 5, It.IsAny<int>(), "Purchase", It.IsAny<CancellationToken>()), Times.Once);
            _productRepoMock.Verify(r => r.UpdateAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task CreatePurchaseWithDetailsAsync_ProductNotFound_ThrowsValidationException()
        {
            // Arrange
            _productRepoMock.Setup(r => r.TryIncrementStockAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                            .ReturnsAsync(false);
            _unitOfWorkMock.Setup(r => r.BeginTransactionAsync(It.IsAny<CancellationToken>()))
                            .Returns(Task.CompletedTask);
            _unitOfWorkMock.Setup(r => r.RollbackAsync(It.IsAny<CancellationToken>()))
                            .Returns(Task.CompletedTask);

            var service = CreateService();
            var purchase = new Purchase { SupplierId = 5, PurchaseDate = DateTime.UtcNow };
            var details = new List<PurchaseDetail>
            {
                new PurchaseDetail { ProductId = 99, Quantity = 1, UnitPrice = 10m }
            };

            // Act & Assert
            await Assert.ThrowsAsync<ValidationException>(() => service.CreatePurchaseWithDetailsAsync(purchase, details));
            _unitOfWorkMock.Verify(r => r.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWorkMock.Verify(r => r.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
            _purchaseRepoMock.Verify(r => r.AddAsync(It.IsAny<Purchase>(), It.IsAny<CancellationToken>()), Times.Never);
            _outstandingMock.Verify(r => r.AddSupplierOutstandingAsync(It.IsAny<SupplierOutstanding>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task CreatePurchaseWithDetailsAsync_ExceptionDuringRepo_RollsBackTransaction()
        {
            // Arrange
            _productRepoMock.Setup(r => r.TryIncrementStockAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                            .ReturnsAsync(true);
            _purchaseRepoMock.Setup(r => r.AddAsync(It.IsAny<Purchase>(), It.IsAny<CancellationToken>()))
                             .ThrowsAsync(new InvalidOperationException("DB error"));
            _unitOfWorkMock.Setup(r => r.BeginTransactionAsync(It.IsAny<CancellationToken>()))
                            .Returns(Task.CompletedTask);
            _unitOfWorkMock.Setup(r => r.RollbackAsync(It.IsAny<CancellationToken>()))
                            .Returns(Task.CompletedTask);

            var service = CreateService();
            var purchase = new Purchase { SupplierId = 5, PurchaseDate = DateTime.UtcNow };
            var details = new List<PurchaseDetail> { new PurchaseDetail { ProductId = 1, Quantity = 1, UnitPrice = 10m } };

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreatePurchaseWithDetailsAsync(purchase, details));
            _unitOfWorkMock.Verify(r => r.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWorkMock.Verify(r => r.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        // ───────────────────────── New Phase-3 hardening tests ─────────────────────────

        [Fact]
        public async Task CreatePurchaseWithDetailsAsync_AggregatesDuplicateLines_ForSameProduct()
        {
            // Arrange
            SetupHappyPath();
            var service = CreateService();
            var purchase = new Purchase { SupplierId = 5, PurchaseDate = DateTime.UtcNow };
            var details = new List<PurchaseDetail>
            {
                new PurchaseDetail { ProductId = 1, Quantity = 2, UnitPrice = 10m },
                new PurchaseDetail { ProductId = 1, Quantity = 3, UnitPrice = 10m },
                new PurchaseDetail { ProductId = 2, Quantity = 1, UnitPrice = 7m }
            };
            _productRepoMock.Setup(r => r.TryIncrementStockAsync(2, It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                            .ReturnsAsync(true);

            // Act
            await service.CreatePurchaseWithDetailsAsync(purchase, details);

            // Assert — stock increments use aggregated quantities (2+3=5 for product 1, 1 for product 2)
            _productRepoMock.Verify(r => r.TryIncrementStockAsync(1, 5, It.IsAny<int>(), "Purchase", It.IsAny<CancellationToken>()), Times.Once);
            _productRepoMock.Verify(r => r.TryIncrementStockAsync(2, 1, It.IsAny<int>(), "Purchase", It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task CreatePurchaseWithDetailsAsync_RoundsLineTotals_HalfEvenToTwoDecimals()
        {
            // Arrange
            SetupHappyPath();
            var service = CreateService();
            var purchase = new Purchase { SupplierId = 5, PurchaseDate = DateTime.UtcNow };

            // 0.125m * 1 = 0.125 → banker's round to 2dp = 0.12 (rounds to even).
            var details = new List<PurchaseDetail>
            {
                new PurchaseDetail { ProductId = 1, Quantity = 1, UnitPrice = 0.125m }
            };
            _productRepoMock.Setup(r => r.TryIncrementStockAsync(1, 1, It.IsAny<int>(), "Purchase", It.IsAny<CancellationToken>()))
                            .ReturnsAsync(true);

            Purchase? capturedPurchase = null;
            _purchaseRepoMock.Setup(r => r.AddAsync(It.IsAny<Purchase>(), It.IsAny<CancellationToken>()))
                             .Callback<Purchase, CancellationToken>((p, _) => capturedPurchase = p)
                             .ReturnsAsync((Purchase p, CancellationToken _) => p);

            // Act
            await service.CreatePurchaseWithDetailsAsync(purchase, details);

            // Assert: NetAmount is rounded to 2dp. 0.125 → 0.12 (MidpointRounding.ToEven).
            Assert.NotNull(capturedPurchase);
            Assert.Equal(0.12m, capturedPurchase!.NetAmount);
        }

        [Fact]
        public async Task CreatePurchaseWithDetailsAsync_SupplierOutstanding_CreatedForCreditPurchase()
        {
            // Arrange
            SetupHappyPath(supplierId: 5, invoiceNo: "PUR-2026-000001");
            var service = CreateService();
            var purchase = new Purchase { SupplierId = 5, PurchaseDate = DateTime.UtcNow, PaidAmount = 0m };
            var details = new List<PurchaseDetail>
            {
                new PurchaseDetail { ProductId = 1, Quantity = 2, UnitPrice = 50m }
            };

            // Act
            await service.CreatePurchaseWithDetailsAsync(purchase, details);

            // Assert: SupplierOutstanding row created with Debit=0, Credit=100, Balance=100
            _outstandingMock.Verify(o => o.AddSupplierOutstandingAsync(
                It.Is<SupplierOutstanding>(so =>
                    so.SupplierId == 5 &&
                    so.PurchaseId == 42 &&
                    so.DebitAmount == 0m &&
                    so.CreditAmount == 100m &&
                    so.Balance == 100m &&
                    so.Status == "Open"),
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task CreatePurchaseWithDetailsAsync_FullyPaid_DoesNotCreateOutstanding()
        {
            // Arrange
            SetupHappyPath();
            var service = CreateService();
            // NetAmount = 100, PaidAmount = 100 — fully paid, no outstanding.
            var purchase = new Purchase { SupplierId = 5, PurchaseDate = DateTime.UtcNow, PaidAmount = 100m };
            var details = new List<PurchaseDetail>
            {
                new PurchaseDetail { ProductId = 1, Quantity = 2, UnitPrice = 50m }
            };

            // Act
            await service.CreatePurchaseWithDetailsAsync(purchase, details);

            // Assert
            _outstandingMock.Verify(o => o.AddSupplierOutstandingAsync(It.IsAny<SupplierOutstanding>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task CreatePurchaseWithDetailsAsync_AuditFailure_RollsBackEntireTransaction()
        {
            // Arrange
            SetupHappyPath();
            _auditServiceMock.Setup(a => a.LogAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(),
                It.IsAny<object>(), It.IsAny<object>(), It.IsAny<int?>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("audit DB down"));
            _unitOfWorkMock.Setup(r => r.RollbackAsync(It.IsAny<CancellationToken>()))
                            .Returns(Task.CompletedTask);

            var service = CreateService();
            var purchase = new Purchase { SupplierId = 5, PurchaseDate = DateTime.UtcNow };
            var details = new List<PurchaseDetail> { new PurchaseDetail { ProductId = 1, Quantity = 1, UnitPrice = 10m } };

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreatePurchaseWithDetailsAsync(purchase, details));
            _unitOfWorkMock.Verify(r => r.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWorkMock.Verify(r => r.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task ReceivePurchaseAsync_UsesAtomicStockIncrement_AndWritesMovement()
        {
            // Arrange
            var purchase = new Purchase { Id = 42, SupplierId = 5, InvoiceNo = "PUR-2026-000001", Status = "Active" };
            var existingDetails = new List<PurchaseDetail>
            {
                new PurchaseDetail { Id = 10, PurchaseId = 42, ProductId = 1, Quantity = 5, UnitPrice = 10m, ReceivedQuantity = 0 },
                new PurchaseDetail { Id = 11, PurchaseId = 42, ProductId = 2, Quantity = 3, UnitPrice = 7m, ReceivedQuantity = 0 }
            };
            _purchaseRepoMock.Setup(r => r.GetByIdAsync(42, It.IsAny<CancellationToken>()))
                             .ReturnsAsync(purchase);
            _detailRepoMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                           .ReturnsAsync(existingDetails);
            _productRepoMock.Setup(r => r.TryIncrementStockAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                            .ReturnsAsync(true);
            _unitOfWorkMock.Setup(r => r.BeginTransactionAsync(It.IsAny<CancellationToken>()))
                            .Returns(Task.CompletedTask);
            _unitOfWorkMock.Setup(r => r.CommitAsync(It.IsAny<CancellationToken>()))
                            .Returns(Task.CompletedTask);

            var service = CreateService();

            // Act
            var receivedItems = new List<(int, int)>
            {
                (10, 5),  // receive all 5 of product 1
                (11, 1)   // receive 1 of 3 of product 2
            };
            await service.ReceivePurchaseAsync(42, 1, receivedItems);

            // Assert: atomic stock increment (NOT the old non-atomic AdjustStockAsync)
            _productRepoMock.Verify(r => r.TryIncrementStockAsync(1, 5, It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
            _productRepoMock.Verify(r => r.TryIncrementStockAsync(2, 1, It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
            _productRepoMock.Verify(r => r.AdjustStockAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
            // Stock movement created inside transaction
            _stockMovementMock.Verify(s => s.AddPurchaseMovementAsync(
                It.IsAny<Purchase>(), It.IsAny<IReadOnlyList<PurchaseDetail>>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()), Times.Once);
            // Audit inside transaction
            _auditServiceMock.Verify(a => a.LogAsync(nameof(Purchase), 42, "Receive", null, It.IsAny<object>(),
                null, null, It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task CreatePurchaseWithDetailsAsync_EmptyDetails_ThrowsValidationException()
        {
            var service = CreateService();
            var purchase = new Purchase { SupplierId = 5, PurchaseDate = DateTime.UtcNow };

            await Assert.ThrowsAsync<ValidationException>(() =>
                service.CreatePurchaseWithDetailsAsync(purchase, Array.Empty<PurchaseDetail>()));
        }
    }
}
