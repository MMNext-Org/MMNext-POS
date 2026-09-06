using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Controls;
using DevExpress.XtraEditors.Repository;
using MMNextPOS.Application.Services;

namespace MMNextPOS.WinForms.Reports
{
    /// <summary>
    /// Base class for report parameter forms.
    /// </summary>
    public abstract class ReportParameterForm : XtraForm
    {
        protected Dictionary<string, object> _parameters = new();
        protected bool _isValid = false;

        protected ReportParameterForm(string title)
        {
            Text = title;
            Size = new Size(450, 400);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowIcon = false;
            ShowInTaskbar = false;
            KeyPreview = true;

            InitializeBaseComponents();
        }

        protected virtual void InitializeBaseComponents()
        {
            var mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3,
                Padding = new Padding(20)
            };
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 60));

            // Parameter controls will be added by derived classes
            var paramPanel = new PanelControl
            {
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyles.NoBorder
            };
            mainLayout.Controls.Add(paramPanel, 0, 0);

            // Buttons
            var buttonPanel = new PanelControl
            {
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyles.NoBorder
            };
            var okButton = new SimpleButton
            {
                Text = "Generate",
                DialogResult = DialogResult.OK,
                Width = 100,
                Height = 35,
                Location = new Point(220, 12)
            };
            okButton.Click += (s, e) => { if (ValidateParameters()) _isValid = true; else DialogResult = DialogResult.None; };

            var cancelButton = new SimpleButton
            {
                Text = "Cancel",
                DialogResult = DialogResult.Cancel,
                Width = 100,
                Height = 35,
                Location = new Point(330, 12)
            };

            buttonPanel.Controls.Add(okButton);
            buttonPanel.Controls.Add(cancelButton);
            mainLayout.Controls.Add(buttonPanel, 0, 1);

            Controls.Add(mainLayout);
        }

        protected virtual bool ValidateParameters()
        {
            return true;
        }

        public Dictionary<string, object> GetParameters()
        {
            return _parameters;
        }

        public bool IsValid => _isValid;
    }

    /// <summary>
    /// Parameter form for date range reports.
    /// </summary>
    public class DateRangeParameterForm : ReportParameterForm
    {
        private DateEdit _dateFromEdit = null!;
        private DateEdit _dateToEdit = null!;
        private LookUpEdit _locationEdit = null!;
        private CheckEdit _includeDetailsCheck = null!;

        public DateRangeParameterForm(string title, string reportName) : base(title)
        {
            InitializeParameters();
        }

        private void InitializeParameters()
        {
            var paramPanel = Controls.OfType<TableLayoutPanel>().First().Controls.OfType<PanelControl>().First();
            paramPanel.Controls.Clear();

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 5,
                Padding = new Padding(0)
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 140));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            for (int i = 0; i < 4; i++)
                layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 45));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            // Date From
            layout.Controls.Add(CreateLabel("Date From *:"), 0, 0);
            _dateFromEdit = new DateEdit
            {
                Dock = DockStyle.Fill,
                Properties = { AutoHeight = false, CalendarView = DevExpress.XtraEditors.Repository.CalendarView.Classic }
            };
            _dateFromEdit.EditValue = DateTime.Today.AddDays(-30);
            layout.Controls.Add(_dateFromEdit, 1, 0);

            // Date To
            layout.Controls.Add(CreateLabel("Date To *:"), 0, 1);
            _dateToEdit = new DateEdit
            {
                Dock = DockStyle.Fill,
                Properties = { AutoHeight = false, CalendarView = DevExpress.XtraEditors.Repository.CalendarView.Classic }
            };
            _dateToEdit.EditValue = DateTime.Today;
            layout.Controls.Add(_dateToEdit, 1, 1);

            // Location (optional - will be populated from service)
            layout.Controls.Add(CreateLabel("Location:"), 0, 2);
            _locationEdit = new LookUpEdit
            {
                Dock = DockStyle.Fill,
                Properties = { TextEditStyle = TextEditStyles.Standard, NullText = "All Locations" }
            };
            layout.Controls.Add(_locationEdit, 1, 2);

            // Include Details
            layout.Controls.Add(CreateLabel("Options:"), 0, 3);
            _includeDetailsCheck = new CheckEdit
            {
                Dock = DockStyle.Fill,
                Text = "Include detailed breakdown",
                Checked = true
            };
            layout.Controls.Add(_includeDetailsCheck, 1, 3);

            paramPanel.Controls.Add(layout);
        }

        protected override bool ValidateParameters()
        {
            if (_dateFromEdit.EditValue == null || _dateToEdit.EditValue == null)
            {
                XtraMessageBox.Show(this, "Please select both From and To dates.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            var fromDate = _dateFromEdit.DateTime;
            var toDate = _dateToEdit.DateTime;

            if (fromDate > toDate)
            {
                XtraMessageBox.Show(this, "From date cannot be after To date.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            _parameters["DateFrom"] = fromDate;
            _parameters["DateTo"] = toDate;
            _parameters["LocationId"] = _locationEdit.EditValue ?? 0;
            _parameters["IncludeDetails"] = _includeDetailsCheck.Checked;

            return true;
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

    /// <summary>
    /// Parameter form for single entity reports (e.g., Sale Receipt by SaleId).
    /// </summary>
    public class EntityParameterForm : ReportParameterForm
    {
        private LookUpEdit _entityLookup = null!;
        private TextEdit _entityCodeEdit = null!;
        private RadioGroup _selectionMode = null!;

        public EntityParameterForm(string title, string entityLabel) : base(title)
        {
            InitializeParameters(entityLabel);
        }

        private void InitializeParameters(string entityLabel)
        {
            var paramPanel = Controls.OfType<TableLayoutPanel>().First().Controls.OfType<PanelControl>().First();
            paramPanel.Controls.Clear();

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 4,
                Padding = new Padding(0)
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 140));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            for (int i = 0; i < 3; i++)
                layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            // Selection Mode
            layout.Controls.Add(CreateLabel("Select by:"), 0, 0);
            _selectionMode = new RadioGroup
            {
                Dock = DockStyle.Fill,
                Properties = { Items = { new RadioGroupItem("Code", "By Code/Number"), new RadioGroupItem("Lookup", "Select from List") }, Columns = 2 }
            };
            _selectionMode.SelectedIndex = 0;
            _selectionMode.SelectedIndexChanged += (s, e) => UpdateVisibility();
            layout.Controls.Add(_selectionMode, 1, 0);

            // Code/Number Input
            layout.Controls.Add(CreateLabel($"{entityLabel} Code:"), 0, 1);
            _entityCodeEdit = new TextEdit
            {
                Dock = DockStyle.Fill,
                Properties = { MaxLength = 50 }
            };
            layout.Controls.Add(_entityCodeEdit, 1, 1);

            // Lookup (populated from service)
            layout.Controls.Add(CreateLabel($"Select {entityLabel}:"), 0, 2);
            _entityLookup = new LookUpEdit
            {
                Dock = DockStyle.Fill,
                Properties = { TextEditStyle = TextEditStyles.Standard, NullText = $"Select {entityLabel}...", DisplayMember = "DisplayText", ValueMember = "Id" }
            };
            layout.Controls.Add(_entityLookup, 1, 2);

            paramPanel.Controls.Add(layout);
            UpdateVisibility();
        }

        private void UpdateVisibility()
        {
            var isCodeMode = _selectionMode.SelectedIndex == 0;
            _entityCodeEdit.Visible = isCodeMode;
            _entityLookup.Visible = !isCodeMode;
        }

        protected override bool ValidateParameters()
        {
            if (_selectionMode.SelectedIndex == 0)
            {
                if (string.IsNullOrWhiteSpace(_entityCodeEdit.Text))
                {
                    XtraMessageBox.Show(this, "Please enter a code/number.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }
                _parameters["EntityCode"] = _entityCodeEdit.Text.Trim();
            }
            else
            {
                if (_entityLookup.EditValue == null)
                {
                    XtraMessageBox.Show(this, "Please select an entity.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }
                _parameters["EntityId"] = (int)_entityLookup.EditValue;
            }

            return true;
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
