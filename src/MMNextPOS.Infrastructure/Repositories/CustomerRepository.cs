using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using MySqlConnector;
using MMNextPOS.Domain.Models;

namespace MMNextPOS.Infrastructure.Repositories
{
    public class CustomerRepository : RepositoryBase, ICustomerRepository
    {
        public CustomerRepository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public async Task<Customer> AddAsync(Customer customer, CancellationToken cancellationToken = default)
        {
            const string sql = @"INSERT INTO Customers (Name, Address, Phone, Email) VALUES (@Name, @Address, @Phone, @Email);
                                 SELECT LAST_INSERT_ID();";
            var id = await Connection.ExecuteScalarAsync<long>(sql, customer, Transaction).ConfigureAwait(false);
            customer.Id = (int)id;
            return customer;
        }

        public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            const string sql = "DELETE FROM Customers WHERE Id = @Id";
            await Connection.ExecuteAsync(sql, new { Id = id }, Transaction).ConfigureAwait(false);
        }

        public async Task<IReadOnlyList<Customer>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            const string sql = "SELECT * FROM Customers";
            var result = await Connection.QueryAsync<Customer>(sql, transaction: Transaction).ConfigureAwait(false);
            return result.AsList();
        }

        public async Task<Customer?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            const string sql = "SELECT * FROM Customers WHERE Id = @Id";
            return await Connection.QuerySingleOrDefaultAsync<Customer>(sql, new { Id = id }, Transaction).ConfigureAwait(false);
        }

        public async Task UpdateAsync(Customer customer, CancellationToken cancellationToken = default)
        {
            const string sql = "UPDATE Customers SET Name = @Name, Address = @Address, Phone = @Phone, Email = @Email WHERE Id = @Id";
            await Connection.ExecuteAsync(sql, customer, Transaction).ConfigureAwait(false);
        }
    }
}
