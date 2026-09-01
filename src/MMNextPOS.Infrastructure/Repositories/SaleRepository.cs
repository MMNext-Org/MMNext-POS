using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using MySqlConnector;
using MMNextPOS.Domain.Models;

namespace MMNextPOS.Infrastructure.Repositories
{
    public class SaleRepository : RepositoryBase, ISaleRepository
    {
        public SaleRepository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public async Task<Sale> CreateAsync(Sale sale, CancellationToken cancellationToken = default)
        {
            const string sql = @"INSERT INTO Sales (CustomerId, SaleDate, TotalAmount) VALUES (@CustomerId, @SaleDate, @TotalAmount);
                                 SELECT LAST_INSERT_ID();";
            var id = await Connection.ExecuteScalarAsync<long>(sql, sale, Transaction).ConfigureAwait(false);
            sale.Id = (int)id;
            return sale;
        }

        /// <summary>
        /// Creates a sale header and its details in a single transaction.
        /// Caller must have called <see cref="IUnitOfWork.BeginTransactionAsync"/> before calling this.
        /// </summary>
        public async Task<Sale> CreateSaleWithDetailsAsync(Sale sale, IEnumerable<SaleDetail> details, CancellationToken cancellationToken = default)
        {
            if (Transaction == null)
            {
                throw new InvalidOperationException("No active transaction. Call IUnitOfWork.BeginTransactionAsync first.");
            }

            const string saleSql = @"INSERT INTO Sales (CustomerId, SaleDate, TotalAmount) VALUES (@CustomerId, @SaleDate, @TotalAmount);
                                     SELECT LAST_INSERT_ID();";
            var saleId = await Connection.ExecuteScalarAsync<long>(saleSql, sale, Transaction).ConfigureAwait(false);
            sale.Id = (int)saleId;

            const string detailSql = @"INSERT INTO SaleDetails (SaleId, ProductId, Quantity, UnitPrice) VALUES (@SaleId, @ProductId, @Quantity, @UnitPrice);
                                       SELECT LAST_INSERT_ID();";

            foreach (var detail in details)
            {
                detail.SaleId = sale.Id;
                var detailId = await Connection.ExecuteScalarAsync<long>(detailSql, detail, Transaction).ConfigureAwait(false);
                detail.Id = (int)detailId;
            }

            return sale;
        }

        public async Task AddDetailAsync(SaleDetail detail, CancellationToken cancellationToken = default)
        {
            const string sql = @"INSERT INTO SaleDetails (SaleId, ProductId, Quantity, UnitPrice) VALUES (@SaleId, @ProductId, @Quantity, @UnitPrice);
                                 SELECT LAST_INSERT_ID();";
            var id = await Connection.ExecuteScalarAsync<long>(sql, detail, Transaction).ConfigureAwait(false);
            detail.Id = (int)id;
        }

        public async Task<IReadOnlyList<Sale>> GetRecentAsync(int count = 20, CancellationToken cancellationToken = default)
        {
            const string sql = @"SELECT s.*, c.Name AS CustomerName 
                                FROM Sales s 
                                LEFT JOIN Customers c ON s.CustomerId = c.Id 
                                ORDER BY s.SaleDate DESC 
                                LIMIT @Count";
            var result = await Connection.QueryAsync<Sale>(sql, new { Count = count }, Transaction).ConfigureAwait(false);
            return result.AsList();
        }
    }
}
