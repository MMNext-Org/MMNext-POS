using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using MMNextPOS.Domain.Models;
using MMNextPOS.Infrastructure.Repositories;

namespace MMNextPOS.Infrastructure.Repositories
{
    public class SystemSettingRepository : GenericRepository<SystemSetting>, ISystemSettingRepository
    {
        public SystemSettingRepository(IUnitOfWork unitOfWork)
            : base(unitOfWork, "SystemSettings")
        {
        }

        public async Task<SystemSetting?> GetByKeyAsync(string key, CancellationToken cancellationToken = default)
        {
            const string sql = "SELECT * FROM SystemSettings WHERE `Key` = @Key AND IsDeleted = 0 LIMIT 1";
            return await Connection.QuerySingleOrDefaultAsync<SystemSetting>(sql, new { Key = key }, Transaction).ConfigureAwait(false);
        }

        public async Task<IReadOnlyList<SystemSetting>> GetByCategoryAsync(string category, CancellationToken cancellationToken = default)
        {
            const string sql = "SELECT * FROM SystemSettings WHERE Category = @Category AND IsDeleted = 0 ORDER BY `Key`";
            var result = await Connection.QueryAsync<SystemSetting>(sql, new { Category = category }, Transaction).ConfigureAwait(false);
            return result.AsList();
        }

        public async Task<IReadOnlyList<SystemSetting>> GetSystemSettingsAsync(CancellationToken cancellationToken = default)
        {
            const string sql = "SELECT * FROM SystemSettings WHERE IsSystem = 1 AND IsDeleted = 0 ORDER BY Category, `Key`";
            var result = await Connection.QueryAsync<SystemSetting>(sql, transaction: Transaction).ConfigureAwait(false);
            return result.AsList();
        }
    }
}
