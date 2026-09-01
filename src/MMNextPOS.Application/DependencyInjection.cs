using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MMNextPOS.Infrastructure;
using MMNextPOS.Infrastructure.Repositories;

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
            services.AddScoped<IEmailSettingRepository, EmailSettingRepository>();

            // Register DatabaseInitializer as singleton (run once at startup)
            services.AddSingleton<DatabaseInitializer>();

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
            services.AddScoped<Services.IRoleService, Services.RoleService>();
            services.AddScoped<Services.IUserRoleService, Services.UserRoleService>();
            services.AddScoped<Services.IMenuRoleService, Services.MenuRoleService>();
            services.AddScoped<Services.ISupplierService, Services.SupplierService>();
            services.AddScoped<Services.ISaleTempService, Services.SaleTempService>();
            services.AddScoped<Services.IEmailSettingService, Services.EmailSettingService>();

            return services;
        }
    }
}
