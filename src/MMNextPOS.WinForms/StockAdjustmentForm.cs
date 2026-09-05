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
using MMNextPOS.Application.Services;
using MMNextPOS.Domain.Models;

namespace MMNextPOS.WinForms
{
    public partial class StockAdjustmentForm : EditFormBase
    {
        private readonly IInventoryService _inventoryService;
        private readonly IProductService _productService;

        // UI Controls
        private LookUpEdit _productLookup = null!;
        private SpinEdit _quantityEdit = null!;
        private TextEdit _reasonEdit = null!;
        private LookUpEdit _locationLookup = null!;
        private CheckEdit _isActiveCheck = null!;
        private MemoEdit _notesEdit = null!;
        private LabelControl _availableStockLabel = null!;

        // Data
        private Product _product = null!;
        private bool _isNew = true;

        public StockAdjustmentForm()
            : this(null, null, new Product()) { }

        public StockAdjustmentForm(
            IInventoryService? inventoryService,
            IProductService? productService,
            Product product)
        {
            _inventoryService = inventoryService!;
            _productService = productService!;
            _product = product ?? new Product();
            _isNew = _product.Id == 0;

            InitializeComponent();
            LoadEntityData(_product);
        }

        private void InitializeComponent()
        {
            this.Text = "Stock Adjustment";
            this.Size = new Size(600, 450);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            var mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 12,
                Padding = new Padding(20)
            };
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150));
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            for (int i = 0; i < 11; i++)
                mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 35));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            // Product
            mainLayout.Controls.Add(CreateLabel("Product *:"), 0, 0);
            _productLookup = new LookUpEdit
            {
                Dock = DockStyle.Fill,
                Properties =
                {
                    DisplayMember = "Name",
                    ValueMember = "Id",
                    NullText = "Select product...",
                    ShowHeader = false,
                    AutoHeight = false,
                    BestFitMode = DevExpress.XtraEditors.Controls.BestFitMode.BestFitResizePopup,
                    SearchMode = DevExpress.XtraEditors.Controls.SearchMode.AutoFilter
                }
            };
            _productLookup.EditValueChanged += async (s, e) => await UpdateAvailableStockAsync();
            mainLayout.Controls.Add(_productLookup, 1, 0);

            // Quantity
            mainLayout.Controls.Add(CreateLabel("Quantity *:"), 0, 1);
            _quantityEdit = new SpinEdit
            {
                Dock = DockStyle.Fill,
                Properties = { MinValue = 1, MaxValue = 999999, IsFloatValue = false, Increment = 1m }
            };
            mainLayout.Controls.Add(_quantityEdit, 1, 1);

            // Available Stock (read-only)
            mainLayout.Controls.Add(CreateLabel("Available Stock:"), 0, 2);
            _availableStockLabel = new LabelControl
            {
                Dock = DockStyle.Fill,
                AutoSizeMode = LabelAutoSizeMode.None,
                Appearance = { ForeColor = Color.DarkGreen, Font = new Font("Segoe UI", 10, FontStyle.Bold) }
            };
            mainLayout.Controls.Add(_availableStockLabel, 1, 2);

            // Reason
            mainLayout.Controls.Add(CreateLabel("Reason *:"), 0, 3);
            _reasonEdit = new TextEdit { Dock = DockStyle.Fill, Properties = { MaxLength = 200 } };
            mainLayout.Controls.Add(_reasonEdit, 1, 3);

            // Location
            mainLayout.Controls.Add(CreateLabel("Location:"), 0, 4);
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
            mainLayout.Controls.Add(_locationLookup, 1, 4);

            // Is Active
            mainLayout.Controls.Add(CreateLabel("Is Active:"), 0, 5);
            _isActiveCheck = new CheckEdit { Dock = DockStyle.Fill, Properties = { ValueChecked = "true", ValueUnchecked = "false" } };
            mainLayout.Controls.Add(_isActiveCheck, 1, 5);

            // Notes
            mainLayout.Controls.Add(CreateLabel("Notes:"), 0, 6);
            _notesEdit = new MemoEdit { Dock = DockStyle.Fill, Properties = { AutoHeight = false, MaxLength = 500 } };
            mainLayout.Controls.Add(_notesEdit, 1, 6);

            // Detail lines grid
            mainLayout.Controls.Add(CreateLabel("Detail Lines:"), 0, 7);
            var detailLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 5,
                RowCount = 1,
                Padding = new Padding(5)
            };
            detailLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
            detailLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 15));
            detailLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 15));
            detailLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 15));
            detailLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30));

            var detailView = new GridView(new GridControl { Dock = DockStyle.Fill }) { OptionsView = { ShowGroupPanel = false } };
            detailView.Columns.AddRange(new[]
            {
                new GridColumn { FieldName = "ProductName", Caption = "Product", Width = 150 },
                new GridColumn { FieldName = "Quantity", Caption = "Qty", Width = 80, AppearanceCell = { TextOptions = { HAlignment = DevExpress.Utils.HorzAlignment.Far } } },
                new GridColumn { FieldName = "UnitCost", Caption = "Cost", Width = 100, DisplayFormat = { FormatString = "c2", FormatType = DevExpress.Utils.FormatType.Numeric }, AppearanceCell = { TextOptions = { HAlignment = DevExpress.Utils.HorzAlignment.Far } } },
                new GridColumn { FieldName = "LineTotal", Caption = "Total", Width = 100, DisplayFormat = { FormatString = "c2", FormatType = DevExpress.Utils.FormatType.Numeric }, AppearanceCell = { TextOptions = { HAlignment = DevExpress.Utils.HorzAlignment.Far } } },
                new GridColumn { FieldName = "Actions", Caption = "", Width = 80 }
            });
            var detailGrid = new GridControl { Dock = DockStyle.Fill };
            detailView.GridControl = detailGrid;
            detailGrid.MainView = detailView;
            detailGrid.ViewCollection.Add(detailView);
            mainLayout.Controls.Add(detailGrid, 1, 7);

            // Buttons
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
            _okButton = new SimpleButton { Text = "Adjust", Dock = DockStyle.Right, Width = 100 };
            _okButton.Click += async (s, e) => { if (ValidateForm()) await SaveAdjustmentAsync(); };
            _cancelButton = new SimpleButton { Text = "Cancel", Dock = DockStyle.Left, Width = 100 };
            _cancelButton.Click += (s, e) => DialogResult = DialogResult.Cancel;
            mainLayout.Controls.Add(_okButton, 1, 11);
            mainLayout.Controls.Add(_cancelButton, 0, 11);

            Controls.Add(mainLayout);

            _okButton.Enabled = _isNew;
            _cancelButton.Click += (s, e) => DialogResult = DialogResult.Cancel;
        }

        private async Task UpdateAvailableStockAsync()
        {
            if (_productLookup.EditValue != null)
            {
                var productId = (int)_productLookup.EditValue;
                var available = await _inventoryService.GetAvailableStockAsync(productId, (int?)_locationLookup.EditValue, CancellationToken.None);
                _availableStockLabel.Text = $"Available: {available:N0}";
                var product = await _productService.GetByIdAsync(productId, CancellationToken.None);
                if (product != null)
                    _product = product;
            }
            else
            {
                _availableStockLabel.Text = "Available: --";
            }
        }

        private async Task SaveAdjustmentAsync()
        {
            try
            {
                SetWaitCursor(true);
                _okButton.Enabled = false;
                _cancelButton.Enabled = false;

                var movement = new StockMovement
                {
                    MovementNo = $"ADJ-{DateTime.UtcNow:yyyyMMdd-HHmmss}",
                    MovementType = "Adjust",
                    ProductId = (int)_productLookup.EditValue,
                    Quantity = (int)_quantityEdit.Value,
                    Reason = _reasonEdit.Text.Trim(),
                    LocationId = _locationLookup.EditValue == null ? null : (int?)_locationLookup.EditValue,
                    IsActive = _isActiveCheck.Checked,
                    Notes = _notesEdit.Text.Trim(),
                    CreatedBy = 1,
                    CreatedAt = DateTime.UtcNow
                };

                // Create detail line
                var detail = new StockMovementDetail
                {
                    ProductId = (int)_productLookup.EditValue,
                    Quantity = (int)_quantityEdit.Value,
                    UnitCost = _product?.Price ?? 0m,
                    LineTotal = (int)_quantityEdit.Value * (_product?.Price ?? 0m)
                };

                // Add stock movement via service - Adjust movement adjusts stock by the quantity (can be positive or negative based on reason)
                await _inventoryService.AddStockMovementAsync(movement, new[] { detail }, CancellationToken.None);

                ShowInfo($"Stock adjustment {movement.MovementNo} saved successfully!");
                DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                ShowError($"Failed to save stock adjustment: {ex.Message}");
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

            if (string.IsNullOrWhiteSpace(_productLookup.EditValue?.ToString()))
                return false;

            if (_quantityEdit.EditValue == null || (int)_quantityEdit.EditValue <= 0)
                return false;

            if (_reasonEdit == null || string.IsNullOrWhiteSpace(_reasonEdit.Text))
                return false;

            _okButton.Enabled = isValid;
            return isValid;
        }

        public override void LoadEntityData(object entity)
        {
            var product = (Product)entity;
            _productLookup.EditValue = product.Id;
            _quantityEdit.Value = 1; // Default to 1
            _reasonEdit.Text = "Stock adjustment";
            _isActiveCheck.Checked = true;

            _ = UpdateAvailableStockAsync();

            ValidateForm();
        }

        public override void SaveEntityData(object entity)
        {
            // Handled by SaveAdjustmentAsync
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

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Escape) { this.DialogResult = DialogResult.Cancel; return true; }
            return base.ProcessCmdKey(ref msg, keyData);
        }
    }
}