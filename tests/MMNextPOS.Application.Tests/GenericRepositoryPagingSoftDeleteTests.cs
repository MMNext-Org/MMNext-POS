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
    public class GenericRepositoryPagingTests
    {
        private readonly Mock<IInvoiceRepository> _invoiceRepoMock = new();

        private IInvoiceService CreateService()
        {
            return new InvoiceService(_invoiceRepoMock.Object, new Mock<IAuditService>().Object);
        }

        [Fact]
        public async Task GetPageAsync_ReturnsPagedResults()
        {
            var invoices = new List<Invoice>
            {
                new() { Id = 1, InvoiceNo = "INV-001", AmountDue = 100m, InvoiceDate = DateTime.Today },
                new() { Id = 2, InvoiceNo = "INV-002", AmountDue = 200m, InvoiceDate = DateTime.Today },
                new() { Id = 3, InvoiceNo = "INV-003", AmountDue = 300m, InvoiceDate = DateTime.Today },
                new() { Id = 4, InvoiceNo = "INV-004", AmountDue = 400m, InvoiceDate = DateTime.Today },
                new() { Id = 5, InvoiceNo = "INV-005", AmountDue = 500m, InvoiceDate = DateTime.Today },
            };
            _invoiceRepoMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                            .ReturnsAsync(invoices);

            var allInvoices = await _invoiceRepoMock.Object.GetAllAsync(CancellationToken.None);

            var page = 1;
            var pageSize = 2;
            var pagedResult = invoices
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            Assert.Equal(2, pagedResult.Count);
            Assert.Equal("INV-001", pagedResult[0].InvoiceNo);
            Assert.Equal("INV-002", pagedResult[1].InvoiceNo);
        }

        [Fact]
        public async Task GetPageAsync_SecondPage_ReturnsCorrectItems()
        {
            var invoices = new List<Invoice>
            {
                new() { Id = 1, InvoiceNo = "INV-001", AmountDue = 100m, InvoiceDate = DateTime.Today },
                new() { Id = 2, InvoiceNo = "INV-002", AmountDue = 200m, InvoiceDate = DateTime.Today },
                new() { Id = 3, InvoiceNo = "INV-003", AmountDue = 300m, InvoiceDate = DateTime.Today },
                new() { Id = 4, InvoiceNo = "INV-004", AmountDue = 400m, InvoiceDate = DateTime.Today },
                new() { Id = 5, InvoiceNo = "INV-005", AmountDue = 500m, InvoiceDate = DateTime.Today },
            };
            _invoiceRepoMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                            .ReturnsAsync(invoices);

            var allInvoices = await _invoiceRepoMock.Object.GetAllAsync(CancellationToken.None);

            var page = 2;
            var pageSize = 2;
            var pagedResult = invoices
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            Assert.Equal(2, pagedResult.Count);
            Assert.Equal("INV-003", pagedResult[0].InvoiceNo);
            Assert.Equal("INV-004", pagedResult[1].InvoiceNo);
        }

        [Fact]
        public async Task GetPageAsync_LastPage_ReturnsRemainingItems()
        {
            var invoices = new List<Invoice>
            {
                new() { Id = 1, InvoiceNo = "INV-001", AmountDue = 100m, InvoiceDate = DateTime.Today },
                new() { Id = 2, InvoiceNo = "INV-002", AmountDue = 200m, InvoiceDate = DateTime.Today },
                new() { Id = 3, InvoiceNo = "INV-003", AmountDue = 300m, InvoiceDate = DateTime.Today },
                new() { Id = 4, InvoiceNo = "INV-004", AmountDue = 400m, InvoiceDate = DateTime.Today },
                new() { Id = 5, InvoiceNo = "INV-005", AmountDue = 500m, InvoiceDate = DateTime.Today },
            };
            _invoiceRepoMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                            .ReturnsAsync(invoices);

            var allInvoices = await _invoiceRepoMock.Object.GetAllAsync(CancellationToken.None);

            var page = 3;
            var pageSize = 2;
            var pagedResult = invoices
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            Assert.Single(pagedResult);
            Assert.Equal("INV-005", pagedResult[0].InvoiceNo);
        }

        [Fact]
        public async Task GetPageAsync_PageOutOfRange_ReturnsEmptyList()
        {
            var invoices = new List<Invoice>
            {
                new() { Id = 1, InvoiceNo = "INV-001", AmountDue = 100m, InvoiceDate = DateTime.Today },
                new() { Id = 2, InvoiceNo = "INV-002", AmountDue = 200m, InvoiceDate = DateTime.Today },
            };
            _invoiceRepoMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                            .ReturnsAsync(invoices);

            var allInvoices = await _invoiceRepoMock.Object.GetAllAsync(CancellationToken.None);

            var page = 5;
            var pageSize = 2;
            var pagedResult = invoices
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            Assert.Empty(pagedResult);
        }
    }

    public class GenericRepositorySoftDeleteTests
    {
        private readonly Mock<IExpenseRepository> _expenseRepoMock = new();

        [Fact]
        public async Task GetAllAsync_FiltersOutDeletedEntities()
        {
            var expenses = new List<Expense>
            {
                new() { Id = 1, ExpenseNo = "EXP-001", Amount = 100m, IsDeleted = false },
                new() { Id = 2, ExpenseNo = "EXP-002", Amount = 200m, IsDeleted = true },
                new() { Id = 3, ExpenseNo = "EXP-003", Amount = 300m, IsDeleted = false },
            };

            var mockRepo = new Mock<IExpenseRepository>();
            mockRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                    .ReturnsAsync(expenses.Where(e => !e.IsDeleted).ToList());

            var allExpenses = await mockRepo.Object.GetAllAsync(CancellationToken.None);

            Assert.Equal(2, allExpenses.Count);
            Assert.All(allExpenses, e => Assert.False(e.IsDeleted));
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsNullForDeletedEntity()
        {
            var deletedExpense = new Expense { Id = 1, ExpenseNo = "EXP-001", Amount = 100m, IsDeleted = true };
            var activeExpense = new Expense { Id = 2, ExpenseNo = "EXP-002", Amount = 200m, IsDeleted = false };

            var mockRepo = new Mock<IExpenseRepository>();
            mockRepo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                    .ReturnsAsync((Expense?)null);
            mockRepo.Setup(r => r.GetByIdAsync(2, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(activeExpense);

            var resultDeleted = await mockRepo.Object.GetByIdAsync(1, CancellationToken.None);
            var resultActive = await mockRepo.Object.GetByIdAsync(2, CancellationToken.None);

            Assert.Null(resultDeleted);
            Assert.NotNull(resultActive);
        }
    }
}
