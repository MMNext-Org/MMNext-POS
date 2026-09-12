using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
    /// List page for Sales entity using the generic ListPage base class.
    /// </summary>
    public partial class SalesListPage : ListPage<Sale, ISalesService>
    {
        private readonly ISaleTempService _saleTempService;
        private SimpleButton _resumeDraftButton = null!;
        private SimpleButton _voidSaleButton = null!;

        public SalesListPage(
            ISalesService service,
            ISaleTempService saleTempService,
            IServiceProvider serviceProvider)
            : base(service, serviceProvider)
        {
            _saleTempService = saleTempService ?? throw new ArgumentNullException(nameof(saleTempService));
        }

        protected override string GetPageTitle() => "Sales";

        protected override void InitializeComponent()
        {
            base.InitializeComponent();

            // Add Resume Draft button to toolbar
            _resumeDraftButton = new SimpleButton
            {
                Text = "Resume Draft",
                Location = new Point(800, 10),
                Width = 110,
                Height = 30
            };
            _resumeDraftButton.Click += async (s, e) => await OnResumeDraftAsync();
            _resumeDraftButton.ToolTip = "Resume a saved draft";

            // Add Void Sale button to toolbar
            _voidSaleButton = new SimpleButton
            {
                Text = "Void Sale",
                Location = new Point(920, 10),
                Width = 100,
                Height = 30
            };
            _voidSaleButton.Click += async (s, e) => await OnVoidSaleAsync();
            _voidSaleButton.ToolTip = "Void a completed sale";

            // Add buttons to toolbar
            var toolbar = Controls.OfType<PanelControl>().FirstOrDefault(p => p.Dock == DockStyle.Top);
            toolbar?.Controls.Add(_resumeDraftButton);
            toolbar?.Controls.Add(_voidSaleButton);
        }

        protected override void ConfigureColumns(GridView view)
        {
            view.Columns.Clear();
            view.Columns.AddRange(new[]
            {
                new GridColumn { FieldName = "Id", Caption = "Sale #", Width = 80, Visible = true },
                new GridColumn { FieldName = "CustomerName", Caption = "Customer", Width = 200, Visible = true },
                new GridColumn { FieldName = "SaleDate", Caption = "Date", Width = 150, Visible = true },
                new GridColumn { FieldName = "TotalAmount", Caption = "Total", Width = 120, Visible = true, DisplayFormat = { FormatString = "c2" } },
                new GridColumn { FieldName = "Status", Caption = "Status", Width = 120, Visible = true }
            });
        }

        protected override async Task<IEnumerable<Sale>> GetItemsAsync(CancellationToken cancellationToken)
        {
            var sales = await _service.GetAllAsync();

            // Apply date from filter
            if (_dateFromEdit?.EditValue != null)
            {
                var fromDate = (DateTime)_dateFromEdit.EditValue;
                sales = sales.Where(s => s.SaleDate >= fromDate).ToList();
            }

            // Apply date to filter
            if (_dateToEdit?.EditValue != null)
            {
                var toDate = (DateTime)_dateToEdit.EditValue;
                sales = sales.Where(s => s.SaleDate < toDate.Date.AddDays(1)).ToList();
            }

            // Apply status filter
            if (!string.IsNullOrWhiteSpace(_statusFilter?.EditValue?.ToString()))
            {
                var status = _statusFilter.EditValue.ToString();
                sales = sales.Where(s => s.Status == status).ToList();
            }

            // Return most recent 100 after filtering
            return sales.OrderByDescending(s => s.SaleDate).Take(100);
        }

        protected override async Task OnEditAsync(Sale entity)
        {
            // Edit handled by derived forms
        }

        protected override async Task DeleteAsync(int id, CancellationToken cancellationToken)
        {
            // Delete handled by derived forms
        }

        private async Task OnResumeDraftAsync()
        {
            try
            {
                SetWaitCursor(true);

                // Load all drafts
                var drafts = await _saleTempService.GetAllAsync(CancellationToken);
                var draftList = drafts.Where(d => d.Status == "Draft").ToList();

                if (!draftList.Any())
                {
                    ShowInfo("No drafts available to resume.");
                    return;
                }

                // Show draft selection dialog
                using var selector = new DraftSelectorForm(draftList);
                if (selector.ShowDialog(this) != DialogResult.OK || selector.SelectedDraft == null)
                    return;

                // Open NewSaleForm with selected draft
                var newSaleForm = _serviceProvider.GetRequiredService<NewSaleForm>();
                newSaleForm.SetDraft(selector.SelectedDraft);
                if (newSaleForm.ShowDialog(this) == DialogResult.OK)
                {
                    // Refresh the list after completing the sale
                    await LoadAsync();
                }
            }
            catch (Exception ex)
            {
                ShowError($"Failed to resume draft: {ex.Message}");
            }
            finally
            {
                SetWaitCursor(false);
            }
        }

        private async Task OnVoidSaleAsync()
        {
            try
            {
                SetWaitCursor(true);

                // Get selected sale
                if (_gridView.FocusedRowHandle < 0)
                {
                    ShowInfo("Please select a sale to void.");
                    return;
                }

                var sale = _gridView.GetRow(_gridView.FocusedRowHandle) as Sale;
                if (sale == null)
                {
                    ShowError("Failed to get selected sale.");
                    return;
                }

                // Check if sale can be voided
                if (sale.Status == "Voided")
                {
                    ShowInfo("This sale is already voided.");
                    return;
                }

                if (sale.Status == "Returned" || sale.Status == "PartiallyReturned")
                {
                    ShowInfo($"Cannot void a {sale.Status?.ToLower() ?? "returned"} sale. Process a return reversal instead.");
                    return;
                }

                var reason = Microsoft.VisualBasic.Interaction.InputBox(
                    "Enter reason for voiding this sale:",
                    "Void Sale",
                    "Customer request");

                if (string.IsNullOrWhiteSpace(reason))
                {
                    ShowInfo("Void cancelled - no reason provided.");
                    return;
                }

                if (!ShowConfirm($"Are you sure you want to void sale #{sale.Id}?\n\nReason: {reason}"))
                {
                    return;
                }

                try
                {
                    SetWaitCursor(true);

                    // Call the void service
                    var voidedSale = await _service.VoidSaleAsync(sale.Id, reason, CancellationToken);

                    ShowInfo($"Sale #{voidedSale.Id} voided successfully.");

                    // Refresh the list
                    await LoadAsync();
                }
                catch (Exception ex)
                {
                    ShowError($"Failed to void sale: {ex.Message}");
                }
                finally
                {
                    SetWaitCursor(false);
                }
            }
            catch (Exception ex)
            {
                ShowError($"Failed to void sale: {ex.Message}");
            }
            finally
            {
                SetWaitCursor(false);
            }
        }

        private async Task ApplyFilterAsync()
        {
            try
            {
                SetWaitCursor(true);
                await LoadAsync();
            }
            catch (Exception ex)
            {
                ShowError($"Failed to apply filter: {ex.Message}");
            }
            finally
            {
                SetWaitCursor(false);
            }
        }

        private async Task ExportSalesAsync()
        {
            try
            {
                SetWaitCursor(true);

                var sales = await _service.GetAllAsync();

                // Apply filters
                if (_dateFromEdit.EditValue != null)
                {
                    var fromDate = (DateTime)_dateFromEdit.EditValue;
                    sales = sales.Where(s => s.SaleDate >= fromDate).ToList();
                }

                if (_dateToEdit.EditValue != null)
                {
                    var toDate = (DateTime)_dateToEdit.EditValue;
                    sales = sales.Where(s => s.SaleDate < toDate.Date.AddDays(1)).ToList();
                }

                if (!string.IsNullOrWhiteSpace(_statusFilter.EditValue?.ToString()))
                {
                    var status = _statusFilter.EditValue.ToString();
                    sales = sales.Where(s => s.Status == status).ToList();
                }

                // Generate CSV
                var csv = new System.Text.StringBuilder();
                csv.AppendLine("Sale #,Date,Customer,Total,Status");

                foreach (var sale in sales.OrderByDescending(s => s.SaleDate))
                {
                    csv.AppendLine($"{sale.Id},{sale.SaleDate:yyyy-MM-dd HH:mm},{sale.CustomerName ?? "Walk-in"},{sale.TotalAmount:C2},{sale.Status}");
                }

                // Save to file
                var saveDialog = new SaveFileDialog
                {
                    Filter = "CSV Files|*.csv",
                    FileName = $"Sales_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
                };

                if (saveDialog.ShowDialog(this) == DialogResult.OK)
                {
                    await System.IO.File.WriteAllTextAsync(saveDialog.FileName, csv.ToString());
                    ShowInfo($"Exported {sales.Count} sales to {saveDialog.FileName}");
                }
            }
            catch (Exception ex)
            {
                ShowError($"Failed to export: {ex.Message}");
            }
            finally
            {
                SetWaitCursor(false);
            }
        }

        private DevExpress.Utils.Svg.SvgImage? GetResumeDraftIcon()
        {
            return null;
        }
    }

    /// <summary>
    /// Dialog for selecting a draft to resume.
    /// </summary>
    public class DraftSelectorForm : XtraForm
    {
        public SaleTemp? SelectedDraft { get; private set; }

        private readonly DevExpress.XtraGrid.GridControl _grid = new();
        private readonly GridView _view = new();
        private readonly SimpleButton _okButton = new();
        private readonly SimpleButton _cancelButton = new();

        public DraftSelectorForm(IEnumerable<SaleTemp> drafts)
        {
            this.Text = "Select Draft to Resume";
            this.Size = new Size(600, 400);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.Sizable;

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                Padding = new Padding(10)
            };
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));

            _grid.Dock = DockStyle.Fill;
            _view.GridControl = _grid;
            _view.OptionsBehavior.Editable = false;
            _view.OptionsSelection.MultiSelect = false;
            _view.Columns.AddRange(new[]
            {
                new DevExpress.XtraGrid.Columns.GridColumn { FieldName = "Id", Caption = "Draft #", Width = 80 },
                new DevExpress.XtraGrid.Columns.GridColumn { FieldName = "CustomerName", Caption = "Customer", Width = 200 },
                new DevExpress.XtraGrid.Columns.GridColumn { FieldName = "SaleDate", Caption = "Date", Width = 150 },
                new DevExpress.XtraGrid.Columns.GridColumn { FieldName = "TotalAmount", Caption = "Total", Width = 120, DisplayFormat = { FormatString = "c2" } },
                new DevExpress.XtraGrid.Columns.GridColumn { FieldName = "Status", Caption = "Status", Width = 100 }
            });
            _grid.DataSource = drafts.ToList();
            _grid.MainView = _view;
            _grid.ViewCollection.Add(_view);
            _view.DoubleClick += (s, e) => { if (_view.FocusedRowHandle >= 0) AcceptSelection(); };

            var buttonPanel = new PanelControl { Dock = DockStyle.Fill, BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder };
            _okButton.Text = "Resume";
            _okButton.Location = new Point(10, 10);
            _okButton.Width = 100;
            _okButton.Height = 30;
            _okButton.Click += (s, e) => AcceptSelection();

            _cancelButton.Text = "Cancel";
            _cancelButton.Location = new Point(120, 10);
            _cancelButton.Width = 100;
            _cancelButton.Height = 30;
            _cancelButton.Click += (s, e) => this.DialogResult = DialogResult.Cancel;

            buttonPanel.Controls.Add(_okButton);
            buttonPanel.Controls.Add(_cancelButton);

            layout.Controls.Add(_grid, 0, 0);
            layout.Controls.Add(buttonPanel, 0, 1);
            this.Controls.Add(layout);
        }

        private void AcceptSelection()
        {
            var row = _view.GetRow(_view.FocusedRowHandle) as SaleTemp;
            if (row != null)
            {
                SelectedDraft = row;
                this.DialogResult = DialogResult.OK;
            }
        }
    }
}
