using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using MMNextPOS.Domain.Models;
using MMNextPOS.Infrastructure.Repositories;

namespace MMNextPOS.Infrastructure.Repositories
{
    public class DataMigrationRepository : GenericRepository<DataMigration>, IDataMigrationRepository
    {
        public DataMigrationRepository(IUnitOfWork unitOfWork)
            : base(unitOfWork, "DataMigrations")
        {
        }

        public async Task<IReadOnlyList<DataMigration>> GetPendingMigrationsAsync(CancellationToken cancellationToken = default)
        {
            const string sql = "SELECT * FROM DataMigrations WHERE Status = 'Pending' AND IsDeleted = 0 ORDER BY CreatedAt";
            var result = await Connection.QueryAsync<DataMigration>(sql, transaction: Transaction).ConfigureAwait(false);
            return result.AsList();
        }

        public async Task<IReadOnlyList<DataMigration>> GetRunningMigrationsAsync(CancellationToken cancellationToken = default)
        {
            const string sql = "SELECT * FROM DataMigrations WHERE Status = 'Running' AND IsDeleted = 0 ORDER BY StartedAt";
            var result = await Connection.QueryAsync<DataMigration>(sql, transaction: Transaction).ConfigureAwait(false);
            return result.AsList();
        }

        public async Task<IReadOnlyList<DataMigration>> GetScheduledMigrationsAsync(CancellationToken cancellationToken = default)
        {
            const string sql = "SELECT * FROM DataMigrations WHERE IsScheduled = 1 AND Status = 'Pending' AND IsDeleted = 0 ORDER BY ScheduledAt";
            var result = await Connection.QueryAsync<DataMigration>(sql, transaction: Transaction).ConfigureAwait(false);
            return result.AsList();
        }

        public async Task<DataMigration?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
        {
            const string sql = "SELECT * FROM DataMigrations WHERE Name = @Name AND IsDeleted = 0 LIMIT 1";
            return await Connection.QuerySingleOrDefaultAsync<DataMigration>(sql, new { Name = name }, Transaction).ConfigureAwait(false);
        }
    }
}
