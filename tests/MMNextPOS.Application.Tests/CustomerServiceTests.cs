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
    public class CustomerServiceTests
    {
        private readonly Mock<ICustomerRepository> _customerRepoMock = new();

        private ICustomerService CreateService()
        {
            return new CustomerService(_customerRepoMock.Object);
        }

        [Fact]
        public async Task GetByIdAsync_ExistingCustomer_ReturnsCustomer()
        {
            // Arrange
            var customer = new Customer { Id = 1, Name = "John Doe", Address = "123 Main St", Phone = "555-1234", Email = "john@example.com" };
            _customerRepoMock.Setup(r => r.GetByIdAsync(customer.Id, It.IsAny<CancellationToken>()))
                            .ReturnsAsync(customer);

            var service = CreateService();

            // Act
            var result = await service.GetByIdAsync(customer.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(customer.Id, result.Id);
            Assert.Equal(customer.Name, result.Name);
            Assert.Equal(customer.Address, result.Address);
            Assert.Equal(customer.Phone, result.Phone);
            Assert.Equal(customer.Email, result.Email);
        }

        [Fact]
        public async Task GetByIdAsync_NonExistentCustomer_ReturnsNull()
        {
            // Arrange
            _customerRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                            .ReturnsAsync((Customer?)null!);

            var service = CreateService();

            // Act
            var result = await service.GetByIdAsync(999);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetAllAsync_ReturnsAllCustomers()
        {
            // Arrange
            var customers = new List<Customer>
            {
                new() { Id = 1, Name = "Alice Smith", Email = "alice@example.com" },
                new() { Id = 2, Name = "Bob Johnson", Email = "bob@example.com" },
                new() { Id = 3, Name = "Carol Williams", Email = "carol@example.com" }
            };
            _customerRepoMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                            .ReturnsAsync(customers);

            var service = CreateService();

            // Act
            var result = await service.GetAllAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.Count);
        }

        [Fact]
        public async Task AddAsync_ValidCustomer_ReturnsAddedCustomer()
        {
            // Arrange
            var customer = new Customer { Name = "New Customer", Email = "new@example.com" };
            var addedCustomer = new Customer { Id = 1, Name = "New Customer", Email = "new@example.com" };
            _customerRepoMock.Setup(r => r.AddAsync(customer, It.IsAny<CancellationToken>()))
                            .ReturnsAsync(addedCustomer);

            var service = CreateService();

            // Act
            var result = await service.AddAsync(customer);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(addedCustomer.Id, result.Id);
            Assert.Equal(customer.Name, result.Name);
            _customerRepoMock.Verify(r => r.AddAsync(customer, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_ExistingCustomer_UpdatesCustomer()
        {
            // Arrange
            var existingCustomer = new Customer { Id = 1, Name = "Old Name", Email = "old@example.com" };
            var updatedCustomer = new Customer { Id = 1, Name = "New Name", Email = "new@example.com" };
            _customerRepoMock.Setup(r => r.UpdateAsync(existingCustomer, It.IsAny<CancellationToken>()))
                            .Returns(Task.CompletedTask);

            var service = CreateService();

            // Act
            await service.UpdateAsync(updatedCustomer);

            // Assert
            _customerRepoMock.Verify(r => r.UpdateAsync(It.Is<Customer>(c => c.Name == "New Name" && c.Email == "new@example.com"), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_NonExistentCustomer_StillCallsUpdate()
        {
            // Arrange
            var nonExistentCustomer = new Customer { Id = 999, Name = "Does Not Exist", Email = "nonexistent@example.com" };
            _customerRepoMock.Setup(r => r.UpdateAsync(nonExistentCustomer, It.IsAny<CancellationToken>()))
                            .Returns(Task.CompletedTask);

            var service = CreateService();

            // Act
            await service.UpdateAsync(nonExistentCustomer);

            // Assert
            _customerRepoMock.Verify(r => r.UpdateAsync(nonExistentCustomer, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_ExistingCustomer_DeletesCustomer()
        {
            // Arrange
            var customer = new Customer { Id = 1, Name = "Customer To Delete", Email = "delete@example.com" };
            _customerRepoMock.Setup(r => r.DeleteAsync(customer.Id, It.IsAny<CancellationToken>()))
                            .Returns(Task.CompletedTask);

            var service = CreateService();

            // Act
            await service.DeleteAsync(customer.Id);

            // Assert
            _customerRepoMock.Verify(r => r.DeleteAsync(customer.Id, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_NonExistentCustomer_StillCallsDelete()
        {
            // Arrange
            _customerRepoMock.Setup(r => r.DeleteAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                            .Returns(Task.CompletedTask);

            var service = CreateService();

            // Act
            await service.DeleteAsync(999);

            // Assert
            _customerRepoMock.Verify(r => r.DeleteAsync(999, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetByIdAsync_ThrowsArgumentException_WhenRepositoryThrows()
        {
            // Arrange
            var ex = new InvalidOperationException("DB error");
            _customerRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                            .ThrowsAsync(ex);

            var service = CreateService();

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => service.GetByIdAsync(1));
        }
    }
}
