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
    public class SerialNumberServiceTests
    {
        private readonly Mock<ISerialNumberRepository> _serialNumberRepoMock = new();
        private readonly Mock<ISerialBatchRepository> _batchRepoMock = new();
        private readonly Mock<ISerialTrackingRepository> _trackingRepoMock = new();
        private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
        private readonly Mock<IAuditService> _auditServiceMock = new();

        private ISerialNumberService CreateService()
        {
            return new SerialNumberService(
                _serialNumberRepoMock.Object,
                _batchRepoMock.Object,
                _trackingRepoMock.Object,
                _unitOfWorkMock.Object,
                _auditServiceMock.Object);
        }

        private void SetupHappyPath()
        {
            _serialNumberRepoMock.Setup(r => r.AddAsync(It.IsAny<SerialNumber>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((SerialNumber s, CancellationToken _) => { s.Id = 1; return s; });
            _serialNumberRepoMock.Setup(r => r.GetBySerialNumberAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new SerialNumber { Id = 1, SerialNumberValue = "SN-0001-2026-000001", ProductId = 1, Status = SerialStatus.Available, LocationId = 1 });
            _serialNumberRepoMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<SerialNumber> { new() { Id = 1, SerialNumberValue = "SN-0001-2026-000001", ProductId = 1, Status = SerialStatus.Available, LocationId = 1 } });
            _serialNumberRepoMock.Setup(r => r.GetByProductIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<SerialNumber> { new() { Id = 1, SerialNumberValue = "SN-0001-2026-000001", ProductId = 1, Status = SerialStatus.Available, LocationId = 1 } });
            _serialNumberRepoMock.Setup(r => r.GetAvailableByProductIdAsync(It.IsAny<int>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<SerialNumber> { new() { Id = 1, SerialNumberValue = "SN-0001-2026-000001", ProductId = 1, Status = SerialStatus.Available, LocationId = 1 } });
            _trackingRepoMock.Setup(r => r.AddAsync(It.IsAny<SerialTracking>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new SerialTracking());
            _batchRepoMock.Setup(r => r.GetByBatchNumberAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new SerialBatch { Id = 1, BatchNumber = "BATCH-001", ProductId = 1 });
            _auditServiceMock.Setup(a => a.LogAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(),
                It.IsAny<object?>(), It.IsAny<object?>(), It.IsAny<int?>(), It.IsAny<string?>(),
                It.IsAny<string?>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        // Helper to override GetBySerialNumberAsync for sold status
        private void SetupSerialAsSold()
        {
            _serialNumberRepoMock.Invocations.Clear(); // Reset to avoid conflicts
            _serialNumberRepoMock.Setup(r => r.AddAsync(It.IsAny<SerialNumber>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((SerialNumber s, CancellationToken _) => { s.Id = 1; return s; });
            _serialNumberRepoMock.Setup(r => r.GetBySerialNumberAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new SerialNumber { Id = 1, SerialNumberValue = "SN-0001-2026-000001", ProductId = 1, Status = SerialStatus.Sold, LocationId = 1 });
            _serialNumberRepoMock.Setup(r => r.UpdateAsync(It.IsAny<SerialNumber>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            _trackingRepoMock.Setup(r => r.AddAsync(It.IsAny<SerialTracking>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new SerialTracking());
            _auditServiceMock.Setup(a => a.LogAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(),
                It.IsAny<object?>(), It.IsAny<object?>(), It.IsAny<int?>(), It.IsAny<string?>(),
                It.IsAny<string?>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task GenerateSerialAsync_ValidInputs_CreatesSerialNumber()
        {
            // Arrange
            SetupHappyPath();
            var service = CreateService();

            // Act
            var result = await service.GenerateSerialAsync(1, 1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            _serialNumberRepoMock.Verify(r => r.AddAsync(It.IsAny<SerialNumber>(), It.IsAny<CancellationToken>()), Times.Once);
            _trackingRepoMock.Verify(r => r.AddAsync(It.IsAny<SerialTracking>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetBySerialNumberAsync_ExistingSerial_ReturnsSerial()
        {
            // Arrange
            SetupHappyPath();
            var service = CreateService();

            // Act
            var result = await service.GetBySerialNumberAsync("SN-0001-2026-000001");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("SN-0001-2026-000001", result.SerialNumberValue);
        }

        [Fact]
        public async Task GetBySerialNumberAsync_NonExistentSerial_ReturnsNull()
        {
            // Arrange
            _serialNumberRepoMock.Setup(r => r.GetBySerialNumberAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((SerialNumber?)null);
            var service = CreateService();

            // Act
            var result = await service.GetBySerialNumberAsync("INVALID");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetByProductIdAsync_ValidProductId_ReturnsSerials()
        {
            // Arrange
            SetupHappyPath();
            var service = CreateService();

            // Act
            var result = await service.GetByProductIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
        }

        [Fact]
        public async Task GetAvailableByProductAsync_ValidProductAndLocation_ReturnsAvailableSerials()
        {
            // Arrange
            SetupHappyPath();
            var service = CreateService();

            // Act
            var result = await service.GetAvailableByProductAsync(1, 1);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
        }

        [Fact]
        public async Task AssignToSaleAsync_ValidSerial_AssignsSerial()
        {
            // Arrange
            SetupHappyPath();
            var service = CreateService();
            var serialNumber = "SN-0001-2026-000001";

            // Act
            var result = await service.AssignToSaleAsync(serialNumber, 1, 1, 1);

            // Assert
            Assert.NotNull(result);
            _serialNumberRepoMock.Verify(r => r.UpdateAsync(It.IsAny<SerialNumber>(), It.IsAny<CancellationToken>()), Times.Once);
            _trackingRepoMock.Verify(r => r.AddAsync(It.IsAny<SerialTracking>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task AssignToSaleAsync_SerialNotFound_ThrowsInvalidOperationException()
        {
            // Arrange
            _serialNumberRepoMock.Setup(r => r.GetBySerialNumberAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((SerialNumber?)null);
            var service = CreateService();

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => service.AssignToSaleAsync("INVALID", 1, 1, 1));
        }

        [Fact]
        public async Task ReturnAsync_ValidSerial_ReturnsSerial()
        {
            // Arrange
            SetupSerialAsSold(); // Only use the sold setup, not the happy path
            var service = CreateService();
            var serialNumber = "SN-0001-2026-000001";

            // Act
            var result = await service.ReturnAsync(serialNumber, 1, 1, "Customer return");

            // Assert
            Assert.NotNull(result);
            _serialNumberRepoMock.Verify(r => r.UpdateAsync(It.IsAny<SerialNumber>(), It.IsAny<CancellationToken>()), Times.Once);
            _trackingRepoMock.Verify(r => r.AddAsync(It.IsAny<SerialTracking>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task TransferAsync_ValidSerial_TransfersSerial()
        {
            // Arrange
            SetupHappyPath();
            var service = CreateService();
            var serialNumber = "SN-0001-2026-000001";

            // Act
            var result = await service.TransferAsync(serialNumber, 1, 2, 1, 1);

            // Assert
            Assert.NotNull(result);
            _serialNumberRepoMock.Verify(r => r.UpdateAsync(It.IsAny<SerialNumber>(), It.IsAny<CancellationToken>()), Times.Once);
            _trackingRepoMock.Verify(r => r.AddAsync(It.IsAny<SerialTracking>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task MarkExpiredAsync_ValidSerial_MarksExpired()
        {
            // Arrange
            SetupHappyPath();
            var service = CreateService();
            var serialNumber = "SN-0001-2026-000001";

            // Act
            var result = await service.MarkExpiredAsync(serialNumber, 1);

            // Assert
            Assert.NotNull(result);
            _serialNumberRepoMock.Verify(r => r.UpdateAsync(It.IsAny<SerialNumber>(), It.IsAny<CancellationToken>()), Times.Once);
            _trackingRepoMock.Verify(r => r.AddAsync(It.IsAny<SerialTracking>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task MarkDamagedAsync_ValidSerial_MarksDamaged()
        {
            // Arrange
            SetupHappyPath();
            var service = CreateService();
            var serialNumber = "SN-0001-2026-000001";

            // Act
            var result = await service.MarkDamagedAsync(serialNumber, "Physical damage", 1);

            // Assert
            Assert.NotNull(result);
            _serialNumberRepoMock.Verify(r => r.UpdateAsync(It.IsAny<SerialNumber>(), It.IsAny<CancellationToken>()), Times.Once);
            _trackingRepoMock.Verify(r => r.AddAsync(It.IsAny<SerialTracking>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task ValidateSerialAsync_ValidSerial_ReturnsTrue()
        {
            // Arrange
            SetupHappyPath();
            var service = CreateService();

            // Act
            var result = await service.ValidateSerialAsync("SN-0001-2026-000001", 1);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task ValidateSerialAsync_InvalidSerial_ReturnsFalse()
        {
            // Arrange
            _serialNumberRepoMock.Setup(r => r.GetBySerialNumberAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((SerialNumber?)null);
            var service = CreateService();

            // Act
            var result = await service.ValidateSerialAsync("INVALID", 1);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task GenerateSerialNumberAsync_GeneratesValidFormat()
        {
            // Arrange
            SetupHappyPath();
            var service = CreateService();

            // Act
            var result = await service.GenerateSerialNumberAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.StartsWith("SN-0001-2026-", result);
            Assert.Contains("-", result.Substring(9)); // Has components
        }

        [Fact]
        public async Task GetAllSerialsAsync_NoLocationFilter_ReturnsAllActiveSerials()
        {
            // Arrange
            SetupHappyPath();
            var service = CreateService();

            // Act
            var result = await service.GetAllSerialsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
        }

        [Fact]
        public async Task GetAllSerialsAsync_WithLocationFilter_ReturnsFilteredSerials()
        {
            // Arrange
            SetupHappyPath();
            var service = CreateService();

            // Act
            var result = await service.GetAllSerialsAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
        }
    }
}
