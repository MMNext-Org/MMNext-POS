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

            // Register DatabaseInitializer as singleton (run once at startup)
            services.AddSingleton<DatabaseInitializer>();

            // Register application services
            services.AddScoped<Services.ISalesService, Services.SalesService>();
            services.AddScoped<Services.ICustomerService, Services.CustomerService>();
            services.AddScoped<Services.IProductService, Services.ProductService>();

            return services;
        }
    }
}
