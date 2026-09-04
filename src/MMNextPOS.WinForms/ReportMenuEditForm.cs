using System;
using System.Drawing;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using MMNextPOS.Domain.Models;

namespace MMNextPOS.WinForms
{
    public partial class ReportMenuEditForm : EditFormBase
    {
        private ReportMenus _reportMenu = null!;
        private bool _isNew = true;

        private TextEdit _codeEdit = null!;
        private TextEdit _nameEdit = null!;
        private TextEdit _parentCodeEdit = null!;
        private TextEdit _formNameEdit = null!;
        private TextEdit _assemblyNameEdit = null!;
        private TextEdit _iconNameEdit = null!;
        private SpinEdit _displayOrderEdit = null!;
        private CheckEdit _isVisibleCheck = null!;
        private CheckEdit _isReportCheck = null!;
        private TextEdit _reportFileNameEdit = null!;
        private MemoEdit _descriptionEdit = null!;

        public ReportMenuEditForm() : this(new ReportMenus()) { }

        public ReportMenuEditForm(ReportMenus reportMenu)
        {
            _reportMenu = reportMenu ?? new ReportMenus();
            _isNew = _reportMenu.Id == 0;

            InitializeComponent();
            LoadEntityData(_reportMenu);
        }

        private void InitializeComponent()
        {
            Text = _isNew ? "New Report Menu" : "Edit Report Menu";
            Size = new Size(500, 650);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;

            var mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 12,
                Padding = new Padding(20)
            };
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            for (int i = 0; i < 11; i++)
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

            // Parent Code
            mainLayout.Controls.Add(CreateLabel("Parent Code:"), 0, 2);
            _parentCodeEdit = new TextEdit { Dock = DockStyle.Fill, Properties = { MaxLength = 50 } };
            mainLayout.Controls.Add(_parentCodeEdit, 1, 2);

            // Form Name
            mainLayout.Controls.Add(CreateLabel("Form Name:"), 0, 3);
            _formNameEdit = new TextEdit { Dock = DockStyle.Fill, Properties = { MaxLength = 100 } };
            mainLayout.Controls.Add(_formNameEdit, 1, 3);

            // Assembly Name
            mainLayout.Controls.Add(CreateLabel("Assembly Name:"), 0, 4);
            _assemblyNameEdit = new TextEdit { Dock = DockStyle.Fill, Properties = { MaxLength = 100 } };
            mainLayout.Controls.Add(_assemblyNameEdit, 1, 4);

            // Icon Name
            mainLayout.Controls.Add(CreateLabel("Icon Name:"), 0, 5);
            _iconNameEdit = new TextEdit { Dock = DockStyle.Fill, Properties = { MaxLength = 50 } };
            mainLayout.Controls.Add(_iconNameEdit, 1, 5);

            // Display Order
            mainLayout.Controls.Add(CreateLabel("Display Order:"), 0, 6);
            _displayOrderEdit = new SpinEdit { Dock = DockStyle.Fill, Properties = { MinValue = 0, MaxValue = 9999 } };
            mainLayout.Controls.Add(_displayOrderEdit, 1, 6);

            // Is Visible
            mainLayout.Controls.Add(CreateLabel("Visible:"), 0, 7);
            _isVisibleCheck = new CheckEdit { Dock = DockStyle.Fill, Properties = { ValueChecked = "true", ValueUnchecked = "false" } };
            mainLayout.Controls.Add(_isVisibleCheck, 1, 7);

            // Is Report
            mainLayout.Controls.Add(CreateLabel("Is Report:"), 0, 8);
            _isReportCheck = new CheckEdit { Dock = DockStyle.Fill, Properties = { ValueChecked = "true", ValueUnchecked = "false" } };
            mainLayout.Controls.Add(_isReportCheck, 1, 8);

            // Report File Name
            mainLayout.Controls.Add(CreateLabel("Report File:"), 0, 9);
            _reportFileNameEdit = new TextEdit { Dock = DockStyle.Fill, Properties = { MaxLength = 200 } };
            mainLayout.Controls.Add(_reportFileNameEdit, 1, 9);

            // Description
            mainLayout.Controls.Add(CreateLabel("Description:"), 0, 10);
            _descriptionEdit = new MemoEdit { Dock = DockStyle.Fill, Properties = { MaxLength = 500 } };
            mainLayout.Controls.Add(_descriptionEdit, 1, 10);

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
            var menu = (ReportMenus)entity;
            _codeEdit.Text = menu.Code;
            _nameEdit.Text = menu.Name;
            _parentCodeEdit.Text = menu.ParentCode;
            _formNameEdit.Text = menu.FormName;
            _assemblyNameEdit.Text = menu.AssemblyName;
            _iconNameEdit.Text = menu.IconName;
            _displayOrderEdit.Value = menu.DisplayOrder;
            _isVisibleCheck.Checked = menu.IsVisible;
            _isReportCheck.Checked = menu.IsReport;
            _reportFileNameEdit.Text = menu.ReportFileName;
            _descriptionEdit.Text = menu.Description;

            ValidateForm();
        }

        protected override void SaveEntityData(object entity)
        {
            var menu = (ReportMenus)entity;
            menu.Code = _codeEdit.Text ?? string.Empty;
            menu.Name = _nameEdit.Text ?? string.Empty;
            menu.ParentCode = _parentCodeEdit.Text ?? string.Empty;
            menu.FormName = _formNameEdit.Text ?? string.Empty;
            menu.AssemblyName = _assemblyNameEdit.Text ?? string.Empty;
            menu.IconName = _iconNameEdit.Text ?? string.Empty;
            menu.DisplayOrder = (int)_displayOrderEdit.Value;
            menu.IsVisible = _isVisibleCheck.Checked;
            menu.IsReport = _isReportCheck.Checked;
            menu.ReportFileName = _reportFileNameEdit.Text ?? string.Empty;
            menu.Description = _descriptionEdit.Text ?? string.Empty;
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