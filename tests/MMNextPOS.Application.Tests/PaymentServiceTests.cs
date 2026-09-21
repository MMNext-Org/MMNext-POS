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
    public class PaymentServiceTests
    {
        private readonly Mock<IPaymentRepository> _repoMock = new();
        private readonly Mock<IAuditService> _auditServiceMock = new();

        private IPaymentService CreateService()
        {
            return new PaymentService(_repoMock.Object, _auditServiceMock.Object);
        }

        private void SetupHappyPath(int paymentId = 1)
        {
            var payment = new Payment
            {
                Id = paymentId,
                PaymentNo = "PAY-2026-001",
                PaymentType = "Customer",
                CustomerId = 1,
                SaleId = 10,
                Amount = 100m,
                Method = "Cash",
                PaymentDate = DateTime.UtcNow,
                Status = "Completed"
            };

            _repoMock.Setup(r => r.GetByIdAsync(paymentId, It.IsAny<CancellationToken>()))
                     .ReturnsAsync(payment);
            _repoMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                     .ReturnsAsync(new List<Payment> { payment });
            _repoMock.Setup(r => r.AddAsync(It.IsAny<Payment>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync((Payment p, CancellationToken _) => { p.Id = paymentId; return p; });
            _repoMock.Setup(r => r.UpdateAsync(It.IsAny<Payment>(), It.IsAny<CancellationToken>()))
                     .Returns(Task.CompletedTask);
            _repoMock.Setup(r => r.DeleteAsync(paymentId, It.IsAny<CancellationToken>()))
                     .Returns(Task.CompletedTask);
            _repoMock.Setup(r => r.GetPageAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(new PagedResult<Payment> { Items = new List<Payment> { payment }, TotalCount = 1 });

            _auditServiceMock.Setup(a => a.LogAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(),
                It.IsAny<object?>(), It.IsAny<object?>(), It.IsAny<int?>(), It.IsAny<string?>(),
                It.IsAny<string?>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        // ───────────────────────── CRUD Tests ─────────────────────────

        [Fact]
        public async Task GetByIdAsync_ExistingPayment_ReturnsPayment()
        {
            SetupHappyPath();
            var service = CreateService();

            var result = await service.GetByIdAsync(1);

            Assert.NotNull(result);
            Assert.Equal(1, result!.Id);
            Assert.Equal("PAY-2026-001", result.PaymentNo);
        }

        [Fact]
        public async Task GetByIdAsync_NonExistentPayment_ReturnsNull()
        {
            _repoMock.Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync((Payment?)null!);
            var service = CreateService();

            var result = await service.GetByIdAsync(999);

            Assert.Null(result);
        }

        [Fact]
        public async Task GetAllAsync_ReturnsAllPayments()
        {
            SetupHappyPath();
            var service = CreateService();

            var result = await service.GetAllAsync();

            Assert.NotNull(result);
            Assert.Single(result);
        }

        [Fact]
        public async Task AddAsync_ValidPayment_ReturnsAddedPayment()
        {
            SetupHappyPath();
            var service = CreateService();
            var payment = new Payment
            {
                PaymentType = "Customer",
                CustomerId = 1,
                SaleId = 10,
                Amount = 200m,
                Method = "Card",
                PaymentDate = DateTime.UtcNow,
                Status = "Completed"
            };

            var result = await service.AddAsync(payment);

            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            _repoMock.Verify(r => r.AddAsync(It.Is<Payment>(p => p.Amount == 200m), It.IsAny<CancellationToken>()), Times.Once);
            _auditServiceMock.Verify(a => a.LogAsync(nameof(Payment), 1, "Create", null, It.IsAny<Payment>(),
                It.IsAny<int?>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_ExistingPayment_UpdatesPayment()
        {
            SetupHappyPath();
            var service = CreateService();
            var payment = new Payment
            {
                Id = 1,
                PaymentNo = "PAY-2026-001",
                PaymentType = "Customer",
                CustomerId = 1,
                Amount = 150m,
                Method = "Cash",
                PaymentDate = DateTime.UtcNow,
                Status = "Completed"
            };

            await service.UpdateAsync(payment);

            _repoMock.Verify(r => r.UpdateAsync(It.Is<Payment>(p => p.Amount == 150m), It.IsAny<CancellationToken>()), Times.Once);
            _auditServiceMock.Verify(a => a.LogAsync(nameof(Payment), 1, "Update", It.IsAny<Payment>(), It.IsAny<Payment>(),
                It.IsAny<int?>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_ExistingPayment_DeletesPayment()
        {
            SetupHappyPath();
            var service = CreateService();

            await service.DeleteAsync(1);

            _repoMock.Verify(r => r.DeleteAsync(1, It.IsAny<CancellationToken>()), Times.Once);
            _auditServiceMock.Verify(a => a.LogAsync(nameof(Payment), 1, "Delete", It.IsAny<Payment>(), null,
                It.IsAny<int?>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        // ───────────────────────── Query by Reference Tests ─────────────────────────

        [Fact]
        public async Task GetBySaleAsync_SaleId_ReturnsPaymentsForSale()
        {
            var payment1 = new Payment { Id = 1, SaleId = 10, Amount = 100m };
            var payment2 = new Payment { Id = 2, SaleId = 10, Amount = 50m };
            var payment3 = new Payment { Id = 3, SaleId = 20, Amount = 200m };
            _repoMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                     .ReturnsAsync(new List<Payment> { payment1, payment2, payment3 });
            var service = CreateService();

            var result = await service.GetBySaleAsync(10);

            Assert.Equal(2, result.Count);
            Assert.All(result, p => Assert.Equal(10, p.SaleId));
        }

        [Fact]
        public async Task GetByPurchaseAsync_PurchaseId_ReturnsPaymentsForPurchase()
        {
            var payment1 = new Payment { Id = 1, PurchaseId = 5, Amount = 100m };
            var payment2 = new Payment { Id = 2, PurchaseId = 5, Amount = 50m };
            var payment3 = new Payment { Id = 3, PurchaseId = 10, Amount = 200m };
            _repoMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                     .ReturnsAsync(new List<Payment> { payment1, payment2, payment3 });
            var service = CreateService();

            var result = await service.GetByPurchaseAsync(5);

            Assert.Equal(2, result.Count);
            Assert.All(result, p => Assert.Equal(5, p.PurchaseId));
        }

        [Fact]
        public async Task GetByCustomerAsync_CustomerId_ReturnsPaymentsForCustomer()
        {
            var payment1 = new Payment { Id = 1, CustomerId = 1, Amount = 100m };
            var payment2 = new Payment { Id = 2, CustomerId = 1, Amount = 50m };
            var payment3 = new Payment { Id = 3, CustomerId = 2, Amount = 200m };
            _repoMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                     .ReturnsAsync(new List<Payment> { payment1, payment2, payment3 });
            var service = CreateService();

            var result = await service.GetByCustomerAsync(1);

            Assert.Equal(2, result.Count);
            Assert.All(result, p => Assert.Equal(1, p.CustomerId));
        }

        [Fact]
        public async Task GetBySupplierAsync_SupplierId_ReturnsPaymentsForSupplier()
        {
            var payment1 = new Payment { Id = 1, SupplierId = 1, Amount = 100m };
            var payment2 = new Payment { Id = 2, SupplierId = 1, Amount = 50m };
            var payment3 = new Payment { Id = 3, SupplierId = 2, Amount = 200m };
            _repoMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                     .ReturnsAsync(new List<Payment> { payment1, payment2, payment3 });
            var service = CreateService();

            var result = await service.GetBySupplierAsync(1);

            Assert.Equal(2, result.Count);
            Assert.All(result, p => Assert.Equal(1, p.SupplierId));
        }

        [Fact]
        public async Task GetByDateRangeAsync_FromTo_ReturnsPaymentsInRange()
        {
            var today = DateTime.UtcNow;
            var payment1 = new Payment { Id = 1, PaymentDate = today.AddDays(-5), Amount = 100m };
            var payment2 = new Payment { Id = 2, PaymentDate = today, Amount = 50m };
            var payment3 = new Payment { Id = 3, PaymentDate = today.AddDays(5), Amount = 200m };
            _repoMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                     .ReturnsAsync(new List<Payment> { payment1, payment2, payment3 });
            var service = CreateService();

            var result = await service.GetByDateRangeAsync(today.AddDays(-10), today.AddDays(-1));

            Assert.Single(result);
            Assert.Equal(1, result[0].Id);
        }

        // ───────────────────────── ProcessPaymentAsync Tests ─────────────────────────

        [Fact]
        public async Task ProcessPaymentAsync_ValidPayment_ReturnsProcessedPayment()
        {
            SetupHappyPath();
            var service = CreateService();
            var payment = new Payment
            {
                PaymentType = "Customer",
                CustomerId = 1,
                SaleId = 10,
                Amount = 100m,
                Method = "Cash",
                PaymentDate = DateTime.UtcNow,
                Status = "Completed"
            };

            var result = await service.ProcessPaymentAsync(payment);

            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.NotNull(result.PaymentNo);
            Assert.StartsWith("PAY-", result.PaymentNo);
        }

        [Fact]
        public async Task ProcessPaymentAsync_NullPayment_ThrowsArgumentNullException()
        {
            var service = CreateService();

            await Assert.ThrowsAsync<ArgumentNullException>(() => service.ProcessPaymentAsync(null!));
        }

        [Fact]
        public async Task ProcessPaymentAsync_ZeroAmount_ThrowsValidationException()
        {
            var service = CreateService();
            var payment = new Payment
            {
                PaymentType = "Customer",
                CustomerId = 1,
                Amount = 0m,
                Method = "Cash",
                PaymentDate = DateTime.UtcNow
            };

            await Assert.ThrowsAsync<ValidationException>(() => service.ProcessPaymentAsync(payment));
        }

        [Fact]
        public async Task ProcessPaymentAsync_EmptyMethod_ThrowsValidationException()
        {
            var service = CreateService();
            var payment = new Payment
            {
                PaymentType = "Customer",
                CustomerId = 1,
                Amount = 100m,
                Method = "",
                PaymentDate = DateTime.UtcNow
            };

            await Assert.ThrowsAsync<ValidationException>(() => service.ProcessPaymentAsync(payment));
        }

        [Fact]
        public async Task ProcessPaymentAsync_GeneratesPaymentNumber_WhenNotProvided()
        {
            SetupHappyPath();
            var service = CreateService();
            var payment = new Payment
            {
                PaymentType = "Customer",
                CustomerId = 1,
                Amount = 100m,
                Method = "Cash",
                PaymentDate = DateTime.UtcNow,
                Status = "Completed"
            };

            var result = await service.ProcessPaymentAsync(payment);

            Assert.NotNull(result.PaymentNo);
            Assert.StartsWith("PAY-", result.PaymentNo);
        }
    }
}
