using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using MMNextPOS.Domain.Models;
using MMNextPOS.Infrastructure.Repositories;

namespace MMNextPOS.Infrastructure.Repositories
{
    public class SuperAdminLogRepository : GenericRepository<SuperAdminLog>, ISuperAdminLogRepository
    {
        public SuperAdminLogRepository(IUnitOfWork unitOfWork)
            : base(unitOfWork, "SuperAdminLogs")
        {
        }

        public async Task<IReadOnlyList<SuperAdminLog>> GetByUserAsync(int userId, DateTime? fromDate = null, DateTime? toDate = null, CancellationToken cancellationToken = default)
        {
            var sql = "SELECT * FROM SuperAdminLogs WHERE PerformedByUserId = @UserId AND IsDeleted = 0";
            var queryParams = new { UserId = userId };

            if (fromDate.HasValue)
            {
                sql += " AND CreatedAt >= @FromDate";
            }
            if (toDate.HasValue)
            {
                sql += " AND CreatedAt <= @ToDate";
            }

            sql += " ORDER BY CreatedAt DESC";

            var parameters = new DynamicParameters();
            parameters.Add("UserId", userId);
            if (fromDate.HasValue) parameters.Add("FromDate", fromDate.Value);
            if (toDate.HasValue) parameters.Add("ToDate", toDate.Value);

            var result = await Connection.QueryAsync<SuperAdminLog>(sql, parameters, Transaction).ConfigureAwait(false);
            return result.AsList();
        }

        public async Task<IReadOnlyList<SuperAdminLog>> GetByModuleAsync(string module, DateTime? fromDate = null, DateTime? toDate = null, CancellationToken cancellationToken = default)
        {
            var sql = "SELECT * FROM SuperAdminLogs WHERE Module = @Module AND IsDeleted = 0";
            var parameters = new DynamicParameters();
            parameters.Add("Module", module);

            if (fromDate.HasValue)
            {
                sql += " AND CreatedAt >= @FromDate";
            }
            if (toDate.HasValue)
            {
                sql += " AND CreatedAt <= @ToDate";
            }

            sql += " ORDER BY CreatedAt DESC";

            if (fromDate.HasValue) parameters.Add("FromDate", fromDate.Value);
            if (toDate.HasValue) parameters.Add("ToDate", toDate.Value);

            var result = await Connection.QueryAsync<SuperAdminLog>(sql, parameters, Transaction).ConfigureAwait(false);
            return result.AsList();
        }

        public async Task<IReadOnlyList<SuperAdminLog>> GetByActionAsync(string action, DateTime? fromDate = null, DateTime? toDate = null, CancellationToken cancellationToken = default)
        {
            var sql = "SELECT * FROM SuperAdminLogs WHERE Action = @Action AND IsDeleted = 0";
            var parameters = new DynamicParameters();
            parameters.Add("Action", action);

            if (fromDate.HasValue)
            {
                sql += " AND CreatedAt >= @FromDate";
            }
            if (toDate.HasValue)
            {
                sql += " AND CreatedAt <= @ToDate";
            }

            sql += " ORDER BY CreatedAt DESC";

            if (fromDate.HasValue) parameters.Add("FromDate", fromDate.Value);
            if (toDate.HasValue) parameters.Add("ToDate", toDate.Value);

            var result = await Connection.QueryAsync<SuperAdminLog>(sql, parameters, Transaction).ConfigureAwait(false);
            return result.AsList();
        }

        public async Task<IReadOnlyList<SuperAdminLog>> GetByDateRangeAsync(DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default)
        {
            const string sql = "SELECT * FROM SuperAdminLogs WHERE CreatedAt >= @FromDate AND CreatedAt <= @ToDate AND IsDeleted = 0 ORDER BY CreatedAt DESC";
            var result = await Connection.QueryAsync<SuperAdminLog>(sql, new { FromDate = fromDate, ToDate = toDate }, Transaction).ConfigureAwait(false);
            return result.AsList();
        }
    }
}
