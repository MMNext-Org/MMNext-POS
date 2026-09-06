using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using DevExpress.XtraPrinting;
using DevExpress.XtraReports.UI;
using Microsoft.Extensions.DependencyInjection;
using MMNextPOS.Application.Services;
using MMNextPOS.Domain.Models;
using MMNextPOS.Infrastructure.Repositories;
using MMNextPOS.WinForms.Services;
using MMNextPOS.WinForms.Reports;

namespace MMNextPOS.WinForms
{
    /// <summary>
    /// Reports viewer form with DevExpress XtraReport preview and print capabilities.
    /// </summary>
    public partial class ReportsViewerForm : XtraForm
    {
        private readonly WinFormsReportService _reportService;
        private readonly IServiceProvider _serviceProvider;
        private XtraReport _currentReport = null!;

        private ComboBoxEdit _reportSelector = null!;
        private DevExpress.XtraPrinting.Preview.DocumentViewer _documentViewer = null!;
        private SimpleButton _printButton = null!;
        private SimpleButton _exportPdfButton = null!;
        private SimpleButton _exportExcelButton = null!;
        private SimpleButton _refreshButton = null!;
        private PanelControl _toolbarPanel = null!;

        public ReportsViewerForm(WinFormsReportService reportService, IServiceProvider serviceProvider)
        {
            _reportService = reportService ?? throw new ArgumentNullException(nameof(reportService));
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            InitializeComponent();
            LoadReports();
        }

        private void InitializeComponent()
        {
            Text = "Reports Viewer";
            Size = new Size(1200, 800);
            StartPosition = FormStartPosition.CenterParent;

            var mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3,
                Padding = new Padding(10)
            };
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 60));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));

            // Toolbar
            _toolbarPanel = new PanelControl
            {
                Dock = DockStyle.Fill,
                BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder
            };

            var reportLabel = new LabelControl { Text = "Report:", Location = new Point(10, 18), AutoSizeMode = LabelAutoSizeMode.None, Size = new Size(60, 25) };
            _reportSelector = new ComboBoxEdit
            {
                Location = new Point(80, 15),
                Width = 300,
                Properties = { TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor }
            };
            _reportSelector.SelectedIndexChanged += (s, e) => LoadSelectedReport();

            _refreshButton = new SimpleButton { Text = "Refresh", Location = new Point(400, 15), Width = 80, Height = 30 };
            _refreshButton.Click += (s, e) => LoadSelectedReport();

            _printButton = new SimpleButton { Text = "Print", Location = new Point(500, 15), Width = 80, Height = 30 };
            _printButton.Click += (s, e) => PrintReport();

            _exportPdfButton = new SimpleButton { Text = "Export PDF", Location = new Point(590, 15), Width = 100, Height = 30 };
            _exportPdfButton.Click += (s, e) => ExportReport("PDF");

            _exportExcelButton = new SimpleButton { Text = "Export Excel", Location = new Point(700, 15), Width = 100, Height = 30 };
            _exportExcelButton.Click += (s, e) => ExportReport("Excel");

            _toolbarPanel.Controls.Add(reportLabel);
            _toolbarPanel.Controls.Add(_reportSelector);
            _toolbarPanel.Controls.Add(_refreshButton);
            _toolbarPanel.Controls.Add(_printButton);
            _toolbarPanel.Controls.Add(_exportPdfButton);
            _toolbarPanel.Controls.Add(_exportExcelButton);

            // Document Viewer
            _documentViewer = new DevExpress.XtraPrinting.Preview.DocumentViewer
            {
                Dock = DockStyle.Fill
            };

            // Status bar
            var statusPanel = new PanelControl
            {
                Dock = DockStyle.Fill,
                BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder
            };
            var statusLabel = new LabelControl
            {
                Text = "Ready",
                Location = new Point(10, 10),
                AutoSizeMode = LabelAutoSizeMode.None,
                Size = new Size(400, 25)
            };
            statusPanel.Controls.Add(statusLabel);

            mainLayout.Controls.Add(_toolbarPanel, 0, 0);
            mainLayout.Controls.Add(_documentViewer, 0, 1);
            mainLayout.Controls.Add(statusPanel, 0, 2);

            Controls.Add(mainLayout);
        }

        private async void LoadReports()
        {
            try
            {
                var menus = await _reportService.GetReportMenusAsync(true);
                _reportSelector.Properties.Items.Clear();
                foreach (var menu in menus)
                {
                    _reportSelector.Properties.Items.Add(new ReportMenuItem(menu));
                }
                if (_reportSelector.Properties.Items.Count > 0)
                    _reportSelector.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show(this, $"Failed to load reports: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

private async void LoadSelectedReport()
        {
            if (_reportSelector.SelectedItem is not ReportMenuItem item) return;

            try
            {
                // Get parameters based on report type
                var parameters = await GetReportParametersAsync(item.Menu.Code);
                if (parameters == null)
                {
                    // User cancelled parameter form
                    return;
                }

                var reportBytes = await _reportService.GenerateReportAsync(item.Menu.Code, parameters);

                using var stream = new MemoryStream(reportBytes);
                _currentReport = new XtraReport();
                _currentReport.LoadLayout(stream);

                _documentViewer.DocumentSource = _currentReport;
                _currentReport.CreateDocument();
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show(this, $"Failed to load report: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task<Dictionary<string, object>?> GetReportParametersAsync(string reportCode)
        {
            // Map report codes to parameter forms
            return reportCode switch
            {
                // Date range reports
                "PROFIT_LOSS" or "CASH_FLOW" or "OUTSTANDING" or "SALE_HISTORY" or "STOCK_MOVEMENT" 
                    => await ShowDateRangeParameterFormAsync(),

                // Single entity reports
                "SALE_RECEIPT" or "SALE_INVOICE" 
                    => await ShowEntityParameterFormAsync("Sale"),

                "PURCHASE_INVOICE" 
                    => await ShowEntityParameterFormAsync("Purchase"),

                // Reports with no parameters
                "STOCK_LIST" or "BARCODE_LABELS" 
                    => new Dictionary<string, object>(),

                _ => new Dictionary<string, object>()
            };
        }

        private async Task<Dictionary<string, object>?> ShowDateRangeParameterFormAsync()
        {
            using var form = _serviceProvider.GetRequiredService<DateRangeParameterForm>();
            var result = form.ShowDialog(this);
            if (result == DialogResult.OK && form.IsValid)
            {
                return form.GetParameters();
            }
            return null;
        }

        private async Task<Dictionary<string, object>?> ShowEntityParameterFormAsync(string entityType)
        {
            using var form = _serviceProvider.GetRequiredService<EntityParameterForm>();
            // The form title will be set in constructor
            var result = form.ShowDialog(this);
            if (result == DialogResult.OK && form.IsValid)
            {
                return form.GetParameters();
            }
            return null;
        }

        private void PrintReport()
        {
            if (_currentReport == null) return;
            _currentReport.PrintDialog();
        }

        private void ExportReport(string format)
        {
            if (_currentReport == null) return;

            using var dialog = new SaveFileDialog
            {
                Filter = format == "PDF" ? "PDF Files (*.pdf)|*.pdf" : "Excel Files (*.xlsx)|*.xlsx",
                FileName = $"{_currentReport.DisplayName}_{DateTime.Now:yyyyMMdd}.{format.ToLower()}"
            };

            if (dialog.ShowDialog(this) != DialogResult.OK) return;

            try
            {
                if (format == "PDF")
                    _currentReport.ExportToPdf(dialog.FileName);
                else
                    _currentReport.ExportToXlsx(dialog.FileName);

                XtraMessageBox.Show(this, $"Report exported to {dialog.FileName}", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show(this, $"Export failed: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private class ReportMenuItem
        {
            public ReportMenus Menu { get; }
            public ReportMenuItem(ReportMenus menu) { Menu = menu; }
            public override string ToString() => $"{Menu.Name} ({Menu.Code})";
        }
    }
}