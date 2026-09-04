using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using MMNextPOS.Application;
using MMNextPOS.Application.Services;
using MMNextPOS.Infrastructure;
using MMNextPOS.Infrastructure.Repositories;

namespace MMNextPOS.WinForms
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            // Load configuration from appsettings.json
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            // Configure and initialize Serilog
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .CreateLogger();

            try
            {
                // Start the health check endpoint in background
                var healthCheckCts = new CancellationTokenSource();
                var healthCheckTask = Task.Run(() => RunHealthCheckEndpoint(configuration, healthCheckCts.Token));

                try
                {
                    // Configure DI container
                    var services = new ServiceCollection();

                    // Register Serilog logger
                    services.AddSingleton<Serilog.ILogger>(sp => Log.Logger);
                    services.AddSingleton<Microsoft.Extensions.Logging.ILogger>(sp => 
                        new Microsoft.Extensions.Logging.LoggerFactory().CreateLogger("MMNextPOS"));

                    services.AddApplication(configuration); // registers repos & services

                    // Core Forms
                    services.AddTransient<MainForm>();
                    services.AddTransient<NewSaleForm>();
                    services.AddTransient<OutstandingForm>();
                    services.AddTransient<ReportsViewerForm>(); // Reports viewer

                    // WinForms Services
                    services.AddTransient<Services.WinFormsReportService>(); // WinForms DevExpress ReportService

                    // Core ListPages
                    services.AddTransient<ProductsListPage>();
                    services.AddTransient<CustomersListPage>();
                    services.AddTransient<SalesListPage>();
                    services.AddTransient<OutstandingListPage>();

                    // Master Data ListPages
                    services.AddTransient<CategoriesListPage>();
                    services.AddTransient<UnitsListPage>();
                    services.AddTransient<GroupsListPage>();
                    services.AddTransient<CurrenciesListPage>();
                    services.AddTransient<TaxesListPage>();
                    services.AddTransient<DiscountsListPage>();
                    services.AddTransient<LocationsListPage>();
                    services.AddTransient<CompaniesListPage>();
                    services.AddTransient<UsersListPage>();
                    services.AddTransient<RolesListPage>();
                    services.AddTransient<ReportMenusListPage>();
                    services.AddTransient<EmailSettingsListPage>();
                    services.AddTransient<SuppliersListPage>();

                    // Transaction ListPages
                    services.AddTransient<SaleTempsListPage>();
                    services.AddTransient<SalesReturnsListPage>();
                    services.AddTransient<PurchasesListPage>();
                    services.AddTransient<PurchaseReturnsListPage>();
                    services.AddTransient<StockMovementsListPage>();
                    services.AddTransient<AssembliesListPage>();
                    services.AddTransient<StockTransfersListPage>();
                    services.AddTransient<ExpensesListPage>();
                    services.AddTransient<ExpenseTypesListPage>();
                    services.AddTransient<PaymentsListPage>();

                    // Build provider
                    using var serviceProvider = services.BuildServiceProvider();

                    // Ensure DB schema exists before launching UI
                    var dbInit = serviceProvider.GetRequiredService<DatabaseInitializer>();
                    dbInit.InitializeAsync().GetAwaiter().GetResult();

                    // Resolve the main form
                    var mainForm = serviceProvider.GetRequiredService<MainForm>();

                    System.Windows.Forms.Application.SetHighDpiMode(HighDpiMode.SystemAware);
                    System.Windows.Forms.Application.EnableVisualStyles();
                    System.Windows.Forms.Application.SetCompatibleTextRenderingDefault(false);
                    System.Windows.Forms.Application.Run(mainForm);
                }
                finally
                {
                    healthCheckCts.Cancel();
                    healthCheckTask.Wait(TimeSpan.FromSeconds(5));
                    Log.CloseAndFlush();
                }
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Application terminated unexpectedly");
                throw;
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        static async Task RunHealthCheckEndpoint(IConfiguration configuration, CancellationToken cancellationToken)
        {
            var healthCheckPort = configuration.GetValue<int>("HealthChecks:Port", 5001);
            var healthCheckPath = configuration.GetValue<string>("HealthChecks:Path", "/health");

            var builder = WebApplication.CreateBuilder(new WebApplicationOptions
            {
                ApplicationName = "MMNextPOS Health Checks",
                ContentRootPath = Directory.GetCurrentDirectory()
            });

            builder.WebHost.UseUrls($"http://localhost:{healthCheckPort}");
            builder.WebHost.ConfigureKestrel(options =>
            {
                options.ListenLocalhost(healthCheckPort);
            });

            builder.Services.AddHealthChecks()
                .AddAsyncCheck("Database", async (CancellationToken ct) =>
                {
                    try
                    {
                        using var scope = CreateHealthCheckScope();
                        var dbInit = scope.ServiceProvider.GetRequiredService<DatabaseInitializer>();
                        await dbInit.InitializeAsync(ct);
                        return HealthCheckResult.Healthy("Database connection OK");
                    }
                    catch (Exception ex)
                    {
                        return HealthCheckResult.Unhealthy("Database connection failed", ex);
                    }
                })
                .AddCheck("Disk Space", () =>
                {
                    var drives = System.IO.DriveInfo.GetDrives();
                    var systemDrive = drives.FirstOrDefault(d => d.IsReady && d.Name.StartsWith("C"));
                    if (systemDrive != null)
                    {
                        var freeSpaceGB = systemDrive.AvailableFreeSpace / (1024 * 1024 * 1024);
                        if (freeSpaceGB < 1)
                            return HealthCheckResult.Degraded($"Low disk space: {freeSpaceGB:F1} GB free");
                        if (freeSpaceGB < 5)
                            return HealthCheckResult.Degraded($"Low disk space: {freeSpaceGB:F1} GB free");
                    }
                    return HealthCheckResult.Healthy("Disk space OK");
                })
                .AddCheck("Memory", () =>
                {
                    var process = System.Diagnostics.Process.GetCurrentProcess();
                    var workingSetMB = process.WorkingSet64 / (1024 * 1024);
                    if (workingSetMB > 1000)
                        return HealthCheckResult.Degraded($"High memory usage: {workingSetMB} MB");
                    return HealthCheckResult.Healthy($"Memory usage: {workingSetMB} MB");
                });

            var app = builder.Build();

            app.MapHealthChecks("/health", new HealthCheckOptions
            {
                Predicate = _ => true,
                ResponseWriter = async (context, report) =>
                {
                    context.Response.ContentType = "application/json";
                    var result = new
                    {
                        status = report.Status.ToString(),
                        checks = report.Entries.Select(e => new
                        {
                            name = e.Key,
                            status = e.Value.Status.ToString(),
                            description = e.Value.Description,
                            duration = e.Value.Duration.TotalMilliseconds
                        }),
                        totalDuration = report.TotalDuration.TotalMilliseconds
                    };
                    await context.Response.WriteAsJsonAsync(result);
                }
            });

            app.MapHealthChecks("/health/live", new HealthCheckOptions
            {
                Predicate = check => check.Tags.Contains("live"),
                ResponseWriter = async (context, report) =>
                {
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsJsonAsync(new { status = report.Status.ToString() });
                }
            });

            app.MapHealthChecks("/health/ready", new HealthCheckOptions
            {
                Predicate = check => check.Tags.Contains("ready"),
                ResponseWriter = async (context, report) =>
                {
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsJsonAsync(new { status = report.Status.ToString() });
                }
            });

            // Add a simple UI for health checks
            app.MapHealthChecks("/health-ui", new HealthCheckOptions
            {
                Predicate = _ => true,
                ResponseWriter = async (context, report) =>
                {
                    context.Response.ContentType = "text/html";
                    var html = $@"
<!DOCTYPE html>
<html>
<head>
    <title>MMNextPOS Health Checks</title>
    <style>
        body {{ font-family: Arial, sans-serif; margin: 20px; }}
        .healthy {{ color: green; }}
        .degraded {{ color: orange; }}
        .unhealthy {{ color: red; }}
        table {{ border-collapse: collapse; width: 100%; }}
        th, td {{ border: 1px solid #ddd; padding: 8px; text-align: left; }}
        th {{ background-color: #f2f2f2; }}
    </style>
</head>
<body>
    <h1>MMNextPOS Health Checks</h1>
    <p>Status: <span class='{report.Status.ToString().ToLower()}'>{report.Status}</span></p>
    <p>Total Duration: {report.TotalDuration.TotalMilliseconds:F0} ms</p>
    <table>
        <tr><th>Check</th><th>Status</th><th>Description</th><th>Duration (ms)</th></tr>
";
                    foreach (var entry in report.Entries)
                    {
                        html += $"<tr><td>{entry.Key}</td><td class='{entry.Value.Status.ToString().ToLower()}'>{entry.Value.Status}</td><td>{entry.Value.Description}</td><td>{entry.Value.Duration.TotalMilliseconds:F0}</td></tr>";
                    }
                    html += @"
    </table>
</body>
</html>";
                    await context.Response.WriteAsync(html);
                }
            });

            Log.Information("Starting health check endpoint on http://localhost:{Port}{Path}", healthCheckPort, healthCheckPath);
            await app.RunAsync(cancellationToken);
        }

        static IServiceScope CreateHealthCheckScope()
        {
            var services = new ServiceCollection();
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            services.AddSingleton<Serilog.ILogger>(sp => Log.Logger);
            services.AddApplication(configuration);
            return services.BuildServiceProvider().CreateScope();
        }
    }
}