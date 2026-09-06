using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Controls;
using DevExpress.XtraEditors.Repository;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraGrid.Views.Grid;
using MMNextPOS.Application.Services;
using MMNextPOS.Domain.Models;

namespace MMNextPOS.WinForms
{
    /// <summary>
    /// Read-only display form for active/live sale drafts.
    /// Shows the current sale details without allowing edits.
    /// </summary>
    public class LiveSaleForm : AsyncFormBase
    {
        private readonly ISaleTempService _saleTempService;
        private readonly ISalesService _salesService;
        private readonly IProductService _productService;
        private readonly ICustomerService _customerService;

        // UI Controls
        private LookUpEdit _customerLookup = null!;
        private DevExpress.XtraGrid.GridControl _detailsGrid = null!;
        private GridView _detailsView = null!;
        private LabelControl _totalLabel = null!;
        private SimpleButton _printButton = null!;
        private BindingList<SaleDetailViewModel> _lineItems = new();

        // Cache for products
        private List<Product> _allProducts = new();

        // Current draft being displayed
        private SaleTemp? _currentDraft = null;

        public LiveSaleForm(
            ISaleTempService saleTempService,
            ISalesService salesService,
            IProductService productService,
            ICustomerService customerService)
        {
            _saleTempService = saleTempService ?? throw new ArgumentNullException(nameof(saleTempService));
            _salesService = salesService ?? throw new ArgumentNullException(nameof(salesService));
            _productService = productService ?? throw new ArgumentNullException(nameof(productService));
            _customerService = customerService ?? throw new ArgumentNullException(nameof(customerService));

            InitializeComponent();
            this.Load += async (s, e) => await LoadReferenceDataAsync();
        }

        private void InitializeComponent()
        {
            this.Text = "Live Sale";
            this.Size = new Size(900, 650);
            this.MinimumSize = new Size(800, 550);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.KeyPreview = true;

            // Main layout
            var mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 4,
                Padding = new Padding(10)
            };
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 60));  // Header
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));  // Grid
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));  // Total
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 60));  // Buttons

            // Header panel (Customer)
            var headerPanel = new PanelControl
            {
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyles.NoBorder
            };

            var customerLabel = new LabelControl
            {
                Text = "Customer:",
                Location = new Point(10, 15),
                AutoSizeMode = LabelAutoSizeMode.None,
                Size = new Size(80, 25)
            };

            _customerLookup = new LookUpEdit
            {
                Location = new Point(100, 12),
                Width = 300,
                Properties =
                {
                    DisplayMember = "Name",
                    ValueMember = "Id",
                    NullText = "Select customer...",
                    SearchMode = SearchMode.AutoFilter,
                    AutoSearchColumnIndex = 0
                }
            };
            _customerLookup.Properties.Columns.Add(new LookUpColumnInfo("Name", "Name"));
            _customerLookup.Properties.Columns.Add(new LookUpColumnInfo("Phone", "Phone"));

            headerPanel.Controls.Add(customerLabel);
            headerPanel.Controls.Add(_customerLookup);

            // Details Grid
            _detailsGrid = new DevExpress.XtraGrid.GridControl
            {
                Dock = DockStyle.Fill
            };

            _detailsView = new GridView(_detailsGrid)
            {
                OptionsSelection = { MultiSelect = false },
                OptionsView = { ShowGroupPanel = false, EnableAppearanceEvenRow = true, EnableAppearanceOddRow = true },
                GridControl = _detailsGrid
            };

            _detailsGrid.MainView = _detailsView;
            _detailsGrid.ViewCollection.Add(_detailsView);

            // Configure columns - read-only display
            _detailsView.Columns.Clear();
            _detailsView.Columns.AddRange(new[]
            {
                new DevExpress.XtraGrid.Columns.GridColumn
                {
                    FieldName = "ProductName",
                    Caption = "Product",
                    Width = 250,
                    Visible = true,
                    OptionsColumn = { ReadOnly = true }
                },
                new DevExpress.XtraGrid.Columns.GridColumn
                {
                    FieldName = "Sku",
                    Caption = "SKU",
                    Width = 100,
                    Visible = true,
                    OptionsColumn = { ReadOnly = true }
                },
                new DevExpress.XtraGrid.Columns.GridColumn
                {
                    FieldName = "Quantity",
                    Caption = "Qty",
                    Width = 70,
                    Visible = true,
                    OptionsColumn = { ReadOnly = true }
                },
                new DevExpress.XtraGrid.Columns.GridColumn
                {
                    FieldName = "UnitPrice",
                    Caption = "Unit Price",
                    Width = 100,
                    Visible = true,
                    DisplayFormat = { FormatString = "c2", FormatType = DevExpress.Utils.FormatType.Numeric },
                    OptionsColumn = { ReadOnly = true }
                },
                new DevExpress.XtraGrid.Columns.GridColumn
                {
                    FieldName = "LineTotal",
                    Caption = "Line Total",
                    Width = 120,
                    Visible = true,
                    DisplayFormat = { FormatString = "c2", FormatType = DevExpress.Utils.FormatType.Numeric },
                    OptionsColumn = { ReadOnly = true }
                },
                new DevExpress.XtraGrid.Columns.GridColumn
                {
                    FieldName = "AvailableStock",
                    Caption = "Stock",
                    Width = 70,
                    Visible = true,
                    OptionsColumn = { ReadOnly = true }
                }
            });

            _detailsView.CellValueChanged += DetailsView_CellValueChanged;
            _detailsGrid.DataSource = _lineItems;

            // Buttons panel
            var buttonPanel = new PanelControl
            {
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyles.NoBorder
            };

            _printButton = new SimpleButton
            {
                Text = "Print Receipt",
                Location = new Point(10, 15),
                Width = 120,
                Height = 35,
                Enabled = false
            };
            _printButton.Click += async (s, e) => await PrintReceiptAsync();

            var closeButton = new SimpleButton
            {
                Text = "Close",
                Location = new Point(140, 15),
                Width = 100,
                Height = 35
            };
            closeButton.Click += (s, e) => this.DialogResult = DialogResult.Cancel;

            buttonPanel.Controls.Add(_printButton);
            buttonPanel.Controls.Add(closeButton);

            // Assemble
            mainLayout.Controls.Add(headerPanel, 0, 0);
            mainLayout.Controls.Add(_detailsGrid, 0, 1);
            mainLayout.Controls.Add(_totalLabel = new LabelControl
            {
                Text = "Total: 0.00",
                Appearance = { Font = new Font("Segoe UI", 16, FontStyle.Bold), ForeColor = Color.DarkGreen },
                Dock = DockStyle.Fill,
                AutoSizeMode = LabelAutoSizeMode.None,
                Location = new Point(10, 10),
                Size = new Size(300, 30)
            }, 0, 2);
            mainLayout.Controls.Add(buttonPanel, 0, 3);

            this.Controls.Add(mainLayout);
        }

        private async Task LoadReferenceDataAsync()
        {
            try
            {
                SetWaitCursor(true);

                // Load customers
                var customers = await _customerService.GetAllAsync(CancellationToken);
                _customerLookup.Properties.DataSource = customers;

                // Load products for reference
                _allProducts = (await _productService.GetAllAsync(CancellationToken)).ToList();

                // If there's a draft being displayed, load it
                if (_currentDraft != null)
                {
                    await LoadDraftAsync();
                }
            }
            catch (Exception ex)
            {
                ShowError($"Failed to load reference data: {ex.Message}");
            }
            finally
            {
                SetWaitCursor(false);
            }
        }

        private async Task LoadDraftAsync()
        {
            if (_currentDraft == null) return;

            try
            {
                SetWaitCursor(true);

                // Load customer
                if (_currentDraft.CustomerId.HasValue)
                {
                    _customerLookup.EditValue = _currentDraft.CustomerId.Value;
                }

                // Load line items from draft
                // In a real implementation, you'd load from SaleTempDetail table
                // For now, we'll reconstruct from the draft data
                _lineItems.Clear();

                // Populate with placeholder lines based on draft net amount
                // This would typically come from the detail table
                _totalLabel.Text = $"Total: {_currentDraft.NetAmount:C2}";

                // Enable print button for active draft
                _printButton.Enabled = _currentDraft.Status == "Active" || _currentDraft.Status == "Draft";
            }
            catch (Exception ex)
            {
                ShowError($"Failed to load draft: {ex.Message}");
            }
            finally
            {
                SetWaitCursor(false);
            }
        }

        private void DetailsView_CellValueChanged(object sender, DevExpress.XtraGrid.Views.Base.CellValueChangedEventArgs e)
        {
            // Read-only form - no cell value changes allowed
            // Events handled by the original sale form
        }

        private void UpdateTotal()
        {
            decimal total = _lineItems.Sum(l => l.LineTotal);
            _totalLabel.Text = $"Total: {total:C2}";
        }

        private async Task PrintReceiptAsync()
        {
            if (_currentDraft == null)
            {
                ShowInfo("No sale to print.");
                return;
            }

            try
            {
                SetWaitCursor(true);
                _printButton.Enabled = false;

                // Generate receipt using the draft data
                var receipt = GenerateReceiptText();
                XtraMessageBox.Show(this, receipt, $"Receipt - Sale #{_currentDraft.Id}", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                ShowError($"Failed to print receipt: {ex.Message}");
            }
            finally
            {
                SetWaitCursor(false);
                _printButton.Enabled = true;
            }
        }

        private string GenerateReceiptText()
        {
            var lines = new List<string>
            {
                "MMNext POS - Receipt",
                "====================",
                $"Sale #: {_currentDraft?.Id ?? 0}",
                $"Date: {DateTime.Now:yyyy-MM-dd HH:mm:ss}",
                $"Customer: {_currentDraft?.CustomerName ?? "Walk-in"}",
                "----------------------------"
            };

            foreach (var line in _lineItems)
            {
                lines.Add($"{line.ProductName} x{line.Quantity} @ {line.UnitPrice:C2} = {line.LineTotal:C2}");
            }

            lines.Add("----------------------------");
            lines.Add($"Total: {_lineItems.Sum(l => l.LineTotal):C2}");
            lines.Add("====================");
            lines.Add("Thank you for your purchase!");

            return string.Join(Environment.NewLine, lines);
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Escape)
            {
                this.DialogResult = DialogResult.Cancel;
                return true;
            }
            if (keyData == (Keys.Control | Keys.P))
            {
                _ = PrintReceiptAsync();
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        #region ViewModel

        private class SaleDetailViewModel
        {
            public int ProductId { get; set; }
            public string ProductName { get; set; } = string.Empty;
            public string Sku { get; set; } = string.Empty;
            public int Quantity { get; set; }
            public decimal UnitPrice { get; set; }
            public int AvailableStock { get; set; }
            public decimal LineTotal => Quantity * UnitPrice;
        }

        #endregion
    }
}
