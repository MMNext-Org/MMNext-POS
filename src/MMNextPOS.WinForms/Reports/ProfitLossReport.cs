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
    /// Profit & Loss Statement Report - shows revenue, COGS, gross profit, expenses, net profit.
    /// </summary>
    public class ProfitLossReport : BaseReport
    {

        // Controls
        private XRLabel _periodLabel = null!;
        private XRLabel _locationLabel = null!;
        private XRLabel _generatedLabel = null!;
        private XRTable _detailTable = null!;

        // Footer
        private XRLabel _totalRevenueLabel = null!;
        private XRLabel _cogsLabel = null!;
        private XRLabel _grossProfitLabel = null!;
        private XRLabel _grossMarginLabel = null!;
        private XRLabel _operatingExpensesLabel = null!;
        private XRLabel _netProfitLabel = null!;
        private XRLabel _netMarginLabel = null!;

        private List<StarProfitLossReport> _plReports = new();

        public ProfitLossReport(IReportService reportService, ILocationService locationService)
        {
            _reportService = reportService ?? throw new ArgumentNullException(nameof(reportService));
            _locationService = locationService ?? throw new ArgumentNullException(nameof(locationService));

            InitializeReport();
        }

        private void InitializeReport()
        {
            Name = "rptProfitLoss";
            DisplayName = "Profit & Loss Statement";
            PageWidth = 827;
            PageHeight = 1169;
            Margins = new Margins(50, 50, 50, 50);
            PaperKind = DXPaperKind.A4;
            Font = new Font("Segoe UI", 9.75f);

            var headerBand = new ReportHeaderBand { HeightF = 130 };
            var detailBand = new DetailBand { HeightF = 25 };
            var footerBand = new ReportFooterBand { HeightF = 120 };
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
            var titleLabel = CreateReportTitle("Profit & Loss Statement", yPos, width);
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
            _detailTable.Rows.Add(CreateDetailRow(new[] { "Location", "Total Revenue", "% of Revenue" }));
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

            // Financial Summary
            var summaryTable = new XRTable
            {
                LocationF = new PointF(rightAlign, yPos),
                SizeF = new SizeF(300, 120),
                Font = new Font("Segoe UI", 9)
            };

            _totalRevenueLabel = new XRLabel { Text = "Total Revenue: ", Font = new Font("Segoe UI", 9, FontStyle.Bold) };
            _cogsLabel = new XRLabel { Text = "Cost of Goods Sold: ", Font = new Font("Segoe UI", 9) };
            _grossProfitLabel = new XRLabel { Text = "Gross Profit: ", Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = Color.Green };
            _grossMarginLabel = new XRLabel { Text = "Gross Margin: ", Font = new Font("Segoe UI", 9) };
            _operatingExpensesLabel = new XRLabel { Text = "Operating Expenses: ", Font = new Font("Segoe UI", 9) };
            _netProfitLabel = new XRLabel { Text = "Net Profit: ", Font = new Font("Segoe UI", 10, FontStyle.Bold) };
            _netMarginLabel = new XRLabel { Text = "Net Margin: ", Font = new Font("Segoe UI", 9) };

            summaryTable.Rows.Add(CreateTotalRow("Total Revenue:", _totalRevenueLabel));
            summaryTable.Rows.Add(CreateTotalRow("Cost of Goods Sold:", _cogsLabel));
            summaryTable.Rows.Add(CreateTotalRow("Gross Profit:", _grossProfitLabel));
            summaryTable.Rows.Add(CreateTotalRow("Gross Margin:", _grossMarginLabel));
            summaryTable.Rows.Add(CreateTotalRow("Operating Expenses:", _operatingExpensesLabel));
            summaryTable.Rows.Add(CreateTotalRow("Net Profit:", _netProfitLabel));
            summaryTable.Rows.Add(CreateTotalRow("Net Margin:", _netMarginLabel));

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
        /// Populates the report with P&L data.
        /// </summary>
        public async Task PopulateAsync(int locationId, DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default)
        {
            _plReports = (await _reportService!.GetProfitLossReportsAsync(locationId, fromDate, toDate, cancellationToken)).ToList();

            if (!_plReports.Any())
                return;

            var report = _plReports.First(); // Aggregate or use first for period

            // Update header
            _periodLabel.Text = $"Period: {FormatDate(fromDate)} to {FormatDate(toDate)}";

            var location = await _locationService!.GetByIdAsync(locationId, cancellationToken);
            _locationLabel.Text = location != null ? $"Location: {location.Name}" : $"Location ID: {locationId}";
            _generatedLabel.Text = $"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}";

            // Update footer
            _totalRevenueLabel.Text = $"Total Revenue: {FormatCurrency(report.TotalRevenue)}";
            _cogsLabel.Text = $"Cost of Goods Sold: {FormatCurrency(report.CostOfGoodsSold)}";

            var grossProfit = report.TotalRevenue - report.CostOfGoodsSold;
            _grossProfitLabel.Text = $"Gross Profit: {FormatCurrency(grossProfit)}";

            var grossMargin = report.TotalRevenue > 0 ? (grossProfit / report.TotalRevenue) * 100 : 0;
            _grossMarginLabel.Text = $"Gross Margin: {grossMargin:F2}%";

            _operatingExpensesLabel.Text = $"Operating Expenses: {FormatCurrency(report.OperatingExpenses)}";

            var netProfit = grossProfit - report.OperatingExpenses;
            _netProfitLabel.Text = $"Net Profit: {FormatCurrency(netProfit)}";

            var netMargin = report.TotalRevenue > 0 ? (netProfit / report.TotalRevenue) * 100 : 0;
            _netMarginLabel.Text = $"Net Margin: {netMargin:F2}%";

            // Build detail rows for each location/report
            foreach (var pl in _plReports)
            {
                var row = CreateDetailRow(new[]
                {
                    pl.LocationId.ToString(), // Would need location name lookup
                    FormatCurrency(pl.TotalRevenue),
                    ((pl.TotalRevenue > 0 ? pl.TotalRevenue / pl.TotalRevenue * 100 : 0).ToString("F2")) + "%"
                });
                _detailTable.Rows.Add(row);
            }
        }

        public async Task<byte[]> GenerateProfitLossAsync(
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
