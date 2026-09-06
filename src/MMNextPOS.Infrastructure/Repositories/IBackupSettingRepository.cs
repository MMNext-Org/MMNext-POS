using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MMNextPOS.Domain.Models;

namespace MMNextPOS.Infrastructure.Repositories
{
    public interface IBackupSettingRepository : IRepository<BackupSetting>
    {
        Task<IReadOnlyList<BackupSetting>> GetActiveBackupsAsync(CancellationToken cancellationToken = default);
        Task<BackupSetting?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
    }
}
