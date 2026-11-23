using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DevExpress.XtraBars.Navigation;
using DevExpress.XtraBars.Docking;
using DevExpress.XtraBars;
using DevExpress.XtraEditors;
using DevExpress.XtraGrid;
using DevExpress.XtraPrinting;
using DevExpress.XtraPrinting.Printing;
using DevExpress.XtraReports.Designer;
using DevExpress.XtraReports.Export;
using DevExpress.XtraReports.Export.Pdf;
using DevExpress.XtraReports.Localization;
using DevExpress.XtraReports.UserInterface;
using DevExpress.XtraSpire;
using MMNextPOS.Application.Services;
using MMNextPOS.Application.Dashboard;
using MMNextPOS.Application.Services;
using MMNextPOS.Domain.Models;
using MMNextPOS.WinForms.Services;
using MMNextPOS.Infrastructure.Repositories;
using MMNextPOS.WinForms.Templates;
using Microsoft.Extensions.DependencyInjection;
using MMNextPOS.WinForms.Templates;

namespace MMNextPOS.WinForms.Pages;

/// <summary>
/// Dashboard Page with KPI widgets and quick access to key reports.
/// </summary>
public class DashboardPage : ListPage;
{
    private readonly IDashboardService _dashboardService;
    private readonly IServiceProvider _serviceProvider;
    private readonly IWinFormsReportService _reportService;
    private readonly IMainNavigationService _navigationService;
    
    private ListBoxBoxItem _kpiTodaySales = null!;
    private ListBoxBoxItem _kpiOutstanding = null!;
    private ListBoxBoxItem _kpiCashFlow = null!;
    private ListBoxBoxItem _kpiLowStock = null!;
    private GridControl _dashboardGrid = null!;
    private ReportGalleryPanelControl _reportGallery = null!;
    private PanelControl _buttonPanel = null!;
    
    public DashboardPage(
        IServiceProvider serviceProvider,
        IDashboardService dashboardService,
        IWinFormsReportService reportService,
        IMainNavigationService navigationService)
    {
        _dashboardService = dashboardService;
        _serviceProvider = serviceProvider;
        _reportService = reportService;
        _navigationService = navigationService;
        
        Name = "Dashboard";
        Caption = "Dashboard";
    }
    
    protected override void Initialize()
    {
        // Add navigation buttons
        var titlePanel = new TitlePanelControl { 
            Title = "Dashboard", 
            Subtitle = "Key Metrics & Reports", 
            Dock = DockStyle.Top,
            Height = 50
        };
        titlePanel.Controls.AddRange(new Control[2] { _kpiTodaySales, _kpiOutstanding });
        
        // Add buttons
        _buttonPanel = new PanelControl();
        var newSaleButton = new ButtonItem();
        newSaleButton.Caption = "New Sale";
        newSaleButton.ItemClick += (s, e) => _navigationService.NavigateTo("New Sale");
        _buttonPanel.Controls.Add(newSaleButton);
        
        // Add dashboard grid
        var gridPanel = new PanelControl();
        _dashboardGrid = new GridControl();
        _dashboardGrid.Dock = DockStyle.Fill;
        _dashboardGrid.DataSource = new BindingList<DashboardWidget>();
        _dashboardGrid.MainView = new GridView(_dashboardGrid);
        _dashboardGrid.MainView.OptionsBehavior.UseInitializeOnDesiredFocus = true;
        _dashboardGrid.MainView.OptionsSelection.MultiSelect = true;
        _dashboardGrid.MainView.OptionsSelection.MultiSelectMode = SelectionMode.MultiSimple;
        
        // Add report gallery
        _reportGallery = new ReportGalleryPanelControl();
        _reportGallery.Dock = DockStyle.Bottom;
        _reportGallery.Height = 200;
        
        // Add button panel
        var buttonPanel = new PanelControl();
        buttonPanel.Dock = DockStyle.Bottom;
        buttonPanel.Height = 40;
        _buttonPanel = buttonPanel;
        
        // Add navigation bar items
        var mainPanel = new PanelControl();
        var leftPanel = new PanelControl();
        var rightPanel = new PanelControl();
        
        leftPanel.Dock = DockStyle.Left;
        rightPanel.Dock = DockStyle.Right;
        leftPanel.Width = 250;
        rightPanel.Width = 150;
        mainPanel.Dock = DockStyle.Fill;
        mainPanel.Controls.Add(leftPanel);
        mainPanel.Controls.Add(rightPanel);
        mainPanel.Controls.Add(buttonPanel);
        
        // Final setup
        Pages.Add(titlePanel);
        Pages.Add(mainPanel);
        Pages.Add(_reportGallery);
        Pages.Add(_buttonPanel);
    }
    
    public override async Task OnActivatedAsync(CancellationToken cancellationToken = default)
    {
        await LoadDashboardWidgetsAsync();
        await PopulateKeyMetricsAsync();
        await PopulateReportGalleryAsync();
        base.OnActivatedAsync(cancellationToken);
    }
    
    private async Task LoadDashboardWidgetsAsync()
    {
        var widgets = await _dashboardService.GetActiveWidgetsAsync();
        _dashboardGrid.DataSource = new BindingList<DashboardWidget>(widgets);
    }
    
    private async Task PopulateKeyMetricsAsync()
    {
        // Today's Sales
        var salesService = _serviceProvider.GetRequiredService<ISalesService>();
        var todaySales = await salesService.GetTodaySalesAsync();
        _kpiTodaySales.Caption = $"Today's Sales: {todaySales:C}";
        
        // Outstanding Balance
        var outstandingService = _serviceProvider.GetRequiredService<IOutstandingService>();
        var outstanding = await outstandingService.GetCurrentOutstandingAsync();
        _kpiOutstanding.Caption = $"Outstanding Balance: {outstanding:C}";
        
        // Cash Flow
        var cashFlowService = _serviceProvider.GetRequiredService<ICashFlowService>();
        var cashFlow = await cashFlowService.GetDailyCashFlowAsync(DateTime.Today);
        _kpiCashFlow.Caption = $"Today's Cash Flow: {cashFlow:C}";
        
        // Low Stock Items
        var inventoryService = _serviceProvider.GetRequiredService<IInventoryService>();
        var lowStock = await inventoryService.GetLowStockItemsAsync(DateTime.Today);
        _kpiLowStock.Caption = $"Low Stock Items: {lowStock.Count}";
    }
    
    private async Task PopulateReportGalleryAsync()
    {
        // Create report buttons
        _buttonPanel.Controls.Clear();
        
        // Add key report buttons
        var buttons = new List<(string caption, string reportName, Action action)>
        {
            ("Daily Sales Summary", "DailySaleSummary", async () => {
                await _reportService.GenerateDailySaleSummaryAsync(DateTime.Today, CancellationToken.None);
            }),
            ("Sales Invoice", "SaleInvoiceReport", async () => {
                await _reportService.GenerateSaleReportAsync("SaleInvoiceReport", new Dictionary<string, object> { });
            }),
            ("Purchase Invoice", "PurchaseInvoiceReport", async () => {
                await _reportService.GeneratePurchaseReportAsync("PurchaseInvoiceReport", new Dictionary<string, object> { });
            }),
            ("Stock List", "StockListReport", async () => {
                await _reportService.GenerateReportAsync("StockListReport", new Dictionary<string, object> { });
            }),
            ("Cash Flow Report", "CashFlowReport", async () => {
                await _reportService.GenerateReportAsync("CashFlowReport", new Dictionary<string, object> { 
                    { "StartDate", DateTime.Today.AddDays(-30), 
                      "EndDate", DateTime.Today }
                });
            }),
            ("Outstanding Report", "OutstandingReport", async () => {
                await _reportService.GenerateReportAsync("OutstandingReport", new Dictionary<string, object> { 
                    { "LocationId", _userSession.CurrentLocationId },
                    { "AsOfDate", DateTime.Today }
                });
            })
        };
        
        foreach (var (caption, reportName, action) in buttons)
        {
            var button = new ButtonItem();
            button.Caption = caption;
            button.Click += (s, e) => action();
            _buttonPanel.Controls.Add(button);
        }
    }
}