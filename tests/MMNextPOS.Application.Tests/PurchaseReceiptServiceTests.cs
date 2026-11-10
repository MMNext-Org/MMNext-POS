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
        private readonly Mock<IAuditService> _auditMock = new();

        private IPurchaseReceiptService CreateService() => new PurchaseReceiptService(
            _receiptRepoMock.Object, _detailRepoMock.Object, _auditMock.Object);

        private static PurchaseReceipt NewReceipt(int id = 1, string receiptNo = "PR-001") => new()
        {
            Id = id,
            ReceiptNo = receiptNo,
            PurchaseId = 10,
            PurchaseDate = DateTime.UtcNow
        };

        private static PurchaseReceiptDetail NewDetail(int id, int productId, int quantity) => new()
        {
            Id = id,
            PurchaseReceiptId = 1,
            ProductSku = "SKU-" + productId,
            ProductName = "Product " + productId,
            Quantity = quantity,
            UnitPrice = 10m,
            LineTotal = quantity * 10m
        };

        [Fact]
        public void Constructor_NullReceiptRepo_ThrowsArgumentNullException() =>
            Assert.Throws<ArgumentNullException>(() => new PurchaseReceiptService(null!, _detailRepoMock.Object, _auditMock.Object));

        [Fact]
        public void Constructor_NullDetailRepo_ThrowsArgumentNullException() =>
            Assert.Throws<ArgumentNullException>(() => new PurchaseReceiptService(_receiptRepoMock.Object, null!, _auditMock.Object));

        [Fact]
        public void Constructor_NullAudit_ThrowsArgumentNullException() =>
            Assert.Throws<ArgumentNullException>(() => new PurchaseReceiptService(_receiptRepoMock.Object, _detailRepoMock.Object, null!));

        [Fact]
        public async Task CreateReceiptAsync_PersistsWithDetailsAndLogsAudit()
        {
            // Arrange
            var receipt = NewReceipt();
            var details = new List<PurchaseReceiptDetail> { NewDetail(1, 100, 3), NewDetail(2, 101, 2) };
            _receiptRepoMock.Setup(r => r.AddWithDetailsAsync(It.IsAny<PurchaseReceipt>(), It.IsAny<IEnumerable<PurchaseReceiptDetail>>(), It.IsAny<CancellationToken>()))
                            .Callback<PurchaseReceipt, IEnumerable<PurchaseReceiptDetail>, CancellationToken>((r, _, __) => r.Id = 42)
                            .ReturnsAsync((PurchaseReceipt r, IEnumerable<PurchaseReceiptDetail> _, CancellationToken __) => r);
            _auditMock.Setup(a => a.LogAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<object>(), It.IsAny<object>(), It.IsAny<int?>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                      .Returns(Task.CompletedTask);

            var service = CreateService();
            var result = await service.CreateReceiptAsync(receipt, details);

            // Assert
            Assert.Equal(42, result.Id);
            _receiptRepoMock.Verify(r => r.AddWithDetailsAsync(receipt, details, It.IsAny<CancellationToken>()), Times.Once);
            _auditMock.Verify(a => a.LogAsync(nameof(PurchaseReceipt), 42, "Create", null, It.IsAny<object>(), It.IsAny<int?>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task UpdateReceiptAsync_FetchesExisting_UpdatesAndLogsAudit()
        {
            // Arrange
            var existing = NewReceipt(1);
            var updated = NewReceipt(1, "PR-002");
            _receiptRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(existing);
            _receiptRepoMock.Setup(r => r.UpdateAsync(It.IsAny<PurchaseReceipt>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
            _auditMock.Setup(a => a.LogAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<object>(), It.IsAny<object>(), It.IsAny<int?>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                      .Returns(Task.CompletedTask);

            var service = CreateService();
            await service.UpdateReceiptAsync(updated);

            _receiptRepoMock.Verify(r => r.UpdateAsync(updated, It.IsAny<CancellationToken>()), Times.Once);
            _auditMock.Verify(a => a.LogAsync(nameof(PurchaseReceipt), 1, "Update", existing, updated, It.IsAny<int?>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task DeleteReceiptAsync_DeletesAndLogsAudit()
        {
            // Arrange
            var existing = NewReceipt(5, "PR-005");
            _receiptRepoMock.Setup(r => r.GetByIdAsync(5, It.IsAny<CancellationToken>())).ReturnsAsync(existing);
            _receiptRepoMock.Setup(r => r.DeleteAsync(5, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
            _auditMock.Setup(a => a.LogAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<object>(), It.IsAny<object>(), It.IsAny<int?>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                      .Returns(Task.CompletedTask);

            var service = CreateService();
            await service.DeleteReceiptAsync(5);

            _receiptRepoMock.Verify(r => r.DeleteAsync(5, It.IsAny<CancellationToken>()), Times.Once);
            _auditMock.Verify(a => a.LogAsync(nameof(PurchaseReceipt), 5, "Delete", existing, null, It.IsAny<int?>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetByIdAsync_PassesThroughToRepo()
        {
            // Arrange
            var receipt = NewReceipt(3);
            _receiptRepoMock.Setup(r => r.GetByIdAsync(3, It.IsAny<CancellationToken>())).ReturnsAsync(receipt);
            var service = CreateService();

            // Act
            var result = await service.GetByIdAsync(3);

            Assert.Same(receipt, result);
        }

        [Fact]
        public async Task GetByPurchaseIdAsync_PassesThroughToRepo()
        {
            // Arrange
            var receipt = NewReceipt(4);
            _receiptRepoMock.Setup(r => r.GetByPurchaseIdAsync(10, It.IsAny<CancellationToken>())).ReturnsAsync(receipt);
            var service = CreateService();

            // Act
            var result = await service.GetByPurchaseIdAsync(10);

            Assert.Same(receipt, result);
        }

        [Fact]
        public async Task GetByDateRangeAsync_PassesThroughToRepo()
        {
            // Arrange
            var from = new DateTime(2026, 1, 1);
            var to = new DateTime(2026, 1, 31);
            var receipts = new List<PurchaseReceipt> { NewReceipt(1), NewReceipt(2) };
            _receiptRepoMock.Setup(r => r.GetByDateRangeAsync(from, to, It.IsAny<CancellationToken>())).ReturnsAsync(receipts);
            var service = CreateService();

            // Act
            var result = await service.GetByDateRangeAsync(from, to);

            Assert.Collection(result, _ => { }, _ => { });
        }

        [Fact]
        public async Task GetReceiptDetailsAsync_PassesThroughToDetailRepo()
        {
            // Arrange
            var details = new List<PurchaseReceiptDetail> { NewDetail(1, 100, 3) };
            _detailRepoMock.Setup(r => r.GetByReceiptIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(details);
            var service = CreateService();

            // Act
            var result = await service.GetReceiptDetailsAsync(1);

            Assert.Single(result);
            Assert.Same(details[0], result.First());
        }
    }
}
