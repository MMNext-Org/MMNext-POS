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

        public MainNavigationService(IMenuRoleService menuRoleService)
        {
            _menuRoleService = menuRoleService;
        }

        public async Task<IReadOnlyList<NavigationPageModel>> GetNavigationAsync(string role, CancellationToken cancellationToken = default)
        {
            var menus = await _menuRoleService.GetAllAsync(cancellationToken);

            var navigationPages = new List<NavigationPageModel>();

            // Map MenuCode values to navigation page captions
            var menuCodes = menus.Select(m => m.MenuCode).Distinct();

            foreach (var code in menuCodes)
            {
                switch (code.ToUpperInvariant())
                {
                    case "SALES":
                        navigationPages.Add(new NavigationPageModel { Caption = "Sales" });
                        break;
                    case "PRODUCT":
                    case "INVENTORY":
                        navigationPages.Add(new NavigationPageModel { Caption = "Products" });
                        break;
                    case "CUSTOMER":
                        navigationPages.Add(new NavigationPageModel { Caption = "Customers" });
                        break;
                    case "REPORT":
                    case "REPORTS":
                        navigationPages.Add(new NavigationPageModel { Caption = "Reports" });
                        break;
                    default:
                        navigationPages.Add(new NavigationPageModel { Caption = code });
                        break;
                }
            }

            return navigationPages;
        }

        public async Task<IReadOnlyList<NavigationPageModel>> GetDefaultNavigationAsync(CancellationToken cancellationToken = default)
        {
            return await GetNavigationAsync("User", cancellationToken);
        }
    }
}
