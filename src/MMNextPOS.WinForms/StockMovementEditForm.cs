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
    public partial class StockMovementEditForm : EditFormBase
    {
        private readonly IInventoryService? _inventoryService;
        private readonly IProductService? _productService;
        private readonly ICustomerService? _customerService;
        private readonly ISupplierService? _supplierService;

        // UI Controls
        private LookUpEdit _productLookup = null!;
        private LookUpEdit _locationLookup = null!;
        private TextEdit _movementNoEdit = null!;
        private DateEdit _movementDateEdit = null!;
        private ComboBoxEdit _movementTypeCombo = null!;
        private SpinEdit _quantityEdit = null!;
        private TextEdit _reasonEdit = null!;
        private LookUpEdit _supplierLookup = null!;
        private LookUpEdit _customerLookup = null!;
        private CheckEdit _isActiveCheck = null!;
        private MemoEdit _notesEdit = null!;
        private LabelControl _availableStockLabel = null!;

        // Data
        private StockMovement _movement = null!;
        private bool _isNew = true;
        private List<StockMovementDetailViewModel> _detailLines = new();

        public StockMovementEditForm()
            : this(null, null, null, null, new StockMovement()) { }

        public StockMovementEditForm(
            IInventoryService? inventoryService,
            IProductService? productService,
            ICustomerService? customerService,
            ISupplierService? supplierService,
            StockMovement movement)
        {
            _inventoryService = inventoryService;
            _productService = productService;
            _customerService = customerService;
            _supplierService = supplierService;
            _movement = movement ?? new StockMovement();
            _isNew = _movement.Id == 0;

            InitializeComponent();
            LoadEntityData(_movement);
        }

        private void InitializeComponent()
        {
            this.Text = _isNew ? "New Stock Movement" : "Edit Stock Movement";
            this.Size = new Size(700, 550);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.KeyPreview = true;

            // Main layout
            var mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 14,
                Padding = new Padding(20)
            };
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150));
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            for (int i = 0; i < 13; i++)
                mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 38));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            // Movement No
            mainLayout.Controls.Add(CreateLabel("Movement # *:"), 0, 0);
            _movementNoEdit = new TextEdit
            {
                Dock = DockStyle.Fill,
                Properties = { MaxLength = 50 }
            };
            mainLayout.Controls.Add(_movementNoEdit, 1, 0);

            // Movement Type
            mainLayout.Controls.Add(CreateLabel("Type *:"), 0, 1);
            _movementTypeCombo = new ComboBoxEdit
            {
                Dock = DockStyle.Fill,
                Properties = { TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor }
            };
            _movementTypeCombo.Properties.Items.AddRange(new[] { "Issue", "Receive", "Adjust", "Damaged", "Lost" });
            _movementTypeCombo.SelectedIndex = 0;
            mainLayout.Controls.Add(_movementTypeCombo, 1, 1);

            // Location
            mainLayout.Controls.Add(CreateLabel("Location *:"), 0, 2);
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
            _locationLookup.EditValueChanged += (s, e) => ValidateForm();
            mainLayout.Controls.Add(_locationLookup, 1, 3);

            // Quantity
            mainLayout.Controls.Add(CreateLabel("Quantity *:"), 0, 4);
            _quantityEdit = new SpinEdit
            {
                Dock = DockStyle.Fill,
                Properties = { MinValue = 1, MaxValue = 999999, IsFloatValue = true, Increment = 1m }
            };
            _quantityEdit.EditValueChanged += (s, e) => ValidateForm();
            mainLayout.Controls.Add(_quantityEdit, 1, 4);

            // Available Stock (read-only)
            mainLayout.Controls.Add(CreateLabel("Available Stock:"), 0, 5);
            _availableStockLabel = new LabelControl
            {
                Dock = DockStyle.Fill,
                AutoSizeMode = LabelAutoSizeMode.None,
                Appearance = { ForeColor = Color.DarkGreen, Font = new Font("Segoe UI", 10, FontStyle.Bold) }
            };
            mainLayout.Controls.Add(_availableStockLabel, 1, 5);

            // Reason
            mainLayout.Controls.Add(CreateLabel("Reason *:"), 0, 6);
            _reasonEdit = new TextEdit { Dock = DockStyle.Fill, Properties = { MaxLength = 200 } };
            _reasonEdit.EditValueChanged += (s, e) => ValidateForm();
            mainLayout.Controls.Add(_reasonEdit, 1, 6);

            // Supplier (for Issue/Damaged/Lost movements)
            mainLayout.Controls.Add(CreateLabel("Supplier:"), 0, 7);
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
            mainLayout.Controls.Add(_supplierLookup, 1, 7);

            // Customer (for Issue movements)
            mainLayout.Controls.Add(CreateLabel("Customer:"), 0, 8);
            _customerLookup = new LookUpEdit
            {
                Dock = DockStyle.Fill,
                Properties =
                {
                    DisplayMember = "Name",
                    ValueMember = "Id",
                    NullText = "Select customer...",
                    ShowHeader = false,
                    AutoHeight = false,
                    BestFitMode = DevExpress.XtraEditors.Controls.BestFitMode.BestFitResizePopup,
                    SearchMode = DevExpress.XtraEditors.Controls.SearchMode.AutoFilter
                }
            };
            mainLayout.Controls.Add(_customerLookup, 1, 8);

            // Status
            mainLayout.Controls.Add(CreateLabel("Status *:"), 0, 9);
            _isActiveCheck = new CheckEdit { Dock = DockStyle.Fill, Properties = { ValueChecked = "true", ValueUnchecked = "false" } };
            mainLayout.Controls.Add(_isActiveCheck, 1, 9);

            // Notes
            mainLayout.Controls.Add(CreateLabel("Notes:"), 0, 10);
            _notesEdit = new MemoEdit { Dock = DockStyle.Fill, Properties = { AutoHeight = false, MaxLength = 500 } };
            mainLayout.Controls.Add(_notesEdit, 1, 10);

            // Detail lines grid
            mainLayout.Controls.Add(CreateLabel("Detail Lines:"), 0, 11);
            var detailLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 6,
                RowCount = 1,
                Padding = new Padding(5)
            };
            detailLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20));
            detailLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 15));
            detailLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 15));
            detailLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 15));
            detailLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 15));
            detailLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20));

            var detailView = new GridView(new GridControl { Dock = DockStyle.Fill }) { OptionsView = { ShowGroupPanel = false } };
            detailView.Columns.AddRange(new[]
            {
                new GridColumn { FieldName = "ProductName", Caption = "Product", Width = 150 },
                new GridColumn { FieldName = "Quantity", Caption = "Qty", Width = 80, AppearanceCell = { TextOptions = { HAlignment = DevExpress.Utils.HorzAlignment.Far } } },
                new GridColumn { FieldName = "UnitPrice", Caption = "Price", Width = 100, DisplayFormat = { FormatString = "c2", FormatType = DevExpress.Utils.FormatType.Numeric }, AppearanceCell = { TextOptions = { HAlignment = DevExpress.Utils.HorzAlignment.Far } } },
                new GridColumn { FieldName = "LineTotal", Caption = "Total", Width = 100, DisplayFormat = { FormatString = "c2", FormatType = DevExpress.Utils.FormatType.Numeric }, AppearanceCell = { TextOptions = { HAlignment = DevExpress.Utils.HorzAlignment.Far } } },
                new GridColumn { FieldName = "AvailableStock", Caption = "Stock", Width = 100 },
                new GridColumn { FieldName = "Actions", Caption = "", Width = 80 }
            });
            var detailGrid = new GridControl { Dock = DockStyle.Fill };
            detailView.GridControl = detailGrid;
            detailGrid.MainView = detailView;
            detailGrid.ViewCollection.Add(detailView);
            mainLayout.Controls.Add(detailGrid, 1, 11);

            // Buttons
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
            _okButton = new SimpleButton { Text = "Save", Dock = DockStyle.Right, Width = 100 };
            _okButton.Click += async (s, e) => { if (ValidateForm()) await SaveMovementAsync(); };
            _cancelButton = new SimpleButton { Text = "Cancel", Dock = DockStyle.Left, Width = 100 };
            _cancelButton.Click += (s, e) => DialogResult = DialogResult.Cancel;
            mainLayout.Controls.Add(_okButton, 1, 13);
            mainLayout.Controls.Add(_cancelButton, 0, 13);

            Controls.Add(mainLayout);
        }

        private void UpdateAvailableStock()
        {
            if (_productLookup.EditValue != null && _inventoryService != null)
            {
                var productId = (int)_productLookup.EditValue;
                var available = _inventoryService.GetAvailableStockAsync(productId, null, CancellationToken.None).Result;
                _availableStockLabel.Text = $"Available: {available:N0}";
            }
            else
            {
                _availableStockLabel.Text = "Available: --";
            }
        }

        private void OnMovementTypeChanged()
        {
            // Enable/disable supplier/customer based on movement type
            var type = _movementTypeCombo.EditValue?.ToString();
            var isIssue = type == "Issue";
            var isReceive = type == "Receive";

            _supplierLookup.Visible = isReceive;
            _customerLookup.Visible = isIssue;

            if (!isReceive) _supplierLookup.EditValue = null;
            if (!isIssue) _customerLookup.EditValue = null;
        }

        private async Task SaveMovementAsync()
        {
            // Validate
            if (_movementTypeCombo.EditValue == null)
            {
                ShowInfo("Please select a movement type.");
                return;
            }

            if (_productLookup.EditValue == null)
            {
                ShowInfo("Please select a product.");
                return;
            }

            try
            {
                SetWaitCursor(true);
                _okButton.Enabled = false;
                _cancelButton.Enabled = false;

                var movement = _movement;
                movement.MovementNo = _movementNoEdit.Text;
                movement.MovementType = _movementTypeCombo.Text;
                movement.MovementDate = _movementDateEdit.DateTime;
                movement.LocationId = (int?)_locationLookup.EditValue;
                movement.Reason = _reasonEdit.Text;
                movement.IsActive = _isActiveCheck.Checked;
                movement.Notes = _notesEdit.Text;

                // Update detail lines
                var details = _detailLines.Select(l => new StockMovementDetail
                {
                    ProductId = l.ProductId,
                    Quantity = l.Quantity,
                    UnitCost = l.UnitPrice,
                    LineTotal = l.LineTotal
                }).ToList();

                // Update stock movement
                await _inventoryService!.AddStockMovementAsync(_movement, details, CancellationToken.None);

                ShowInfo($"Stock movement {movement.MovementNo} saved successfully!");
                DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                ShowError($"Failed to save stock movement: {ex.Message}");
            }
            finally
            {
                SetWaitCursor(false);
                _okButton.Enabled = true;
                _cancelButton.Enabled = true;
            }
        }

        protected override bool ValidateForm()
        {
            bool isValid = true;

            if (string.IsNullOrWhiteSpace(_movementNoEdit.Text))
                return false;

            if (_productLookup.EditValue == null)
                return false;

            if (_movementTypeCombo.EditValue == null)
                return false;

            _okButton.Enabled = isValid;
            return isValid;
        }

        public override void LoadEntityData(object entity)
        {
            var movement = (StockMovement)entity;
            _movementNoEdit.Text = movement.MovementNo;
            _movementTypeCombo.Text = movement.MovementType;
            _movementDateEdit.EditValue = movement.MovementDate;
            _locationLookup.EditValue = movement.LocationId;
            _isActiveCheck.Checked = movement.IsActive;
            _notesEdit.Text = movement.Notes ?? string.Empty;
            ValidateForm();
        }

        public override void SaveEntityData(object entity)
        {
            // Handled by SaveMovementAsync
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
    }

    public class StockMovementDetailViewModel
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string Sku { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public int AvailableStock { get; set; }
        public decimal LineTotal => Quantity * UnitPrice;
}
}