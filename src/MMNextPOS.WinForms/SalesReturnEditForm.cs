using System;
using System.Drawing;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using MMNextPOS.Domain.Models;

namespace MMNextPOS.WinForms
{
    /// <summary>
    /// Edit form for a sales return header. Line items are processed via the
    /// sales return flow (stock restore + customer credit); this form adjusts
    /// the return header fields only.
    /// </summary>
    public partial class SalesReturnEditForm : EditFormBase
    {
        private SalesReturn _salesReturn = null!;
        private bool _isNew = true;

        private TextEdit _returnNoEdit = null!;
        private SpinEdit _saleIdEdit = null!;
        private SpinEdit _customerIdEdit = null!;
        private DateEdit _returnDateEdit = null!;
        private SpinEdit _totalAmountEdit = null!;
        private MemoEdit _reasonEdit = null!;
        private ComboBoxEdit _statusCombo = null!;

        public SalesReturnEditForm() : this(new SalesReturn()) { }

        public SalesReturnEditForm(SalesReturn salesReturn)
        {
            _salesReturn = salesReturn ?? new SalesReturn();
            _isNew = _salesReturn.Id == 0;

            InitializeComponent();
            LoadEntityData(_salesReturn);
        }

        private void InitializeComponent()
        {
            Text = _isNew ? "New Sales Return" : "Edit Sales Return";
            Size = new Size(520, 480);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;

            var mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 8,
                Padding = new Padding(20)
            };
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130));
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            for (int i = 0; i < 7; i++)
                mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 45));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            // Return No
            mainLayout.Controls.Add(CreateLabel("Return No *:"), 0, 0);
            _returnNoEdit = new TextEdit { Dock = DockStyle.Fill, Properties = { MaxLength = 50 } };
            _returnNoEdit.EditValueChanged += (s, e) => ValidateForm();
            mainLayout.Controls.Add(_returnNoEdit, 1, 0);

            // Sale Id
            mainLayout.Controls.Add(CreateLabel("Sale Id:"), 0, 1);
            _saleIdEdit = new SpinEdit { Dock = DockStyle.Fill, Properties = { MinValue = 0, MaxValue = int.MaxValue } };
            mainLayout.Controls.Add(_saleIdEdit, 1, 1);

            // Customer Id
            mainLayout.Controls.Add(CreateLabel("Customer Id:"), 0, 2);
            _customerIdEdit = new SpinEdit { Dock = DockStyle.Fill, Properties = { MinValue = 0, MaxValue = int.MaxValue } };
            mainLayout.Controls.Add(_customerIdEdit, 1, 2);

            // Return Date
            mainLayout.Controls.Add(CreateLabel("Return Date:"), 0, 3);
            _returnDateEdit = new DateEdit { Dock = DockStyle.Fill, Properties = { CalendarView = DevExpress.XtraEditors.Repository.CalendarView.Classic } };
            mainLayout.Controls.Add(_returnDateEdit, 1, 3);

            // Total Amount
            mainLayout.Controls.Add(CreateLabel("Total Amount:"), 0, 4);
            _totalAmountEdit = new SpinEdit { Dock = DockStyle.Fill, Properties = { MinValue = 0, MaxValue = decimal.MaxValue, DisplayFormat = { FormatString = "n2", FormatType = DevExpress.Utils.FormatType.Numeric } } };
            mainLayout.Controls.Add(_totalAmountEdit, 1, 4);

            // Reason
            mainLayout.Controls.Add(CreateLabel("Reason:"), 0, 5);
            _reasonEdit = new MemoEdit { Dock = DockStyle.Fill, Properties = { MaxLength = 500 } };
            mainLayout.Controls.Add(_reasonEdit, 1, 5);

            // Status
            mainLayout.Controls.Add(CreateLabel("Status:"), 0, 6);
            _statusCombo = new ComboBoxEdit { Dock = DockStyle.Fill, Properties = { TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor } };
            _statusCombo.Properties.Items.AddRange(new[] { "Active", "Completed", "Voided" });
            mainLayout.Controls.Add(_statusCombo, 1, 6);

            Controls.Add(mainLayout);
        }

        protected override bool ValidateForm()
        {
            bool isValid = true;

            if (string.IsNullOrWhiteSpace(_returnNoEdit.Text))
                isValid = false;

            _okButton.Enabled = isValid;
            return isValid;
        }

        public override void LoadEntityData(object entity)
        {
            _salesReturn = (SalesReturn)entity;
            _returnNoEdit.Text = _salesReturn.ReturnNo;
            _saleIdEdit.Value = _salesReturn.SaleId;
            _customerIdEdit.Value = _salesReturn.CustomerId;
            _returnDateEdit.EditValue = _salesReturn.ReturnDate == default ? DateTime.Today : _salesReturn.ReturnDate;
            _totalAmountEdit.Value = _salesReturn.TotalAmount;
            _reasonEdit.Text = _salesReturn.Reason ?? string.Empty;
            _statusCombo.EditValue = string.IsNullOrEmpty(_salesReturn.Status) ? "Active" : _salesReturn.Status;

            ValidateForm();
        }

        public override void SaveEntityData(object entity)
        {
            var salesReturn = (SalesReturn)entity;
            salesReturn.ReturnNo = _returnNoEdit.Text.Trim();
            salesReturn.SaleId = (int)_saleIdEdit.Value;
            salesReturn.CustomerId = (int)_customerIdEdit.Value;
            salesReturn.ReturnDate = _returnDateEdit.DateTime == default ? DateTime.Today : _returnDateEdit.DateTime;
            salesReturn.TotalAmount = _totalAmountEdit.Value;
            salesReturn.Reason = string.IsNullOrWhiteSpace(_reasonEdit.Text) ? null : _reasonEdit.Text.Trim();
            salesReturn.Status = _statusCombo.EditValue?.ToString() ?? "Active";
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
