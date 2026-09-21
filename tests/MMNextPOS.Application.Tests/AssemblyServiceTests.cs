using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MMNextPOS.Application.Services;
using MMNextPOS.Domain.Models;
using MMNextPOS.Infrastructure;
using MMNextPOS.Infrastructure.Repositories;
using Moq;
using Xunit;

namespace MMNextPOS.Application.Tests
{
    public class AssemblyServiceTests
    {
        private readonly Mock<IAssemblyRepository> _assemblyRepoMock = new();
        private readonly Mock<IAssemblyDetailRepository> _detailRepoMock = new();
        private readonly Mock<IProductRepository> _productRepoMock = new();
        private readonly Mock<IStockMovementService> _stockMovementMock = new();
        private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
        private readonly Mock<IAuditService> _auditServiceMock = new();

        private IAssemblyService CreateService()
        {
            return new AssemblyService(
                _assemblyRepoMock.Object,
                _detailRepoMock.Object,
                _productRepoMock.Object,
                _stockMovementMock.Object,
                _unitOfWorkMock.Object,
                _auditServiceMock.Object);
        }

        private void SetupHappyPath(int assemblyId = 1)
        {
            var assembly = new Assembly
            {
                Id = assemblyId,
                AssemblyNo = $"ASM-2026-{assemblyId:D3}",
                OutputProductId = 100,
                OutputQuantity = 1,
                TotalCost = 50m,
                Status = "Active"
            };

            var details = new List<AssemblyDetail>
            {
                new() { Id = 1, AssemblyId = assemblyId, ComponentProductId = 1, Quantity = 2, UnitCost = 20m, LineTotal = 40m },
                new() { Id = 2, AssemblyId = assemblyId, ComponentProductId = 2, Quantity = 1, UnitCost = 10m, LineTotal = 10m }
            };

            _assemblyRepoMock.Setup(r => r.GetByIdAsync(assemblyId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(assembly);
            _assemblyRepoMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Assembly> { assembly });
            _detailRepoMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(details);
            _assemblyRepoMock.Setup(r => r.AddAsync(It.IsAny<Assembly>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Assembly a, CancellationToken _) => { a.Id = assemblyId; return a; });
            _assemblyRepoMock.Setup(r => r.UpdateAsync(It.IsAny<Assembly>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            _detailRepoMock.Setup(r => r.AddAsync(It.IsAny<AssemblyDetail>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((AssemblyDetail d, CancellationToken _) => { d.Id = 1; return d; });
            _productRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Product { Id = 1, Name = "Component 1", StockQuantity = 100, Price = 20m });
            _productRepoMock.Setup(r => r.GetByIdAsync(2, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Product { Id = 2, Name = "Component 2", StockQuantity = 50, Price = 10m });
            _productRepoMock.Setup(r => r.TryIncrementStockAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);
            _productRepoMock.Setup(r => r.TryDecrementStockAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);
            _stockMovementMock.Setup(s => s.AddAssemblyMovementAsync(It.IsAny<int>(), It.IsAny<IReadOnlyList<AssemblyDetail>>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new StockMovement { Id = 1 });
            _unitOfWorkMock.Setup(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            _unitOfWorkMock.Setup(u => u.CommitAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            _auditServiceMock.Setup(a => a.LogAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(),
                It.IsAny<object>(), It.IsAny<object>(), It.IsAny<int?>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task GetByIdAsync_ExistingAssembly_ReturnsAssembly()
        {
            // Arrange
            SetupHappyPath();
            var service = CreateService();

            // Act
            var result = await service.GetByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
        }

        [Fact]
        public async Task GetAllAsync_ReturnsAllAssemblies()
        {
            // Arrange
            SetupHappyPath();
            var service = CreateService();

            // Act
            var result = await service.GetAllAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
        }

        [Fact]
        public async Task CreateAsync_ValidAssembly_CreatesAssembly()
        {
            // Arrange
            SetupHappyPath();
            var service = CreateService();
            var assembly = new Assembly
            {
                AssemblyNo = "ASM-2026-001",
                OutputProductId = 100,
                OutputQuantity = 10,
                TotalCost = 0m
            };
            var components = new List<AssemblyDetail>
            {
                new() { ComponentProductId = 1, Quantity = 2, UnitCost = 20m },
                new() { ComponentProductId = 2, Quantity = 1, UnitCost = 10m }
            };

            // Act
            var result = await service.CreateAsync(assembly, components, 1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            _unitOfWorkMock.Verify(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWorkMock.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
            _assemblyRepoMock.Verify(r => r.AddAsync(It.IsAny<Assembly>(), It.IsAny<CancellationToken>()), Times.Once);
            Assert.Equal(2, components.Count); // 2 components created
        }

        [Fact]
        public async Task DeleteAsync_ExistingAssembly_DeletesAssembly()
        {
            // Arrange
            SetupHappyPath();
            var service = CreateService();

            // Act
            await service.DeleteAsync(1, 1);

            // Assert
            _assemblyRepoMock.Verify(r => r.DeleteAsync(1, It.IsAny<CancellationToken>()), Times.Once);
            _auditServiceMock.Verify(a => a.LogAsync(nameof(Assembly), 1, "Delete", It.IsAny<object>(), null, 1, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task BuildAssemblyAsync_ValidRequest_BuildsAndUpdatesStock()
        {
            // Arrange
            SetupHappyPath();
            var service = CreateService();

            // Act
            var result = await service.BuildAssemblyAsync(1, 5, 1);

            // Assert
            Assert.NotNull(result);
            _unitOfWorkMock.Verify(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWorkMock.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
            _stockMovementMock.Verify(s => s.AddAssemblyMovementAsync(It.IsAny<int>(), It.IsAny<IReadOnlyList<AssemblyDetail>>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Once);
            _productRepoMock.Verify(r => r.TryIncrementStockAsync(100, 5, 1, It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task BuildAssemblyAsync_InsufficientComponentStock_ThrowsValidationException()
        {
            // Arrange
            SetupHappyPath();
            _productRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Product { Id = 1, Name = "Component 1", StockQuantity = 1, Price = 20m }); // Only 1 left, need 2

            var service = CreateService();

            // Act & Assert
            await Assert.ThrowsAsync<ValidationException>(() => service.BuildAssemblyAsync(1, 1, 1));
            _unitOfWorkMock.Verify(u => u.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task DeassembleAsync_ValidRequest_ReversesAssembly()
        {
            // Arrange
            SetupHappyPath();
            var service = CreateService();

            // Act
            var result = await service.DeassembleAsync(1, 2, 1);

            // Assert
            Assert.NotNull(result);
            _unitOfWorkMock.Verify(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWorkMock.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
            _stockMovementMock.Verify(s => s.AddDeassemblyMovementAsync(It.IsAny<int>(), It.IsAny<IReadOnlyList<AssemblyDetail>>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Once);
            // Verify stock adjustments for both components
            _productRepoMock.Verify(r => r.TryIncrementStockAsync(1, 4, 1, It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once); // 2 units * 2 qty
            _productRepoMock.Verify(r => r.TryIncrementStockAsync(2, 2, 1, It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once); // 2 units * 1 qty
            _productRepoMock.Verify(r => r.TryDecrementStockAsync(100, 2, 1, It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once); // Output
        }

        [Fact]
        public async Task GetAssemblyDetailsAsync_ValidAssembly_ReturnsDetails()
        {
            // Arrange
            SetupHappyPath();
            var service = CreateService();

            // Act
            var result = await service.GetAssemblyDetailsAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task CalculateAssemblyCostAsync_ValidAssembly_ReturnsCost()
        {
            // Arrange
            SetupHappyPath();
            var service = CreateService();

            // Act
            var result = await service.CalculateAssemblyCostAsync(1);

            // Assert
            Assert.Equal(50m, result); // 2 components: 2*20 + 1*10 = 50
        }

        [Fact]
        public async Task GetCostVarianceAsync_ValidAssembly_ReturnsVariance()
        {
            // Arrange
            SetupHappyPath();
            var service = CreateService();

            // Act
            var result = await service.GetCostVarianceAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.AssemblyId);
            Assert.Equal(50m, result.StandardCost);
            Assert.Equal(50m, result.ActualCost); // Same as standard for now
            Assert.Equal(0m, result.Variance); // No variance
        }
    }
}
