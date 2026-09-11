using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using MySqlConnector;
using MMNextPOS.Domain.Models;

namespace MMNextPOS.Infrastructure.Repositories
{
    public class SalesReturnDetailRepository : GenericRepository<SalesReturnDetail>, ISalesReturnDetailRepository
    {
        public SalesReturnDetailRepository(IUnitOfWork unitOfWork)
            : base(unitOfWork, "SalesReturnDetails")
        {
        }

        /// <inheritdoc />
        public async Task<IReadOnlyList<SalesReturnDetail>> GetBySaleDetailIdAsync(int saleDetailId, CancellationToken cancellationToken = default)
        {
            const string sql = @"SELECT * FROM SalesReturnDetails WHERE SaleDetailId = @SaleDetailId AND IsDeleted = 0";
            var result = await Connection.QueryAsync<SalesReturnDetail>(sql, new { SaleDetailId = saleDetailId }, Transaction).ConfigureAwait(false);
            return result.AsList();
        }
    }
}
