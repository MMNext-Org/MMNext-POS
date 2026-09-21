using System;
using System.Collections.Generic;
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
using MMNextPOS.Infrastructure;
using MMNextPOS.Infrastructure.Repositories;

namespace MMNextPOS.WinForms
{
    public partial class StockTransferEditForm : EditFormBase
    {
        private readonly IStockTransferService? _service;
        private readonly ILocationRepository? _locationRepo;
        private readonly IProductRepository? _productRepo;
        private readonly IStockTransferDetailRepository? _detailRepo;
        private readonly IUnitOfWork? _unitOfWork;
        private readonly IAuditService? _auditService;

        private StockTransfer _entity = new();
        private List<StockTransferDetail> _details = new();
        private bool _isNew = true;

        private LookUpEdit? _fromLocationEdit;
        private LookUpEdit? _toLocationEdit;
        private DateEdit? _transferDateEdit;
        private MemoEdit? _notesEdit;
        private GridControl? _detailsGrid;
        private GridView? _detailsView;
        private SimpleButton? _addDetailButton;
        private SimpleButton? _removeDetailButton;

        public StockTransferEditForm() : this(null) { }

        public StockTransferEditForm(
            StockTransfer? entity = null,
            IStockTransferService? service = null,
            ILocationRepository? locationRepo = null,
            IProductRepository? productRepo = null,
            IStockTransferDetailRepository? detailRepo = null,
            IUnitOfWork? unitOfWork = null,
            IAuditService? auditService = null)
        {
            _entity = entity ?? new StockTransfer();
            _service = service;
            _locationRepo = locationRepo;
            _productRepo = productRepo;
            _detailRepo = detailRepo;
            _unitOfWork = unitOfWork;
            _auditService = auditService;
            _isNew = _entity.Id == 0;

            InitializeComponent();
            LoadEntityData(_entity);
        }

        private void InitializeComponent()
        {
            Text = _isNew ? "New Stock Transfer" : "Edit Stock Transfer";
            Size = new Size(700, 600);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;

            var mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 8,
                Padding = new Padding(15)
            };
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130));
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 45));

            // CreateLabel
            mainLayout.Controls.Add(CreateLabelControl("From Location *:"), 0, 0);
            _fromLocationEdit = new LookUpEdit { Dock = DockStyle.Fill };
            mainLayout.Controls.Add(_fromLocationEdit, 1, 0);

            mainLayout.Controls.Add(CreateLabelControl("To Location *:"), 0, 1);
            _toLocationEdit = new LookUpEdit { Dock = DockStyle.Fill, Properties = { NullText = "Select location..." } };
            mainLayout.Controls.Add(_toLocationEdit, 1, 1);

            mainLayout.Controls.Add(CreateLabelControl("Transfer Date:"), 0, 2);
            _transferDateEdit = new DateEdit { Dock = DockStyle.Fill, Properties = { ShowToday = true } };
            mainLayout.Controls.Add(_transferDateEdit, 1, 2);

            mainLayout.Controls.Add(CreateLabelControl("Notes:"), 0, 3);
            _notesEdit = new MemoEdit { Dock = DockStyle.Fill, Properties = { MaxLength = 500 } };
            mainLayout.Controls.Add(_notesEdit, 1, 3);

            mainLayout.Controls.Add(new LabelControl(), 0, 4);

            // Details grid
            var detailPanel = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2 };
            detailPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            detailPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));

            _detailsGrid = new GridControl { Dock = DockStyle.Fill };
            _detailsView = new GridView(_detailsGrid)
            {
                OptionsView = { ShowGroupPanel = false },
                OptionsBehavior = { Editable = false, ReadOnly = true }
            };
            _detailsView.Columns.AddRange(new[]
            {
                new GridColumn { FieldName = "ProductId", Caption = "Product ID", Visible = true, Width = 120 },
                new GridColumn { FieldName = "Quantity", Caption = "Qty", Visible = true, Width = 80 },
                new GridColumn { FieldName = "UnitPrice", Caption = "Unit Price", Visible = true, Width = 120, DisplayFormat = { FormatString = "c2", FormatType = DevExpress.Utils.FormatType.Numeric } },
                new GridColumn { FieldName = "SerialNumber", Caption = "Serial #", Visible = true, Width = 150 },
                new GridColumn { FieldName = "Notes", Caption = "Notes", Visible = true, Width = 200 }
            });
            _detailsGrid.ViewCollection.Add(_detailsView);
            _detailsGrid.MainView = _detailsView;
            detailPanel.Controls.Add(_detailsGrid, 0, 0);

            var buttonPanel = new PanelControl { Dock = DockStyle.Fill };
            _addDetailButton = new SimpleButton { Text = "Add", Dock = DockStyle.Top, Width = 100, Height = 35 };
            _addDetailButton.Click += OnAddDetailAsync;
            _removeDetailButton = new SimpleButton { Text = "Remove", Dock = DockStyle.Top, Width = 100, Height = 35 };
            _removeDetailButton.Click += OnRemoveDetail;
            buttonPanel.Controls.Add(_addDetailButton);
            buttonPanel.Controls.Add(_removeDetailButton);
            detailPanel.Controls.Add(buttonPanel, 1, 0);

            mainLayout.Controls.Add(detailPanel, 1, 5);
            mainLayout.Controls.Add(new LabelControl(), 0, 6);

            var buttonsRow = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 3 };
            buttonsRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            buttonsRow.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100));
            buttonsRow.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100));
            buttonsRow.Controls.Add(_okButton, 1, 0);
            buttonsRow.Controls.Add(_cancelButton, 2, 0);
            mainLayout.Controls.Add(buttonsRow, 1, 7);

            Controls.Add(mainLayout);

            if (_isNew) _transferDateEdit.DateTime = DateTime.UtcNow;
        }

        private static LabelControl CreateLabelControl(string text)
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

        private async void LoadEntityData(StockTransfer entity)
        {
            _entity = entity;
            if (_fromLocationEdit != null) _fromLocationEdit.EditValue = entity.FromLocationId;
            if (_toLocationEdit != null) _toLocationEdit.EditValue = entity.ToLocationId;
            if (_transferDateEdit != null) _transferDateEdit.DateTime = entity.TransferDate;
            if (_notesEdit != null) _notesEdit.Text = entity.Notes ?? string.Empty;

            await LoadLocationsAsync();
            await LoadDetailsAsync();
        }

        private async Task LoadLocationsAsync()
        {
            if (_locationRepo == null) return;
            try
            {
                var locations = await _locationRepo.GetAllAsync();
                if (_fromLocationEdit != null)
                {
                    _fromLocationEdit.Properties.DataSource = locations.OrderBy(l => l.Name).ToList();
                    _fromLocationEdit.Properties.DisplayMember = "Name";
                    _fromLocationEdit.Properties.ValueMember = "Id";
                }
                if (_toLocationEdit != null)
                {
                    _toLocationEdit.Properties.DataSource = locations.OrderBy(l => l.Name).ToList();
                    _toLocationEdit.Properties.DisplayMember = "Name";
                    _toLocationEdit.Properties.ValueMember = "Id";
                }
            }
            catch { }
        }

        private async Task LoadDetailsAsync()
        {
            if (_detailRepo == null) return;
            if (_entity.Id > 0)
            {
                try
                {
                    var allDetails = await _detailRepo.GetAllAsync();
                    _details = allDetails.Where(d => d.StockTransferId == _entity.Id).ToList();
                }
                catch { }
            }
            if (_detailsGrid != null) _detailsGrid.DataSource = _details;
            _detailsView?.RefreshData();
        }

        private void OnAddDetailAsync(object? sender, EventArgs e)
        {
            if (_productRepo == null) { ShowError("Product service not available."); return; }
            ShowError("Add detail functionality to be implemented."); // Simplified: would need product picker
        }

        private void OnRemoveDetail(object? sender, EventArgs e)
        {
            if (_detailsView == null || _details == null) return;
            if (_detailsView.FocusedRowHandle >= 0 && _detailsView.FocusedRowHandle < _details.Count)
            {
                _details.RemoveAt(_detailsView.FocusedRowHandle);
                if (_detailsGrid != null) _detailsGrid.DataSource = null;
                if (_detailsGrid != null) _detailsGrid.DataSource = _details;
                _detailsView.RefreshData();
            }
        }

        protected override bool ValidateForm()
        {
            if (_fromLocationEdit?.EditValue is not int fromId || fromId <= 0) { ShowError("From location required."); return false; }
            if (_toLocationEdit?.EditValue is not int toId || toId <= 0) { ShowError("To location required."); return false; }
            if (fromId == toId) { ShowError("From and To locations must differ."); return false; }
            return true;
        }

        public StockTransfer GetEntity() => _entity;
        public List<StockTransferDetail> GetDetails() => _details;

        public async Task<bool> SaveAsync()
        {
            try
            {
                if (!ValidateForm()) return false;
                if (_service == null) { ShowError("Service not available."); return false; }

                SaveEntityData(_entity);
                if (_isNew)
                {
                    var created = await _service.CreateTransferAsync(_entity, _details, 1, CancellationToken.None);
                    _entity = created;
                }
                else
                {
                    await _service.ReleaseTransferAsync(_entity.Id, 1, CancellationToken.None);
                }
                return true;
            }
            catch (Exception ex)
            {
                ShowError($"Save failed: {ex.Message}");
                return false;
            }
        }

        public override void SaveEntityData(object entity)
        {
            var transfer = (StockTransfer)entity;
            transfer.TransferNo = _entity.TransferNo;
            transfer.FromLocationId = _fromLocationEdit?.EditValue is int fromId ? fromId : transfer.FromLocationId;
            transfer.ToLocationId = _toLocationEdit?.EditValue is int toId ? toId : transfer.ToLocationId;
            transfer.TransferDate = _transferDateEdit?.DateTime ?? transfer.TransferDate;
            transfer.Notes = _notesEdit?.Text;
            transfer.Status = _entity.Status;
        }
    }
}
