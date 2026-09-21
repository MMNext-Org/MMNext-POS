using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MMNextPOS.Application.Services;
using MMNextPOS.Domain.Models;
using MMNextPOS.Infrastructure;
using MMNextPOS.Infrastructure.Repositories;
using Moq;
using Xunit;

namespace MMNextPOS.Application.Tests
{
    /// <summary>
    /// Unit tests for DashboardService.
    /// </summary>
    public class DashboardServiceTests
    {
        private readonly Mock<IRepository<DashboardWidget>> _widgetRepoMock = new();
        private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
        private readonly Mock<IAuditService> _auditServiceMock = new();

        private IDashboardService CreateService()
        {
            return new DashboardService(
                _widgetRepoMock.Object,
                _unitOfWorkMock.Object,
                _auditServiceMock.Object);
        }

        private void SetupHappyPath()
        {
            _widgetRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new DashboardWidget { Id = 1, Name = "Test Widget", WidgetType = "KPI", IsVisible = true, IsEnabled = true });

            _widgetRepoMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<DashboardWidget>
                {
                    new() { Id = 1, Name = "Sales KPI", WidgetType = "KPI", IsVisible = true, DisplayOrder = 1, IsEnabled = true },
                    new() { Id = 2, Name = "Inventory Widget", WidgetType = "List", IsVisible = true, DisplayOrder = 2, IsEnabled = true },
                    new() { Id = 3, Name = "Hidden Widget", WidgetType = "Chart", IsVisible = false, DisplayOrder = 3, IsEnabled = false }
                });

            _widgetRepoMock.Setup(r => r.AddAsync(It.IsAny<DashboardWidget>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((DashboardWidget w, CancellationToken _) => { w.Id = 10; return w; });

            _widgetRepoMock.Setup(r => r.UpdateAsync(It.IsAny<DashboardWidget>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _widgetRepoMock.Setup(r => r.DeleteAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _unitOfWorkMock.Setup(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _unitOfWorkMock.Setup(u => u.CommitAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _auditServiceMock.Setup(a => a.LogAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(),
                It.IsAny<object>(), It.IsAny<object>(), It.IsAny<int?>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task GetWidgetAsync_ExistingWidget_ReturnsWidget()
        {
            SetupHappyPath();
            var service = CreateService();

            var result = await service.GetWidgetAsync(1);

            Assert.NotNull(result);
            Assert.Equal("Test Widget", result!.Name);
        }

        [Fact]
        public async Task GetActiveWidgetsAsync_ReturnsOnlyVisibleWidgets()
        {
            SetupHappyPath();
            var service = CreateService();

            var result = await service.GetActiveWidgetsAsync();

            // Only widgets 1 and 2 are visible, widget 3 is hidden
            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task GetActiveWidgetsAsync_WithUserId_ReturnsUserSpecificOrGlobal()
        {
            SetupHappyPath();
            var service = CreateService();

            var result = await service.GetActiveWidgetsAsync(userId: 5);

            Assert.NotNull(result);
        }

        [Fact]
        public async Task CreateWidgetAsync_ValidWidget_CreatesAndAudits()
        {
            SetupHappyPath();
            var service = CreateService();
            var newWidget = new DashboardWidget
            {
                Name = "New Widget",
                WidgetType = "Chart",
                DataSource = "Sales",
                IsEnabled = true
            };

            var result = await service.CreateWidgetAsync(newWidget, userId: 1);

            Assert.NotNull(result);
            Assert.Equal(10, result.Id);
            _unitOfWorkMock.Verify(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWorkMock.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
            _widgetRepoMock.Verify(r => r.AddAsync(It.IsAny<DashboardWidget>(), It.IsAny<CancellationToken>()), Times.Once);
            _auditServiceMock.Verify(a => a.LogAsync(nameof(DashboardWidget), 10, "Create", null, It.IsAny<object>(), 1, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task UpdateWidgetAsync_ValidWidget_UpdatesAndAudits()
        {
            SetupHappyPath();
            var service = CreateService();
            var widget = new DashboardWidget
            {
                Id = 1,
                Name = "Updated Name",
                WidgetType = "Chart",
                DataSource = "Inventory",
                DisplayOrder = 5
            };

            await service.UpdateWidgetAsync(widget, userId: 1);

            _widgetRepoMock.Verify(r => r.UpdateAsync(It.IsAny<DashboardWidget>(), It.IsAny<CancellationToken>()), Times.Once);
            _auditServiceMock.Verify(a => a.LogAsync(nameof(DashboardWidget), 1, "Update", It.IsAny<object>(), widget, 1, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task DeleteWidgetAsync_ExistingWidget_DeletesAndAudits()
        {
            SetupHappyPath();
            var service = CreateService();

            await service.DeleteWidgetAsync(1, userId: 1);

            _widgetRepoMock.Verify(r => r.DeleteAsync(1, It.IsAny<CancellationToken>()), Times.Once);
            _auditServiceMock.Verify(a => a.LogAsync(nameof(DashboardWidget), 1, "Delete", It.IsAny<object>(), null, 1, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task RefreshWidgetAsync_UpdatesTimestamp()
        {
            SetupHappyPath();
            var service = CreateService();

            await service.RefreshWidgetAsync(1, userId: 1);

            _widgetRepoMock.Verify(r => r.UpdateAsync(It.Is<DashboardWidget>(w => w.LastRefreshedAt.HasValue && w.RefreshCount == 1), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task ReorderWidgetsAsync_UpdatesDisplayOrder()
        {
            SetupHappyPath();
            var service = CreateService();
            var orders = new Dictionary<int, int> { { 1, 3 }, { 2, 1 } };

            var result = await service.ReorderWidgetsAsync(orders, userId: 1);

            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            _widgetRepoMock.Verify(r => r.UpdateAsync(It.IsAny<DashboardWidget>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
        }

        [Fact]
        public void GetDefaultWidgets_ReturnsExpectedWidgets()
        {
            var defaults = DashboardService.GetDefaultWidgets();

            Assert.True(defaults.Count >= 3);
            Assert.Contains(defaults, w => w.Name == "Today's Sales");
            Assert.Contains(defaults, w => w.Name == "Top Products");
        }
    }
}
