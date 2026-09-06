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

        private UserService CreateService()
        {
            return new UserService(_userRepoMock.Object, _userRoleRepoMock.Object, _roleRepoMock.Object);
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

        [Fact]
        public async Task AuthenticateAsync_ValidCredentials_ReturnsUserAndUpdatesLastLogin()
        {
            // Arrange
            var user = CreateUser("admin", "Admin@123");
            _userRepoMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                         .ReturnsAsync(new List<User> { user });

            var service = CreateService();

            // Act
            var result = await service.AuthenticateAsync("admin", "Admin@123");

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.LastLoginAt);
            _userRepoMock.Verify(r => r.UpdateAsync(user, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task AuthenticateAsync_UsernameMatchIsCaseInsensitive()
        {
            // Arrange
            var user = CreateUser("Admin", "password123");
            _userRepoMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                         .ReturnsAsync(new List<User> { user });

            var service = CreateService();

            // Act
            var result = await service.AuthenticateAsync("admin", "password123");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Admin", result.Username);
        }

        [Fact]
        public async Task AuthenticateAsync_IncorrectPassword_ReturnsNull()
        {
            // Arrange
            var user = CreateUser("admin", "correct-password");
            _userRepoMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                         .ReturnsAsync(new List<User> { user });

            var service = CreateService();

            // Act
            var result = await service.AuthenticateAsync("admin", "wrong-password");

            // Assert
            Assert.Null(result);
            _userRepoMock.Verify(r => r.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task AuthenticateAsync_InactiveUser_ReturnsNull()
        {
            // Arrange
            var user = CreateUser("admin", "Admin@123", isActive: false);
            _userRepoMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                         .ReturnsAsync(new List<User> { user });

            var service = CreateService();

            // Act
            var result = await service.AuthenticateAsync("admin", "Admin@123");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task AuthenticateAsync_UnknownUser_ReturnsNull()
        {
            // Arrange
            _userRepoMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                         .ReturnsAsync(new List<User>());

            var service = CreateService();

            // Act
            var result = await service.AuthenticateAsync("ghost", "password");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task AuthenticateAsync_EmptyCredentials_ReturnsNullWithoutQuerying()
        {
            // Arrange
            var service = CreateService();

            // Act
            var result = await service.AuthenticateAsync("", "secret");

            // Assert
            Assert.Null(result);
            _userRepoMock.Verify(r => r.GetAllAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task GetUserRolesAsync_ReturnsOnlyActiveRolesForUser()
        {
            // Arrange
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

            // Act
            var result = await service.GetUserRolesAsync(1);

            // Assert
            var role = Assert.Single(result);
            Assert.Equal("Admin", role.Code);
        }

        [Fact]
        public async Task GetUserRolesAsync_NoRoles_ReturnsEmpty()
        {
            // Arrange
            _userRoleRepoMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                             .ReturnsAsync(new List<UserRole>());

            var service = CreateService();

            // Act
            var result = await service.GetUserRolesAsync(1);

            // Assert
            Assert.Empty(result);
            _roleRepoMock.Verify(r => r.GetAllAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public void HashPassword_VerifiesAndUsesUniqueSalt()
        {
            // Act
            var hash1 = UserService.HashPassword("password");
            var hash2 = UserService.HashPassword("password");

            // Assert
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
