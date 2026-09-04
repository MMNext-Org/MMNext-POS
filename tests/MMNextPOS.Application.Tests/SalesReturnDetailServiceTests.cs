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
    public class SalesReturnDetailServiceTests
    {
        private readonly Mock<ISalesReturnDetailRepository> _salesReturnDetailRepoMock = new();
        private readonly Mock<IAuditService> _auditServiceMock = new();

        private ISalesReturnDetailService CreateService()
        {
            return new SalesReturnDetailService(_salesReturnDetailRepoMock.Object, _auditServiceMock.Object);
        }

        [Fact]
        public async Task GetAllAsync_ReturnsAllSalesReturnDetails()
        {
            // Arrange
            var details = new List<SalesReturnDetail>
            {
                new() { Id = 1, SalesReturnId = 1, ProductId = 10, Quantity = 2, UnitPrice = 50m, Reason = "Defective" },
                new() { Id = 2, SalesReturnId = 1, ProductId = 15, Quantity = 1, UnitPrice = 75m, Reason = "Wrong item" },
                new() { Id = 3, SalesReturnId = 2, ProductId = 20, Quantity = 3, UnitPrice = 25m, Reason = "Damaged" }
            };
            _salesReturnDetailRepoMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                                      .ReturnsAsync(details);

            var service = CreateService();

            // Act
            var result = await service.GetAllAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.Count);
        }

        [Fact]
        public async Task GetByIdAsync_ExistingSalesReturnDetail_ReturnsSalesReturnDetail()
        {
            // Arrange
            var detail = new SalesReturnDetail
            {
                Id = 1,
                SalesReturnId = 1,
                ProductId = 10,
                Quantity = 2,
                UnitPrice = 50m,
                Reason = "Defective"
            };
            _salesReturnDetailRepoMock.Setup(r => r.GetByIdAsync(detail.Id, It.IsAny<CancellationToken>()))
                                      .ReturnsAsync(detail);

            var service = CreateService();

            // Act
            var result = await service.GetByIdAsync(detail.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(detail.Id, result.Id);
            Assert.Equal(detail.SalesReturnId, result.SalesReturnId);
            Assert.Equal(detail.ProductId, result.ProductId);
            Assert.Equal(detail.Quantity, result.Quantity);
            Assert.Equal(detail.UnitPrice, result.UnitPrice);
            Assert.Equal(detail.Reason, result.Reason);
        }

        [Fact]
        public async Task GetByIdAsync_NonExistentSalesReturnDetail_ReturnsNull()
        {
            // Arrange
            _salesReturnDetailRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                                      .ReturnsAsync((SalesReturnDetail?)null!);

            var service = CreateService();

            // Act
            var result = await service.GetByIdAsync(999);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task AddAsync_ValidSalesReturnDetail_ReturnsAddedSalesReturnDetail()
        {
            // Arrange
            var detail = new SalesReturnDetail
            {
                SalesReturnId = 1,
                ProductId = 10,
                Quantity = 2,
                UnitPrice = 50m,
                Reason = "Defective"
            };
            var addedDetail = new SalesReturnDetail
            {
                Id = 4,
                SalesReturnId = 1,
                ProductId = 10,
                Quantity = 2,
                UnitPrice = 50m,
                Reason = "Defective"
            };
            _salesReturnDetailRepoMock.Setup(r => r.AddAsync(detail, It.IsAny<CancellationToken>()))
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
            Assert.Equal(detail.SalesReturnId, result.SalesReturnId);
            Assert.Equal(detail.ProductId, result.ProductId);
            Assert.Equal(detail.Quantity, result.Quantity);
            Assert.Equal(detail.UnitPrice, result.UnitPrice);
            _salesReturnDetailRepoMock.Verify(r => r.AddAsync(detail, It.IsAny<CancellationToken>()), Times.Once);
            _auditServiceMock.Verify(a => a.LogAsync(nameof(SalesReturnDetail), addedDetail.Id, "Create", null, addedDetail, It.IsAny<int?>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_ExistingSalesReturnDetail_UpdatesSalesReturnDetail()
        {
            // Arrange
            var existingDetail = new SalesReturnDetail { Id = 1, SalesReturnId = 1, Quantity = 2, UnitPrice = 50m, Reason = "Defective" };
            var updatedDetail = new SalesReturnDetail { Id = 1, SalesReturnId = 1, Quantity = 3, UnitPrice = 55m, Reason = "Wrong item" };
            _salesReturnDetailRepoMock.Setup(r => r.GetByIdAsync(existingDetail.Id, It.IsAny<CancellationToken>()))
                                      .ReturnsAsync(existingDetail);
            _salesReturnDetailRepoMock.Setup(r => r.UpdateAsync(existingDetail, It.IsAny<CancellationToken>()))
                                      .Returns(Task.CompletedTask);
            _auditServiceMock.Setup(a => a.LogAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(),
                It.IsAny<object>(), It.IsAny<object>(), It.IsAny<int?>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var service = CreateService();

            // Act
            await service.UpdateAsync(updatedDetail);

            // Assert
            _salesReturnDetailRepoMock.Verify(r => r.UpdateAsync(It.Is<SalesReturnDetail>(d => d.Quantity == 3 && d.UnitPrice == 55m && d.Reason == "Wrong item"), It.IsAny<CancellationToken>()), Times.Once);
            _auditServiceMock.Verify(a => a.LogAsync(nameof(SalesReturnDetail), updatedDetail.Id, "Update", existingDetail, updatedDetail, It.IsAny<int?>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_ExistingSalesReturnDetail_DeletesSalesReturnDetail()
        {
            // Arrange
            var detail = new SalesReturnDetail { Id = 1, SalesReturnId = 1, Quantity = 2, UnitPrice = 50m };
            _salesReturnDetailRepoMock.Setup(r => r.GetByIdAsync(detail.Id, It.IsAny<CancellationToken>()))
                                      .ReturnsAsync(detail);
            _salesReturnDetailRepoMock.Setup(r => r.DeleteAsync(detail.Id, It.IsAny<CancellationToken>()))
                                      .Returns(Task.CompletedTask);
            _auditServiceMock.Setup(a => a.LogAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(),
                It.IsAny<object>(), It.IsAny<object>(), It.IsAny<int?>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var service = CreateService();

            // Act
            await service.DeleteAsync(detail.Id);

            // Assert
            _salesReturnDetailRepoMock.Verify(r => r.DeleteAsync(detail.Id, It.IsAny<CancellationToken>()), Times.Once);
            _auditServiceMock.Verify(a => a.LogAsync(nameof(SalesReturnDetail), detail.Id, "Delete", detail, null, It.IsAny<int?>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetByIdAsync_ThrowsException_WhenRepositoryThrows()
        {
            // Arrange
            var ex = new InvalidOperationException("DB error");
            _salesReturnDetailRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                                      .ThrowsAsync(ex);

            var service = CreateService();

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => service.GetByIdAsync(1));
        }
    }
}
