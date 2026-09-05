using System.Collections.Generic;
using MMNextPOS.Domain.Models;

namespace MMNextPOS.Application.Services
{
    /// <summary>
    /// Holds the current authenticated user's session information.
    /// Scoped per application lifetime (WinForms app runs as single session).
    /// </summary>
    public interface IUserSession
    {
        /// <summary>
        /// The currently authenticated user.
        /// </summary>
        User? CurrentUser { get; set; }

        /// <summary>
        /// The roles assigned to the current user.
        /// </summary>
        IReadOnlyList<Role> Roles { get; set; }

        /// <summary>
        /// Whether a user is currently authenticated.
        /// </summary>
        bool IsAuthenticated { get; }

        /// <summary>
        /// Checks if the current user has a specific role.
        /// </summary>
        /// <param name="roleCode">The role code to check (e.g., "Admin", "Cashier").</param>
        /// <returns>True if the user has the role; otherwise false.</returns>
        bool HasRole(string roleCode);

        /// <summary>
        /// Clears the session (logout).
        /// </summary>
        void Clear();
    }
}