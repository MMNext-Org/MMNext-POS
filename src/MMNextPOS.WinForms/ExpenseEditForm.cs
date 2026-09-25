using System;
using System.Drawing;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using MMNextPOS.Domain.Models;

namespace MMNextPOS.WinForms
{
    public partial class ExpenseEditForm : EditFormBase
    {
        private Expense _expense = null!;
        private bool _isNew = true;

        private TextEdit _expenseNoEdit = null!;
        private LookUpEdit _expenseTypeLookup = null!;
        private DateEdit _expenseDateEdit = null!;
        private SpinEdit _amountEdit = null!;
        private LookUpEdit _locationLookup = null!;
        private LookUpEdit _paidByLookup = null!;
        private MemoEdit _notesEdit = null!;
        private ComboBoxEdit _statusCombo = null!;

        public ExpenseEditForm() : this(new Expense()) { }

        public ExpenseEditForm(Expense expense)
        {
            _expense = expense ?? new Expense();
            _isNew = _expense.Id == 0;

            InitializeComponent();
            LoadEntityData(_expense);
        }

        private void InitializeComponent()
        {
            Text = _isNew ? "New Expense" : "Edit Expense";
            Size = new Size(600, 500);
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
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 140));
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            for (int i = 0; i < 9; i++)
                mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            // Expense No
            mainLayout.Controls.Add(CreateLabel("Expense # *:"), 0, 0);
            _expenseNoEdit = new TextEdit { Dock = DockStyle.Fill, Properties = { MaxLength = 50 } };
            _expenseNoEdit.EditValueChanged += (s, e) => ValidateForm();
            mainLayout.Controls.Add(_expenseNoEdit, 1, 0);

            // Expense Type
            mainLayout.Controls.Add(CreateLabel("Expense Type *:"), 0, 1);
            _expenseTypeLookup = new LookUpEdit
            {
                Dock = DockStyle.Fill,
                Properties =
                {
                    DisplayMember = "Name",
                    ValueMember = "Id",
                    NullText = "Select expense type...",
                    ShowHeader = false,
                    AutoHeight = false,
                    BestFitMode = DevExpress.XtraEditors.Controls.BestFitMode.BestFitResizePopup,
                    SearchMode = DevExpress.XtraEditors.Controls.SearchMode.AutoFilter
                }
            };
            mainLayout.Controls.Add(_expenseTypeLookup, 1, 1);

            // Date
            mainLayout.Controls.Add(CreateLabel("Date *:"), 0, 2);
            _expenseDateEdit = new DateEdit
            {
                Dock = DockStyle.Fill,
                EditValue = DateTime.Today,
                Properties =
                {
                    AutoHeight = false,
                    CalendarView = DevExpress.XtraEditors.Repository.CalendarView.Classic
                }
            };
            mainLayout.Controls.Add(_expenseDateEdit, 1, 2);

            // Amount
            mainLayout.Controls.Add(CreateLabel("Amount *:"), 0, 3);
            _amountEdit = new SpinEdit
            {
                Dock = DockStyle.Fill,
                Properties = { AutoHeight = false, MinValue = 0, MaxValue = 999999999, IsFloatValue = true, Increment = 0.01m }
            };
            mainLayout.Controls.Add(_amountEdit, 1, 3);

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

        // Paid By (User)
        mainLayout.Controls.Add(CreateLabel("Paid By:"), 0, 5);
        _paidByLookup = new LookUpEdit
        {
            Dock = DockStyle.Fill,
            Properties =
            {
                DisplayMember = "Username",
                ValueMember = "Id",
                NullText = "Select user...",
                ShowHeader = false,
                AutoHeight = false,
                BestFitMode = DevExpress.XtraEditors.Controls.BestFitMode.BestFitResizePopup,
                SearchMode = DevExpress.XtraEditors.Controls.SearchMode.AutoFilter
            }
        };
        mainLayout.Controls.Add(_paidByLookup, 1, 5);

        // Status
        mainLayout.Controls.Add(CreateLabel("Status:"), 0, 6);
        _statusCombo = new ComboBoxEdit
        {
            Dock = DockStyle.Fill,
            Properties =
            {
                AutoHeight = false,
                TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor,
                Items = { "Active", "Closed" }
            }
        };
        _statusCombo.SelectedIndex = 0;
        mainLayout.Controls.Add(_statusCombo, 1, 6);

            // Notes
            mainLayout.Controls.Add(CreateLabel("Notes:"), 0, 7);
            _notesEdit = new MemoEdit
            {
                Dock = DockStyle.Fill,
                Properties = { AutoHeight = false, MaxLength = 500 }
            };
            mainLayout.Controls.Add(_notesEdit, 1, 7);

            // Paid Amount (summary)
            mainLayout.Controls.Add(CreateLabel("Paid Amount:"), 0, 8);
            var _paidAmountDisplay = new LabelControl
            {
                Dock = DockStyle.Fill,
                Text = "0.00",
                AutoSizeMode = LabelAutoSizeMode.None,
                Appearance = { TextOptions = { HAlignment = DevExpress.Utils.HorzAlignment.Far } },
                Padding = new Padding(0, 0, 10, 0)
            };
            mainLayout.Controls.Add(_paidAmountDisplay, 1, 8);

            Controls.Add(mainLayout);

            _okButton.Enabled = _isNew;
            _okButton.Click += (s, e) => { if (ValidateForm()) DialogResult = DialogResult.OK; };
            _cancelButton.Click += (s, e) => DialogResult = DialogResult.Cancel;
        }

        protected override bool ValidateForm()
        {
            bool isValid = true;

            if (string.IsNullOrWhiteSpace(_expenseNoEdit.Text))
                isValid = false;

            if (_expenseTypeLookup.EditValue == null)
                isValid = false;

            if (_expenseDateEdit.EditValue == null)
                isValid = false;

            if (_amountEdit.Value <= 0m)
                isValid = false;

            _okButton.Enabled = isValid;
            return isValid;
        }

        public override void LoadEntityData(object entity)
        {
            var expense = (Expense)entity;
            _expenseNoEdit.Text = expense.ExpenseNo;

            if (expense.ExpenseTypeId > 0)
                _expenseTypeLookup.EditValue = expense.ExpenseTypeId;

            _expenseDateEdit.EditValue = expense.ExpenseDate;
            _amountEdit.Value = expense.Amount;
            if (expense.LocationId.HasValue)
                _locationLookup.EditValue = expense.LocationId.Value;

            _statusCombo.Text = expense.Status;

            _notesEdit.Text = string.IsNullOrWhiteSpace(expense.Notes) ? null : expense.Notes.Trim();

            ValidateForm();
        }

        public override void SaveEntityData(object entity)
        {
            var expense = (Expense)entity;
            expense.ExpenseNo = _expenseNoEdit.Text.Trim();
            expense.ExpenseTypeId = _expenseTypeLookup.EditValue == null ? 0 : Convert.ToInt32(_expenseTypeLookup.EditValue);
            expense.ExpenseDate = _expenseDateEdit.DateTime;
            expense.Amount = _amountEdit.Value;
            expense.LocationId = _locationLookup.EditValue == null ? (int?)null : (int?)_locationLookup.EditValue;
            expense.Status = _statusCombo.Text;
            expense.Notes = string.IsNullOrWhiteSpace(_notesEdit.Text) ? null : _notesEdit.Text.Trim();
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
