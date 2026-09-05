using System;
using System.Drawing;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using MMNextPOS.Domain.Models;

namespace MMNextPOS.WinForms
{
    public partial class CurrencyEditForm : EditFormBase
    {
        private Currency _currency = null!;
        private bool _isNew = true;

        private TextEdit _codeEdit = null!;
        private TextEdit _nameEdit = null!;
        private TextEdit _symbolEdit = null!;
        private SpinEdit _exchangeRateEdit = null!;
        private CheckEdit _isActiveCheck = null!;
        private CheckEdit _isDefaultCheck = null!;

        public CurrencyEditForm() : this(new Currency()) { }

        public CurrencyEditForm(Currency currency)
        {
            _currency = currency ?? new Currency();
            _isNew = _currency.Id == 0;

            InitializeComponent();
            LoadEntityData(_currency);
        }

        private void InitializeComponent()
        {
            Text = _isNew ? "New Currency" : "Edit Currency";
            Size = new Size(500, 450);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;

            var mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 7,
                Padding = new Padding(20)
            };
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            for (int i = 0; i < 6; i++)
                mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            // Code
            mainLayout.Controls.Add(CreateLabel("Code *:"), 0, 0);
            _codeEdit = new TextEdit { Dock = DockStyle.Fill, Properties = { MaxLength = 3, CharacterCasing = CharacterCasing.Upper } };
            _codeEdit.EditValueChanged += (s, e) => ValidateForm();
            mainLayout.Controls.Add(_codeEdit, 1, 0);

            // Name
            mainLayout.Controls.Add(CreateLabel("Name *:"), 0, 1);
            _nameEdit = new TextEdit { Dock = DockStyle.Fill, Properties = { MaxLength = 50 } };
            _nameEdit.EditValueChanged += (s, e) => ValidateForm();
            mainLayout.Controls.Add(_nameEdit, 1, 1);

            // Symbol
            mainLayout.Controls.Add(CreateLabel("Symbol:"), 0, 2);
            _symbolEdit = new TextEdit { Dock = DockStyle.Fill, Properties = { MaxLength = 5 } };
            mainLayout.Controls.Add(_symbolEdit, 1, 2);

            // Exchange Rate
            mainLayout.Controls.Add(CreateLabel("Exchange Rate *:"), 0, 3);
            _exchangeRateEdit = new SpinEdit
            {
                Dock = DockStyle.Fill,
                Properties = { MinValue = 0.000001m, MaxValue = 999999m, IsFloatValue = true, Increment = 0.0001m }
            };
            _exchangeRateEdit.Properties.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            _exchangeRateEdit.Properties.DisplayFormat.FormatString = "n6";
            _exchangeRateEdit.EditValueChanged += (s, e) => ValidateForm();
            mainLayout.Controls.Add(_exchangeRateEdit, 1, 3);

            // Is Active
            mainLayout.Controls.Add(CreateLabel("Active:"), 0, 4);
            _isActiveCheck = new CheckEdit { Dock = DockStyle.Fill, Properties = { ValueChecked = "true", ValueUnchecked = "false" } };
            mainLayout.Controls.Add(_isActiveCheck, 1, 4);

            // Is Default
            mainLayout.Controls.Add(CreateLabel("Default:"), 0, 5);
            _isDefaultCheck = new CheckEdit { Dock = DockStyle.Fill, Properties = { ValueChecked = "true", ValueUnchecked = "false" } };
            mainLayout.Controls.Add(_isDefaultCheck, 1, 5);

            Controls.Add(mainLayout);
        }

        protected override bool ValidateForm()
        {
            bool isValid = true;

            if (string.IsNullOrWhiteSpace(_codeEdit.Text) || _codeEdit.Text.Length != 3)
                isValid = false;

            if (string.IsNullOrWhiteSpace(_nameEdit.Text))
                isValid = false;

            if (_exchangeRateEdit.Value <= 0)
                isValid = false;

            _okButton.Enabled = isValid;
            return isValid;
        }

        public override void LoadEntityData(object entity)
        {
            var currency = (Currency)entity;
            _codeEdit.Text = currency.Code;
            _nameEdit.Text = currency.Name;
            _symbolEdit.Text = currency.Symbol ?? string.Empty;
            _exchangeRateEdit.Value = currency.ExchangeRate;
            _isActiveCheck.Checked = currency.IsActive;
            _isDefaultCheck.Checked = currency.IsDefault;

            ValidateForm();
        }

        public override void SaveEntityData(object entity)
        {
            var currency = (Currency)entity;
            currency.Code = _codeEdit.Text.Trim().ToUpperInvariant();
            currency.Name = _nameEdit.Text.Trim();
            currency.Symbol = string.IsNullOrWhiteSpace(_symbolEdit.Text) ? null : _symbolEdit.Text.Trim();
            currency.ExchangeRate = _exchangeRateEdit.Value;
            currency.IsActive = _isActiveCheck.Checked;
            currency.IsDefault = _isDefaultCheck.Checked;
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