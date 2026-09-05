using System;
using System.Drawing;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Controls;
using MMNextPOS.Domain.Models;

namespace MMNextPOS.WinForms
{
    public partial class ThemeEditForm : EditFormBase
    {
        private Theme _theme = null!;
        private bool _isNew = true;

        private TextEdit _codeEdit = null!;
        private TextEdit _nameEdit = null!;
        private MemoEdit _descriptionEdit = null!;
        private ColorEdit _primaryColorEdit = null!;
        private ColorEdit _secondaryColorEdit = null!;
        private ColorEdit _accentColorEdit = null!;
        private ColorEdit _backgroundColorEdit = null!;
        private ColorEdit _textColorEdit = null!;
        private TextEdit _fontFamilyEdit = null!;
        private TextEdit _fontSizeEdit = null!;
        private CheckEdit _isDefaultCheck = null!;
        private CheckEdit _isActiveCheck = null!;

        public ThemeEditForm() : this(new Theme()) { }

        public ThemeEditForm(Theme theme)
        {
            _theme = theme ?? new Theme();
            _isNew = _theme.Id == 0;

            InitializeComponent();
            LoadEntityData(_theme);
        }

        private void InitializeComponent()
        {
            Text = _isNew ? "New Theme" : "Edit Theme";
            Size = new Size(550, 550);
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
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 140));
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            for (int i = 0; i < 10; i++)
                mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 45));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            // Code
            mainLayout.Controls.Add(CreateLabel("Code *:"), 0, 0);
            _codeEdit = new TextEdit { Dock = DockStyle.Fill, Properties = { MaxLength = 50 } };
            _codeEdit.EditValueChanged += (s, e) => ValidateForm();
            mainLayout.Controls.Add(_codeEdit, 1, 0);

            // Name
            mainLayout.Controls.Add(CreateLabel("Name *:"), 0, 1);
            _nameEdit = new TextEdit { Dock = DockStyle.Fill, Properties = { MaxLength = 200 } };
            _nameEdit.EditValueChanged += (s, e) => ValidateForm();
            mainLayout.Controls.Add(_nameEdit, 1, 1);

            // Description
            mainLayout.Controls.Add(CreateLabel("Description:"), 0, 2);
            _descriptionEdit = new MemoEdit { Dock = DockStyle.Fill, Properties = { MaxLength = 500 } };
            mainLayout.Controls.Add(_descriptionEdit, 1, 2);

            // Primary Color
            mainLayout.Controls.Add(CreateLabel("Primary Color:"), 0, 3);
            _primaryColorEdit = new ColorEdit { Dock = DockStyle.Fill };
            mainLayout.Controls.Add(_primaryColorEdit, 1, 3);

            // Secondary Color
            mainLayout.Controls.Add(CreateLabel("Secondary Color:"), 0, 4);
            _secondaryColorEdit = new ColorEdit { Dock = DockStyle.Fill };
            mainLayout.Controls.Add(_secondaryColorEdit, 1, 4);

            // Accent Color
            mainLayout.Controls.Add(CreateLabel("Accent Color:"), 0, 5);
            _accentColorEdit = new ColorEdit { Dock = DockStyle.Fill };
            mainLayout.Controls.Add(_accentColorEdit, 1, 5);

            // Background Color
            mainLayout.Controls.Add(CreateLabel("Background Color:"), 0, 6);
            _backgroundColorEdit = new ColorEdit { Dock = DockStyle.Fill };
            mainLayout.Controls.Add(_backgroundColorEdit, 1, 6);

            // Text Color
            mainLayout.Controls.Add(CreateLabel("Text Color:"), 0, 7);
            _textColorEdit = new ColorEdit { Dock = DockStyle.Fill };
            mainLayout.Controls.Add(_textColorEdit, 1, 7);

            // Font Family
            mainLayout.Controls.Add(CreateLabel("Font Family:"), 0, 8);
            _fontFamilyEdit = new TextEdit { Dock = DockStyle.Fill, Properties = { MaxLength = 100 } };
            mainLayout.Controls.Add(_fontFamilyEdit, 1, 8);

            // Font Size
            mainLayout.Controls.Add(CreateLabel("Font Size:"), 0, 9);
            _fontSizeEdit = new TextEdit { Dock = DockStyle.Fill, Properties = { MaxLength = 20 } };
            mainLayout.Controls.Add(_fontSizeEdit, 1, 9);

            // Is Default
            mainLayout.Controls.Add(CreateLabel("Default:"), 0, 10);
            _isDefaultCheck = new CheckEdit { Dock = DockStyle.Fill };
            mainLayout.Controls.Add(_isDefaultCheck, 1, 10);

            // Is Active (add below Is Default)
            var spacerRow = new PanelControl { Dock = DockStyle.Fill, BorderStyle = BorderStyles.NoBorder };
            mainLayout.Controls.Add(spacerRow, 0, 11);
            mainLayout.SetColumnSpan(spacerRow, 2);

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
            _theme = (Theme)entity;
            _codeEdit.Text = _theme.Code;
            _nameEdit.Text = _theme.Name;
            _descriptionEdit.Text = _theme.Description ?? string.Empty;
            _primaryColorEdit.Color = TryParseColor(_theme.PrimaryColor) ?? System.Drawing.Color.FromArgb(0, 120, 212);
            _secondaryColorEdit.Color = TryParseColor(_theme.SecondaryColor) ?? System.Drawing.Color.FromArgb(224, 224, 224);
            _accentColorEdit.Color = TryParseColor(_theme.AccentColor) ?? System.Drawing.Color.FromArgb(255, 107, 53);
            _backgroundColorEdit.Color = TryParseColor(_theme.BackgroundColor) ?? System.Drawing.Color.White;
            _textColorEdit.Color = TryParseColor(_theme.TextColor) ?? System.Drawing.Color.FromArgb(51, 51, 51);
            if (_fontFamilyEdit != null) _fontFamilyEdit.Text = _theme.FontFamily;
            if (_fontSizeEdit != null) _fontSizeEdit.Text = _theme.FontSize;
            _isDefaultCheck.Checked = _theme.IsDefault;
            _isActiveCheck.Checked = _theme.IsActive;

            ValidateForm();
        }

        public override void SaveEntityData(object entity)
        {
            var theme = (Theme)entity;
            theme.Code = _codeEdit.Text?.Trim() ?? string.Empty;
            theme.Name = _nameEdit.Text?.Trim() ?? string.Empty;
            theme.Description = _descriptionEdit.Text?.Trim() ?? string.Empty;
            theme.PrimaryColor = ColorToHex(_primaryColorEdit.Color);
            theme.SecondaryColor = ColorToHex(_secondaryColorEdit.Color);
            theme.AccentColor = ColorToHex(_accentColorEdit.Color);
            theme.BackgroundColor = ColorToHex(_backgroundColorEdit.Color);
            theme.TextColor = ColorToHex(_textColorEdit.Color);
            theme.FontFamily = _fontFamilyEdit.Text?.Trim() ?? "Segoe UI";
            theme.FontSize = _fontSizeEdit.Text?.Trim() ?? "12pt";
            theme.IsDefault = _isDefaultCheck.Checked;
            theme.IsActive = _isActiveCheck.Checked;
        }

        private System.Drawing.Color? TryParseColor(string hex)
        {
            if (string.IsNullOrWhiteSpace(hex))
                return null;

            try
            {
                return System.Drawing.ColorTranslator.FromHtml(hex);
            }
            catch
            {
                return null;
            }
        }

        private string ColorToHex(System.Drawing.Color color)
        {
            return System.Drawing.ColorTranslator.ToHtml(color);
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