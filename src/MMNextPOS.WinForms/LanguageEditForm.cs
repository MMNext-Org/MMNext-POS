using System;
using System.Drawing;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using MMNextPOS.Domain.Models;

namespace MMNextPOS.WinForms
{
    public partial class LanguageEditForm : EditFormBase
    {
        private Language _language = null!;
        private bool _isNew = true;

        private TextEdit _codeEdit = null!;
        private TextEdit _nameEdit = null!;
        private TextEdit _nativeNameEdit = null!;
        private TextEdit _cultureCodeEdit = null!;
        private TextEdit _flagIconEdit = null!;
        private SpinEdit _displayOrderEdit = null!;
        private CheckEdit _isDefaultCheck = null!;
        private CheckEdit _isActiveCheck = null!;
        private CheckEdit _isRTLCheck = null!;

        public LanguageEditForm() : this(new Language()) { }

        public LanguageEditForm(Language language)
        {
            _language = language ?? new Language();
            _isNew = _language.Id == 0;

            InitializeComponent();
            LoadEntityData(_language);
        }

        private void InitializeComponent()
        {
            Text = _isNew ? "New Language" : "Edit Language";
            Size = new Size(550, 480);
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
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 140));
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            for (int i = 0; i < 8; i++)
                mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 45));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            // Code (ISO 639-1)
            mainLayout.Controls.Add(CreateLabel("Code *:"), 0, 0);
            _codeEdit = new TextEdit { Dock = DockStyle.Fill, Properties = { MaxLength = 10 } };
            _codeEdit.EditValueChanged += (s, e) => ValidateForm();
            mainLayout.Controls.Add(_codeEdit, 1, 0);

            // Name (English)
            mainLayout.Controls.Add(CreateLabel("Name *:"), 0, 1);
            _nameEdit = new TextEdit { Dock = DockStyle.Fill, Properties = { MaxLength = 100 } };
            _nameEdit.EditValueChanged += (s, e) => ValidateForm();
            mainLayout.Controls.Add(_nameEdit, 1, 1);

            // Native Name
            mainLayout.Controls.Add(CreateLabel("Native Name:"), 0, 2);
            _nativeNameEdit = new TextEdit { Dock = DockStyle.Fill, Properties = { MaxLength = 100 } };
            mainLayout.Controls.Add(_nativeNameEdit, 1, 2);

            // Culture Code (e.g., en-US, my-MM)
            mainLayout.Controls.Add(CreateLabel("Culture Code:"), 0, 3);
            _cultureCodeEdit = new TextEdit { Dock = DockStyle.Fill, Properties = { MaxLength = 20 } };
            mainLayout.Controls.Add(_cultureCodeEdit, 1, 3);

            // Flag Icon (emoji or path)
            mainLayout.Controls.Add(CreateLabel("Flag Icon:"), 0, 4);
            _flagIconEdit = new TextEdit { Dock = DockStyle.Fill, Properties = { MaxLength = 200 } };
            mainLayout.Controls.Add(_flagIconEdit, 1, 4);

            // Display Order
            mainLayout.Controls.Add(CreateLabel("Display Order:"), 0, 5);
            _displayOrderEdit = new SpinEdit { Dock = DockStyle.Fill, Properties = { MinValue = 0, MaxValue = 9999 } };
            mainLayout.Controls.Add(_displayOrderEdit, 1, 5);

            // Is Default
            mainLayout.Controls.Add(CreateLabel("Default:"), 0, 6);
            _isDefaultCheck = new CheckEdit { Dock = DockStyle.Fill };
            mainLayout.Controls.Add(_isDefaultCheck, 1, 6);

            // Is Active
            mainLayout.Controls.Add(CreateLabel("Active:"), 0, 7);
            _isActiveCheck = new CheckEdit { Dock = DockStyle.Fill };
            mainLayout.Controls.Add(_isActiveCheck, 1, 7);

            // Is RTL
            mainLayout.Controls.Add(CreateLabel("Right-to-Left:"), 0, 8);
            _isRTLCheck = new CheckEdit { Dock = DockStyle.Fill };
            mainLayout.Controls.Add(_isRTLCheck, 1, 8);

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

        public override void LoadEntityData(object entity)
        {
            _language = (Language)entity;
            _codeEdit.Text = _language.Code;
            _nameEdit.Text = _language.Name;
            _nativeNameEdit.Text = _language.NativeName;
            _cultureCodeEdit.Text = _language.CultureCode;
            _flagIconEdit.Text = _language.FlagIcon;
            _displayOrderEdit.Value = _language.DisplayOrder;
            _isDefaultCheck.Checked = _language.IsDefault;
            _isActiveCheck.Checked = _language.IsActive;
            _isRTLCheck.Checked = _language.IsRTL;

            ValidateForm();
        }

        public override void SaveEntityData(object entity)
        {
            var language = (Language)entity;
            language.Code = _codeEdit.Text.Trim().ToLowerInvariant();
            language.Name = _nameEdit.Text.Trim();
            language.NativeName = _nativeNameEdit.Text.Trim();
            language.CultureCode = _cultureCodeEdit.Text.Trim();
            language.FlagIcon = _flagIconEdit.Text.Trim();
            language.DisplayOrder = (int)_displayOrderEdit.Value;
            language.IsDefault = _isDefaultCheck.Checked;
            language.IsActive = _isActiveCheck.Checked;
            language.IsRTL = _isRTLCheck.Checked;
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