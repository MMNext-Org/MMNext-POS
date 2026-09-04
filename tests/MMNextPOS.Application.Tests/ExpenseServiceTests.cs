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
    public class ExpenseServiceTests
    {
        private readonly Mock<IExpenseRepository> _expenseRepoMock = new();
        private readonly Mock<IAuditService> _auditServiceMock = new();

        private IExpenseService CreateService()
        {
            return new ExpenseService(_expenseRepoMock.Object, _auditServiceMock.Object);
        }

        [Fact]
        public async Task GetByIdAsync_ExistingExpense_ReturnsExpense()
        {
            // Arrange
            var expense = new Expense
            {
                Id = 1,
                ExpenseNo = "EXP-001",
                ExpenseTypeId = 1,
                ExpenseDate = DateTime.Today,
                Amount = 100.50m,
                PaymentMethod = "Cash",
                Description = "Office rent"
            };
            _expenseRepoMock.Setup(r => r.GetByIdAsync(expense.Id, It.IsAny<CancellationToken>()))
                            .ReturnsAsync(expense);

            var service = CreateService();

            // Act
            var result = await service.GetByIdAsync(expense.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expense.Id, result.Id);
            Assert.Equal(expense.ExpenseNo, result.ExpenseNo);
            Assert.Equal(expense.Amount, result.Amount);
            Assert.Equal(expense.PaymentMethod, result.PaymentMethod);
        }

        [Fact]
        public async Task GetByIdAsync_NonExistentExpense_ReturnsNull()
        {
            // Arrange
            _expenseRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                            .ReturnsAsync((Expense?)null!);

            var service = CreateService();

            // Act
            var result = await service.GetByIdAsync(999);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetAllAsync_ReturnsAllExpenses()
        {
            // Arrange
            var expenses = new List<Expense>
            {
                new() { Id = 1, ExpenseNo = "EXP-001", Amount = 100m, ExpenseDate = DateTime.Today },
                new() { Id = 2, ExpenseNo = "EXP-002", Amount = 200m, ExpenseDate = DateTime.Today },
                new() { Id = 3, ExpenseNo = "EXP-003", Amount = 300m, ExpenseDate = DateTime.Today }
            };
            _expenseRepoMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                            .ReturnsAsync(expenses);

            var service = CreateService();

            // Act
            var result = await service.GetAllAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.Count);
        }

        [Fact]
        public async Task AddAsync_ValidExpense_ReturnsAddedExpense()
        {
            // Arrange
            var expense = new Expense
            {
                ExpenseNo = "EXP-004",
                ExpenseTypeId = 1,
                ExpenseDate = DateTime.Today,
                Amount = 150.75m,
                PaymentMethod = "Card"
            };
            var addedExpense = new Expense
            {
                Id = 4,
                ExpenseNo = "EXP-004",
                ExpenseTypeId = 1,
                ExpenseDate = DateTime.Today,
                Amount = 150.75m,
                PaymentMethod = "Card"
            };
            _expenseRepoMock.Setup(r => r.AddAsync(expense, It.IsAny<CancellationToken>()))
                            .ReturnsAsync(addedExpense);
            _auditServiceMock.Setup(a => a.LogAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(),
                It.IsAny<object>(), It.IsAny<object>(), It.IsAny<int?>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var service = CreateService();

            // Act
            var result = await service.AddAsync(expense);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(addedExpense.Id, result.Id);
            Assert.Equal(expense.ExpenseNo, result.ExpenseNo);
            Assert.Equal(expense.Amount, result.Amount);
            _expenseRepoMock.Verify(r => r.AddAsync(expense, It.IsAny<CancellationToken>()), Times.Once);
            _auditServiceMock.Verify(a => a.LogAsync(nameof(Expense), addedExpense.Id, "Create", null, addedExpense, It.IsAny<int?>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_ExistingExpense_UpdatesExpense()
        {
            // Arrange
            var existingExpense = new Expense { Id = 1, ExpenseNo = "EXP-001", Amount = 100m, Description = "Old" };
            var updatedExpense = new Expense { Id = 1, ExpenseNo = "EXP-001", Amount = 150m, Description = "Updated" };
            _expenseRepoMock.Setup(r => r.GetByIdAsync(existingExpense.Id, It.IsAny<CancellationToken>()))
                            .ReturnsAsync(existingExpense);
            _expenseRepoMock.Setup(r => r.UpdateAsync(existingExpense, It.IsAny<CancellationToken>()))
                            .Returns(Task.CompletedTask);
            _auditServiceMock.Setup(a => a.LogAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(),
                It.IsAny<object>(), It.IsAny<object>(), It.IsAny<int?>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var service = CreateService();

            // Act
            await service.UpdateAsync(updatedExpense);

            // Assert
            _expenseRepoMock.Verify(r => r.UpdateAsync(It.Is<Expense>(e => e.Amount == 150m && e.Description == "Updated"), It.IsAny<CancellationToken>()), Times.Once);
            _auditServiceMock.Verify(a => a.LogAsync(nameof(Expense), updatedExpense.Id, "Update", existingExpense, updatedExpense, It.IsAny<int?>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_ExistingExpense_DeletesExpense()
        {
            // Arrange
            var expense = new Expense { Id = 1, ExpenseNo = "EXP-001", Amount = 100m };
            _expenseRepoMock.Setup(r => r.GetByIdAsync(expense.Id, It.IsAny<CancellationToken>()))
                            .ReturnsAsync(expense);
            _expenseRepoMock.Setup(r => r.DeleteAsync(expense.Id, It.IsAny<CancellationToken>()))
                            .Returns(Task.CompletedTask);
            _auditServiceMock.Setup(a => a.LogAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(),
                It.IsAny<object>(), It.IsAny<object>(), It.IsAny<int?>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var service = CreateService();

            // Act
            await service.DeleteAsync(expense.Id);

            // Assert
            _expenseRepoMock.Verify(r => r.DeleteAsync(expense.Id, It.IsAny<CancellationToken>()), Times.Once);
            _auditServiceMock.Verify(a => a.LogAsync(nameof(Expense), expense.Id, "Delete", expense, null, It.IsAny<int?>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetByIdAsync_ThrowsException_WhenRepositoryThrows()
        {
            // Arrange
            var ex = new InvalidOperationException("DB error");
            _expenseRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                            .ThrowsAsync(ex);

            var service = CreateService();

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => service.GetByIdAsync(1));
        }
    }
}
