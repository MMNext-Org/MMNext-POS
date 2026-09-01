using System;
using System.Threading;
using System.Threading.Tasks;
using MMNextPOS.Domain.Models;
using MMNextPOS.Infrastructure;
using MMNextPOS.Infrastructure.Repositories;
using Testcontainers.MySql;
using Xunit;

namespace MMNextPOS.Infrastructure.Tests
{
    /// <summary>
    /// Unit-of-work behavioural tests backed by a real MySQL instance
    /// spun up via Testcontainers (no hard-coded connection strings).
    /// </summary>
    public class UnitOfWorkTests : IAsyncLifetime
    {
        private MySqlContainer _container = null!;
        private string _connectionString = null!;

        public async Task InitializeAsync()
        {
            _container = new MySqlBuilder()
                .WithDatabase("mmnextpos_test")
                .WithUsername("test")
                .WithPassword("test")
                .WithImage("mysql:8.0")
                .WithCleanUp(true)
                .Build();
            await _container.StartAsync();
            _connectionString = _container.GetConnectionString();

            // Ensure schema exists
            var uow = new MySqlUnitOfWork(_connectionString);
            var initializer = new DatabaseInitializer(uow);
            await initializer.InitializeAsync();
            await uow.DisposeAsync();
        }

        public async Task DisposeAsync()
        {
            if (_container != null)
            {
                await _container.DisposeAsync();
            }
        }

        [Fact]
        public async Task CommitAsync_WhenCalledOnTransaction_CommandsSucceed()
        {
            // Arrange
            var uow = new MySqlUnitOfWork(_connectionString);
            var repo = new CustomerRepository(uow);

            await uow.BeginTransactionAsync();

            // Act – add then commit
            var cust = new Customer { Name = "UoW Test", Email = "u@example.com" };
            var added = await repo.AddAsync(cust);
            await uow.CommitAsync();

            // Cleanup
            await repo.DeleteAsync(added.Id);
            Assert.True(uow.Transaction == null); // released
        }

        [Fact]
        public async Task RollbackAsync_WhenCalledOnTransaction_DeletesAreUncommitted()
        {
            var uow = new MySqlUnitOfWork(_connectionString);
            var repo = new CustomerRepository(uow);

            await uow.BeginTransactionAsync();

            var cust = new Customer { Name = "To be rolled back", Email = "rb@example.com" };
            var added = await repo.AddAsync(cust);

            // Act – roll back
            await uow.RollbackAsync();

            var after = await repo.GetByIdAsync(added.Id);
            Assert.Null(after); // not persisted

            await uow.DisposeAsync();
        }

        [Fact]
        public async Task GetByIdAsync_ExecutedOutsideTransaction_WorksWithoutException()
        {
            var uow = new MySqlUnitOfWork(_connectionString);
            var repo = new CustomerRepository(uow);

            // Act
            var list = await repo.GetAllAsync();

            // Assert – just need no failure; returning empty list is fine
            Assert.NotNull(list);
        }
    }
}
