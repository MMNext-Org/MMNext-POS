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
    /// <summary>
    /// Unit tests for OutstandingService.GetCustomerAgeingAsync (AR ageing buckets).
    /// </summary>
    public class OutstandingServiceAgeingTests
    {
        private static readonly DateTime AsOf = new DateTime(2026, 10, 4);

        [Fact]
        public async Task GetCustomerAgeingAsync_NoRecords_ReturnsEmpty()
        {
            var service = CreateService(new List<CustomerOutstanding>());

            var result = await service.GetCustomerAgeingAsync(AsOf);

            Assert.Empty(result);
        }

        [Fact]
        public async Task GetCustomerAgeingAsync_BucketsByAge()
        {
            var records = new List<CustomerOutstanding>
            {
                NewOpen(customerId: 1, daysAgo: 10, balance: 100m),   // current (0-30)
                NewOpen(customerId: 1, daysAgo: 45, balance: 200m),   // 31-60
                NewOpen(customerId: 1, daysAgo: 75, balance: 300m),   // 61-90
                NewOpen(customerId: 1, daysAgo: 120, balance: 400m),  // 90+
            };
            var service = CreateService(records);

            var result = await service.GetCustomerAgeingAsync(AsOf);

            var entry = Assert.Single(result);
            Assert.Equal(100m, entry.CurrentBalance);
            Assert.Equal(200m, entry.Days31To60);
            Assert.Equal(300m, entry.Days61To90);
            Assert.Equal(400m, entry.Days90Plus);
            Assert.Equal(1000m, entry.TotalBalance);
        }

        [Fact]
        public async Task GetCustomerAgeingAsync_BoundaryDay30_IsCurrent_Day31_IsNextBucket()
        {
            var records = new List<CustomerOutstanding>
            {
                NewOpen(customerId: 1, daysAgo: 30, balance: 50m),
                NewOpen(customerId: 1, daysAgo: 31, balance: 60m),
                NewOpen(customerId: 1, daysAgo: 60, balance: 70m),
                NewOpen(customerId: 1, daysAgo: 61, balance: 80m),
                NewOpen(customerId: 1, daysAgo: 90, balance: 90m),
                NewOpen(customerId: 1, daysAgo: 91, balance: 100m),
            };
            var service = CreateService(records);

            var result = await service.GetCustomerAgeingAsync(AsOf);

            var entry = Assert.Single(result);
            Assert.Equal(50m, entry.CurrentBalance);
            Assert.Equal(130m, entry.Days31To60);
            Assert.Equal(170m, entry.Days61To90);
            Assert.Equal(100m, entry.Days90Plus);
        }

        [Fact]
        public async Task GetCustomerAgeingAsync_ExcludesNonOpenAndZeroAndFuture()
        {
            var records = new List<CustomerOutstanding>
            {
                NewOpen(customerId: 1, daysAgo: 10, balance: 100m, status: "Open"),
                NewOpen(customerId: 1, daysAgo: 10, balance: 100m, status: "Cleared"),
                NewOpen(customerId: 1, daysAgo: 10, balance: 0m),
                NewOpen(customerId: 1, daysAgo: -5, balance: 100m),   // future-dated, excluded
            };
            var service = CreateService(records);

            var result = await service.GetCustomerAgeingAsync(AsOf);

            var entry = Assert.Single(result);
            Assert.Equal(100m, entry.CurrentBalance);
            Assert.Equal(100m, entry.TotalBalance);
        }

        [Fact]
        public async Task GetCustomerAgeingAsync_GroupsPerCustomer()
        {
            var records = new List<CustomerOutstanding>
            {
                NewOpen(customerId: 2, daysAgo: 5, balance: 10m),
                NewOpen(customerId: 1, daysAgo: 200, balance: 99m),
                NewOpen(customerId: 2, daysAgo: 95, balance: 20m),
            };
            var service = CreateService(records);

            var result = await service.GetCustomerAgeingAsync(AsOf);

            Assert.Equal(2, result.Count);
            // Ordered by CustomerId
            var first = result[0];
            var second = result[1];
            Assert.Equal(1, first.CustomerId);
            Assert.Equal(99m, first.Days90Plus);
            Assert.Equal(2, second.CustomerId);
            Assert.Equal(10m, second.CurrentBalance);
            Assert.Equal(20m, second.Days90Plus);
            Assert.Equal(30m, second.TotalBalance);
        }

        private static CustomerOutstanding NewOpen(int customerId, int daysAgo, decimal balance, string status = "Open")
        {
            return new CustomerOutstanding
            {
                CustomerId = customerId,
                TransactionDate = AsOf.AddDays(-daysAgo),
                DebitAmount = balance,
                Balance = balance,
                Status = status
            };
        }

        private static OutstandingService CreateService(IReadOnlyList<CustomerOutstanding> customerRecords)
        {
            var customerRepo = new Mock<ICustomerOutstandingRepository>();
            customerRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(customerRecords);

            var supplierRepo = new Mock<ISupplierOutstandingRepository>();
            supplierRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<SupplierOutstanding>());

            var audit = new Mock<IAuditService>();

            return new OutstandingService(customerRepo.Object, supplierRepo.Object, audit.Object);
        }
    }
}
