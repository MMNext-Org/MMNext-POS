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
    public class PurchaseDetailServiceTests
    {
        private readonly Mock<IPurchaseDetailRepository> _repoMock = new();
        private readonly Mock<IAuditService> _auditMock = new();

        private IPurchaseDetailService CreateService() => new PurchaseDetailService(_repoMock.Object, _auditMock.Object);

        private static PurchaseDetail NewDetail(int id = 1, int purchaseId = 10, int productId = 100, int quantity = 2) => new()
        {
            Id = id,
            PurchaseId = purchaseId,
            ProductId = productId,
            Quantity = quantity,
            UnitPrice = 10m,
            LineTotal = quantity * 10m
        };

        [Fact]
        public void Constructor_NullRepo_ThrowsArgumentNullException() =>
            Assert.Throws<ArgumentNullException>(() => new PurchaseDetailService(null!, _auditMock.Object));

        [Fact]
        public void Constructor_NullAudit_ThrowsArgumentNullException() =>
            Assert.Throws<ArgumentNullException>(() => new PurchaseDetailService(_repoMock.Object, null!));

        [Fact]
        public async Task AddAsync_PersistsAndLogsAudit()
        {
            // Arrange
            var detail = NewDetail();
            _repoMock.Setup(r => r.AddAsync(It.IsAny<PurchaseDetail>(), It.IsAny<CancellationToken>()))
                     .Callback<PurchaseDetail, CancellationToken>((d, _) => d.Id = 42)
                     .ReturnsAsync((PurchaseDetail d, CancellationToken _) => d);
            _auditMock.Setup(a => a.LogAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<object>(), It.IsAny<object>(), It.IsAny<int?>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                      .Returns(Task.CompletedTask);

            var service = CreateService();
            var result = await service.AddAsync(detail);

            Assert.Equal(42, result.Id);
            _repoMock.Verify(r => r.AddAsync(detail, It.IsAny<CancellationToken>()), Times.Once);
            _auditMock.Verify(a => a.LogAsync(nameof(PurchaseDetail), 42, "Create", null, It.IsAny<object>(), It.IsAny<int?>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_FetchesExisting_UpdatesAndLogsAudit()
        {
            // Arrange
            var existing = NewDetail(1);
            var updated = NewDetail(1, purchaseId: 10, productId: 200, quantity: 5);
            _repoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(existing);
            _repoMock.Setup(r => r.UpdateAsync(It.IsAny<PurchaseDetail>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
            _auditMock.Setup(a => a.LogAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<object>(), It.IsAny<object>(), It.IsAny<int?>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                      .Returns(Task.CompletedTask);

            var service = CreateService();
            await service.UpdateAsync(updated);

            _repoMock.Verify(r => r.UpdateAsync(updated, It.IsAny<CancellationToken>()), Times.Once);
            _auditMock.Verify(a => a.LogAsync(nameof(PurchaseDetail), 1, "Update", existing, updated, It.IsAny<int?>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_DeletesAndLogsAudit()
        {
            // Arrange
            var existing = NewDetail(5);
            _repoMock.Setup(r => r.GetByIdAsync(5, It.IsAny<CancellationToken>())).ReturnsAsync(existing);
            _repoMock.Setup(r => r.DeleteAsync(5, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
            _auditMock.Setup(a => a.LogAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<object>(), It.IsAny<object>(), It.IsAny<int?>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                      .Returns(Task.CompletedTask);

            var service = CreateService();
            await service.DeleteAsync(5);

            _repoMock.Verify(r => r.DeleteAsync(5, It.IsAny<CancellationToken>()), Times.Once);
            _auditMock.Verify(a => a.LogAsync(nameof(PurchaseDetail), 5, "Delete", existing, null, It.IsAny<int?>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetByIdAsync_PassesThroughToRepo()
        {
            // Arrange
            var detail = NewDetail(3);
            _repoMock.Setup(r => r.GetByIdAsync(3, It.IsAny<CancellationToken>())).ReturnsAsync(detail);
            var service = CreateService();

            // Act
            var result = await service.GetByIdAsync(3);

            Assert.Same(detail, result);
        }

        [Fact]
        public async Task GetByPurchaseIdAsync_FiltersByPurchaseId()
        {
            // Arrange
            var all = new List<PurchaseDetail>
            {
                NewDetail(1, purchaseId: 10),
                NewDetail(2, purchaseId: 10),
                NewDetail(3, purchaseId: 20)
            };
            _repoMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(all);
            var service = CreateService();

            // Act
            var result = await service.GetByPurchaseIdAsync(10);

            // Assert
            Assert.Collection(result, _ => { }, _ => { });
            Assert.All(result, d => Assert.Equal(10, d.PurchaseId));
        }
    }
}
