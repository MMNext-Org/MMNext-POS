using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DevExpress.XtraPrinting;
using System.Drawing.Printing;
using DevExpress.Drawing.Printing;
using DevExpress.XtraReports.UI;
using MMNextPOS.Application.Services;

namespace MMNextPOS.WinForms.Reports
{
    /// <summary>
    /// AR (Customer) Ageing Report - open receivables bucketed into
    /// 0-30 / 31-60 / 61-90 / 90+ day ranges per customer.
    /// </summary>
    public class CustomerAgeingReport : BaseReport
    {
        private XRLabel _asOfDateLabel = null!;
        private XRTable _detailTable = null!;
        private XRLabel _totalLabel = null!;

        public CustomerAgeingReport(
            IOutstandingService outstandingService,
            ICustomerService customerService)
        {
            _outstandingService = outstandingService ?? throw new ArgumentNullException(nameof(outstandingService));
            _customerService = customerService ?? throw new ArgumentNullException(nameof(customerService));

            InitializeReport();
        }

        private void InitializeReport()
        {
            Name = "rptCustomerAgeing";
            DisplayName = "AR Ageing (Customer)";
            PageWidth = 827;
            PageHeight = 1169;
            Margins = new Margins(50, 50, 50, 50);
            PaperKind = DXPaperKind.A4;
            Font = new Font("Segoe UI", 9.75f);

            var headerBand = new ReportHeaderBand { HeightF = 120 };
            var detailBand = new DetailBand { HeightF = 300 };
            var footerBand = new ReportFooterBand { HeightF = 60 };
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
            float width = 727;

            var companyPanel = CreateCompanyHeader(0, width);
            band.Controls.Add(companyPanel);

            var titleLabel = CreateReportTitle("AR Ageing Report (Customer Receivables)", 85, width);
            band.Controls.Add(titleLabel);

            _asOfDateLabel = new XRLabel
            {
                Text = "",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.Gray,
                LocationF = new PointF(0, 100),
                SizeF = new SizeF(width, 18),
                TextAlignment = TextAlignment.MiddleCenter
            };
            band.Controls.Add(_asOfDateLabel);
        }

        private void BuildDetail(DetailBand band)
        {
            _detailTable = new XRTable
            {
                LocationF = new PointF(0, 0),
                SizeF = new SizeF(727, 30),
                Name = "ageingTable"
            };

            _detailTable.Rows.Add(CreateHeaderRow(new[]
            {
                "Customer", "0-30 Days", "31-60 Days", "61-90 Days", "90+ Days", "Total"
            }));

            band.Controls.Add(_detailTable);
        }

        private void BuildFooter(ReportFooterBand band)
        {
            _totalLabel = new XRLabel
            {
                Text = "",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(51, 51, 51)
            };

            var summaryTable = new XRTable
            {
                LocationF = new PointF(300, 10),
                SizeF = new SizeF(427, 35)
            };
            summaryTable.Rows.Add(CreateTotalRow("Total Receivables:", _totalLabel));
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
        }

        /// <summary>
        /// Populates the report with AR ageing data as of the given date.
        /// </summary>
        public async Task PopulateAsync(DateTime asOfDate, CancellationToken cancellationToken = default)
        {
            var entries = await _outstandingService!.GetCustomerAgeingAsync(asOfDate, cancellationToken);

            _asOfDateLabel.Text = $"As of: {FormatDate(asOfDate)}";

            // Remove rows added by previous PopulateAsync calls (keep header row)
            while (_detailTable.Rows.Count > 1)
            {
                _detailTable.Rows.RemoveAt(_detailTable.Rows.Count - 1);
            }

            // Pre-fetch all customers once to avoid N+1 queries inside the row loop.
            var allCustomers = await _customerService!.GetAllAsync(cancellationToken).ConfigureAwait(false);
            var customerNames = allCustomers.ToDictionary(c => c.Id, c => c.Name ?? $"Customer #{c.Id}");

            decimal grandTotal = 0;
            foreach (var entry in entries.OrderBy(e => e.CustomerId))
            {
                _detailTable.Rows.Add(CreateDetailRow(new[]
                {
                    customerNames.TryGetValue(entry.CustomerId, out var name) ? name : $"Customer #{entry.CustomerId}",
                    FormatCurrency(entry.CurrentBalance),
                    FormatCurrency(entry.Days31To60),
                    FormatCurrency(entry.Days61To90),
                    FormatCurrency(entry.Days90Plus),
                    FormatCurrency(entry.TotalBalance)
                }));

                grandTotal += entry.TotalBalance;
            }

            if (entries.Count == 0)
            {
                _detailTable.Rows.Add(CreateDetailRow(new[]
                {
                    "No open receivables", "", "", "", "", ""
                }));
            }

            _totalLabel.Text = FormatCurrency(grandTotal);
        }

        /// <summary>
        /// Generates the report as PDF bytes.
        /// </summary>
        public async Task<byte[]> GenerateAgeingAsync(DateTime asOfDate, CancellationToken cancellationToken = default)
        {
            await PopulateAsync(asOfDate, cancellationToken);

            using var stream = new System.IO.MemoryStream();
            this.ExportToPdf(stream);
            return stream.ToArray();
        }
    }
}
