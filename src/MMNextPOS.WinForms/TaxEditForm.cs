using System;
using System.Drawing;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using MMNextPOS.Domain.Models;

namespace MMNextPOS.WinForms
{
    public partial class TaxEditForm : EditFormBase
    {
        private Tax _tax = null!;
        private bool _isNew = true;

        private TextEdit _codeEdit = null!;
        private TextEdit _nameEdit = null!;
        private SpinEdit _rateEdit = null!;
        private SpinEdit _displayOrderEdit = null!;
        private CheckEdit _isActiveCheck = null!;

        public TaxEditForm() : this(new Tax()) { }

        public TaxEditForm(Tax tax)
        {
            _tax = tax ?? new Tax();
            _isNew = _tax.Id == 0;

            InitializeComponent();
            LoadEntityData(_tax);
        }

        private void InitializeComponent()
        {
            Text = _isNew ? "New Tax" : "Edit Tax";
            Size = new Size(500, 400);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;

            var mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 6,
                Padding = new Padding(20)
            };
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            for (int i = 0; i < 5; i++)
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
                Properties = { MinValue = 0m, MaxValue = 1m, IsFloatValue = true, Increment = 0.005m }
            };
            _rateEdit.Properties.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            _rateEdit.Properties.DisplayFormat.FormatString = "p4";
            _rateEdit.EditValueChanged += (s, e) => ValidateForm();
            mainLayout.Controls.Add(_rateEdit, 1, 2);

            // Is Active
            mainLayout.Controls.Add(CreateLabel("Active:"), 0, 3);
            _isActiveCheck = new CheckEdit { Dock = DockStyle.Fill, Properties = { ValueChecked = "true", ValueUnchecked = "false" } };
            mainLayout.Controls.Add(_isActiveCheck, 1, 3);

            // Display Order
            mainLayout.Controls.Add(CreateLabel("Display Order:"), 0, 4);
            _displayOrderEdit = new SpinEdit { Dock = DockStyle.Fill, Properties = { MinValue = 0, MaxValue = 9999 } };
            mainLayout.Controls.Add(_displayOrderEdit, 1, 4);

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
            var tax = (Tax)entity;
            _codeEdit.Text = tax.Code;
            _nameEdit.Text = tax.Name;
            _rateEdit.Value = tax.Rate;
            _isActiveCheck.Checked = tax.IsActive;
            _displayOrderEdit.Value = tax.DisplayOrder;

            ValidateForm();
        }

        public override void SaveEntityData(object entity)
        {
            var tax = (Tax)entity;
            tax.Code = _codeEdit.Text.Trim();
            tax.Name = _nameEdit.Text.Trim();
            tax.Rate = _rateEdit.Value;
            tax.IsActive = _isActiveCheck.Checked;
            tax.DisplayOrder = (int)_displayOrderEdit.Value;
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