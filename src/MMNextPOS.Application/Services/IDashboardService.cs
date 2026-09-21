using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MMNextPOS.Domain.Models;

namespace MMNextPOS.Application.Services
{
    public interface IDashboardService
    {
        Task<DashboardWidget?> GetWidgetAsync(int id, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<DashboardWidget>> GetActiveWidgetsAsync(int? userId = null, int? locationId = null, CancellationToken cancellationToken = default);
        Task<DashboardWidget> CreateWidgetAsync(DashboardWidget widget, int userId, CancellationToken cancellationToken = default);
        Task<DashboardWidget> UpdateWidgetAsync(DashboardWidget widget, int userId, CancellationToken cancellationToken = default);
        Task DeleteWidgetAsync(int id, int userId, CancellationToken cancellationToken = default);
        Task RefreshWidgetAsync(int widgetId, int userId, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<DashboardWidget>> ReorderWidgetsAsync(Dictionary<int, int> newOrders, int userId, CancellationToken cancellationToken = default);

        static List<DashboardWidget> GetDefaultWidgets()
        {
            return new List<DashboardWidget>
            {
                new DashboardWidget
                {
                    Name = "Today's Sales",
                    DisplayName = "Today's Sales",
                    WidgetType = "KPI",
                    DataSource = "Sales",
                    DataSourceMethod = "GetTodayTotal",
                    Size = "Medium",
                    DisplayOrder = 1,
                    IsDefault = true,
                    IsEnabled = true
                },
                new DashboardWidget
                {
                    Name = "Top Products",
                    DisplayName = "Top Selling Products",
                    WidgetType = "List",
                    DataSource = "Sales",
                    DataSourceMethod = "GetTopSellingProducts",
                    Size = "Medium",
                    DisplayOrder = 2,
                    IsDefault = true,
                    IsEnabled = true,
                    MaxRecords = 10
                },
                new DashboardWidget
                {
                    Name = "Low Stock Alert",
                    DisplayName = "Low Stock Items",
                    WidgetType = "List",
                    DataSource = "Inventory",
                    DataSourceMethod = "GetLowStockItems",
                    Size = "Medium",
                    DisplayOrder = 3,
                    IsDefault = true,
                    IsEnabled = true
                }
            };
        }
    }
}
