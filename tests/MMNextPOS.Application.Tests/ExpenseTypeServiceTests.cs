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
    public class ExpenseTypeServiceTests
    {
        private readonly Mock<IExpenseTypeRepository> _expenseTypeRepoMock = new();
        private readonly Mock<IAuditService> _auditServiceMock = new();

        private IExpenseTypeService CreateService()
        {
            return new ExpenseTypeService(_expenseTypeRepoMock.Object, _auditServiceMock.Object);
        }

        [Fact]
        public async Task GetByIdAsync_ExistingExpenseType_ReturnsExpenseType()
        {
            // Arrange
            var expenseType = new ExpenseType
            {
                Id = 1,
                Code = "RENT",
                Name = "Office Rent",
                Description = "Monthly office rent",
                IsActive = true,
                DisplayOrder = 1
            };
            _expenseTypeRepoMock.Setup(r => r.GetByIdAsync(expenseType.Id, It.IsAny<CancellationToken>()))
                                .ReturnsAsync(expenseType);

            var service = CreateService();

            // Act
            var result = await service.GetByIdAsync(expenseType.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expenseType.Id, result.Id);
            Assert.Equal(expenseType.Code, result.Code);
            Assert.Equal(expenseType.Name, result.Name);
            Assert.True(result.IsActive);
        }

        [Fact]
        public async Task GetByIdAsync_NonExistentExpenseType_ReturnsNull()
        {
            // Arrange
            _expenseTypeRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                                .ReturnsAsync((ExpenseType?)null!);

            var service = CreateService();

            // Act
            var result = await service.GetByIdAsync(999);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetAllAsync_ReturnsAllExpenseTypes()
        {
            // Arrange
            var expenseTypes = new List<ExpenseType>
            {
                new() { Id = 1, Code = "RENT", Name = "Office Rent", IsActive = true },
                new() { Id = 2, Code = "UTIL", Name = "Utilities", IsActive = true },
                new() { Id = 3, Code = "SAL", Name = "Salaries", IsActive = true }
            };
            _expenseTypeRepoMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                                .ReturnsAsync(expenseTypes);

            var service = CreateService();

            // Act
            var result = await service.GetAllAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.Count);
        }

        [Fact]
        public async Task AddAsync_ValidExpenseType_ReturnsAddedExpenseType()
        {
            // Arrange
            var expenseType = new ExpenseType
            {
                Code = "SUPPLY",
                Name = "Office Supplies",
                Description = "Stationery and supplies",
                IsActive = true,
                DisplayOrder = 4
            };
            var addedExpenseType = new ExpenseType
            {
                Id = 4,
                Code = "SUPPLY",
                Name = "Office Supplies",
                Description = "Stationery and supplies",
                IsActive = true,
                DisplayOrder = 4
            };
            _expenseTypeRepoMock.Setup(r => r.AddAsync(expenseType, It.IsAny<CancellationToken>()))
                                .ReturnsAsync(addedExpenseType);
            _auditServiceMock.Setup(a => a.LogAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(),
                It.IsAny<object>(), It.IsAny<object>(), It.IsAny<int?>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var service = CreateService();

            // Act
            var result = await service.AddAsync(expenseType);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(addedExpenseType.Id, result.Id);
            Assert.Equal(expenseType.Code, result.Code);
            Assert.Equal(expenseType.Name, result.Name);
            _expenseTypeRepoMock.Verify(r => r.AddAsync(expenseType, It.IsAny<CancellationToken>()), Times.Once);
            _auditServiceMock.Verify(a => a.LogAsync(nameof(ExpenseType), addedExpenseType.Id, "Create", null, addedExpenseType, It.IsAny<int?>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_ExistingExpenseType_UpdatesExpenseType()
        {
            // Arrange
            var existingExpenseType = new ExpenseType { Id = 1, Code = "RENT", Name = "Old Name", IsActive = true };
            var updatedExpenseType = new ExpenseType { Id = 1, Code = "RENT", Name = "New Name", IsActive = false };
            _expenseTypeRepoMock.Setup(r => r.GetByIdAsync(existingExpenseType.Id, It.IsAny<CancellationToken>()))
                                .ReturnsAsync(existingExpenseType);
            _expenseTypeRepoMock.Setup(r => r.UpdateAsync(existingExpenseType, It.IsAny<CancellationToken>()))
                                .Returns(Task.CompletedTask);
            _auditServiceMock.Setup(a => a.LogAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(),
                It.IsAny<object>(), It.IsAny<object>(), It.IsAny<int?>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var service = CreateService();

            // Act
            await service.UpdateAsync(updatedExpenseType);

            // Assert
            _expenseTypeRepoMock.Verify(r => r.UpdateAsync(It.Is<ExpenseType>(e => e.Name == "New Name" && e.IsActive == false), It.IsAny<CancellationToken>()), Times.Once);
            _auditServiceMock.Verify(a => a.LogAsync(nameof(ExpenseType), updatedExpenseType.Id, "Update", existingExpenseType, updatedExpenseType, It.IsAny<int?>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_ExistingExpenseType_DeletesExpenseType()
        {
            // Arrange
            var expenseType = new ExpenseType { Id = 1, Code = "RENT", Name = "Office Rent" };
            _expenseTypeRepoMock.Setup(r => r.GetByIdAsync(expenseType.Id, It.IsAny<CancellationToken>()))
                                .ReturnsAsync(expenseType);
            _expenseTypeRepoMock.Setup(r => r.DeleteAsync(expenseType.Id, It.IsAny<CancellationToken>()))
                                .Returns(Task.CompletedTask);
            _auditServiceMock.Setup(a => a.LogAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(),
                It.IsAny<object>(), It.IsAny<object>(), It.IsAny<int?>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var service = CreateService();

            // Act
            await service.DeleteAsync(expenseType.Id);

            // Assert
            _expenseTypeRepoMock.Verify(r => r.DeleteAsync(expenseType.Id, It.IsAny<CancellationToken>()), Times.Once);
            _auditServiceMock.Verify(a => a.LogAsync(nameof(ExpenseType), expenseType.Id, "Delete", expenseType, null, It.IsAny<int?>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetByIdAsync_ThrowsException_WhenRepositoryThrows()
        {
            // Arrange
            var ex = new InvalidOperationException("DB error");
            _expenseTypeRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                                .ThrowsAsync(ex);

            var service = CreateService();

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => service.GetByIdAsync(1));
        }
    }
}
