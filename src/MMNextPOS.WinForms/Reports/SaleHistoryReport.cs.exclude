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
    /// Sale History Report - detailed list of sales transactions with filtering.
    /// </summary>
    public class SaleHistoryReport : BaseReport
    {

        // Controls
        private XRLabel _periodLabel = null!;
        private XRLabel _filterLabel = null!;
        private XRTable _detailTable = null!;
        private XRLabel _totalSalesLabel = null!;
        private XRLabel _totalAmountLabel = null!;
        private XRLabel _avgSaleLabel = null!;

        private List<Sale> _sales = new();

        public SaleHistoryReport(
            ISalesService salesService,
            IProductService productService,
            ICustomerService customerService)
        {
            _salesService = salesService ?? throw new ArgumentNullException(nameof(salesService));
            _productService = productService ?? throw new ArgumentNullException(nameof(productService));
            _customerService = customerService ?? throw new ArgumentNullException(nameof(customerService));

            InitializeReport();
        }

        private void InitializeReport()
        {
            Name = "rptSaleHistory";
            DisplayName = "Sale History";
            PageWidth = 827;
            PageHeight = 1169;
            Margins = new Margins(50, 50, 50, 50);
            PaperKind = DXPaperKind.A4;
            Font = new Font("Segoe UI", 9.75f);

            var headerBand = new ReportHeaderBand { HeightF = 130 };
            var detailBand = new DetailBand { HeightF = 25 };
            var footerBand = new ReportFooterBand { HeightF = 80 };
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
            var titleLabel = CreateReportTitle("Sale History Report", yPos, width);
            band.Controls.Add(titleLabel);
            yPos += 35;

            // Period info
            var paramTable = new XRTable
            {
                LocationF = new PointF(0, yPos),
                SizeF = new SizeF(width, 40),
                Font = new Font("Segoe UI", 9)
            };

            _periodLabel = new XRLabel { Text = "Period: ", Font = new Font("Segoe UI", 9) };
            _filterLabel = new XRLabel { Text = "Filter: All Sales", Font = new Font("Segoe UI", 9) };

            paramTable.Rows.Add(CreateInfoRow("Period:", _periodLabel));
            paramTable.Rows.Add(CreateInfoRow("Filter:", _filterLabel));

            band.Controls.Add(paramTable);
        }


        private void BuildDetail(DetailBand band)
        {
            _detailTable = new XRTable
            {
                LocationF = new PointF(0, 0),
                SizeF = new SizeF(727, 25),
                Font = new Font("Segoe UI", 9)
            };
            _detailTable.Rows.Add(CreateHeaderRow(new[] { "Date", "Invoice #", "Customer", "Items", "Payment", "Status", "Total" }));
            band.Controls.Add(_detailTable);
        }

        private void BuildFooter(ReportFooterBand band)
        {
            float yPos = 10;
            float width = 727;
            float rightAlign = width - 300;

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
                SizeF = new SizeF(300, 70),
                Font = new Font("Segoe UI", 9)
            };

            _totalSalesLabel = new XRLabel { Text = "Total Transactions: ", Font = new Font("Segoe UI", 9) };
            _totalAmountLabel = new XRLabel { Text = "Total Amount: ", Font = new Font("Segoe UI", 9, FontStyle.Bold) };
            _avgSaleLabel = new XRLabel { Text = "Average Sale: ", Font = new Font("Segoe UI", 9) };

            summaryTable.Rows.Add(CreateInfoRow("Total Transactions:", _totalSalesLabel));
            summaryTable.Rows.Add(CreateInfoRow("Total Amount:", _totalAmountLabel));
            summaryTable.Rows.Add(CreateTotalRow("Average Sale:", _avgSaleLabel));

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
        /// Populates the report with sale history data.
        /// </summary>
        public async Task PopulateAsync(
            DateTime fromDate, 
            DateTime toDate, 
            int? customerId = null, 
            int? locationId = null,
            string? status = null,
            CancellationToken cancellationToken = default)
        {
            // Note: This would need ISalesService to support filtering
            // For now, we'll get all sales and filter in memory
            _sales = (await _salesService.GetAllAsync(cancellationToken)).ToList();

            // Apply filters
            _sales = _sales.Where(s => s.SaleDate >= fromDate && s.SaleDate <= toDate).ToList();

            if (customerId.HasValue)
                _sales = _sales.Where(s => s.CustomerId == customerId.Value).ToList();

            if (locationId.HasValue)
                _sales = _sales.Where(s => s.LocationId == locationId.Value).ToList();

            if (!string.IsNullOrEmpty(status))
                _sales = _sales.Where(s => s.Status.Equals(status, StringComparison.OrdinalIgnoreCase)).ToList();

            // Update header
            _periodLabel.Text = $"Period: {FormatDate(fromDate)} to {FormatDate(toDate)}";
            var filters = new List<string>();
            if (customerId.HasValue) filters.Add($"Customer ID: {customerId.Value}");
            if (locationId.HasValue) filters.Add($"Location ID: {locationId.Value}");
            if (!string.IsNullOrEmpty(status)) filters.Add($"Status: {status}");
            _filterLabel.Text = $"Filter: {(filters.Any() ? string.Join(", ", filters) : "All Sales")}";

            // Populate detail rows
            foreach (var sale in _sales.OrderBy(s => s.SaleDate))
            {
                var customer = sale.CustomerId > 0 ? await _customerService.GetByIdAsync(sale.CustomerId, cancellationToken) : null;
                var row = CreateDetailRow(new[]
                {
                    FormatDateTime(sale.SaleDate),
                    $"INV-{sale.Id:D6}",
                    customer?.Name ?? "Walk-in",
                    "N/A", // Would need item count
                    sale.Status, // Payment method would need lookup
                    sale.Status,
                    FormatCurrency(sale.TotalAmount)
                });
                _detailTable.Rows.Add(row);
            }

            // Update footer
            _totalSalesLabel.Text = $"Total Transactions: {_sales.Count}";
            var totalAmount = _sales.Sum(s => s.TotalAmount);
            _totalAmountLabel.Text = $"Total Amount: {FormatCurrency(totalAmount)}";
            _avgSaleLabel.Text = $"Average Sale: {FormatCurrency(_sales.Any() ? totalAmount / _sales.Count : 0)}";
        }

        public async Task<byte[]> GenerateSaleHistoryAsync(
            DateTime fromDate,
            DateTime toDate,
            int? customerId = null,
            int? locationId = null,
            string? status = null,
            CancellationToken cancellationToken = default)
        {
            await PopulateAsync(fromDate, toDate, customerId, locationId, status, cancellationToken);
            
            using var stream = new System.IO.MemoryStream();
            this.ExportToPdf(stream);
            return stream.ToArray();
        }
    }
}