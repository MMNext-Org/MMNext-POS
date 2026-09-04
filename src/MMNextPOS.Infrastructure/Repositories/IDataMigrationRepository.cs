using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MMNextPOS.Domain.Models;

namespace MMNextPOS.Infrastructure.Repositories
{
    public interface IDataMigrationRepository : IRepository<DataMigration>
    {
        Task<IReadOnlyList<DataMigration>> GetPendingMigrationsAsync(CancellationToken cancellationToken = default);
        Task<IReadOnlyList<DataMigration>> GetRunningMigrationsAsync(CancellationToken cancellationToken = default);
        Task<IReadOnlyList<DataMigration>> GetScheduledMigrationsAsync(CancellationToken cancellationToken = default);
        Task<DataMigration?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
    }
}