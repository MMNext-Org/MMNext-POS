using System;
using System.Drawing;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using MMNextPOS.Domain.Models;

namespace MMNextPOS.WinForms
{
    public partial class PaymentEditForm : EditFormBase
    {
        private Payment _payment = null!;
        private bool _isNew = true;

        private TextEdit _paymentNoEdit = null!;
        private LookUpEdit _customerLookup = null!;
        private LookUpEdit _supplierLookup = null!;
        private DateEdit _paymentDateEdit = null!;
        private SpinEdit _amountEdit = null!;
        private ComboBoxEdit _paymentTypeCombo = null!;
        private ComboBoxEdit _methodCombo = null!;
        private TextEdit _referenceNoEdit = null!;
        private TextEdit _bankNameEdit = null!;
        private TextEdit _chequeNoEdit = null!;
        private DateEdit _chequeDateEdit = null!;
        private ComboBoxEdit _statusCombo = null!;
        private MemoEdit _notesEdit = null!;

        public PaymentEditForm() : this(new Payment()) { }

        public PaymentEditForm(Payment payment)
        {
            _payment = payment ?? new Payment();
            _isNew = _payment.Id == 0;

            InitializeComponent();
            LoadEntityData(_payment);
        }

        private void InitializeComponent()
        {
            Text = _isNew ? "New Payment" : "Edit Payment";
            Size = new Size(600, 500);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;

            var mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 11,
                Padding = new Padding(20)
            };
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130));
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            for (int i = 0; i < 10; i++)
                mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 45));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            // Payment No
            mainLayout.Controls.Add(CreateLabel("Payment # *:"), 0, 0);
            _paymentNoEdit = new TextEdit { Dock = DockStyle.Fill, Properties = { MaxLength = 50 } };
            _paymentNoEdit.EditValueChanged += (s, e) => ValidateForm();
            mainLayout.Controls.Add(_paymentNoEdit, 1, 0);

            // Payment Type
            mainLayout.Controls.Add(CreateLabel("Payment Type *:"), 0, 1);
            _paymentTypeCombo = new ComboBoxEdit
            {
                Dock = DockStyle.Fill,
                Properties =
                {
                    AutoHeight = false,
                    TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor,
                    Items = { "Customer", "Supplier" }
                }
            };
            _paymentTypeCombo.SelectedIndex = 0;
            _paymentTypeCombo.EditValueChanged += (s, e) => ValidateForm();
            mainLayout.Controls.Add(_paymentTypeCombo, 1, 1);

            // Customer
            mainLayout.Controls.Add(CreateLabel("Customer:"), 0, 2);
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
            mainLayout.Controls.Add(_customerLookup, 1, 2);

            // Supplier
            mainLayout.Controls.Add(CreateLabel("Supplier:"), 0, 3);
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
            mainLayout.Controls.Add(_supplierLookup, 1, 3);

            // Payment Date
            mainLayout.Controls.Add(CreateLabel("Date *:"), 0, 4);
            _paymentDateEdit = new DateEdit
            {
                Dock = DockStyle.Fill,
                EditValue = DateTime.Today,
                Properties =
                {
                    AutoHeight = false,
                    CalendarView = DevExpress.XtraEditors.Repository.CalendarView.Classic
                }
            };
            mainLayout.Controls.Add(_paymentDateEdit, 1, 4);

            // Amount
            mainLayout.Controls.Add(CreateLabel("Amount *:"), 0, 5);
            _amountEdit = new SpinEdit
            {
                Dock = DockStyle.Fill,
                Properties = { AutoHeight = false, MinValue = 0, MaxValue = 999999999, IsFloatValue = true, Increment = 0.01m }
            };
            mainLayout.Controls.Add(_amountEdit, 1, 5);

            // Method
            mainLayout.Controls.Add(CreateLabel("Method *:"), 0, 6);
            _methodCombo = new ComboBoxEdit
            {
                Dock = DockStyle.Fill,
                Properties =
                {
                    AutoHeight = false,
                    TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor,
                    Items = { "Cash", "Bank", "Cheque", "Card", "Mobile" }
                }
            };
            _methodCombo.SelectedIndex = 0;
            mainLayout.Controls.Add(_methodCombo, 1, 6);

            // Reference No
            mainLayout.Controls.Add(CreateLabel("Reference No:"), 0, 7);
            _referenceNoEdit = new TextEdit { Dock = DockStyle.Fill, Properties = { MaxLength = 100 } };
            mainLayout.Controls.Add(_referenceNoEdit, 1, 7);

            // Bank Name
            mainLayout.Controls.Add(CreateLabel("Bank Name:"), 0, 8);
            _bankNameEdit = new TextEdit { Dock = DockStyle.Fill, Properties = { MaxLength = 100 } };
            mainLayout.Controls.Add(_bankNameEdit, 1, 8);

            // Cheque No
            mainLayout.Controls.Add(CreateLabel("Cheque No:"), 0, 9);
            _chequeNoEdit = new TextEdit { Dock = DockStyle.Fill, Properties = { MaxLength = 50 } };
            mainLayout.Controls.Add(_chequeNoEdit, 1, 9);

            // Cheque Date
            mainLayout.Controls.Add(CreateLabel("Cheque Date:"), 0, 10);
            _chequeDateEdit = new DateEdit
            {
                Dock = DockStyle.Fill,
                Properties =
                {
                    AutoHeight = false,
                    CalendarView = DevExpress.XtraEditors.Repository.CalendarView.Classic
                }
            };
            mainLayout.Controls.Add(_chequeDateEdit, 1, 10);

            // Status
            mainLayout.Controls.Add(CreateLabel("Status *:"), 0, 11);
            _statusCombo = new ComboBoxEdit
            {
                Dock = DockStyle.Fill,
                Properties =
                {
                    AutoHeight = false,
                    TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor,
                    Items = { "Pending", "Cleared", "Bounced", "Cancelled" }
                }
            };
            _statusCombo.SelectedIndex = 0;
            mainLayout.Controls.Add(_statusCombo, 1, 11);

            // Notes
            mainLayout.Controls.Add(CreateLabel("Notes:"), 0, 12);
            _notesEdit = new MemoEdit
            {
                Dock = DockStyle.Fill,
                Properties = { AutoHeight = false, MaxLength = 500 }
            };
            mainLayout.Controls.Add(_notesEdit, 1, 12);

            Controls.Add(mainLayout);

            _okButton.Enabled = _isNew;
            _okButton.Click += (s, e) => { if (ValidateForm()) DialogResult = DialogResult.OK; };
            _cancelButton.Click += (s, e) => DialogResult = DialogResult.Cancel;
        }

        protected override bool ValidateForm()
        {
            bool isValid = true;

            if (string.IsNullOrWhiteSpace(_paymentNoEdit.Text))
                isValid = false;

            if (string.IsNullOrWhiteSpace(_paymentTypeCombo.Text))
                isValid = false;

            if (_amountEdit.EditValue == null)
                isValid = false;

            if (string.IsNullOrWhiteSpace(_methodCombo.Text))
                isValid = false;

            if (string.IsNullOrWhiteSpace(_statusCombo.Text))
                isValid = false;

            _okButton.Enabled = isValid;
            return isValid;
        }

        public override void LoadEntityData(object entity)
        {
            var payment = (Payment)entity;
            _paymentNoEdit.Text = payment.PaymentNo;
            _paymentTypeCombo.Text = payment.PaymentType;

            if (payment.CustomerId.HasValue)
                _customerLookup.EditValue = payment.CustomerId.Value;

            if (payment.SupplierId.HasValue)
                _supplierLookup.EditValue = payment.SupplierId.Value;

            _paymentDateEdit.EditValue = payment.PaymentDate;
            _amountEdit.Value = payment.Amount;
            _methodCombo.Text = payment.Method;
            _referenceNoEdit.Text = payment.ReferenceNo ?? string.Empty;
            _bankNameEdit.Text = payment.BankName ?? string.Empty;
            _chequeNoEdit.Text = payment.ChequeNo ?? string.Empty;

            if (payment.ChequeDate.HasValue)
                _chequeDateEdit.EditValue = payment.ChequeDate.Value;

            _statusCombo.Text = payment.Status;
            _notesEdit.Text = payment.Notes ?? string.Empty;

            ValidateForm();
        }

        public override void SaveEntityData(object entity)
        {
            var payment = (Payment)entity;
            payment.PaymentNo = _paymentNoEdit.Text.Trim();
            payment.PaymentType = _paymentTypeCombo.Text;
            payment.CustomerId = _customerLookup.EditValue == null ? (int?)null : Convert.ToInt32(_customerLookup.EditValue);
            payment.SupplierId = _supplierLookup.EditValue == null ? (int?)null : Convert.ToInt32(_supplierLookup.EditValue);
            payment.PaymentDate = _paymentDateEdit.DateTime;
            payment.Amount = _amountEdit.Value;
            payment.Method = _methodCombo.Text;
            payment.ReferenceNo = _referenceNoEdit.Text.Trim();
            payment.BankName = _bankNameEdit.Text.Trim();
            payment.ChequeNo = _chequeNoEdit.Text.Trim();
            payment.ChequeDate = _chequeDateEdit.DateTime;
            payment.Status = _statusCombo.Text;
            payment.Notes = _notesEdit.Text.Trim();
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