using System;
using System.Drawing;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using MMNextPOS.Domain.Models;

namespace MMNextPOS.WinForms
{
    public partial class CategoryEditForm : EditFormBase
    {
        private Category _category = null!;
        private bool _isNew = true;

        private TextEdit _codeEdit = null!;
        private TextEdit _nameEdit = null!;
        private MemoEdit _descriptionEdit = null!;
        private SpinEdit _displayOrderEdit = null!;
        private CheckEdit _isActiveCheck = null!;

        public CategoryEditForm() : this(new Category()) { }

        public CategoryEditForm(Category category)
        {
            _category = category ?? new Category();
            _isNew = _category.Id == 0;

            InitializeComponent();
            LoadEntityData(_category);
        }

        private void InitializeComponent()
        {
            Text = _isNew ? "New Category" : "Edit Category";
            Size = new Size(500, 400);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;

            var mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 6,
                Padding = new Padding(20)
            };
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            for (int i = 0; i < 5; i++)
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

            // Display Order
            mainLayout.Controls.Add(CreateLabel("Display Order:"), 0, 3);
            _displayOrderEdit = new SpinEdit { Dock = DockStyle.Fill, Properties = { MinValue = 0, MaxValue = 9999 } };
            mainLayout.Controls.Add(_displayOrderEdit, 1, 3);

            // Is Active
            mainLayout.Controls.Add(CreateLabel("Active:"), 0, 4);
            _isActiveCheck = new CheckEdit { Dock = DockStyle.Fill, Properties = { ValueChecked = "true", ValueUnchecked = "false" } };
            mainLayout.Controls.Add(_isActiveCheck, 1, 4);

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
            _category = (Category)entity;
            _codeEdit.Text = _category.Code;
            _nameEdit.Text = _category.Name;
            _descriptionEdit.Text = _category.Description ?? string.Empty;
            _displayOrderEdit.Value = _category.DisplayOrder;
            _isActiveCheck.Checked = _category.IsActive;

            ValidateForm();
        }

        public override void SaveEntityData(object entity)
        {
            var category = (Category)entity;
            category.Code = _codeEdit.Text.Trim();
            category.Name = _nameEdit.Text.Trim();
            category.Description = string.IsNullOrWhiteSpace(_descriptionEdit.Text) ? null : _descriptionEdit.Text.Trim();
            category.DisplayOrder = (int)_displayOrderEdit.Value;
            category.IsActive = _isActiveCheck.Checked;
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
