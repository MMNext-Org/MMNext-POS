using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using MMNextPOS.Domain.Models;
using MMNextPOS.Infrastructure.Repositories;

namespace MMNextPOS.Infrastructure.Repositories
{
    public class BackupSettingRepository : GenericRepository<BackupSetting>, IBackupSettingRepository
    {
        public BackupSettingRepository(IUnitOfWork unitOfWork)
            : base(unitOfWork, "BackupSettings")
        {
        }

        public async Task<IReadOnlyList<BackupResult>> GetBackupHistoryAsync(int backupSettingId, int limit = 50, CancellationToken cancellationToken = default)
        {
            // Placeholder implementation: In a real application, this would query a dedicated backup history table.
            const string sql = "SELECT BackupSettingId, StartedAt, CompletedAt, Status, BackupSizeBytes, FilePath, ErrorMessage FROM BackupHistory WHERE BackupSettingId = @BackupSettingId ORDER BY StartedAt DESC LIMIT @Limit";
            // Assuming a BackupHistory table exists for this example
            return (await Connection.QueryAsync<BackupResult>(sql, new { BackupSettingId = backupSettingId, Limit = limit })).ToList();
        }

        public async Task<IReadOnlyList<BackupSetting>> GetActiveBackupsAsync(CancellationToken cancellationToken = default)
        {
            const string sql = "SELECT * FROM BackupSettings WHERE IsDeleted = 0 AND IsActive = 1";
            return (await Connection.QueryAsync<BackupSetting>(sql, Transaction).ConfigureAwait(false)).ToList();
        }

        public async Task<BackupSetting?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
        {
            const string sql = "SELECT * FROM BackupSettings WHERE Name = @Name AND IsDeleted = 0 LIMIT 1";
            return await Connection.QuerySingleOrDefaultAsync<BackupSetting>(sql, new { Name = name }, Transaction).ConfigureAwait(false);
        }
    }
}
