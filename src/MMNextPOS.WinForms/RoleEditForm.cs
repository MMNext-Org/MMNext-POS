using System;
using System.Drawing;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using MMNextPOS.Domain.Models;

namespace MMNextPOS.WinForms
{
    public partial class RoleEditForm : EditFormBase
    {
        private Role _role = null!;
        private bool _isNew = true;

        private TextEdit _codeEdit = null!;
        private TextEdit _nameEdit = null!;
        private MemoEdit _descriptionEdit = null!;
        private CheckEdit _isActiveCheck = null!;

        public RoleEditForm() : this(new Role()) { }

        public RoleEditForm(Role role)
        {
            _role = role ?? new Role();
            _isNew = _role.Id == 0;

            InitializeComponent();
            LoadEntityData(_role);
        }

        private void InitializeComponent()
        {
            Text = _isNew ? "New Role" : "Edit Role";
            Size = new Size(500, 400);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;

            var mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 5,
                Padding = new Padding(20)
            };
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            for (int i = 0; i < 4; i++)
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

            // Description
            mainLayout.Controls.Add(CreateLabel("Description:"), 0, 2);
            _descriptionEdit = new MemoEdit { Dock = DockStyle.Fill, Properties = { MaxLength = 500 } };
            mainLayout.Controls.Add(_descriptionEdit, 1, 2);

            // Is Active
            mainLayout.Controls.Add(CreateLabel("Active:"), 0, 3);
            _isActiveCheck = new CheckEdit { Dock = DockStyle.Fill, Properties = { ValueChecked = "true", ValueUnchecked = "false" } };
            mainLayout.Controls.Add(_isActiveCheck, 1, 3);

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
            var role = (Role)entity;
            _codeEdit.Text = role.Code;
            _nameEdit.Text = role.Name;
            _descriptionEdit.Text = role.Description ?? string.Empty;
            _isActiveCheck.Checked = role.IsActive;

            ValidateForm();
        }

        protected override void SaveEntityData(object entity)
        {
            var role = (Role)entity;
            role.Code = _codeEdit.Text.Trim();
            role.Name = _nameEdit.Text.Trim();
            role.Description = string.IsNullOrWhiteSpace(_descriptionEdit.Text) ? null : _descriptionEdit.Text.Trim();
            role.IsActive = _isActiveCheck.Checked;
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