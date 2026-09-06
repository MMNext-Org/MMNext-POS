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
    /// Form for holding and resuming a sale draft.
    /// Allows users to pause an in-progress sale and resume later.
    /// </summary>
    public class SalesHoldForm : AsyncFormBase
    {
        private readonly ISalesService _salesService;
        private readonly ISaleTempService _saleTempService;
        private readonly IProductService _productService;
        private readonly ICustomerService _customerService;

        // UI Controls - matching NewSaleForm pattern
        private LookUpEdit _customerLookup = null!;
        private DevExpress.XtraGrid.GridControl _detailsGrid = null!;
        private GridView _detailsView = null!;
        private SimpleButton _addLineButton = null!;
        private SimpleButton _removeLineButton = null!;
        private SimpleButton _resumeButton = null!;
        private SimpleButton _cancelButton = null!;
        private LabelControl _totalLabel = null!;
        private TextEdit _searchProductBox = null!;
        private SimpleButton _printButton = null!;
        private BindingList<SaleDetailViewModel> _lineItems = new();

        // Cache for products (for lookup/resume)
        private List<Product> _allProducts = new();

        // Current draft being held
        private SaleTemp? _currentDraft = null;

        public SalesHoldForm(
            ISalesService salesService,
            ISaleTempService saleTempService,
            IProductService productService,
            ICustomerService customerService)
        {
            _salesService = salesService ?? throw new ArgumentNullException(nameof(salesService));
            _saleTempService = saleTempService ?? throw new ArgumentNullException(nameof(saleTempService));
            _productService = productService ?? throw new ArgumentNullException(nameof(productService));
            _customerService = customerService ?? throw new ArgumentNullException(nameof(customerService));

            InitializeComponent();
            this.Load += async (s, e) => await LoadReferenceDataAsync();
        }

        private void InitializeComponent()
        {
            this.Text = "Hold Sale";
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

            // Header panel (Customer + Product search)
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

            var productSearchLabel = new LabelControl
            {
                Text = "Product:",
                Location = new Point(420, 15),
                AutoSizeMode = LabelAutoSizeMode.None,
                Size = new Size(80, 25)
            };

            _searchProductBox = new TextEdit
            {
                Location = new Point(500, 12),
                Width = 300,
                Properties = { NullValuePrompt = "Type product name or SKU..." }
            };
            _searchProductBox.EditValueChanged += (s, e) => FilterProducts();

            headerPanel.Controls.Add(customerLabel);
            headerPanel.Controls.Add(_customerLookup);
            headerPanel.Controls.Add(productSearchLabel);
            headerPanel.Controls.Add(_searchProductBox);

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

            // Configure columns
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
                    ColumnEdit = new RepositoryItemSpinEdit { MinValue = 1, MaxValue = 9999 }
                },
                new DevExpress.XtraGrid.Columns.GridColumn
                {
                    FieldName = "UnitPrice",
                    Caption = "Unit Price",
                    Width = 100,
                    Visible = true,
                    DisplayFormat = { FormatString = "c2", FormatType = DevExpress.Utils.FormatType.Numeric },
                    ColumnEdit = new RepositoryItemSpinEdit() { MinValue = 0, MaxValue = 999999 }
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

            // Buttons for adding/removing lines
            var lineButtonsPanel = new PanelControl
            {
                Dock = DockStyle.Bottom,
                Height = 40,
                BorderStyle = BorderStyles.NoBorder
            };

            _addLineButton = new SimpleButton
            {
                Text = "Add Product",
                Location = new Point(10, 5),
                Width = 120,
                Height = 30
            };
            _addLineButton.Click += async (s, e) => await AddProductLineAsync();

            _removeLineButton = new SimpleButton
            {
                Text = "Remove Line",
                Location = new Point(140, 5),
                Width = 120,
                Height = 30
            };
            _removeLineButton.Click += (s, e) => RemoveSelectedLine();

            lineButtonsPanel.Controls.Add(_addLineButton);
            lineButtonsPanel.Controls.Add(_removeLineButton);

            // Total label panel
            var totalPanel = new PanelControl
            {
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyles.NoBorder
            };

            _totalLabel = new LabelControl
            {
                Text = "Total: 0.00",
                Appearance = { Font = new Font("Segoe UI", 16, FontStyle.Bold), ForeColor = Color.DarkGreen },
                Location = new Point(10, 10),
                AutoSizeMode = LabelAutoSizeMode.None,
                Size = new Size(300, 30)
            };
            totalPanel.Controls.Add(_totalLabel);

            // Bottom buttons - Resume and Cancel
            var buttonPanel = new PanelControl
            {
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyles.NoBorder
            };

            _resumeButton = new SimpleButton
            {
                Text = "Resume Sale",
                Location = new Point(10, 15),
                Width = 120,
                Height = 35
            };
            _resumeButton.Click += async (s, e) => await ResumeSaleAsync();

            _cancelButton = new SimpleButton
            {
                Text = "Cancel",
                Location = new Point(140, 15),
                Width = 100,
                Height = 35
            };
            _cancelButton.Click += (s, e) => this.DialogResult = DialogResult.Cancel;

            _printButton = new SimpleButton
            {
                Text = "Print Receipt",
                Location = new Point(250, 15),
                Width = 100,
                Height = 35,
                Enabled = false
            };
            _printButton.Click += async (s, e) => await PrintReceiptAsync();

            buttonPanel.Controls.Add(_resumeButton);
            buttonPanel.Controls.Add(_cancelButton);
            buttonPanel.Controls.Add(_printButton);

            // Assemble
            mainLayout.Controls.Add(headerPanel, 0, 0);
            mainLayout.Controls.Add(_detailsGrid, 0, 1);
            mainLayout.Controls.Add(totalPanel, 0, 2);
            mainLayout.Controls.Add(buttonPanel, 0, 3);

            this.Controls.Add(mainLayout);
            this.Controls.Add(lineButtonsPanel);
        }

        private async Task LoadReferenceDataAsync()
        {
            try
            {
                SetWaitCursor(true);

                // Load customers
                var customers = await _customerService.GetAllAsync(CancellationToken);
                _customerLookup.Properties.DataSource = customers;

                // Load products for lookup
                _allProducts = (await _productService.GetAllAsync(CancellationToken)).ToList();

                // If there's a draft being held, load it
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

                // Load existing line items from draft
                // In a real implementation, you'd load from SaleTempDetail table
                // For now, we'll reconstruct from the draft's net amount
                _totalLabel.Text = $"Total: {_currentDraft.NetAmount:C2}";

                // Populate line items placeholder (would need detail service)
                // For now, show basic info
                _lineItems.Clear();
                UpdateTotal();

                // Enable print button for resumed draft
                _printButton.Enabled = true;
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

        private void FilterProducts()
        {
            // Could implement a dropdown popup with filtered products
            // For now, we'll use a simple approach - the user types and presses Enter or clicks Add Product
        }

        private async Task AddProductLineAsync()
        {
            var searchText = _searchProductBox.EditValue?.ToString()?.Trim();
            if (string.IsNullOrEmpty(searchText))
            {
                ShowInfo("Please enter a product name or SKU to search.");
                return;
            }

            var matches = _allProducts.Where(p =>
                p.Name.Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
                p.Sku.Contains(searchText, StringComparison.OrdinalIgnoreCase)).ToList();

            if (!matches.Any())
            {
                ShowInfo($"No product found matching '{searchText}'.");
                return;
            }

            Product selectedProduct;
            if (matches.Count == 1)
            {
                selectedProduct = matches[0];
            }
            else
            {
                // Show selection dialog
                using var selector = new ProductSelectorForm(matches);
                if (selector.ShowDialog(this) != DialogResult.OK || selector.SelectedProduct == null)
                    return;
                selectedProduct = selector.SelectedProduct;
            }

            // Check if already in lines
            var existing = _lineItems.FirstOrDefault(l => l.ProductId == selectedProduct.Id);
            if (existing != null)
            {
                existing.Quantity++;
                _detailsView.RefreshData();
                UpdateTotal();
                return;
            }

            // Add new line
            var line = new SaleDetailViewModel
            {
                ProductId = selectedProduct.Id,
                ProductName = selectedProduct.Name,
                Sku = selectedProduct.Sku,
                Quantity = 1,
                UnitPrice = selectedProduct.Price,
                AvailableStock = selectedProduct.StockQuantity
            };

            _lineItems.Add(line);
            _searchProductBox.EditValue = null;
            _detailsView.BestFitColumns();
            UpdateTotal();
        }

        private void RemoveSelectedLine()
        {
            var rowHandle = _detailsView.FocusedRowHandle;
            if (rowHandle >= 0)
            {
                var line = _detailsView.GetRow(rowHandle) as SaleDetailViewModel;
                if (line != null)
                {
                    _lineItems.Remove(line);
                    UpdateTotal();
                }
            }
        }

        private void DetailsView_CellValueChanged(object sender, DevExpress.XtraGrid.Views.Base.CellValueChangedEventArgs e)
        {
            if (e.Column.FieldName == "Quantity")
            {
                var line = _detailsView.GetRow(e.RowHandle) as SaleDetailViewModel;
                if (line != null)
                {
                    if (line.Quantity > line.AvailableStock)
                    {
                        // Clamp the committed value to available stock.
                        line.Quantity = Math.Max(1, line.AvailableStock);
                        _detailsView.SetRowCellValue(e.RowHandle, "Quantity", line.Quantity);
                        ShowInfo($"Insufficient stock. Available: {line.AvailableStock}");
                    }
                    _detailsView.RefreshRow(e.RowHandle);
                    UpdateTotal();
                }
            }
            else if (e.Column.FieldName == "UnitPrice")
            {
                var line = _detailsView.GetRow(e.RowHandle) as SaleDetailViewModel;
                if (line != null)
                {
                    // LineTotal is computed from Quantity * UnitPrice; just refresh row + total.
                    _detailsView.RefreshRow(e.RowHandle);
                    UpdateTotal();
                }
            }
        }

        private void UpdateTotal()
        {
            decimal total = _lineItems.Sum(l => l.LineTotal);
            _totalLabel.Text = $"Total: {total:C2}";
        }

        private async Task ResumeSaleAsync()
        {
            // Validate
            if (_customerLookup.EditValue == null)
            {
                ShowInfo("Please select a customer.");
                return;
            }

            if (!_lineItems.Any())
            {
                ShowInfo("Please add at least one product line before resuming.");
                return;
            }

            // Check stock for all lines
            foreach (var line in _lineItems)
            {
                if (line.Quantity > line.AvailableStock)
                {
                    ShowError($"Insufficient stock for {line.ProductName}. Available: {line.AvailableStock}, Requested: {line.Quantity}");
                    return;
                }
            }

            try
            {
                SetWaitCursor(true);
                _resumeButton.Enabled = false;
                _cancelButton.Enabled = false;

                var customerId = Convert.ToInt32(_customerLookup.EditValue);

                // Build sale from held draft
                var sale = new Sale
                {
                    CustomerId = customerId,
                    SaleDate = DateTime.UtcNow,
                    TotalAmount = _lineItems.Sum(l => l.LineTotal)
                };

                var details = _lineItems.Select(l => new SaleDetail
                {
                    ProductId = l.ProductId,
                    Quantity = l.Quantity,
                    UnitPrice = l.UnitPrice
                }).ToList();

                // Update the sale temp draft and complete the sale
                if (_currentDraft != null)
                {
                    // Update existing draft with new data
                    _currentDraft.CustomerId = customerId;
                    _currentDraft.CustomerName = _customerLookup.Text;
                    _currentDraft.SaleDate = DateTime.UtcNow;
                    _currentDraft.TotalAmount = sale.TotalAmount;
                    _currentDraft.NetAmount = _lineItems.Sum(l => l.LineTotal);
                    _currentDraft.DiscountAmount = 0;
                    _currentDraft.TaxAmount = 0;
                    _currentDraft.NetAmount = _lineItems.Sum(l => l.LineTotal);
                    _currentDraft.Status = "Active";
                    _currentDraft.Notes = "Resumed from hold";

                    await _saleTempService.UpdateAsync(_currentDraft, CancellationToken);
                }
                else
                {
                    // Create new draft
                    var draft = new SaleTemp
                    {
                        CustomerId = customerId,
                        CustomerName = _customerLookup.Text,
                        SaleDate = DateTime.UtcNow,
                        TotalAmount = sale.TotalAmount,
                        NetAmount = _lineItems.Sum(l => l.LineTotal),
                        Status = "Active"
                    };
                    var createdDraft = await _saleTempService.AddAsync(draft, CancellationToken);
                    _currentDraft = createdDraft;
                }

                ShowInfo($"Sale resumed successfully! Sale #{_currentDraft?.Id ?? 0}");
                _printButton.Enabled = true;
                this.DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                ShowError($"Failed to resume sale: {ex.Message}");
            }
            finally
            {
                SetWaitCursor(false);
                _resumeButton.Enabled = true;
                _cancelButton.Enabled = true;
            }
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
            if (keyData == (Keys.Control | Keys.S))
            {
                _ = ResumeSaleAsync();
                return true;
            }
            if (keyData == Keys.F2)
            {
                _ = AddProductLineAsync();
                return true;
            }
            if (keyData == Keys.Enter && _searchProductBox.Focused)
            {
                _ = AddProductLineAsync();
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
