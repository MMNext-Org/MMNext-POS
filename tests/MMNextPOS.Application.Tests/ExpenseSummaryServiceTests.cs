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
    public class ExpenseSummaryServiceTests
    {
        private readonly Mock<IExpenseRepository> _expenseRepoMock = new();
        private readonly Mock<IExpenseTypeRepository> _expenseTypeRepoMock = new();

        private IExpenseSummaryService CreateService()
        {
            return new ExpenseSummaryService(_expenseRepoMock.Object, _expenseTypeRepoMock.Object);
        }

        [Fact]
        public async Task GetMonthlySummaryAsync_ExistingExpenses_ReturnsSummaryWithCategories()
        {
            // Arrange
            var year = 2026;
            var month = 1;
            var startDate = new DateTime(year, month, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1).AddHours(23).AddMinutes(59).AddSeconds(59);

            var expenses = new List<Expense>
            {
                new() { Id = 1, ExpenseNo = "EXP-001", ExpenseTypeId = 1, ExpenseDate = new DateTime(2026, 1, 15), Amount = 100m, Status = "Active" },
                new() { Id = 2, ExpenseNo = "EXP-002", ExpenseTypeId = 1, ExpenseDate = new DateTime(2026, 1, 20), Amount = 200m, Status = "Active" },
                new() { Id = 3, ExpenseNo = "EXP-003", ExpenseTypeId = 2, ExpenseDate = new DateTime(2026, 1, 25), Amount = 50m, Status = "Active" },
                new() { Id = 4, ExpenseNo = "EXP-004", ExpenseTypeId = 1, ExpenseDate = new DateTime(2026, 2, 1), Amount = 300m, Status = "Active" },
                new() { Id = 5, ExpenseNo = "EXP-005", ExpenseTypeId = 3, ExpenseDate = new DateTime(2026, 1, 10), Amount = 75m, Status = "Cancelled" }
            };
            var expenseTypes = new List<ExpenseType>
            {
                new() { Id = 1, Code = "RENT", Name = "Rent", IsActive = true },
                new() { Id = 2, Code = "UTIL", Name = "Utilities", IsActive = true },
                new() { Id = 3, Code = "SUP", Name = "Supplies", IsActive = true }
            };

            _expenseRepoMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                            .ReturnsAsync(expenses);
            _expenseTypeRepoMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                                .ReturnsAsync(expenseTypes);

            var service = CreateService();

            // Act
            var result = await service.GetMonthlySummaryAsync(year, month);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(year, result.Year);
            Assert.Equal(month, result.Month);
            Assert.Equal(350m, result.TotalAmount);
            Assert.Equal(3, result.TransactionCount);
            Assert.Equal(2, result.Categories.Count);
            var rentCat = result.Categories.FirstOrDefault(c => c.ExpenseTypeId == 1);
            Assert.NotNull(rentCat);
            Assert.Equal("RENT", rentCat.ExpenseTypeCode);
            Assert.Equal("Rent", rentCat.ExpenseTypeName);
            Assert.Equal(300m, rentCat.Amount);
            Assert.Equal(2, rentCat.Count);
        }

        [Fact]
        public async Task GetMonthlySummaryAsync_NoExpenses_ReturnsEmptySummary()
        {
            // Arrange
            var year = 2026;
            var month = 1;

            _expenseRepoMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                            .ReturnsAsync(new List<Expense>());
            _expenseTypeRepoMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                                .ReturnsAsync(new List<ExpenseType>());

            var service = CreateService();

            // Act
            var result = await service.GetMonthlySummaryAsync(year, month);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(0m, result.TotalAmount);
            Assert.Equal(0, result.TransactionCount);
            Assert.Empty(result.Categories);
        }

        [Fact]
        public async Task GetMonthlySummaryAsync_CancelledExpenses_Excluded()
        {
            // Arrange
            var year = 2026;
            var month = 1;

            var expenses = new List<Expense>
            {
                new() { Id = 1, ExpenseNo = "EXP-001", ExpenseTypeId = 1, ExpenseDate = new DateTime(2026, 1, 15), Amount = 100m, Status = "Cancelled" },
                new() { Id = 2, ExpenseNo = "EXP-002", ExpenseTypeId = 1, ExpenseDate = new DateTime(2026, 1, 20), Amount = 200m, Status = "Active" }
            };
            var expenseTypes = new List<ExpenseType>
            {
                new() { Id = 1, Code = "RENT", Name = "Rent", IsActive = true }
            };

            _expenseRepoMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                            .ReturnsAsync(expenses);
            _expenseTypeRepoMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                                .ReturnsAsync(expenseTypes);

            var service = CreateService();

            // Act
            var result = await service.GetMonthlySummaryAsync(year, month);

            // Assert
            Assert.Equal(200m, result.TotalAmount);
            Assert.Equal(1, result.TransactionCount);
            Assert.Single(result.Categories);
            Assert.Equal(200m, result.Categories[0].Amount);
        }

        [Fact]
        public async Task GetYearlySummaryAsync_Returns12MonthlySummaries()
        {
            // Arrange
            var year = 2026;

            _expenseRepoMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                            .ReturnsAsync(new List<Expense>());
            _expenseTypeRepoMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                                .ReturnsAsync(new List<ExpenseType>());

            var service = CreateService();

            // Act
            var result = await service.GetYearlySummaryAsync(year);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(12, result.Count);
            Assert.All(result, s => Assert.Equal(year, s.Year));
        }

        [Fact]
        public async Task GetCategoryBreakdownAsync_ReturnsCategoriesFromMonthlySummary()
        {
            // Arrange
            var year = 2026;
            var month = 1;

            var expenses = new List<Expense>
            {
                new() { Id = 1, ExpenseNo = "EXP-001", ExpenseTypeId = 1, ExpenseDate = new DateTime(2026, 1, 15), Amount = 100m, Status = "Active" }
            };
            var expenseTypes = new List<ExpenseType>
            {
                new() { Id = 1, Code = "RENT", Name = "Rent", IsActive = true }
            };

            _expenseRepoMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                            .ReturnsAsync(expenses);
            _expenseTypeRepoMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                                .ReturnsAsync(expenseTypes);

            var service = CreateService();

            // Act
            var result = await service.GetCategoryBreakdownAsync(year, month);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal(1, result[0].ExpenseTypeId);
            Assert.Equal("RENT", result[0].ExpenseTypeCode);
        }

        [Fact]
        public async Task GetTotalExpensesAsync_ExistingExpenses_ReturnsSum()
        {
            // Arrange
            var startDate = new DateTime(2026, 1, 1);
            var endDate = new DateTime(2026, 1, 31);

            var expenses = new List<Expense>
            {
                new() { Id = 1, ExpenseNo = "EXP-001", ExpenseDate = new DateTime(2026, 1, 10), Amount = 100m, Status = "Active" },
                new() { Id = 2, ExpenseNo = "EXP-002", ExpenseDate = new DateTime(2026, 1, 20), Amount = 200m, Status = "Active" },
                new() { Id = 3, ExpenseNo = "EXP-003", ExpenseDate = new DateTime(2026, 2, 1), Amount = 300m, Status = "Active" },
                new() { Id = 4, ExpenseNo = "EXP-004", ExpenseDate = new DateTime(2026, 1, 15), Amount = 50m, Status = "Cancelled" }
            };

            _expenseRepoMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                            .ReturnsAsync(expenses);

            var service = CreateService();

            // Act
            var result = await service.GetTotalExpensesAsync(startDate, endDate);

            // Assert
            Assert.Equal(300m, result);
        }

        [Fact]
        public async Task GetTotalExpensesAsync_NoExpensesInRange_ReturnsZero()
        {
            // Arrange
            var startDate = new DateTime(2026, 1, 1);
            var endDate = new DateTime(2026, 1, 31);

            _expenseRepoMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                            .ReturnsAsync(new List<Expense>());

            var service = CreateService();

            // Act
            var result = await service.GetTotalExpensesAsync(startDate, endDate);

            // Assert
            Assert.Equal(0m, result);
        }
    }
}
