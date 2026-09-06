using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;
using DevExpress.XtraPrinting;
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
    /// Cash Flow Report - shows opening balance, cash inflows/outflows, closing balance.
    /// </summary>
    public class CashFlowReport : BaseReport
    {

        // Controls
        private XRLabel _periodLabel = null!;
        private XRLabel _locationLabel = null!;
        private XRLabel _generatedLabel = null!;
        private XRTable _detailTable = null!;

        // Footer
        private XRLabel _openingBalanceLabel = null!;
        private XRLabel _totalSalesLabel = null!;
        private XRLabel _totalPurchasesLabel = null!;
        private XRLabel _totalExpensesLabel = null!;
        private XRLabel _totalCollectionsLabel = null!;
        private XRLabel _totalPaymentsLabel = null!;
        private XRLabel _closingBalanceLabel = null!;
        private XRLabel _netCashFlowLabel = null!;

        private List<StarCashFlowReport> _cfReports = new();

        public CashFlowReport(IReportService reportService, ILocationService locationService)
        {
            _reportService = reportService ?? throw new ArgumentNullException(nameof(reportService));
            _locationService = locationService ?? throw new ArgumentNullException(nameof(locationService));

            InitializeReport();
        }

        private void InitializeReport()
        {
            Name = "rptCashFlow";
            DisplayName = "Cash Flow Statement";
            PageWidth = 827;
            PageHeight = 1169;
            Margins = new Margins(50, 50, 50, 50);
            PaperKind = DXPaperKind.A4;
            Font = new Font("Segoe UI", 9.75f);

            var headerBand = new ReportHeaderBand { HeightF = 130 };
            var detailBand = new DetailBand { HeightF = 25 };
            var footerBand = new ReportFooterBand { HeightF = 140 };
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
            var titleLabel = CreateReportTitle("Cash Flow Statement", yPos, width);
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
            _locationLabel = new XRLabel { Text = "Location: ", Font = new Font("Segoe UI", 9) };
            _generatedLabel = new XRLabel { Text = $"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}", Font = new Font("Segoe UI", 9) };

            paramTable.Rows.Add(CreateInfoRow("Period:", _periodLabel));
            paramTable.Rows.Add(CreateInfoRow("Location:", _locationLabel));

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
            _detailTable.Rows.Add(CreateDetailRow(new[] { "Date", "Description", "Inflow", "Outflow", "Balance" }));
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

            // Cash Flow Summary
            var summaryTable = new XRTable
            {
                LocationF = new PointF(rightAlign, yPos),
                SizeF = new SizeF(300, 140),
                Font = new Font("Segoe UI", 9)
            };

            _openingBalanceLabel = new XRLabel { Text = "Opening Balance: ", Font = new Font("Segoe UI", 9) };
            _totalSalesLabel = new XRLabel { Text = "Total Sales: ", Font = new Font("Segoe UI", 9), ForeColor = Color.Green };
            _totalPurchasesLabel = new XRLabel { Text = "Total Purchases: ", Font = new Font("Segoe UI", 9), ForeColor = Color.Red };
            _totalExpensesLabel = new XRLabel { Text = "Total Expenses: ", Font = new Font("Segoe UI", 9), ForeColor = Color.Red };
            _totalCollectionsLabel = new XRLabel { Text = "Total Collections: ", Font = new Font("Segoe UI", 9), ForeColor = Color.Green };
            _totalPaymentsLabel = new XRLabel { Text = "Total Payments: ", Font = new Font("Segoe UI", 9), ForeColor = Color.Red };
            _closingBalanceLabel = new XRLabel { Text = "Closing Balance: ", Font = new Font("Segoe UI", 10, FontStyle.Bold) };
            _netCashFlowLabel = new XRLabel { Text = "Net Cash Flow: ", Font = new Font("Segoe UI", 10, FontStyle.Bold) };

            summaryTable.Rows.Add(CreateInfoRow("Opening Balance:", _openingBalanceLabel));
            summaryTable.Rows.Add(CreateInfoRow("Total Sales:", _totalSalesLabel));
            summaryTable.Rows.Add(CreateInfoRow("Total Purchases:", _totalPurchasesLabel));
            summaryTable.Rows.Add(CreateInfoRow("Total Expenses:", _totalExpensesLabel));
            summaryTable.Rows.Add(CreateInfoRow("Total Collections:", _totalCollectionsLabel));
            summaryTable.Rows.Add(CreateInfoRow("Total Payments:", _totalPaymentsLabel));

            var sepRow = new XRTableRow { HeightF = 2 };
            sepRow.Cells.Add(new XRTableCell { Borders = BorderSide.None });
            sepRow.Cells.Add(new XRTableCell { Borders = BorderSide.None });
            summaryTable.Rows.Add(sepRow);

            summaryTable.Rows.Add(CreateTotalRow("Closing Balance:", _closingBalanceLabel));
            summaryTable.Rows.Add(CreateTotalRow("Net Cash Flow:", _netCashFlowLabel));

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
        /// Populates the report with cash flow data.
        /// </summary>
        public async Task PopulateAsync(int locationId, DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default)
        {
            _cfReports = (await _reportService!.GetCashFlowReportsAsync(locationId, fromDate, toDate, cancellationToken)).ToList();

            // Update header
            _periodLabel.Text = $"Period: {FormatDate(fromDate)} to {FormatDate(toDate)}";

            var location = await _locationService!.GetByIdAsync(locationId, cancellationToken);
            _locationLabel.Text = location != null ? $"Location: {location.Name}" : $"Location ID: {locationId}";
            _generatedLabel.Text = $"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}";

            if (!_cfReports.Any())
                return;

            var report = _cfReports.First();

            // Update footer
            _openingBalanceLabel.Text = $"Opening Balance: {FormatCurrency(report.OpeningBalance)}";
            _totalSalesLabel.Text = $"Total Sales: {FormatCurrency(report.TotalSales)}";
            _totalPurchasesLabel.Text = $"Total Purchases: {FormatCurrency(report.TotalPurchases)}";
            _totalExpensesLabel.Text = $"Total Expenses: {FormatCurrency(report.TotalExpenses)}";
            _totalCollectionsLabel.Text = $"Total Collections: {FormatCurrency(report.TotalCollections)}";
            _totalPaymentsLabel.Text = $"Total Payments: {FormatCurrency(report.TotalPayments)}";
            _closingBalanceLabel.Text = $"Closing Balance: {FormatCurrency(report.ClosingBalance)}";

            var netCashFlow = report.ClosingBalance - report.OpeningBalance;
            _netCashFlowLabel.Text = $"Net Cash Flow: {FormatCurrency(netCashFlow)}";
            _netCashFlowLabel.ForeColor = netCashFlow >= 0 ? Color.Green : Color.Red;

            // Build detail rows
            foreach (var cf in _cfReports)
            {
                var row = CreateDetailRow(new[]
                {
                    FormatDate(cf.ReportDate),
                    cf.Notes,
                    FormatCurrency(cf.TotalSales + cf.TotalCollections),
                    FormatCurrency(cf.TotalPurchases + cf.TotalExpenses + cf.TotalPayments),
                    FormatCurrency(cf.ClosingBalance)
                });
                _detailTable.Rows.Add(row);
            }
        }

        public async Task<byte[]> GenerateCashFlowAsync(
            int locationId,
            DateTime fromDate,
            DateTime toDate,
            CancellationToken cancellationToken = default)
        {
            await PopulateAsync(locationId, fromDate, toDate, cancellationToken);

            using var stream = new System.IO.MemoryStream();
            this.ExportToPdf(stream);
            return stream.ToArray();
        }
    }
}
