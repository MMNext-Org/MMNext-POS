using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MMNextPOS.Application.Services;
using MMNextPOS.Domain.Models;
using MMNextPOS.Infrastructure.Repositories;
using Moq;
using Xunit;

namespace MMNextPOS.Application.Tests
{
    public class SettingServiceTests
    {
        private readonly Mock<ICategoryRepository> _categoryRepoMock = new();
        private readonly Mock<IUnitRepository> _unitRepoMock = new();
        private readonly Mock<ICurrencyRepository> _currencyRepoMock = new();
        private readonly Mock<ITaxRepository> _taxRepoMock = new();
        private readonly Mock<IDiscountRepository> _discountRepoMock = new();
        private readonly Mock<IThemeRepository> _themeRepoMock = new();
        private readonly Mock<ILanguageRepository> _languageRepoMock = new();
        private readonly Mock<IAuditService> _auditServiceMock = new();

        private ISettingService CreateService()
        {
            return new SettingService(
                _categoryRepoMock.Object,
                _unitRepoMock.Object,
                _currencyRepoMock.Object,
                _taxRepoMock.Object,
                _discountRepoMock.Object,
                _themeRepoMock.Object,
                _languageRepoMock.Object,
                _auditServiceMock.Object);
        }

        [Fact]
        public async Task GetCategoriesAsync_ReturnsAllCategories()
        {
            var categories = new List<Category>
            {
                new() { Id = 1, Code = "CAT001", Name = "Electronics", IsActive = true },
                new() { Id = 2, Code = "CAT002", Name = "Clothing", IsActive = true },
            };
            _categoryRepoMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                             .ReturnsAsync(categories);

            var service = CreateService();
            var result = await service.GetCategoriesAsync();

            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task AddCategoryAsync_ValidCategory_ReturnsAddedCategory()
        {
            var category = new Category { Code = "CAT003", Name = "Home Appliances", IsActive = true };
            var addedCategory = new Category { Id = 3, Code = "CAT003", Name = "Home Appliances", IsActive = true };
            _categoryRepoMock.Setup(r => r.AddAsync(category, It.IsAny<CancellationToken>()))
                             .ReturnsAsync(addedCategory);
            _auditServiceMock.Setup(a => a.LogAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(),
                It.IsAny<object>(), It.IsAny<object>(), It.IsAny<int?>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var service = CreateService();
            var result = await service.AddCategoryAsync(category);

            Assert.NotNull(result);
            Assert.Equal(addedCategory.Id, result.Id);
            Assert.Equal(category.Code, result.Code);
            _categoryRepoMock.Verify(r => r.AddAsync(category, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task UpdateCategoryAsync_ExistingCategory_UpdatesCategory()
        {
            var existing = new Category { Id = 1, Code = "CAT001", Name = "Old Name" };
            var updated = new Category { Id = 1, Code = "CAT001", Name = "New Name" };
            _categoryRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                             .ReturnsAsync(existing);
            _categoryRepoMock.Setup(r => r.UpdateAsync(existing, It.IsAny<CancellationToken>()))
                             .Returns(Task.CompletedTask);
            _auditServiceMock.Setup(a => a.LogAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(),
                It.IsAny<object>(), It.IsAny<object>(), It.IsAny<int?>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var service = CreateService();
            await service.UpdateCategoryAsync(updated);

            _categoryRepoMock.Verify(r => r.UpdateAsync(It.Is<Category>(c => c.Name == "New Name"), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetCurrenciesAsync_ReturnsAllCurrencies()
        {
            var currencies = new List<Currency>
            {
                new() { Id = 1, Code = "USD", Name = "US Dollar", IsActive = true },
                new() { Id = 2, Code = "EUR", Name = "Euro", IsActive = true },
            };
            _currencyRepoMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                             .ReturnsAsync(currencies);

            var service = CreateService();
            var result = await service.GetCurrenciesAsync();

            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task AddCurrencyAsync_ValidCurrency_ReturnsAddedCurrency()
        {
            var currency = new Currency { Code = "GBP", Name = "British Pound", Symbol = "£", ExchangeRate = 1.25m, IsActive = true };
            var addedCurrency = new Currency { Id = 3, Code = "GBP", Name = "British Pound", Symbol = "£", ExchangeRate = 1.25m, IsActive = true };
            _currencyRepoMock.Setup(r => r.AddAsync(currency, It.IsAny<CancellationToken>()))
                             .ReturnsAsync(addedCurrency);
            _auditServiceMock.Setup(a => a.LogAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(),
                It.IsAny<object>(), It.IsAny<object>(), It.IsAny<int?>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var service = CreateService();
            var result = await service.AddCurrencyAsync(currency);

            Assert.NotNull(result);
            Assert.Equal(addedCurrency.Id, result.Id);
            Assert.Equal(currency.Code, result.Code);
        }

        [Fact]
        public async Task GetTaxesAsync_ReturnsAllTaxes()
        {
            var taxes = new List<Tax>
            {
                new() { Id = 1, Code = "VAT10", Name = "VAT 10%", Rate = 0.10m, IsActive = true },
                new() { Id = 2, Code = "VAT5", Name = "VAT 5%", Rate = 0.05m, IsActive = true },
            };
            _taxRepoMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                        .ReturnsAsync(taxes);

            var service = CreateService();
            var result = await service.GetTaxesAsync();

            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task AddTaxAsync_ValidTax_ReturnsAddedTax()
        {
            var tax = new Tax { Code = "GST", Name = "GST 7%", Rate = 0.07m, IsActive = true };
            var addedTax = new Tax { Id = 3, Code = "GST", Name = "GST 7%", Rate = 0.07m, IsActive = true };
            _taxRepoMock.Setup(r => r.AddAsync(tax, It.IsAny<CancellationToken>()))
                        .ReturnsAsync(addedTax);
            _auditServiceMock.Setup(a => a.LogAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(),
                It.IsAny<object>(), It.IsAny<object>(), It.IsAny<int?>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var service = CreateService();
            var result = await service.AddTaxAsync(tax);

            Assert.NotNull(result);
            Assert.Equal(addedTax.Id, result.Id);
            Assert.Equal(tax.Code, result.Code);
        }

        [Fact]
        public async Task GetDiscountsAsync_ReturnsAllDiscounts()
        {
            var discounts = new List<Discount>
            {
                new() { Id = 1, Code = "DISC10", Name = "10% Off", Rate = 0.10m, IsActive = true },
                new() { Id = 2, Code = "DISC20", Name = "20% Off", Rate = 0.20m, IsActive = true },
            };
            _discountRepoMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                             .ReturnsAsync(discounts);

            var service = CreateService();
            var result = await service.GetDiscountsAsync();

            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task AddThemeAsync_ValidTheme_ReturnsAddedTheme()
        {
            var theme = new Theme { Code = "DARK", Name = "Dark Theme", IsActive = true };
            var addedTheme = new Theme { Id = 1, Code = "DARK", Name = "Dark Theme", IsActive = true };
            var themeRepoMock = new Mock<IThemeRepository>();
            themeRepoMock.Setup(r => r.AddAsync(theme, It.IsAny<CancellationToken>()))
                         .ReturnsAsync(addedTheme);
            _auditServiceMock.Setup(a => a.LogAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(),
                It.IsAny<object>(), It.IsAny<object>(), It.IsAny<int?>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var service = new SettingService(
                Mock.Of<ICategoryRepository>(),
                Mock.Of<IUnitRepository>(),
                Mock.Of<ICurrencyRepository>(),
                Mock.Of<ITaxRepository>(),
                Mock.Of<IDiscountRepository>(),
                themeRepoMock.Object,
                Mock.Of<ILanguageRepository>(),
                _auditServiceMock.Object);

            var result = await service.AddThemeAsync(theme);

            Assert.NotNull(result);
            Assert.Equal(addedTheme.Id, result.Id);
        }

        [Fact]
        public async Task GetLanguagesAsync_ReturnsAllLanguages()
        {
            var languages = new List<Language>
            {
                new() { Id = 1, Code = "en", Name = "English", IsActive = true },
                new() { Id = 2, Code = "my", Name = "Myanmar", IsActive = true },
            };
            _languageRepoMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                             .ReturnsAsync(languages);

            var service = CreateService();
            var result = await service.GetLanguagesAsync();

            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task AddLanguageAsync_ValidLanguage_ReturnsAddedLanguage()
        {
            var language = new Language { Code = "zh", Name = "Chinese", IsActive = true };
            var addedLanguage = new Language { Id = 3, Code = "zh", Name = "Chinese", IsActive = true };
            var languageRepoMock = new Mock<ILanguageRepository>();
            languageRepoMock.Setup(r => r.AddAsync(language, It.IsAny<CancellationToken>()))
                            .ReturnsAsync(addedLanguage);
            _auditServiceMock.Setup(a => a.LogAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(),
                It.IsAny<object>(), It.IsAny<object>(), It.IsAny<int?>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var service = new SettingService(
                Mock.Of<ICategoryRepository>(),
                Mock.Of<IUnitRepository>(),
                Mock.Of<ICurrencyRepository>(),
                Mock.Of<ITaxRepository>(),
                Mock.Of<IDiscountRepository>(),
                Mock.Of<IThemeRepository>(),
                languageRepoMock.Object,
                _auditServiceMock.Object);

            var result = await service.AddLanguageAsync(language);

            Assert.NotNull(result);
            Assert.Equal(addedLanguage.Id, result.Id);
        }
    }
}
