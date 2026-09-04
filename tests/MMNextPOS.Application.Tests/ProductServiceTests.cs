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
    public class ProductServiceTests
    {
        private readonly Mock<IProductRepository> _productRepoMock = new();

        private IProductService CreateService()
        {
            return new ProductService(_productRepoMock.Object);
        }

        [Fact]
        public async Task GetByIdAsync_ExistingProduct_ReturnsProduct()
        {
            // Arrange
            var product = new Product { Id = 1, Sku = "SKU001", Name = "Test Product", Price = 10m, StockQuantity = 50 };
            _productRepoMock.Setup(r => r.GetByIdAsync(product.Id, It.IsAny<CancellationToken>()))
                            .ReturnsAsync(product);

            var service = CreateService();

            // Act
            var result = await service.GetByIdAsync(product.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(product.Id, result.Id);
            Assert.Equal(product.Sku, result.Sku);
            Assert.Equal(product.Name, result.Name);
            Assert.Equal(product.Price, result.Price);
            Assert.Equal(product.StockQuantity, result.StockQuantity);
        }

        [Fact]
        public async Task GetByIdAsync_NonExistentProduct_ReturnsNull()
        {
            // Arrange
            _productRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                            .ReturnsAsync((Product?)null!);

            var service = CreateService();

            // Act
            var result = await service.GetByIdAsync(999);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetAllAsync_ReturnsAllProducts()
        {
            // Arrange
            var products = new List<Product>
            {
                new() { Id = 1, Sku = "SKU001", Name = "Product A", Price = 10m, StockQuantity = 50 },
                new() { Id = 2, Sku = "SKU002", Name = "Product B", Price = 20m, StockQuantity = 30 },
                new() { Id = 3, Sku = "SKU003", Name = "Product C", Price = 15m, StockQuantity = 100 }
            };
            _productRepoMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                            .ReturnsAsync(products);

            var service = CreateService();

            // Act
            var result = await service.GetAllAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.Count);
        }

        [Fact]
        public async Task AddAsync_ValidProduct_ReturnsAddedProduct()
        {
            // Arrange
            var product = new Product { Sku = "SKU001", Name = "New Product", Price = 15m, StockQuantity = 100 };
            var addedProduct = new Product { Id = 1, Sku = "SKU001", Name = "New Product", Price = 15m, StockQuantity = 100 };
            _productRepoMock.Setup(r => r.AddAsync(product, It.IsAny<CancellationToken>()))
                            .ReturnsAsync(addedProduct);

            var service = CreateService();

            // Act
            var result = await service.AddAsync(product);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(addedProduct.Id, result.Id);
            Assert.Equal(product.Sku, result.Sku);
            _productRepoMock.Verify(r => r.AddAsync(product, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_ExistingProduct_UpdatesProduct()
        {
            // Arrange
            var existingProduct = new Product { Id = 1, Sku = "SKU001", Name = "Old Name", Price = 10m, StockQuantity = 50 };
            var updatedProduct = new Product { Id = 1, Sku = "SKU001", Name = "New Name", Price = 15m, StockQuantity = 75 };
            _productRepoMock.Setup(r => r.UpdateAsync(existingProduct, It.IsAny<CancellationToken>()))
                            .Returns(Task.CompletedTask);

            var service = CreateService();

            // Act
            await service.UpdateAsync(updatedProduct);

            // Assert
            _productRepoMock.Verify(r => r.UpdateAsync(It.Is<Product>(p => p.Name == "New Name" && p.Price == 15m), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_NonExistentProduct_StillCallsUpdate()
        {
            // Arrange
            var nonExistentProduct = new Product { Id = 999, Sku = "SKU999", Name = "Does Not Exist", Price = 0m, StockQuantity = 0 };
            _productRepoMock.Setup(r => r.UpdateAsync(nonExistentProduct, It.IsAny<CancellationToken>()))
                            .Returns(Task.CompletedTask);

            var service = CreateService();

            // Act
            await service.UpdateAsync(nonExistentProduct);

            // Assert
            _productRepoMock.Verify(r => r.UpdateAsync(nonExistentProduct, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_ExistingProduct_DeletesProduct()
        {
            // Arrange
            var product = new Product { Id = 1, Sku = "SKU001", Name = "Product To Delete", Price = 10m, StockQuantity = 50 };
            _productRepoMock.Setup(r => r.DeleteAsync(product.Id, It.IsAny<CancellationToken>()))
                            .Returns(Task.CompletedTask);

            var service = CreateService();

            // Act
            await service.DeleteAsync(product.Id);

            // Assert
            _productRepoMock.Verify(r => r.DeleteAsync(product.Id, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_NonExistentProduct_StillCallsDelete()
        {
            // Arrange
            _productRepoMock.Setup(r => r.DeleteAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                            .Returns(Task.CompletedTask);

            var service = CreateService();

            // Act
            await service.DeleteAsync(999);

            // Assert
            _productRepoMock.Verify(r => r.DeleteAsync(999, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetByIdAsync_ThrowsArgumentException_WhenRepositoryThrows()
        {
            // Arrange
            var ex = new InvalidOperationException("DB error");
            _productRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                            .ThrowsAsync(ex);

            var service = CreateService();

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => service.GetByIdAsync(1));
        }
    }
}
