using System;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraGrid.Views.Grid;
using MMNextPOS.Application.Services;
using MMNextPOS.Domain.Models;

namespace MMNextPOS.WinForms
{
    /// <summary>
    /// Super admin console: administration (change-date) log viewer, security
    /// audit report and system maintenance actions (clear cache, rebuild
    /// indexes, vacuum database).
    /// </summary>
    public class SuperAdminForm : AsyncFormBase
    {
        private readonly ISuperAdminService _service;

        private TabControl _tabs = null!;

        // Log tab
        private DateEdit _logFromEdit = null!;
        private DateEdit _logToEdit = null!;
        private SimpleButton _logRefreshButton = null!;
        private GridControl _logGrid = null!;
        private GridView _logGridView = null!;

        // Security audit tab
        private DateEdit _auditFromEdit = null!;
        private DateEdit _auditToEdit = null!;
        private SimpleButton _runAuditButton = null!;
        private GridControl _auditGrid = null!;
        private GridView _auditGridView = null!;

        // Maintenance tab
        private SimpleButton _clearCacheButton = null!;
        private SimpleButton _rebuildIndexesButton = null!;
        private SimpleButton _vacuumButton = null!;
        private LabelControl _maintenanceStatusLabel = null!;

        public SuperAdminForm(ISuperAdminService service)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));

            InitializeComponent();
            _ = LoadLogsAsync();
        }

        private void InitializeComponent()
        {
            Text = "Super Admin";
            Size = new Size(1100, 700);
            MinimumSize = new Size(900, 560);
            StartPosition = FormStartPosition.CenterParent;

            _tabs = new TabControl { Dock = DockStyle.Fill };

            _tabs.TabPages.Add(BuildLogTab());
            _tabs.TabPages.Add(BuildAuditTab());
            _tabs.TabPages.Add(BuildMaintenanceTab());

            Controls.Add(_tabs);
        }

        private TabPage BuildLogTab()
        {
            var page = new TabPage("Administration Log");

            var toolbar = new PanelControl
            {
                Dock = DockStyle.Top,
                Height = 52,
                BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder
            };
            toolbar.Controls.Add(new LabelControl { Text = "From:", Location = new Point(10, 16) });
            _logFromEdit = new DateEdit { Location = new Point(55, 13), Width = 120, Properties = { CalendarView = DevExpress.XtraEditors.Repository.CalendarView.Classic, NullDate = DateTime.MinValue } };
            toolbar.Controls.Add(_logFromEdit);
            toolbar.Controls.Add(new LabelControl { Text = "To:", Location = new Point(185, 16) });
            _logToEdit = new DateEdit { Location = new Point(215, 13), Width = 120, Properties = { CalendarView = DevExpress.XtraEditors.Repository.CalendarView.Classic, NullDate = DateTime.MinValue } };
            toolbar.Controls.Add(_logToEdit);
            _logRefreshButton = new SimpleButton { Text = "Refresh", Width = 90, Height = 30, Location = new Point(350, 11) };
            _logRefreshButton.Click += async (s, e) => await LoadLogsAsync();
            toolbar.Controls.Add(_logRefreshButton);

            _logGrid = new GridControl { Dock = DockStyle.Fill };
            _logGridView = new GridView(_logGrid)
            {
                OptionsBehavior = { ReadOnly = true },
                OptionsSelection = { MultiSelect = false },
                OptionsView = { ShowGroupPanel = false, EnableAppearanceEvenRow = true, EnableAppearanceOddRow = true },
                GridControl = _logGrid
            };
            _logGrid.MainView = _logGridView;
            _logGrid.ViewCollection.Add(_logGridView);
            ConfigureLogColumns(_logGridView);

            page.Controls.Add(_logGrid);
            page.Controls.Add(toolbar);
            return page;
        }

        private static void ConfigureLogColumns(GridView view)
        {
            view.Columns.Clear();
            view.Columns.AddRange(new[]
            {
                new GridColumn { FieldName = "Id", Caption = "ID", Width = 50, Visible = true },
                new GridColumn { FieldName = "CreatedAt", Caption = "Timestamp", Width = 130, Visible = true },
                new GridColumn { FieldName = "Module", Caption = "Module", Width = 90, Visible = true },
                new GridColumn { FieldName = "Action", Caption = "Action", Width = 110, Visible = true },
                new GridColumn { FieldName = "EntityType", Caption = "Entity", Width = 110, Visible = true },
                new GridColumn { FieldName = "EntityId", Caption = "Entity ID", Width = 70, Visible = true },
                new GridColumn { FieldName = "PerformedBy", Caption = "Performed By", Width = 120, Visible = true },
                new GridColumn { FieldName = "IpAddress", Caption = "IP Address", Width = 110, Visible = true },
                new GridColumn { FieldName = "Severity", Caption = "Severity", Width = 80, Visible = true },
                new GridColumn { FieldName = "Description", Caption = "Description", Width = 250, Visible = true },
                new GridColumn { FieldName = "IsSensitive", Caption = "Sensitive", Width = 70, Visible = true }
            });
        }

        private TabPage BuildAuditTab()
        {
            var page = new TabPage("Security Audit");

            var toolbar = new PanelControl
            {
                Dock = DockStyle.Top,
                Height = 52,
                BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder
            };
            toolbar.Controls.Add(new LabelControl { Text = "From:", Location = new Point(10, 16) });
            _auditFromEdit = new DateEdit { Location = new Point(55, 13), Width = 120, Properties = { CalendarView = DevExpress.XtraEditors.Repository.CalendarView.Classic, NullDate = DateTime.MinValue } };
            toolbar.Controls.Add(_auditFromEdit);
            toolbar.Controls.Add(new LabelControl { Text = "To:", Location = new Point(185, 16) });
            _auditToEdit = new DateEdit { Location = new Point(215, 13), Width = 120, Properties = { CalendarView = DevExpress.XtraEditors.Repository.CalendarView.Classic, NullDate = DateTime.MinValue } };
            toolbar.Controls.Add(_auditToEdit);
            _runAuditButton = new SimpleButton { Text = "Run Audit", Width = 100, Height = 30, Location = new Point(350, 11) };
            _runAuditButton.Click += async (s, e) => await RunSecurityAuditAsync();
            toolbar.Controls.Add(_runAuditButton);

            _auditGrid = new GridControl { Dock = DockStyle.Fill };
            _auditGridView = new GridView(_auditGrid)
            {
                OptionsBehavior = { ReadOnly = true },
                OptionsSelection = { MultiSelect = false },
                OptionsView = { ShowGroupPanel = false, EnableAppearanceEvenRow = true, EnableAppearanceOddRow = true },
                GridControl = _auditGrid
            };
            _auditGrid.MainView = _auditGridView;
            _auditGrid.ViewCollection.Add(_auditGridView);
            _auditGridView.Columns.Clear();
            _auditGridView.Columns.AddRange(new[]
            {
                new GridColumn { FieldName = "Category", Caption = "Category", Width = 130, Visible = true },
                new GridColumn { FieldName = "Check", Caption = "Check", Width = 180, Visible = true },
                new GridColumn { FieldName = "Status", Caption = "Status", Width = 80, Visible = true },
                new GridColumn { FieldName = "Severity", Caption = "Severity", Width = 80, Visible = true },
                new GridColumn { FieldName = "Details", Caption = "Details", Width = 300, Visible = true },
                new GridColumn { FieldName = "Recommendation", Caption = "Recommendation", Width = 250, Visible = true }
            });

            page.Controls.Add(_auditGrid);
            page.Controls.Add(toolbar);
            return page;
        }

        private TabPage BuildMaintenanceTab()
        {
            var page = new TabPage("Maintenance");

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 6,
                Padding = new Padding(30)
            };
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 20));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            _clearCacheButton = new SimpleButton { Text = "Clear Cache", Dock = DockStyle.Fill, Height = 36 };
            _clearCacheButton.Click += async (s, e) => await RunMaintenanceAsync("Clear Cache", _service.ClearCacheAsync);
            layout.Controls.Add(_clearCacheButton, 0, 0);

            _rebuildIndexesButton = new SimpleButton { Text = "Rebuild Indexes", Dock = DockStyle.Fill, Height = 36 };
            _rebuildIndexesButton.Click += async (s, e) => await RunMaintenanceAsync("Rebuild Indexes", _service.RebuildIndexesAsync);
            layout.Controls.Add(_rebuildIndexesButton, 0, 1);

            _vacuumButton = new SimpleButton { Text = "Vacuum Database", Dock = DockStyle.Fill, Height = 36 };
            _vacuumButton.Click += async (s, e) => await RunMaintenanceAsync("Vacuum Database", _service.VacuumDatabaseAsync);
            layout.Controls.Add(_vacuumButton, 0, 2);

            _maintenanceStatusLabel = new LabelControl
            {
                Text = "Ready.",
                Dock = DockStyle.Fill,
                AutoSizeMode = LabelAutoSizeMode.None
            };
            layout.Controls.Add(_maintenanceStatusLabel, 0, 4);

            page.Controls.Add(layout);
            return page;
        }

        private async Task LoadLogsAsync()
        {
            SetWaitCursor(true);
            try
            {
                var fromDate = _logFromEdit.DateTime == DateTime.MinValue ? DateTime.UtcNow.AddDays(-30) : _logFromEdit.DateTime;
                var toDate = _logToEdit.DateTime == DateTime.MinValue ? DateTime.UtcNow : _logToEdit.DateTime;

                var logs = await _service.GetByDateRangeAsync(fromDate, toDate, CancellationToken);
                _logGrid.DataSource = logs;
                _logGridView.BestFitColumns();
            }
            catch (Exception ex)
            {
                ShowError($"Failed to load administration log: {ex.Message}");
            }
            finally
            {
                SetWaitCursor(false);
            }
        }

        private async Task RunSecurityAuditAsync()
        {
            SetWaitCursor(true);
            try
            {
                var fromDate = _auditFromEdit.DateTime == DateTime.MinValue ? DateTime.UtcNow.AddDays(-30) : _auditFromEdit.DateTime;
                var toDate = _auditToEdit.DateTime == DateTime.MinValue ? DateTime.UtcNow : _auditToEdit.DateTime;

                var results = await _service.GetSecurityAuditAsync(fromDate, toDate, CancellationToken);
                _auditGrid.DataSource = results;
                _auditGridView.BestFitColumns();
            }
            catch (Exception ex)
            {
                ShowError($"Security audit failed: {ex.Message}");
            }
            finally
            {
                SetWaitCursor(false);
            }
        }

        private async Task RunMaintenanceAsync(string actionName, Func<CancellationToken, Task<bool>> action)
        {
            if (!ShowConfirm($"Run '{actionName}' now?"))
                return;

            SetWaitCursor(true);
            try
            {
                var ok = await action(CancellationToken);
                _maintenanceStatusLabel.Text = ok
                    ? $"{actionName}: completed successfully at {DateTime.Now:HH:mm:ss}."
                    : $"{actionName}: failed. See the administration log for details.";
                ShowInfo(_maintenanceStatusLabel.Text);
            }
            catch (Exception ex)
            {
                _maintenanceStatusLabel.Text = $"{actionName}: error - {ex.Message}";
                ShowError($"{actionName} failed: {ex.Message}");
            }
            finally
            {
                SetWaitCursor(false);
            }
        }
    }
}
