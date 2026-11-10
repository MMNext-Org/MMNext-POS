using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MMNextPOS.Application.Services;
using MMNextPOS.Domain.Models;
using MMNextPOS.Infrastructure.Repositories;
using Moq;
using Xunit;

namespace MMNextPOS.Application.Tests
{
    public class PurchaseReceiptServiceTests
    {
        private readonly Mock<IPurchaseReceiptRepository> _receiptRepoMock = new();
        private readonly Mock<IPurchaseReceiptDetailRepository> _detailRepoMock = new();
        private readonly Mock<IAuditService> _auditServiceMock = new();

        private IPurchaseReceiptService CreateService()
        {
            return new PurchaseReceiptService(
                _receiptRepoMock.Object,
                _detailRepoMock.Object,
                _auditServiceMock.Object);
        }

        [Fact]
        public async Task GetByIdAsync_ExistingReceipt_ReturnsReceipt()
        {
            // Arrange
            var receipt = new PurchaseReceipt { Id = 1, ReceiptNo = "PR-001", PurchaseId = 1 };
            _receiptRepoMock.Setup(r => r.GetByIdAsync(receipt.Id, It.IsAny<CancellationToken>()))
                            .ReturnsAsync(receipt);

            var service = CreateService();

            // Act
            var result = await service.GetByIdAsync(receipt.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(receipt.Id, result.Id);
            Assert.Equal(receipt.ReceiptNo, result.ReceiptNo);
        }

        [Fact]
        public async Task GetByIdAsync_NonExistentReceipt_ReturnsNull()
        {
            // Arrange
            _receiptRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                            .ReturnsAsync((PurchaseReceipt?)null!);

            var service = CreateService();

            // Act
            var result = await service.GetByIdAsync(999);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetByPurchaseIdAsync_ExistingReceipt_ReturnsReceipt()
        {
            // Arrange
            var purchaseId = 1;
            var receipt = new PurchaseReceipt { Id = 1, ReceiptNo = "PR-001", PurchaseId = purchaseId };
            _receiptRepoMock.Setup(r => r.GetByPurchaseIdAsync(purchaseId, It.IsAny<CancellationToken>()))
                            .ReturnsAsync(receipt);

            var service = CreateService();

            // Act
            var result = await service.GetByPurchaseIdAsync(purchaseId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(purchaseId, result.PurchaseId);
        }

        [Fact]
        public async Task GetByDateRangeAsync_ReturnsReceiptsInRange()
        {
            // Arrange
            var fromDate = new DateTime(2026, 1, 1);
            var toDate = new DateTime(2026, 1, 31);
            var receipts = new List<PurchaseReceipt>
            {
                new() { Id = 1, ReceiptNo = "PR-001", PurchaseDate = new DateTime(2026, 1, 10), TotalAmount = 100m },
                new() { Id = 2, ReceiptNo = "PR-002", PurchaseDate = new DateTime(2026, 1, 20), TotalAmount = 200m }
            };
            _receiptRepoMock.Setup(r => r.GetByDateRangeAsync(fromDate, toDate, It.IsAny<CancellationToken>()))
                            .ReturnsAsync(receipts);

            var service = CreateService();

            // Act
            var result = await service.GetByDateRangeAsync(fromDate, toDate);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task CreateReceiptAsync_ValidReceiptAndDetails_CreatesAndLogsAudit()
        {
            // Arrange
            var receipt = new PurchaseReceipt { ReceiptNo = "PR-001", PurchaseId = 1, TotalAmount = 100m };
            var details = new List<PurchaseReceiptDetail>
            {
                new PurchaseReceiptDetail { ProductName = "Product A", Quantity = 5, UnitPrice = 10m }
            };
            var createdReceipt = new PurchaseReceipt { Id = 1, ReceiptNo = "PR-001", PurchaseId = 1, TotalAmount = 100m };
            _receiptRepoMock.Setup(r => r.AddWithDetailsAsync(receipt, details, It.IsAny<CancellationToken>()))
                            .ReturnsAsync(createdReceipt);
            _auditServiceMock.Setup(a => a.LogAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(),
                It.IsAny<object>(), It.IsAny<object>(), It.IsAny<int?>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var service = CreateService();

            // Act
            var result = await service.CreateReceiptAsync(receipt, details);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(createdReceipt.Id, result.Id);
            _receiptRepoMock.Verify(r => r.AddWithDetailsAsync(receipt, details, It.IsAny<CancellationToken>()), Times.Once);
            _auditServiceMock.Verify(a => a.LogAsync(nameof(PurchaseReceipt), createdReceipt.Id, "Create", null, createdReceipt, It.IsAny<int?>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task UpdateReceiptAsync_ExistingReceipt_UpdatesAndLogsAudit()
        {
            // Arrange
            var existing = new PurchaseReceipt { Id = 1, ReceiptNo = "PR-001", TotalAmount = 100m };
            var updated = new PurchaseReceipt { Id = 1, ReceiptNo = "PR-001-UPDATED", TotalAmount = 150m };
            _receiptRepoMock.Setup(r => r.GetByIdAsync(existing.Id, It.IsAny<CancellationToken>()))
                            .ReturnsAsync(existing);
            _receiptRepoMock.Setup(r => r.UpdateAsync(updated, It.IsAny<CancellationToken>()))
                            .Returns(Task.CompletedTask);
            _auditServiceMock.Setup(a => a.LogAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(),
                It.IsAny<object>(), It.IsAny<object>(), It.IsAny<int?>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var service = CreateService();

            // Act
            await service.UpdateReceiptAsync(updated);

            // Assert
            _receiptRepoMock.Verify(r => r.UpdateAsync(It.Is<PurchaseReceipt>(r => r.TotalAmount == 150m && r.ReceiptNo == "PR-001-UPDATED"), It.IsAny<CancellationToken>()), Times.Once);
            _auditServiceMock.Verify(a => a.LogAsync(nameof(PurchaseReceipt), updated.Id, "Update", existing, updated, It.IsAny<int?>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task DeleteReceiptAsync_ExistingReceipt_DeletesAndLogsAudit()
        {
            // Arrange
            var existing = new PurchaseReceipt { Id = 1, ReceiptNo = "PR-001", TotalAmount = 100m };
            _receiptRepoMock.Setup(r => r.GetByIdAsync(existing.Id, It.IsAny<CancellationToken>()))
                            .ReturnsAsync(existing);
            _receiptRepoMock.Setup(r => r.DeleteAsync(existing.Id, It.IsAny<CancellationToken>()))
                            .Returns(Task.CompletedTask);
            _auditServiceMock.Setup(a => a.LogAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(),
                It.IsAny<object>(), It.IsAny<object>(), It.IsAny<int?>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var service = CreateService();

            // Act
            await service.DeleteReceiptAsync(existing.Id);

            // Assert
            _receiptRepoMock.Verify(r => r.DeleteAsync(existing.Id, It.IsAny<CancellationToken>()), Times.Once);
            _auditServiceMock.Verify(a => a.LogAsync(nameof(PurchaseReceipt), existing.Id, "Delete", existing, null, It.IsAny<int?>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetReceiptDetailsAsync_ExistingReceipt_ReturnsDetails()
        {
            // Arrange
            var receiptId = 1;
            var details = new List<PurchaseReceiptDetail>
            {
                new() { Id = 1, PurchaseReceiptId = receiptId, ProductName = "Product A", Quantity = 5 },
                new() { Id = 2, PurchaseReceiptId = receiptId, ProductName = "Product B", Quantity = 3 }
            };
            _detailRepoMock.Setup(r => r.GetByReceiptIdAsync(receiptId, It.IsAny<CancellationToken>()))
                           .ReturnsAsync(details);

            var service = CreateService();

            // Act
            var result = await service.GetReceiptDetailsAsync(receiptId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task GetByIdAsync_RepositoryThrows_ThrowsException()
        {
            // Arrange
            var ex = new InvalidOperationException("DB error");
            _receiptRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                            .ThrowsAsync(ex);

            var service = CreateService();

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => service.GetByIdAsync(1));
        }
    }
}
