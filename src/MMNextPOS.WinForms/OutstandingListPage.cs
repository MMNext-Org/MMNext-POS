using System;
using System.Collections.Generic;
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
    /// List page for Outstanding entries (both Customer and Supplier).
    /// </summary>
    public partial class OutstandingListPage : AsyncFormBase
    {
        private readonly IOutstandingService _outstandingService;
        private readonly IServiceProvider _serviceProvider;
        private readonly ISupplierService _supplierService;
        private readonly ICustomerService _customerService;

        private GridControl _grid = null!;
        private GridView _gridView = null!;
        private ComboBoxEdit _typeFilter = null!;
        private SimpleButton _refreshButton = null!;
        private SimpleButton _newSupplierButton = null!;
        private SimpleButton _newCustomerButton = null!;
        private SimpleButton _editButton = null!;
        private SimpleButton _deleteButton = null!;
        private LabelControl _statusLabel = null!;

        public OutstandingListPage(
            IOutstandingService outstandingService,
            IServiceProvider serviceProvider,
            ISupplierService supplierService,
            ICustomerService customerService)
        {
            _outstandingService = outstandingService ?? throw new ArgumentNullException(nameof(outstandingService));
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _supplierService = supplierService ?? throw new ArgumentNullException(nameof(supplierService));
            _customerService = customerService ?? throw new ArgumentNullException(nameof(customerService));

            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Outstanding Management";
            this.Size = new Size(1000, 700);
            this.MinimumSize = new Size(800, 500);
            this.StartPosition = FormStartPosition.CenterParent;

            // Toolbar controls
            _typeFilter = new ComboBoxEdit
            {
                Location = new Point(70, 12),
                Width = 180,
                Properties = { TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor }
            };
            _typeFilter.Properties.Items.AddRange(new[] { "All", "Supplier Outstanding", "Customer Outstanding" });
            _typeFilter.SelectedIndex = 0;
            _typeFilter.EditValueChanged += async (s, e) => await LoadAsync();

            _refreshButton = new SimpleButton
            {
                Text = "Refresh",
                Location = new Point(260, 10),
                Width = 100,
                Height = 30
            };
            _refreshButton.Click += async (s, e) => await LoadAsync();

            _newSupplierButton = new SimpleButton
            {
                Text = "New Supplier Outstanding",
                Location = new Point(370, 10),
                Width = 180,
                Height = 30
            };
            _newSupplierButton.Click += async (s, e) => await OpenNewSupplierOutstandingAsync();

            _newCustomerButton = new SimpleButton
            {
                Text = "New Customer Outstanding",
                Location = new Point(560, 10),
                Width = 180,
                Height = 30
            };
            _newCustomerButton.Click += async (s, e) => await OpenNewCustomerOutstandingAsync();

            _editButton = new SimpleButton
            {
                Text = "Edit",
                Location = new Point(750, 10),
                Width = 100,
                Height = 30,
                Enabled = false
            };
            _editButton.Click += async (s, e) => await OnEditAsync();

            _deleteButton = new SimpleButton
            {
                Text = "Delete",
                Location = new Point(860, 10),
                Width = 100,
                Height = 30,
                Enabled = false
            };
            _deleteButton.Click += async (s, e) => await OnDeleteAsync();

            // Toolbar
            var toolbar = new PanelControl
            {
                Dock = DockStyle.Top,
                Height = 50,
                BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder
            };

            toolbar.Controls.Add(new LabelControl { Text = "Type:", Location = new Point(10, 15), AutoSizeMode = LabelAutoSizeMode.None, Size = new Size(60, 25) });
            toolbar.Controls.Add(_typeFilter);
            toolbar.Controls.Add(_refreshButton);
            toolbar.Controls.Add(_newSupplierButton);
            toolbar.Controls.Add(_newCustomerButton);
            toolbar.Controls.Add(_editButton);
            toolbar.Controls.Add(_deleteButton);

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

            ConfigureColumns();

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
                RowCount = 3,
                Padding = new Padding(10)
            };
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));

            mainLayout.Controls.Add(toolbar, 0, 0);
            mainLayout.Controls.Add(_grid, 0, 1);
            mainLayout.Controls.Add(_statusLabel, 0, 2);
            Controls.Add(mainLayout);
        }

        private void ConfigureColumns()
        {
            _gridView.Columns.Clear();
            _gridView.Columns.AddRange(new[]
            {
                new GridColumn { FieldName = "Id", Caption = "ID", Width = 60, Visible = true },
                new GridColumn { FieldName = "Type", Caption = "Type", Width = 120, Visible = true },
                new GridColumn { FieldName = "PartyName", Caption = "Supplier/Customer", Width = 200, Visible = true },
                new GridColumn { FieldName = "TransactionDate", Caption = "Date", Width = 120, Visible = true, DisplayFormat = { FormatString = "g", FormatType = DevExpress.Utils.FormatType.DateTime } },
                new GridColumn { FieldName = "DebitAmount", Caption = "Debit", Width = 100, Visible = true, DisplayFormat = { FormatString = "c2", FormatType = DevExpress.Utils.FormatType.Numeric } },
                new GridColumn { FieldName = "CreditAmount", Caption = "Credit", Width = 100, Visible = true, DisplayFormat = { FormatString = "c2", FormatType = DevExpress.Utils.FormatType.Numeric } },
                new GridColumn { FieldName = "Balance", Caption = "Balance", Width = 100, Visible = true, DisplayFormat = { FormatString = "c2", FormatType = DevExpress.Utils.FormatType.Numeric } },
                new GridColumn { FieldName = "Status", Caption = "Status", Width = 80, Visible = true },
                new GridColumn { FieldName = "Description", Caption = "Description", Width = 200, Visible = true }
            });
        }

        private async Task LoadAsync()
        {
            try
            {
                SetWaitCursor(true);
                var items = await GetItemsAsync();
                _grid.DataSource = items;
                _gridView.BestFitColumns();
                UpdateStatusBar(items.Count);
            }
            catch (Exception ex)
            {
                ShowError($"Failed to load outstanding: {ex.Message}");
            }
            finally
            {
                SetWaitCursor(false);
            }
        }

        private async Task<List<OutstandingDisplayItem>> GetItemsAsync()
        {
            var result = new List<OutstandingDisplayItem>();
            var filter = _typeFilter.Text;

            if (filter != "Customer Outstanding")
            {
                var supplierOutstanding = await _outstandingService.GetAllSupplierOutstandingAsync(CancellationToken);
                var suppliers = await _supplierService.GetAllAsync(CancellationToken);
                var supplierDict = suppliers.ToDictionary(s => s.Id, s => s.Name);

                foreach (var o in supplierOutstanding)
                {
                    result.Add(new OutstandingDisplayItem
                    {
                        Id = o.Id,
                        Type = "Supplier",
                        PartyId = o.SupplierId,
                        PartyName = supplierDict.TryGetValue(o.SupplierId, out var name) ? name : $"Supplier {o.SupplierId}",
                        TransactionDate = o.TransactionDate,
                        DebitAmount = o.DebitAmount,
                        CreditAmount = o.CreditAmount,
                        Balance = o.Balance,
                        Status = o.Status,
                        Description = o.Description
                    });
                }
            }

            if (filter != "Supplier Outstanding")
            {
                var customerOutstanding = await _outstandingService.GetAllCustomerOutstandingAsync(CancellationToken);
                var customers = await _customerService.GetAllAsync(CancellationToken);
                var customerDict = customers.ToDictionary(c => c.Id, c => c.Name);

                foreach (var o in customerOutstanding)
                {
                    result.Add(new OutstandingDisplayItem
                    {
                        Id = o.Id,
                        Type = "Customer",
                        PartyId = o.CustomerId,
                        PartyName = customerDict.TryGetValue(o.CustomerId, out var name) ? name : $"Customer {o.CustomerId}",
                        TransactionDate = o.TransactionDate,
                        DebitAmount = o.DebitAmount,
                        CreditAmount = o.CreditAmount,
                        Balance = o.Balance,
                        Status = o.Status,
                        Description = o.Description
                    });
                }
            }

            // Sort by date descending
            result.Sort((a, b) => b.TransactionDate.CompareTo(a.TransactionDate));

            return result;
        }

        private void UpdateStatusBar(int count)
        {
            _statusLabel.Text = $"{count} outstanding entries";
        }

        private void OnFocusedRowChanged(object sender, DevExpress.XtraGrid.Views.Base.FocusedRowChangedEventArgs e)
        {
            bool hasRow = _gridView.FocusedRowHandle >= 0;
            _editButton.Enabled = hasRow;
            _deleteButton.Enabled = hasRow;
        }

        private async Task OnEditAsync()
        {
            if (_gridView.FocusedRowHandle >= 0)
            {
                var item = _gridView.GetRow(_gridView.FocusedRowHandle) as OutstandingDisplayItem;
                if (item != null)
                {
                    if (item.Type == "Supplier")
                    {
                        var list = await _outstandingService.GetSupplierOutstandingAsync(item.PartyId, CancellationToken);
                        var outstanding = list.FirstOrDefault(o => o.Id == item.Id);
                        if (outstanding != null)
                        {
                            using var dialog = new OutstandingForm(_outstandingService, _supplierService, outstanding, true);
                            if (dialog.ShowDialog(this) == DialogResult.OK)
                            {
                                await LoadAsync();
                            }
                        }
                    }
                    else
                    {
                        var list = await _outstandingService.GetCustomerOutstandingAsync(item.PartyId, CancellationToken);
                        var outstanding = list.FirstOrDefault(o => o.Id == item.Id);
                        if (outstanding != null)
                        {
                            using var dialog = new OutstandingForm(_outstandingService, _customerService, outstanding, false);
                            if (dialog.ShowDialog(this) == DialogResult.OK)
                            {
                                await LoadAsync();
                            }
                        }
                    }
                }
            }
        }

        private async Task OnDeleteAsync()
        {
            if (_gridView.FocusedRowHandle >= 0)
            {
                var item = _gridView.GetRow(_gridView.FocusedRowHandle) as OutstandingDisplayItem;
                if (item != null && ShowConfirm($"Delete outstanding entry {item.Id}?"))
                {
                    await RunAsync(async ct =>
                    {
                        if (item.Type == "Supplier")
                        {
                            await _outstandingService.DeleteSupplierOutstandingAsync(item.Id, ct);
                        }
                        else
                        {
                            await _outstandingService.DeleteCustomerOutstandingAsync(item.Id, ct);
                        }
                        await LoadAsync();
                    });
                }
            }
        }

        private async Task OpenNewSupplierOutstandingAsync()
        {
            using var dialog = new OutstandingForm(_outstandingService, _supplierService, true);
            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
                await LoadAsync();
            }
        }

        private async Task OpenNewCustomerOutstandingAsync()
        {
            using var dialog = new OutstandingForm(_outstandingService, _customerService, false);
            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
                await LoadAsync();
            }
        }
    }

    /// <summary>
    /// Display item for outstanding entries in the grid.
    /// </summary>
    public class OutstandingDisplayItem
    {
        public int Id { get; set; }
        public string Type { get; set; } = ""; // "Supplier" or "Customer"
        public int PartyId { get; set; }
        public string PartyName { get; set; } = "";
        public DateTime TransactionDate { get; set; }
        public decimal DebitAmount { get; set; }
        public decimal CreditAmount { get; set; }
        public decimal Balance { get; set; }
        public string Status { get; set; } = "";
        public string? Description { get; set; }
    }
}
