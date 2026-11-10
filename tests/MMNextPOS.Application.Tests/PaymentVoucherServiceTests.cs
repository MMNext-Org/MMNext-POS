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
        private readonly Mock<IAuditService> _auditMock = new();

        private IPaymentVoucherService CreateService() => new PaymentVoucherService(_repoMock.Object, _auditMock.Object);

        private static PaymentVoucher NewVoucher(int id = 1, string voucherNo = "PV-001") => new()
        {
            Id = id,
            VoucherNo = voucherNo,
            VoucherDate = DateTime.UtcNow,
            Amount = 100m,
            Status = "Posted"
        };

        [Fact]
        public void Constructor_NullRepo_ThrowsArgumentNullException() =>
            Assert.Throws<ArgumentNullException>(() => new PaymentVoucherService(null!, _auditMock.Object));

        [Fact]
        public void Constructor_NullAudit_ThrowsArgumentNullException() =>
            Assert.Throws<ArgumentNullException>(() => new PaymentVoucherService(_repoMock.Object, null!));

        [Fact]
        public async Task AddAsync_PersistsAndLogsAudit()
        {
            // Arrange
            var voucher = NewVoucher();
            _repoMock.Setup(r => r.AddAsync(It.IsAny<PaymentVoucher>(), It.IsAny<CancellationToken>()))
                     .Callback<PaymentVoucher, CancellationToken>((v, _) => v.Id = 42)
                     .ReturnsAsync((PaymentVoucher v, CancellationToken _) => v);
            _auditMock.Setup(a => a.LogAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<object>(), It.IsAny<object>(), It.IsAny<int?>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                      .Returns(Task.CompletedTask);

            var service = CreateService();

            // Act
            var result = await service.AddAsync(voucher);

            // Assert
            Assert.Equal(42, result.Id);
            _repoMock.Verify(r => r.AddAsync(voucher, It.IsAny<CancellationToken>()), Times.Once);
            _auditMock.Verify(a => a.LogAsync(nameof(PaymentVoucher), 42, "Create", null, It.IsAny<object>(), It.IsAny<int?>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_FetchesExisting_UpdatesAndLogsAudit()
        {
            // Arrange
            var existing = NewVoucher(1);
            var updated = NewVoucher(1, "PV-002");
            updated.Amount = 200m;
            _repoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(existing);
            _repoMock.Setup(r => r.UpdateAsync(It.IsAny<PaymentVoucher>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
            _auditMock.Setup(a => a.LogAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<object>(), It.IsAny<object>(), It.IsAny<int?>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                      .Returns(Task.CompletedTask);

            var service = CreateService();
            await service.UpdateAsync(updated);

            // Assert
            _repoMock.Verify(r => r.UpdateAsync(updated, It.IsAny<CancellationToken>()), Times.Once);
            _auditMock.Verify(a => a.LogAsync(nameof(PaymentVoucher), 1, "Update", existing, updated, It.IsAny<int?>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_DeletesAndLogsAudit()
        {
            // Arrange
            var existing = NewVoucher(5, "PV-005");
            _repoMock.Setup(r => r.GetByIdAsync(5, It.IsAny<CancellationToken>())).ReturnsAsync(existing);
            _repoMock.Setup(r => r.DeleteAsync(5, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
            _auditMock.Setup(a => a.LogAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<object>(), It.IsAny<object>(), It.IsAny<int?>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                      .Returns(Task.CompletedTask);

            var service = CreateService();
            await service.DeleteAsync(5);

            // Assert
            _repoMock.Verify(r => r.DeleteAsync(5, It.IsAny<CancellationToken>()), Times.Once);
            _auditMock.Verify(a => a.LogAsync(nameof(PaymentVoucher), 5, "Delete", existing, null, It.IsAny<int?>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetByIdAsync_PassesThroughToRepo()
        {
            // Arrange
            var voucher = NewVoucher(3);
            _repoMock.Setup(r => r.GetByIdAsync(3, It.IsAny<CancellationToken>())).ReturnsAsync(voucher);
            var service = CreateService();

            // Act
            var result = await service.GetByIdAsync(3);

            // Assert
            Assert.Same(voucher, result);
        }

        [Fact]
        public async Task GetByVoucherNoAsync_PassesThroughToRepo()
        {
            // Arrange
            var voucher = NewVoucher(4, "PV-004");
            _repoMock.Setup(r => r.GetByVoucherNoAsync("PV-004", It.IsAny<CancellationToken>())).ReturnsAsync(voucher);
            var service = CreateService();

            // Act
            var result = await service.GetByVoucherNoAsync("PV-004");

            Assert.Same(voucher, result);
        }

        [Fact]
        public async Task GetByDateRangeAsync_PassesThroughToRepo()
        {
            // Arrange
            var from = new DateTime(2026, 1, 1);
            var to = new DateTime(2026, 1, 31);
            var vouchers = new List<PaymentVoucher> { NewVoucher(1), NewVoucher(2) };
            _repoMock.Setup(r => r.GetByDateRangeAsync(from, to, It.IsAny<CancellationToken>())).ReturnsAsync(vouchers);
            var service = CreateService();

            // Act
            var result = await service.GetByDateRangeAsync(from, to);

            Assert.Collection(result, _ => { }, _ => { });
        }

        [Fact]
        public async Task GetByCustomerAsync_PassesThroughToRepo()
        {
            // Arrange
            _repoMock.Setup(r => r.GetByCustomerAsync(7, It.IsAny<CancellationToken>())).ReturnsAsync(new List<PaymentVoucher>());
            var service = CreateService();

            // Act
            var result = await service.GetByCustomerAsync(7);

            Assert.Empty(result);
        }

        [Fact]
        public async Task GetBySupplierAsync_PassesThroughToRepo()
        {
            // Arrange
            _repoMock.Setup(r => r.GetBySupplierAsync(8, It.IsAny<CancellationToken>())).ReturnsAsync(new List<PaymentVoucher>());
            var service = CreateService();

            // Act
            var result = await service.GetBySupplierAsync(8);

            Assert.Empty(result);
        }

        [Fact]
        public async Task GetBySaleAsync_PassesThroughToRepo()
        {
            // Arrange
            _repoMock.Setup(r => r.GetBySaleAsync(9, It.IsAny<CancellationToken>())).ReturnsAsync(new List<PaymentVoucher>());
            var service = CreateService();

            // Act
            var result = await service.GetBySaleAsync(9);

            Assert.Empty(result);
        }

        [Fact]
        public async Task GetByPurchaseAsync_PassesThroughToRepo()
        {
            // Arrange
            _repoMock.Setup(r => r.GetByPurchaseAsync(10, It.IsAny<CancellationToken>())).ReturnsAsync(new List<PaymentVoucher>());
            var service = CreateService();

            // Act
            var result = await service.GetByPurchaseAsync(10);

            Assert.Empty(result);
        }
    }
}
