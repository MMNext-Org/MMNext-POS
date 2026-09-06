using System;
using System.Drawing;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using MMNextPOS.Domain.Models;

namespace MMNextPOS.WinForms
{
    public partial class UserEditForm : EditFormBase
    {
        private User _user = null!;
        private bool _isNew = true;

        private TextEdit _usernameEdit = null!;
        private TextEdit _passwordEdit = null!;
        private TextEdit _confirmPasswordEdit = null!;
        private TextEdit _emailEdit = null!;
        private TextEdit _fullNameEdit = null!;
        private TextEdit _phoneEdit = null!;
        private CheckEdit _isActiveCheck = null!;
        private LookUpEdit _locationLookup = null!;
        private LookUpEdit _companyLookup = null!;

        public UserEditForm() : this(new User()) { }

        public UserEditForm(User user)
        {
            _user = user ?? new User();
            _isNew = _user.Id == 0;

            InitializeComponent();
            LoadEntityData(_user);
        }

        private void InitializeComponent()
        {
            Text = _isNew ? "New User" : "Edit User";
            Size = new Size(500, 550);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;

            var mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 11,
                Padding = new Padding(20)
            };
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            for (int i = 0; i < 10; i++)
                mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            // Username
            mainLayout.Controls.Add(CreateLabel("Username *:"), 0, 0);
            _usernameEdit = new TextEdit { Dock = DockStyle.Fill, Properties = { MaxLength = 50 } };
            _usernameEdit.EditValueChanged += (s, e) => ValidateForm();
            mainLayout.Controls.Add(_usernameEdit, 1, 0);

            // Password
            mainLayout.Controls.Add(CreateLabel(_isNew ? "Password *:" : "Password:"), 0, 1);
            _passwordEdit = new TextEdit { Dock = DockStyle.Fill, Properties = { MaxLength = 100, PasswordChar = '*' } };
            if (!_isNew)
                _passwordEdit.Properties.PasswordChar = '\0';
            _passwordEdit.EditValueChanged += (s, e) => ValidateForm();
            mainLayout.Controls.Add(_passwordEdit, 1, 1);

            // Confirm Password
            mainLayout.Controls.Add(CreateLabel(_isNew ? "Confirm Password *:" : "Confirm Password:"), 0, 2);
            _confirmPasswordEdit = new TextEdit { Dock = DockStyle.Fill, Properties = { MaxLength = 100, PasswordChar = '*' } };
            if (!_isNew)
                _confirmPasswordEdit.Properties.PasswordChar = '\0';
            _confirmPasswordEdit.EditValueChanged += (s, e) => ValidateForm();
            mainLayout.Controls.Add(_confirmPasswordEdit, 1, 2);

            // Email
            mainLayout.Controls.Add(CreateLabel("Email *:"), 0, 3);
            _emailEdit = new TextEdit { Dock = DockStyle.Fill, Properties = { MaxLength = 100 } };
            _emailEdit.EditValueChanged += (s, e) => ValidateForm();
            mainLayout.Controls.Add(_emailEdit, 1, 3);

            // Full Name
            mainLayout.Controls.Add(CreateLabel("Full Name *:"), 0, 4);
            _fullNameEdit = new TextEdit { Dock = DockStyle.Fill, Properties = { MaxLength = 150 } };
            _fullNameEdit.EditValueChanged += (s, e) => ValidateForm();
            mainLayout.Controls.Add(_fullNameEdit, 1, 4);

            // Phone
            mainLayout.Controls.Add(CreateLabel("Phone:"), 0, 5);
            _phoneEdit = new TextEdit { Dock = DockStyle.Fill, Properties = { MaxLength = 20 } };
            mainLayout.Controls.Add(_phoneEdit, 1, 5);

            // Is Active
            mainLayout.Controls.Add(CreateLabel("Active:"), 0, 6);
            _isActiveCheck = new CheckEdit { Dock = DockStyle.Fill, Properties = { ValueChecked = "true", ValueUnchecked = "false" } };
            mainLayout.Controls.Add(_isActiveCheck, 1, 6);

            // Location
            mainLayout.Controls.Add(CreateLabel("Location:"), 0, 7);
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
            mainLayout.Controls.Add(_locationLookup, 1, 7);

            // Company
            mainLayout.Controls.Add(CreateLabel("Company:"), 0, 8);
            _companyLookup = new LookUpEdit
            {
                Dock = DockStyle.Fill,
                Properties =
                {
                    DisplayMember = "Name",
                    ValueMember = "Id",
                    NullText = "Select company...",
                    ShowHeader = false,
                    AutoHeight = false,
                    BestFitMode = DevExpress.XtraEditors.Controls.BestFitMode.BestFitResizePopup,
                    SearchMode = DevExpress.XtraEditors.Controls.SearchMode.AutoFilter
                }
            };
            mainLayout.Controls.Add(_companyLookup, 1, 8);

            Controls.Add(mainLayout);
        }

        protected override bool ValidateForm()
        {
            bool isValid = true;

            if (string.IsNullOrWhiteSpace(_usernameEdit.Text))
                isValid = false;

            if (string.IsNullOrWhiteSpace(_emailEdit.Text) || !_emailEdit.Text.Contains("@"))
                isValid = false;

            if (string.IsNullOrWhiteSpace(_fullNameEdit.Text))
                isValid = false;

            if (_isNew && string.IsNullOrWhiteSpace(_passwordEdit.Text))
                isValid = false;

            if (_isNew && _passwordEdit.Text != _confirmPasswordEdit.Text)
                isValid = false;

            _okButton.Enabled = isValid;
            return isValid;
        }

        public override void LoadEntityData(object entity)
        {
            var user = (User)entity;
            _usernameEdit.Text = user.Username;
            _emailEdit.Text = user.Email;
            _fullNameEdit.Text = user.FullName;
            _phoneEdit.Text = user.Phone ?? string.Empty;
            _isActiveCheck.Checked = user.IsActive;

            if (user.LocationId.HasValue)
                _locationLookup.EditValue = user.LocationId.Value;

            if (user.CompanyId.HasValue)
                _companyLookup.EditValue = user.CompanyId.Value;

            if (!_isNew)
            {
                _passwordEdit.Text = string.Empty;
                _confirmPasswordEdit.Text = string.Empty;
            }

            ValidateForm();
        }

        public override void SaveEntityData(object entity)
        {
            var user = (User)entity;
            user.Username = _usernameEdit.Text.Trim();
            user.Email = _emailEdit.Text.Trim();
            user.FullName = _fullNameEdit.Text.Trim();
            user.Phone = string.IsNullOrWhiteSpace(_phoneEdit.Text) ? null : _phoneEdit.Text.Trim();
            user.IsActive = _isActiveCheck.Checked;
            user.LocationId = _locationLookup.EditValue == null ? null : (int?)_locationLookup.EditValue;
            user.CompanyId = _companyLookup.EditValue == null ? null : (int?)_companyLookup.EditValue;

            if (!string.IsNullOrWhiteSpace(_passwordEdit.Text))
            {
                // In real implementation, hash the password
                user.PasswordHash = _passwordEdit.Text; // Placeholder
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
