using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MMNextPOS.Domain.Models;
using MMNextPOS.Infrastructure.Repositories;

namespace MMNextPOS.Application.Services
{
    public class LanguageService : ILanguageService
    {
        private readonly ILanguageRepository _repo;

        public LanguageService(ILanguageRepository repo)
        {
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
        }

        public Task<Language> AddAsync(Language language, CancellationToken cancellationToken = default)
            => _repo.AddAsync(language, cancellationToken);

        public Task DeleteAsync(int id, CancellationToken cancellationToken = default)
            => _repo.DeleteAsync(id, cancellationToken);

        public Task<IReadOnlyList<Language>> GetAllAsync(CancellationToken cancellationToken = default)
            => _repo.GetAllAsync(cancellationToken);

        public Task<Language?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
            => _repo.GetByIdAsync(id, cancellationToken);

        public async Task<Language?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(code))
                return null;

            var languages = await _repo.GetAllAsync(cancellationToken);
            return languages.FirstOrDefault(l => string.Equals(l.Code, code, StringComparison.OrdinalIgnoreCase));
        }

        public async Task<IReadOnlyList<Language>> GetActiveAsync(CancellationToken cancellationToken = default)
        {
            var languages = await _repo.GetAllAsync(cancellationToken);
            return languages.Where(l => l.IsActive && !l.IsDeleted).OrderBy(l => l.DisplayOrder).ThenBy(l => l.Name).ToList();
        }

        public async Task<Language?> GetDefaultAsync(CancellationToken cancellationToken = default)
        {
            var languages = await _repo.GetAllAsync(cancellationToken);
            return languages.FirstOrDefault(l => l.IsDefault && l.IsActive && !l.IsDeleted);
        }

        public Task UpdateAsync(Language language, CancellationToken cancellationToken = default)
            => _repo.UpdateAsync(language, cancellationToken);

        public async Task<bool> SetDefaultAsync(int id, CancellationToken cancellationToken = default)
        {
            var languages = await _repo.GetAllAsync(cancellationToken);

            // Clear current default
            foreach (var lang in languages.Where(l => l.IsDefault))
            {
                lang.IsDefault = false;
                lang.UpdatedAt = DateTime.UtcNow;
                await _repo.UpdateAsync(lang, cancellationToken);
            }

            // Set new default
            var newDefault = languages.FirstOrDefault(l => l.Id == id);
            if (newDefault == null)
                return false;

            newDefault.IsDefault = true;
            newDefault.UpdatedAt = DateTime.UtcNow;
            await _repo.UpdateAsync(newDefault, cancellationToken);

            return true;
        }
    }
}
