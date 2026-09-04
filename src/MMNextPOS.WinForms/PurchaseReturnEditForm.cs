using System;
using System.Drawing;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using MMNextPOS.Domain.Models;

namespace MMNextPOS.WinForms
{
    public partial class PurchaseReturnEditForm : EditFormBase
    {
        private PurchaseReturn _purchaseReturn = null!;
        private bool _isNew = true;

        private TextEdit _returnNoEdit = null!;
        private LookUpEdit _purchaseLookup = null!;
        private LookUpEdit _supplierLookup = null!;
        private DateEdit _returnDateEdit = null!;
        private SpinEdit _totalAmountEdit = null!;
        private MemoEdit _reasonEdit = null!;
        private ComboBoxEdit _statusCombo = null!;

        public PurchaseReturnEditForm() : this(new PurchaseReturn()) { }

        public PurchaseReturnEditForm(PurchaseReturn purchaseReturn)
        {
            _purchaseReturn = purchaseReturn ?? new PurchaseReturn();
            _isNew = _purchaseReturn.Id == 0;

            InitializeComponent();
            LoadEntityData(_purchaseReturn);
        }

        private void InitializeComponent()
        {
            Text = _isNew ? "New Purchase Return" : "Edit Purchase Return";
            Size = new Size(600, 550);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;

            var mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 10,
                Padding = new Padding(20)
            };
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            for (int i = 0; i < 9; i++)
                mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            // Return No
            mainLayout.Controls.Add(CreateLabel("Return # *:"), 0, 0);
            _returnNoEdit = new TextEdit { Dock = DockStyle.Fill, Properties = { MaxLength = 50 } };
            _returnNoEdit.EditValueChanged += (s, e) => ValidateForm();
            mainLayout.Controls.Add(_returnNoEdit, 1, 0);

            // Original Purchase
            mainLayout.Controls.Add(CreateLabel("Original Purchase *:"), 0, 1);
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
            _purchaseLookup.EditValueChanged += (s, e) => ValidateForm();
            mainLayout.Controls.Add(_purchaseLookup, 1, 1);

            // Supplier
            mainLayout.Controls.Add(CreateLabel("Supplier *:"), 0, 2);
            var _supplierLookup = new LookUpEdit
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
            mainLayout.Controls.Add(_supplierLookup, 1, 2);

            // Return Date
            mainLayout.Controls.Add(CreateLabel("Return Date *:"), 0, 3);
            var _returnDateEdit = new DateEdit
            {
                Dock = DockStyle.Fill,
                EditValue = DateTime.Today,
                Properties =
                {
                    AutoHeight = false,
                    CalendarView = DevExpress.XtraEditors.Repository.CalendarView.Classic
                }
            };
            mainLayout.Controls.Add(_returnDateEdit, 1, 3);

            // Total Amount
            mainLayout.Controls.Add(CreateLabel("Total Amount *:"), 0, 4);
            _totalAmountEdit = new SpinEdit
            {
                Dock = DockStyle.Fill,
                Properties = { AutoHeight = false, MinValue = 0, MaxValue = 999999999, IsFloatValue = true, Increment = 0.01m }
            };
            mainLayout.Controls.Add(_totalAmountEdit, 1, 4);

            // Reason
            mainLayout.Controls.Add(CreateLabel("Reason *:"), 0, 5);
            _reasonEdit = new MemoEdit { Dock = DockStyle.Fill, Properties = { AutoHeight = false, MaxLength = 500 } };
            mainLayout.Controls.Add(_reasonEdit, 1, 5);

            // Status
            mainLayout.Controls.Add(CreateLabel("Status:"), 0, 6);
            _statusCombo = new ComboBoxEdit
            {
                Dock = DockStyle.Fill,
                Properties =
                {
                    AutoHeight = false,
                    TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor,
                    Items = { "Active", "Cancelled" }
                }
            };
            _statusCombo.SelectedIndex = 0;
            mainLayout.Controls.Add(_statusCombo, 1, 7);

            // Notes
            mainLayout.Controls.Add(CreateLabel("Notes:"), 0, 8);
            var notesEdit = new MemoEdit { Dock = DockStyle.Fill, Properties = { AutoHeight = false, MaxLength = 500 } };
            mainLayout.Controls.Add(notesEdit, 1, 8);

            // Notes
            mainLayout.Controls.Add(CreateLabel("Notes:"), 0, 9);
            var notesEdit2 = new MemoEdit { Dock = DockStyle.Fill, Properties = { AutoHeight = false, MaxLength = 500 } };
            mainLayout.Controls.Add(notesEdit2, 1, 9);

            // Status
            mainLayout.Controls.Add(CreateLabel("Status:"), 0, 10);
            var _statusCombo2 = new ComboBoxEdit
            {
                Dock = DockStyle.Fill,
                Properties =
                {
                    AutoHeight = false,
                    TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor,
                    Items = { "Active", "Cancelled" }
                }
            };
            _statusCombo2.SelectedIndex = 0;
            mainLayout.Controls.Add(_statusCombo2, 1, 10);

            Controls.Add(mainLayout);

            _okButton.Enabled = _isNew;
            _okButton.Click += (s, e) => { if (ValidateForm()) DialogResult = DialogResult.OK; };
            _cancelButton.Click += (s, e) => DialogResult = DialogResult.Cancel;
        }

        protected override bool ValidateForm()
        {
            bool isValid = true;

            if (string.IsNullOrWhiteSpace(_returnNoEdit.Text))
                isValid = false;

            if (_purchaseLookup.EditValue == null)
                isValid = false;

            if (_supplierLookup.EditValue == null)
                isValid = false;

            if (_returnDateEdit.EditValue == null)
                isValid = false;

            _okButton.Enabled = isValid;
            return isValid;
        }

        protected override void LoadEntityData(object entity)
        {
            var pr = (PurchaseReturn)entity;
            _returnNoEdit.Text = pr.ReturnNo;

            if (_purchaseLookup.Properties.DataSource != null)
                _purchaseLookup.EditValue = pr.PurchaseId;

            if (_supplierLookup.Properties.DataSource != null)
                _supplierLookup.EditValue = pr.SupplierId;

            _returnDateEdit.EditValue = pr.ReturnDate;
            _totalAmountEdit.Value = pr.TotalAmount;
            _reasonEdit.Text = pr.Reason ?? string.Empty;
            _statusCombo.Text = pr.Status;

            ValidateForm();
        }

        protected override void SaveEntityData(object entity)
        {
            var pr = (PurchaseReturn)entity;
            pr.ReturnNo = _returnNoEdit.Text.Trim();
            pr.PurchaseId = Convert.ToInt32(_purchaseLookup.EditValue);
            pr.SupplierId = Convert.ToInt32(_supplierLookup.EditValue);
            pr.ReturnDate = _returnDateEdit.DateTime;
            pr.TotalAmount = _totalAmountEdit.Value;
            pr.Reason = string.IsNullOrWhiteSpace(_reasonEdit.Text) ? null : _reasonEdit.Text.Trim();
            pr.Status = _statusCombo.Text;
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