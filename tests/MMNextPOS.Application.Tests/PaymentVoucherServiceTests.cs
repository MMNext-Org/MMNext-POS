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
    public class PaymentVoucherServiceTests
    {
        private readonly Mock<IPaymentVoucherRepository> _repoMock = new();
        private readonly Mock<IAuditService> _auditServiceMock = new();

        private IPaymentVoucherService CreateService()
        {
            return new PaymentVoucherService(_repoMock.Object, _auditServiceMock.Object);
        }

        [Fact]
        public async Task GetByIdAsync_ExistingVoucher_ReturnsVoucher()
        {
            // Arrange
            var voucher = new PaymentVoucher { Id = 1, VoucherNo = "PV-001", Amount = 100m };
            _repoMock.Setup(r => r.GetByIdAsync(voucher.Id, It.IsAny<CancellationToken>()))
                     .ReturnsAsync(voucher);

            var service = CreateService();

            // Act
            var result = await service.GetByIdAsync(voucher.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(voucher.Id, result.Id);
            Assert.Equal(voucher.VoucherNo, result.VoucherNo);
        }

        [Fact]
        public async Task GetByIdAsync_NonExistentVoucher_ReturnsNull()
        {
            // Arrange
            _repoMock.Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync((PaymentVoucher?)null!);

            var service = CreateService();

            // Act
            var result = await service.GetByIdAsync(999);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetByVoucherNoAsync_ExistingVoucherNo_ReturnsVoucher()
        {
            // Arrange
            var voucher = new PaymentVoucher { Id = 1, VoucherNo = "PV-001", Amount = 100m };
            _repoMock.Setup(r => r.GetByVoucherNoAsync(voucher.VoucherNo, It.IsAny<CancellationToken>()))
                     .ReturnsAsync(voucher);

            var service = CreateService();

            // Act
            var result = await service.GetByVoucherNoAsync(voucher.VoucherNo);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(voucher.VoucherNo, result.VoucherNo);
        }

        [Fact]
        public async Task GetByDateRangeAsync_ReturnsVouchersInRange()
        {
            // Arrange
            var fromDate = new DateTime(2026, 1, 1);
            var toDate = new DateTime(2026, 1, 31);
            var vouchers = new List<PaymentVoucher>
            {
                new() { Id = 1, VoucherNo = "PV-001", VoucherDate = new DateTime(2026, 1, 10), Amount = 100m },
                new() { Id = 2, VoucherNo = "PV-002", VoucherDate = new DateTime(2026, 1, 20), Amount = 200m }
            };
            _repoMock.Setup(r => r.GetByDateRangeAsync(fromDate, toDate, It.IsAny<CancellationToken>()))
                     .ReturnsAsync(vouchers);

            var service = CreateService();

            // Act
            var result = await service.GetByDateRangeAsync(fromDate, toDate);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task GetByCustomerAsync_ReturnsVouchersForCustomer()
        {
            // Arrange
            var customerId = 5;
            var vouchers = new List<PaymentVoucher>
            {
                new() { Id = 1, VoucherNo = "PV-001", CustomerId = customerId, Amount = 100m }
            };
            _repoMock.Setup(r => r.GetByCustomerAsync(customerId, It.IsAny<CancellationToken>()))
                     .ReturnsAsync(vouchers);

            var service = CreateService();

            // Act
            var result = await service.GetByCustomerAsync(customerId);

            // Assert
            Assert.Single(result);
            Assert.Equal(customerId, result[0].CustomerId);
        }

        [Fact]
        public async Task GetBySupplierAsync_ReturnsVouchersForSupplier()
        {
            // Arrange
            var supplierId = 7;
            var vouchers = new List<PaymentVoucher>
            {
                new() { Id = 1, VoucherNo = "PV-001", SupplierId = supplierId, Amount = 200m }
            };
            _repoMock.Setup(r => r.GetBySupplierAsync(supplierId, It.IsAny<CancellationToken>()))
                     .ReturnsAsync(vouchers);

            var service = CreateService();

            // Act
            var result = await service.GetBySupplierAsync(supplierId);

            // Assert
            Assert.Single(result);
            Assert.Equal(supplierId, result[0].SupplierId);
        }

        [Fact]
        public async Task GetBySaleAsync_ReturnsVouchersForSale()
        {
            // Arrange
            var saleId = 10;
            var vouchers = new List<PaymentVoucher>
            {
                new() { Id = 1, VoucherNo = "PV-001", SaleId = saleId, Amount = 150m }
            };
            _repoMock.Setup(r => r.GetBySaleAsync(saleId, It.IsAny<CancellationToken>()))
                     .ReturnsAsync(vouchers);

            var service = CreateService();

            // Act
            var result = await service.GetBySaleAsync(saleId);

            // Assert
            Assert.Single(result);
            Assert.Equal(saleId, result[0].SaleId);
        }

        [Fact]
        public async Task GetByPurchaseAsync_ReturnsVouchersForPurchase()
        {
            // Arrange
            var purchaseId = 20;
            var vouchers = new List<PaymentVoucher>
            {
                new() { Id = 1, VoucherNo = "PV-001", PurchaseId = purchaseId, Amount = 300m }
            };
            _repoMock.Setup(r => r.GetByPurchaseAsync(purchaseId, It.IsAny<CancellationToken>()))
                     .ReturnsAsync(vouchers);

            var service = CreateService();

            // Act
            var result = await service.GetByPurchaseAsync(purchaseId);

            // Assert
            Assert.Single(result);
            Assert.Equal(purchaseId, result[0].PurchaseId);
        }

        [Fact]
        public async Task AddAsync_ValidVoucher_ReturnsAddedVoucherAndLogsAudit()
        {
            // Arrange
            var voucher = new PaymentVoucher { VoucherNo = "PV-001", Amount = 100m };
            var addedVoucher = new PaymentVoucher { Id = 1, VoucherNo = "PV-001", Amount = 100m };
            _repoMock.Setup(r => r.AddAsync(voucher, It.IsAny<CancellationToken>()))
                     .ReturnsAsync(addedVoucher);
            _auditServiceMock.Setup(a => a.LogAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(),
                It.IsAny<object>(), It.IsAny<object>(), It.IsAny<int?>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var service = CreateService();

            // Act
            var result = await service.AddAsync(voucher);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(addedVoucher.Id, result.Id);
            _repoMock.Verify(r => r.AddAsync(voucher, It.IsAny<CancellationToken>()), Times.Once);
            _auditServiceMock.Verify(a => a.LogAsync(nameof(PaymentVoucher), addedVoucher.Id, "Create", null, addedVoucher, It.IsAny<int?>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_ExistingVoucher_UpdatesAndLogsAudit()
        {
            // Arrange
            var existing = new PaymentVoucher { Id = 1, VoucherNo = "PV-001", Amount = 100m };
            var updated = new PaymentVoucher { Id = 1, VoucherNo = "PV-001-UPDATED", Amount = 150m };
            _repoMock.Setup(r => r.GetByIdAsync(existing.Id, It.IsAny<CancellationToken>()))
                     .ReturnsAsync(existing);
            _repoMock.Setup(r => r.UpdateAsync(updated, It.IsAny<CancellationToken>()))
                     .Returns(Task.CompletedTask);
            _auditServiceMock.Setup(a => a.LogAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(),
                It.IsAny<object>(), It.IsAny<object>(), It.IsAny<int?>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var service = CreateService();

            // Act
            await service.UpdateAsync(updated);

            // Assert
            _repoMock.Verify(r => r.UpdateAsync(It.Is<PaymentVoucher>(v => v.Amount == 150m && v.VoucherNo == "PV-001-UPDATED"), It.IsAny<CancellationToken>()), Times.Once);
            _auditServiceMock.Verify(a => a.LogAsync(nameof(PaymentVoucher), updated.Id, "Update", existing, updated, It.IsAny<int?>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_ExistingVoucher_DeletesAndLogsAudit()
        {
            // Arrange
            var existing = new PaymentVoucher { Id = 1, VoucherNo = "PV-001", Amount = 100m };
            _repoMock.Setup(r => r.GetByIdAsync(existing.Id, It.IsAny<CancellationToken>()))
                     .ReturnsAsync(existing);
            _repoMock.Setup(r => r.DeleteAsync(existing.Id, It.IsAny<CancellationToken>()))
                     .Returns(Task.CompletedTask);
            _auditServiceMock.Setup(a => a.LogAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(),
                It.IsAny<object>(), It.IsAny<object>(), It.IsAny<int?>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var service = CreateService();

            // Act
            await service.DeleteAsync(existing.Id);

            // Assert
            _repoMock.Verify(r => r.DeleteAsync(existing.Id, It.IsAny<CancellationToken>()), Times.Once);
            _auditServiceMock.Verify(a => a.LogAsync(nameof(PaymentVoucher), existing.Id, "Delete", existing, null, It.IsAny<int?>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetByIdAsync_RepositoryThrows_ThrowsException()
        {
            // Arrange
            var ex = new InvalidOperationException("DB error");
            _repoMock.Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                     .ThrowsAsync(ex);

            var service = CreateService();

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => service.GetByIdAsync(1));
        }
    }
}
