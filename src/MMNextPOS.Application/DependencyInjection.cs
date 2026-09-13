using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;

using MMNextPOS.Infrastructure;
using MMNextPOS.Infrastructure.Repositories;
using MMNextPOS.Application.Services;

namespace MMNextPOS.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
        {
            // Configure connection string options
            services.Configure<ConnectionStringOptions>(configuration.GetSection(ConnectionStringOptions.SectionName));

            // Register UnitOfWork as scoped (per request/operation)
            services.AddScoped<IUnitOfWork>(sp =>
            {
                var options = sp.GetRequiredService<IOptions<ConnectionStringOptions>>();
                return new MySqlUnitOfWork(options.Value.Default);
            });

            // Register infrastructure repositories as scoped services
            services.AddScoped<IProductRepository, ProductRepository>();
            services.AddScoped<ISaleRepository, SaleRepository>();
            services.AddScoped<ISaleDetailRepository, SaleDetailRepository>();
            services.AddScoped<ICustomerRepository, CustomerRepository>();
            services.AddScoped<ICategoryRepository, CategoryRepository>();
            services.AddScoped<IUnitRepository, UnitRepository>();
            services.AddScoped<IGroupRepository, GroupRepository>();
            services.AddScoped<ICurrencyRepository, CurrencyRepository>();
            services.AddScoped<ITaxRepository, TaxRepository>();
            services.AddScoped<IDiscountRepository, DiscountRepository>();
            services.AddScoped<ILocationRepository, LocationRepository>();
            services.AddScoped<ICompanyRepository, CompanyRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IRoleRepository, RoleRepository>();
            services.AddScoped<IUserRoleRepository, UserRoleRepository>();
            services.AddScoped<IMenuRoleRepository, MenuRoleRepository>();
            services.AddScoped<ISupplierRepository, SupplierRepository>();
            services.AddScoped<ISaleTempRepository, SaleTempRepository>();
            services.AddScoped<ISaleTempDetailRepository, SaleTempDetailRepository>();
            services.AddScoped<IEmailSettingRepository, EmailSettingRepository>();
            services.AddScoped<IInvoiceRepository, InvoiceRepository>();
            services.AddScoped<ISalesReturnRepository, SalesReturnRepository>();
            services.AddScoped<ISalesReturnDetailRepository, SalesReturnDetailRepository>();
            services.AddScoped<IPurchaseRepository, PurchaseRepository>();
            services.AddScoped<IPurchaseDetailRepository, PurchaseDetailRepository>();
            services.AddScoped<IPurchaseReturnRepository, PurchaseReturnRepository>();
            services.AddScoped<IPurchaseReturnDetailRepository, PurchaseReturnDetailRepository>();
            services.AddScoped<IStockMovementRepository, StockMovementRepository>();
            services.AddScoped<IStockMovementDetailRepository, StockMovementDetailRepository>();
            services.AddScoped<IAssemblyRepository, AssemblyRepository>();
            services.AddScoped<IAssemblyDetailRepository, AssemblyDetailRepository>();
            services.AddScoped<IStockTransferRepository, StockTransferRepository>();
            services.AddScoped<IStockTransferDetailRepository, StockTransferDetailRepository>();
            services.AddScoped<IStarCashFlowReportRepository, StarCashFlowReportRepository>();
            services.AddScoped<IStarProfitLossReportRepository, StarProfitLossReportRepository>();
            services.AddScoped<IStarStockBalanceReportRepository, StarStockBalanceReportRepository>();
            services.AddScoped<IStarReorderReportRepository, StarReorderReportRepository>();
            services.AddScoped<IStarOutstandingReportRepository, StarOutstandingReportRepository>();
            services.AddScoped<IIssueHeaderRepository, IssueHeaderRepository>();
            services.AddScoped<ILicenseInfoRepository, LicenseInfoRepository>();
            services.AddScoped<IDeviceRequestRepository, DeviceRequestRepository>();
            services.AddScoped<IPcClientRepository, PcClientRepository>();
            services.AddScoped<IMobileClientRepository, MobileClientRepository>();
            services.AddScoped<IAppInfoRepository, AppInfoRepository>();
            services.AddScoped<IPCUpdateRepository, PCUpdateRepository>();
            services.AddScoped<IClientUpdateRequestRepository, ClientUpdateRequestRepository>();
            services.AddScoped<IReportMenusRepository, ReportMenusRepository>();
            services.AddScoped<IThemeRepository, ThemeRepository>();
            services.AddScoped<ILanguageRepository, LanguageRepository>();
            services.AddScoped<IChangeDateLogRepository, ChangeDateLogRepository>();
            services.AddScoped<ICustomerOutstandingRepository, CustomerOutstandingRepository>();
            services.AddScoped<ISupplierOutstandingRepository, SupplierOutstandingRepository>();

            // Admin/Cross-cutting repositories
            services.AddScoped<MMNextPOS.Infrastructure.Repositories.ISystemSettingRepository, MMNextPOS.Infrastructure.Repositories.SystemSettingRepository>();
            services.AddScoped<MMNextPOS.Infrastructure.Repositories.IBackupSettingRepository, MMNextPOS.Infrastructure.Repositories.BackupSettingRepository>();
            services.AddScoped<MMNextPOS.Infrastructure.Repositories.IDataMigrationRepository, MMNextPOS.Infrastructure.Repositories.DataMigrationRepository>();
            services.AddScoped<MMNextPOS.Infrastructure.Repositories.ISuperAdminLogRepository, MMNextPOS.Infrastructure.Repositories.SuperAdminLogRepository>();

            // Receipt/Voucher repositories
            services.AddScoped<MMNextPOS.Infrastructure.Repositories.ISaleReceiptRepository, MMNextPOS.Infrastructure.Repositories.SaleReceiptRepository>();
            services.AddScoped<MMNextPOS.Infrastructure.Repositories.ISaleReceiptDetailRepository, MMNextPOS.Infrastructure.Repositories.SaleReceiptDetailRepository>();
            services.AddScoped<MMNextPOS.Infrastructure.Repositories.IPurchaseReceiptRepository, MMNextPOS.Infrastructure.Repositories.PurchaseReceiptRepository>();
            services.AddScoped<MMNextPOS.Infrastructure.Repositories.IPurchaseReceiptDetailRepository, MMNextPOS.Infrastructure.Repositories.PurchaseReceiptDetailRepository>();
            services.AddScoped<MMNextPOS.Infrastructure.Repositories.IPaymentVoucherRepository, MMNextPOS.Infrastructure.Repositories.PaymentVoucherRepository>();

            // Register MigrationRunner as scoped (uses IUnitOfWork which is scoped)
            services.AddScoped<IMigrationRunner, MigrationRunner>();

            // Register DatabaseInitializer as scoped (depends on IMigrationRunner which is scoped)
            services.AddScoped<DatabaseInitializer>();

            // Register application services
            services.AddScoped<Services.ISalesService, Services.SalesService>();
            services.AddScoped<Services.ICustomerService, Services.CustomerService>();
            services.AddScoped<Services.IProductService, Services.ProductService>();
            services.AddScoped<Services.ICategoryService, Services.CategoryService>();
            services.AddScoped<Services.IUnitService, Services.UnitService>();
            services.AddScoped<Services.IGroupService, Services.GroupService>();
            services.AddScoped<Services.ICurrencyService, Services.CurrencyService>();
            services.AddScoped<Services.ITaxService, Services.TaxService>();
            services.AddScoped<Services.IDiscountService, Services.DiscountService>();
            services.AddScoped<Services.ILocationService, Services.LocationService>();
            services.AddScoped<Services.ICompanyService, Services.CompanyService>();
            services.AddScoped<Services.IUserService, Services.UserService>();
            services.AddScoped<Services.IUserSession, Services.UserSession>();
            services.AddScoped<Services.IRoleService, Services.RoleService>();
            services.AddScoped<Services.IUserRoleService, Services.UserRoleService>();
            services.AddScoped<Services.IMenuRoleService, Services.MenuRoleService>();
            services.AddScoped<Services.ISupplierService, Services.SupplierService>();
            services.AddScoped<Services.ISaleTempService, Services.SaleTempService>();
            services.AddScoped<Services.ISaleTempDetailService, Services.SaleTempDetailService>();
            services.AddScoped<Services.IEmailSettingService, Services.EmailSettingService>();
            services.AddScoped<Services.IInvoiceService, Services.InvoiceService>();
            services.AddScoped<Services.ISalesReturnService, Services.SalesReturnService>();
            services.AddScoped<Services.ISalesReturnDetailService, Services.SalesReturnDetailService>();
            services.AddScoped<Services.IPurchaseService, Services.PurchaseService>();
            services.AddScoped<Services.IPurchaseDetailService, Services.PurchaseDetailService>();
            services.AddScoped<Services.IPurchaseReturnService, Services.PurchaseReturnService>();
            services.AddScoped<Services.IInventoryService, Services.InventoryService>();
            services.AddScoped<Services.ISettingService, Services.SettingService>();
            services.AddScoped<Services.ILicenseInfoService, Services.LicenseInfoService>();
            services.AddSingleton<Services.IDeviceFingerprintService, Services.DeviceFingerprintService>();
            services.AddScoped<Services.ILicenseGuard, Services.LicenseGuard>();
            services.AddScoped<Services.IReportService, Services.ReportService>();

            // Theme & Language services
            services.AddScoped<Services.IThemeService, Services.ThemeService>();
            services.AddScoped<Services.ILanguageService, Services.LanguageService>();

            // Receipt/Voucher services
            services.AddScoped<Services.ISaleReceiptService, Services.SaleReceiptService>();
            services.AddScoped<Services.IPurchaseReceiptService, Services.PurchaseReceiptService>();
            services.AddScoped<Services.IPaymentVoucherService, Services.PaymentVoucherService>();

            // Phase 2: Sales hardening — atomic stock reservation, stock-movement audit, invoice numbering, customer outstanding
            services.AddScoped<Services.IStockMovementService, Services.StockMovementService>();
            services.AddScoped<Services.IInvoiceNumberGenerator, Services.DbInvoiceNumberGenerator>();
            services.AddScoped<Services.IReturnNumberGenerator, Services.DbReturnNumberGenerator>();
            services.AddScoped<Services.IOutstandingService, Services.OutstandingService>();

            // Phase 2b: IAuditService is consumed by ISalesService (and the various
            // supporting services it composes). It must be registered so the DI
            // container can resolve ISalesService. Phase 2 introduced the dependency
            // but missed the registration; the audit-failure integration test in
            // SalesServiceIntegrationTests surfaces this.
            services.AddScoped<Services.IAuditService, Services.AuditService>();

            // Tax rate service for automatic tax calculation by customer type/jurisdiction
            services.AddScoped<Services.ITaxRateService, Services.TaxRateService>();

            // Admin/Cross-cutting services
            services.AddScoped<Services.ISystemSettingService, Services.SystemSettingService>();
            services.AddScoped<Services.IBackupService, Services.BackupService>();
            services.AddScoped<Services.IMigrationService, Services.MigrationService>();
            services.AddScoped<Services.ISuperAdminService, Services.SuperAdminService>();
            services.AddScoped<Services.ITurnstileVerificationService, Services.CloudflareTurnstileVerificationService>();

            return services;
        }
    }
}
