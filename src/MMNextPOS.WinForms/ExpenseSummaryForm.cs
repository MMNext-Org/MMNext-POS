using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using DevExpress.XtraCharts;
using DevExpress.XtraEditors;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraGrid.Views.Grid;
using MMNextPOS.Application.Services;
using MMNextPOS.Domain.Models;

namespace MMNextPOS.WinForms
{
    public class ExpenseSummaryForm : AsyncFormBase
    {
        private readonly IExpenseSummaryService _summaryService;

        private ComboBoxEdit _yearCombo = null!;
        private LookUpEdit _monthCombo = null!;
        private SimpleButton _refreshButton = null!;
        private ChartControl _chart = null!;
        private GridControl _categoryGrid = null!;
        private GridView _categoryView = null!;
        private LabelControl _totalLabel = null!;
        private LabelControl _countLabel = null!;
        private LabelControl _avgLabel = null!;

        public ExpenseSummaryForm(IExpenseSummaryService summaryService)
        {
            _summaryService = summaryService ?? throw new ArgumentNullException(nameof(summaryService));
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            Text = "Monthly Expense Summary";
            Size = new Size(1100, 750);
            MinimumSize = new Size(900, 600);
            StartPosition = FormStartPosition.CenterParent;

            var mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3,
                Padding = new Padding(15)
            };
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 60));  // Header/Controls
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 55));    // Chart
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 45));    // Grid

            // Header Panel with Controls
            var headerPanel = new PanelControl { Dock = DockStyle.Fill, BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder };
            var headerLayout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 6, Padding = new Padding(5) };
            headerLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 80));
            headerLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
            headerLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 80));
            headerLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150));
            headerLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            headerLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));

            headerLayout.Controls.Add(new LabelControl { Text = "Year:", Dock = DockStyle.Fill, Padding = new Padding(0, 8, 10, 0), AutoSizeMode = LabelAutoSizeMode.None }, 0, 0);

            _yearCombo = new ComboBoxEdit
            {
                Dock = DockStyle.Fill,
                Properties = { TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor }
            };
            var currentYear = DateTime.Today.Year;
            for (int y = currentYear - 5; y <= currentYear + 1; y++)
            {
                _yearCombo.Properties.Items.Add(y);
            }
            _yearCombo.SelectedItem = currentYear;
            _yearCombo.EditValueChanged += (_, _) =>
            {
#pragma warning disable CS4014
                RefreshSummary();
#pragma warning restore CS4014
            };
            headerLayout.Controls.Add(_yearCombo, 1, 0);

            headerLayout.Controls.Add(new LabelControl { Text = "Month:", Dock = DockStyle.Fill, Padding = new Padding(10, 8, 10, 0), AutoSizeMode = LabelAutoSizeMode.None }, 2, 0);

            _monthCombo = new LookUpEdit
            {
                Dock = DockStyle.Fill,
                Properties =
                {
                    DisplayMember = "Name",
                    ValueMember = "Month",
                    NullText = "Select month...",
                    ShowHeader = false,
                    TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor
                }
            };
            var monthItems = new List<MonthItem>();
            for (int m = 1; m <= 12; m++)
            {
                monthItems.Add(new MonthItem { Month = m, Name = System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(m) });
            }
            _monthCombo.Properties.DataSource = monthItems;
            _monthCombo.EditValue = DateTime.Today.Month;
            _monthCombo.EditValueChanged += (_, _) =>
            {
#pragma warning disable CS4014
                RefreshSummary();
#pragma warning restore CS4014
            };
            headerLayout.Controls.Add(_monthCombo, 3, 0);

            // Summary Labels
            _totalLabel = new LabelControl { Text = "Total: 0.00", Dock = DockStyle.Fill, Padding = new Padding(10, 8, 0, 0), Appearance = { Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = Color.DarkBlue }, AutoSizeMode = LabelAutoSizeMode.None };
            headerLayout.Controls.Add(_totalLabel, 4, 0);

            _refreshButton = new SimpleButton
            {
                Text = "Refresh",
                Dock = DockStyle.Fill,
                Height = 35,
                Margin = new Padding(10, 10, 0, 0)
            };
            _refreshButton.Click += async (_, _) => await RefreshSummary();
            headerLayout.Controls.Add(_refreshButton, 5, 0);

            headerPanel.Controls.Add(headerLayout);
            mainLayout.Controls.Add(headerPanel, 0, 0);

            // Chart Panel
            var chartPanel = new PanelControl { Dock = DockStyle.Fill };
            _chart = new ChartControl { Dock = DockStyle.Fill };
            SetupChart();
            chartPanel.Controls.Add(_chart);
            mainLayout.Controls.Add(chartPanel, 0, 1);

            // Category Grid Panel
            var gridPanel = new PanelControl { Dock = DockStyle.Fill };
            var gridLayout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2 };
            gridLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 35));
            gridLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            var gridHeaderLayout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 4, Padding = new Padding(5) };
            gridHeaderLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
            gridHeaderLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
            gridHeaderLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
            gridHeaderLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));

            _countLabel = new LabelControl { Text = "Transactions: 0", Dock = DockStyle.Fill, Padding = new Padding(5, 5, 0, 0), AutoSizeMode = LabelAutoSizeMode.None };
            _avgLabel = new LabelControl { Text = "Avg/Transaction: 0.00", Dock = DockStyle.Fill, Padding = new Padding(5, 5, 0, 0), AutoSizeMode = LabelAutoSizeMode.None };
            var exportButton = new SimpleButton { Text = "Export to CSV", Dock = DockStyle.Fill, Height = 30 };
            exportButton.Click += OnExportCsv;

            gridHeaderLayout.Controls.Add(_countLabel, 0, 0);
            gridHeaderLayout.Controls.Add(_avgLabel, 1, 0);
            gridHeaderLayout.Controls.Add(new LabelControl(), 2, 0);
            gridHeaderLayout.Controls.Add(exportButton, 3, 0);

            gridLayout.Controls.Add(gridHeaderLayout, 0, 0);

            _categoryGrid = new GridControl { Dock = DockStyle.Fill };
            _categoryView = new GridView(_categoryGrid)
            {
                OptionsBehavior = { ReadOnly = true },
                OptionsSelection = { MultiSelect = false },
                OptionsView = { ShowGroupPanel = false, EnableAppearanceEvenRow = true, EnableAppearanceOddRow = true },
                GridControl = _categoryGrid
            };
            _categoryGrid.MainView = _categoryView;
            _categoryGrid.ViewCollection.Add(_categoryView);
            SetupCategoryGrid();

            gridLayout.Controls.Add(_categoryGrid, 0, 1);
            gridPanel.Controls.Add(gridLayout);
            mainLayout.Controls.Add(gridPanel, 0, 2);

            Controls.Add(mainLayout);
            Load += OnFormLoad;
        }

        private class MonthItem
        {
            public int Month { get; set; }
            public string Name { get; set; } = string.Empty;
        }

        private void SetupChart()
        {
            _chart.Titles.Clear();
            var title = new ChartTitle { Text = "Monthly Expense Trend", Font = new Font("Segoe UI", 14, FontStyle.Bold), Alignment = StringAlignment.Center };
            _chart.Titles.Add(title);

            _chart.Legend.Visibility = DevExpress.Utils.DefaultBoolean.True;
            _chart.Legend.AlignmentHorizontal = LegendAlignmentHorizontal.Center;
            _chart.Legend.AlignmentVertical = LegendAlignmentVertical.BottomOutside;

            var diagram = (XYDiagram)_chart.Diagram;
            diagram.AxisX.Title.Text = "Month";
            diagram.AxisX.Title.Visibility = DevExpress.Utils.DefaultBoolean.True;
            diagram.AxisX.Label.Angle = -45;
            diagram.AxisY.Title.Text = "Amount";
            diagram.AxisY.Title.Visibility = DevExpress.Utils.DefaultBoolean.True;
            diagram.AxisY.Label.TextPattern = "{V:N0}";
            diagram.EnableAxisXZooming = true;
            diagram.EnableAxisYZooming = true;
        }

        private void SetupCategoryGrid()
        {
            _categoryView.Columns.Clear();

            _categoryView.Columns.Add(new GridColumn
            {
                FieldName = "ExpenseTypeName",
                Caption = "Category",
                Width = 200,
                Visible = true,
                OptionsColumn = { ReadOnly = true }
            });

            _categoryView.Columns.Add(new GridColumn
            {
                FieldName = "Count",
                Caption = "Count",
                Width = 80,
                Visible = true,
                OptionsColumn = { ReadOnly = true },
                AppearanceCell = { TextOptions = { HAlignment = DevExpress.Utils.HorzAlignment.Center } }
            });

            var amountCol = new GridColumn
            {
                FieldName = "Amount",
                Caption = "Amount",
                Width = 130,
                Visible = true,
                OptionsColumn = { ReadOnly = true },
                DisplayFormat = { FormatString = "N2", FormatType = DevExpress.Utils.FormatType.Numeric },
                AppearanceCell = { TextOptions = { HAlignment = DevExpress.Utils.HorzAlignment.Far } }
            };
            _categoryView.Columns.Add(amountCol);

            var pctCol = new GridColumn
            {
                FieldName = "Percentage",
                Caption = "% of Total",
                Width = 100,
                Visible = true,
                OptionsColumn = { ReadOnly = true },
                DisplayFormat = { FormatString = "N2", FormatType = DevExpress.Utils.FormatType.Numeric },
                AppearanceCell = { TextOptions = { HAlignment = DevExpress.Utils.HorzAlignment.Far } }
            };
            _categoryView.Columns.Add(pctCol);

            _categoryView.BestFitColumns();
        }

        private async void OnFormLoad(object? sender, EventArgs e)
        {
            await RefreshSummary();
        }

        private async Task RefreshSummary()
        {
            if (_yearCombo.SelectedItem == null || _monthCombo.EditValue == null) return;

            int year = (int)_yearCombo.SelectedItem;
            int month = (int)_monthCombo.EditValue;

            try
            {
                SetWaitCursor(true);

                // Get monthly summary
                var summary = await _summaryService.GetMonthlySummaryAsync(year, month, CancellationToken).ConfigureAwait(false);

                // Update summary labels
                _totalLabel.Text = $"Total: {summary.TotalAmount:N2}";
                _countLabel.Text = $"Transactions: {summary.TransactionCount}";
                _avgLabel.Text = summary.TransactionCount > 0
                    ? $"Avg/Transaction: {summary.TotalAmount / summary.TransactionCount:N2}"
                    : "Avg/Transaction: 0.00";

                // Update category grid
                _categoryGrid.DataSource = summary.Categories.ToList();
                _categoryView.BestFitColumns();

                // Update chart with yearly data
                await UpdateYearlyChart(year);
            }
            catch (Exception ex)
            {
                ShowError($"Failed to load expense summary: {ex.Message}");
            }
            finally
            {
                SetWaitCursor(false);
            }
        }

        private async Task UpdateYearlyChart(int year)
        {
            try
            {
                var yearlyData = await _summaryService.GetYearlySummaryAsync(year, CancellationToken).ConfigureAwait(false);

                _chart.Series.Clear();

                // Total Amount Series (Bar)
                var totalSeries = new Series("Total Amount", ViewType.Bar);
                totalSeries.Label.TextPattern = "{V:N0}";
                totalSeries.LabelsVisibility = DevExpress.Utils.DefaultBoolean.True;

                // Transaction Count Series (Line on secondary axis)
                var countSeries = new Series("Transaction Count", ViewType.Line);
                var lineView = (LineSeriesView)countSeries.View;
                lineView.MarkerVisibility = DevExpress.Utils.DefaultBoolean.True;
                lineView.LineMarkerOptions.Size = 8;
                countSeries.LabelsVisibility = DevExpress.Utils.DefaultBoolean.False;

                // Manually add data points to both series
                foreach (var monthData in yearlyData)
                {
                    totalSeries.Points.Add(new SeriesPoint(monthData.MonthName, monthData.TotalAmount));
                    countSeries.Points.Add(new SeriesPoint(monthData.MonthName, monthData.TransactionCount));
                }

                _chart.Series.Add(totalSeries);
                _chart.Series.Add(countSeries);

                // Setup secondary axis for count
                var secondaryAxisY = new SecondaryAxisY("Count Axis");
                secondaryAxisY.Title.Text = "Transaction Count";
                secondaryAxisY.Title.Visibility = DevExpress.Utils.DefaultBoolean.True;
                ((XYDiagram)_chart.Diagram).SecondaryAxesY.Add(secondaryAxisY);
                ((LineSeriesView)countSeries.View).AxisY = secondaryAxisY;
            }
            catch (Exception ex)
            {
                // Don't show error for chart - it's secondary
                System.Diagnostics.Debug.WriteLine($"Chart update failed: {ex.Message}");
            }
        }

        private void OnExportCsv(object? sender, EventArgs e)
        {
            if (_categoryView.DataRowCount == 0)
            {
                ShowInfo("No data to export.");
                return;
            }

            using var saveDialog = new SaveFileDialog
            {
                Filter = "CSV Files (*.csv)|*.csv",
                FileName = $"ExpenseSummary_{DateTime.Today:yyyyMMdd}.csv"
            };

            if (saveDialog.ShowDialog(this) != DialogResult.OK) return;

            try
            {
                var lines = new List<string>
                {
                    "Category,Count,Amount,Percentage"
                };

                for (int i = 0; i < _categoryView.DataRowCount; i++)
                {
                    var row = _categoryView.GetDataRow(i);
                    if (row == null) continue;

                    var category = row["ExpenseTypeName"]?.ToString() ?? "";
                    var count = row["Count"]?.ToString() ?? "0";
                    var amount = row["Amount"]?.ToString() ?? "0";
                    var pct = row["Percentage"]?.ToString() ?? "0";

                    lines.Add($"\"{category}\",{count},{amount},{pct}");
                }

                System.IO.File.WriteAllLines(saveDialog.FileName, lines);
                ShowInfo($"Exported to {saveDialog.FileName}");
            }
            catch (Exception ex)
            {
                ShowError($"Export failed: {ex.Message}");
            }
        }
    }
}
