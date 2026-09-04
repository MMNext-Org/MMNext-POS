using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MMNextPOS.Application.Services;
using MMNextPOS.Domain.Models;

namespace MMNextPOS.Application.Services
{
    /// <summary>
    /// Simple navigation page model without DevExpress dependency.
    /// </summary>
    public class NavigationPageModel
    {
        public string Caption { get; set; } = "";
        public string? Icon { get; set; }
    }

    /// <summary>
    /// Provides navigation menu configuration based on user roles.
    /// </summary>
    public interface IMainNavigationService
    {
        /// <summary>
        /// Gets the configured navigation pages for the given user role.
        /// </summary>
        /// <param name="role">The user's role identifier.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        Task<IReadOnlyList<NavigationPageModel>> GetNavigationAsync(string role, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the default navigation configuration for the current user.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        Task<IReadOnlyList<NavigationPageModel>> GetDefaultNavigationAsync(CancellationToken cancellationToken = default);
    }
}
