using System;
using System.Drawing;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using MMNextPOS.Domain.Models;

namespace MMNextPOS.WinForms
{
    public partial class PurchaseEditForm : EditFormBase
    {
        private Purchase _purchase = null!;
        private bool _isNew = true;

        private TextEdit _invoiceNoEdit = null!;
        private LookUpEdit _supplierLookup = null!;
        private DateEdit _purchaseDateEdit = null!;
        private SpinEdit _totalAmountEdit = null!;
        private SpinEdit _discountAmountEdit = null!;
        private SpinEdit _taxAmountEdit = null!;
        private SpinEdit _netAmountEdit = null!;
        private SpinEdit _paidAmountEdit = null!;
        private ComboBoxEdit _statusCombo = null!;
        private LookUpEdit _locationLookup = null!;
        private MemoEdit _notesEdit = null!;

        public PurchaseEditForm() : this(new Purchase()) { }

        public PurchaseEditForm(Purchase purchase)
        {
            _purchase = purchase ?? new Purchase();
            _isNew = _purchase.Id == 0;

            InitializeComponent();
            LoadEntityData(_purchase);
        }

        private void InitializeComponent()
        {
            Text = _isNew ? "New Purchase" : "Edit Purchase";
            Size = new Size(600, 650);
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
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            for (int i = 0; i < 11; i++)
                mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            // Invoice No
            mainLayout.Controls.Add(CreateLabel("Invoice # *:"), 0, 0);
            _invoiceNoEdit = new TextEdit { Dock = DockStyle.Fill, Properties = { MaxLength = 50 } };
            _invoiceNoEdit.EditValueChanged += (s, e) => ValidateForm();
            mainLayout.Controls.Add(_invoiceNoEdit, 1, 0);

            // Supplier
            mainLayout.Controls.Add(CreateLabel("Supplier *:"), 0, 1);
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
            mainLayout.Controls.Add(_supplierLookup, 1, 1);

            // Purchase Date
            mainLayout.Controls.Add(CreateLabel("Date *:"), 0, 2);
            _purchaseDateEdit = new DateEdit
            {
                Dock = DockStyle.Fill,
                EditValue = DateTime.Today,
                Properties =
                {
                    AutoHeight = false,
                    CalendarView = DevExpress.XtraEditors.Repository.CalendarView.Classic
                }
            };
            mainLayout.Controls.Add(_purchaseDateEdit, 1, 2);

            // Total Amount
            mainLayout.Controls.Add(CreateLabel("Total Amount:"), 0, 3);
            _totalAmountEdit = new SpinEdit
            {
                Dock = DockStyle.Fill,
                Properties = { AutoHeight = false, MinValue = 0, MaxValue = 999999999, IsFloatValue = true, Increment = 0.01m }
            };
            mainLayout.Controls.Add(_totalAmountEdit, 1, 3);

            // Discount Amount
            mainLayout.Controls.Add(CreateLabel("Discount:"), 0, 4);
            _discountAmountEdit = new SpinEdit
            {
                Dock = DockStyle.Fill,
                Properties = { AutoHeight = false, MinValue = 0, MaxValue = 999999999, IsFloatValue = true, Increment = 0.01m }
            };
            mainLayout.Controls.Add(_discountAmountEdit, 1, 4);

            // Tax Amount
            mainLayout.Controls.Add(CreateLabel("Tax:"), 0, 5);
            _taxAmountEdit = new SpinEdit
            {
                Dock = DockStyle.Fill,
                Properties = { AutoHeight = false, MinValue = 0, MaxValue = 999999999, IsFloatValue = true, Increment = 0.01m }
            };
            mainLayout.Controls.Add(_taxAmountEdit, 1, 5);

            // Net Amount
            mainLayout.Controls.Add(CreateLabel("Net Amount:"), 0, 6);
            _netAmountEdit = new SpinEdit
            {
                Dock = DockStyle.Fill,
                Properties = { AutoHeight = false, MinValue = 0, MaxValue = 999999999, IsFloatValue = true, Increment = 0.01m }
            };
            mainLayout.Controls.Add(_netAmountEdit, 1, 6);

            // Paid Amount
            mainLayout.Controls.Add(CreateLabel("Paid Amount:"), 0, 7);
            _paidAmountEdit = new SpinEdit
            {
                Dock = DockStyle.Fill,
                Properties = { AutoHeight = false, MinValue = 0, MaxValue = 999999999, IsFloatValue = true, Increment = 0.01m }
            };
            mainLayout.Controls.Add(_paidAmountEdit, 1, 7);

            // Status
            mainLayout.Controls.Add(CreateLabel("Status:"), 0, 8);
            _statusCombo = new ComboBoxEdit
            {
                Dock = DockStyle.Fill,
                Properties =
                {
                    AutoHeight = false,
                    TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor,
                    Items = { "Active", "Hold", "Returned", "Cancelled" }
                }
            };
            _statusCombo.SelectedIndex = 0;
            mainLayout.Controls.Add(_statusCombo, 1, 8);

            // Location
            mainLayout.Controls.Add(CreateLabel("Location:"), 0, 9);
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
            mainLayout.Controls.Add(_locationLookup, 1, 9);

            // Notes
            mainLayout.Controls.Add(CreateLabel("Notes:"), 0, 10);
            var _notesEdit = new MemoEdit
            {
                Dock = DockStyle.Fill,
                Properties = { AutoHeight = false, MaxLength = 500 }
            };
            mainLayout.Controls.Add(_notesEdit, 1, 10);

            Controls.Add(mainLayout);

            _okButton.Enabled = _isNew;
            _okButton.Click += (s, e) => { if (ValidateForm()) DialogResult = DialogResult.OK; };
            _cancelButton.Click += (s, e) => DialogResult = DialogResult.Cancel;
        }

        protected override bool ValidateForm()
        {
            bool isValid = true;

            if (string.IsNullOrWhiteSpace(_invoiceNoEdit.Text))
                isValid = false;

            if (_supplierLookup.EditValue == null)
                isValid = false;

            if (_purchaseDateEdit.EditValue == null)
                isValid = false;

            _okButton.Enabled = isValid;
            return isValid;
        }

        protected override void LoadEntityData(object entity)
        {
            var purchase = (Purchase)entity;
            _invoiceNoEdit.Text = purchase.InvoiceNo;

            if (purchase.SupplierId > 0)
                _supplierLookup.EditValue = purchase.SupplierId;

            _purchaseDateEdit.EditValue = purchase.PurchaseDate;
            _totalAmountEdit.Value = purchase.TotalAmount;
            _discountAmountEdit.Value = purchase.DiscountAmount;
            _taxAmountEdit.Value = purchase.TaxAmount;
            _netAmountEdit.Value = purchase.NetAmount;
            _paidAmountEdit.Value = purchase.PaidAmount;
            _statusCombo.Text = purchase.Status;

            if (purchase.LocationId.HasValue)
                _locationLookup.EditValue = purchase.LocationId.Value;

            _notesEdit.Text = purchase.Notes ?? string.Empty;

            ValidateForm();
        }

        protected override void SaveEntityData(object entity)
        {
            var purchase = (Purchase)entity;
            purchase.InvoiceNo = _invoiceNoEdit.Text.Trim();
            purchase.SupplierId = Convert.ToInt32(_supplierLookup.EditValue);
            purchase.PurchaseDate = _purchaseDateEdit.DateTime;
            purchase.TotalAmount = _totalAmountEdit.Value;
            purchase.DiscountAmount = _discountAmountEdit.Value;
            purchase.TaxAmount = _taxAmountEdit.Value;
            purchase.NetAmount = _netAmountEdit.Value;
            purchase.PaidAmount = _paidAmountEdit.Value;
            purchase.Status = _statusCombo.Text;
            purchase.LocationId = _locationLookup.EditValue == null ? null : (int?)_locationLookup.EditValue;
            purchase.Notes = string.IsNullOrWhiteSpace(_notesEdit.Text) ? null : _notesEdit.Text.Trim();
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