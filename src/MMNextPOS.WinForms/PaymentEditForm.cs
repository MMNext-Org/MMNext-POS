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
        private LookUpEdit _paymentTypeLookup = null!;
        private DateEdit _paymentDateEdit = null!;
        private LookUpEdit _methodLookup = null!;
        private SpinEdit _amountEdit = null!;
        private TextEdit _referenceNoEdit = null!;
        private TextEdit _bankNameEdit = null!;
        private TextEdit _chequeNoEdit = null!;
        private DateEdit _chequeDateEdit = null!;
        private LookUpEdit _saleLookup = null!;
        private LookUpEdit _purchaseLookup = null!;
        private LookUpEdit _customerLookup = null!;
        private LookUpEdit _supplierLookup = null!;
        private MemoEdit _notesEdit = null!;
        private ComboBoxEdit _statusCombo = null!;

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
            Size = new Size(700, 600);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;

            var mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 14,
                Padding = new Padding(20)
            };
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 140));
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            for (int i = 0; i < 13; i++)
                mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 45));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            // Payment No
            mainLayout.Controls.Add(CreateLabel("Payment # *:"), 0, 0);
            _paymentNoEdit = new TextEdit { Dock = DockStyle.Fill, Properties = { MaxLength = 50 } };
            _paymentNoEdit.EditValueChanged += (s, e) => ValidateForm();
            mainLayout.Controls.Add(_paymentNoEdit, 1, 0);

            // Payment Type (Customer / Supplier)
            mainLayout.Controls.Add(CreateLabel("Payment Type *:"), 0, 1);
            _paymentTypeLookup = new LookUpEdit
            {
                Dock = DockStyle.Fill,
                Properties =
                {
                    DisplayMember = "Value",
                    ValueMember = "Key",
                    NullText = "Select payment type...",
                    DataSource = new List<object>
                    {
                        new { Key = "Customer", Value = "Customer" },
                        new { Key = "Supplier", Value = "Supplier" }
                    }
                }
            };
            mainLayout.Controls.Add(_paymentTypeLookup, 1, 1);

            // Date
            mainLayout.Controls.Add(CreateLabel("Date *:"), 0, 2);
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
            mainLayout.Controls.Add(_paymentDateEdit, 1, 2);

            // Method
            mainLayout.Controls.Add(CreateLabel("Method *:"), 0, 3);
            _methodLookup = new LookUpEdit
            {
                Dock = DockStyle.Fill,
                Properties =
                {
                    DisplayMember = "Value",
                    ValueMember = "Key",
                    NullText = "Select method...",
                    DataSource = new List<object>
                    {
                        new { Key = "Cash", Value = "Cash" },
                        new { Key = "Bank", Value = "Bank" },
                        new { Key = "Cheque", Value = "Cheque" },
                        new { Key = "Card", Value = "Card" },
                        new { Key = "Mobile", Value = "Mobile" }
                    }
                }
            };
            mainLayout.Controls.Add(_methodLookup, 1, 3);

            // Amount
            mainLayout.Controls.Add(CreateLabel("Amount *:"), 0, 4);
            _amountEdit = new SpinEdit
            {
                Dock = DockStyle.Fill,
                Properties = { AutoHeight = false, MinValue = 0, MaxValue = 999999999, IsFloatValue = true, Increment = 0.01m }
            };
            mainLayout.Controls.Add(_amountEdit, 1, 4);

            // Reference No
            mainLayout.Controls.Add(CreateLabel("Reference #:"), 0, 5);
            _referenceNoEdit = new TextEdit { Dock = DockStyle.Fill, Properties = { MaxLength = 50 } };
            mainLayout.Controls.Add(_referenceNoEdit, 1, 5);

            // Bank Name
            mainLayout.Controls.Add(CreateLabel("Bank Name:"), 0, 6);
            _bankNameEdit = new TextEdit { Dock = DockStyle.Fill, Properties = { MaxLength = 100 } };
            mainLayout.Controls.Add(_bankNameEdit, 1, 6);

            // Cheque No
            mainLayout.Controls.Add(CreateLabel("Cheque #:"), 0, 7);
            _chequeNoEdit = new TextEdit { Dock = DockStyle.Fill, Properties = { MaxLength = 20 } };
            mainLayout.Controls.Add(_chequeNoEdit, 1, 7);

            // Cheque Date
            mainLayout.Controls.Add(CreateLabel("Cheque Date:"), 0, 8);
            _chequeDateEdit = new DateEdit
            {
                Dock = DockStyle.Fill,
                Properties =
                {
                    AutoHeight = false,
                    CalendarView = DevExpress.XtraEditors.Repository.CalendarView.Classic
                }
            };
            mainLayout.Controls.Add(_chequeDateEdit, 1, 8);

            // Linked Sale
            mainLayout.Controls.Add(CreateLabel("Linked Sale:"), 0, 9);
            _saleLookup = new LookUpEdit
            {
                Dock = DockStyle.Fill,
                Properties =
                {
                    DisplayMember = "InvoiceNo",
                    ValueMember = "Id",
                    NullText = "Select sale...",
                    ShowHeader = false,
                    AutoHeight = false,
                    BestFitMode = DevExpress.XtraEditors.Controls.BestFitMode.BestFitResizePopup,
                    SearchMode = DevExpress.XtraEditors.Controls.SearchMode.AutoFilter
                }
            };
            mainLayout.Controls.Add(_saleLookup, 1, 9);

            // Linked Purchase
            mainLayout.Controls.Add(CreateLabel("Linked Purchase:"), 0, 10);
            _purchaseLookup = new LookUpEdit
            {
                Dock = DockStyle.Fill,
                Properties =
                {
                    DisplayMember = "InvoiceNo",
                    ValueMember = "Id",
                    NullText = "Select purchase...",
                    ShowHeader = false,
                    AutoHeight = false,
                    BestFitMode = DevExpress.XtraEditors.Controls.BestFitMode.BestFitResizePopup,
                    SearchMode = DevExpress.XtraEditors.Controls.SearchMode.AutoFilter
                }
            };
            mainLayout.Controls.Add(_purchaseLookup, 1, 10);

            // Customer
            mainLayout.Controls.Add(CreateLabel("Customer:"), 0, 11);
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
            mainLayout.Controls.Add(_customerLookup, 1, 11);

            // Supplier
            mainLayout.Controls.Add(CreateLabel("Supplier:"), 0, 12);
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
            mainLayout.Controls.Add(_supplierLookup, 1, 12);

            // Notes
            mainLayout.Controls.Add(CreateLabel("Notes:"), 0, 13);
            _notesEdit = new MemoEdit
            {
                Dock = DockStyle.Fill,
                Properties = { AutoHeight = false, MaxLength = 500 }
            };
            mainLayout.Controls.Add(_notesEdit, 1, 13);

            // Status
            mainLayout.Controls.Add(CreateLabel("Status:"), 0, 14);
            _statusCombo = new ComboBoxEdit
            {
                Dock = DockStyle.Fill,
                Properties =
                {
                    Items = { "Cleared", "Pending", "Bounced", "Cancelled" },
                    TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor
                }
            };
            mainLayout.Controls.Add(_statusCombo, 1, 14);

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

            if (_paymentTypeLookup.EditValue == null)
                isValid = false;

            if (_paymentDateEdit.EditValue == null)
                isValid = false;

            if (_methodLookup.EditValue == null)
                isValid = false;

            if (_amountEdit.Value <= 0m)
                isValid = false;

            _okButton.Enabled = isValid;
            return isValid;
        }

        public override void LoadEntityData(object entity)
        {
            var payment = (Payment)entity;
            _paymentNoEdit.Text = payment.PaymentNo;

            // Set payment type based on whether customer or supplier is selected
            if (payment.SupplierId > 0)
                _paymentTypeLookup.EditValue = "Supplier";
            else if (payment.CustomerId > 0)
                _paymentTypeLookup.EditValue = "Customer";

            _paymentDateEdit.EditValue = payment.PaymentDate;
            _methodLookup.EditValue = payment.Method;
            _amountEdit.Value = payment.Amount;
            _referenceNoEdit.Text = payment.ReferenceNo ?? string.Empty;
            _bankNameEdit.Text = payment.BankName ?? string.Empty;
            _chequeNoEdit.Text = payment.ChequeNo ?? string.Empty;

            if (payment.ChequeDate.HasValue)
                _chequeDateEdit.EditValue = payment.ChequeDate.Value;

            if (payment.SaleId.HasValue)
                _saleLookup.EditValue = payment.SaleId.Value;
            else if (payment.PurchaseId.HasValue)
                _purchaseLookup.EditValue = payment.PurchaseId.Value;
            else if (payment.CustomerId > 0)
                _customerLookup.EditValue = payment.CustomerId.Value;
            else if (payment.SupplierId > 0)
                _supplierLookup.EditValue = payment.SupplierId.Value;

            _notesEdit.Text = string.IsNullOrWhiteSpace(payment.Notes) ? null : payment.Notes.Trim();
            _statusCombo.EditValue = payment.Status ?? "Cleared";

            ValidateForm();
        }

        public override void SaveEntityData(object entity)
        {
            var payment = (Payment)entity;
            payment.PaymentNo = _paymentNoEdit.Text.Trim();
            var paymentType = _paymentTypeLookup.EditValue?.ToString();
            // Determine supplier/customer based on payment type
            if (paymentType == "Supplier")
            {
                payment.SupplierId = _supplierLookup.EditValue == null ? 0 : Convert.ToInt32(_supplierLookup.EditValue);
                payment.CustomerId = null;
            }
            else
            {
                payment.CustomerId = _customerLookup.EditValue == null ? 0 : Convert.ToInt32(_customerLookup.EditValue);
                payment.SupplierId = null;
            }
            payment.PaymentDate = _paymentDateEdit.DateTime;
            payment.Method = _methodLookup.EditValue?.ToString() ?? "Cash";
            payment.Amount = _amountEdit.Value;
            payment.ReferenceNo = _referenceNoEdit.Text.Trim();
            payment.BankName = _bankNameEdit.Text.Trim();
            payment.ChequeNo = _chequeNoEdit.Text.Trim();
            payment.ChequeDate = _chequeDateEdit.DateTime;
            payment.Notes = string.IsNullOrWhiteSpace(_notesEdit.Text) ? null : _notesEdit.Text.Trim();
            payment.Status = _statusCombo.EditValue?.ToString() ?? "Cleared";

            // Clear linked entities not selected
            if (paymentType == "Supplier")
            {
                payment.PurchaseId = _purchaseLookup.EditValue == null ? (int?)null : Convert.ToInt32(_purchaseLookup.EditValue);
                payment.SaleId = null;
                payment.CustomerId = null;
            }
            else
            {
                payment.SaleId = _saleLookup.EditValue == null ? (int?)null : Convert.ToInt32(_saleLookup.EditValue);
                payment.PurchaseId = null;
                payment.SupplierId = null;
            }
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
