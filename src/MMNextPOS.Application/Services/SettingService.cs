using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MMNextPOS.Application.Services;
using MMNextPOS.Domain.Models;
using MMNextPOS.Infrastructure.Repositories;

namespace MMNextPOS.Application.Services
{
    public class SettingService : ISettingService
    {
        private readonly ICategoryRepository _categoryRepo;
        private readonly IUnitRepository _unitRepo;
        private readonly ICurrencyRepository _currencyRepo;
        private readonly ITaxRepository _taxRepo;
        private readonly IDiscountRepository _discountRepo;
        private readonly IThemeRepository _themeRepo;
        private readonly ILanguageRepository _languageRepo;
        private readonly IAuditService _auditService;

        public SettingService(
            ICategoryRepository categoryRepo,
            IUnitRepository unitRepo,
            ICurrencyRepository currencyRepo,
            ITaxRepository taxRepo,
            IDiscountRepository discountRepo,
            IThemeRepository themeRepo,
            ILanguageRepository languageRepo,
            IAuditService auditService)
        {
            _categoryRepo = categoryRepo ?? throw new ArgumentNullException(nameof(categoryRepo));
            _unitRepo = unitRepo ?? throw new ArgumentNullException(nameof(unitRepo));
            _currencyRepo = currencyRepo ?? throw new ArgumentNullException(nameof(currencyRepo));
            _taxRepo = taxRepo ?? throw new ArgumentNullException(nameof(taxRepo));
            _discountRepo = discountRepo ?? throw new ArgumentNullException(nameof(discountRepo));
            _themeRepo = themeRepo ?? throw new ArgumentNullException(nameof(themeRepo));
            _languageRepo = languageRepo ?? throw new ArgumentNullException(nameof(languageRepo));
            _auditService = auditService ?? throw new ArgumentNullException(nameof(auditService));
        }

        // Category
        public Task<Category?> GetCategoryByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return _categoryRepo.GetByIdAsync(id, cancellationToken);
        }

        public Task<IReadOnlyList<Category>> GetCategoriesAsync(CancellationToken cancellationToken = default)
        {
            return _categoryRepo.GetAllAsync(cancellationToken);
        }

        public async Task<Category> AddCategoryAsync(Category category, CancellationToken cancellationToken = default)
        {
            var result = await _categoryRepo.AddAsync(category, cancellationToken).ConfigureAwait(false);
            await _auditService.LogAsync(nameof(Category), result.Id, "Create", null, result, 1, "System", $"Created category {result.Code} - {result.Name}", cancellationToken).ConfigureAwait(false);
            return result;
        }

        public async Task UpdateCategoryAsync(Category category, CancellationToken cancellationToken = default)
        {
            var existing = await _categoryRepo.GetByIdAsync(category.Id, cancellationToken).ConfigureAwait(false);
            await _categoryRepo.UpdateAsync(category, cancellationToken).ConfigureAwait(false);
            await _auditService.LogAsync(nameof(Category), category.Id, "Update", existing, category, 1, "System", $"Updated category {category.Code} - {category.Name}", cancellationToken).ConfigureAwait(false);
        }

        public async Task DeleteCategoryAsync(int id, CancellationToken cancellationToken = default)
        {
            var existing = await _categoryRepo.GetByIdAsync(id, cancellationToken).ConfigureAwait(false);
            await _categoryRepo.DeleteAsync(id, cancellationToken).ConfigureAwait(false);
            await _auditService.LogAsync(nameof(Category), id, "Delete", existing, null, 1, "System", $"Deleted category {existing?.Code ?? id.ToString()}", cancellationToken).ConfigureAwait(false);
        }

        // Unit
        public Task<Unit?> GetUnitByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return _unitRepo.GetByIdAsync(id, cancellationToken);
        }

        public Task<IReadOnlyList<Unit>> GetUnitsAsync(CancellationToken cancellationToken = default)
        {
            return _unitRepo.GetAllAsync(cancellationToken);
        }

        public async Task<Unit> AddUnitAsync(Unit unit, CancellationToken cancellationToken = default)
        {
            var result = await _unitRepo.AddAsync(unit, cancellationToken).ConfigureAwait(false);
            await _auditService.LogAsync(nameof(Unit), result.Id, "Create", null, result, 1, "System", $"Created unit {result.Code} - {result.Name}", cancellationToken).ConfigureAwait(false);
            return result;
        }

        public async Task UpdateUnitAsync(Unit unit, CancellationToken cancellationToken = default)
        {
            var existing = await _unitRepo.GetByIdAsync(unit.Id, cancellationToken).ConfigureAwait(false);
            await _unitRepo.UpdateAsync(unit, cancellationToken).ConfigureAwait(false);
            await _auditService.LogAsync(nameof(Unit), unit.Id, "Update", existing, unit, 1, "System", $"Updated unit {unit.Code} - {unit.Name}", cancellationToken).ConfigureAwait(false);
        }

        public async Task DeleteUnitAsync(int id, CancellationToken cancellationToken = default)
        {
            var existing = await _unitRepo.GetByIdAsync(id, cancellationToken).ConfigureAwait(false);
            await _unitRepo.DeleteAsync(id, cancellationToken).ConfigureAwait(false);
            await _auditService.LogAsync(nameof(Unit), id, "Delete", existing, null, 1, "System", $"Deleted unit {existing?.Code ?? id.ToString()}", cancellationToken).ConfigureAwait(false);
        }

        // Currency
        public Task<Currency?> GetCurrencyByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return _currencyRepo.GetByIdAsync(id, cancellationToken);
        }

        public Task<IReadOnlyList<Currency>> GetCurrenciesAsync(CancellationToken cancellationToken = default)
        {
            return _currencyRepo.GetAllAsync(cancellationToken);
        }

        public async Task<Currency> AddCurrencyAsync(Currency currency, CancellationToken cancellationToken = default)
        {
            var result = await _currencyRepo.AddAsync(currency, cancellationToken).ConfigureAwait(false);
            await _auditService.LogAsync(nameof(Currency), result.Id, "Create", null, result, 1, "System", $"Created currency {result.Code} - {result.Name}", cancellationToken).ConfigureAwait(false);
            return result;
        }

        public async Task UpdateCurrencyAsync(Currency currency, CancellationToken cancellationToken = default)
        {
            var existing = await _currencyRepo.GetByIdAsync(currency.Id, cancellationToken).ConfigureAwait(false);
            await _currencyRepo.UpdateAsync(currency, cancellationToken).ConfigureAwait(false);
            await _auditService.LogAsync(nameof(Currency), currency.Id, "Update", existing, currency, 1, "System", $"Updated currency {currency.Code} - {currency.Name}", cancellationToken).ConfigureAwait(false);
        }

        public async Task DeleteCurrencyAsync(int id, CancellationToken cancellationToken = default)
        {
            var existing = await _currencyRepo.GetByIdAsync(id, cancellationToken).ConfigureAwait(false);
            await _currencyRepo.DeleteAsync(id, cancellationToken).ConfigureAwait(false);
            await _auditService.LogAsync(nameof(Currency), id, "Delete", existing, null, 1, "System", $"Deleted currency {existing?.Code ?? id.ToString()}", cancellationToken).ConfigureAwait(false);
        }

        // Tax
        public Task<Tax?> GetTaxByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return _taxRepo.GetByIdAsync(id, cancellationToken);
        }

        public Task<IReadOnlyList<Tax>> GetTaxesAsync(CancellationToken cancellationToken = default)
        {
            return _taxRepo.GetAllAsync(cancellationToken);
        }

        public async Task<Tax> AddTaxAsync(Tax tax, CancellationToken cancellationToken = default)
        {
            var result = await _taxRepo.AddAsync(tax, cancellationToken).ConfigureAwait(false);
            await _auditService.LogAsync(nameof(Tax), result.Id, "Create", null, result, 1, "System", $"Created tax {result.Code} - {result.Name} ({result.Rate}%)", cancellationToken).ConfigureAwait(false);
            return result;
        }

        public async Task UpdateTaxAsync(Tax tax, CancellationToken cancellationToken = default)
        {
            var existing = await _taxRepo.GetByIdAsync(tax.Id, cancellationToken).ConfigureAwait(false);
            await _taxRepo.UpdateAsync(tax, cancellationToken).ConfigureAwait(false);
            await _auditService.LogAsync(nameof(Tax), tax.Id, "Update", existing, tax, 1, "System", $"Updated tax {tax.Code} - {tax.Name}", cancellationToken).ConfigureAwait(false);
        }

        public async Task DeleteTaxAsync(int id, CancellationToken cancellationToken = default)
        {
            var existing = await _taxRepo.GetByIdAsync(id, cancellationToken).ConfigureAwait(false);
            await _taxRepo.DeleteAsync(id, cancellationToken).ConfigureAwait(false);
            await _auditService.LogAsync(nameof(Tax), id, "Delete", existing, null, 1, "System", $"Deleted tax {existing?.Code ?? id.ToString()}", cancellationToken).ConfigureAwait(false);
        }

        // Discount
        public Task<Discount?> GetDiscountByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return _discountRepo.GetByIdAsync(id, cancellationToken);
        }

        public Task<IReadOnlyList<Discount>> GetDiscountsAsync(CancellationToken cancellationToken = default)
        {
            return _discountRepo.GetAllAsync(cancellationToken);
        }

        public async Task<Discount> AddDiscountAsync(Discount discount, CancellationToken cancellationToken = default)
        {
            var result = await _discountRepo.AddAsync(discount, cancellationToken).ConfigureAwait(false);
            await _auditService.LogAsync(nameof(Discount), result.Id, "Create", null, result, 1, "System", $"Created discount {result.Code} - {result.Name} ({result.Rate}%)", cancellationToken).ConfigureAwait(false);
            return result;
        }

        public async Task UpdateDiscountAsync(Discount discount, CancellationToken cancellationToken = default)
        {
            var existing = await _discountRepo.GetByIdAsync(discount.Id, cancellationToken).ConfigureAwait(false);
            await _discountRepo.UpdateAsync(discount, cancellationToken).ConfigureAwait(false);
            await _auditService.LogAsync(nameof(Discount), discount.Id, "Update", existing, discount, 1, "System", $"Updated discount {discount.Code} - {discount.Name}", cancellationToken).ConfigureAwait(false);
        }

        public async Task DeleteDiscountAsync(int id, CancellationToken cancellationToken = default)
        {
            var existing = await _discountRepo.GetByIdAsync(id, cancellationToken).ConfigureAwait(false);
            await _discountRepo.DeleteAsync(id, cancellationToken).ConfigureAwait(false);
            await _auditService.LogAsync(nameof(Discount), id, "Delete", existing, null, 1, "System", $"Deleted discount {existing?.Code ?? id.ToString()}", cancellationToken).ConfigureAwait(false);
        }

        // Theme
        public Task<Theme?> GetThemeByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return _themeRepo.GetByIdAsync(id, cancellationToken);
        }

        public Task<IReadOnlyList<Theme>> GetThemesAsync(CancellationToken cancellationToken = default)
        {
            return _themeRepo.GetAllAsync(cancellationToken);
        }

        public async Task<Theme> AddThemeAsync(Theme theme, CancellationToken cancellationToken = default)
        {
            var result = await _themeRepo.AddAsync(theme, cancellationToken).ConfigureAwait(false);
            await _auditService.LogAsync(nameof(Theme), result.Id, "Create", null, result, 1, "System", $"Created theme {result.Code} - {result.Name}", cancellationToken).ConfigureAwait(false);
            return result;
        }

        public async Task UpdateThemeAsync(Theme theme, CancellationToken cancellationToken = default)
        {
            var existing = await _themeRepo.GetByIdAsync(theme.Id, cancellationToken).ConfigureAwait(false);
            await _themeRepo.UpdateAsync(theme, cancellationToken).ConfigureAwait(false);
            await _auditService.LogAsync(nameof(Theme), theme.Id, "Update", existing, theme, 1, "System", $"Updated theme {theme.Code} - {theme.Name}", cancellationToken).ConfigureAwait(false);
        }

        public async Task DeleteThemeAsync(int id, CancellationToken cancellationToken = default)
        {
            var existing = await _themeRepo.GetByIdAsync(id, cancellationToken).ConfigureAwait(false);
            await _themeRepo.DeleteAsync(id, cancellationToken).ConfigureAwait(false);
            await _auditService.LogAsync(nameof(Theme), id, "Delete", existing, null, 1, "System", $"Deleted theme {existing?.Code ?? id.ToString()}", cancellationToken).ConfigureAwait(false);
        }

        // Language
        public Task<Language?> GetLanguageByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return _languageRepo.GetByIdAsync(id, cancellationToken);
        }

        public Task<IReadOnlyList<Language>> GetLanguagesAsync(CancellationToken cancellationToken = default)
        {
            return _languageRepo.GetAllAsync(cancellationToken);
        }

        public async Task<Language> AddLanguageAsync(Language language, CancellationToken cancellationToken = default)
        {
            var result = await _languageRepo.AddAsync(language, cancellationToken).ConfigureAwait(false);
            await _auditService.LogAsync(nameof(Language), result.Id, "Create", null, result, 1, "System", $"Created language {result.Code} - {result.Name}", cancellationToken).ConfigureAwait(false);
            return result;
        }

        public async Task UpdateLanguageAsync(Language language, CancellationToken cancellationToken = default)
        {
            var existing = await _languageRepo.GetByIdAsync(language.Id, cancellationToken).ConfigureAwait(false);
            await _languageRepo.UpdateAsync(language, cancellationToken).ConfigureAwait(false);
            await _auditService.LogAsync(nameof(Language), language.Id, "Update", existing, language, 1, "System", $"Updated language {language.Code} - {language.Name}", cancellationToken).ConfigureAwait(false);
        }

        public async Task DeleteLanguageAsync(int id, CancellationToken cancellationToken = default)
        {
            var existing = await _languageRepo.GetByIdAsync(id, cancellationToken).ConfigureAwait(false);
            await _languageRepo.DeleteAsync(id, cancellationToken).ConfigureAwait(false);
            await _auditService.LogAsync(nameof(Language), id, "Delete", existing, null, 1, "System", $"Deleted language {existing?.Code ?? id.ToString()}", cancellationToken).ConfigureAwait(false);
        }
    }
}
