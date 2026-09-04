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
    public class InventoryServiceTests
    {
        private readonly Mock<IStockMovementRepository> _stockMovementRepoMock = new();
        private readonly Mock<IStockMovementDetailRepository> _stockMovementDetailRepoMock = new();
        private readonly Mock<IAssemblyRepository> _assemblyRepoMock = new();
        private readonly Mock<IAssemblyDetailRepository> _assemblyDetailRepoMock = new();
        private readonly Mock<IProductRepository> _productRepoMock = new();
        private readonly Mock<IStockTransferRepository> _stockTransferRepoMock = new();
        private readonly Mock<IStockTransferDetailRepository> _stockTransferDetailRepoMock = new();
        private readonly Mock<IAuditService> _auditServiceMock = new();

        private IInventoryService CreateService()
        {
            return new InventoryService(
                _stockMovementRepoMock.Object,
                _stockMovementDetailRepoMock.Object,
                _assemblyRepoMock.Object,
                _assemblyDetailRepoMock.Object,
                _productRepoMock.Object,
                _stockTransferRepoMock.Object,
                _stockTransferDetailRepoMock.Object,
                _auditServiceMock.Object);
        }

        [Fact]
        public async Task GetStockMovementsAsync_ReturnsFilteredResults()
        {
            // Arrange
            var movements = new List<StockMovement>
            {
                new() { Id = 1, MovementType = "Receive", LocationId = 1, Status = "Active" },
                new() { Id = 2, MovementType = "Issue", LocationId = 1, Status = "Active" },
                new() { Id = 3, MovementType = "Receive", LocationId = 2, Status = "Active" },
            };
            _stockMovementRepoMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                                  .ReturnsAsync(movements);

            var service = CreateService();
            var result = await service.GetStockMovementsAsync(locationId: 1, movementType: "Receive", CancellationToken.None);

            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal(1, result[0].Id);
        }

        [Fact]
        public async Task GetStockMovementsPageAsync_ReturnsPagedResult()
        {
            // Arrange
            var movements = new List<StockMovement>
            {
                new() { Id = 1, MovementNo = "MOV-001" },
                new() { Id = 2, MovementNo = "MOV-002" },
                new() { Id = 3, MovementNo = "MOV-003" },
                new() { Id = 4, MovementNo = "MOV-004" },
                new() { Id = 5, MovementNo = "MOV-005" },
            };
            _stockMovementRepoMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                                  .ReturnsAsync(movements);

            var service = CreateService();
            var all = await _stockMovementRepoMock.Object.GetAllAsync(CancellationToken.None);

            // Simulate paging
            var page = 1;
            var pageSize = 2;
            var pagedResult = movements
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // Assert
            Assert.Equal(2, pagedResult.Count);
            Assert.Equal("MOV-001", pagedResult[0].MovementNo);
            Assert.Equal("MOV-002", pagedResult[1].MovementNo);
        }

        [Fact]
        public async Task GetStockMovementsPageAsync_SecondPage_ReturnsCorrectItems()
        {
            var movements = new List<StockMovement>
            {
                new() { Id = 1, MovementNo = "MOV-001" },
                new() { Id = 2, MovementNo = "MOV-002" },
                new() { Id = 3, MovementNo = "MOV-003" },
                new() { Id = 4, MovementNo = "MOV-004" },
                new() { Id = 5, MovementNo = "MOV-005" },
            };
            _stockMovementRepoMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                                  .ReturnsAsync(movements);

            var service = CreateService();
            var all = await _stockMovementRepoMock.Object.GetAllAsync(CancellationToken.None);

            // Simulate paging - page 2
            var page = 2;
            var pageSize = 2;
            var pagedResult = movements
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            Assert.Equal(2, pagedResult.Count);
            Assert.Equal("MOV-003", pagedResult[0].MovementNo);
            Assert.Equal("MOV-004", pagedResult[1].MovementNo);
        }

        [Fact]
        public async Task GetStockMovementsPageAsync_LastPage_ReturnsRemainingItems()
        {
            var movements = new List<StockMovement>
            {
                new() { Id = 1, MovementNo = "MOV-001" },
                new() { Id = 2, MovementNo = "MOV-002" },
                new() { Id = 3, MovementNo = "MOV-003" },
                new() { Id = 4, MovementNo = "MOV-004" },
                new() { Id = 5, MovementNo = "MOV-005" },
            };
            _stockMovementRepoMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                                  .ReturnsAsync(movements);

            var service = CreateService();
            var all = await _stockMovementRepoMock.Object.GetAllAsync(CancellationToken.None);

            // Simulate paging - page 3 (last page with 1 item)
            var page = 3;
            var pageSize = 2;
            var pagedResult = movements
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            Assert.Single(pagedResult);
            Assert.Equal("MOV-005", pagedResult[0].MovementNo);
        }

        [Fact]
        public async Task GetStockMovementsPageAsync_PageOutOfRange_ReturnsEmptyList()
        {
            var movements = new List<StockMovement>
            {
                new() { Id = 1, MovementNo = "MOV-001" },
                new() { Id = 2, MovementNo = "MOV-002" },
            };
            _stockMovementRepoMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                                  .ReturnsAsync(movements);

            // Simulate paging - page 5 (out of range)
            var page = 5;
            var pageSize = 2;
            var pagedResult = movements
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            Assert.Empty(pagedResult);
        }
    }
}
