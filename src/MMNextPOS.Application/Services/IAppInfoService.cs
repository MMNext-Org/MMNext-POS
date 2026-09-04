using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MMNextPOS.Domain.Models;

namespace MMNextPOS.Application.Services
{
    public interface IAppInfoService
    {
        Task<AppInfo?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<AppInfo>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<AppInfo> AddAsync(AppInfo entity, CancellationToken cancellationToken = default);
        Task UpdateAsync(AppInfo entity, CancellationToken cancellationToken = default);
        Task DeleteAsync(int id, CancellationToken cancellationToken = default);
    }
}
