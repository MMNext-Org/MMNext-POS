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
    public class PurchaseServiceTests
    {
        private readonly Mock<IPurchaseRepository> _purchaseRepoMock = new();
        private readonly Mock<IPurchaseDetailRepository> _detailRepoMock = new();
        private readonly Mock<IPurchaseReturnRepository> _returnRepoMock = new();
        private readonly Mock<IPurchaseReturnDetailRepository> _returnDetailRepoMock = new();
        private readonly Mock<IProductRepository> _productRepoMock = new();
        private readonly Mock<IAuditService> _auditServiceMock = new();

        private IPurchaseService CreateService()
        {
            return new PurchaseService(
                _purchaseRepoMock.Object,
                _detailRepoMock.Object,
                _returnRepoMock.Object,
                _returnDetailRepoMock.Object,
                _productRepoMock.Object,
                _auditServiceMock.Object);
        }

        [Fact]
        public async Task GetByIdAsync_ExistingPurchase_ReturnsPurchase()
        {
            var purchase = new Purchase
            {
                Id = 1,
                InvoiceNo = "PUR-001",
                SupplierId = 10,
                PurchaseDate = DateTime.Today,
                TotalAmount = 1000m,
                Status = "Active"
            };
            _purchaseRepoMock.Setup(r => r.GetByIdAsync(purchase.Id, It.IsAny<CancellationToken>()))
                            .ReturnsAsync(purchase);

            var service = CreateService();
            var result = await service.GetByIdAsync(purchase.Id);

            Assert.NotNull(result);
            Assert.Equal(purchase.Id, result.Id);
            Assert.Equal(purchase.InvoiceNo, result.InvoiceNo);
            Assert.Equal(purchase.TotalAmount, result.TotalAmount);
        }

        [Fact]
        public async Task GetPageAsync_ReturnsPagedResult()
        {
            var purchases = new List<Purchase>
            {
                new() { Id = 1, InvoiceNo = "PUR-001", TotalAmount = 100m },
                new() { Id = 2, InvoiceNo = "PUR-002", TotalAmount = 200m },
                new() { Id = 3, InvoiceNo = "PUR-003", TotalAmount = 300m },
                new() { Id = 4, InvoiceNo = "PUR-004", TotalAmount = 400m },
                new() { Id = 5, InvoiceNo = "PUR-005", TotalAmount = 500m },
            };
            _purchaseRepoMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                            .ReturnsAsync(purchases);

            var service = CreateService();

            // Simulate paging
            var all = await _purchaseRepoMock.Object.GetAllAsync(CancellationToken.None);
            var paged = all.Skip((1 - 1) * 2).Take(2).ToList();

            Assert.Equal(2, paged.Count);
        }

        [Fact]
        public async Task AddAsync_ValidPurchase_ReturnsAddedPurchase()
        {
            var purchase = new Purchase { InvoiceNo = "PUR-004", SupplierId = 1, PurchaseDate = DateTime.Today, TotalAmount = 150.75m, Status = "Active" };
            var addedPurchase = new Purchase { Id = 4, InvoiceNo = "PUR-004", SupplierId = 1, PurchaseDate = DateTime.Today, TotalAmount = 150.75m, Status = "Active" };
            _purchaseRepoMock.Setup(r => r.AddAsync(purchase, It.IsAny<CancellationToken>()))
                            .ReturnsAsync(addedPurchase);
            _auditServiceMock.Setup(a => a.LogAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(),
                It.IsAny<object>(), It.IsAny<object>(), It.IsAny<int?>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var service = CreateService();
            var result = await service.AddAsync(purchase);

            Assert.NotNull(result);
            Assert.Equal(addedPurchase.Id, result.Id);
            Assert.Equal(purchase.InvoiceNo, result.InvoiceNo);
            _purchaseRepoMock.Verify(r => r.AddAsync(purchase, It.IsAny<CancellationToken>()), Times.Once);
            _auditServiceMock.Verify(a => a.LogAsync(nameof(Purchase), addedPurchase.Id, "Create", null, addedPurchase, It.IsAny<int?>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_ExistingPurchase_UpdatesPurchase()
        {
            var existingPurchase = new Purchase { Id = 1, InvoiceNo = "PUR-001", TotalAmount = 100m, Status = "Active" };
            var updatedPurchase = new Purchase { Id = 1, InvoiceNo = "PUR-001", TotalAmount = 150m, Status = "Completed" };
            _purchaseRepoMock.Setup(r => r.GetByIdAsync(existingPurchase.Id, It.IsAny<CancellationToken>()))
                            .ReturnsAsync(existingPurchase);
            _purchaseRepoMock.Setup(r => r.UpdateAsync(existingPurchase, It.IsAny<CancellationToken>()))
                            .Returns(Task.CompletedTask);
            _auditServiceMock.Setup(a => a.LogAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(),
                It.IsAny<object>(), It.IsAny<object>(), It.IsAny<int?>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var service = CreateService();
            await service.UpdateAsync(updatedPurchase);

            _purchaseRepoMock.Verify(r => r.UpdateAsync(It.Is<Purchase>(p => p.TotalAmount == 150m && p.Status == "Completed"), It.IsAny<CancellationToken>()), Times.Once);
            _auditServiceMock.Verify(a => a.LogAsync(nameof(Purchase), updatedPurchase.Id, "Update", existingPurchase, updatedPurchase, It.IsAny<int?>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_ExistingPurchase_DeletesPurchase()
        {
            var purchase = new Purchase { Id = 1, InvoiceNo = "PUR-001", TotalAmount = 100m };
            _purchaseRepoMock.Setup(r => r.GetByIdAsync(purchase.Id, It.IsAny<CancellationToken>()))
                            .ReturnsAsync(purchase);
            _purchaseRepoMock.Setup(r => r.DeleteAsync(purchase.Id, It.IsAny<CancellationToken>()))
                            .Returns(Task.CompletedTask);
            _auditServiceMock.Setup(a => a.LogAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(),
                It.IsAny<object>(), It.IsAny<object>(), It.IsAny<int?>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var service = CreateService();
            await service.DeleteAsync(purchase.Id);

            _purchaseRepoMock.Verify(r => r.DeleteAsync(purchase.Id, It.IsAny<CancellationToken>()), Times.Once);
            _auditServiceMock.Verify(a => a.LogAsync(nameof(Purchase), purchase.Id, "Delete", purchase, null, It.IsAny<int?>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetByIdAsync_ThrowsException_WhenRepositoryThrows()
        {
            var ex = new InvalidOperationException("DB error");
            _purchaseRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                            .ThrowsAsync(ex);

            var service = CreateService();
            await Assert.ThrowsAsync<InvalidOperationException>(() => service.GetByIdAsync(1));
        }
    }

    public class PurchaseDetailServiceTests
    {
        private readonly Mock<IPurchaseDetailRepository> _purchaseDetailRepoMock = new();
        private readonly Mock<IAuditService> _auditServiceMock = new();

        private IPurchaseDetailService CreateService()
        {
            return new PurchaseDetailService(_purchaseDetailRepoMock.Object, _auditServiceMock.Object);
        }

        [Fact]
        public async Task GetByIdAsync_ExistingPurchaseDetail_ReturnsPurchaseDetail()
        {
            var detail = new PurchaseDetail { Id = 1, PurchaseId = 1, ProductId = 10, Quantity = 5, UnitPrice = 100m, LineTotal = 500m };
            _purchaseDetailRepoMock.Setup(r => r.GetByIdAsync(detail.Id, It.IsAny<CancellationToken>()))
                                   .ReturnsAsync(detail);

            var service = CreateService();
            var result = await service.GetByIdAsync(detail.Id);

            Assert.NotNull(result);
            Assert.Equal(detail.Id, result.Id);
            Assert.Equal(detail.PurchaseId, result.PurchaseId);
            Assert.Equal(detail.Quantity, result.Quantity);
            Assert.Equal(detail.UnitPrice, result.UnitPrice);
        }

        [Fact]
        public async Task AddAsync_ValidPurchaseDetail_ReturnsAddedDetail()
        {
            var detail = new PurchaseDetail { PurchaseId = 1, ProductId = 10, Quantity = 3, UnitPrice = 50m, LineTotal = 150m };
            var addedDetail = new PurchaseDetail { Id = 2, PurchaseId = 1, ProductId = 10, Quantity = 3, UnitPrice = 50m, LineTotal = 150m };
            _purchaseDetailRepoMock.Setup(r => r.AddAsync(detail, It.IsAny<CancellationToken>()))
                                   .ReturnsAsync(addedDetail);
            _auditServiceMock.Setup(a => a.LogAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(),
                It.IsAny<object>(), It.IsAny<object>(), It.IsAny<int?>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var service = CreateService();
            var result = await service.AddAsync(detail);

            Assert.NotNull(result);
            Assert.Equal(addedDetail.Id, result.Id);
            Assert.Equal(detail.PurchaseId, result.PurchaseId);
            _purchaseDetailRepoMock.Verify(r => r.AddAsync(detail, It.IsAny<CancellationToken>()), Times.Once);
            _auditServiceMock.Verify(a => a.LogAsync(nameof(PurchaseDetail), addedDetail.Id, "Create", null, addedDetail, It.IsAny<int?>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
