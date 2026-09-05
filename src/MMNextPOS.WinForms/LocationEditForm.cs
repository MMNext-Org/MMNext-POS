using System;
using System.Drawing;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using MMNextPOS.Domain.Models;

namespace MMNextPOS.WinForms
{
    public partial class LocationEditForm : EditFormBase
    {
        private Location _location = null!;
        private bool _isNew = true;

        private TextEdit _codeEdit = null!;
        private TextEdit _nameEdit = null!;
        private MemoEdit _addressEdit = null!;
        private TextEdit _cityEdit = null!;
        private TextEdit _phoneEdit = null!;
        private TextEdit _emailEdit = null!;
        private CheckEdit _isActiveCheck = null!;
        private CheckEdit _isHeadquarterCheck = null!;
        private SpinEdit _displayOrderEdit = null!;

        public LocationEditForm() : this(new Location()) { }

        public LocationEditForm(Location location)
        {
            _location = location ?? new Location();
            _isNew = _location.Id == 0;

            InitializeComponent();
            LoadEntityData(_location);
        }

        private void InitializeComponent()
        {
            Text = _isNew ? "New Location" : "Edit Location";
            Size = new Size(500, 600);
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

            // Address
            mainLayout.Controls.Add(CreateLabel("Address:"), 0, 2);
            _addressEdit = new MemoEdit { Dock = DockStyle.Fill, Properties = { MaxLength = 500 } };
            mainLayout.Controls.Add(_addressEdit, 1, 2);

            // City
            mainLayout.Controls.Add(CreateLabel("City:"), 0, 3);
            _cityEdit = new TextEdit { Dock = DockStyle.Fill, Properties = { MaxLength = 100 } };
            mainLayout.Controls.Add(_cityEdit, 1, 3);

            // Phone
            mainLayout.Controls.Add(CreateLabel("Phone:"), 0, 4);
            _phoneEdit = new TextEdit { Dock = DockStyle.Fill, Properties = { MaxLength = 20 } };
            mainLayout.Controls.Add(_phoneEdit, 1, 4);

            // Email
            mainLayout.Controls.Add(CreateLabel("Email:"), 0, 5);
            _emailEdit = new TextEdit { Dock = DockStyle.Fill, Properties = { MaxLength = 100 } };
            mainLayout.Controls.Add(_emailEdit, 1, 5);

            // Is Active
            mainLayout.Controls.Add(CreateLabel("Active:"), 0, 6);
            _isActiveCheck = new CheckEdit { Dock = DockStyle.Fill, Properties = { ValueChecked = "true", ValueUnchecked = "false" } };
            mainLayout.Controls.Add(_isActiveCheck, 1, 6);

            // Is Headquarter
            mainLayout.Controls.Add(CreateLabel("Headquarter:"), 0, 7);
            _isHeadquarterCheck = new CheckEdit { Dock = DockStyle.Fill, Properties = { ValueChecked = "true", ValueUnchecked = "false" } };
            mainLayout.Controls.Add(_isHeadquarterCheck, 1, 7);

            // Display Order
            mainLayout.Controls.Add(CreateLabel("Display Order:"), 0, 8);
            _displayOrderEdit = new SpinEdit { Dock = DockStyle.Fill, Properties = { MinValue = 0, MaxValue = 9999 } };
            mainLayout.Controls.Add(_displayOrderEdit, 1, 8);

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
            var location = (Location)entity;
            _codeEdit.Text = location.Code;
            _nameEdit.Text = location.Name;
            _addressEdit.Text = location.Address ?? string.Empty;
            _cityEdit.Text = location.City ?? string.Empty;
            _phoneEdit.Text = location.Phone ?? string.Empty;
            _emailEdit.Text = location.Email ?? string.Empty;
            _isActiveCheck.Checked = location.IsActive;
            _isHeadquarterCheck.Checked = location.IsHeadquarter;
            _displayOrderEdit.Value = location.DisplayOrder;

            ValidateForm();
        }

        public override void SaveEntityData(object entity)
        {
            var location = (Location)entity;
            location.Code = _codeEdit.Text.Trim();
            location.Name = _nameEdit.Text.Trim();
            location.Address = string.IsNullOrWhiteSpace(_addressEdit.Text) ? null : _addressEdit.Text.Trim();
            location.City = string.IsNullOrWhiteSpace(_cityEdit.Text) ? null : _cityEdit.Text.Trim();
            location.Phone = string.IsNullOrWhiteSpace(_phoneEdit.Text) ? null : _phoneEdit.Text.Trim();
            location.Email = string.IsNullOrWhiteSpace(_emailEdit.Text) ? null : _emailEdit.Text.Trim();
            location.IsActive = _isActiveCheck.Checked;
            location.IsHeadquarter = _isHeadquarterCheck.Checked;
            location.DisplayOrder = (int)_displayOrderEdit.Value;
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