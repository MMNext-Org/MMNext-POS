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
    /// Form for stock movements (Issue, Receive, Adjust, Damaged, Lost, Expired, Assembly/Deassembly)
    /// </summary>
    public class StockMovementForm : AsyncFormBase
    {
        private readonly IInventoryService _inventoryService;
        private readonly IProductService _productService;
        private readonly ILocationService _locationService;
        private readonly ISupplierService _supplierService;

        // UI Controls
        private LookUpEdit _movementTypeCombo = null!;
        private LookUpEdit _locationLookup = null!;
        private LookUpEdit _supplierLookup = null!;
        private DateEdit _movementDateEdit = null!;
        private TextEdit _referenceNoEdit = null!;
        private MemoEdit _notesEdit = null!;

        private DevExpress.XtraGrid.GridControl _detailsGrid = null!;
        private GridView _detailsView = null!;
        private SimpleButton _addLineButton = null!;
        private SimpleButton _removeLineButton = null!;
        private LabelControl _totalLabel = null!;
        private SimpleButton _saveButton = null!;
        private SimpleButton _cancelButton = null!;

        private BindingList<StockMovementDetailViewModel> _lineItems = new();
        private List<Product> _allProducts = new();
        private int? _editingMovementId = null;

        public StockMovementForm(
            IInventoryService inventoryService,
            IProductService productService,
            ILocationService locationService,
            ISupplierService supplierService)
        {
            _inventoryService = inventoryService ?? throw new ArgumentNullException(nameof(inventoryService));
            _productService = productService ?? throw new ArgumentNullException(nameof(productService));
            _locationService = locationService ?? throw new ArgumentNullException(nameof(locationService));
            _supplierService = supplierService ?? throw new ArgumentNullException(nameof(supplierService));

            InitializeComponent();
            this.Load += async (s, e) => await LoadReferenceDataAsync();
        }

        public StockMovementForm(
            IInventoryService inventoryService,
            IProductService productService,
            ILocationService locationService,
            ISupplierService supplierService,
            int movementId) : this(inventoryService, productService, locationService, supplierService)
        {
            _editingMovementId = movementId;
        }

        private void InitializeComponent()
        {
            this.Text = "Stock Movement";
            this.Size = new Size(1000, 700);
            this.MinimumSize = new Size(900, 600);
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
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 120));  // Header
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));  // Grid
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));  // Total
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 60));  // Buttons

            // Header panel
            var headerPanel = new PanelControl
            {
                Dock = DockStyle.Fill,
                BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder
            };

            var headerLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 6,
                RowCount = 3,
                Padding = new Padding(10)
            };
            headerLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100));
            headerLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30));
            headerLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100));
            headerLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30));
            headerLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100));
            headerLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30));
            headerLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
            headerLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
            headerLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));

            // Movement Type
            headerLayout.Controls.Add(CreateLabel("Type *:"), 0, 0);
            _movementTypeCombo = new LookUpEdit
            {
                Dock = DockStyle.Fill,
                Properties =
                {
                    DisplayMember = "Name",
                    ValueMember = "Value",
                    NullText = "Select type...",
                    ShowHeader = false,
                    AutoHeight = false,
                    TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor
                }
            };
            _movementTypeCombo.Properties.DataSource = new[]
            {
                new { Value = "Issue", Name = "Issue (Out)" },
                new { Value = "Receive", Name = "Receive (In)" },
                new { Value = "Adjust", Name = "Adjustment" },
                new { Value = "Damaged", Name = "Damaged" },
                new { Value = "Lost", Name = "Lost" },
                new { Value = "Expired", Name = "Expired" },
                new { Value = "Assembly", Name = "Assembly (BOM)" },
                new { Value = "Deassembly", Name = "Deassembly" },
            };
            _movementTypeCombo.EditValueChanged += (s, e) => OnMovementTypeChanged();
            headerLayout.Controls.Add(_movementTypeCombo, 1, 0);

            // Location
            headerLayout.Controls.Add(CreateLabel("Location *:"), 2, 0);
            _locationLookup = new LookUpEdit
            {
                Dock = DockStyle.Fill,
                Properties =
                {
                    DisplayMember = "Name",
                    ValueMember = "Id",
                    NullText = "Select location...",
                    ShowHeader = false,
                    AutoHeight = false,
                    BestFitMode = DevExpress.XtraEditors.Controls.BestFitMode.BestFitResizePopup,
                    SearchMode = DevExpress.XtraEditors.Controls.SearchMode.AutoFilter
                }
            };
            headerLayout.Controls.Add(_locationLookup, 3, 0);

            // Supplier (for Receive)
            headerLayout.Controls.Add(CreateLabel("Supplier:"), 4, 0);
            _supplierLookup = new LookUpEdit
            {
                Dock = DockStyle.Fill,
                Properties =
                {
                    DisplayMember = "Name",
                    ValueMember = "Id",
                    NullText = "Select supplier...",
                    ShowHeader = false,
                    AutoHeight = false,
                    BestFitMode = DevExpress.XtraEditors.Controls.BestFitMode.BestFitResizePopup,
                    SearchMode = DevExpress.XtraEditors.Controls.SearchMode.AutoFilter
                }
            };
            headerLayout.Controls.Add(_supplierLookup, 5, 0);

            // Movement Date
            headerLayout.Controls.Add(CreateLabel("Date *:"), 0, 1);
            _movementDateEdit = new DateEdit
            {
                Dock = DockStyle.Fill,
                EditValue = DateTime.Today,
                Properties =
                {
                    AutoHeight = false,
                    CalendarView = DevExpress.XtraEditors.Repository.CalendarView.Classic
                }
            };
            headerLayout.Controls.Add(_movementDateEdit, 1, 1);

            // Reference No
            headerLayout.Controls.Add(CreateLabel("Ref #:"), 2, 1);
            _referenceNoEdit = new TextEdit
            {
                Dock = DockStyle.Fill,
                Properties = { MaxLength = 50 }
            };
            headerLayout.Controls.Add(_referenceNoEdit, 3, 1);

            // Notes
            headerLayout.Controls.Add(CreateLabel("Notes:"), 0, 2);
            _notesEdit = new MemoEdit
            {
                Dock = DockStyle.Fill,
                Properties = { AutoHeight = false, MaxLength = 500 }
            };
            headerLayout.Controls.Add(_notesEdit, 1, 2);
            headerLayout.SetColumnSpan(_notesEdit, 5);

            headerPanel.Controls.Add(headerLayout);
            mainLayout.Controls.Add(headerPanel, 0, 0);

            // Details Grid
            _detailsGrid = new DevExpress.XtraGrid.GridControl { Dock = DockStyle.Fill };
            _detailsView = new GridView(_detailsGrid)
            {
                OptionsSelection = { MultiSelect = false },
                OptionsView = { ShowGroupPanel = false, EnableAppearanceEvenRow = true, EnableAppearanceOddRow = true },
                GridControl = _detailsGrid
            };
            _detailsGrid.MainView = _detailsView;
            _detailsGrid.ViewCollection.Add(_detailsView);

            _detailsView.Columns.Clear();
            _detailsView.Columns.AddRange(new[]
            {
                new GridColumn { FieldName = "ProductName", Caption = "Product", Width = 250, Visible = true, OptionsColumn = { ReadOnly = true } },
                new GridColumn { FieldName = "Sku", Caption = "SKU", Width = 100, Visible = true, OptionsColumn = { ReadOnly = true } },
                new GridColumn { FieldName = "Quantity", Caption = "Qty", Width = 80, Visible = true, ColumnEdit = new RepositoryItemSpinEdit { MinValue = 1, MaxValue = 9999 } },
                new GridColumn { FieldName = "UnitCost", Caption = "Unit Cost", Width = 100, Visible = true, DisplayFormat = { FormatString = "c2", FormatType = DevExpress.Utils.FormatType.Numeric }, ColumnEdit = new RepositoryItemSpinEdit() { MinValue = 0, MaxValue = 999999 } },
                new GridColumn { FieldName = "LineTotal", Caption = "Total", Width = 120, Visible = true, DisplayFormat = { FormatString = "c2", FormatType = DevExpress.Utils.FormatType.Numeric }, OptionsColumn = { ReadOnly = true } },
                new GridColumn { FieldName = "SerialNumber", Caption = "Serial #", Width = 120, Visible = true },
                new GridColumn { FieldName = "Notes", Caption = "Notes", Width = 150, Visible = true },
            });

            _detailsView.CellValueChanged += DetailsView_CellValueChanged;
            _detailsGrid.DataSource = _lineItems;

// Add/Remove line buttons
            var lineButtonsPanel = new PanelControl
            {
                Dock = DockStyle.Bottom,
                Height = 40,
                BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder
            };

            _addLineButton = new SimpleButton
            {
                Text = "Add Line",
                Location = new Point(10, 5),
                Width = 100,
                Height = 30
            };
            _addLineButton.Click += async (s, e) => await AddProductLineAsync();

            _removeLineButton = new SimpleButton
            {
                Text = "Remove Line",
                Location = new Point(120, 5),
                Width = 100,
                Height = 30
            };
            _removeLineButton.Click += (s, e) => RemoveSelectedLine();

            lineButtonsPanel.Controls.Add(_addLineButton);
            lineButtonsPanel.Controls.Add(_removeLineButton);

            // Total panel
            var totalPanel = new PanelControl
            {
                Dock = DockStyle.Fill,
                BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder
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

            // Bottom buttons
            var buttonPanel = new PanelControl
            {
                Dock = DockStyle.Fill,
                BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder
            };

            _saveButton = new SimpleButton
            {
                Text = "Save",
                Location = new Point(10, 15),
                Width = 100,
                Height = 35
            };
            _saveButton.Click += async (s, e) => await SaveMovementAsync();

            _cancelButton = new SimpleButton
            {
                Text = "Cancel",
                Location = new Point(120, 15),
                Width = 100,
                Height = 35
            };
            _cancelButton.Click += (s, e) => this.DialogResult = DialogResult.Cancel;

            buttonPanel.Controls.Add(_saveButton);
            buttonPanel.Controls.Add(_cancelButton);

            // Assemble
            mainLayout.Controls.Add(headerPanel, 0, 0);
            mainLayout.Controls.Add(_detailsGrid, 0, 1);
            mainLayout.Controls.Add(totalPanel, 0, 2);
            mainLayout.Controls.Add(lineButtonsPanel, 1, 1);
            mainLayout.Controls.Add(buttonPanel, 0, 3);

            Controls.Add(mainLayout);
            Controls.Add(lineButtonsPanel);
        }

        private LabelControl CreateLabel(string text)
        {
            return new LabelControl
            {
                Text = text,
                Dock = DockStyle.Fill,
                AutoSizeMode = LabelAutoSizeMode.None,
                Appearance = { TextOptions = { HAlignment = DevExpress.Utils.HorzAlignment.Far } },
                Padding = new Padding(0, 0, 10, 0)
            };
        }

        private async Task LoadReferenceDataAsync()
        {
            try
            {
                SetWaitCursor(true);

                // Load locations
                var locations = await _locationService.GetAllAsync(CancellationToken);
                _locationLookup.Properties.DataSource = locations.Where(l => l.IsActive).ToList();
                _locationLookup.Properties.DisplayMember = "Name";
                _locationLookup.Properties.ValueMember = "Id";
                _locationLookup.Properties.NullText = "Select location...";
                _locationLookup.Properties.ShowHeader = false;
                _locationLookup.Properties.AutoHeight = false;
                _locationLookup.Properties.BestFitMode = DevExpress.XtraEditors.Controls.BestFitMode.BestFitResizePopup;
                _locationLookup.Properties.SearchMode = DevExpress.XtraEditors.Controls.SearchMode.AutoFilter;
                _locationLookup.Properties.Columns.Add(new DevExpress.XtraEditors.Controls.LookUpColumnInfo("Name", "Name"));
                _locationLookup.Properties.Columns.Add(new DevExpress.XtraEditors.Controls.LookUpColumnInfo("Code", "Code"));
                _locationLookup.Properties.Columns["Id"].Visible = false;
                _locationLookup.Properties.Columns["Address"].Visible = false;
                _locationLookup.Properties.Columns["City"].Visible = false;
                _locationLookup.Properties.Columns["Phone"].Visible = false;
                _locationLookup.Properties.Columns["Email"].Visible = false;
                _locationLookup.Properties.Columns["IsActive"].Visible = false;
                _locationLookup.Properties.Columns["CreatedAt"].Visible = false;
                _locationLookup.Properties.Columns["UpdatedAt"].Visible = false;

                // Load suppliers
                var suppliers = await _supplierService.GetAllAsync(CancellationToken);
                _supplierLookup.Properties.DataSource = suppliers.Where(s => s.IsActive).ToList();
                _supplierLookup.Properties.DisplayMember = "Name";
                _supplierLookup.Properties.ValueMember = "Id";
                _supplierLookup.Properties.NullText = "Select supplier...";
                _supplierLookup.Properties.ShowHeader = false;
                _supplierLookup.Properties.AutoHeight = false;
                _supplierLookup.Properties.BestFitMode = DevExpress.XtraEditors.Controls.BestFitMode.BestFitResizePopup;
                _supplierLookup.Properties.SearchMode = DevExpress.XtraEditors.Controls.SearchMode.AutoFilter;
                _supplierLookup.Properties.Columns.Add(new DevExpress.XtraEditors.Controls.LookUpColumnInfo("Name", "Name"));
                _supplierLookup.Properties.Columns.Add(new DevExpress.XtraEditors.Controls.LookUpColumnInfo("Code", "Code"));
                _supplierLookup.Properties.Columns["Id"].Visible = false;
                _supplierLookup.Properties.Columns["Address"].Visible = false;
                _supplierLookup.Properties.Columns["City"].Visible = false;
                _locationLookup.Properties.Columns["Country"].Visible = false;
                _locationLookup.Properties.Columns["Phone"].Visible = false;
                _locationLookup.Properties.Columns["Email"].Visible = false;
                _supplierLookup.Properties.Columns["ContactPerson"].Visible = false;
                _supplierLookup.Properties.Columns["TaxId"].Visible = false;
                _supplierLookup.Properties.Columns["CreditLimit"].Visible = false;
                _supplierLookup.Properties.Columns["PaymentTermDays"].Visible = false;
                _supplierLookup.Properties.Columns["IsActive"].Visible = false;
                _supplierLookup.Properties.Columns["CreatedAt"].Visible = false;
                _supplierLookup.Properties.Columns["UpdatedAt"].Visible = false;

                // Load products for lookup in AddProductLineAsync
                var products = await _productService.GetAllAsync(CancellationToken);
                _allProducts = products.Where(p => p.IsActive).ToList();

                if (_editingMovementId.HasValue)
                {
                    // Load existing movement for editing
                    // Would need a GetByIdAsync on IInventoryService
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

        private void OnMovementTypeChanged()
        {
            // Show/hide supplier field based on movement type
            var type = _movementTypeCombo.EditValue?.ToString();
            var isReceive = type == "Receive";
            _supplierLookup.Visible = isReceive;
        }

        private async Task AddProductLineAsync()
        {
            // Product selector dialog
            using var selector = new ProductSelectorForm(_allProducts);
            if (selector.ShowDialog(this) != DialogResult.OK || selector.SelectedProduct == null)
                return;

            var selectedProduct = selector.SelectedProduct;

            var existing = _lineItems.FirstOrDefault(l => l.ProductId == selectedProduct.Id);
            if (existing != null)
            {
                existing.Quantity++;
                _detailsView.RefreshData();
                UpdateTotal();
                return;
            }

            var line = new StockMovementDetailViewModel
            {
                ProductId = selectedProduct.Id,
                ProductName = selectedProduct.Name,
                Sku = selectedProduct.Sku,
                Quantity = 1,
                UnitCost = selectedProduct.Price,
                AvailableStock = selectedProduct.StockQuantity
            };

            _lineItems.Add(line);
            _detailsView.BestFitColumns();
            UpdateTotal();
        }

        private void RemoveSelectedLine()
        {
            var rowHandle = _detailsView.FocusedRowHandle;
            if (rowHandle >= 0)
            {
                var line = _detailsView.GetRow(rowHandle) as StockMovementDetailViewModel;
                if (line != null)
                {
                    _lineItems.Remove(line);
                    UpdateTotal();
                }
            }
        }

        private void DetailsView_CellValueChanged(object sender, DevExpress.XtraGrid.Views.Base.CellValueChangedEventArgs e)
        {
            if (e.Column.FieldName == "Quantity" || e.Column.FieldName == "UnitCost")
            {
                var line = _detailsView.GetRow(e.RowHandle) as StockMovementDetailViewModel;
                if (line != null)
                {
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

        private async Task SaveMovementAsync()
        {
            if (!_lineItems.Any())
            {
                ShowInfo("Please add at least one line item.");
                return;
            }

            // Validate based on movement type
            var type = _movementTypeCombo.EditValue?.ToString();
            if (string.IsNullOrEmpty(type))
            {
                ShowInfo("Please select a movement type.");
                return;
            }

            if (_locationLookup.EditValue == null)
            {
                ShowInfo("Please select a location.");
                return;
            }

            if (_movementDateEdit.EditValue == null)
            {
                ShowInfo("Please select a date.");
                return;
            }

            if (type == "Receive" && _supplierLookup.EditValue == null)
            {
                ShowInfo("Please select a supplier for Receive movements.");
                return;
            }

            try
            {
                SetWaitCursor(true);

                var movement = new StockMovement
                {
                    MovementNo = $"STK-{DateTime.Now:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..6].ToUpper()}",
                    MovementType = _movementTypeCombo.EditValue?.ToString() ?? "Adjust",
                    MovementDate = _movementDateEdit.DateTime,
                    LocationId = Convert.ToInt32(_locationLookup.EditValue),
                    SupplierId = _supplierLookup.EditValue != null ? Convert.ToInt32(_supplierLookup.EditValue) : null,
                    Reason = _notesEdit.Text,
                    Status = "Active",
                    CreatedByUserId = 1 // Would come from auth context
                };

                var details = _lineItems.Select(l => new StockMovementDetail
                {
                    ProductId = l.ProductId,
                    Quantity = l.Quantity,
                    UnitCost = l.UnitCost,
                    SerialNumber = l.SerialNumber,
                    Notes = l.Notes
                }).ToList();

                var createdMovement = await _inventoryService.AddStockMovementAsync(movement, details, CancellationToken);

                ShowInfo($"Stock movement {createdMovement.MovementNo} created successfully!");
                this.DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                ShowError($"Failed to save stock movement: {ex.Message}");
            }
            finally
            {
                SetWaitCursor(false);
            }
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
                _ = SaveMovementAsync();
                return true;
            }
            if (keyData == Keys.Enter && _referenceNoEdit?.Focused == true)
            {
                // Would need search box
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        #region ViewModel

        private class StockMovementDetailViewModel
        {
            public int ProductId { get; set; }
            public string ProductName { get; set; } = string.Empty;
            public string Sku { get; set; } = string.Empty;
            public int Quantity { get; set; }
            public decimal UnitCost { get; set; }
            public int AvailableStock { get; set; }
            public string? SerialNumber { get; set; }
            public string? Notes { get; set; }
            public decimal LineTotal => Quantity * UnitCost;
        }

        #endregion
    }
}