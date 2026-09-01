using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using MMNextPOS.Application;
using MMNextPOS.Application.Services;
using MMNextPOS.Infrastructure;

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
                // Configure DI container
                var services = new ServiceCollection();

                // Register Serilog logger
                services.AddSingleton<ILogger>(sp => Log.Logger);

                services.AddApplication(configuration); // registers repos & services
                services.AddTransient<MainForm>(); // Register the real main form
                services.AddTransient<NewSaleForm>(); // Register New Sale dialog

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
                // Close and flush Serilog
                Log.CloseAndFlush();
            }
        }
    }
}