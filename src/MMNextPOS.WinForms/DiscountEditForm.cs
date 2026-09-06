using System;
using System.Drawing;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using MMNextPOS.Domain.Models;

namespace MMNextPOS.WinForms
{
    public partial class DiscountEditForm : EditFormBase
    {
        private Discount _discount = null!;
        private bool _isNew = true;

        private TextEdit _codeEdit = null!;
        private TextEdit _nameEdit = null!;
        private SpinEdit _rateEdit = null!;
        private SpinEdit _minAmountEdit = null!;
        private SpinEdit _maxAmountEdit = null!;
        private SpinEdit _displayOrderEdit = null!;
        private CheckEdit _isActiveCheck = null!;

        public DiscountEditForm() : this(new Discount()) { }

        public DiscountEditForm(Discount discount)
        {
            _discount = discount ?? new Discount();
            _isNew = _discount.Id == 0;

            InitializeComponent();
            LoadEntityData(_discount);
        }

        private void InitializeComponent()
        {
            Text = _isNew ? "New Discount" : "Edit Discount";
            Size = new Size(500, 500);
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
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            for (int i = 0; i < 7; i++)
                mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            // Code
            mainLayout.Controls.Add(CreateLabel("Code *:"), 0, 0);
            _codeEdit = new TextEdit { Dock = DockStyle.Fill, Properties = { MaxLength = 50 } };
            _codeEdit.EditValueChanged += (s, e) => ValidateForm();
            mainLayout.Controls.Add(_codeEdit, 1, 0);

            // Name
            mainLayout.Controls.Add(CreateLabel("Name *:"), 0, 1);
            _nameEdit = new TextEdit { Dock = DockStyle.Fill, Properties = { MaxLength = 100 } };
            _nameEdit.EditValueChanged += (s, e) => ValidateForm();
            mainLayout.Controls.Add(_nameEdit, 1, 1);

            // Rate
            mainLayout.Controls.Add(CreateLabel("Rate *:"), 0, 2);
            _rateEdit = new SpinEdit
            {
                Dock = DockStyle.Fill,
                Properties = { MinValue = 0m, MaxValue = 1m, IsFloatValue = true, Increment = 0.01m }
            };
            _rateEdit.Properties.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            _rateEdit.Properties.DisplayFormat.FormatString = "p2";
            _rateEdit.EditValueChanged += (s, e) => ValidateForm();
            mainLayout.Controls.Add(_rateEdit, 1, 2);

            // Min Amount
            mainLayout.Controls.Add(CreateLabel("Min Amount:"), 0, 3);
            _minAmountEdit = new SpinEdit
            {
                Dock = DockStyle.Fill,
                Properties = { MinValue = 0m, MaxValue = 999999999m, IsFloatValue = true, Increment = 0.01m }
            };
            _minAmountEdit.Properties.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            _minAmountEdit.Properties.DisplayFormat.FormatString = "c2";
            mainLayout.Controls.Add(_minAmountEdit, 1, 3);

            // Max Amount
            mainLayout.Controls.Add(CreateLabel("Max Amount:"), 0, 4);
            _maxAmountEdit = new SpinEdit
            {
                Dock = DockStyle.Fill,
                Properties = { MinValue = 0m, MaxValue = 999999999m, IsFloatValue = true, Increment = 0.01m }
            };
            _maxAmountEdit.Properties.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            _maxAmountEdit.Properties.DisplayFormat.FormatString = "c2";
            mainLayout.Controls.Add(_maxAmountEdit, 1, 4);

            // Is Active
            mainLayout.Controls.Add(CreateLabel("Active:"), 0, 5);
            _isActiveCheck = new CheckEdit { Dock = DockStyle.Fill, Properties = { ValueChecked = "true", ValueUnchecked = "false" } };
            mainLayout.Controls.Add(_isActiveCheck, 1, 5);

            // Display Order
            mainLayout.Controls.Add(CreateLabel("Display Order:"), 0, 6);
            _displayOrderEdit = new SpinEdit { Dock = DockStyle.Fill, Properties = { MinValue = 0, MaxValue = 9999 } };
            mainLayout.Controls.Add(_displayOrderEdit, 1, 6);

            Controls.Add(mainLayout);
        }

        protected override bool ValidateForm()
        {
            bool isValid = true;

            if (string.IsNullOrWhiteSpace(_codeEdit.Text))
                isValid = false;

            if (string.IsNullOrWhiteSpace(_nameEdit.Text))
                isValid = false;

            if (_rateEdit.Value < 0 || _rateEdit.Value > 1)
                isValid = false;

            _okButton.Enabled = isValid;
            return isValid;
        }

        public override void LoadEntityData(object entity)
        {
            var discount = (Discount)entity;
            _codeEdit.Text = discount.Code;
            _nameEdit.Text = discount.Name;
            _rateEdit.Value = discount.Rate;
            _minAmountEdit.Value = discount.MinimumAmount ?? 0m;
            _maxAmountEdit.Value = discount.MaximumAmount ?? 0m;
            _isActiveCheck.Checked = discount.IsActive;
            _displayOrderEdit.Value = discount.DisplayOrder;

            ValidateForm();
        }

        public override void SaveEntityData(object entity)
        {
            var discount = (Discount)entity;
            discount.Code = _codeEdit.Text.Trim();
            discount.Name = _nameEdit.Text.Trim();
            discount.Rate = _rateEdit.Value;
            discount.MinimumAmount = _minAmountEdit.Value == 0 ? null : _minAmountEdit.Value;
            discount.MaximumAmount = _maxAmountEdit.Value == 0 ? null : _maxAmountEdit.Value;
            discount.IsActive = _isActiveCheck.Checked;
            discount.DisplayOrder = (int)_displayOrderEdit.Value;
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
