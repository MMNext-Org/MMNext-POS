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
    public class StockTransferServiceTests
    {
        private readonly Mock<IStockTransferRepository> _transferRepoMock = new();
        private readonly Mock<IStockTransferDetailRepository> _detailRepoMock = new();
        private readonly Mock<IProductRepository> _productRepoMock = new();
        private readonly Mock<IStockMovementService> _stockMovementMock = new();
        private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
        private readonly Mock<IAuditService> _auditServiceMock = new();

        private IStockTransferService CreateService()
        {
            return new StockTransferService(
                _transferRepoMock.Object,
                _detailRepoMock.Object,
                _productRepoMock.Object,
                _stockMovementMock.Object,
                _unitOfWorkMock.Object,
                _auditServiceMock.Object);
        }

        private void SetupHappyPath()
        {
            var transfer = new StockTransfer
            {
                Id = 1,
                TransferNo = "ST-2026-000001",
                FromLocationId = 1,
                ToLocationId = 2,
                Status = "Draft",
                TransferDate = DateTime.UtcNow
            };

            var details = new List<StockTransferDetail>
            {
                new() { Id = 1, StockTransferId = 1, ProductId = 1, Quantity = 10, UnitPrice = 10m, ReceivedQuantity = 0 },
                new() { Id = 2, StockTransferId = 1, ProductId = 2, Quantity = 5, UnitPrice = 20m, ReceivedQuantity = 0 }
            };

            _productRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Product { Id = 1, Name = "Product 1", Price = 10m, StockQuantity = 100 });
            _productRepoMock.Setup(r => r.GetByIdAsync(2, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Product { Id = 2, Name = "Product 2", Price = 20m, StockQuantity = 50 });
            _productRepoMock.Setup(r => r.TryDecrementStockAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);
            _productRepoMock.Setup(r => r.TryIncrementStockAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);
            _transferRepoMock.Setup(r => r.AddAsync(It.IsAny<StockTransfer>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((StockTransfer t, CancellationToken _) => { t.Id = 1; return t; });
            _transferRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(transfer);
            _transferRepoMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<StockTransfer> { transfer });
            _detailRepoMock.Setup(r => r.AddAsync(It.IsAny<StockTransferDetail>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((StockTransferDetail d, CancellationToken _) => { d.Id = 1; return d; });
            _detailRepoMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(details);
            _detailRepoMock.Setup(r => r.UpdateAsync(It.IsAny<StockTransferDetail>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            _stockMovementMock.Setup(s => s.AddTransferOutMovementAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<decimal>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new StockMovement { Id = 1 });
            _stockMovementMock.Setup(s => s.AddTransferInMovementAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<decimal>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new StockMovement { Id = 2 });
            _unitOfWorkMock.Setup(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            _unitOfWorkMock.Setup(u => u.CommitAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            _auditServiceMock.Setup(a => a.LogAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(),
                It.IsAny<object>(), It.IsAny<object>(), It.IsAny<int?>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task CreateTransferAsync_HappyPath_CreatesTransfer()
        {
            // Arrange
            SetupHappyPath();
            var service = CreateService();
            var transfer = new StockTransfer
            {
                FromLocationId = 1,
                ToLocationId = 2,
                TransferDate = DateTime.UtcNow
            };
            var details = new List<StockTransferDetail>
            {
                new() { ProductId = 1, Quantity = 10, UnitPrice = 10m },
                new() { ProductId = 2, Quantity = 5, UnitPrice = 20m }
            };

            // Act
            var result = await service.CreateTransferAsync(transfer, details, 1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            _unitOfWorkMock.Verify(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWorkMock.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
            _transferRepoMock.Verify(r => r.AddAsync(It.IsAny<StockTransfer>(), It.IsAny<CancellationToken>()), Times.Once);
            _detailRepoMock.Verify(r => r.AddAsync(It.IsAny<StockTransferDetail>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
        }

        [Fact]
        public async Task CreateTransferAsync_SameLocations_ThrowsValidationException()
        {
            // Arrange
            var service = CreateService();
            var transfer = new StockTransfer
            {
                FromLocationId = 1,
                ToLocationId = 1, // Same location
                TransferDate = DateTime.UtcNow
            };
            var details = new List<StockTransferDetail>
            {
                new() { ProductId = 1, Quantity = 10, UnitPrice = 10m }
            };

            // Act & Assert
            await Assert.ThrowsAsync<ValidationException>(() => service.CreateTransferAsync(transfer, details, 1));
        }

        [Fact]
        public async Task CreateTransferAsync_InsufficientStock_ThrowsValidationException()
        {
            // Arrange
            _productRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Product { Id = 1, Name = "Product 1", Price = 10m, StockQuantity = 5 }); // Only 5 stock, requesting 10
            var service = CreateService();
            var transfer = new StockTransfer
            {
                FromLocationId = 1,
                ToLocationId = 2,
                TransferDate = DateTime.UtcNow
            };
            var details = new List<StockTransferDetail>
            {
                new() { ProductId = 1, Quantity = 10, UnitPrice = 10m } // Requesting more than available
            };

            // Act & Assert
            await Assert.ThrowsAsync<ValidationException>(() => service.CreateTransferAsync(transfer, details, 1));
        }

        [Fact]
        public async Task ReleaseTransferAsync_HappyPath_ReleasesTransfer()
        {
            // Arrange
            SetupHappyPath();
            var service = CreateService();
            service = AsDraftStatus(service);

            // Act
            var result = await service.ReleaseTransferAsync(1, 1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("InTransit", result.Status);
            _unitOfWorkMock.Verify(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWorkMock.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task ReleaseTransferAsync_NotDraft_ThrowsInvalidOperationException()
        {
            // Arrange
            SetupHappyPath();
            var service = CreateService();
            service = AsStatus(service, "Received"); // Try to release a received transfer

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => service.ReleaseTransferAsync(1, 1));
        }

        [Fact]
        public async Task ReceiveTransferAsync_HappyPath_ReceivesTransfer()
        {
            // Arrange
            SetupHappyPath();
            var service = CreateService();
            service = AsStatus(service, "InTransit");
            var details = new List<StockTransferDetail>
            {
                new() { Id = 1, StockTransferId = 1, ProductId = 1, Quantity = 10, UnitPrice = 10m, ReceivedQuantity = 0 },
                new() { Id = 2, StockTransferId = 1, ProductId = 2, Quantity = 5, UnitPrice = 20m, ReceivedQuantity = 0 }
            };
            _detailRepoMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(details);

            // Act
            var receivedItems = new List<(int, int, string?)> { (1, 10, "SN-001"), (2, 5, "SN-002") };
            var result = await service.ReceiveTransferAsync(1, receivedItems, 1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Received", result.Status);
            _unitOfWorkMock.Verify(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWorkMock.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task ReceiveTransferAsync_PartialReceive_SetsPartialStatus()
        {
            // Arrange
            SetupHappyPath();
            var service = CreateService();
            service = AsStatus(service, "InTransit");
            var details = new List<StockTransferDetail>
            {
                new() { Id = 1, StockTransferId = 1, ProductId = 1, Quantity = 10, UnitPrice = 10m, ReceivedQuantity = 0 },
                new() { Id = 2, StockTransferId = 1, ProductId = 2, Quantity = 5, UnitPrice = 20m, ReceivedQuantity = 0 }
            };
            _detailRepoMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(details);

            // Act - only receive first item
            var receivedItems = new List<(int, int, string?)> { (1, 10, "SN-001") };
            var result = await service.ReceiveTransferAsync(1, receivedItems, 1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("PartiallyReceived", result.Status);
        }

        [Fact]
        public async Task ReceiveTransferAsync_NotInTransit_ThrowsInvalidOperationException()
        {
            // Arrange
            SetupHappyPath();
            var service = CreateService();
            service = AsStatus(service, "Draft"); // Try to receive a draft transfer

            // Act & Assert
            var receivedItems = new List<(int, int, string?)> { (1, 10, null) };
            await Assert.ThrowsAsync<InvalidOperationException>(() => service.ReceiveTransferAsync(1, receivedItems, 1));
        }

        [Fact]
        public async Task CancelTransferAsync_HappyPath_CancelsTransfer()
        {
            // Arrange
            SetupHappyPath();
            var service = CreateService();
            service = AsStatus(service, "Draft");

            // Act
            var result = await service.CancelTransferAsync(1, 1, "Changed mind");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Cancelled", result.Status);
            _unitOfWorkMock.Verify(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWorkMock.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task CancelTransferAsync_CannotCancelReceived_ThrowsInvalidOperationException()
        {
            // Arrange
            SetupHappyPath();
            var service = CreateService();
            service = AsStatus(service, "Received");

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => service.CancelTransferAsync(1, 1, "Too late"));
        }

        [Fact]
        public async Task GetTransfersAsync_NoFilters_ReturnsAllTransfers()
        {
            // Arrange
            SetupHappyPath();
            var service = CreateService();

            // Act
            var result = await service.GetTransfersAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
        }

        [Fact]
        public async Task GetTransfersAsync_FilterByFromLocation_ReturnsFiltered()
        {
            // Arrange
            SetupHappyPath();
            var service = CreateService();

            // Act
            var result = await service.GetTransfersAsync(fromLocationId: 1);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
        }

        [Fact]
        public async Task GetTransfersAsync_FilterByStatus_ReturnsFiltered()
        {
            // Arrange
            SetupHappyPath();
            var service = CreateService();

            // Act
            var result = await service.GetTransfersAsync(status: "Draft");

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
        }

        [Fact]
        public async Task GetInTransitTransfersAsync_ReturnsInTransitTransfers()
        {
            // Arrange
            SetupHappyPath();
            var inTransitTransfer = new StockTransfer { Id = 2, Status = "InTransit" };
            _transferRepoMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<StockTransfer>
                {
                    new StockTransfer { Id = 1, Status = "Draft" },
                    new StockTransfer { Id = 2, Status = "InTransit" },
                    new StockTransfer { Id = 3, Status = "InTransit" }
                });
            var service = CreateService();

            // Act
            var result = await service.GetInTransitTransfersAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task GetTransferDetailsAsync_ReturnsDetailsForTransfer()
        {
            // Arrange
            SetupHappyPath();
            var service = CreateService();

            // Act
            var result = await service.GetTransferDetailsAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count); // Two details in setup
        }

        [Fact]
        public async Task GetTransferCostAsync_CalculatesTotalCost()
        {
            // Arrange
            SetupHappyPath();
            var service = CreateService();

            // Act
            var result = await service.GetTransferCostAsync(1);

            // Assert
            // Expected: 10 * 10 + 5 * 20 = 100 + 100 = 200
            Assert.Equal(200m, result);
        }

        // Helper methods
        private IStockTransferService AsDraftStatus(IStockTransferService service)
        {
            var transfer = _transferRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new StockTransfer { Id = 1, Status = "Draft" });
            return service;
        }

        private IStockTransferService AsStatus(IStockTransferService service, string status)
        {
            _transferRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new StockTransfer { Id = 1, Status = status });
            return service;
        }
    }
}
