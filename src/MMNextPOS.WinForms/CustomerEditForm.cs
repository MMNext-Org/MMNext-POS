using System;
using System.Drawing;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using MMNextPOS.Domain.Models;

namespace MMNextPOS.WinForms
{
    public partial class CustomerEditForm : EditFormBase
    {
        private Customer _customer = null!;
        private bool _isNew = true;

        private TextEdit _codeEdit = null!;
        private TextEdit _nameEdit = null!;
        private MemoEdit _addressEdit = null!;
        private TextEdit _phoneEdit = null!;
        private TextEdit _emailEdit = null!;
        private CheckEdit _isActiveCheck = null!;

        public CustomerEditForm() : this(new Customer()) { }

        public CustomerEditForm(Customer customer)
        {
            _customer = customer ?? new Customer();
            _isNew = _customer.Id == 0;

            InitializeComponent();
            LoadEntityData(_customer);
        }

        private void InitializeComponent()
        {
            Text = _isNew ? "New Customer" : "Edit Customer";
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
            _codeEdit = new TextEdit { Dock = DockStyle.Fill, Properties = { MaxLength = 20 } };
            _codeEdit.EditValueChanged += (s, e) => ValidateForm();
            mainLayout.Controls.Add(_codeEdit, 1, 0);

            // Name
            mainLayout.Controls.Add(CreateLabel("Name *:"), 0, 1);
            _nameEdit = new TextEdit { Dock = DockStyle.Fill, Properties = { MaxLength = 150 } };
            _nameEdit.EditValueChanged += (s, e) => ValidateForm();
            mainLayout.Controls.Add(_nameEdit, 1, 1);

            // Address
            mainLayout.Controls.Add(CreateLabel("Address:"), 0, 2);
            _addressEdit = new MemoEdit { Dock = DockStyle.Fill, Properties = { MaxLength = 500 } };
            mainLayout.Controls.Add(_addressEdit, 1, 2);

            // Phone
            mainLayout.Controls.Add(CreateLabel("Phone:"), 0, 3);
            _phoneEdit = new TextEdit { Dock = DockStyle.Fill, Properties = { MaxLength = 20 } };
            mainLayout.Controls.Add(_phoneEdit, 1, 3);

            // Email
            mainLayout.Controls.Add(CreateLabel("Email:"), 0, 4);
            _emailEdit = new TextEdit { Dock = DockStyle.Fill, Properties = { MaxLength = 100 } };
            mainLayout.Controls.Add(_emailEdit, 1, 4);

            // Is Active
            mainLayout.Controls.Add(CreateLabel("Active:"), 0, 5);
            _isActiveCheck = new CheckEdit { Dock = DockStyle.Fill, Properties = { ValueChecked = "true", ValueUnchecked = "false" } };
            mainLayout.Controls.Add(_isActiveCheck, 1, 5);

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
            var customer = (Customer)entity;
            _codeEdit.Text = customer.Code;
            _nameEdit.Text = customer.Name;
            _addressEdit.Text = customer.Address ?? string.Empty;
            _phoneEdit.Text = customer.Phone ?? string.Empty;
            _emailEdit.Text = customer.Email ?? string.Empty;
            _isActiveCheck.Checked = customer.IsActive;

            ValidateForm();
        }

        protected override void SaveEntityData(object entity)
        {
            var customer = (Customer)entity;
            customer.Code = _codeEdit.Text.Trim();
            customer.Name = _nameEdit.Text.Trim();
            customer.Address = string.IsNullOrWhiteSpace(_addressEdit.Text) ? null : _addressEdit.Text.Trim();
            customer.Phone = string.IsNullOrWhiteSpace(_phoneEdit.Text) ? null : _phoneEdit.Text.Trim();
            customer.Email = string.IsNullOrWhiteSpace(_emailEdit.Text) ? null : _emailEdit.Text.Trim();
            customer.IsActive = _isActiveCheck.Checked;
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