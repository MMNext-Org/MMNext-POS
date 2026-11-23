using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DevExpress.XtraPrinting;
using DevExpress.XtraReports.UI;
using MMNextPOS.Application.Services;
using MMNextPOS.Domain.Models;
using MMNextPOS.WinForms.Services;

namespace MMNextPOS.WinForms.Reports
{
    /// <summary>
    /// StarReorder Report – displays reorder level information for products.
    /// </summary>
    public class StarReorderReport : BaseReport
    {
        private readonly IProductService _productService;
        private readonly IInventoryService _inventoryService;
        private readonly ISupplierService _supplierService;
        private readonly ITranslationService _translationService;
        private readonly IStarReorderReportRepository _reorderRepo;
        private XRTable? _detailTable;
        private XRLabel? _reportDateLabel;
        private XRLabel? _totalProductsLabel;
        private XRLabel? _totalReorderLabel;

        public StarReorderReport(
            IProductService productService,
            IInventoryService inventoryService,
            ISupplierService supplierService,
            ITranslationService translationService,
            IStarReorderReportRepository reorderRepo)
        {
            _productService = productService ?? throw new ArgumentNullException(nameof(productService));
            _inventoryService = inventoryService ?? throw new ArgumentNullException(nameof(inventoryService));
            _supplierService = supplierService ?? throw new ArgumentNullException(nameof(supplierService));
            _translationService = translationService ?? throw new ArgumentNullException(nameof(translationService));
            _reorderRepo = reorderRepo ?? throw new ArgumentNullException(nameof(reorderRepo));

            InitializeReport();
        }

        private void InitializeReport()
        {
            Name = "rptStarReorder";
            DisplayName = "Reorder Level Report";
            PageWidth = 827;  // A4 width
            PageHeight = 1169; // A4 height
            Margins = new Margins(50, 50, 50, 50);
            PaperKind = DXPaperKind.A4;
            Font = new Font("Segoe UI", 9);

            var headerBand = new ReportHeaderBand { HeightF = 120 };
            var detailBand = new DetailBand { HeightF = 25 };
            var footerBand = new ReportFooterBand { HeightF = 100 };

            Bands.Add(headerBand);
            Bands.Add(detailBand);
            Bands.Add(footerBand);

            BuildHeader(headerBand);
            BuildDetail(detailBand);
            BuildFooter(footerBand);
        }

        private void BuildHeader(ReportHeaderBand band)
        {
            float yPos = 5;

            // Company header
            var companyHeader = CreateCompanyHeader(yPos, 827);
            band.Controls.Add(companyHeader);
            yPos += 60;

            // Report title
            var titleLabel = CreateReportTitle("Reorder Level Report", yPos, 827);
            band.Controls.Add(titleLabel);
            yPos += 25;

            // Date parameter
            _reportDateLabel = new XRLabel
            {
                Text = "Date: ",
                Font = new Font("Segoe UI", 10),
                LocationF = new PointF(0, yPos),
                SizeF = new SizeF(827, 22),
                TextAlignment = TextAlignment.MiddleLeft
            };
            band.Controls.Add(_reportDateLabel);
            yPos += 25;

            // Separator
            var sep = new XRLine
            {
                LocationF = new PointF(0, yPos),
                SizeF = new SizeF(827, 2),
                ForeColor = Color.Black
            };
            band.Controls.Add(sep);
            yPos += 5;

            // Table header
            var headerTable = new XRTable
            {
                LocationF = new PointF(0, yPos),
                SizeF = new SizeF(827, 25)
            };
            headerTable.Rows.Add(CreateHeaderRow(new[] { "Product", "Current Stock", "Reorder Level", "Reorder Qty", "Supplier", "Days of Stock" }));
            band.Controls.Add(headerTable);
        }

        private void BuildDetail(DetailBand band)
        {
            _detailTable = new XRTable
            {
                LocationF = new PointF(0, 0),
                SizeF = new SizeF(827, 25),
                Font = new Font("Segoe UI", 9)
            };
            // Add a placeholder row (will be replaced with actual data)
            _detailTable.Rows.Add(CreateDetailRow(new[] { "", "", "", "", "", "" }));
            band.Controls.Add(_detailTable);
        }

        private void BuildFooter(ReportFooterBand band)
        {
            float yPos = 5;

            // Separator
            var sep = new XRLine
            {
                LocationF = new PointF(0, yPos),
                SizeF = new SizeF(827, 2),
                ForeColor = Color.Black
            };
            band.Controls.Add(sep);
            yPos += 10;

            // Summary labels
            _totalProductsLabel = new XRLabel
            {
                Text = "Total Products: ",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                LocationF = new PointF(0, yPos),
                SizeF = new SizeF(250, 22),
                TextAlignment = TextAlignment.MiddleLeft
            };
            band.Controls.Add(_totalProductsLabel);

            _totalReorderLabel = new XRLabel
            {
                Text = "Products Needing Reorder: ",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                LocationF = new PointF(270, yPos),
                SizeF = new SizeF(350, 22),
                TextAlignment = TextAlignment.MiddleLeft
            };
            band.Controls.Add(_totalReorderLabel);
        }

        /// <summary>
        /// Generates the report for the specified location and date.
        /// </summary>
        public async Task<byte[]> GenerateReorderReportAsync(int locationId, DateTime asOfDate, CancellationToken cancellationToken = default)
        {
            // Clear existing detail rows (keep header row)
            if (_detailTable != null)
            {
                while (_detailTable.Rows.Count > 1)
                {
                    _detailTable.Rows.RemoveAt(_detailTable.Rows.Count - 1);
                }
            }

            // Get reorder reports for the location and date
            var reports = await _reorderRepo.GetAllAsync(cancellationToken);
            var filteredReports = reports
                .Where(r => r.LocationId == locationId)
                .ToList();

            // If no reports found, we still need to show a message or empty table
            decimal totalProducts = 0;
            int totalReorderNeeded = 0;

            foreach (var report in filteredReports.OrderBy(r => r.ProductName))
            {
                // Create a detail row for each product
                var row = _detailTable.Rows.Add(CreateDetailRow(new[]
                {
                    report.ProductName,
                    report.CurrentStock.ToString(),
                    report.ReorderLevel.ToString(),
                    report.ReorderQuantity.ToString(),
                    report.SupplierName,
                    report.DaysOfStock.ToString()
                }));

                if (report.CurrentStock < report.ReorderLevel)
                {
                    totalReorderNeeded++;
                }
                totalProducts++;
            }

            // Update footer labels
            if (_totalProductsLabel != null)
                _totalProductsLabel.Text = $"Total Products: {totalProducts}";

            if (_totalReorderLabel != null)
                _totalReorderLabel.Text = $"Products Needing Reorder: {totalReorderNeeded}";

            // Generate report bytes
            using var stream = new MemoryStream();
            this.ExportToPdf(stream);
            return stream.ToArray();
        }
    }
}
