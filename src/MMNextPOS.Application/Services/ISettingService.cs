using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MMNextPOS.Domain.Models;

namespace MMNextPOS.Application.Services
{
    public interface ISettingService
    {
        // Category
        Task<Category?> GetCategoryByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<Category>> GetCategoriesAsync(CancellationToken cancellationToken = default);
        Task<Category> AddCategoryAsync(Category category, CancellationToken cancellationToken = default);
        Task UpdateCategoryAsync(Category category, CancellationToken cancellationToken = default);
        Task DeleteCategoryAsync(int id, CancellationToken cancellationToken = default);

        // Unit
        Task<Unit?> GetUnitByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<Unit>> GetUnitsAsync(CancellationToken cancellationToken = default);
        Task<Unit> AddUnitAsync(Unit unit, CancellationToken cancellationToken = default);
        Task UpdateUnitAsync(Unit unit, CancellationToken cancellationToken = default);
        Task DeleteUnitAsync(int id, CancellationToken cancellationToken = default);

        // Currency
        Task<Currency?> GetCurrencyByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<Currency>> GetCurrenciesAsync(CancellationToken cancellationToken = default);
        Task<Currency> AddCurrencyAsync(Currency currency, CancellationToken cancellationToken = default);
        Task UpdateCurrencyAsync(Currency currency, CancellationToken cancellationToken = default);
        Task DeleteCurrencyAsync(int id, CancellationToken cancellationToken = default);

        // Tax
        Task<Tax?> GetTaxByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<Tax>> GetTaxesAsync(CancellationToken cancellationToken = default);
        Task<Tax> AddTaxAsync(Tax tax, CancellationToken cancellationToken = default);
        Task UpdateTaxAsync(Tax tax, CancellationToken cancellationToken = default);
        Task DeleteTaxAsync(int id, CancellationToken cancellationToken = default);

        // Discount
        Task<Discount?> GetDiscountByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<Discount>> GetDiscountsAsync(CancellationToken cancellationToken = default);
        Task<Discount> AddDiscountAsync(Discount discount, CancellationToken cancellationToken = default);
        Task UpdateDiscountAsync(Discount discount, CancellationToken cancellationToken = default);
        Task DeleteDiscountAsync(int id, CancellationToken cancellationToken = default);

        // Theme
        Task<Theme?> GetThemeByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<Theme>> GetThemesAsync(CancellationToken cancellationToken = default);
        Task<Theme> AddThemeAsync(Theme theme, CancellationToken cancellationToken = default);
        Task UpdateThemeAsync(Theme theme, CancellationToken cancellationToken = default);
        Task DeleteThemeAsync(int id, CancellationToken cancellationToken = default);

        // Language
        Task<Language?> GetLanguageByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<Language>> GetLanguagesAsync(CancellationToken cancellationToken = default);
        Task<Language> AddLanguageAsync(Language language, CancellationToken cancellationToken = default);
        Task UpdateLanguageAsync(Language language, CancellationToken cancellationToken = default);
        Task DeleteLanguageAsync(int id, CancellationToken cancellationToken = default);
    }
}
