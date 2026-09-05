using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BCryptNet = BCrypt.Net.BCrypt;
using MMNextPOS.Domain.Models;
using MMNextPOS.Infrastructure.Repositories;

namespace MMNextPOS.Application.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _repo;
        private readonly IUserRoleRepository _userRoleRepo;
        private readonly IRoleRepository _roleRepo;

        public UserService(
            IUserRepository repo,
            IUserRoleRepository userRoleRepo,
            IRoleRepository roleRepo)
        {
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
            _userRoleRepo = userRoleRepo ?? throw new ArgumentNullException(nameof(userRoleRepo));
            _roleRepo = roleRepo ?? throw new ArgumentNullException(nameof(roleRepo));
        }

        public Task<User> AddAsync(User user, CancellationToken cancellationToken = default)
            => _repo.AddAsync(user, cancellationToken);

        public Task DeleteAsync(int id, CancellationToken cancellationToken = default)
            => _repo.DeleteAsync(id, cancellationToken);

        public Task<IReadOnlyList<User>> GetAllAsync(CancellationToken cancellationToken = default)
            => _repo.GetAllAsync(cancellationToken);

        public Task<User?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
            => _repo.GetByIdAsync(id, cancellationToken);

        public Task UpdateAsync(User user, CancellationToken cancellationToken = default)
            => _repo.UpdateAsync(user, cancellationToken);

        public async Task<User?> AuthenticateAsync(string username, string password, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                return null;

            var users = await _repo.GetAllAsync(cancellationToken);
            var user = users.FirstOrDefault(u => 
                string.Equals(u.Username, username, StringComparison.OrdinalIgnoreCase));

            if (user == null || !user.IsActive)
                return null;

            // Verify password using BCrypt
            bool passwordValid = false;
            try
            {
                passwordValid = BCryptNet.Verify(password, user.PasswordHash);
            }
            catch
            {
                // Hash format invalid or other verification error
                passwordValid = false;
            }

            if (!passwordValid)
                return null;

            // Update last login timestamp
            user.LastLoginAt = DateTime.UtcNow;
            await _repo.UpdateAsync(user, cancellationToken);

            return user;
        }

        public async Task<IReadOnlyList<Role>> GetUserRolesAsync(int userId, CancellationToken cancellationToken = default)
        {
            var userRoles = await _userRoleRepo.GetAllAsync(cancellationToken);
            var roleIds = userRoles.Where(ur => ur.UserId == userId).Select(ur => ur.RoleId).ToList();

            if (!roleIds.Any())
                return Array.Empty<Role>();

            var roles = await _roleRepo.GetAllAsync(cancellationToken);
            return roles.Where(r => roleIds.Contains(r.Id) && r.IsActive).ToList();
        }

        public async Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(currentPassword) || string.IsNullOrWhiteSpace(newPassword))
                return false;

            if (newPassword.Length < 6)
                throw new ArgumentException("New password must be at least 6 characters long.");

            var user = await _repo.GetByIdAsync(userId, cancellationToken);
            if (user == null || !user.IsActive)
                return false;

            // Verify current password
            bool passwordValid = false;
            try
            {
                passwordValid = BCryptNet.Verify(currentPassword, user.PasswordHash);
            }
            catch
            {
                passwordValid = false;
            }

            if (!passwordValid)
                return false;

            // Hash new password and update
            user.PasswordHash = BCryptNet.HashPassword(newPassword, BCryptNet.GenerateSalt(12));
            user.UpdatedAt = DateTime.UtcNow;
            await _repo.UpdateAsync(user, cancellationToken);

            return true;
        }

        public async Task<bool> ResetPasswordAsync(int userId, string newPassword, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(newPassword))
                return false;

            if (newPassword.Length < 6)
                throw new ArgumentException("New password must be at least 6 characters long.");

            var user = await _repo.GetByIdAsync(userId, cancellationToken);
            if (user == null || !user.IsActive)
                return false;

            // Hash new password and update (no current password verification)
            user.PasswordHash = BCryptNet.HashPassword(newPassword, BCryptNet.GenerateSalt(12));
            user.UpdatedAt = DateTime.UtcNow;
            await _repo.UpdateAsync(user, cancellationToken);

            return true;
        }

        /// <summary>
        /// Hashes a plain-text password using BCrypt.
        /// </summary>
        public static string HashPassword(string password)
        {
            return BCryptNet.HashPassword(password, BCryptNet.GenerateSalt(12));
        }
    }
}
