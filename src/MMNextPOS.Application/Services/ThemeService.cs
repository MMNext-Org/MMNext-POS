using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MMNextPOS.Domain.Models;
using MMNextPOS.Infrastructure.Repositories;

namespace MMNextPOS.Application.Services
{
    public class ThemeService : IThemeService
    {
        private readonly IThemeRepository _repo;
        private Theme? _currentTheme;

        public ThemeService(IThemeRepository repo)
        {
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
        }

        public Task<Theme> AddAsync(Theme theme, CancellationToken cancellationToken = default)
            => _repo.AddAsync(theme, cancellationToken);

        public Task DeleteAsync(int id, CancellationToken cancellationToken = default)
            => _repo.DeleteAsync(id, cancellationToken);

        public Task<IReadOnlyList<Theme>> GetAllAsync(CancellationToken cancellationToken = default)
            => _repo.GetAllAsync(cancellationToken);

        public Task<Theme?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
            => _repo.GetByIdAsync(id, cancellationToken);

        public async Task<Theme?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(code))
                return null;

            var themes = await _repo.GetAllAsync(cancellationToken);
            return themes.FirstOrDefault(t => string.Equals(t.Code, code, StringComparison.OrdinalIgnoreCase));
        }

        public async Task<IReadOnlyList<Theme>> GetActiveAsync(CancellationToken cancellationToken = default)
        {
            var themes = await _repo.GetAllAsync(cancellationToken);
            return themes.Where(t => t.IsActive && !t.IsDeleted).OrderBy(t => t.Name).ToList();
        }

        public async Task<Theme?> GetDefaultAsync(CancellationToken cancellationToken = default)
        {
            var themes = await _repo.GetAllAsync(cancellationToken);
            var defaultTheme = themes.FirstOrDefault(t => t.IsDefault && t.IsActive && !t.IsDeleted);

            // Cache the current theme
            if (defaultTheme != null)
                _currentTheme = defaultTheme;

            return defaultTheme;
        }

        public Task UpdateAsync(Theme theme, CancellationToken cancellationToken = default)
            => _repo.UpdateAsync(theme, cancellationToken);

        public async Task<bool> SetDefaultAsync(int id, CancellationToken cancellationToken = default)
        {
            var themes = await _repo.GetAllAsync(cancellationToken);

            // Clear current default
            foreach (var theme in themes.Where(t => t.IsDefault))
            {
                theme.IsDefault = false;
                theme.UpdatedAt = DateTime.UtcNow;
                await _repo.UpdateAsync(theme, cancellationToken);
            }

            // Set new default
            var newDefault = themes.FirstOrDefault(t => t.Id == id);
            if (newDefault == null)
                return false;

            newDefault.IsDefault = true;
            newDefault.UpdatedAt = DateTime.UtcNow;
            await _repo.UpdateAsync(newDefault, cancellationToken);

            return true;
        }

        public Task ApplyThemeAsync(Theme theme)
        {
            // This is implemented in the WinForms-specific ThemeApplier
            _currentTheme = theme;
            return Task.CompletedTask;
        }

        public Theme? GetCurrentTheme() => _currentTheme;
    }
}
