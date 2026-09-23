using System;
using System.Drawing;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using MMNextPOS.Domain.Models;

namespace MMNextPOS.WinForms
{
    /// <summary>
    /// Edit form for SaleTemp (held/draft sale) header metadata.
    /// Line items are edited via the sale screen itself; this form adjusts
    /// the hold record's header fields only.
    /// </summary>
    public partial class SaleTempEditForm : EditFormBase
    {
        private SaleTemp _saleTemp = null!;
        private bool _isNew = true;

        private SpinEdit _customerIdEdit = null!;
        private TextEdit _customerNameEdit = null!;
        private TextEdit _customerPhoneEdit = null!;
        private DateEdit _saleDateEdit = null!;
        private SpinEdit _totalAmountEdit = null!;
        private SpinEdit _discountAmountEdit = null!;
        private SpinEdit _taxAmountEdit = null!;
        private SpinEdit _netAmountEdit = null!;
        private ComboBoxEdit _statusCombo = null!;
        private SpinEdit _locationIdEdit = null!;
        private MemoEdit _notesEdit = null!;

        public SaleTempEditForm() : this(new SaleTemp()) { }

        public SaleTempEditForm(SaleTemp saleTemp)
        {
            _saleTemp = saleTemp ?? new SaleTemp();
            _isNew = _saleTemp.Id == 0;

            InitializeComponent();
            LoadEntityData(_saleTemp);
        }

        private void InitializeComponent()
        {
            Text = _isNew ? "New Held Sale" : "Edit Held Sale";
            Size = new Size(520, 620);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;

            var mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 12,
                Padding = new Padding(20)
            };
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130));
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            for (int i = 0; i < 11; i++)
                mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 45));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            // Customer Id (0 = walk-in)
            mainLayout.Controls.Add(CreateLabel("Customer Id:"), 0, 0);
            _customerIdEdit = new SpinEdit { Dock = DockStyle.Fill, Properties = { MinValue = 0, MaxValue = int.MaxValue } };
            mainLayout.Controls.Add(_customerIdEdit, 1, 0);

            // Customer Name
            mainLayout.Controls.Add(CreateLabel("Customer Name:"), 0, 1);
            _customerNameEdit = new TextEdit { Dock = DockStyle.Fill, Properties = { MaxLength = 150 } };
            mainLayout.Controls.Add(_customerNameEdit, 1, 1);

            // Customer Phone
            mainLayout.Controls.Add(CreateLabel("Customer Phone:"), 0, 2);
            _customerPhoneEdit = new TextEdit { Dock = DockStyle.Fill, Properties = { MaxLength = 30 } };
            mainLayout.Controls.Add(_customerPhoneEdit, 1, 2);

            // Sale Date
            mainLayout.Controls.Add(CreateLabel("Sale Date:"), 0, 3);
            _saleDateEdit = new DateEdit { Dock = DockStyle.Fill, Properties = { CalendarView = DevExpress.XtraEditors.Repository.CalendarView.Classic } };
            mainLayout.Controls.Add(_saleDateEdit, 1, 3);

            // Total Amount
            mainLayout.Controls.Add(CreateLabel("Total Amount:"), 0, 4);
            _totalAmountEdit = new SpinEdit { Dock = DockStyle.Fill, Properties = { MinValue = 0, MaxValue = decimal.MaxValue, DisplayFormat = { FormatString = "n2", FormatType = DevExpress.Utils.FormatType.Numeric } } };
            mainLayout.Controls.Add(_totalAmountEdit, 1, 4);

            // Discount Amount
            mainLayout.Controls.Add(CreateLabel("Discount:"), 0, 5);
            _discountAmountEdit = new SpinEdit { Dock = DockStyle.Fill, Properties = { MinValue = 0, MaxValue = decimal.MaxValue, DisplayFormat = { FormatString = "n2", FormatType = DevExpress.Utils.FormatType.Numeric } } };
            mainLayout.Controls.Add(_discountAmountEdit, 1, 5);

            // Tax Amount
            mainLayout.Controls.Add(CreateLabel("Tax:"), 0, 6);
            _taxAmountEdit = new SpinEdit { Dock = DockStyle.Fill, Properties = { MinValue = 0, MaxValue = decimal.MaxValue, DisplayFormat = { FormatString = "n2", FormatType = DevExpress.Utils.FormatType.Numeric } } };
            mainLayout.Controls.Add(_taxAmountEdit, 1, 6);

            // Net Amount
            mainLayout.Controls.Add(CreateLabel("Net Amount:"), 0, 7);
            _netAmountEdit = new SpinEdit { Dock = DockStyle.Fill, Properties = { MinValue = 0, MaxValue = decimal.MaxValue, DisplayFormat = { FormatString = "n2", FormatType = DevExpress.Utils.FormatType.Numeric } } };
            mainLayout.Controls.Add(_netAmountEdit, 1, 7);

            // Status
            mainLayout.Controls.Add(CreateLabel("Status:"), 0, 8);
            _statusCombo = new ComboBoxEdit { Dock = DockStyle.Fill, Properties = { TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor } };
            _statusCombo.Properties.Items.AddRange(new[] { "Draft", "Finalized", "Voided" });
            mainLayout.Controls.Add(_statusCombo, 1, 8);

            // Location Id (0 = none)
            mainLayout.Controls.Add(CreateLabel("Location Id:"), 0, 9);
            _locationIdEdit = new SpinEdit { Dock = DockStyle.Fill, Properties = { MinValue = 0, MaxValue = int.MaxValue } };
            mainLayout.Controls.Add(_locationIdEdit, 1, 9);

            // Notes
            mainLayout.Controls.Add(CreateLabel("Notes:"), 0, 10);
            _notesEdit = new MemoEdit { Dock = DockStyle.Fill, Properties = { MaxLength = 500 } };
            mainLayout.Controls.Add(_notesEdit, 1, 10);

            Controls.Add(mainLayout);
        }

        protected override bool ValidateForm()
        {
            return true;
        }

        public override void LoadEntityData(object entity)
        {
            _saleTemp = (SaleTemp)entity;
            _customerIdEdit.Value = _saleTemp.CustomerId ?? 0;
            _customerNameEdit.Text = _saleTemp.CustomerName ?? string.Empty;
            _customerPhoneEdit.Text = _saleTemp.CustomerPhone ?? string.Empty;
            _saleDateEdit.EditValue = _saleTemp.SaleDate == default ? DateTime.Today : _saleTemp.SaleDate;
            _totalAmountEdit.Value = _saleTemp.TotalAmount;
            _discountAmountEdit.Value = _saleTemp.DiscountAmount;
            _taxAmountEdit.Value = _saleTemp.TaxAmount;
            _netAmountEdit.Value = _saleTemp.NetAmount;
            _statusCombo.EditValue = string.IsNullOrEmpty(_saleTemp.Status) ? "Draft" : _saleTemp.Status;
            _locationIdEdit.Value = _saleTemp.LocationId ?? 0;
            _notesEdit.Text = _saleTemp.Notes ?? string.Empty;

            ValidateForm();
        }

        public override void SaveEntityData(object entity)
        {
            var saleTemp = (SaleTemp)entity;
            saleTemp.CustomerId = _customerIdEdit.Value <= 0 ? null : (int)_customerIdEdit.Value;
            saleTemp.CustomerName = string.IsNullOrWhiteSpace(_customerNameEdit.Text) ? null : _customerNameEdit.Text.Trim();
            saleTemp.CustomerPhone = string.IsNullOrWhiteSpace(_customerPhoneEdit.Text) ? null : _customerPhoneEdit.Text.Trim();
            saleTemp.SaleDate = _saleDateEdit.DateTime == default ? DateTime.Today : _saleDateEdit.DateTime;
            saleTemp.TotalAmount = _totalAmountEdit.Value;
            saleTemp.DiscountAmount = _discountAmountEdit.Value;
            saleTemp.TaxAmount = _taxAmountEdit.Value;
            saleTemp.NetAmount = _netAmountEdit.Value;
            saleTemp.Status = _statusCombo.EditValue?.ToString() ?? "Draft";
            saleTemp.LocationId = _locationIdEdit.Value <= 0 ? null : (int)_locationIdEdit.Value;
            saleTemp.Notes = string.IsNullOrWhiteSpace(_notesEdit.Text) ? null : _notesEdit.Text.Trim();
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
}
