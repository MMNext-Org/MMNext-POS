using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MMNextPOS.Application.Services;
using MMNextPOS.Domain.Models;
using MMNextPOS.Infrastructure.Repositories;
using Moq;
using Xunit;

namespace MMNextPOS.Application.Tests
{
    public class UserServiceTests
    {
        private readonly Mock<IUserRepository> _userRepoMock = new();
        private readonly Mock<IUserRoleRepository> _userRoleRepoMock = new();
        private readonly Mock<IRoleRepository> _roleRepoMock = new();
        private readonly Mock<IVerificationCodeRepository> _verificationCodeRepoMock = new();

        private UserService CreateService()
        {
            return new UserService(_userRepoMock.Object, _userRoleRepoMock.Object, _roleRepoMock.Object, _verificationCodeRepoMock.Object);
        }

        private static User CreateUser(string username, string password, bool isActive = true)
        {
            return new User
            {
                Id = 1,
                Username = username,
                FullName = "Test User",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password, BCrypt.Net.BCrypt.GenerateSalt(4)),
                IsActive = isActive
            };
        }

        private void SetupAuthenticateUser(string username, User? user)
        {
            _userRepoMock
                .Setup(r => r.GetByUsernameAsync(username, It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);
        }

        [Fact]
        public async Task AuthenticateAsync_ValidCredentials_ReturnsUserAndUpdatesLastLogin()
        {
            var user = CreateUser("admin", "Admin@123");
            SetupAuthenticateUser("admin", user);

            var service = CreateService();
            var result = await service.AuthenticateAsync("admin", "Admin@123");

            Assert.NotNull(result);
            Assert.NotNull(result.LastLoginAt);
            _userRepoMock.Verify(r => r.GetByUsernameAsync("admin", It.IsAny<CancellationToken>()), Times.Once);
            _userRepoMock.Verify(r => r.GetAllAsync(It.IsAny<CancellationToken>()), Times.Never);
            _userRepoMock.Verify(r => r.UpdateAsync(user, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task AuthenticateAsync_UsernameMatchIsCaseInsensitive()
        {
            var user = CreateUser("Admin", "password123");
            SetupAuthenticateUser("admin", user);

            var service = CreateService();
            var result = await service.AuthenticateAsync("admin", "password123");

            Assert.NotNull(result);
            Assert.Equal("Admin", result.Username);
        }

        [Fact]
        public async Task AuthenticateAsync_DatabaseCandidateMustMatchOrdinalIgnoreCase()
        {
            var user = CreateUser("café", "password123");
            SetupAuthenticateUser("cafe", user);

            var service = CreateService();
            var result = await service.AuthenticateAsync("cafe", "password123");

            Assert.Null(result);
            _userRepoMock.Verify(r => r.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task AuthenticateAsync_IncorrectPassword_ReturnsNull()
        {
            var user = CreateUser("admin", "correct-password");
            SetupAuthenticateUser("admin", user);

            var service = CreateService();
            var result = await service.AuthenticateAsync("admin", "wrong-password");

            Assert.Null(result);
            _userRepoMock.Verify(r => r.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task AuthenticateAsync_InactiveUser_ReturnsNull()
        {
            var user = CreateUser("admin", "Admin@123", isActive: false);
            SetupAuthenticateUser("admin", user);

            var service = CreateService();
            var result = await service.AuthenticateAsync("admin", "Admin@123");

            Assert.Null(result);
            _userRepoMock.Verify(r => r.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task AuthenticateAsync_UnknownUser_ReturnsNull()
        {
            SetupAuthenticateUser("ghost", null);

            var service = CreateService();
            var result = await service.AuthenticateAsync("ghost", "password");

            Assert.Null(result);
        }

        [Fact]
        public async Task AuthenticateAsync_EmptyCredentials_ReturnsNullWithoutQuerying()
        {
            var service = CreateService();
            var result = await service.AuthenticateAsync("", "secret");

            Assert.Null(result);
            _userRepoMock.Verify(r => r.GetByUsernameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
            _userRepoMock.Verify(r => r.GetAllAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task GetUserRolesAsync_ReturnsOnlyActiveRolesForUser()
        {
            var userRoles = new List<UserRole>
            {
                new() { Id = 1, UserId = 1, RoleId = 10 },
                new() { Id = 2, UserId = 1, RoleId = 20 },
                new() { Id = 3, UserId = 2, RoleId = 30 }
            };
            var roles = new List<Role>
            {
                new() { Id = 10, Code = "Admin", Name = "Administrator", IsActive = true },
                new() { Id = 20, Code = "Cashier", Name = "Cashier", IsActive = false },
                new() { Id = 30, Code = "Manager", Name = "Manager", IsActive = true }
            };

            _userRoleRepoMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                             .ReturnsAsync(userRoles);
            _roleRepoMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                         .ReturnsAsync(roles);

            var service = CreateService();
            var result = await service.GetUserRolesAsync(1);

            var role = Assert.Single(result);
            Assert.Equal("Admin", role.Code);
        }

        [Fact]
        public async Task GetUserRolesAsync_NoRoles_ReturnsEmpty()
        {
            _userRoleRepoMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                             .ReturnsAsync(new List<UserRole>());

            var service = CreateService();
            var result = await service.GetUserRolesAsync(1);

            Assert.Empty(result);
            _roleRepoMock.Verify(r => r.GetAllAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public void HashPassword_VerifiesAndUsesUniqueSalt()
        {
            var hash1 = UserService.HashPassword("password");
            var hash2 = UserService.HashPassword("password");

            Assert.NotEqual(hash1, hash2);
            Assert.True(BCrypt.Net.BCrypt.Verify("password", hash1));
        }

        [Fact]
        public void SeededAdminHash_VerifiesDefaultPassword()
        {
            // The DatabaseInitializer seeds this exact hash for the default 'admin' user.
            // This test guards that the shipped default login (admin / Admin@123) actually works.
            const string seedAdminHash = "$2a$12$H2IyHt.B8odnd3/3AZW54u3XYGjV/JDCRlKq.iP2HPg1UsDm9w.xe";

            Assert.True(BCrypt.Net.BCrypt.Verify("Admin@123", seedAdminHash));
        }
    }
}
