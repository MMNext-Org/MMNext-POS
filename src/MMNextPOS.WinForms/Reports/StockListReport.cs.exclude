using System;
using System.Collections.Generic;
using System.Drawing;
using DevExpress.XtraPrinting;
using System.Drawing.Printing;
using DevExpress.Drawing.Printing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DevExpress.XtraReports.UI;
using MMNextPOS.Application.Services;
using MMNextPOS.Domain.Models;

namespace MMNextPOS.WinForms.Reports
{
    /// <summary>
    /// Stock List Report - lists all products with current stock levels.
    /// </summary>
    public class StockListReport : BaseReport
    {

        // Controls
        private XRLabel _reportDateLabel = null!;
        private XRLabel _locationLabel = null!;
        private XRLabel _filterLabel = null!;
        private XRTable _detailTable = null!;
        private XRLabel _totalProductsLabel = null!;
        private XRLabel _totalValueLabel = null!;
        private XRLabel _lowStockLabel = null!;

        private List<Product> _products = new();

        public StockListReport(
            IProductService productService,
            IInventoryService inventoryService,
            ISettingService settingService)
        {
            _productService = productService ?? throw new ArgumentNullException(nameof(productService));
            _inventoryService = inventoryService ?? throw new ArgumentNullException(nameof(inventoryService));
            _settingService = settingService ?? throw new ArgumentNullException(nameof(settingService));

            InitializeReport();
        }

        private void InitializeReport()
        {
            Name = "rptStockList";
            DisplayName = "Stock List";
            PageWidth = 827;
            PageHeight = 1169;
            Margins = new Margins(50, 50, 50, 50);
            PaperKind = DXPaperKind.A4;
            Font = new Font("Segoe UI", 9.75f);

            var headerBand = new ReportHeaderBand { HeightF = 150 };
            var detailBand = new DetailBand { HeightF = 25 };
            var footerBand = new ReportFooterBand { HeightF = 100 };
            var pageFooterBand = new PageFooterBand { HeightF = 50 };

            Bands.Add(headerBand);
            Bands.Add(detailBand);
            Bands.Add(footerBand);
            Bands.Add(pageFooterBand);

            BuildHeader(headerBand);
            BuildDetail(detailBand);
            BuildFooter(footerBand);
            BuildPageFooter(pageFooterBand);
        }

        private void BuildHeader(ReportHeaderBand band)
        {
            float yPos = 10;
            float width = 727;

            // Company Header
            var companyPanel = CreateCompanyHeader(0, width);
            companyPanel.LocationF = new PointF(0, yPos);
            band.Controls.Add(companyPanel);
            yPos += 90;

            // Report Title
            var titleLabel = CreateReportTitle("Stock List Report", yPos, width);
            band.Controls.Add(titleLabel);
            yPos += 35;

            // Report parameters
            var paramTable = new XRTable
            {
                LocationF = new PointF(0, yPos),
                SizeF = new SizeF(width, 60),
                Font = new Font("Segoe UI", 9)
            };

            _reportDateLabel = new XRLabel { Text = $"As of: {DateTime.Today:yyyy-MM-dd}", Font = new Font("Segoe UI", 9) };
            _locationLabel = new XRLabel { Text = "Location: All Locations", Font = new Font("Segoe UI", 9) };
            _filterLabel = new XRLabel { Text = "Filter: All Products", Font = new Font("Segoe UI", 9) };

            paramTable.Rows.Add(CreateInfoRow("Report Date:", _reportDateLabel));
            paramTable.Rows.Add(CreateInfoRow("Location:", _locationLabel));
            paramTable.Rows.Add(CreateInfoRow("Filter:", _filterLabel));

            band.Controls.Add(paramTable);
        }


        private void BuildDetail(DetailBand band)
        {
            _detailTable = new XRTable
            {
                LocationF = new PointF(0, 0),
                SizeF = new SizeF(727, 25),
                Font = new Font("Segoe UI", 8)
            };
            _detailTable.Rows.Add(CreateDetailRow(new[] { "", "", "", "", "", "", "", "", "" }));
            band.Controls.Add(_detailTable);
        }

        private void BuildFooter(ReportFooterBand band)
        {
            float yPos = 10;
            float width = 727;
            float rightAlign = width - 250;

            // Separator
            var sep = new XRLine
            {
                LocationF = new PointF(0, yPos),
                SizeF = new SizeF(width, 2),
                ForeColor = Color.Black
            };
            band.Controls.Add(sep);
            yPos += 10;

            // Summary
            var summaryTable = new XRTable
            {
                LocationF = new PointF(rightAlign, yPos),
                SizeF = new SizeF(250, 80),
                Font = new Font("Segoe UI", 9)
            };

            _totalProductsLabel = new XRLabel { Text = "Total Products: ", Font = new Font("Segoe UI", 9) };
            _totalValueLabel = new XRLabel { Text = "Total Stock Value: ", Font = new Font("Segoe UI", 9) };
            _lowStockLabel = new XRLabel { Text = "Low Stock Items: ", Font = new Font("Segoe UI", 9), ForeColor = Color.FromArgb(200, 80, 80) };

            summaryTable.Rows.Add(CreateInfoRow("Total Products:", _totalProductsLabel));
            summaryTable.Rows.Add(CreateInfoRow("Total Stock Value:", _totalValueLabel));
            summaryTable.Rows.Add(CreateTotalRow("Low Stock Items:", _lowStockLabel));

            band.Controls.Add(summaryTable);
        }

        private void BuildPageFooter(PageFooterBand band)
        {
            var pageInfo = new XRPageInfo
            {
                Format = "Page {0} of {1}",
                Font = new Font("Segoe UI", 8),
                ForeColor = Color.Gray,
                LocationF = new PointF(0, 10),
                SizeF = new SizeF(350, 20),
                TextAlignment = TextAlignment.MiddleLeft
            };
            band.Controls.Add(pageInfo);

            var printDate = new XRLabel
            {
                Text = $"Printed: {DateTime.Now:yyyy-MM-dd HH:mm:ss}",
                Font = new Font("Segoe UI", 8),
                ForeColor = Color.Gray,
                LocationF = new PointF(377, 10),
                SizeF = new SizeF(350, 20),
                TextAlignment = TextAlignment.MiddleRight
            };
            band.Controls.Add(printDate);
        }

        /// <summary>
        /// Populates the report with stock data.
        /// </summary>
        public async Task PopulateAsync(int? locationId = null, int? categoryId = null, bool lowStockOnly = false, CancellationToken cancellationToken = default)
        {
            _products = (await _productService.GetAllAsync(cancellationToken)).ToList();

            // Apply filters
            if (categoryId.HasValue)
            {
                _products = _products.Where(p => p.CategoryId == categoryId.Value).ToList();
            }

            if (lowStockOnly)
            {
                _products = _products.Where(p => p.StockQuantity <= 10).ToList(); // Threshold could be configurable
            }

            // Update header
            _reportDateLabel.Text = $"As of: {DateTime.Today:yyyy-MM-dd}";
            _locationLabel.Text = locationId.HasValue ? $"Location ID: {locationId.Value}" : "Location: All Locations";
            _filterLabel.Text = lowStockOnly ? "Filter: Low Stock Only" : "Filter: All Products";

            // Update summary
            int totalProducts = _products.Count;
            decimal totalValue = _products.Sum(p => p.Price * p.StockQuantity);
            int lowStockCount = _products.Count(p => p.StockQuantity <= 10);

            _totalProductsLabel.Text = $"Total Products: {totalProducts}";
            _totalValueLabel.Text = $"Total Stock Value: {FormatCurrency(totalValue)}";
            _lowStockLabel.Text = $"Low Stock Items: {lowStockCount}";
        }

        public async Task<byte[]> GenerateStockListAsync(
            int? locationId = null, 
            int? categoryId = null, 
            bool lowStockOnly = false, 
            CancellationToken cancellationToken = default)
        {
            await PopulateAsync(locationId, categoryId, lowStockOnly, cancellationToken);
            
            using var stream = new System.IO.MemoryStream();
            this.ExportToPdf(stream);
            return stream.ToArray();
        }
    }
}