using System;
using System.Collections.Generic;
using System.Linq;
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
        private readonly Mock<IAuditService> _auditServiceMock = new();

        private IPurchaseDetailService CreateService()
        {
            return new PurchaseDetailService(_repoMock.Object, _auditServiceMock.Object);
        }

        [Fact]
        public async Task GetByIdAsync_ExistingDetail_ReturnsDetail()
        {
            // Arrange
            var detail = new PurchaseDetail { Id = 1, PurchaseId = 1, ProductId = 1, Quantity = 5 };
            _repoMock.Setup(r => r.GetByIdAsync(detail.Id, It.IsAny<CancellationToken>()))
                     .ReturnsAsync(detail);

            var service = CreateService();

            // Act
            var result = await service.GetByIdAsync(detail.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(detail.Id, result.Id);
            Assert.Equal(detail.PurchaseId, result.PurchaseId);
        }

        [Fact]
        public async Task GetByIdAsync_NonExistentDetail_ReturnsNull()
        {
            // Arrange
            _repoMock.Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync((PurchaseDetail?)null!);

            var service = CreateService();

            // Act
            var result = await service.GetByIdAsync(999);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetByPurchaseIdAsync_ReturnsDetailsForPurchase()
        {
            // Arrange
            var purchaseId = 1;
            var allDetails = new List<PurchaseDetail>
            {
                new() { Id = 1, PurchaseId = purchaseId, ProductId = 1, Quantity = 5 },
                new() { Id = 2, PurchaseId = purchaseId, ProductId = 2, Quantity = 3 },
                new() { Id = 3, PurchaseId = 2, ProductId = 1, Quantity = 2 }
            };
            _repoMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                     .ReturnsAsync(allDetails);

            var service = CreateService();

            // Act
            var result = await service.GetByPurchaseIdAsync(purchaseId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.All(result, d => Assert.Equal(purchaseId, d.PurchaseId));
        }

        [Fact]
        public async Task GetByPurchaseIdAsync_NoDetails_ReturnsEmptyList()
        {
            // Arrange
            var purchaseId = 999;
            var allDetails = new List<PurchaseDetail>
            {
                new() { Id = 1, PurchaseId = 1, ProductId = 1, Quantity = 5 }
            };
            _repoMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                     .ReturnsAsync(allDetails);

            var service = CreateService();

            // Act
            var result = await service.GetByPurchaseIdAsync(purchaseId);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task AddAsync_ValidDetail_ReturnsAddedDetailAndLogsAudit()
        {
            // Arrange
            var detail = new PurchaseDetail { PurchaseId = 1, ProductId = 1, Quantity = 5, UnitPrice = 10m };
            var addedDetail = new PurchaseDetail { Id = 1, PurchaseId = 1, ProductId = 1, Quantity = 5, UnitPrice = 10m };
            _repoMock.Setup(r => r.AddAsync(detail, It.IsAny<CancellationToken>()))
                     .ReturnsAsync(addedDetail);
            _auditServiceMock.Setup(a => a.LogAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(),
                It.IsAny<object>(), It.IsAny<object>(), It.IsAny<int?>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var service = CreateService();

            // Act
            var result = await service.AddAsync(detail);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(addedDetail.Id, result.Id);
            _repoMock.Verify(r => r.AddAsync(detail, It.IsAny<CancellationToken>()), Times.Once);
            _auditServiceMock.Verify(a => a.LogAsync(nameof(PurchaseDetail), addedDetail.Id, "Create", null, addedDetail, It.IsAny<int?>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_ExistingDetail_UpdatesAndLogsAudit()
        {
            // Arrange
            var existing = new PurchaseDetail { Id = 1, PurchaseId = 1, ProductId = 1, Quantity = 5, UnitPrice = 10m };
            var updated = new PurchaseDetail { Id = 1, PurchaseId = 1, ProductId = 1, Quantity = 10, UnitPrice = 10m };
            _repoMock.Setup(r => r.GetByIdAsync(existing.Id, It.IsAny<CancellationToken>()))
                     .ReturnsAsync(existing);
            _repoMock.Setup(r => r.UpdateAsync(updated, It.IsAny<CancellationToken>()))
                     .Returns(Task.CompletedTask);
            _auditServiceMock.Setup(a => a.LogAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(),
                It.IsAny<object>(), It.IsAny<object>(), It.IsAny<int?>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var service = CreateService();

            // Act
            await service.UpdateAsync(updated);

            // Assert
            _repoMock.Verify(r => r.UpdateAsync(It.Is<PurchaseDetail>(d => d.Quantity == 10), It.IsAny<CancellationToken>()), Times.Once);
            _auditServiceMock.Verify(a => a.LogAsync(nameof(PurchaseDetail), updated.Id, "Update", existing, updated, It.IsAny<int?>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_ExistingDetail_DeletesAndLogsAudit()
        {
            // Arrange
            var existing = new PurchaseDetail { Id = 1, PurchaseId = 1, ProductId = 1, Quantity = 5 };
            _repoMock.Setup(r => r.GetByIdAsync(existing.Id, It.IsAny<CancellationToken>()))
                     .ReturnsAsync(existing);
            _repoMock.Setup(r => r.DeleteAsync(existing.Id, It.IsAny<CancellationToken>()))
                     .Returns(Task.CompletedTask);
            _auditServiceMock.Setup(a => a.LogAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(),
                It.IsAny<object>(), It.IsAny<object>(), It.IsAny<int?>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var service = CreateService();

            // Act
            await service.DeleteAsync(existing.Id);

            // Assert
            _repoMock.Verify(r => r.DeleteAsync(existing.Id, It.IsAny<CancellationToken>()), Times.Once);
            _auditServiceMock.Verify(a => a.LogAsync(nameof(PurchaseDetail), existing.Id, "Delete", existing, null, It.IsAny<int?>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetByIdAsync_RepositoryThrows_ThrowsException()
        {
            // Arrange
            var ex = new InvalidOperationException("DB error");
            _repoMock.Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                     .ThrowsAsync(ex);

            var service = CreateService();

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => service.GetByIdAsync(1));
        }
    }
}
