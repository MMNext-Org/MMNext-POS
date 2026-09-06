using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MMNextPOS.Application.Services;
using MMNextPOS.Domain.Models;
using Moq;
using Xunit;

namespace MMNextPOS.Application.Tests
{
    public class MainNavigationServiceTests
    {
        private readonly Mock<IMenuRoleService> _menuRoleServiceMock = new();
        private readonly Mock<IRoleService> _roleServiceMock = new();

        private MainNavigationService CreateService()
        {
            return new MainNavigationService(_menuRoleServiceMock.Object, _roleServiceMock.Object);
        }

        [Fact]
        public async Task GetNavigationAsync_ReturnsOnlyViewableMenusForRole()
        {
            // Arrange
            var role = new Role { Id = 10, Code = "Manager", IsActive = true };
            _roleServiceMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                            .ReturnsAsync(new List<Role> { role });

            var menus = new List<MenuRole>
            {
                new() { Id = 1, RoleId = 10, MenuCode = "SALES", CanView = true },
                new() { Id = 2, RoleId = 10, MenuCode = "REPORT", CanView = true },
                new() { Id = 3, RoleId = 10, MenuCode = "SUPERADMIN", CanView = false },
                new() { Id = 4, RoleId = 20, MenuCode = "LICENSE", CanView = true }
            };
            _menuRoleServiceMock.Setup(m => m.GetAllAsync(It.IsAny<CancellationToken>()))
                                .ReturnsAsync(menus);

            var service = CreateService();

            // Act
            var result = await service.GetNavigationAsync("manager");

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Contains(result, p => p.Caption == "Sales");
            Assert.Contains(result, p => p.Caption == "Reports");
        }

        [Fact]
        public async Task GetNavigationAsync_UnknownRole_ReturnsEmpty()
        {
            // Arrange
            _roleServiceMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                            .ReturnsAsync(new List<Role>());

            var service = CreateService();

            // Act
            var result = await service.GetNavigationAsync("Ghost");

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetNavigationAsync_InactiveRole_ReturnsEmpty()
        {
            // Arrange
            var role = new Role { Id = 10, Code = "Admin", IsActive = false };
            _roleServiceMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                            .ReturnsAsync(new List<Role> { role });
            _menuRoleServiceMock.Setup(m => m.GetAllAsync(It.IsAny<CancellationToken>()))
                                .ReturnsAsync(new List<MenuRole>
                                {
                                    new() { Id = 1, RoleId = 10, MenuCode = "SALES", CanView = true }
                                });

            var service = CreateService();

            // Act
            var result = await service.GetNavigationAsync("Admin");

            // Assert
            Assert.Empty(result);
        }
    }
}
