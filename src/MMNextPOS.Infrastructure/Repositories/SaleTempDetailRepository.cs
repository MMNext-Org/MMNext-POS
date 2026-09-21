using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using MySqlConnector;
using MMNextPOS.Domain.Models;

namespace MMNextPOS.Infrastructure.Repositories
{
    /// <summary>
    /// Repository for SaleTempDetail entities.
    /// </summary>
    public class SaleTempDetailRepository : GenericRepository<SaleTempDetail>, ISaleTempDetailRepository
    {
        public SaleTempDetailRepository(IUnitOfWork unitOfWork)
            : base(unitOfWork, "SaleTempDetails")
        {
        }

        /// <inheritdoc />
        public async Task<IReadOnlyList<SaleTempDetail>> GetBySaleTempIdAsync(int saleTempId, CancellationToken cancellationToken = default)
        {
            const string sql = @"SELECT * FROM SaleTempDetails WHERE SaleTempId = @SaleTempId AND IsDeleted = 0";
            var result = await Connection.QueryAsync<SaleTempDetail>(sql, new { SaleTempId = saleTempId }, Transaction).ConfigureAwait(false);
            return result.AsList();
        }

        /// <inheritdoc />
        public async Task DeleteBySaleTempIdAsync(int saleTempId, CancellationToken cancellationToken = default)
        {
            // Soft delete all details for a SaleTemp
            var sql = @"UPDATE SaleTempDetails SET IsDeleted = 1 WHERE SaleTempId = @SaleTempId";
            await Connection.ExecuteAsync(sql, new { SaleTempId = saleTempId }, Transaction).ConfigureAwait(false);
        }
    }
}
