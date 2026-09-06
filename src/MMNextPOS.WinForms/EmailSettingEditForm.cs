using System;
using System.Drawing;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using MMNextPOS.Domain.Models;

namespace MMNextPOS.WinForms
{
    public partial class EmailSettingEditForm : EditFormBase
    {
        private EmailSetting _emailSetting = null!;
        private bool _isNew = true;

        private TextEdit _smtpHostEdit = null!;
        private SpinEdit _smtpPortEdit = null!;
        private TextEdit _smtpUsernameEdit = null!;
        private TextEdit _smtpPasswordEdit = null!;
        private TextEdit _fromAddressEdit = null!;
        private TextEdit _fromNameEdit = null!;
        private CheckEdit _enableTlsCheck = null!;
        private CheckEdit _isActiveCheck = null!;

        public EmailSettingEditForm() : this(new EmailSetting()) { }

        public EmailSettingEditForm(EmailSetting emailSetting)
        {
            _emailSetting = emailSetting ?? new EmailSetting();
            _isNew = _emailSetting.Id == 0;

            InitializeComponent();
            LoadEntityData(_emailSetting);
        }

        private void InitializeComponent()
        {
            Text = _isNew ? "New Email Setting" : "Edit Email Setting";
            Size = new Size(500, 500);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;

            var mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 9,
                Padding = new Padding(20)
            };
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            for (int i = 0; i < 8; i++)
                mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            // SMTP Host
            mainLayout.Controls.Add(CreateLabel("SMTP Host *:"), 0, 0);
            _smtpHostEdit = new TextEdit { Dock = DockStyle.Fill, Properties = { MaxLength = 200 } };
            _smtpHostEdit.EditValueChanged += (s, e) => ValidateForm();
            mainLayout.Controls.Add(_smtpHostEdit, 1, 0);

            // SMTP Port
            mainLayout.Controls.Add(CreateLabel("SMTP Port *:"), 0, 1);
            _smtpPortEdit = new SpinEdit
            {
                Dock = DockStyle.Fill,
                Properties = { MinValue = 1, MaxValue = 65535, IsFloatValue = false }
            };
            _smtpPortEdit.EditValueChanged += (s, e) => ValidateForm();
            mainLayout.Controls.Add(_smtpPortEdit, 1, 1);

            // SMTP Username
            mainLayout.Controls.Add(CreateLabel("Username *:"), 0, 2);
            _smtpUsernameEdit = new TextEdit { Dock = DockStyle.Fill, Properties = { MaxLength = 100 } };
            _smtpUsernameEdit.EditValueChanged += (s, e) => ValidateForm();
            mainLayout.Controls.Add(_smtpUsernameEdit, 1, 2);

            // SMTP Password
            mainLayout.Controls.Add(CreateLabel("Password *:"), 0, 3);
            _smtpPasswordEdit = new TextEdit { Dock = DockStyle.Fill, Properties = { MaxLength = 100, PasswordChar = '*' } };
            _smtpPasswordEdit.EditValueChanged += (s, e) => ValidateForm();
            mainLayout.Controls.Add(_smtpPasswordEdit, 1, 3);

            // From Address
            mainLayout.Controls.Add(CreateLabel("From Address *:"), 0, 4);
            _fromAddressEdit = new TextEdit { Dock = DockStyle.Fill, Properties = { MaxLength = 100 } };
            _fromAddressEdit.EditValueChanged += (s, e) => ValidateForm();
            mainLayout.Controls.Add(_fromAddressEdit, 1, 4);

            // From Name
            mainLayout.Controls.Add(CreateLabel("From Name *:"), 0, 5);
            _fromNameEdit = new TextEdit { Dock = DockStyle.Fill, Properties = { MaxLength = 100 } };
            _fromNameEdit.EditValueChanged += (s, e) => ValidateForm();
            mainLayout.Controls.Add(_fromNameEdit, 1, 5);

            // Enable TLS
            mainLayout.Controls.Add(CreateLabel("TLS:"), 0, 6);
            _enableTlsCheck = new CheckEdit { Dock = DockStyle.Fill, Properties = { ValueChecked = "true", ValueUnchecked = "false" } };
            mainLayout.Controls.Add(_enableTlsCheck, 1, 6);

            // Is Active
            mainLayout.Controls.Add(CreateLabel("Active:"), 0, 7);
            _isActiveCheck = new CheckEdit { Dock = DockStyle.Fill, Properties = { ValueChecked = "true", ValueUnchecked = "false" } };
            mainLayout.Controls.Add(_isActiveCheck, 1, 7);

            Controls.Add(mainLayout);
        }

        protected override bool ValidateForm()
        {
            bool isValid = true;

            if (string.IsNullOrWhiteSpace(_smtpHostEdit.Text))
                isValid = false;

            if (_smtpPortEdit.Value <= 0 || _smtpPortEdit.Value > 65535)
                isValid = false;

            if (string.IsNullOrWhiteSpace(_smtpUsernameEdit.Text))
                isValid = false;

            if (string.IsNullOrWhiteSpace(_smtpPasswordEdit.Text))
                isValid = false;

            if (string.IsNullOrWhiteSpace(_fromAddressEdit.Text) || !_fromAddressEdit.Text.Contains("@"))
                isValid = false;

            if (string.IsNullOrWhiteSpace(_fromNameEdit.Text))
                isValid = false;

            _okButton.Enabled = isValid;
            return isValid;
        }

        public override void LoadEntityData(object entity)
        {
            var email = (EmailSetting)entity;
            _smtpHostEdit.Text = email.SmtpHost;
            _smtpPortEdit.Value = email.SmtpPort;
            _smtpUsernameEdit.Text = email.SmtpUsername;
            _smtpPasswordEdit.Text = email.SmtpPassword;
            _fromAddressEdit.Text = email.FromAddress;
            _fromNameEdit.Text = email.FromName;
            _enableTlsCheck.Checked = email.EnableTls;
            _isActiveCheck.Checked = email.IsActive;

            ValidateForm();
        }

        public override void SaveEntityData(object entity)
        {
            var email = (EmailSetting)entity;
            email.SmtpHost = _smtpHostEdit.Text.Trim();
            email.SmtpPort = (int)_smtpPortEdit.Value;
            email.SmtpUsername = _smtpUsernameEdit.Text.Trim();
            email.SmtpPassword = _smtpPasswordEdit.Text.Trim();
            email.FromAddress = _fromAddressEdit.Text.Trim();
            email.FromName = _fromNameEdit.Text.Trim();
            email.EnableTls = _enableTlsCheck.Checked;
            email.IsActive = _isActiveCheck.Checked;
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
