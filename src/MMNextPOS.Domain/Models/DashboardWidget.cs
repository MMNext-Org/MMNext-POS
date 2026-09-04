using System;
using System.ComponentModel.DataAnnotations;

namespace MMNextPOS.Domain.Models
{
    /// <summary>
    /// Represents a dashboard widget configuration for the POS system.
    /// </summary>
    public class DashboardWidget : EntityBase
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? DisplayName { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        [Required]
        [MaxLength(50)]
        public string WidgetType { get; set; } = string.Empty; // Chart, Grid, KPI, List, Gauge, Progress, Table

        [Required]
        [MaxLength(50)]
        public string DataSource { get; set; } = string.Empty; // Sales, Inventory, Customers, Expenses, Purchases, Outstanding, CashFlow, ProfitLoss

        [MaxLength(100)]
        public string? DataSourceMethod { get; set; } // GetTopSellingProducts, GetDailySales, GetLowStockItems, etc.

        [MaxLength(2000)]
        public string? Configuration { get; set; } // JSON configuration for the widget (filters, date ranges, chart options, etc.)

        [MaxLength(100)]
        public string? ChartType { get; set; } // Bar, Line, Pie, Doughnut, Area, Column, Radar, PolarArea

        [MaxLength(50)]
        public string TimeRange { get; set; } = "Last30Days"; // Today, Last7Days, Last30Days, ThisMonth, LastMonth, ThisQuarter, ThisYear, Custom

        public DateTime? CustomStartDate { get; set; }

        public DateTime? CustomEndDate { get; set; }

        public int RefreshIntervalMinutes { get; set; } = 5; // Auto-refresh interval

        public int Width { get; set; } = 6; // Grid width (1-12 for 12-column grid)

        public int Height { get; set; } = 4; // Grid height in rows

        public int DisplayOrder { get; set; } = 0;

        public int? LocationId { get; set; } // Null means global/all locations

        public int? UserId { get; set; } // Null means global/all users, specific user for personal widgets

        public bool IsDefault { get; set; } = false; // System default widget

        public bool IsVisible { get; set; } = true;

        public bool IsEnabled { get; set; } = true;

        [MaxLength(50)]
        public string Size { get; set; } = "Medium"; // Small, Medium, Large, FullWidth

        [MaxLength(100)]
        public string? Icon { get; set; } // FontAwesome or custom icon class

        [MaxLength(50)]
        public string? ColorScheme { get; set; } // Primary, Secondary, Success, Warning, Danger, Info, Custom

        [MaxLength(500)]
        public string? CustomCssClass { get; set; }

        public int MaxRecords { get; set; } = 50; // Max records to display for list/grid widgets

        [MaxLength(100)]
        public string? GroupBy { get; set; } // Field to group by

        [MaxLength(100)]
        public string? SortBy { get; set; } // Field to sort by

        [MaxLength(10)]
        public string SortDirection { get; set; } = "DESC"; // ASC, DESC

        public bool ShowHeader { get; set; } = true;

        public bool ShowFooter { get; set; } = false;

        public bool AllowExport { get; set; } = true;

        public bool AllowDrillDown { get; set; } = false;

        [MaxLength(200)]
        public string? DrillDownUrl { get; set; } // URL or route for drill-down

        [MaxLength(500)]
        public string? DrillDownParameters { get; set; } // JSON parameters for drill-down

        public DateTime? LastRefreshedAt { get; set; }

        public int RefreshCount { get; set; } = 0;

        [MaxLength(500)]
        public string? LastError { get; set; }

        public bool HasError { get; set; } = false;
    }
}
