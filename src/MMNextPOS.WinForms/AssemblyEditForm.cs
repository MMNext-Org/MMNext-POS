using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Views.Grid;
using MMNextPOS.Domain.Models;

namespace MMNextPOS.WinForms
{
    /// <summary>
    /// Edit form for an assembly (bill of materials) including its component lines.
    /// Persisted by the caller via IInventoryService.AddAssemblyAsync / UpdateAssemblyAsync.
    /// </summary>
    public partial class AssemblyEditForm : EditFormBase
    {
        private Assembly _assembly = null!;
        private bool _isNew = true;

        private TextEdit _assemblyNoEdit = null!;
        private SpinEdit _outputProductIdEdit = null!;
        private SpinEdit _outputQuantityEdit = null!;
        private DateEdit _assemblyDateEdit = null!;
        private SpinEdit _totalCostEdit = null!;
        private SpinEdit _locationIdEdit = null!;
        private ComboBoxEdit _statusCombo = null!;
        private MemoEdit _notesEdit = null!;

        private GridControl _detailsGrid = null!;
        private GridView _detailsView = null!;
        private SimpleButton _removeDetailButton = null!;
        private BindingList<AssemblyDetail> _details = null!;

        public AssemblyEditForm() : this(new Assembly(), null) { }

        public AssemblyEditForm(Assembly assembly, IEnumerable<AssemblyDetail>? details = null)
        {
            _assembly = assembly ?? new Assembly();
            _isNew = _assembly.Id == 0;
            _details = new BindingList<AssemblyDetail>((details ?? Enumerable.Empty<AssemblyDetail>()).ToList());

            InitializeComponent();
            LoadEntityData(_assembly);
        }

        private void InitializeComponent()
        {
            Text = _isNew ? "New Assembly" : "Edit Assembly";
            Size = new Size(760, 640);
            StartPosition = FormStartPosition.CenterParent;
            MinimizeBox = false;
            MaximizeBox = false;

            var mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 10,
                Padding = new Padding(15)
            };
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 140));
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            for (int i = 0; i < 8; i++)
                mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40)); // remove button row
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));  // details grid

            // Assembly No
            mainLayout.Controls.Add(CreateLabel("Assembly No *:"), 0, 0);
            _assemblyNoEdit = new TextEdit { Dock = DockStyle.Fill, Properties = { MaxLength = 50 } };
            _assemblyNoEdit.EditValueChanged += (s, e) => ValidateForm();
            mainLayout.Controls.Add(_assemblyNoEdit, 1, 0);

            // Output Product
            mainLayout.Controls.Add(CreateLabel("Output Product Id *:"), 0, 1);
            _outputProductIdEdit = new SpinEdit { Dock = DockStyle.Fill, Properties = { MinValue = 0, MaxValue = int.MaxValue } };
            _outputProductIdEdit.EditValueChanged += (s, e) => ValidateForm();
            mainLayout.Controls.Add(_outputProductIdEdit, 1, 1);

            // Output Quantity
            mainLayout.Controls.Add(CreateLabel("Output Quantity *:"), 0, 2);
            _outputQuantityEdit = new SpinEdit { Dock = DockStyle.Fill, Properties = { MinValue = 1, MaxValue = int.MaxValue } };
            _outputQuantityEdit.EditValueChanged += (s, e) => ValidateForm();
            mainLayout.Controls.Add(_outputQuantityEdit, 1, 2);

            // Assembly Date
            mainLayout.Controls.Add(CreateLabel("Assembly Date:"), 0, 3);
            _assemblyDateEdit = new DateEdit { Dock = DockStyle.Fill, Properties = { CalendarView = DevExpress.XtraEditors.Repository.CalendarView.Classic } };
            mainLayout.Controls.Add(_assemblyDateEdit, 1, 3);

            // Total Cost
            mainLayout.Controls.Add(CreateLabel("Total Cost:"), 0, 4);
            _totalCostEdit = new SpinEdit { Dock = DockStyle.Fill, Properties = { MinValue = 0, MaxValue = decimal.MaxValue, DisplayFormat = { FormatString = "n2", FormatType = DevExpress.Utils.FormatType.Numeric } } };
            mainLayout.Controls.Add(_totalCostEdit, 1, 4);

            // Location Id (0 = none)
            mainLayout.Controls.Add(CreateLabel("Location Id:"), 0, 5);
            _locationIdEdit = new SpinEdit { Dock = DockStyle.Fill, Properties = { MinValue = 0, MaxValue = int.MaxValue } };
            mainLayout.Controls.Add(_locationIdEdit, 1, 5);

            // Status
            mainLayout.Controls.Add(CreateLabel("Status:"), 0, 6);
            _statusCombo = new ComboBoxEdit { Dock = DockStyle.Fill, Properties = { TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor } };
            _statusCombo.Properties.Items.AddRange(new[] { "Active", "Completed", "Cancelled" });
            mainLayout.Controls.Add(_statusCombo, 1, 6);

            // Notes
            mainLayout.Controls.Add(CreateLabel("Notes:"), 0, 7);
            _notesEdit = new MemoEdit { Dock = DockStyle.Fill, Properties = { MaxLength = 500 } };
            mainLayout.Controls.Add(_notesEdit, 1, 7);

            // Remove detail button
            _removeDetailButton = new SimpleButton { Text = "Remove Selected Component", Dock = DockStyle.Left, Width = 200 };
            _removeDetailButton.Click += (s, e) =>
            {
                if (_detailsView.FocusedRowHandle >= 0)
                    _detailsView.DeleteRow(_detailsView.FocusedRowHandle);
            };
            mainLayout.Controls.Add(CreateLabel("Components:"), 0, 8);
            mainLayout.Controls.Add(_removeDetailButton, 1, 8);

            // Details grid (editable; add rows via the new item row at top)
            _detailsGrid = new GridControl { Dock = DockStyle.Fill };
            _detailsView = new GridView(_detailsGrid)
            {
                OptionsBehavior = { Editable = true, AllowAddRows = DevExpress.Utils.DefaultBoolean.True },
                OptionsView = { NewItemRowPosition = NewItemRowPosition.Top, ShowGroupPanel = false },
                GridControl = _detailsGrid
            };
            _detailsGrid.MainView = _detailsView;
            _detailsGrid.ViewCollection.Add(_detailsView);
            _detailsGrid.DataSource = _details;
            mainLayout.Controls.Add(_detailsGrid, 0, 9);
            mainLayout.SetColumnSpan(_detailsGrid, 2);

            Controls.Add(mainLayout);
        }

        /// <summary>
        /// Returns the component lines currently entered in the grid.
        /// LineTotal is recomputed from quantity and unit cost.
        /// </summary>
        public IReadOnlyList<AssemblyDetail> GetDetails()
        {
            _detailsView.CloseEditor();
            _detailsView.UpdateCurrentRow();

            var lines = new List<AssemblyDetail>();
            foreach (var detail in _details)
            {
                if (detail.ComponentProductId <= 0 || detail.Quantity <= 0)
                    continue;

                detail.LineTotal = detail.Quantity * detail.UnitCost;
                lines.Add(detail);
            }
            return lines;
        }

        protected override bool ValidateForm()
        {
            bool isValid = true;

            if (string.IsNullOrWhiteSpace(_assemblyNoEdit.Text))
                isValid = false;

            if (_outputProductIdEdit.Value <= 0)
                isValid = false;

            if (_outputQuantityEdit.Value <= 0)
                isValid = false;

            _okButton.Enabled = isValid;
            return isValid;
        }

        public override void LoadEntityData(object entity)
        {
            _assembly = (Assembly)entity;
            _assemblyNoEdit.Text = _assembly.AssemblyNo;
            _outputProductIdEdit.Value = _assembly.OutputProductId;
            _outputQuantityEdit.Value = Math.Max(1, _assembly.OutputQuantity);
            _assemblyDateEdit.EditValue = _assembly.AssemblyDate == default ? DateTime.Today : _assembly.AssemblyDate;
            _totalCostEdit.Value = _assembly.TotalCost;
            _locationIdEdit.Value = _assembly.LocationId ?? 0;
            _statusCombo.EditValue = string.IsNullOrEmpty(_assembly.Status) ? "Active" : _assembly.Status;
            _notesEdit.Text = _assembly.Notes ?? string.Empty;

            ValidateForm();
        }

        public override void SaveEntityData(object entity)
        {
            var assembly = (Assembly)entity;
            assembly.AssemblyNo = _assemblyNoEdit.Text.Trim();
            assembly.OutputProductId = (int)_outputProductIdEdit.Value;
            assembly.OutputQuantity = (int)_outputQuantityEdit.Value;
            assembly.AssemblyDate = _assemblyDateEdit.DateTime == default ? DateTime.Today : _assemblyDateEdit.DateTime;
            assembly.TotalCost = _totalCostEdit.Value;
            assembly.LocationId = _locationIdEdit.Value <= 0 ? null : (int)_locationIdEdit.Value;
            assembly.Status = _statusCombo.EditValue?.ToString() ?? "Active";
            assembly.Notes = string.IsNullOrWhiteSpace(_notesEdit.Text) ? null : _notesEdit.Text.Trim();
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
