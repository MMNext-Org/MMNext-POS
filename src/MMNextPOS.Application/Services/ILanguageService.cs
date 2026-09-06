using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MMNextPOS.Domain.Models;

namespace MMNextPOS.Application.Services
{
    /// <summary>
    /// Service for managing application languages.
    /// </summary>
    public interface ILanguageService
    {
        Task<Language?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<Language?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<Language>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<IReadOnlyList<Language>> GetActiveAsync(CancellationToken cancellationToken = default);
        Task<Language?> GetDefaultAsync(CancellationToken cancellationToken = default);
        Task<Language> AddAsync(Language language, CancellationToken cancellationToken = default);
        Task UpdateAsync(Language language, CancellationToken cancellationToken = default);
        Task DeleteAsync(int id, CancellationToken cancellationToken = default);
        Task<bool> SetDefaultAsync(int id, CancellationToken cancellationToken = default);
    }
}
