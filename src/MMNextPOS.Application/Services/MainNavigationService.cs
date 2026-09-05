using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MMNextPOS.Domain.Models;

namespace MMNextPOS.Application.Services
{
    /// <summary>
    /// Implementation of IMainNavigationService that builds navigation
    /// from the user's role-based menus.
    /// </summary>
    public class MainNavigationService : IMainNavigationService
    {
        private readonly IMenuRoleService _menuRoleService;
        private readonly IRoleService _roleService;

        public MainNavigationService(IMenuRoleService menuRoleService, IRoleService roleService)
        {
            _menuRoleService = menuRoleService ?? throw new ArgumentNullException(nameof(menuRoleService));
            _roleService = roleService ?? throw new ArgumentNullException(nameof(roleService));
        }

        public async Task<IReadOnlyList<NavigationPageModel>> GetNavigationAsync(string role, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(role))
                return Array.Empty<NavigationPageModel>();

            var roles = await _roleService.GetAllAsync(cancellationToken).ConfigureAwait(false);
            var matchedRole = roles.FirstOrDefault(r =>
                string.Equals(r.Code, role, StringComparison.OrdinalIgnoreCase) && r.IsActive);

            if (matchedRole == null)
                return Array.Empty<NavigationPageModel>();

            var menus = await _menuRoleService.GetAllAsync(cancellationToken).ConfigureAwait(false);
            var menuCodes = menus
                .Where(m => m.RoleId == matchedRole.Id && m.CanView)
                .Select(m => m.MenuCode)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            return menuCodes
                .OrderBy(MapOrder, Comparer<int>.Default)
                .ThenBy(code => code, StringComparer.OrdinalIgnoreCase)
                .Select(code => new NavigationPageModel { Caption = MapCaption(code) })
                .ToList();
        }

        public Task<IReadOnlyList<NavigationPageModel>> GetDefaultNavigationAsync(CancellationToken cancellationToken = default)
        {
            IReadOnlyList<NavigationPageModel> pages = new[]
            {
                new NavigationPageModel { Caption = "Products" },
                new NavigationPageModel { Caption = "Customers" }
            };

            return Task.FromResult(pages);
        }

        private static int MapOrder(string code)
        {
            switch (code.ToUpperInvariant())
            {
                case "SALES": return 1;
                case "PRODUCT":
                case "INVENTORY": return 2;
                case "CUSTOMER": return 3;
                case "PURCHASE": return 4;
                case "OUTSTANDING": return 5;
                case "EXPENSE": return 6;
                case "WAREHOUSE": return 7;
                case "REPORT":
                case "REPORTS": return 8;
                case "SETTINGS": return 9;
                default: return int.MaxValue;
            }
        }

        private static string MapCaption(string code)
        {
            switch (code.ToUpperInvariant())
            {
                case "SALES": return "Sales";
                case "PRODUCT":
                case "INVENTORY": return "Products";
                case "CUSTOMER": return "Customers";
                case "PURCHASE": return "Purchases";
                case "OUTSTANDING": return "Outstanding";
                case "EXPENSE": return "Expenses";
                case "WAREHOUSE": return "Stock Transfers";
                case "REPORT":
                case "REPORTS": return "Reports";
                case "SETTINGS": return "Settings";
                default: return code;
            }
        }
    }
}