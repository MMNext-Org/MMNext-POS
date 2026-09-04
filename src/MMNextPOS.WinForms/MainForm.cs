using System;
using System.Collections.Generic;
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

        public MainForm(
            IServiceProvider serviceProvider,
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
            WinFormsReportService reportService)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
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
            _reportService = reportService ?? throw new ArgumentNullException(nameof(reportService));

            InitializeComponent();
            InitializeListPages();
            InitializeNavigation();
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
            _reportsViewerForm = new ReportsViewerForm(_reportService);
        }

        private void InitializeNavigation()
        {
            // ============ CORE MODULES ============
            var productsPage = new NavigationPage { Caption = "Products" };
            productsPage.Controls.Add(_productsListPage);
            _navigationPane.Pages.Add(productsPage);

            var customersPage = new NavigationPage { Caption = "Customers" };
            customersPage.Controls.Add(_customersListPage);
            _navigationPane.Pages.Add(customersPage);

            var salesPage = new NavigationPage { Caption = "Sales" };
            // Add New Sale button to the Sales page toolbar
            var newSaleButton = new SimpleButton
            {
                Text = "New Sale",
                Location = new Point(10, 10),
                Width = 100,
                Height = 35
            };
            newSaleButton.Click += (s, e) => OpenNewSaleDialog();
            _salesListPage.Controls.Add(newSaleButton);

            // Add Hold Sale button to the Sales page toolbar
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

            var outstandingPage = new NavigationPage { Caption = "Outstanding" };
            outstandingPage.Controls.Add(_outstandingListPage);
            _navigationPane.Pages.Add(outstandingPage);

            // ============ MASTER DATA ============
            var categoriesPage = new NavigationPage { Caption = "Categories" };
            categoriesPage.Controls.Add(_categoriesListPage);
            _navigationPane.Pages.Add(categoriesPage);

            var unitsPage = new NavigationPage { Caption = "Units" };
            unitsPage.Controls.Add(_unitsListPage);
            _navigationPane.Pages.Add(unitsPage);

            var groupsPage = new NavigationPage { Caption = "Groups" };
            groupsPage.Controls.Add(_groupsListPage);
            _navigationPane.Pages.Add(groupsPage);

            var currenciesPage = new NavigationPage { Caption = "Currencies" };
            currenciesPage.Controls.Add(_currenciesListPage);
            _navigationPane.Pages.Add(currenciesPage);

            var taxesPage = new NavigationPage { Caption = "Taxes" };
            taxesPage.Controls.Add(_taxesListPage);
            _navigationPane.Pages.Add(taxesPage);

            var discountsPage = new NavigationPage { Caption = "Discounts" };
            discountsPage.Controls.Add(_discountsListPage);
            _navigationPane.Pages.Add(discountsPage);

            var locationsPage = new NavigationPage { Caption = "Locations" };
            locationsPage.Controls.Add(_locationsListPage);
            _navigationPane.Pages.Add(locationsPage);

            var companiesPage = new NavigationPage { Caption = "Companies" };
            companiesPage.Controls.Add(_companiesListPage);
            _navigationPane.Pages.Add(companiesPage);

            var suppliersPage = new NavigationPage { Caption = "Suppliers" };
            suppliersPage.Controls.Add(_suppliersListPage);
            _navigationPane.Pages.Add(suppliersPage);

            // ============ ADMINISTRATION ============
            var usersPage = new NavigationPage { Caption = "Users" };
            usersPage.Controls.Add(_usersListPage);
            _navigationPane.Pages.Add(usersPage);

            var rolesPage = new NavigationPage { Caption = "Roles" };
            rolesPage.Controls.Add(_rolesListPage);
            _navigationPane.Pages.Add(rolesPage);

            var reportMenusPage = new NavigationPage { Caption = "Report Menus" };
            reportMenusPage.Controls.Add(_reportMenusListPage);
            _navigationPane.Pages.Add(reportMenusPage);

            var emailSettingsPage = new NavigationPage { Caption = "Email Settings" };
            emailSettingsPage.Controls.Add(_emailSettingsListPage);
            _navigationPane.Pages.Add(emailSettingsPage);

            // ============ REPORTS ============
            var reportsPage = new NavigationPage { Caption = "Reports" };
            reportsPage.Controls.Add(_reportsViewerForm);
            _navigationPane.Pages.Add(reportsPage);

            // ============ TRANSACTIONS ============
            var saleTempsPage = new NavigationPage { Caption = "Sale Drafts" };
            saleTempsPage.Controls.Add(_saleTempsListPage);
            _navigationPane.Pages.Add(saleTempsPage);

            var salesReturnsPage = new NavigationPage { Caption = "Sales Returns" };
            salesReturnsPage.Controls.Add(_salesReturnsListPage);
            _navigationPane.Pages.Add(salesReturnsPage);

            var purchasesPage = new NavigationPage { Caption = "Purchases" };
            purchasesPage.Controls.Add(_purchasesListPage);
            _navigationPane.Pages.Add(purchasesPage);

            var purchaseReturnsPage = new NavigationPage { Caption = "Purchase Returns" };
            purchaseReturnsPage.Controls.Add(_purchaseReturnsListPage);
            _navigationPane.Pages.Add(purchaseReturnsPage);

            var stockMovementsPage = new NavigationPage { Caption = "Stock Movements" };
            stockMovementsPage.Controls.Add(_stockMovementsListPage);
            _navigationPane.Pages.Add(stockMovementsPage);

            var assembliesPage = new NavigationPage { Caption = "Assemblies (BOM)" };
            assembliesPage.Controls.Add(_assembliesListPage);
            _navigationPane.Pages.Add(assembliesPage);

            var stockTransfersPage = new NavigationPage { Caption = "Stock Transfers" };
            stockTransfersPage.Controls.Add(_stockTransfersListPage);
            _navigationPane.Pages.Add(stockTransfersPage);

            var expensesPage = new NavigationPage { Caption = "Expenses" };
            expensesPage.Controls.Add(_expensesListPage);
            _navigationPane.Pages.Add(expensesPage);

            var expenseTypesPage = new NavigationPage { Caption = "Expense Types" };
            expenseTypesPage.Controls.Add(_expenseTypesListPage);
            _navigationPane.Pages.Add(expenseTypesPage);

            var paymentsPage = new NavigationPage { Caption = "Payments" };
            paymentsPage.Controls.Add(_paymentsListPage);
            _navigationPane.Pages.Add(paymentsPage);
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