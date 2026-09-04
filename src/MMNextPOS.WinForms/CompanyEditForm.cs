using System;
using System.Drawing;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using MMNextPOS.Domain.Models;

namespace MMNextPOS.WinForms
{
    public partial class CompanyEditForm : EditFormBase
    {
        private Company _company = null!;
        private bool _isNew = true;

        private TextEdit _codeEdit = null!;
        private TextEdit _nameEdit = null!;
        private TextEdit _regNumberEdit = null!;
        private TextEdit _taxIdEdit = null!;
        private MemoEdit _addressEdit = null!;
        private TextEdit _cityEdit = null!;
        private TextEdit _countryEdit = null!;
        private TextEdit _phoneEdit = null!;
        private TextEdit _emailEdit = null!;
        private TextEdit _websiteEdit = null!;
        private TextEdit _logoPathEdit = null!;
        private CheckEdit _isActiveCheck = null!;

        public CompanyEditForm() : this(new Company()) { }

        public CompanyEditForm(Company company)
        {
            _company = company ?? new Company();
            _isNew = _company.Id == 0;

            InitializeComponent();
            LoadEntityData(_company);
        }

        private void InitializeComponent()
        {
            Text = _isNew ? "New Company" : "Edit Company";
            Size = new Size(600, 650);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;

            var mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 13,
                Padding = new Padding(20)
            };
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            for (int i = 0; i < 12; i++)
                mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            // Code
            mainLayout.Controls.Add(CreateLabel("Code *:"), 0, 0);
            _codeEdit = new TextEdit { Dock = DockStyle.Fill, Properties = { MaxLength = 20 } };
            _codeEdit.EditValueChanged += (s, e) => ValidateForm();
            mainLayout.Controls.Add(_codeEdit, 1, 0);

            // Name
            mainLayout.Controls.Add(CreateLabel("Name *:"), 0, 1);
            _nameEdit = new TextEdit { Dock = DockStyle.Fill, Properties = { MaxLength = 150 } };
            _nameEdit.EditValueChanged += (s, e) => ValidateForm();
            mainLayout.Controls.Add(_nameEdit, 1, 1);

            // Registration Number
            mainLayout.Controls.Add(CreateLabel("Reg. Number:"), 0, 2);
            _regNumberEdit = new TextEdit { Dock = DockStyle.Fill, Properties = { MaxLength = 50 } };
            mainLayout.Controls.Add(_regNumberEdit, 1, 2);

            // Tax ID
            mainLayout.Controls.Add(CreateLabel("Tax ID:"), 0, 3);
            _taxIdEdit = new TextEdit { Dock = DockStyle.Fill, Properties = { MaxLength = 50 } };
            mainLayout.Controls.Add(_taxIdEdit, 1, 3);

            // Address
            mainLayout.Controls.Add(CreateLabel("Address:"), 0, 4);
            _addressEdit = new MemoEdit { Dock = DockStyle.Fill, Properties = { MaxLength = 500 } };
            mainLayout.Controls.Add(_addressEdit, 1, 4);

            // City
            mainLayout.Controls.Add(CreateLabel("City:"), 0, 5);
            _cityEdit = new TextEdit { Dock = DockStyle.Fill, Properties = { MaxLength = 100 } };
            mainLayout.Controls.Add(_cityEdit, 1, 5);

            // Country
            mainLayout.Controls.Add(CreateLabel("Country:"), 0, 6);
            _countryEdit = new TextEdit { Dock = DockStyle.Fill, Properties = { MaxLength = 100 } };
            mainLayout.Controls.Add(_countryEdit, 1, 6);

            // Phone
            mainLayout.Controls.Add(CreateLabel("Phone:"), 0, 7);
            _phoneEdit = new TextEdit { Dock = DockStyle.Fill, Properties = { MaxLength = 20 } };
            mainLayout.Controls.Add(_phoneEdit, 1, 7);

            // Email
            mainLayout.Controls.Add(CreateLabel("Email:"), 0, 8);
            _emailEdit = new TextEdit { Dock = DockStyle.Fill, Properties = { MaxLength = 100 } };
            mainLayout.Controls.Add(_emailEdit, 1, 8);

            // Website
            mainLayout.Controls.Add(CreateLabel("Website:"), 0, 9);
            _websiteEdit = new TextEdit { Dock = DockStyle.Fill, Properties = { MaxLength = 200 } };
            mainLayout.Controls.Add(_websiteEdit, 1, 9);

            // Logo Path
            mainLayout.Controls.Add(CreateLabel("Logo Path:"), 0, 10);
            _logoPathEdit = new TextEdit { Dock = DockStyle.Fill, Properties = { MaxLength = 500 } };
            mainLayout.Controls.Add(_logoPathEdit, 1, 10);

            // Is Active
            mainLayout.Controls.Add(CreateLabel("Active:"), 0, 11);
            _isActiveCheck = new CheckEdit { Dock = DockStyle.Fill, Properties = { ValueChecked = "true", ValueUnchecked = "false" } };
            mainLayout.Controls.Add(_isActiveCheck, 1, 11);

            Controls.Add(mainLayout);
        }

        protected override bool ValidateForm()
        {
            bool isValid = true;

            if (string.IsNullOrWhiteSpace(_codeEdit.Text))
                isValid = false;

            if (string.IsNullOrWhiteSpace(_nameEdit.Text))
                isValid = false;

            _okButton.Enabled = isValid;
            return isValid;
        }

        protected override void LoadEntityData(object entity)
        {
            var company = (Company)entity;
            _codeEdit.Text = company.Code;
            _nameEdit.Text = company.Name;
            _regNumberEdit.Text = company.RegistrationNumber ?? string.Empty;
            _taxIdEdit.Text = company.TaxId ?? string.Empty;
            _addressEdit.Text = company.Address ?? string.Empty;
            _cityEdit.Text = company.City ?? string.Empty;
            _countryEdit.Text = company.Country ?? string.Empty;
            _phoneEdit.Text = company.Phone ?? string.Empty;
            _emailEdit.Text = company.Email ?? string.Empty;
            _websiteEdit.Text = company.Website ?? string.Empty;
            _logoPathEdit.Text = company.LogoPath ?? string.Empty;
            _isActiveCheck.Checked = company.IsActive;

            ValidateForm();
        }

        protected override void SaveEntityData(object entity)
        {
            var company = (Company)entity;
            company.Code = _codeEdit.Text.Trim();
            company.Name = _nameEdit.Text.Trim();
            company.RegistrationNumber = string.IsNullOrWhiteSpace(_regNumberEdit.Text) ? null : _regNumberEdit.Text.Trim();
            company.TaxId = string.IsNullOrWhiteSpace(_taxIdEdit.Text) ? null : _taxIdEdit.Text.Trim();
            company.Address = string.IsNullOrWhiteSpace(_addressEdit.Text) ? null : _addressEdit.Text.Trim();
            company.City = string.IsNullOrWhiteSpace(_cityEdit.Text) ? null : _cityEdit.Text.Trim();
            company.Country = string.IsNullOrWhiteSpace(_countryEdit.Text) ? null : _countryEdit.Text.Trim();
            company.Phone = string.IsNullOrWhiteSpace(_phoneEdit.Text) ? null : _phoneEdit.Text.Trim();
            company.Email = string.IsNullOrWhiteSpace(_emailEdit.Text) ? null : _emailEdit.Text.Trim();
            company.Website = string.IsNullOrWhiteSpace(_websiteEdit.Text) ? null : _websiteEdit.Text.Trim();
            company.LogoPath = string.IsNullOrWhiteSpace(_logoPathEdit.Text) ? null : _logoPathEdit.Text.Trim();
            company.IsActive = _isActiveCheck.Checked;
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