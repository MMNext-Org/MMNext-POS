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
    public class ExpiryManagementServiceTests
    {
        private readonly Mock<ISerialBatchRepository> _batchRepoMock = new();
        private readonly Mock<ISerialNumberRepository> _serialRepoMock = new();
        private readonly Mock<IProductRepository> _productRepoMock = new();
        private readonly Mock<IAuditService> _auditServiceMock = new();

        private ExpiryManagementService CreateService()
        {
            return new ExpiryManagementService(
                _batchRepoMock.Object,
                _serialRepoMock.Object,
                _productRepoMock.Object,
                _auditServiceMock.Object);
        }

        private void SetupHappyPath()
        {
            var today = DateTime.UtcNow;
            var batch1 = new SerialBatch { Id = 1, BatchNumber = "BATCH-001", ProductId = 1, ExpiryDate = today.AddDays(5), IsActive = true };
            var batch2 = new SerialBatch { Id = 2, BatchNumber = "BATCH-002", ProductId = 1, ExpiryDate = today.AddDays(15), IsActive = true };
            var batch3 = new SerialBatch { Id = 3, BatchNumber = "BATCH-003", ProductId = 1, ExpiryDate = today.AddDays(-1), IsActive = false }; // Expired

            _batchRepoMock.Setup(r => r.GetExpiringBatchesAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((int days, CancellationToken _) =>
                {
                    var cutoff = today.AddDays(days);
                    return new List<SerialBatch> { batch1, batch2 }.Where(b => b.ExpiryDate <= cutoff).ToList();
                });
            _batchRepoMock.Setup(r => r.GetExpiredBatchesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<SerialBatch> { batch3 });
            _batchRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((int id, CancellationToken _) => id switch
                {
                    1 => batch1,
                    2 => batch2,
                    3 => batch3,
                    _ => null
                });
            _batchRepoMock.Setup(r => r.GetActiveBatchesByProductAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<SerialBatch> { batch1, batch2 }); // FEFO order: 5 days, then 15 days
            _batchRepoMock.Setup(r => r.UpdateAsync(It.IsAny<SerialBatch>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _serialRepoMock.Setup(r => r.GetByProductIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((int productId, CancellationToken _) =>
                {
                    return new List<SerialNumber> {
                        new SerialNumber { Id = 1, SerialNumberValue = "SN-001", ProductId = productId, BatchId = 1, Status = SerialStatus.Available },
                        new SerialNumber { Id = 2, SerialNumberValue = "SN-002", ProductId = productId, BatchId = 3, Status = SerialStatus.Available }
                    };
                });
            _serialRepoMock.Setup(r => r.UpdateAsync(It.IsAny<SerialNumber>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _productRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Product { Id = 1, Name = "Test Product", StockQuantity = 100 });
            _productRepoMock.Setup(r => r.TryIncrementStockAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            _auditServiceMock.Setup(a => a.LogAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(),
                It.IsAny<object>(), It.IsAny<object>(), It.IsAny<int?>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task GetExpiringBatchesAsync_DefaultThreshold_ReturnsExpiringBatches()
        {
            // Arrange
            SetupHappyPath();
            var service = CreateService();

            // Act
            var result = await service.GetExpiringBatchesAsync(30);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.All(result, batch => Assert.True(batch.IsExpiringSoon(30)));
        }

        [Fact]
        public async Task GetExpiringBatchesAsync_HighThreshold_FiltersCorrectly()
        {
            // Arrange
            SetupHappyPath();
            var service = CreateService();

            // Act
            var result = await service.GetExpiringBatchesAsync(10);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result); // Only batch1 expires within 10 days
        }

        [Fact]
        public async Task GetExpiredBatchesAsync_ReturnsExpiredOnly()
        {
            // Arrange
            SetupHappyPath();
            var service = CreateService();

            // Act
            var result = await service.GetExpiredBatchesAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal("BATCH-003", result[0].BatchNumber);
        }

        [Fact]
        public async Task MarkBatchExpiredAsync_ValidBatch_MarksAsExpired()
        {
            // Arrange
            SetupHappyPath();
            var service = CreateService();

            // Act
            var result = await service.MarkBatchExpiredAsync(1, 1);

            // Assert
            Assert.False(result.IsActive);
            _batchRepoMock.Verify(r => r.UpdateAsync(It.Is<SerialBatch>(b => b.IsActive == false), It.IsAny<CancellationToken>()), Times.Once);
            _auditServiceMock.Verify(a => a.LogAsync(nameof(SerialBatch), 1, "MarkExpired", null, It.IsAny<SerialBatch>(), 1, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task MarkBatchExpiredAsync_NonExistentBatch_ThrowsKeyNotFoundException()
        {
            // Arrange
            SetupHappyPath();
            _batchRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((SerialBatch?)null);
            var service = CreateService();

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => service.MarkBatchExpiredAsync(999, 1));
        }

        [Fact]
        public async Task ProcessExpiryBatchAsync_WithExpiredBatches_ProcessesAll()
        {
            // Arrange
            SetupHappyPath();
            var service = CreateService();

            // Act
            await service.ProcessExpiryBatchAsync();

            // Assert
            _batchRepoMock.Verify(r => r.GetExpiredBatchesAsync(It.IsAny<CancellationToken>()), Times.Once);
            _batchRepoMock.Verify(r => r.UpdateAsync(It.IsAny<SerialBatch>(), It.IsAny<CancellationToken>()), Times.AtLeastOnce());
            _serialRepoMock.Verify(r => r.UpdateAsync(It.IsAny<SerialNumber>(), It.IsAny<CancellationToken>()), Times.AtLeastOnce());
        }

        [Fact]
        public async Task GetFefoBatchesAsync_ReturnsOrderedByExpiryDate()
        {
            // Arrange
            SetupHappyPath();
            var service = CreateService();

            // Act
            var result = await service.GetFefoBatchesAsync(1, 1);

            // Assert
            Assert.NotNull(result);
            Assert.True(result[0].ExpiryDate <= result[1].ExpiryDate); // FEFO order
        }
    }
}
