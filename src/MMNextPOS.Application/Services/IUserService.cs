using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MMNextPOS.Domain.Models;

namespace MMNextPOS.Application.Services
{
    public interface IUserService
    {
        Task<User?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<User>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<User> AddAsync(User user, CancellationToken cancellationToken = default);
        Task UpdateAsync(User user, CancellationToken cancellationToken = default);
        Task DeleteAsync(int id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Authenticates a user by username and password.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <param name="password">The plain-text password.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The authenticated user if successful; otherwise null.</returns>
        Task<User?> AuthenticateAsync(string username, string password, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the roles assigned to a user.
        /// </summary>
        /// <param name="userId">The user ID.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>List of roles for the user.</returns>
        Task<IReadOnlyList<Role>> GetUserRolesAsync(int userId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Changes the password for the current user (requires current password).
        /// </summary>
        /// <param name="userId">The user ID.</param>
        /// <param name="currentPassword">The current password.</param>
        /// <param name="newPassword">The new password.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>True if password was changed successfully.</returns>
        Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword, CancellationToken cancellationToken = default);

        /// <summary>
        /// Resets a user's password (admin-initiated, no current password required).
        /// </summary>
        /// <param name="userId">The user ID.</param>
        /// <param name="newPassword">The new password.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>True if password was reset successfully.</returns>
        Task<bool> ResetPasswordAsync(int userId, string newPassword, CancellationToken cancellationToken = default);
    }
}
