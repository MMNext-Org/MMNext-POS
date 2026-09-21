using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MMNextPOS.Domain.Models;
using MMNextPOS.Infrastructure;
using MMNextPOS.Infrastructure.Repositories;

namespace MMNextPOS.Application.Services
{
    /// <summary>
    /// Service for managing dashboard widgets and dashboard data retrieval.
    /// </summary>
    public class DashboardService : IDashboardService
    {
        private readonly IRepository<DashboardWidget> _widgetRepo;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAuditService _auditService;

        public DashboardService(
            IRepository<DashboardWidget> widgetRepo,
            IUnitOfWork unitOfWork,
            IAuditService auditService)
        {
            _widgetRepo = widgetRepo ?? throw new ArgumentNullException(nameof(widgetRepo));
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _auditService = auditService ?? throw new ArgumentNullException(nameof(auditService));
        }

        public async Task<DashboardWidget?> GetWidgetAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _widgetRepo.GetByIdAsync(id, cancellationToken);
        }

        public async Task<IReadOnlyList<DashboardWidget>> GetActiveWidgetsAsync(int? userId = null, int? locationId = null, CancellationToken cancellationToken = default)
        {
            var all = await _widgetRepo.GetAllAsync(cancellationToken);
            var filtered = all
                .Where(w => !w.IsDeleted && w.IsEnabled && w.IsVisible);

            if (userId.HasValue)
                filtered = filtered.Where(w => w.UserId == null || w.UserId == userId.Value);

            if (locationId.HasValue)
                filtered = filtered.Where(w => w.LocationId == null || w.LocationId == locationId.Value);

            return filtered.OrderBy(w => w.DisplayOrder).ToList();
        }

        public async Task<DashboardWidget> CreateWidgetAsync(DashboardWidget widget, int userId, CancellationToken cancellationToken = default)
        {
            await _unitOfWork.BeginTransactionAsync(cancellationToken);

            try
            {
                widget.CreatedAt = DateTime.UtcNow;
                widget.CreatedBy = userId;

                var created = await _widgetRepo.AddAsync(widget, cancellationToken);

                await _auditService.LogAsync(
                    nameof(DashboardWidget), created.Id,
                    "Create",
                    null,
                    new { widget.Name, widget.WidgetType, widget.DataSource, widget.DisplayOrder },
                    userId,
                    "System",
                    $"Dashboard widget created: {widget.Name}",
                    cancellationToken);

                await _unitOfWork.CommitAsync(cancellationToken);
                return created;
            }
            catch
            {
                await _unitOfWork.RollbackAsync(cancellationToken);
                throw;
            }
        }

        public async Task<DashboardWidget> UpdateWidgetAsync(DashboardWidget widget, int userId, CancellationToken cancellationToken = default)
        {
            await _unitOfWork.BeginTransactionAsync(cancellationToken);

            try
            {
                var existing = await _widgetRepo.GetByIdAsync(widget.Id, cancellationToken);
                if (existing == null)
                    throw new InvalidOperationException($"Widget {widget.Id} not found.");

                var oldValues = new
                {
                    existing.Name,
                    existing.WidgetType,
                    existing.DataSource,
                    existing.IsVisible,
                    existing.DisplayOrder
                };

                widget.UpdatedAt = DateTime.UtcNow;
                widget.UpdatedBy = userId;

                await _widgetRepo.UpdateAsync(widget, cancellationToken);

                await _auditService.LogAsync(
                    nameof(DashboardWidget), widget.Id,
                    "Update",
                    oldValues,
                    widget,
                    userId,
                    "System",
                    $"Dashboard widget updated: {widget.Name}",
                    cancellationToken);

                await _unitOfWork.CommitAsync(cancellationToken);
                return widget;
            }
            catch
            {
                await _unitOfWork.RollbackAsync(cancellationToken);
                throw;
            }
        }

        public async Task DeleteWidgetAsync(int id, int userId, CancellationToken cancellationToken = default)
        {
            await _unitOfWork.BeginTransactionAsync(cancellationToken);

            try
            {
                var widget = await _widgetRepo.GetByIdAsync(id, cancellationToken);
                if (widget == null)
                    throw new KeyNotFoundException($"Widget {id} not found.");

                await _widgetRepo.DeleteAsync(id, cancellationToken);

                await _auditService.LogAsync(
                    nameof(DashboardWidget), id,
                    "Delete",
                    new { widget.Name, widget.WidgetType },
                    null,
                    userId,
                    "System",
                    $"Dashboard widget deleted: {widget.Name}",
                    cancellationToken);

                await _unitOfWork.CommitAsync(cancellationToken);
            }
            catch
            {
                await _unitOfWork.RollbackAsync(cancellationToken);
                throw;
            }
        }

        public async Task RefreshWidgetAsync(int widgetId, int userId, CancellationToken cancellationToken = default)
        {
            await _unitOfWork.BeginTransactionAsync(cancellationToken);

            try
            {
                var widget = await _widgetRepo.GetByIdAsync(widgetId, cancellationToken);
                if (widget == null)
                    throw new KeyNotFoundException($"Widget {widgetId} not found.");

                widget.LastRefreshedAt = DateTime.UtcNow;
                widget.RefreshCount++;
                widget.UpdatedAt = DateTime.UtcNow;
                widget.UpdatedBy = userId;

                await _widgetRepo.UpdateAsync(widget, cancellationToken);

                await _unitOfWork.CommitAsync(cancellationToken);
            }
            catch
            {
                await _unitOfWork.RollbackAsync(cancellationToken);
                throw;
            }
        }

        public async Task<IReadOnlyList<DashboardWidget>> ReorderWidgetsAsync(Dictionary<int, int> newOrders, int userId, CancellationToken cancellationToken = default)
        {
            await _unitOfWork.BeginTransactionAsync(cancellationToken);

            try
            {
                var updatedWidgets = new List<DashboardWidget>();

                foreach (var kvp in newOrders)
                {
                    var widget = await _widgetRepo.GetByIdAsync(kvp.Key, cancellationToken);
                    if (widget != null)
                    {
                        widget.DisplayOrder = kvp.Value;
                        widget.UpdatedAt = DateTime.UtcNow;
                        widget.UpdatedBy = userId;
                        await _widgetRepo.UpdateAsync(widget, cancellationToken);
                        updatedWidgets.Add(widget);
                    }
                }

                await _unitOfWork.CommitAsync(cancellationToken);
                return updatedWidgets;
            }
            catch
            {
                await _unitOfWork.RollbackAsync(cancellationToken);
                throw;
            }
        }

        /// <summary>
        /// Gets default dashboard widgets for a new user (system defaults).
        /// </summary>
        public static List<DashboardWidget> GetDefaultWidgets()
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
                },
                new DashboardWidget
                {
                    Name = "Outstanding Balance",
                    DisplayName = "Outstanding Balances",
                    WidgetType = "KPI",
                    DataSource = "Outstanding",
                    DataSourceMethod = "GetTotalOutstanding",
                    Size = "Medium",
                    DisplayOrder = 4,
                    IsDefault = true,
                    IsEnabled = true
                },
                new DashboardWidget
                {
                    Name = "Recent Sales",
                    DisplayName = "Recent Sales",
                    WidgetType = "List",
                    DataSource = "Sales",
                    DataSourceMethod = "GetRecentSales",
                    Size = "Large",
                    DisplayOrder = 5,
                    IsDefault = true,
                    IsEnabled = true,
                    MaxRecords = 20
                }
            };
        }
    }
}
