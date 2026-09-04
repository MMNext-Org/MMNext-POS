using System;
using System.Drawing;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using MMNextPOS.Domain.Models;

namespace MMNextPOS.WinForms
{
    public partial class GroupEditForm : EditFormBase
    {
        private Group _group = null!;
        private bool _isNew = true;

        private TextEdit _codeEdit = null!;
        private TextEdit _nameEdit = null!;
        private MemoEdit _descriptionEdit = null!;
        private LookUpEdit _parentGroupLookup = null!;
        private SpinEdit _displayOrderEdit = null!;
        private CheckEdit _isActiveCheck = null!;

        public GroupEditForm() : this(new Group()) { }

        public GroupEditForm(Group group)
        {
            _group = group ?? new Group();
            _isNew = _group.Id == 0;

            InitializeComponent();
            LoadEntityData(_group);
        }

        private void InitializeComponent()
        {
            Text = _isNew ? "New Group" : "Edit Group";
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

            // Parent Group
            mainLayout.Controls.Add(CreateLabel("Parent Group:"), 0, 3);
            _parentGroupLookup = new LookUpEdit
            {
                Dock = DockStyle.Fill,
                Properties =
                {
                    DisplayMember = "Name",
                    ValueMember = "Id",
                    NullText = "No parent (top level)",
                    ShowHeader = false,
                    AutoHeight = false,
                    BestFitMode = DevExpress.XtraEditors.Controls.BestFitMode.BestFitResizePopup,
                    SearchMode = DevExpress.XtraEditors.Controls.SearchMode.AutoFilter
                }
            };
            mainLayout.Controls.Add(_parentGroupLookup, 1, 3);

            // Display Order
            mainLayout.Controls.Add(CreateLabel("Display Order:"), 0, 4);
            _displayOrderEdit = new SpinEdit { Dock = DockStyle.Fill, Properties = { MinValue = 0, MaxValue = 9999 } };
            mainLayout.Controls.Add(_displayOrderEdit, 1, 4);

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
            var group = (Group)entity;
            _codeEdit.Text = group.Code;
            _nameEdit.Text = group.Name;
            _descriptionEdit.Text = group.Description ?? string.Empty;
            _displayOrderEdit.Value = group.DisplayOrder;
            _isActiveCheck.Checked = group.IsActive;

            if (group.ParentGroupId.HasValue)
                _parentGroupLookup.EditValue = group.ParentGroupId.Value;

            ValidateForm();
        }

        protected override void SaveEntityData(object entity)
        {
            var group = (Group)entity;
            group.Code = _codeEdit.Text.Trim();
            group.Name = _nameEdit.Text.Trim();
            group.Description = string.IsNullOrWhiteSpace(_descriptionEdit.Text) ? null : _descriptionEdit.Text.Trim();
            group.ParentGroupId = _parentGroupLookup.EditValue == null ? null : (int?)_parentGroupLookup.EditValue;
            group.DisplayOrder = (int)_displayOrderEdit.Value;
            group.IsActive = _isActiveCheck.Checked;
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