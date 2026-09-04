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

        public async Task<IReadOnlyList<BackupSetting>> GetActiveBackupsAsync(CancellationToken cancellationToken = default)
        {
            const string sql = "SELECT * FROM BackupSettings WHERE IsActive = 1 AND IsDeleted = 0 ORDER BY NextRunAt";
            var result = await Connection.QueryAsync<BackupSetting>(sql, transaction: Transaction).ConfigureAwait(false);
            return result.AsList();
        }

        public async Task<BackupSetting?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
        {
            const string sql = "SELECT * FROM BackupSettings WHERE Name = @Name AND IsDeleted = 0 LIMIT 1";
            return await Connection.QuerySingleOrDefaultAsync<BackupSetting>(sql, new { Name = name }, Transaction).ConfigureAwait(false);
        }
    }
}