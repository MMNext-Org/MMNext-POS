using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraGrid.Views.Grid;
using Microsoft.Extensions.DependencyInjection;
using MMNextPOS.Application.Services;
using MMNextPOS.Domain.Models;

namespace MMNextPOS.WinForms
{
    /// <summary>
    /// Monthly expense summary report form.
    /// </summary>
    public partial class ExpenseMonthlySummaryForm : Form
    {
        private readonly IExpenseService _expenseService;
        private readonly IServiceProvider _serviceProvider;

        private LabelControl _lblPeriodFrom = null!;
        private LabelControl _lblPeriodTo = null!;
        private DateEdit _dateFromEdit = null!;
        private DateEdit _dateToEdit = null!;
        private SimpleButton _btnGenerate = null!;
        private GridControl _grid = null!;
        private GridView _view = null!;
        private LabelControl _lblTotal = null!;

        public ExpenseMonthlySummaryForm(IExpenseService expenseService, IServiceProvider serviceProvider)
        {
            _expenseService = expenseService ?? throw new ArgumentNullException(nameof(expenseService));
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));

            InitializeComponent();
            InitializeDefaults();
        }

        private void InitializeComponent()
        {
            Text = "Expense Monthly Summary";
            Size = new Size(800, 600);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;

            var mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 4,
                Padding = new Padding(15)
            };
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50)); // Period selector
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40)); // Buttons
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100)); // Grid
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30)); // Total

            // Period selector
            var periodPanel = new PanelControl { Dock = DockStyle.Fill, BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder };
            var periodLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 4,
                RowCount = 1,
                Padding = new Padding(5)
            };
            periodLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
            periodLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
            periodLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
            periodLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));

            _lblPeriodFrom = new LabelControl { Text = "From:", AutoSizeMode = LabelAutoSizeMode.None, Dock = DockStyle.Fill };
            _dateFromEdit = new DateEdit { Dock = DockStyle.Fill };
            _lblPeriodTo = new LabelControl { Text = "To:", AutoSizeMode = LabelAutoSizeMode.None, Dock = DockStyle.Fill };
            _dateToEdit = new DateEdit { Dock = DockStyle.Fill };

            periodLayout.Controls.Add(_lblPeriodFrom, 0, 0);
            periodLayout.Controls.Add(_dateFromEdit, 1, 0);
            periodLayout.Controls.Add(_lblPeriodTo, 2, 0);
            periodLayout.Controls.Add(_dateToEdit, 3, 0);
            periodPanel.Controls.Add(periodLayout);
            mainLayout.Controls.Add(periodPanel, 0, 0);

            // Buttons
            var buttonPanel = new PanelControl { Dock = DockStyle.Fill, BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder };
            _btnGenerate = new SimpleButton { Text = "Generate", Dock = DockStyle.Fill, Location = new Point(10, 5), Width = 100 };
            _btnGenerate.Click += BtnGenerate_Click;
            var btnClose = new SimpleButton { Text = "Close", Dock = DockStyle.Fill, Location = new Point(130, 5), Width = 100 };
            btnClose.Click += (s, e) => this.Close();
            buttonPanel.Controls.Add(_btnGenerate);
            buttonPanel.Controls.Add(btnClose);
            mainLayout.Controls.Add(buttonPanel, 0, 1);

            // Grid
            _grid = new GridControl { Dock = DockStyle.Fill };
            _view = new GridView(_grid) { OptionsSelection = { MultiSelect = false }, OptionsView = { ShowGroupPanel = false } };
            _grid.MainView = _view;
            mainLayout.Controls.Add(_grid, 0, 2);

            // Total
            _lblTotal = new LabelControl
            {
                Dock = DockStyle.Fill,
                Text = "Total: $0.00",
                AutoSizeMode = LabelAutoSizeMode.None,
                Font = new System.Drawing.Font("Segoe UI", 12, FontStyle.Bold),
                Appearance = { TextOptions = { HAlignment = DevExpress.Utils.HorzAlignment.Far } }
            };
            mainLayout.Controls.Add(_lblTotal, 0, 3);

            Controls.Add(mainLayout);
        }

        private void InitializeDefaults()
        {
            // Set default period to current month
            _dateFromEdit.EditValue = DateTime.Now.AddDays(-(DateTime.Now.Day - 1)); // Start of current month
            _dateToEdit.EditValue = DateTime.Now; // End of current month
        }

        private void BtnGenerate_Click(object? sender, EventArgs e)
        {
            GenerateSummary();
        }

        private async void GenerateSummary()
        {
            try
            {
                var fromDate = _dateFromEdit.DateTime;
                var toDate = _dateToEdit.DateTime;

                if (fromDate > toDate)
                {
                    ShowError("From date must be before To date.");
                    return;
                }

                var expenses = await _expenseService.GetByDateRangeAsync(fromDate, toDate, CancellationToken.None);

                // Group by month and payment type
                var grouped = expenses
                    .GroupBy(e => new { e.ExpenseDate.Year, e.ExpenseDate.Month })
                    .Select(g => new
                    {
                        MonthYear = $"{g.Key.Month:D2}/{g.Key.Year}",
                        TotalAmount = g.Sum(e => e.Amount),
                        Count = g.Count(),
                        ByType = g.GroupBy(e => e.ExpenseTypeId > 0 ? $"Type {e.ExpenseTypeId}" : "Uncategorized")
                            .ToDictionary(
                                fg => fg.Key,
                                fg => fg.Sum(e => e.Amount)
                            )
                    })
                    .OrderBy(g => g.MonthYear)
                    .ToList();

                // Bind to grid
                _grid.DataSource = grouped;

                // Format columns
                _view.Columns.Clear();
                _view.Columns.AddRange(new[]
                {
                    new GridColumn { FieldName = "MonthYear", Caption = "Month", Width = 100, Visible = true },
                    new GridColumn { FieldName = "TotalAmount", Caption = "Total", Width = 120, Visible = true, DisplayFormat = { FormatString = "c2", FormatType = DevExpress.Utils.FormatType.Numeric } },
                    new GridColumn { FieldName = "Count", Caption = "Count", Width = 70, Visible = true, DisplayFormat = { FormatString = "n0", FormatType = DevExpress.Utils.FormatType.Numeric } }
                });

                // Calculate total
                decimal grandTotal = grouped.Sum(g => g.TotalAmount);
                _lblTotal.Text = $"Total: {grandTotal:C2}";
            }
            catch (Exception ex)
            {
                ShowError($"Failed to generate summary: {ex.Message}");
            }
        }

        // Helper methods mimicking AsyncFormBase
        private void ShowInfo(string message) => MessageBox.Show(message, "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
        private void ShowError(string message) => MessageBox.Show(message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
    }
}
