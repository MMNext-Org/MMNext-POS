using System;
using System.Drawing;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using DevExpress.XtraTab;
using MMNextPOS.Domain.Models;

namespace MMNextPOS.WinForms
{
    public partial class SupplierEditForm : EditFormBase
    {
        private Supplier _supplier = null!;
        private bool _isNew = true;

        // Tab pages
        private DevExpress.XtraTab.XtraTabPage? _generalTab;
        private DevExpress.XtraTab.XtraTabPage? _addressTab;
        private DevExpress.XtraTab.XtraTabPage? _contactTab;
        private DevExpress.XtraTab.XtraTabPage? _financialTab;
        private DevExpress.XtraTab.XtraTabControl? _tabControl;

        private TextEdit _codeEdit = null!;
        private TextEdit _nameEdit = null!;
        private MemoEdit _addressEdit = null!;
        private TextEdit _cityEdit = null!;
        private TextEdit _countryEdit = null!;
        private TextEdit _phoneEdit = null!;
        private TextEdit _emailEdit = null!;
        private TextEdit _contactPersonEdit = null!;
        private TextEdit _taxIdEdit = null!;
        private SpinEdit _creditLimitEdit = null!;
        private SpinEdit _paymentTermDaysEdit = null!;
        private CheckEdit _isActiveCheck = null!;

        public SupplierEditForm() : this(new Supplier()) { }

        public SupplierEditForm(Supplier supplier)
        {
            _supplier = supplier ?? new Supplier();
            _isNew = _supplier.Id == 0;

            InitializeComponent();
            LoadEntityData(_supplier);
        }

        private void InitializeComponent()
        {
            Text = _isNew ? "New Supplier" : "Edit Supplier";
            Size = new Size(700, 550);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;

            // Tab control
            _tabControl = new DevExpress.XtraTab.XtraTabControl
            {
                Dock = DockStyle.Fill,
                Location = new Point(0, 0)
            };

            _tabControl.TabPages.Add(_generalTab = new DevExpress.XtraTab.XtraTabPage { Text = "General" });
            _tabControl.TabPages.Add(_addressTab = new DevExpress.XtraTab.XtraTabPage { Text = "Address" });
            _tabControl.TabPages.Add(_contactTab = new DevExpress.XtraTab.XtraTabPage { Text = "Contact" });
            _tabControl.TabPages.Add(_financialTab = new DevExpress.XtraTab.XtraTabPage { Text = "Financial" });

            Controls.Add(_tabControl);

            // --- GENERAL TAB ---
            var generalLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 6,
                Padding = new Padding(20)
            };
            generalLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130));
            generalLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            for (int i = 0; i < 5; i++)
                generalLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 45));
            generalLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            _generalTab.Controls.Add(generalLayout);

            // Code
            generalLayout.Controls.Add(CreateLabel("Code *:"), 0, 0);
            _codeEdit = new TextEdit { Dock = DockStyle.Fill, Properties = { MaxLength = 20 } };
            _codeEdit.EditValueChanged += (s, e) => ValidateForm();
            generalLayout.Controls.Add(_codeEdit, 1, 0);

            // Name
            generalLayout.Controls.Add(CreateLabel("Name *:"), 0, 1);
            _nameEdit = new TextEdit { Dock = DockStyle.Fill, Properties = { MaxLength = 150 } };
            _nameEdit.EditValueChanged += (s, e) => ValidateForm();
            generalLayout.Controls.Add(_nameEdit, 1, 1);

            // Address
            generalLayout.Controls.Add(CreateLabel("Address:"), 0, 2);
            _addressEdit = new MemoEdit { Dock = DockStyle.Fill, Properties = { MaxLength = 500 } };
            generalLayout.Controls.Add(_addressEdit, 1, 2);

            // Phone
            generalLayout.Controls.Add(CreateLabel("Phone:"), 0, 3);
            _phoneEdit = new TextEdit { Dock = DockStyle.Fill, Properties = { MaxLength = 20 } };
            generalLayout.Controls.Add(_phoneEdit, 1, 3);

            // Email
            generalLayout.Controls.Add(CreateLabel("Email:"), 0, 3);
            _emailEdit = new TextEdit { Dock = DockStyle.Fill, Properties = { MaxLength = 100 } };
            generalLayout.Controls.Add(_emailEdit, 1, 3);

            // Contact Person
            generalLayout.Controls.Add(CreateLabel("Contact Person:"), 0, 4);
            _contactPersonEdit = new TextEdit { Dock = DockStyle.Fill, Properties = { MaxLength = 100 } };
            generalLayout.Controls.Add(_contactPersonEdit, 1, 4);

            // Is Active
            generalLayout.Controls.Add(CreateLabel("Active:"), 0, 5);
            _isActiveCheck = new CheckEdit { Dock = DockStyle.Fill, Properties = { ValueChecked = "true", ValueUnchecked = "false" } };
            generalLayout.Controls.Add(_isActiveCheck, 1, 5);

            _generalTab.Controls.Add(generalLayout);

            // --- ADDRESS TAB ---
            var addressLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 4,
                Padding = new Padding(20)
            };
            addressLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130));
            addressLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            for (int i = 0; i < 3; i++)
                addressLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 45));
            addressLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            // City
            addressLayout.Controls.Add(CreateLabel("City:"), 0, 0);
            _cityEdit = new TextEdit { Dock = DockStyle.Fill, Properties = { MaxLength = 100 } };
            addressLayout.Controls.Add(_cityEdit, 1, 0);

            // Country
            addressLayout.Controls.Add(CreateLabel("Country:"), 0, 1);
            _countryEdit = new TextEdit { Dock = DockStyle.Fill, Properties = { MaxLength = 100 } };
            addressLayout.Controls.Add(_countryEdit, 1, 1);

            // Phone (secondary)
            addressLayout.Controls.Add(CreateLabel("Phone:"), 0, 2);
            _phoneEdit = new TextEdit { Dock = DockStyle.Fill, Properties = { MaxLength = 20 } };
            addressLayout.Controls.Add(_phoneEdit, 1, 2);

            // Email (secondary)
            addressLayout.Controls.Add(CreateLabel("Email:"), 0, 3);
            _emailEdit = new TextEdit { Dock = DockStyle.Fill, Properties = { MaxLength = 100 } };
            addressLayout.Controls.Add(_emailEdit, 1, 3);

            _addressTab.Controls.Add(addressLayout);

            // --- CONTACT TAB ---
            var contactLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 4,
                Padding = new Padding(20)
            };
            contactLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130));
            contactLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            for (int i = 0; i < 3; i++)
                contactLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 45));
            contactLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            // Contact Person
            contactLayout.Controls.Add(CreateLabel("Contact Person:"), 0, 0);
            _contactPersonEdit = new TextEdit { Dock = DockStyle.Fill, Properties = { MaxLength = 100 } };
            contactLayout.Controls.Add(_contactPersonEdit, 1, 0);

            // Contact Phone
            contactLayout.Controls.Add(CreateLabel("Phone:"), 0, 1);
            _phoneEdit = new TextEdit { Dock = DockStyle.Fill, Properties = { MaxLength = 20 } };
            contactLayout.Controls.Add(_phoneEdit, 1, 1);

            // Email
            contactLayout.Controls.Add(CreateLabel("Email:"), 0, 2);
            _emailEdit = new TextEdit { Dock = DockStyle.Fill, Properties = { MaxLength = 100 } };
            contactLayout.Controls.Add(_emailEdit, 1, 2);

            // Tax ID
            contactLayout.Controls.Add(CreateLabel("Tax ID:"), 0, 2);
            _taxIdEdit = new TextEdit { Dock = DockStyle.Fill, Properties = { MaxLength = 50 } };
            contactLayout.Controls.Add(_taxIdEdit, 1, 2);

            _contactTab.Controls.Add(contactLayout);

            // --- FINANCIAL TAB ---
            var financialLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 3,
                Padding = new Padding(20)
            };
            financialLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130));
            financialLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            for (int i = 0; i < 3; i++)
                financialLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 45));
            financialLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            // Credit Limit
            financialLayout.Controls.Add(CreateLabel("Credit Limit:"), 0, 0);
            _creditLimitEdit = new SpinEdit
            {
                Dock = DockStyle.Fill,
                Properties =
                {
                    AutoHeight = false,
                    MinValue = 0m,
                    MaxValue = 999999999m,
                    IsFloatValue = true,
                    Increment = 0.01m
                }
            };
            financialLayout.Controls.Add(_creditLimitEdit, 1, 0);

            // Payment Term Days
            financialLayout.Controls.Add(CreateLabel("Payment Terms (Days):"), 0, 1);
            _paymentTermDaysEdit = new SpinEdit { Dock = DockStyle.Fill, Properties = { MinValue = 0, MaxValue = 365 } };
            financialLayout.Controls.Add(_paymentTermDaysEdit, 1, 1);

            // Is Active
            financialLayout.Controls.Add(CreateLabel("Active:"), 0, 2);
            _isActiveCheck = new CheckEdit { Dock = DockStyle.Fill, Properties = { ValueChecked = "true", ValueUnchecked = "false" } };
            financialLayout.Controls.Add(_isActiveCheck, 1, 2);

            _financialTab.Controls.Add(financialLayout);

            _generalTab.Controls.Add(generalLayout);
            _addressTab.Controls.Add(addressLayout);
            _contactTab.Controls.Add(contactLayout);
            _financialTab.Controls.Add(financialLayout);

            // Tab control already added to Controls
            _tabControl.TabPages.Add(_generalTab);
            _tabControl.TabPages.Add(_addressTab);
            _tabControl.TabPages.Add(_contactTab);
            _tabControl.TabPages.Add(_financialTab);

            Controls.Add(_tabControl);
        }

        protected override bool ValidateForm()
        {
            bool isValid = true;

            if (string.IsNullOrWhiteSpace(_codeEdit.Text))
                return false;

            if (string.IsNullOrWhiteSpace(_nameEdit.Text))
                return false;

            _okButton.Enabled = isValid;
            return isValid;
        }

        public override void LoadEntityData(object entity)
        {
            var supplier = (Supplier)entity;
            _codeEdit.Text = supplier.Code;
            _nameEdit.Text = supplier.Name;
            _addressEdit.Text = supplier.Address ?? string.Empty;
            _cityEdit.Text = supplier.City ?? string.Empty;
            _countryEdit.Text = supplier.Country ?? string.Empty;
            _phoneEdit.Text = supplier.Phone ?? string.Empty;
            _emailEdit.Text = supplier.Email ?? string.Empty;
            _contactPersonEdit.Text = supplier.ContactPerson ?? string.Empty;
            _taxIdEdit.Text = supplier.TaxId ?? string.Empty;
            _creditLimitEdit.Value = supplier.CreditLimit ?? 0m;
            _paymentTermDaysEdit.Value = supplier.PaymentTermDays;
            _isActiveCheck.Checked = supplier.IsActive;

            ValidateForm();
        }

        public override void SaveEntityData(object entity)
        {
            var supplier = (Supplier)entity;
            supplier.Code = _codeEdit.Text.Trim();
            supplier.Name = _nameEdit.Text.Trim();
            supplier.Address = string.IsNullOrWhiteSpace(_addressEdit.Text) ? null : _addressEdit.Text.Trim();
            supplier.City = string.IsNullOrWhiteSpace(_cityEdit.Text) ? null : _cityEdit.Text.Trim();
            supplier.Country = string.IsNullOrWhiteSpace(_countryEdit.Text) ? null : _countryEdit.Text.Trim();
            supplier.Phone = string.IsNullOrWhiteSpace(_phoneEdit.Text) ? null : _phoneEdit.Text.Trim();
            supplier.Email = string.IsNullOrWhiteSpace(_emailEdit.Text) ? null : _emailEdit.Text.Trim();
            supplier.ContactPerson = string.IsNullOrWhiteSpace(_contactPersonEdit.Text) ? null : _contactPersonEdit.Text.Trim();
            supplier.TaxId = string.IsNullOrWhiteSpace(_taxIdEdit.Text) ? null : _taxIdEdit.Text.Trim();
            supplier.CreditLimit = _creditLimitEdit.Value == 0 ? null : _creditLimitEdit.Value;
            supplier.PaymentTermDays = (int)_paymentTermDaysEdit.Value;
            supplier.IsActive = _isActiveCheck.Checked;
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
