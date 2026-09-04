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
    public class SalesReturnServiceTests
    {
        private readonly Mock<ISalesReturnRepository> _salesReturnRepoMock = new();
        private readonly Mock<IAuditService> _auditServiceMock = new();

        private ISalesReturnService CreateService()
        {
            return new SalesReturnService(_salesReturnRepoMock.Object, _auditServiceMock.Object);
        }

        [Fact]
        public async Task GetAllAsync_ReturnsAllSalesReturns()
        {
            // Arrange
            var returns = new List<SalesReturn>
            {
                new() { Id = 1, ReturnNo = "RET-001", CustomerId = 5, TotalAmount = 100m, Status = "Active" },
                new() { Id = 2, ReturnNo = "RET-002", CustomerId = 7, TotalAmount = 200m, Status = "Active" },
                new() { Id = 3, ReturnNo = "RET-003", CustomerId = 5, TotalAmount = 150m, Status = "Completed" }
            };
            _salesReturnRepoMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                                .ReturnsAsync(returns);

            var service = CreateService();

            // Act
            var result = await service.GetAllAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.Count);
        }

        [Fact]
        public async Task GetByIdAsync_ExistingSalesReturn_ReturnsSalesReturn()
        {
            // Arrange
            var salesReturn = new SalesReturn
            {
                Id = 1,
                ReturnNo = "RET-001",
                CustomerId = 5,
                TotalAmount = 100m,
                Status = "Active"
            };
            _salesReturnRepoMock.Setup(r => r.GetByIdAsync(salesReturn.Id, It.IsAny<CancellationToken>()))
                                .ReturnsAsync(salesReturn);

            var service = CreateService();

            // Act
            var result = await service.GetByIdAsync(salesReturn.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(salesReturn.Id, result.Id);
            Assert.Equal(salesReturn.ReturnNo, result.ReturnNo);
            Assert.Equal(salesReturn.CustomerId, result.CustomerId);
            Assert.Equal(salesReturn.TotalAmount, result.TotalAmount);
            Assert.Equal(salesReturn.Status, result.Status);
        }

        [Fact]
        public async Task GetByIdAsync_NonExistentSalesReturn_ReturnsNull()
        {
            // Arrange
            _salesReturnRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                                .ReturnsAsync((SalesReturn?)null!);

            var service = CreateService();

            // Act
            var result = await service.GetByIdAsync(999);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task AddAsync_ValidSalesReturn_ReturnsAddedSalesReturn()
        {
            // Arrange
            var salesReturn = new SalesReturn
            {
                ReturnNo = "RET-004",
                SaleId = 10,
                CustomerId = 5,
                ReturnDate = DateTime.Today,
                TotalAmount = 200m,
                Reason = "Defective item",
                Status = "Active"
            };
            var addedSalesReturn = new SalesReturn
            {
                Id = 4,
                ReturnNo = "RET-004",
                SaleId = 10,
                CustomerId = 5,
                ReturnDate = DateTime.Today,
                TotalAmount = 200m,
                Reason = "Defective item",
                Status = "Active"
            };
            _salesReturnRepoMock.Setup(r => r.AddAsync(salesReturn, It.IsAny<CancellationToken>()))
                                .ReturnsAsync(addedSalesReturn);
            _auditServiceMock.Setup(a => a.LogAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(),
                It.IsAny<object>(), It.IsAny<object>(), It.IsAny<int?>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var service = CreateService();

            // Act
            var result = await service.AddAsync(salesReturn);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(addedSalesReturn.Id, result.Id);
            Assert.Equal(salesReturn.ReturnNo, result.ReturnNo);
            Assert.Equal(salesReturn.CustomerId, result.CustomerId);
            Assert.Equal(salesReturn.TotalAmount, result.TotalAmount);
            _salesReturnRepoMock.Verify(r => r.AddAsync(salesReturn, It.IsAny<CancellationToken>()), Times.Once);
            _auditServiceMock.Verify(a => a.LogAsync(nameof(SalesReturn), addedSalesReturn.Id, "Create", null, addedSalesReturn, It.IsAny<int?>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_ExistingSalesReturn_UpdatesSalesReturn()
        {
            // Arrange
            var existingSalesReturn = new SalesReturn { Id = 1, ReturnNo = "RET-001", TotalAmount = 100m, Status = "Active" };
            var updatedSalesReturn = new SalesReturn { Id = 1, ReturnNo = "RET-001", TotalAmount = 150m, Status = "Completed" };
            _salesReturnRepoMock.Setup(r => r.GetByIdAsync(existingSalesReturn.Id, It.IsAny<CancellationToken>()))
                                .ReturnsAsync(existingSalesReturn);
            _salesReturnRepoMock.Setup(r => r.UpdateAsync(existingSalesReturn, It.IsAny<CancellationToken>()))
                                .Returns(Task.CompletedTask);
            _auditServiceMock.Setup(a => a.LogAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(),
                It.IsAny<object>(), It.IsAny<object>(), It.IsAny<int?>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var service = CreateService();

            // Act
            await service.UpdateAsync(updatedSalesReturn);

            // Assert
            _salesReturnRepoMock.Verify(r => r.UpdateAsync(It.Is<SalesReturn>(s => s.TotalAmount == 150m && s.Status == "Completed"), It.IsAny<CancellationToken>()), Times.Once);
            _auditServiceMock.Verify(a => a.LogAsync(nameof(SalesReturn), updatedSalesReturn.Id, "Update", existingSalesReturn, updatedSalesReturn, It.IsAny<int?>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_ExistingSalesReturn_DeletesSalesReturn()
        {
            // Arrange
            var salesReturn = new SalesReturn { Id = 1, ReturnNo = "RET-001", TotalAmount = 100m };
            _salesReturnRepoMock.Setup(r => r.GetByIdAsync(salesReturn.Id, It.IsAny<CancellationToken>()))
                                .ReturnsAsync(salesReturn);
            _salesReturnRepoMock.Setup(r => r.DeleteAsync(salesReturn.Id, It.IsAny<CancellationToken>()))
                                .Returns(Task.CompletedTask);
            _auditServiceMock.Setup(a => a.LogAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(),
                It.IsAny<object>(), It.IsAny<object>(), It.IsAny<int?>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var service = CreateService();

            // Act
            await service.DeleteAsync(salesReturn.Id);

            // Assert
            _salesReturnRepoMock.Verify(r => r.DeleteAsync(salesReturn.Id, It.IsAny<CancellationToken>()), Times.Once);
            _auditServiceMock.Verify(a => a.LogAsync(nameof(SalesReturn), salesReturn.Id, "Delete", salesReturn, null, It.IsAny<int?>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetByIdAsync_ThrowsException_WhenRepositoryThrows()
        {
            // Arrange
            var ex = new InvalidOperationException("DB error");
            _salesReturnRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                                .ThrowsAsync(ex);

            var service = CreateService();

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => service.GetByIdAsync(1));
        }
    }
}
