using System;
using System.Collections.Generic;
using System.IO;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Extensions.DependencyInjection;
using DevExpress.XtraBars.FluentDesignSystem;
using DevExpress.XtraBars.Navigation;
using DevExpress.XtraEditors;
using DevExpress.XtraGrid.Views.Grid;
using MMNextPOS.Application.Services;
using MMNextPOS.Domain.Models;

namespace MMNextPOS.WinForms
{
    /// <summary>
    /// Main application shell using DevExpress FluentDesignForm.
    /// Provides a modern navigation sidebar and content area.
    /// </summary>
    public partial class MainForm : AsyncFormBase
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ISalesService _salesService;
        private readonly IProductService _productService;
        private readonly ICustomerService _customerService;

        // Controls
        private FluentDesignFormContainer _mainContainer = null!;
        private DevExpress.XtraGrid.GridControl _salesGrid = null!;
        private GridView _salesGridView = null!;
        private SimpleButton _refreshButton = null!;
        private SimpleButton _newSaleButton = null!;
        private SimpleButton _exportCsvButton = null!;
        private TextEdit _searchBox = null!;
        private ContextMenuStrip _salesContextMenu = null!;

        // Cached lists for client-side filtering & grids
        private List<Sale> _allSales = new();
        private List<Product> _allProducts = new();
        private List<Customer> _allCustomers = new();

        // Product and customer grids
        private DevExpress.XtraGrid.GridControl _productGrid = null!;
        private GridView _productView = null!;
        private DevExpress.XtraGrid.GridControl _customerGrid = null!;
        private GridView _customerView = null!;

        // Disposal tracking
        private bool _disposed;

        public MainForm(IServiceProvider serviceProvider, ISalesService salesService, IProductService productService, ICustomerService customerService)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _salesService = salesService ?? throw new ArgumentNullException(nameof(salesService));
            _productService = productService ?? throw new ArgumentNullException(nameof(productService));
            _customerService = customerService ?? throw new ArgumentNullException(nameof(customerService));

            InitializeComponent();
            InitializeServices();
        }

        private void InitializeComponent()
        {
            // Form setup
            this.Text = "MMNext POS";
            this.Size = new System.Drawing.Size(1200, 800);
            this.MinimumSize = new System.Drawing.Size(1000, 700);

            // Create navigation elements
            var navigationPane = new NavigationPane();
            navigationPane.Dock = DockStyle.Left;
            navigationPane.Width = 280;

            // Sales page
            var salesPage = new NavigationPage { Caption = "Sales" };
            salesPage.Controls.Add(CreateSalesContent());
            navigationPane.Pages.Add(salesPage);

            // Products page
            var productsPage = new NavigationPage { Caption = "Products" };
            productsPage.Controls.Add(CreateProductsContent());
            navigationPane.Pages.Add(productsPage);

            // Customers page
            var customersPage = new NavigationPage { Caption = "Customers" };
            customersPage.Controls.Add(CreateCustomersContent());
            navigationPane.Pages.Add(customersPage);

            // Reports page
            var reportsPage = new NavigationPage { Caption = "Reports" };
            reportsPage.Controls.Add(CreateReportsContent());
            navigationPane.Pages.Add(reportsPage);

            this.Controls.Add(navigationPane);
        }

        private Control CreateSalesContent()
        {
            _mainContainer = new FluentDesignFormContainer
            {
                Dock = DockStyle.Fill
            };

            // Toolbar panel
            var toolbarPanel = new PanelControl
            {
                Dock = DockStyle.Top,
                Height = 50,
                BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder
            };

            _refreshButton = new SimpleButton
            {
                Text = "Refresh",
                Location = new System.Drawing.Point(10, 10),
                Width = 100,
                Height = 30
            };
            _refreshButton.Click += async (s, e) => await LoadRecentSalesAsync();

            _exportCsvButton = new SimpleButton
            {
                Text = "Export CSV",
                Location = new System.Drawing.Point(120, 10),
                Width = 100,
                Height = 30
            };
            _exportCsvButton.Click += async (s, e) => await ExportSalesToCsvAsync();

            _searchBox = new TextEdit
            {
                Location = new System.Drawing.Point(230, 13),
                Width = 200,
                Properties = { NullValuePrompt = "Search customer..." }
            };
            _searchBox.EditValueChanged += async (s, e) => await FilterSalesAsync();

            _newSaleButton = new SimpleButton
            {
                Text = "New Sale",
                Location = new System.Drawing.Point(440, 10),
                Width = 100,
                Height = 30
            };
            _newSaleButton.Click += (s, e) => OpenNewSaleDialog();

            toolbarPanel.Controls.Add(_refreshButton);
            toolbarPanel.Controls.Add(_exportCsvButton);
            toolbarPanel.Controls.Add(_searchBox);
            toolbarPanel.Controls.Add(_newSaleButton);

            // Grid
            _salesGrid = new DevExpress.XtraGrid.GridControl
            {
                Dock = DockStyle.Fill
            };

            _salesGridView = new GridView(_salesGrid)
            {
                OptionsBehavior = { ReadOnly = true },
                OptionsSelection = { MultiSelect = false },
                OptionsView = { ShowGroupPanel = false, EnableAppearanceEvenRow = true, EnableAppearanceOddRow = true },
                GridControl = _salesGrid
            };

            _salesGrid.MainView = _salesGridView;
            _salesGrid.ViewCollection.Add(_salesGridView);

            // Configure columns - use simple string fields
            _salesGridView.Columns.Clear();
            _salesGridView.Columns.AddRange(new[]
            {
                new DevExpress.XtraGrid.Columns.GridColumn { FieldName = "Id", Caption = "Sale #", Width = 80, Visible = true },
                new DevExpress.XtraGrid.Columns.GridColumn { FieldName = "CustomerName", Caption = "Customer", Width = 200, Visible = true },
                new DevExpress.XtraGrid.Columns.GridColumn { FieldName = "SaleDate", Caption = "Date", Width = 150, Visible = true },
                new DevExpress.XtraGrid.Columns.GridColumn { FieldName = "TotalAmount", Caption = "Total", Width = 120, Visible = true }
            });

            // Context menu
            _salesContextMenu = new ContextMenuStrip();
            var viewDetailsItem = new ToolStripMenuItem("View Details");
            viewDetailsItem.Click += (s, e) => ViewSelectedSaleDetails();
            var printReceiptItem = new ToolStripMenuItem("Print Receipt");
            printReceiptItem.Click += (s, e) => PrintSelectedReceipt();
            _salesContextMenu.Items.Add(viewDetailsItem);
            _salesContextMenu.Items.Add(printReceiptItem);
            _salesGrid.ContextMenuStrip = _salesContextMenu;

            _salesGridView.BestFitColumns();

            _mainContainer.Controls.Add(_salesGrid);
            _mainContainer.Controls.Add(toolbarPanel);

            return _mainContainer;
        }

        private Control CreateProductsContent()
        {
            var panel = new PanelControl { Dock = DockStyle.Fill, BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder };

            var titleLabel = new LabelControl
            {
                Text = "Product Management",
                Dock = DockStyle.Fill
            };

            var productGrid = new DevExpress.XtraGrid.GridControl { Dock = DockStyle.Fill };
            var productView = new GridView(productGrid)
            {
                OptionsBehavior = { ReadOnly = true },
                OptionsSelection = { MultiSelect = false },
                OptionsView = { ShowGroupPanel = false, EnableAppearanceEvenRow = true, EnableAppearanceOddRow = true }
            };
            productGrid.MainView = productView;
            productGrid.ViewCollection.Add(productView);

            productView.Columns.AddRange(new[]
            {
                new DevExpress.XtraGrid.Columns.GridColumn { FieldName = "Id", Caption = "ID", Width = 60, Visible = true },
                new DevExpress.XtraGrid.Columns.GridColumn { FieldName = "Sku", Caption = "SKU", Width = 150, Visible = true },
                new DevExpress.XtraGrid.Columns.GridColumn { FieldName = "Name", Caption = "Product Name", Width = 250, Visible = true },
                new DevExpress.XtraGrid.Columns.GridColumn { FieldName = "Price", Caption = "Price", Width = 100, Visible = true },
                new DevExpress.XtraGrid.Columns.GridColumn { FieldName = "StockQuantity", Caption = "Stock", Width = 80, Visible = true },
                new DevExpress.XtraGrid.Columns.GridColumn { FieldName = "IsActive", Caption = "Active", Width = 70, Visible = true }
            });

            productGrid.DataSource = _allProducts;

            _productGrid = productGrid;
            _productView = productView;

            panel.Controls.Add(productGrid);
            return panel;
        }

        private Control CreateCustomersContent()
        {
            var panel = new PanelControl { Dock = DockStyle.Fill, BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder };

            var titleLabel = new LabelControl
            {
                Text = "Customer Management",
                Dock = DockStyle.Fill
            };

            var customerGrid = new DevExpress.XtraGrid.GridControl { Dock = DockStyle.Fill };
            var customerView = new GridView(customerGrid)
            {
                OptionsBehavior = { ReadOnly = true },
                OptionsSelection = { MultiSelect = false },
                OptionsView = { ShowGroupPanel = false, EnableAppearanceEvenRow = true, EnableAppearanceOddRow = true }
            };
            customerGrid.MainView = customerView;
            customerGrid.ViewCollection.Add(customerView);

            customerView.Columns.AddRange(new[]
            {
                new DevExpress.XtraGrid.Columns.GridColumn { FieldName = "Id", Caption = "ID", Width = 60, Visible = true },
                new DevExpress.XtraGrid.Columns.GridColumn { FieldName = "Name", Caption = "Customer Name", Width = 250, Visible = true },
                new DevExpress.XtraGrid.Columns.GridColumn { FieldName = "Phone", Caption = "Phone", Width = 120, Visible = true },
                new DevExpress.XtraGrid.Columns.GridColumn { FieldName = "Email", Caption = "Email", Width = 200, Visible = true },
                new DevExpress.XtraGrid.Columns.GridColumn { FieldName = "IsActive", Caption = "Active", Width = 70, Visible = true }
            });

            customerGrid.DataSource = _allCustomers;

            _customerGrid = customerGrid;
            _customerView = customerView;

            panel.Controls.Add(customerGrid);
            return panel;
        }

        private Control CreateReportsContent()
        {
            var panel = new PanelControl { Dock = DockStyle.Fill, BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder };

            var titleLabel = new LabelControl
            {
                Text = "Reports",
                Dock = DockStyle.Fill
            };

            var infoLabel = new LabelControl
            {
                Text = "Sales summary and reports module.\n\n• Recent Sales Report\n• Product Performance\n• Customer Summary\n• Daily/Shift summaries",
                Dock = DockStyle.Fill
            };

            panel.Controls.Add(titleLabel);
            panel.Controls.Add(infoLabel);

            return panel;
        }

        private void InitializeServices()
        {
            // Subscribe to form load event
            this.Load += async (s, e) => await LoadReferenceDataAsync();
        }

        private async Task LoadRecentSalesAsync()
        {
            if (_disposed) return;

            try
            {
                SetWaitCursor(true);
                _refreshButton.Enabled = false;
                _newSaleButton.Enabled = false;

                var sales = await _salesService.GetRecentSalesAsync(50);
                _allSales = sales.ToList();
                ApplyFilter();
            }
            catch (Exception ex)
            {
                ShowError($"Failed to load sales: {ex.Message}");
            }
            finally
            {
                SetWaitCursor(false);
                _refreshButton.Enabled = true;
                _newSaleButton.Enabled = true;
            }
        }

        private async Task LoadReferenceDataAsync()
        {
            try
            {
                SetWaitCursor(true);

                // Load recent sales
                var sales = await _salesService.GetRecentSalesAsync(50);
                _allSales = sales.ToList();
                ApplyFilter();

                // Load products for grid
                var products = await _productService.GetAllAsync(CancellationToken);
                _allProducts = products.ToList();

                // Load customers for grid
                var customers = await _customerService.GetAllAsync(CancellationToken);
                _allCustomers = customers.ToList();

                // Refresh grids
                if (_productGrid != null && _productView != null)
                {
                    _productGrid.DataSource = _allProducts;
                    _productView.BestFitColumns();
                }

                if (_customerGrid != null && _customerView != null)
                {
                    _customerGrid.DataSource = _allCustomers;
                    _customerView.BestFitColumns();
                }
            }
            catch (Exception ex)
            {
                ShowError($"Failed to load reference data: {ex.Message}");
            }
            finally
            {
                SetWaitCursor(false);
            }
        }

        private async Task ExportSalesToCsvAsync()
        {
            if (_disposed) return;
            if (_allSales.Count == 0)
            {
                ShowInfo("No sales data to export.");
                return;
            }

            var saveDialog = new SaveFileDialog
            {
                Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*",
                FileName = $"MMNextPOS_Sales_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
            };

            if (saveDialog.ShowDialog() != DialogResult.OK) return;

            try
            {
                SetWaitCursor(true);
                var sb = new System.Text.StringBuilder();

                // Header
                sb.AppendLine("Sale ID,Customer Name,Date,Total Amount");
                sb.AppendLine($"\"{_allSales[0].Id}\",\"{_allSales[0].CustomerName ?? ""}\",\"{_allSales[0].SaleDate:g}\",\"{_allSales[0].TotalAmount:C2}\"");

                // Data rows
                foreach (var sale in _allSales.Skip(1))
                {
                    sb.AppendLine($"{sale.Id},\"{sale.CustomerName ?? ""}\",{sale.SaleDate:o},{sale.TotalAmount:C2}");
                }

                await File.WriteAllTextAsync(saveDialog.FileName, sb.ToString());
                ShowInfo($"Export completed: {saveDialog.FileName}");
            }
            catch (Exception ex)
            {
                ShowError($"Failed to export sales: {ex.Message}");
            }
            finally
            {
                SetWaitCursor(false);
            }
        }

        private Task FilterSalesAsync()
        {
            ApplyFilter();
            return Task.CompletedTask;
        }

        private void ApplyFilter()
        {
            if (_disposed) return;

            var filter = _searchBox?.EditValue?.ToString()?.Trim();
            IEnumerable<Sale> filtered = _allSales;

            if (!string.IsNullOrEmpty(filter))
            {
                filtered = _allSales.Where(s =>
                    (s.CustomerName != null && s.CustomerName.Contains(filter, StringComparison.OrdinalIgnoreCase)) ||
                    s.Id.ToString().Contains(filter));
            }

            _salesGrid.DataSource = new BindingList<Sale>(filtered.ToList());
            _salesGridView.BestFitColumns();
        }

        private Sale? GetSelectedSale()
        {
            var rowHandle = _salesGridView.FocusedRowHandle;
            if (rowHandle < 0) return null;
            return _salesGridView.GetRow(rowHandle) as Sale;
        }

        private void ViewSelectedSaleDetails()
        {
            var sale = GetSelectedSale();
            if (sale == null)
            {
                ShowInfo("Please select a sale first.");
                return;
            }

            XtraMessageBox.Show(this,
                $"Sale #{sale.Id}\nCustomer: {sale.CustomerName ?? "(unknown)"}\nDate: {sale.SaleDate:g}\nTotal: {sale.TotalAmount:C2}",
                "Sale Details",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        private void PrintSelectedReceipt()
        {
            var sale = GetSelectedSale();
            if (sale == null)
            {
                ShowInfo("Please select a sale first.");
                return;
            }

            XtraMessageBox.Show(this,
                $"Receipt printing for Sale #{sale.Id} will be implemented in a later phase.",
                "Receipt Printing",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.F5)
            {
                _ = LoadRecentSalesAsync();
                return true;
            }
            if (keyData == (Keys.Control | Keys.N))
            {
                OpenNewSaleDialog();
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void OpenNewSaleDialog()
        {
            using var dialog = _serviceProvider.GetRequiredService<NewSaleForm>();
            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
                _ = LoadRecentSalesAsync();
            }
        }

        /// <summary>
        /// Clean up resources.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // Unsubscribe + dispose child controls
                    _salesContextMenu?.Dispose();
                    _searchBox?.Dispose();
                    _refreshButton?.Dispose();
                    _exportCsvButton?.Dispose();
                    _newSaleButton?.Dispose();
                    _salesGrid?.Dispose();
                    _salesGridView?.Dispose();
                    _mainContainer?.Dispose();
                    _productGrid?.Dispose();
                    _productView?.Dispose();
                    _customerGrid?.Dispose();
                    _customerView?.Dispose();
                }
                _disposed = true;
            }
            base.Dispose(disposing);
        }
    }
}