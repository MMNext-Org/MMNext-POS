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
    public class SupplierServiceTests
    {
        private readonly Mock<ISupplierRepository> _repoMock = new();

        private ISupplierService CreateService()
        {
            return new SupplierService(_repoMock.Object);
        }

        private void SetupHappyPath(int supplierId = 1)
        {
            var supplier = new Supplier
            {
                Id = supplierId,
                Code = "SUP-001",
                Name = "Test Supplier",
                ContactPerson = "John Doe",
                Phone = "555-0001",
                Email = "supplier@test.com",
                Address = "123 Supplier St",
                City = "Yangon",
                TaxId = "TAX-001",
                CreditLimit = 100000m,
                PaymentTermDays = 30,
                IsActive = true,
                IsDeleted = false
            };

            _repoMock.Setup(r => r.GetByIdAsync(supplierId, It.IsAny<CancellationToken>()))
                     .ReturnsAsync(supplier);
            _repoMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                     .ReturnsAsync(new List<Supplier> { supplier });
            _repoMock.Setup(r => r.AddAsync(It.IsAny<Supplier>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync((Supplier s, CancellationToken _) => { s.Id = supplierId; return s; });
            _repoMock.Setup(r => r.UpdateAsync(It.IsAny<Supplier>(), It.IsAny<CancellationToken>()))
                     .Returns(Task.CompletedTask);
            _repoMock.Setup(r => r.DeleteAsync(supplierId, It.IsAny<CancellationToken>()))
                     .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task GetByIdAsync_ExistingSupplier_ReturnsSupplier()
        {
            SetupHappyPath();
            var service = CreateService();

            var result = await service.GetByIdAsync(1);

            Assert.NotNull(result);
            Assert.Equal(1, result!.Id);
            Assert.Equal("SUP-001", result.Code);
        }

        [Fact]
        public async Task GetByIdAsync_NonExistentSupplier_ReturnsNull()
        {
            _repoMock.Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync((Supplier?)null!);
            var service = CreateService();

            var result = await service.GetByIdAsync(999);

            Assert.Null(result);
        }

        [Fact]
        public async Task GetAllAsync_ReturnsAllSuppliers()
        {
            SetupHappyPath();
            var service = CreateService();

            var result = await service.GetAllAsync();

            Assert.NotNull(result);
            Assert.Single(result);
        }

        [Fact]
        public async Task AddAsync_ValidSupplier_ReturnsAddedSupplier()
        {
            SetupHappyPath();
            var service = CreateService();
            var supplier = new Supplier
            {
                Code = "SUP-002",
                Name = "New Supplier",
                ContactPerson = "Jane Smith",
                Phone = "555-0002",
                Email = "new@supplier.com",
                Address = "456 New St",
                City = "Yangon",
                TaxId = "TAX-002",
                CreditLimit = 200000m,
                PaymentTermDays = 45,
                IsActive = true
            };

            var result = await service.AddAsync(supplier);

            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            _repoMock.Verify(r => r.AddAsync(It.Is<Supplier>(s => s.Code == "SUP-002"), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_ExistingSupplier_UpdatesSupplier()
        {
            SetupHappyPath();
            var service = CreateService();
            var supplier = new Supplier
            {
                Id = 1,
                Code = "SUP-001",
                Name = "Updated Supplier",
                CreditLimit = 150000m,
                IsActive = true
            };

            await service.UpdateAsync(supplier);

            _repoMock.Verify(r => r.UpdateAsync(It.Is<Supplier>(s => s.Name == "Updated Supplier" && s.CreditLimit == 150000m), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_ExistingSupplier_DeletesSupplier()
        {
            SetupHappyPath();
            var service = CreateService();

            await service.DeleteAsync(1);

            _repoMock.Verify(r => r.DeleteAsync(1, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetByIdAsync_ThrowsException_WhenRepositoryThrows()
        {
            var ex = new InvalidOperationException("DB error");
            _repoMock.Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                     .ThrowsAsync(ex);
            var service = CreateService();

            await Assert.ThrowsAsync<InvalidOperationException>(() => service.GetByIdAsync(1));
        }

        [Fact]
        public async Task GetAllAsync_ThrowsException_WhenRepositoryThrows()
        {
            var ex = new InvalidOperationException("DB error");
            _repoMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                     .ThrowsAsync(ex);
            var service = CreateService();

            await Assert.ThrowsAsync<InvalidOperationException>(() => service.GetAllAsync());
        }
    }
}
