using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MMNextPOS.Domain.Models;

namespace MMNextPOS.Infrastructure.Repositories
{
    public interface ISystemSettingRepository : IRepository<SystemSetting>
    {
        Task<SystemSetting?> GetByKeyAsync(string key, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<SystemSetting>> GetByCategoryAsync(string category, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<SystemSetting>> GetSystemSettingsAsync(CancellationToken cancellationToken = default);
    }
}