using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MMNextPOS.Domain.Models;

namespace MMNextPOS.Application.Services
{
    /// <summary>
    /// Service for managing application themes.
    /// </summary>
    public interface IThemeService
    {
        Task<Theme?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<Theme?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<Theme>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<IReadOnlyList<Theme>> GetActiveAsync(CancellationToken cancellationToken = default);
        Task<Theme?> GetDefaultAsync(CancellationToken cancellationToken = default);
        Task<Theme> AddAsync(Theme theme, CancellationToken cancellationToken = default);
        Task UpdateAsync(Theme theme, CancellationToken cancellationToken = default);
        Task DeleteAsync(int id, CancellationToken cancellationToken = default);
        Task<bool> SetDefaultAsync(int id, CancellationToken cancellationToken = default);
        Task ApplyThemeAsync(Theme theme);
    }
}
