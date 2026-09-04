using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
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
    /// Base class for list pages with search, refresh, new, edit, delete, export CSV, paging, and advanced filtering.
    /// T must be an entity with an Id property.
    /// </summary>
    /// <typeparam name="T">Entity type that inherits from EntityBase</typeparam>
    /// <typeparam name="TService">Service type that provides CRUD operations</typeparam>
    public abstract class ListPage<T, TService> : AsyncFormBase
        where T : EntityBase
        where TService : class
    {
        protected readonly TService _service;
        protected readonly IServiceProvider _serviceProvider;

        protected GridControl _grid = null!;
        protected GridView _gridView = null!;
        protected TextEdit _searchBox = null!;
        protected SimpleButton _refreshButton = null!;
        protected SimpleButton _newButton = null!;
        protected SimpleButton _editButton = null!;
        protected SimpleButton _deleteButton = null!;
        protected SimpleButton _exportButton = null!;
        protected LabelControl _statusLabel = null!;
        protected LabelControl _pageInfoLabel = null!;

        // Advanced filter controls
        protected DateEdit _dateFromEdit = null!;
        protected DateEdit _dateToEdit = null!;
        protected ComboBoxEdit _statusFilter = null!;
        protected SimpleButton _advancedFilterButton = null!;
        protected PanelControl _advancedFilterPanel = null!;
        protected bool _isAdvancedFilterExpanded = false;

        // Filter state
        protected DateTime? _filterDateFrom = null;
        protected DateTime? _filterDateTo = null;
        protected string _filterStatus = string.Empty;
        protected string _searchText = string.Empty;

        // Paging
        protected int _currentPage = 1;
        protected int _pageSize = 25;
        protected int _totalCount = 0;

        protected ListPage(TService service, IServiceProvider serviceProvider)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));

            InitializeComponent();
        }

        protected virtual void InitializeComponent()
        {
            this.Text = GetPageTitle();
            this.Size = new Size(1200, 800);
            this.MinimumSize = new Size(1000, 600);
            this.StartPosition = FormStartPosition.CenterParent;

            // Initialize controls
            _searchBox = new TextEdit
            {
                Location = new Point(70, 12),
                Width = 250,
                Properties = { NullValuePrompt = "Search..." }
            };
            _searchBox.EditValueChanged += async (s, e) => { _searchText = _searchBox.Text; await FilterAsync(); };

            _refreshButton = new SimpleButton
            {
                Text = "Refresh",
                Location = new Point(330, 10),
                Width = 80,
                Height = 30
            };
            _refreshButton.Click += async (s, e) => await LoadAsync();

            _newButton = new SimpleButton
            {
                Text = "New",
                Location = new Point(420, 10),
                Width = 80,
                Height = 30
            };
            _newButton.Click += async (s, e) => await OnNewAsync();

            _editButton = new SimpleButton
            {
                Text = "Edit",
                Location = new Point(510, 10),
                Width = 80,
                Height = 30,
                Enabled = false
            };
            _editButton.Click += async (s, e) => await OnEditAsync();

            _deleteButton = new SimpleButton
            {
                Text = "Delete",
                Location = new Point(600, 10),
                Width = 80,
                Height = 30,
                Enabled = false
            };
            _deleteButton.Click += async (s, e) => await OnDeleteAsync();

            _exportButton = new SimpleButton
            {
                Text = "Export CSV",
                Location = new Point(690, 10),
                Width = 100,
                Height = 30
            };
            _exportButton.Click += async (s, e) => await ExportCsvAsync();

            // Advanced filter button
            _advancedFilterButton = new SimpleButton
            {
                Text = "▼ Filters",
                Location = new Point(800, 10),
                Width = 100,
                Height = 30
            };
            _advancedFilterButton.Click += (s, e) => ToggleAdvancedFilter();

            // Toolbar
            var toolbar = new PanelControl
            {
                Dock = DockStyle.Top,
                Height = 50,
                BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder
            };

            toolbar.Controls.Add(new LabelControl { Text = "Search:", Location = new Point(10, 15), AutoSizeMode = LabelAutoSizeMode.None, Size = new Size(60, 25) });
            toolbar.Controls.Add(_searchBox);
            toolbar.Controls.Add(_refreshButton);
            toolbar.Controls.Add(_newButton);
            toolbar.Controls.Add(_editButton);
            toolbar.Controls.Add(_deleteButton);
            toolbar.Controls.Add(_exportButton);
            toolbar.Controls.Add(_advancedFilterButton);

            // Advanced Filter Panel (initially hidden)
            _advancedFilterPanel = new PanelControl
            {
                Dock = DockStyle.Top,
                Height = 0,
                BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.Simple,
                Visible = false
            };
            _advancedFilterPanel.Padding = new Padding(10);

            var filterLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 4,
                RowCount = 2,
                Padding = new Padding(10)
            };
            filterLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100));
            filterLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            filterLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100));
            filterLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            filterLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
            filterLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));

            // Date From
            filterLayout.Controls.Add(new LabelControl { Text = "Date From:", Dock = DockStyle.Fill, AutoSizeMode = LabelAutoSizeMode.None, Appearance = { TextOptions = { HAlignment = DevExpress.Utils.HorzAlignment.Far } } }, 0, 0);
            _dateFromEdit = new DateEdit
            {
                Dock = DockStyle.Fill,
                Properties = { AutoHeight = false, CalendarView = DevExpress.XtraEditors.Repository.CalendarView.Classic, NullDate = DateTime.MinValue }
            };
            _dateFromEdit.EditValueChanged += (s, e) => { _filterDateFrom = _dateFromEdit.DateTime == DateTime.MinValue ? null : _dateFromEdit.DateTime; };
            filterLayout.Controls.Add(_dateFromEdit, 1, 0);

            // Date To
            filterLayout.Controls.Add(new LabelControl { Text = "Date To:", Dock = DockStyle.Fill, AutoSizeMode = LabelAutoSizeMode.None, Appearance = { TextOptions = { HAlignment = DevExpress.Utils.HorzAlignment.Far } } }, 2, 0);
            _dateToEdit = new DateEdit
            {
                Dock = DockStyle.Fill,
                Properties = { AutoHeight = false, CalendarView = DevExpress.XtraEditors.Repository.CalendarView.Classic, NullDate = DateTime.MinValue }
            };
            _dateToEdit.EditValueChanged += (s, e) => { _filterDateTo = _dateToEdit.DateTime == DateTime.MinValue ? null : _dateToEdit.DateTime; };
            filterLayout.Controls.Add(_dateToEdit, 3, 0);

            // Status Filter
            filterLayout.Controls.Add(new LabelControl { Text = "Status:", Dock = DockStyle.Fill, AutoSizeMode = LabelAutoSizeMode.None, Appearance = { TextOptions = { HAlignment = DevExpress.Utils.HorzAlignment.Far } } }, 0, 1);
            _statusFilter = new ComboBoxEdit
            {
                Dock = DockStyle.Fill,
                Properties = { TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor }
            };
            _statusFilter.Properties.Items.AddRange(new[] { "All", "Active", "Inactive", "Draft", "Active", "Hold", "Received", "Cancelled", "Completed", "Voided" });
            _statusFilter.SelectedIndex = 0;
            _statusFilter.EditValueChanged += (s, e) => { _filterStatus = _statusFilter.Text == "All" ? string.Empty : _statusFilter.Text; };
            filterLayout.Controls.Add(_statusFilter, 1, 1);

            // Clear Filters Button
            var clearFiltersButton = new SimpleButton
            {
                Text = "Clear Filters",
                Dock = DockStyle.Fill,
                Height = 30
            };
            clearFiltersButton.Click += (s, e) => ClearFilters();
            filterLayout.Controls.Add(clearFiltersButton, 3, 1);

            _advancedFilterPanel.Controls.Add(filterLayout);

            // Grid
            _grid = new GridControl { Dock = DockStyle.Fill };
            _gridView = new GridView(_grid)
            {
                OptionsBehavior = { ReadOnly = true },
                OptionsSelection = { MultiSelect = false },
                OptionsView = { ShowGroupPanel = false, EnableAppearanceEvenRow = true, EnableAppearanceOddRow = true },
                GridControl = _grid
            };
            _grid.MainView = _gridView;
            _grid.ViewCollection.Add(_gridView);

            _gridView.FocusedRowChanged += OnFocusedRowChanged;
            _gridView.DoubleClick += async (s, e) => await OnEditAsync();

            // Status bar
            _statusLabel = new LabelControl
            {
                Text = "Ready",
                Dock = DockStyle.Bottom,
                Height = 30,
                AutoSizeMode = LabelAutoSizeMode.None,
                Padding = new Padding(10, 0, 0, 0)
            };

            // Layout
            var mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 4,
                Padding = new Padding(10)
            };
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 0)); // Advanced filter panel (dynamic height)
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));

            mainLayout.Controls.Add(toolbar, 0, 0);
            mainLayout.Controls.Add(_advancedFilterPanel, 0, 1);
            mainLayout.Controls.Add(_grid, 0, 2);
            mainLayout.Controls.Add(_statusLabel, 0, 3);
            Controls.Add(mainLayout);
        }

        protected abstract string GetPageTitle();

        protected abstract void ConfigureColumns(GridView view);

        public virtual async Task LoadAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                SetWaitCursor(true);
                var items = await GetFilteredItemsAsync(cancellationToken);
                _grid.DataSource = items;
                ConfigureColumns(_gridView);
                _gridView.BestFitColumns();
                UpdateStatusBar(items?.Count() ?? 0);
            }
            catch (Exception ex)
            {
                ShowError($"Failed to load data: {ex.Message}");
            }
            finally
            {
                SetWaitCursor(false);
            }
        }

        protected virtual async Task<IEnumerable<T>> GetFilteredItemsAsync(CancellationToken cancellationToken)
        {
            var items = await GetItemsAsync(cancellationToken);
            return ApplyFilters(items);
        }

        protected abstract Task<IEnumerable<T>> GetItemsAsync(CancellationToken cancellationToken);

        protected virtual IEnumerable<T> ApplyFilters(IEnumerable<T> items)
        {
            if (items == null) return Enumerable.Empty<T>();

            var filtered = items.AsQueryable();

            // Apply search text filter
            if (!string.IsNullOrWhiteSpace(_searchText))
            {
                filtered = ApplySearchFilter(filtered, _searchText);
            }

            // Apply date range filter
            if (_filterDateFrom.HasValue)
            {
                filtered = ApplyDateFromFilter(filtered, _filterDateFrom.Value);
            }
            if (_filterDateTo.HasValue)
            {
                filtered = ApplyDateToFilter(filtered, _filterDateTo.Value);
            }

            // Apply status filter
            if (!string.IsNullOrEmpty(_filterStatus))
            {
                filtered = ApplyStatusFilter(filtered, _filterStatus);
            }

            return filtered.ToList();
        }

        protected virtual IQueryable<T> ApplySearchFilter(IQueryable<T> query, string searchText)
        {
            // Override in derived class for entity-specific search
            return query;
        }

        protected virtual IQueryable<T> ApplyDateFromFilter(IQueryable<T> query, DateTime dateFrom)
        {
            // Override in derived class for entity-specific date filtering
            return query;
        }

        protected virtual IQueryable<T> ApplyDateToFilter(IQueryable<T> query, DateTime dateTo)
        {
            // Override in derived class for entity-specific date filtering
            return query;
        }

        protected virtual IQueryable<T> ApplyStatusFilter(IQueryable<T> query, string status)
        {
            // Override in derived class for entity-specific status filtering
            return query;
        }

        protected virtual async Task FilterAsync()
        {
            await LoadAsync();
        }

        protected virtual async Task OnNewAsync()
        {
            // Override in derived class
        }

        protected virtual async Task OnEditAsync()
        {
            if (_gridView.FocusedRowHandle >= 0)
            {
                var entity = _gridView.GetRow(_gridView.FocusedRowHandle) as T;
                if (entity != null)
                {
                    await OnEditAsync(entity);
                }
            }
        }

        protected abstract Task OnEditAsync(T entity);

        protected virtual async Task OnDeleteAsync()
        {
            if (_gridView.FocusedRowHandle >= 0)
            {
                var entity = _gridView.GetRow(_gridView.FocusedRowHandle) as T;
                if (entity != null)
                {
                    if (ShowConfirm($"Delete {GetEntityName()} '{entity.Id}'?"))
                    {
                        await RunAsync(async ct =>
                        {
                            await DeleteAsync(entity.Id, ct);
                            await LoadAsync();
                        });
                    }
                }
            }
        }

        protected abstract Task DeleteAsync(int id, CancellationToken cancellationToken);

        protected virtual async Task ExportCsvAsync()
        {
            if (_gridView.DataRowCount == 0)
            {
                ShowInfo("No data to export.");
                return;
            }

            using var dialog = new SaveFileDialog
            {
                Filter = "CSV Files (*.csv)|*.csv",
                FileName = $"{GetEntityName()}_{DateTime.Now:yyyyMMdd}.csv"
            };

            if (dialog.ShowDialog(this) != DialogResult.OK) return;

            try
            {
                SetWaitCursor(true);
                var lines = new List<string>();
                var columns = _gridView.VisibleColumns.Where(c => c.Visible).ToList();

                // Header
                lines.Add(string.Join(",", columns.Select(c => $"\"{c.Caption}\"")));

                // Data rows
                for (int i = 0; i < _gridView.DataRowCount; i++)
                {
                    var row = _gridView.GetDataRow(i);
                    if (row == null) continue;

                    var values = columns.Select(c =>
                    {
                        var val = _gridView.GetRowCellValue(i, c.FieldName);
                        var str = val?.ToString() ?? string.Empty;
                        return $"\"{str.Replace("\"", "\"\"")}\"";
                    });
                    lines.Add(string.Join(",", values));
                }

                await File.WriteAllLinesAsync(dialog.FileName, lines);
                ShowInfo($"Exported to {dialog.FileName}");
            }
            catch (Exception ex)
            {
                ShowError($"Export failed: {ex.Message}");
            }
            finally
            {
                SetWaitCursor(false);
            }
        }

        protected virtual void OnFocusedRowChanged(object sender, DevExpress.XtraGrid.Views.Base.FocusedRowChangedEventArgs e)
        {
            var hasRow = _gridView.FocusedRowHandle >= 0;
            _editButton.Enabled = hasRow;
            _deleteButton.Enabled = hasRow;
        }

        protected virtual string GetEntityName() => typeof(T).Name;

        protected virtual void UpdateStatusBar(int count = 0)
        {
            _statusLabel.Text = $"Showing {count} record(s)";
        }

        // Advanced Filter Methods
        protected virtual void ToggleAdvancedFilter()
        {
            _isAdvancedFilterExpanded = !_isAdvancedFilterExpanded;
            _advancedFilterPanel.Visible = _isAdvancedFilterExpanded;
            
            // Find the TableLayoutPanel row for advanced filter panel
            var mainLayout = Controls.OfType<TableLayoutPanel>().FirstOrDefault();
            if (mainLayout != null && mainLayout.RowCount > 1)
            {
                if (_isAdvancedFilterExpanded)
                {
                    mainLayout.RowStyles[1] = new RowStyle(SizeType.Absolute, 100);
                    _advancedFilterButton.Text = "▲ Filters";
                }
                else
                {
                    mainLayout.RowStyles[1] = new RowStyle(SizeType.Absolute, 0);
                    _advancedFilterButton.Text = "▼ Filters";
                }
            }
        }

        protected virtual void ClearFilters()
        {
            _searchText = string.Empty;
            _searchBox.Text = string.Empty;
            
            _filterDateFrom = null;
            _dateFromEdit.EditValue = null;
            
            _filterDateTo = null;
            _dateToEdit.EditValue = null;
            
            _filterStatus = string.Empty;
            _statusFilter.SelectedIndex = 0;

            _ = LoadAsync();
        }
    }
}