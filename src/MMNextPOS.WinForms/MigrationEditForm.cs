using System;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using MMNextPOS.Application.Services;
using MMNextPOS.Domain.Models;

namespace MMNextPOS.WinForms
{
    /// <summary>
    /// Editor for a data migration definition. Also exposes run, cancel and
    /// history actions for migrations that are already saved.
    /// </summary>
    public class MigrationEditForm : EditFormBase
    {
        private readonly DataMigration _migration;
        private readonly IMigrationService _service;
        private readonly bool _isNew;

        private TextEdit _nameEdit = null!;
        private TextEdit _descriptionEdit = null!;
        private ComboBoxEdit _sourceTypeCombo = null!;
        private MemoEdit _sourceConnectionEdit = null!;
        private ComboBoxEdit _targetTypeCombo = null!;
        private MemoEdit _targetConnectionEdit = null!;
        private ComboBoxEdit _scheduleTypeCombo = null!;
        private SpinEdit _maxRetriesEdit = null!;
        private CheckEdit _isActiveCheck = null!;

        private SimpleButton _runButton = null!;
        private SimpleButton _cancelButton2 = null!;
        private SimpleButton _historyButton = null!;

        public MigrationEditForm(DataMigration migration, IMigrationService service)
        {
            _migration = migration ?? new DataMigration();
            _service = service ?? throw new ArgumentNullException(nameof(service));
            _isNew = _migration.Id == 0;

            InitializeComponent();
            LoadEntityData(_migration);
        }

        private void InitializeComponent()
        {
            Text = _isNew ? "New Data Migration" : "Edit Data Migration";
            Size = new Size(640, 640);
            MinimumSize = new Size(600, 560);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.Sizable;
            MaximizeBox = false;
            MinimizeBox = false;
            AutoScroll = true;

            // Action toolbar (run/cancel/history only make sense once saved)
            var toolbar = new PanelControl
            {
                Dock = DockStyle.Top,
                Height = 52,
                BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder
            };
            _runButton = new SimpleButton { Text = "Run Migration", Width = 120, Height = 32, Location = new Point(10, 10), Enabled = !_isNew };
            _runButton.Click += async (s, e) => await RunMigrationAsync();
            _cancelButton2 = new SimpleButton { Text = "Cancel Run", Width = 100, Height = 32, Location = new Point(140, 10), Enabled = !_isNew };
            _cancelButton2.Click += async (s, e) => await CancelMigrationAsync();
            _historyButton = new SimpleButton { Text = "History", Width = 100, Height = 32, Location = new Point(250, 10), Enabled = !_isNew };
            _historyButton.Click += async (s, e) => await ShowHistoryAsync();
            toolbar.Controls.Add(_runButton);
            toolbar.Controls.Add(_cancelButton2);
            toolbar.Controls.Add(_historyButton);
            Controls.Add(toolbar);

            var mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 10,
                Padding = new Padding(20)
            };
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 170));
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            for (int i = 0; i < 9; i++)
                mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 46));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            int row = 0;

            mainLayout.Controls.Add(CreateLabel("Name *:"), 0, row);
            _nameEdit = new TextEdit { Dock = DockStyle.Fill, Properties = { MaxLength = 100 } };
            _nameEdit.EditValueChanged += (s, e) => ValidateForm();
            mainLayout.Controls.Add(_nameEdit, 1, row++);

            mainLayout.Controls.Add(CreateLabel("Description:"), 0, row);
            _descriptionEdit = new TextEdit { Dock = DockStyle.Fill, Properties = { MaxLength = 500 } };
            mainLayout.Controls.Add(_descriptionEdit, 1, row++);

            mainLayout.Controls.Add(CreateLabel("Source Type:"), 0, row);
            _sourceTypeCombo = CreateCombo(new[] { "MySQL", "SQLServer", "PostgreSQL", "CSV", "Excel", "JSON", "XML" });
            mainLayout.Controls.Add(_sourceTypeCombo, 1, row++);

            mainLayout.Controls.Add(CreateLabel("Source Connection:"), 0, row);
            _sourceConnectionEdit = new MemoEdit { Dock = DockStyle.Fill, Properties = { MaxLength = 500 } };
            mainLayout.Controls.Add(_sourceConnectionEdit, 1, row++);

            mainLayout.Controls.Add(CreateLabel("Target Type:"), 0, row);
            _targetTypeCombo = CreateCombo(new[] { "MySQL", "SQLServer", "PostgreSQL" });
            mainLayout.Controls.Add(_targetTypeCombo, 1, row++);

            mainLayout.Controls.Add(CreateLabel("Target Connection:"), 0, row);
            _targetConnectionEdit = new MemoEdit { Dock = DockStyle.Fill, Properties = { MaxLength = 500 } };
            mainLayout.Controls.Add(_targetConnectionEdit, 1, row++);

            mainLayout.Controls.Add(CreateLabel("Schedule Type:"), 0, row);
            _scheduleTypeCombo = CreateCombo(new[] { "Manual", "Once", "Daily", "Weekly", "Monthly" });
            mainLayout.Controls.Add(_scheduleTypeCombo, 1, row++);

            mainLayout.Controls.Add(CreateLabel("Max Retries:"), 0, row);
            _maxRetriesEdit = new SpinEdit { Dock = DockStyle.Fill, Properties = { MinValue = 0, MaxValue = 10, IsFloatValue = false } };
            mainLayout.Controls.Add(_maxRetriesEdit, 1, row++);

            mainLayout.Controls.Add(CreateLabel("Active:"), 0, row);
            _isActiveCheck = new CheckEdit { Dock = DockStyle.Fill };
            mainLayout.Controls.Add(_isActiveCheck, 1, row++);

            Controls.Add(mainLayout);
        }

        protected override bool ValidateForm()
        {
            bool isValid = !string.IsNullOrWhiteSpace(_nameEdit.Text);
            _okButton.Enabled = isValid;
            return isValid;
        }

        public override void LoadEntityData(object entity)
        {
            var migration = (DataMigration)entity;
            _nameEdit.Text = migration.Name;
            _descriptionEdit.Text = migration.Description;
            SelectByText(_sourceTypeCombo, migration.SourceType);
            _sourceConnectionEdit.Text = migration.SourceConnectionString;
            SelectByText(_targetTypeCombo, migration.TargetType);
            _targetConnectionEdit.Text = migration.TargetConnectionString;
            SelectByText(_scheduleTypeCombo, migration.ScheduleType);
            _maxRetriesEdit.Value = migration.MaxRetries;
            _isActiveCheck.Checked = migration.IsActive;

            ValidateForm();
        }

        public override void SaveEntityData(object entity)
        {
            var migration = (DataMigration)entity;
            migration.Name = _nameEdit.Text.Trim();
            migration.Description = _descriptionEdit.Text.Trim();
            migration.SourceType = string.IsNullOrWhiteSpace(_sourceTypeCombo.Text) ? string.Empty : _sourceTypeCombo.Text;
            migration.SourceConnectionString = _sourceConnectionEdit.Text.Trim();
            migration.TargetType = string.IsNullOrWhiteSpace(_targetTypeCombo.Text) ? "MySQL" : _targetTypeCombo.Text;
            migration.TargetConnectionString = _targetConnectionEdit.Text.Trim();
            migration.ScheduleType = string.IsNullOrWhiteSpace(_scheduleTypeCombo.Text) ? "Manual" : _scheduleTypeCombo.Text;
            migration.MaxRetries = (int)_maxRetriesEdit.Value;
            migration.IsActive = _isActiveCheck.Checked;
        }

        private async Task RunMigrationAsync()
        {
            if (_isNew) return;
            SetWaitCursor(true);
            try
            {
                var ok = await _service.RunMigrationAsync(_migration.Name, CancellationToken);
                ShowInfo(ok ? "Migration completed successfully." : "Migration failed. Check the migration history for details.");
            }
            catch (Exception ex)
            {
                ShowError($"Migration failed: {ex.Message}");
            }
            finally
            {
                SetWaitCursor(false);
            }
        }

        private async Task CancelMigrationAsync()
        {
            if (_isNew) return;
            try
            {
                var ok = await _service.CancelMigrationAsync(_migration.Id, CancellationToken);
                ShowInfo(ok ? "Migration cancellation requested." : "Migration could not be cancelled.");
            }
            catch (Exception ex)
            {
                ShowError($"Cancel failed: {ex.Message}");
            }
        }

        private async Task ShowHistoryAsync()
        {
            if (_isNew) return;
            try
            {
                var history = await _service.GetMigrationHistoryAsync(_migration.Id, 50, CancellationToken);
                if (history == null || history.Count == 0)
                {
                    ShowInfo("No migration history yet.");
                    return;
                }

                var sb = new StringBuilder();
                foreach (var step in history.OrderByDescending(h => h.StartedAt).Take(20))
                {
                    sb.AppendLine($"{step.StepName} | {step.Status} | processed {step.RecordsProcessed}, failed {step.RecordsFailed}");
                    if (!string.IsNullOrWhiteSpace(step.ErrorMessage))
                        sb.AppendLine($"   Error: {step.ErrorMessage}");
                }

                ShowInfo("Recent migration steps:\n\n" + sb);
            }
            catch (Exception ex)
            {
                ShowError($"Failed to load history: {ex.Message}");
            }
        }

        private static ComboBoxEdit CreateCombo(string[] items)
        {
            var combo = new ComboBoxEdit
            {
                Dock = DockStyle.Fill,
                Properties = { TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor }
            };
            combo.Properties.Items.AddRange(items);
            return combo;
        }

        private static void SelectByText(ComboBoxEdit combo, string value)
        {
            for (int i = 0; i < combo.Properties.Items.Count; i++)
            {
                if (string.Equals(combo.Properties.Items[i]?.ToString(), value, StringComparison.OrdinalIgnoreCase))
                {
                    combo.SelectedIndex = i;
                    return;
                }
            }
            combo.SelectedIndex = combo.Properties.Items.Count > 0 ? 0 : -1;
        }

        private static LabelControl CreateLabel(string text)
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