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
    public class OutstandingServiceTests
    {
        private readonly Mock<ICustomerOutstandingRepository> _customerRepoMock = new();
        private readonly Mock<ISupplierOutstandingRepository> _supplierRepoMock = new();
        private readonly Mock<IAuditService> _auditServiceMock = new();

        private IOutstandingService CreateService()
        {
            return new OutstandingService(_customerRepoMock.Object, _supplierRepoMock.Object, _auditServiceMock.Object);
        }

        [Fact]
        public async Task GetCustomerOutstandingAsync_ExistingCustomer_ReturnsOutstandingList()
        {
            // Arrange
            var customerId = 1;
            var outstandings = new List<CustomerOutstanding>
            {
                new() { Id = 1, CustomerId = customerId, SaleId = 10, TransactionDate = DateTime.Today, DebitAmount = 500m, CreditAmount = 0m, Balance = 500m, Status = "Open" },
                new() { Id = 2, CustomerId = customerId, SaleId = 11, TransactionDate = DateTime.Today, DebitAmount = 300m, CreditAmount = 200m, Balance = 600m, Status = "Open" }
            };
            _customerRepoMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                             .ReturnsAsync(outstandings);

            var service = CreateService();

            // Act
            var result = await service.GetCustomerOutstandingAsync(customerId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.All(result, o => Assert.Equal(customerId, o.CustomerId));
        }

        [Fact]
        public async Task GetCustomerOutstandingAsync_NoOutstanding_ReturnsEmptyList()
        {
            // Arrange
            var customerId = 1;
            _customerRepoMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                             .ReturnsAsync(new List<CustomerOutstanding>());

            var service = CreateService();

            // Act
            var result = await service.GetCustomerOutstandingAsync(customerId);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task AddCustomerOutstandingAsync_ValidOutstanding_ReturnsAddedOutstanding()
        {
            // Arrange
            var outstanding = new CustomerOutstanding
            {
                CustomerId = 1,
                SaleId = 10,
                TransactionDate = DateTime.Today,
                DebitAmount = 1000m,
                CreditAmount = 0m,
                Balance = 1000m,
                Status = "Open"
            };
            var addedOutstanding = new CustomerOutstanding
            {
                Id = 3,
                CustomerId = 1,
                SaleId = 10,
                TransactionDate = DateTime.Today,
                DebitAmount = 1000m,
                CreditAmount = 0m,
                Balance = 1000m,
                Status = "Open"
            };
            _customerRepoMock.Setup(r => r.AddAsync(outstanding, It.IsAny<CancellationToken>()))
                             .ReturnsAsync(addedOutstanding);
            _auditServiceMock.Setup(a => a.LogAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(),
                It.IsAny<object>(), It.IsAny<object>(), It.IsAny<int?>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var service = CreateService();

            // Act
            var result = await service.AddCustomerOutstandingAsync(outstanding);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(addedOutstanding.Id, result.Id);
            Assert.Equal(outstanding.CustomerId, result.CustomerId);
            Assert.Equal(outstanding.DebitAmount, result.DebitAmount);
            _customerRepoMock.Verify(r => r.AddAsync(outstanding, It.IsAny<CancellationToken>()), Times.Once);
            _auditServiceMock.Verify(a => a.LogAsync(nameof(CustomerOutstanding), addedOutstanding.Id, "Create", null, addedOutstanding, It.IsAny<int?>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task UpdateCustomerOutstandingAsync_ExistingOutstanding_UpdatesOutstanding()
        {
            // Arrange
            var existingOutstanding = new CustomerOutstanding { Id = 1, CustomerId = 1, Balance = 500m, Status = "Open" };
            var updatedOutstanding = new CustomerOutstanding { Id = 1, CustomerId = 1, Balance = 200m, Status = "Closed" };
            _customerRepoMock.Setup(r => r.GetByIdAsync(existingOutstanding.Id, It.IsAny<CancellationToken>()))
                             .ReturnsAsync(existingOutstanding);
            _customerRepoMock.Setup(r => r.UpdateAsync(existingOutstanding, It.IsAny<CancellationToken>()))
                             .Returns(Task.CompletedTask);
            _auditServiceMock.Setup(a => a.LogAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(),
                It.IsAny<object>(), It.IsAny<object>(), It.IsAny<int?>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var service = CreateService();

            // Act
            await service.UpdateCustomerOutstandingAsync(updatedOutstanding);

            // Assert
            _customerRepoMock.Verify(r => r.UpdateAsync(It.Is<CustomerOutstanding>(o => o.Balance == 200m && o.Status == "Closed"), It.IsAny<CancellationToken>()), Times.Once);
            _auditServiceMock.Verify(a => a.LogAsync(nameof(CustomerOutstanding), updatedOutstanding.Id, "Update", existingOutstanding, updatedOutstanding, It.IsAny<int?>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task DeleteCustomerOutstandingAsync_ExistingOutstanding_DeletesOutstanding()
        {
            // Arrange
            var outstanding = new CustomerOutstanding { Id = 1, CustomerId = 1, Balance = 500m };
            _customerRepoMock.Setup(r => r.GetByIdAsync(outstanding.Id, It.IsAny<CancellationToken>()))
                             .ReturnsAsync(outstanding);
            _customerRepoMock.Setup(r => r.DeleteAsync(outstanding.Id, It.IsAny<CancellationToken>()))
                             .Returns(Task.CompletedTask);
            _auditServiceMock.Setup(a => a.LogAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(),
                It.IsAny<object>(), It.IsAny<object>(), It.IsAny<int?>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var service = CreateService();

            // Act
            await service.DeleteCustomerOutstandingAsync(outstanding.Id);

            // Assert
            _customerRepoMock.Verify(r => r.DeleteAsync(outstanding.Id, It.IsAny<CancellationToken>()), Times.Once);
            _auditServiceMock.Verify(a => a.LogAsync(nameof(CustomerOutstanding), outstanding.Id, "Delete", outstanding, null, It.IsAny<int?>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetSupplierOutstandingAsync_ExistingSupplier_ReturnsOutstandingList()
        {
            // Arrange
            var supplierId = 1;
            var outstandings = new List<SupplierOutstanding>
            {
                new() { Id = 1, SupplierId = supplierId, PurchaseId = 20, TransactionDate = DateTime.Today, DebitAmount = 0m, CreditAmount = 1000m, Balance = 1000m, Status = "Open" },
                new() { Id = 2, SupplierId = supplierId, PurchaseId = 21, TransactionDate = DateTime.Today, DebitAmount = 500m, CreditAmount = 0m, Balance = 500m, Status = "Open" }
            };
            _supplierRepoMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                             .ReturnsAsync(outstandings);

            var service = CreateService();

            // Act
            var result = await service.GetSupplierOutstandingAsync(supplierId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.All(result, o => Assert.Equal(supplierId, o.SupplierId));
        }

        [Fact]
        public async Task GetSupplierOutstandingAsync_NoOutstanding_ReturnsEmptyList()
        {
            // Arrange
            var supplierId = 1;
            _supplierRepoMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                             .ReturnsAsync(new List<SupplierOutstanding>());

            var service = CreateService();

            // Act
            var result = await service.GetSupplierOutstandingAsync(supplierId);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task AddSupplierOutstandingAsync_ValidOutstanding_ReturnsAddedOutstanding()
        {
            // Arrange
            var outstanding = new SupplierOutstanding
            {
                SupplierId = 1,
                PurchaseId = 20,
                TransactionDate = DateTime.Today,
                DebitAmount = 0m,
                CreditAmount = 2000m,
                Balance = 2000m,
                Status = "Open"
            };
            var addedOutstanding = new SupplierOutstanding
            {
                Id = 3,
                SupplierId = 1,
                PurchaseId = 20,
                TransactionDate = DateTime.Today,
                DebitAmount = 0m,
                CreditAmount = 2000m,
                Balance = 2000m,
                Status = "Open"
            };
            _supplierRepoMock.Setup(r => r.AddAsync(outstanding, It.IsAny<CancellationToken>()))
                             .ReturnsAsync(addedOutstanding);
            _auditServiceMock.Setup(a => a.LogAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(),
                It.IsAny<object>(), It.IsAny<object>(), It.IsAny<int?>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var service = CreateService();

            // Act
            var result = await service.AddSupplierOutstandingAsync(outstanding);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(addedOutstanding.Id, result.Id);
            Assert.Equal(outstanding.SupplierId, result.SupplierId);
            Assert.Equal(outstanding.CreditAmount, result.CreditAmount);
            _supplierRepoMock.Verify(r => r.AddAsync(outstanding, It.IsAny<CancellationToken>()), Times.Once);
            _auditServiceMock.Verify(a => a.LogAsync(nameof(SupplierOutstanding), addedOutstanding.Id, "Create", null, addedOutstanding, It.IsAny<int?>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task UpdateSupplierOutstandingAsync_ExistingOutstanding_UpdatesOutstanding()
        {
            // Arrange
            var existingOutstanding = new SupplierOutstanding { Id = 1, SupplierId = 1, Balance = 1000m, Status = "Open" };
            var updatedOutstanding = new SupplierOutstanding { Id = 1, SupplierId = 1, Balance = 500m, Status = "Closed" };
            _supplierRepoMock.Setup(r => r.GetByIdAsync(existingOutstanding.Id, It.IsAny<CancellationToken>()))
                             .ReturnsAsync(existingOutstanding);
            _supplierRepoMock.Setup(r => r.UpdateAsync(existingOutstanding, It.IsAny<CancellationToken>()))
                             .Returns(Task.CompletedTask);
            _auditServiceMock.Setup(a => a.LogAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(),
                It.IsAny<object>(), It.IsAny<object>(), It.IsAny<int?>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var service = CreateService();

            // Act
            await service.UpdateSupplierOutstandingAsync(updatedOutstanding);

            // Assert
            _supplierRepoMock.Verify(r => r.UpdateAsync(It.Is<SupplierOutstanding>(o => o.Balance == 500m && o.Status == "Closed"), It.IsAny<CancellationToken>()), Times.Once);
            _auditServiceMock.Verify(a => a.LogAsync(nameof(SupplierOutstanding), updatedOutstanding.Id, "Update", existingOutstanding, updatedOutstanding, It.IsAny<int?>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task DeleteSupplierOutstandingAsync_ExistingOutstanding_DeletesOutstanding()
        {
            // Arrange
            var outstanding = new SupplierOutstanding { Id = 1, SupplierId = 1, Balance = 1000m };
            _supplierRepoMock.Setup(r => r.GetByIdAsync(outstanding.Id, It.IsAny<CancellationToken>()))
                             .ReturnsAsync(outstanding);
            _supplierRepoMock.Setup(r => r.DeleteAsync(outstanding.Id, It.IsAny<CancellationToken>()))
                             .Returns(Task.CompletedTask);
            _auditServiceMock.Setup(a => a.LogAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(),
                It.IsAny<object>(), It.IsAny<object>(), It.IsAny<int?>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var service = CreateService();

            // Act
            await service.DeleteSupplierOutstandingAsync(outstanding.Id);

            // Assert
            _supplierRepoMock.Verify(r => r.DeleteAsync(outstanding.Id, It.IsAny<CancellationToken>()), Times.Once);
            _auditServiceMock.Verify(a => a.LogAsync(nameof(SupplierOutstanding), outstanding.Id, "Delete", outstanding, null, It.IsAny<int?>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetCustomerOutstandingAsync_ThrowsException_WhenRepositoryThrows()
        {
            // Arrange
            var ex = new InvalidOperationException("DB error");
            _customerRepoMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                             .ThrowsAsync(ex);

            var service = CreateService();

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => service.GetCustomerOutstandingAsync(1));
        }

        [Fact]
        public async Task GetSupplierOutstandingAsync_ThrowsException_WhenRepositoryThrows()
        {
            // Arrange
            var ex = new InvalidOperationException("DB error");
            _supplierRepoMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                             .ThrowsAsync(ex);

            var service = CreateService();

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => service.GetSupplierOutstandingAsync(1));
        }

        // ───────────────────────── ApplyCustomerPaymentAsync Tests ─────────────────────────

        [Fact]
        public async Task ApplyCustomerPaymentAsync_PartialPayment_ReducesBalance()
        {
            // Arrange
            var customerId = 1;
            var outstandings = new List<CustomerOutstanding>
            {
                new() { Id = 1, CustomerId = customerId, SaleId = 10, TransactionDate = DateTime.Today.AddDays(-2), DebitAmount = 500m, CreditAmount = 0m, Balance = 500m, Status = "Open" },
                new() { Id = 2, CustomerId = customerId, SaleId = 11, TransactionDate = DateTime.Today.AddDays(-1), DebitAmount = 500m, CreditAmount = 0m, Balance = 500m, Status = "Open" }
            };
            _customerRepoMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                             .ReturnsAsync(outstandings);
            _customerRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                             .ReturnsAsync(outstandings[0]);
            _customerRepoMock.Setup(r => r.GetByIdAsync(2, It.IsAny<CancellationToken>()))
                             .ReturnsAsync(outstandings[1]);
            _customerRepoMock.Setup(r => r.UpdateAsync(It.IsAny<CustomerOutstanding>(), It.IsAny<CancellationToken>()))
                             .Returns(Task.CompletedTask);
            _auditServiceMock.Setup(a => a.LogAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(),
                It.IsAny<object?>(), It.IsAny<object?>(), It.IsAny<int?>(), It.IsAny<string?>(),
                It.IsAny<string?>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var service = CreateService();

            // Act - Apply payment of 700 (covers first 500 + 200 of second)
            var result = await service.ApplyCustomerPaymentAsync(customerId, 700m, saleId: 10, paymentId: 1, "Test payment");

            // Assert - First outstanding should be cleared (Balance = 0, Status = "Cleared")
            _customerRepoMock.Verify(r => r.UpdateAsync(It.Is<CustomerOutstanding>(o => o.Id == 1 && o.Balance == 0 && o.Status == "Cleared"), It.IsAny<CancellationToken>()), Times.Once);
            // Second outstanding should have Balance = 300 (500 - 200)
            _customerRepoMock.Verify(r => r.UpdateAsync(It.Is<CustomerOutstanding>(o => o.Id == 2 && o.Balance == 300 && o.Status == "Open"), It.IsAny<CancellationToken>()), Times.Once);
            // Audit should be called for each update
            _auditServiceMock.Verify(a => a.LogAsync(nameof(CustomerOutstanding), It.IsAny<int>(), "Update", It.IsAny<object>(), It.IsAny<object>(), It.IsAny<int?>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
        }

        [Fact]
        public async Task ApplyCustomerPaymentAsync_FullPayment_ClearsAllOutstanding()
        {
            // Arrange
            var customerId = 1;
            var outstandings = new List<CustomerOutstanding>
            {
                new() { Id = 1, CustomerId = customerId, SaleId = 10, TransactionDate = DateTime.Today, DebitAmount = 1000m, CreditAmount = 0m, Balance = 1000m, Status = "Open" }
            };
            _customerRepoMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                             .ReturnsAsync(outstandings);
            _customerRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                             .ReturnsAsync(outstandings[0]);
            _customerRepoMock.Setup(r => r.UpdateAsync(It.IsAny<CustomerOutstanding>(), It.IsAny<CancellationToken>()))
                             .Returns(Task.CompletedTask);
            _auditServiceMock.Setup(a => a.LogAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(),
                It.IsAny<object?>(), It.IsAny<object?>(), It.IsAny<int?>(), It.IsAny<string?>(),
                It.IsAny<string?>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var service = CreateService();

            // Act - Apply payment equal to balance
            var result = await service.ApplyCustomerPaymentAsync(customerId, 1000m, saleId: 10, paymentId: 1, "Full payment");

            // Assert - Outstanding should be cleared
            _customerRepoMock.Verify(r => r.UpdateAsync(It.Is<CustomerOutstanding>(o => o.Id == 1 && o.Balance == 0 && o.Status == "Cleared"), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task ApplyCustomerPaymentAsync_Overpayment_CreatesCreditOutstanding()
        {
            // Arrange
            var customerId = 1;
            var outstandings = new List<CustomerOutstanding>
            {
                new() { Id = 1, CustomerId = customerId, SaleId = 10, TransactionDate = DateTime.Today, DebitAmount = 1000m, CreditAmount = 0m, Balance = 1000m, Status = "Open" }
            };
            _customerRepoMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                             .ReturnsAsync(outstandings);
            _customerRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                             .ReturnsAsync(outstandings[0]);
            _customerRepoMock.Setup(r => r.UpdateAsync(It.IsAny<CustomerOutstanding>(), It.IsAny<CancellationToken>()))
                             .Returns(Task.CompletedTask);
            _customerRepoMock.Setup(r => r.AddAsync(It.IsAny<CustomerOutstanding>(), It.IsAny<CancellationToken>()))
                             .ReturnsAsync((CustomerOutstanding o, CancellationToken _) => { o.Id = 2; return o; });
            _auditServiceMock.Setup(a => a.LogAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(),
                It.IsAny<object?>(), It.IsAny<object?>(), It.IsAny<int?>(), It.IsAny<string?>(),
                It.IsAny<string?>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var service = CreateService();

            // Act - Apply payment of 1500 (overpayment of 500)
            var result = await service.ApplyCustomerPaymentAsync(customerId, 1500m, saleId: 10, paymentId: 1, "Overpayment");

            // Assert - Original outstanding cleared
            _customerRepoMock.Verify(r => r.UpdateAsync(It.Is<CustomerOutstanding>(o => o.Id == 1 && o.Balance == 0 && o.Status == "Cleared"), It.IsAny<CancellationToken>()), Times.Once);
            // Credit outstanding created with negative balance
            _customerRepoMock.Verify(r => r.AddAsync(It.Is<CustomerOutstanding>(o => o.Balance == -500m && o.Status == "Credit"), It.IsAny<CancellationToken>()), Times.Once);
            // Result should be the credit outstanding
            Assert.Equal(-500m, result.Balance);
            Assert.Equal("Credit", result.Status);
        }

        [Fact]
        public async Task ApplyCustomerPaymentAsync_ZeroAmount_ThrowsArgumentException()
        {
            var service = CreateService();

            await Assert.ThrowsAsync<ArgumentException>(() => service.ApplyCustomerPaymentAsync(1, 0m, null, null, null));
        }

        [Fact]
        public async Task ApplyCustomerPaymentAsync_NegativeAmount_ThrowsArgumentException()
        {
            var service = CreateService();

            await Assert.ThrowsAsync<ArgumentException>(() => service.ApplyCustomerPaymentAsync(1, -100m, null, null, null));
        }

        [Fact]
        public async Task ApplyCustomerPaymentAsync_NoOpenOutstanding_CreatesCreditOutstanding()
        {
            // Arrange - no open outstandings
            _customerRepoMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                             .ReturnsAsync(new List<CustomerOutstanding>());
            _customerRepoMock.Setup(r => r.AddAsync(It.IsAny<CustomerOutstanding>(), It.IsAny<CancellationToken>()))
                             .ReturnsAsync((CustomerOutstanding o, CancellationToken _) => { o.Id = 1; return o; });
            _auditServiceMock.Setup(a => a.LogAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(),
                It.IsAny<object?>(), It.IsAny<object?>(), It.IsAny<int?>(), It.IsAny<string?>(),
                It.IsAny<string?>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var service = CreateService();

            // Act - Apply payment with no existing outstanding
            var result = await service.ApplyCustomerPaymentAsync(1, 500m, saleId: 10, paymentId: 1, "Advance payment");

            // Assert - Credit outstanding created
            _customerRepoMock.Verify(r => r.AddAsync(It.Is<CustomerOutstanding>(o => o.Balance == -500m && o.Status == "Credit"), It.IsAny<CancellationToken>()), Times.Once);
            Assert.Equal(-500m, result.Balance);
            Assert.Equal("Credit", result.Status);
        }

        // ───────────────────────── ClearCustomerAccountAsync Tests ─────────────────────────

        [Fact]
        public async Task ClearCustomerAccountAsync_OpenOutstanding_ClearsAll()
        {
            // Arrange
            var customerId = 1;
            var outstandings = new List<CustomerOutstanding>
            {
                new() { Id = 1, CustomerId = customerId, SaleId = 10, TransactionDate = DateTime.Today, DebitAmount = 1000m, CreditAmount = 0m, Balance = 1000m, Status = "Open" },
                new() { Id = 2, CustomerId = customerId, SaleId = 11, TransactionDate = DateTime.Today, DebitAmount = 500m, CreditAmount = 0m, Balance = 500m, Status = "Open" }
            };
            _customerRepoMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                             .ReturnsAsync(outstandings);
            _customerRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                             .ReturnsAsync(outstandings[0]);
            _customerRepoMock.Setup(r => r.GetByIdAsync(2, It.IsAny<CancellationToken>()))
                             .ReturnsAsync(outstandings[1]);
            _customerRepoMock.Setup(r => r.UpdateAsync(It.IsAny<CustomerOutstanding>(), It.IsAny<CancellationToken>()))
                             .Returns(Task.CompletedTask);
            _auditServiceMock.Setup(a => a.LogAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(),
                It.IsAny<object?>(), It.IsAny<object?>(), It.IsAny<int?>(), It.IsAny<string?>(),
                It.IsAny<string?>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var service = CreateService();

            // Act
            await service.ClearCustomerAccountAsync(customerId, clearedByUserId: 1, "Account settlement");

            // Assert - Both outstandings cleared
            _customerRepoMock.Verify(r => r.UpdateAsync(It.Is<CustomerOutstanding>(o => o.Id == 1 && o.Balance == 0 && o.Status == "Cleared"), It.IsAny<CancellationToken>()), Times.Once);
            _customerRepoMock.Verify(r => r.UpdateAsync(It.Is<CustomerOutstanding>(o => o.Id == 2 && o.Balance == 0 && o.Status == "Cleared"), It.IsAny<CancellationToken>()), Times.Once);
            // Audit log for clearance
            _auditServiceMock.Verify(a => a.LogAsync(nameof(CustomerOutstanding), 0, "ClearAccount", null, It.IsAny<object>(), It.IsAny<int?>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task ClearCustomerAccountAsync_NoOpenOutstanding_ThrowsInvalidOperationException()
        {
            // Arrange - no open outstandings
            _customerRepoMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                             .ReturnsAsync(new List<CustomerOutstanding>());

            var service = CreateService();

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => service.ClearCustomerAccountAsync(1, 1, "No balance"));
        }

        [Fact]
        public async Task ClearCustomerAccountAsync_SkipsNonOpenOutstanding()
        {
            // Arrange - mix of open and cleared
            var customerId = 1;
            var outstandings = new List<CustomerOutstanding>
            {
                new() { Id = 1, CustomerId = customerId, SaleId = 10, TransactionDate = DateTime.Today, DebitAmount = 1000m, CreditAmount = 0m, Balance = 1000m, Status = "Open" },
                new() { Id = 2, CustomerId = customerId, SaleId = 11, TransactionDate = DateTime.Today, DebitAmount = 500m, CreditAmount = 500m, Balance = 0m, Status = "Cleared" }, // Already cleared
                new() { Id = 3, CustomerId = customerId, SaleId = 12, TransactionDate = DateTime.Today, DebitAmount = 200m, CreditAmount = 0m, Balance = -200m, Status = "Credit" } // Credit
            };
            _customerRepoMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                             .ReturnsAsync(outstandings);
            _customerRepoMock.Setup(r => r.UpdateAsync(It.IsAny<CustomerOutstanding>(), It.IsAny<CancellationToken>()))
                             .Returns(Task.CompletedTask);
            _auditServiceMock.Setup(a => a.LogAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(),
                It.IsAny<object?>(), It.IsAny<object?>(), It.IsAny<int?>(), It.IsAny<string?>(),
                It.IsAny<string?>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var service = CreateService();

            // Act
            await service.ClearCustomerAccountAsync(customerId, 1, "Clear open only");

            // Assert - Only the open one is updated
            _customerRepoMock.Verify(r => r.UpdateAsync(It.Is<CustomerOutstanding>(o => o.Id == 1 && o.Balance == 0 && o.Status == "Cleared"), It.IsAny<CancellationToken>()), Times.Once);
            // The cleared and credit ones should NOT be updated
            _customerRepoMock.Verify(r => r.UpdateAsync(It.Is<CustomerOutstanding>(o => o.Id == 2), It.IsAny<CancellationToken>()), Times.Never);
            _customerRepoMock.Verify(r => r.UpdateAsync(It.Is<CustomerOutstanding>(o => o.Id == 3), It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}
