using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using DevExpress.XtraBars.FluentDesignSystem;
using DevExpress.XtraBars.Navigation;
using DevExpress.XtraEditors;
using Microsoft.Extensions.DependencyInjection;
using MMNextPOS.Application.Services;
using MMNextPOS.Domain.Models;
using MMNextPOS.Infrastructure.Repositories;
using MMNextPOS.WinForms.Services;

namespace MMNextPOS.WinForms
{
    /// <summary>
    /// Main application shell using DevExpress FluentDesignForm.
    /// Provides a modern navigation sidebar and content area using ListPage base classes.
    /// </summary>
    public partial class MainForm : AsyncFormBase
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IUserSession _userSession;
        private readonly IMainNavigationService _navigationService;
        private readonly ISalesService _salesService;
        private readonly IProductService _productService;
        private readonly ICustomerService _customerService;
        private readonly ISupplierService _supplierService;
        private readonly IOutstandingService _outstandingService;
        private readonly ISaleTempService _saleTempService;
        private readonly ISalesReturnService _salesReturnService;
        private readonly IPurchaseService _purchaseService;
        private readonly IInventoryService _inventoryService;
        private readonly IStockTransferRepository _stockTransferRepository;
        private readonly IExpenseService _expenseService;
        private readonly IExpenseTypeService _expenseTypeService;
        private readonly IPaymentRepository _paymentRepository;
        private readonly IPurchaseReturnRepository _purchaseReturnRepository;

        // Master data services
        private readonly ICategoryService _categoryService;
        private readonly IUnitService _unitService;
        private readonly IGroupService _groupService;
        private readonly ICurrencyService _currencyService;
        private readonly ITaxService _taxService;
        private readonly IDiscountService _discountService;
        private readonly ILocationService _locationService;
        private readonly ICompanyService _companyService;
        private readonly IUserService _userService;
        private readonly IRoleService _roleService;
        private readonly IUserRoleService _userRoleService;
        private readonly IMenuRoleService _menuRoleService;
        private readonly IReportMenusService _reportMenusService;
        private readonly IEmailSettingService _emailSettingService;
        private readonly IThemeService _themeService;
        private readonly ILanguageService _languageService;
        private readonly WinFormsReportService _reportService;

        // Concrete list page instances
        private ProductsListPage _productsListPage = null!;
        private CustomersListPage _customersListPage = null!;
        private SalesListPage _salesListPage = null!;
        private OutstandingListPage _outstandingListPage = null!;

        // Master data ListPages
        private CategoriesListPage _categoriesListPage = null!;
        private UnitsListPage _unitsListPage = null!;
        private GroupsListPage _groupsListPage = null!;
        private CurrenciesListPage _currenciesListPage = null!;
        private TaxesListPage _taxesListPage = null!;
        private DiscountsListPage _discountsListPage = null!;
        private LocationsListPage _locationsListPage = null!;
        private CompaniesListPage _companiesListPage = null!;
        private UsersListPage _usersListPage = null!;
        private RolesListPage _rolesListPage = null!;
        private ReportMenusListPage _reportMenusListPage = null!;
        private EmailSettingsListPage _emailSettingsListPage = null!;
        private SuppliersListPage _suppliersListPage = null!;
        private ThemesListPage _themesListPage = null!;
        private LanguagesListPage _languagesListPage = null!;

        // Transaction ListPages
        private SaleTempsListPage _saleTempsListPage = null!;
        private SalesReturnsListPage _salesReturnsListPage = null!;
        private PurchasesListPage _purchasesListPage = null!;
        private PurchaseReturnsListPage _purchaseReturnsListPage = null!;
        private StockMovementsListPage _stockMovementsListPage = null!;
        private AssembliesListPage _assembliesListPage = null!;
        private StockTransfersListPage _stockTransfersListPage = null!;
        private ExpensesListPage _expensesListPage = null!;
        private ExpenseTypesListPage _expenseTypesListPage = null!;
        private PaymentsListPage _paymentsListPage = null!;

        private ReportsViewerForm _reportsViewerForm = null!;

        // Navigation
        private FluentDesignFormContainer _mainContainer = null!;
        private NavigationPane _navigationPane = null!;
        private LabelControl _userInfoLabel = null!;

public MainForm(
            IServiceProvider serviceProvider,
            IUserSession userSession,
            IMainNavigationService navigationService,
            ISalesService salesService,
            IProductService productService,
            ICustomerService customerService,
            ISupplierService supplierService,
            IOutstandingService outstandingService,
            ISaleTempService saleTempService,
            ISalesReturnService salesReturnService,
            IPurchaseService purchaseService,
            IInventoryService inventoryService,
            IStockTransferRepository stockTransferRepository,
            IExpenseService expenseService,
            IExpenseTypeService expenseTypeService,
            IPaymentRepository paymentRepository,
            IPurchaseReturnRepository purchaseReturnRepository,
            ICategoryService categoryService,
            IUnitService unitService,
            IGroupService groupService,
            ICurrencyService currencyService,
            ITaxService taxService,
            IDiscountService discountService,
            ILocationService locationService,
            ICompanyService companyService,
            IUserService userService,
            IRoleService roleService,
            IUserRoleService userRoleService,
            IMenuRoleService menuRoleService,
            IReportMenusService reportMenusService,
            IEmailSettingService emailSettingService,
            IThemeService themeService,
            ILanguageService languageService,
            WinFormsReportService reportService)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _userSession = userSession ?? throw new ArgumentNullException(nameof(userSession));
            _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
            _salesService = salesService ?? throw new ArgumentNullException(nameof(salesService));
            _productService = productService ?? throw new ArgumentNullException(nameof(productService));
            _customerService = customerService ?? throw new ArgumentNullException(nameof(customerService));
            _supplierService = supplierService ?? throw new ArgumentNullException(nameof(supplierService));
            _outstandingService = outstandingService ?? throw new ArgumentNullException(nameof(outstandingService));
            _saleTempService = saleTempService ?? throw new ArgumentNullException(nameof(saleTempService));
            _salesReturnService = salesReturnService ?? throw new ArgumentNullException(nameof(salesReturnService));
            _purchaseService = purchaseService ?? throw new ArgumentNullException(nameof(purchaseService));
            _inventoryService = inventoryService ?? throw new ArgumentNullException(nameof(inventoryService));
            _stockTransferRepository = stockTransferRepository ?? throw new ArgumentNullException(nameof(stockTransferRepository));
            _expenseService = expenseService ?? throw new ArgumentNullException(nameof(expenseService));
            _expenseTypeService = expenseTypeService ?? throw new ArgumentNullException(nameof(expenseTypeService));
            _paymentRepository = paymentRepository ?? throw new ArgumentNullException(nameof(paymentRepository));
            _purchaseReturnRepository = purchaseReturnRepository ?? throw new ArgumentNullException(nameof(purchaseReturnRepository));

            _categoryService = categoryService ?? throw new ArgumentNullException(nameof(categoryService));
            _unitService = unitService ?? throw new ArgumentNullException(nameof(unitService));
            _groupService = groupService ?? throw new ArgumentNullException(nameof(groupService));
            _currencyService = currencyService ?? throw new ArgumentNullException(nameof(currencyService));
            _taxService = taxService ?? throw new ArgumentNullException(nameof(taxService));
            _discountService = discountService ?? throw new ArgumentNullException(nameof(discountService));
            _locationService = locationService ?? throw new ArgumentNullException(nameof(locationService));
            _companyService = companyService ?? throw new ArgumentNullException(nameof(companyService));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _roleService = roleService ?? throw new ArgumentNullException(nameof(roleService));
            _userRoleService = userRoleService ?? throw new ArgumentNullException(nameof(userRoleService));
            _menuRoleService = menuRoleService ?? throw new ArgumentNullException(nameof(menuRoleService));
            _reportMenusService = reportMenusService ?? throw new ArgumentNullException(nameof(reportMenusService));
            _emailSettingService = emailSettingService ?? throw new ArgumentNullException(nameof(emailSettingService));
            _themeService = themeService ?? throw new ArgumentNullException(nameof(themeService));
            _languageService = languageService ?? throw new ArgumentNullException(nameof(languageService));
            _reportService = reportService ?? throw new ArgumentNullException(nameof(reportService));

            InitializeComponent();
            InitializeListPages();
            InitializeNavigation();
            UpdateUserInfo();
            ApplyUserPreferences();
        }

        private void InitializeComponent()
        {
            // Form setup
            this.Text = "MMNext POS";
            this.Size = new System.Drawing.Size(1200, 800);
            this.MinimumSize = new System.Drawing.Size(1000, 700);
            this.StartPosition = FormStartPosition.CenterScreen;

            // Create navigation elements
            _navigationPane = new NavigationPane();
            _navigationPane.Dock = DockStyle.Left;
            _navigationPane.Width = 280;

            // User info panel at top of navigation
            var userInfoPanel = new PanelControl
            {
                Dock = DockStyle.Top,
                Height = 100,
                BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder,
                Padding = new Padding(10, 5, 10, 5)
            };

            var userInfoLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 2,
                Padding = new Padding(0),
                Margin = new Padding(0)
            };
            userInfoLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            userInfoLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 90F));
            userInfoLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            userInfoLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));

            _userInfoLabel = new LabelControl
            {
                Dock = DockStyle.Fill,
                Text = "Loading...",
                Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point),
                Appearance = { TextOptions = { HAlignment = DevExpress.Utils.HorzAlignment.Near }, ForeColor = System.Drawing.Color.FromArgb(80, 80, 80) },
                AutoSizeMode = LabelAutoSizeMode.None
            };
            userInfoLayout.Controls.Add(_userInfoLabel, 0, 0);
            userInfoLayout.SetColumnSpan(_userInfoLabel, 2);

            // Change Password button
            var changePasswordButton = new SimpleButton
            {
                Text = "Change Password",
                Dock = DockStyle.Fill,
                Height = 30,
                Font = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point),
                Appearance = { BackColor = System.Drawing.Color.FromArgb(0, 122, 204), ForeColor = System.Drawing.Color.White },
                ButtonStyle = DevExpress.XtraEditors.Controls.BorderStyles.Simple
            };
            changePasswordButton.Click += OnChangePasswordClick;
            userInfoLayout.Controls.Add(changePasswordButton, 0, 1);

            // Logout button
            var logoutButton = new SimpleButton
            {
                Text = "Logout",
                Dock = DockStyle.Fill,
                Height = 30,
                Font = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point),
                Appearance = { BackColor = System.Drawing.Color.FromArgb(200, 80, 80), ForeColor = System.Drawing.Color.White },
                ButtonStyle = DevExpress.XtraEditors.Controls.BorderStyles.Simple
            };
            logoutButton.Click += OnLogoutClick;
            userInfoLayout.Controls.Add(logoutButton, 1, 1);

            userInfoPanel.Controls.Add(userInfoLayout);
            _navigationPane.Controls.Add(userInfoPanel);

            // Main content container
            _mainContainer = new FluentDesignFormContainer
            {
                Dock = DockStyle.Fill
            };
            this.Controls.Add(_mainContainer);
            this.Controls.Add(_navigationPane);
        }

        private void InitializeListPages()
        {
            // Core modules
            _productsListPage = new ProductsListPage(_productService, _serviceProvider);
            _customersListPage = new CustomersListPage(_customerService, _serviceProvider);
            _salesListPage = new SalesListPage(_salesService, _serviceProvider);
            _outstandingListPage = new OutstandingListPage(_outstandingService, _serviceProvider, _supplierService, _customerService);

            // Master data
            _categoriesListPage = new CategoriesListPage(_categoryService, _serviceProvider);
            _unitsListPage = new UnitsListPage(_unitService, _serviceProvider);
            _groupsListPage = new GroupsListPage(_groupService, _serviceProvider);
            _currenciesListPage = new CurrenciesListPage(_currencyService, _serviceProvider);
            _taxesListPage = new TaxesListPage(_taxService, _serviceProvider);
            _discountsListPage = new DiscountsListPage(_discountService, _serviceProvider);
            _locationsListPage = new LocationsListPage(_locationService, _serviceProvider);
            _companiesListPage = new CompaniesListPage(_companyService, _serviceProvider);
            _usersListPage = new UsersListPage(_userService, _serviceProvider);
            _rolesListPage = new RolesListPage(_roleService, _serviceProvider);
            _reportMenusListPage = new ReportMenusListPage(_reportMenusService, _serviceProvider);
            _emailSettingsListPage = new EmailSettingsListPage(_emailSettingService, _serviceProvider);
            _suppliersListPage = new SuppliersListPage(_supplierService, _serviceProvider);
            _themesListPage = new ThemesListPage(_themeService, _serviceProvider);
            _languagesListPage = new LanguagesListPage(_languageService, _serviceProvider);

            // Transaction ListPages
            _saleTempsListPage = new SaleTempsListPage(_saleTempService, _serviceProvider);
            _salesReturnsListPage = new SalesReturnsListPage(_salesReturnService, _serviceProvider);
            _purchasesListPage = new PurchasesListPage(_purchaseService, _serviceProvider);
            _purchaseReturnsListPage = new PurchaseReturnsListPage(_purchaseReturnRepository, _serviceProvider);
            _stockMovementsListPage = new StockMovementsListPage(_inventoryService, _serviceProvider);
            _assembliesListPage = new AssembliesListPage(_inventoryService, _serviceProvider);
            _stockTransfersListPage = new StockTransfersListPage(_stockTransferRepository, _serviceProvider);
            _expensesListPage = new ExpensesListPage(_expenseService, _serviceProvider);
            _expenseTypesListPage = new ExpenseTypesListPage(_expenseTypeService, _serviceProvider);
            _paymentsListPage = new PaymentsListPage(_paymentRepository, _serviceProvider);

            // Reports
            _reportsViewerForm = new ReportsViewerForm(_reportService, _serviceProvider);
        }

        private void InitializeNavigation()
        {
            if (!_userSession.IsAuthenticated)
                return;

            // Get navigation pages based on user roles
            var roleCodes = _userSession.Roles.Select(r => r.Code).ToList();
            var isAdmin = roleCodes.Any(rc => string.Equals(rc, "Admin", StringComparison.OrdinalIgnoreCase));
            var isManager = roleCodes.Any(rc => string.Equals(rc, "Manager", StringComparison.OrdinalIgnoreCase));
            var isCashier = roleCodes.Any(rc => string.Equals(rc, "Cashier", StringComparison.OrdinalIgnoreCase));
            var isWarehouse = roleCodes.Any(rc => string.Equals(rc, "Warehouse", StringComparison.OrdinalIgnoreCase));

            // ============ CORE MODULES ============
            // Products - visible to all roles
            AddNavigationPage("Products", _productsListPage);

            // Customers - visible to all roles
            AddNavigationPage("Customers", _customersListPage);

            // Sales - visible to Admin, Manager, Cashier
            if (isAdmin || isManager || isCashier)
            {
                var salesPage = new NavigationPage { Caption = "Sales" };
                var newSaleButton = new SimpleButton
                {
                    Text = "New Sale",
                    Location = new Point(10, 10),
                    Width = 100,
                    Height = 35
                };
                newSaleButton.Click += (s, e) => OpenNewSaleDialog();
                _salesListPage.Controls.Add(newSaleButton);

                var holdSaleButton = new SimpleButton
                {
                    Text = "Hold Sale",
                    Location = new Point(120, 10),
                    Width = 100,
                    Height = 35
                };
                holdSaleButton.Click += (s, e) => OpenHoldSaleDialog();
                _salesListPage.Controls.Add(holdSaleButton);

                salesPage.Controls.Add(_salesListPage);
                _navigationPane.Pages.Add(salesPage);
            }

            // Outstanding - visible to Admin, Manager
            if (isAdmin || isManager)
            {
                AddNavigationPage("Outstanding", _outstandingListPage);
            }

            // ============ MASTER DATA ============
            // Master data - visible to Admin, Manager
            if (isAdmin || isManager)
            {
                AddNavigationPage("Categories", _categoriesListPage);
                AddNavigationPage("Units", _unitsListPage);
                AddNavigationPage("Groups", _groupsListPage);
                AddNavigationPage("Currencies", _currenciesListPage);
                AddNavigationPage("Taxes", _taxesListPage);
                AddNavigationPage("Discounts", _discountsListPage);
                AddNavigationPage("Locations", _locationsListPage);
                AddNavigationPage("Companies", _companiesListPage);
                AddNavigationPage("Suppliers", _suppliersListPage);
            }

            // ============ ADMINISTRATION ============
            // Administration - visible only to Admin
            if (isAdmin)
            {
                AddNavigationPage("Users", _usersListPage);
                AddNavigationPage("Roles", _rolesListPage);
                AddNavigationPage("Themes", _themesListPage);
                AddNavigationPage("Languages", _languagesListPage);
                AddNavigationPage("Report Menus", _reportMenusListPage);
                AddNavigationPage("Email Settings", _emailSettingsListPage);
            }

            // ============ REPORTS ============
            // Reports - visible to Admin, Manager
            if (isAdmin || isManager)
            {
                AddNavigationPage("Reports", _reportsViewerForm);
            }

            // ============ TRANSACTIONS ============
            // Transaction pages - visible to Admin, Manager
            if (isAdmin || isManager)
            {
                AddNavigationPage("Sale Drafts", _saleTempsListPage);
                AddNavigationPage("Sales Returns", _salesReturnsListPage);
                AddNavigationPage("Purchases", _purchasesListPage);
                AddNavigationPage("Purchase Returns", _purchaseReturnsListPage);
                AddNavigationPage("Stock Movements", _stockMovementsListPage);
                AddNavigationPage("Assemblies (BOM)", _assembliesListPage);
                AddNavigationPage("Stock Transfers", _stockTransfersListPage);
                AddNavigationPage("Expenses", _expensesListPage);
                AddNavigationPage("Expense Types", _expenseTypesListPage);
                AddNavigationPage("Payments", _paymentsListPage);
            }
            // Warehouse-specific pages
            else if (isWarehouse)
            {
                AddNavigationPage("Stock Movements", _stockMovementsListPage);
                AddNavigationPage("Stock Transfers", _stockTransfersListPage);
            }

            // Select first page by default
            if (_navigationPane.Pages.Count > 0)
            {
                _navigationPane.SelectedPageIndex = 0;
            }
        }

        private void AddNavigationPage(string caption, Control content)
        {
            var page = new NavigationPage { Caption = caption };
            page.Controls.Add(content);
            _navigationPane.Pages.Add(page);
        }

        private void UpdateUserInfo()
        {
            if (_userInfoLabel != null && _userSession.IsAuthenticated)
            {
                var roles = string.Join(", ", _userSession.Roles.Select(r => r.Name));
                _userInfoLabel.Text = $"{_userSession.CurrentUser?.FullName ?? _userSession.CurrentUser?.Username}\n{roles}";
            }
        }

        private async void ApplyUserPreferences()
        {
            try
            {
                // Apply default theme
                var defaultTheme = await _themeService.GetDefaultAsync();
                if (defaultTheme != null)
                {
                    await _themeService.ApplyThemeAsync(defaultTheme);
                }

                // Apply default language (could set thread culture, etc.)
                var defaultLanguage = await _languageService.GetDefaultAsync();
                if (defaultLanguage != null && !string.IsNullOrEmpty(defaultLanguage.CultureCode))
                {
                    try
                    {
                        var culture = new System.Globalization.CultureInfo(defaultLanguage.CultureCode);
                        System.Threading.Thread.CurrentThread.CurrentUICulture = culture;
                        System.Threading.Thread.CurrentThread.CurrentCulture = culture;
                    }
                    catch
                    {
                        // Invalid culture code, ignore
                    }
                }
            }
            catch (Exception ex)
            {
                // Log but don't crash
                System.Diagnostics.Debug.WriteLine($"Failed to apply user preferences: {ex.Message}");
            }
        }

        private void OpenNewSaleDialog()
        {
            using var dialog = _serviceProvider.GetService<NewSaleForm>();
            if (dialog != null && dialog.ShowDialog(this) == DialogResult.OK)
            {
                // Refresh sales list if needed
                _salesListPage.LoadAsync().ConfigureAwait(false);
            }
        }

        private void OpenHoldSaleDialog()
        {
            using var dialog = _serviceProvider.GetService<SalesHoldForm>();
            if (dialog != null && dialog.ShowDialog(this) == DialogResult.OK)
            {
                // Refresh sales list if needed
                _salesListPage.LoadAsync().ConfigureAwait(false);
            }
        }

        private void OnChangePasswordClick(object? sender, EventArgs e)
        {
            using var dialog = _serviceProvider.GetService<ChangePasswordForm>();
            if (dialog != null)
            {
                dialog.ShowDialog(this);
            }
        }

        private void OnLogoutClick(object? sender, EventArgs e)
        {
            if (ShowConfirm("Are you sure you want to logout?", "Confirm Logout"))
            {
                // Clear user session
                _userSession.Clear();

                // Close MainForm and return to LoginForm
                this.DialogResult = DialogResult.Abort; // Special code for logout
                this.Close();
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && _productsListPage != null)
            {
                // Dispose core list pages
                _productsListPage.Dispose();
                _customersListPage.Dispose();
                _salesListPage.Dispose();
                _outstandingListPage.Dispose();

                // Dispose master data list pages
                _categoriesListPage.Dispose();
                _unitsListPage.Dispose();
                _groupsListPage.Dispose();
                _currenciesListPage.Dispose();
                _taxesListPage.Dispose();
                _discountsListPage.Dispose();
                _locationsListPage.Dispose();
                _companiesListPage.Dispose();
                _usersListPage.Dispose();
                _rolesListPage.Dispose();
                _reportMenusListPage.Dispose();
                _emailSettingsListPage.Dispose();
                _suppliersListPage.Dispose();
                _themesListPage.Dispose();
                _languagesListPage.Dispose();

                // Dispose transaction list pages
                _saleTempsListPage.Dispose();
                _salesReturnsListPage.Dispose();
                _purchasesListPage.Dispose();
                _purchaseReturnsListPage.Dispose();
                _stockMovementsListPage.Dispose();
                _assembliesListPage.Dispose();
                _stockTransfersListPage.Dispose();
                _expensesListPage.Dispose();
                _expenseTypesListPage.Dispose();
                _paymentsListPage.Dispose();

                // Dispose reports viewer
                _reportsViewerForm.Dispose();

                // Dispose navigation
                _navigationPane?.Dispose();
                _mainContainer?.Dispose();
                _userInfoLabel?.Dispose();

                _productsListPage = null!;
                _customersListPage = null!;
                _salesListPage = null!;
                _outstandingListPage = null!;
                _categoriesListPage = null!;
                _unitsListPage = null!;
                _groupsListPage = null!;
                _currenciesListPage = null!;
                _taxesListPage = null!;
                _discountsListPage = null!;
                _locationsListPage = null!;
                _companiesListPage = null!;
                _usersListPage = null!;
                _rolesListPage = null!;
                _reportMenusListPage = null!;
                _emailSettingsListPage = null!;
                _suppliersListPage = null!;
                _saleTempsListPage = null!;
                _salesReturnsListPage = null!;
                _purchasesListPage = null!;
                _purchaseReturnsListPage = null!;
                _stockMovementsListPage = null!;
                _assembliesListPage = null!;
                _stockTransfersListPage = null!;
                _expensesListPage = null!;
                _expenseTypesListPage = null!;
                _paymentsListPage = null!;
                _reportsViewerForm = null!;
                _navigationPane = null!;
                _mainContainer = null!;
            }
            base.Dispose(disposing);
        }
    }
}