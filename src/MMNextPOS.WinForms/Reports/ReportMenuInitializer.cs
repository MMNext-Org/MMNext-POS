using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using MMNextPOS.Application.Services;
using MMNextPOS.Domain.Models;

namespace MMNextPOS.WinForms.Reports
{
    /// <summary>
    /// Initializes the Report Menus data, including the StarReorder report entry.
    /// This class should be called during application startup after DI registration.
    /// </summary>
    public static class ReportMenuInitializer
    {
        /// <summary>
        /// Adds the StarReorder menu entry to the ReportMenus table.
        /// </summary>
        /// <param name="serviceProvider">Service provider to resolve the IReportMenusService.</param>
        /// <returns>True if successful.</returns>
        public static async Task<bool> InitializeAsync(IServiceProvider serviceProvider)
        {
            // Resolve the ReportMenusService
            var reportMenusService = serviceProvider.GetRequiredService<IReportMenusService>();

            // Define the new menu entry
            var menuEntry = new ReportMenus
            {
                Code = "StarReorder",
                Name = "Reorder Level",
                ParentCode = "INVENTORY",   // Place under Inventory section
                FormName = "",               // No specific form; report is loaded dynamically
                AssemblyName = "",           // Not used for this entry
                IconName = "icon-reorder",   // Optional icon name
                DisplayOrder = 40,           // Order after existing Inventory reports
                IsVisible = true,
                IsReport = true,
                ReportFileName = "rptStarReorder",
                Description = "Reorder level report showing products needing reorder"
            };

            // Add the entry to the database
            await reportMenusService.AddAsync(menuEntry);

            return true;
        }
    }
}