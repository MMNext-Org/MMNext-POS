using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MMNextPOS.Domain.Models;

namespace MMNextPOS.Application.Services
{
    public interface ISystemSettingService
    {
        Task<SystemSetting?> GetByKeyAsync(string key, CancellationToken cancellationToken = default);
        Task<SystemSetting?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<SystemSetting>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<IReadOnlyList<SystemSetting>> GetByCategoryAsync(string category, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<SystemSetting>> GetSystemSettingsAsync(CancellationToken cancellationToken = default);
        Task<SystemSetting> AddAsync(SystemSetting setting, CancellationToken cancellationToken = default);
        Task UpdateAsync(SystemSetting setting, CancellationToken cancellationToken = default);
        Task DeleteAsync(int id, CancellationToken cancellationToken = default);
    }
}