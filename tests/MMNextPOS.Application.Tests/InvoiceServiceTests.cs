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
    public class InvoiceServiceTests
    {
        private readonly Mock<IInvoiceRepository> _invoiceRepoMock = new();
        private readonly Mock<IAuditService> _auditServiceMock = new();

        private IInvoiceService CreateService()
        {
            return new InvoiceService(_invoiceRepoMock.Object, _auditServiceMock.Object);
        }

        [Fact]
        public async Task GetByIdAsync_ExistingInvoice_ReturnsInvoice()
        {
            // Arrange
            var invoice = new Invoice
            {
                Id = 1,
                InvoiceNo = "INV-001",
                SaleId = 10,
                CustomerId = 5,
                InvoiceDate = DateTime.Today,
                AmountDue = 150.75m,
                Status = "Active"
            };
            _invoiceRepoMock.Setup(r => r.GetByIdAsync(invoice.Id, It.IsAny<CancellationToken>()))
                            .ReturnsAsync(invoice);

            var service = CreateService();

            // Act
            var result = await service.GetByIdAsync(invoice.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(invoice.Id, result.Id);
            Assert.Equal(invoice.InvoiceNo, result.InvoiceNo);
            Assert.Equal(invoice.AmountDue, result.AmountDue);
            Assert.Equal(invoice.Status, result.Status);
        }

        [Fact]
        public async Task GetByIdAsync_NonExistentInvoice_ReturnsNull()
        {
            // Arrange
            _invoiceRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                            .ReturnsAsync((Invoice?)null!);

            var service = CreateService();

            // Act
            var result = await service.GetByIdAsync(999);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetAllAsync_ReturnsAllInvoices()
        {
            // Arrange
            var invoices = new List<Invoice>
            {
                new() { Id = 1, InvoiceNo = "INV-001", AmountDue = 100m, InvoiceDate = DateTime.Today },
                new() { Id = 2, InvoiceNo = "INV-002", AmountDue = 200m, InvoiceDate = DateTime.Today },
                new() { Id = 3, InvoiceNo = "INV-003", AmountDue = 300m, InvoiceDate = DateTime.Today }
            };
            _invoiceRepoMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                            .ReturnsAsync(invoices);

            var service = CreateService();

            // Act
            var result = await service.GetAllAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.Count);
        }

        [Fact]
        public async Task AddAsync_ValidInvoice_ReturnsAddedInvoice()
        {
            // Arrange
            var invoice = new Invoice
            {
                InvoiceNo = "INV-004",
                SaleId = 1,
                CustomerId = 10,
                InvoiceDate = DateTime.Today,
                AmountDue = 150.75m,
                Status = "Active"
            };
            var addedInvoice = new Invoice
            {
                Id = 4,
                InvoiceNo = "INV-004",
                SaleId = 1,
                CustomerId = 10,
                InvoiceDate = DateTime.Today,
                AmountDue = 150.75m,
                Status = "Active"
            };
            _invoiceRepoMock.Setup(r => r.AddAsync(invoice, It.IsAny<CancellationToken>()))
                            .ReturnsAsync(addedInvoice);
            _auditServiceMock.Setup(a => a.LogAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(),
                It.IsAny<object>(), It.IsAny<object>(), It.IsAny<int?>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var service = CreateService();

            // Act
            var result = await service.AddAsync(invoice);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(addedInvoice.Id, result.Id);
            Assert.Equal(invoice.InvoiceNo, result.InvoiceNo);
            Assert.Equal(invoice.AmountDue, result.AmountDue);
            _invoiceRepoMock.Verify(r => r.AddAsync(invoice, It.IsAny<CancellationToken>()), Times.Once);
            _auditServiceMock.Verify(a => a.LogAsync(nameof(Invoice), addedInvoice.Id, "Create", null, addedInvoice, It.IsAny<int?>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_ExistingInvoice_UpdatesInvoice()
        {
            // Arrange
            var existingInvoice = new Invoice { Id = 1, InvoiceNo = "INV-001", AmountDue = 100m, Status = "Active" };
            var updatedInvoice = new Invoice { Id = 1, InvoiceNo = "INV-001", AmountDue = 150m, Status = "Paid" };
            _invoiceRepoMock.Setup(r => r.GetByIdAsync(existingInvoice.Id, It.IsAny<CancellationToken>()))
                            .ReturnsAsync(existingInvoice);
            _invoiceRepoMock.Setup(r => r.UpdateAsync(existingInvoice, It.IsAny<CancellationToken>()))
                            .Returns(Task.CompletedTask);
            _auditServiceMock.Setup(a => a.LogAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(),
                It.IsAny<object>(), It.IsAny<object>(), It.IsAny<int?>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var service = CreateService();

            // Act
            await service.UpdateAsync(updatedInvoice);

            // Assert
            _invoiceRepoMock.Verify(r => r.UpdateAsync(It.Is<Invoice>(i => i.AmountDue == 150m && i.Status == "Paid"), It.IsAny<CancellationToken>()), Times.Once);
            _auditServiceMock.Verify(a => a.LogAsync(nameof(Invoice), updatedInvoice.Id, "Update", existingInvoice, updatedInvoice, It.IsAny<int?>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_ExistingInvoice_DeletesInvoice()
        {
            // Arrange
            var invoice = new Invoice { Id = 1, InvoiceNo = "INV-001", AmountDue = 100m };
            _invoiceRepoMock.Setup(r => r.GetByIdAsync(invoice.Id, It.IsAny<CancellationToken>()))
                            .ReturnsAsync(invoice);
            _invoiceRepoMock.Setup(r => r.DeleteAsync(invoice.Id, It.IsAny<CancellationToken>()))
                            .Returns(Task.CompletedTask);
            _auditServiceMock.Setup(a => a.LogAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(),
                It.IsAny<object>(), It.IsAny<object>(), It.IsAny<int?>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var service = CreateService();

            // Act
            await service.DeleteAsync(invoice.Id);

            // Assert
            _invoiceRepoMock.Verify(r => r.DeleteAsync(invoice.Id, It.IsAny<CancellationToken>()), Times.Once);
            _auditServiceMock.Verify(a => a.LogAsync(nameof(Invoice), invoice.Id, "Delete", invoice, null, It.IsAny<int?>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetByIdAsync_ThrowsException_WhenRepositoryThrows()
        {
            // Arrange
            var ex = new InvalidOperationException("DB error");
            _invoiceRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                            .ThrowsAsync(ex);

            var service = CreateService();

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => service.GetByIdAsync(1));
        }
    }
}
