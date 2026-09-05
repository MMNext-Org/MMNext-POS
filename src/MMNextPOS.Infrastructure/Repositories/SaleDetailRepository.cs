using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using MMNextPOS.Domain.Models;

namespace MMNextPOS.Infrastructure.Repositories
{
    public class SaleDetailRepository : GenericRepository<SaleDetail>, ISaleDetailRepository
    {
        public SaleDetailRepository(IUnitOfWork unitOfWork)
            : base(unitOfWork, "SaleDetails")
        {
        }

        public async Task<IReadOnlyList<SaleDetail>> GetBySaleIdAsync(int saleId, CancellationToken cancellationToken = default)
        {
            const string sql = @"SELECT * FROM SaleDetails WHERE SaleId = @SaleId AND IsDeleted = 0";
            var result = await Connection.QueryAsync<SaleDetail>(sql, new { SaleId = saleId }, Transaction).ConfigureAwait(false);
            return result.AsList();
        }
    }
}
