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
    /// Outstanding Report - shows customer and supplier outstanding balances.
    /// </summary>
    public class OutstandingReport : BaseReport
    {

        // Controls
        private XRLabel _asOfDateLabel = null!;
        private XRLabel _filterLabel = null!;
        private XRTable _customerTable = null!;
        private XRTable _supplierTable = null!;
        private XRLabel _customerTotalLabel = null!;
        private XRLabel _supplierTotalLabel = null!;
        private XRLabel _grandTotalLabel = null!;

        private List<CustomerOutstanding> _customerOutstandings = new();
        private List<SupplierOutstanding> _supplierOutstandings = new();

        public OutstandingReport(
            IOutstandingService outstandingService,
            ICustomerService customerService,
            ISupplierService supplierService)
        {
            _outstandingService = outstandingService ?? throw new ArgumentNullException(nameof(outstandingService));
            _customerService = customerService ?? throw new ArgumentNullException(nameof(customerService));
            _supplierService = supplierService ?? throw new ArgumentNullException(nameof(supplierService));

            InitializeReport();
        }

        private void InitializeReport()
        {
            Name = "rptOutstanding";
            DisplayName = "Outstanding Balances";
            PageWidth = 827;
            PageHeight = 1169;
            Margins = new Margins(50, 50, 50, 50);
            PaperKind = DXPaperKind.A4;
            Font = new Font("Segoe UI", 9.75f);

            var headerBand = new ReportHeaderBand { HeightF = 130 };
            var detailBand = new DetailBand { HeightF = 300 };
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
            var titleLabel = CreateReportTitle("Outstanding Balances Report", yPos, width);
            band.Controls.Add(titleLabel);
            yPos += 35;

            // Period info
            var paramTable = new XRTable
            {
                LocationF = new PointF(0, yPos),
                SizeF = new SizeF(width, 40),
                Font = new Font("Segoe UI", 9)
            };

            _asOfDateLabel = new XRLabel { Text = $"As of: {DateTime.Today:yyyy-MM-dd}", Font = new Font("Segoe UI", 9) };
            _filterLabel = new XRLabel { Text = "Filter: All Parties", Font = new Font("Segoe UI", 9) };

            paramTable.Rows.Add(CreateInfoRow("As of Date:", _asOfDateLabel));
            paramTable.Rows.Add(CreateInfoRow("Filter:", _filterLabel));

            band.Controls.Add(paramTable);
        }


        private void BuildDetail(DetailBand band)
        {
            // The report has two sections - Customer and Supplier outstanding.
            // BuildDetail is a placeholder; actual population happens in PopulateAsync.
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

            _customerTotalLabel = new XRLabel { Text = "Total Receivables: ", Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = Color.Green };
            _supplierTotalLabel = new XRLabel { Text = "Total Payables: ", Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = Color.Red };
            _grandTotalLabel = new XRLabel { Text = "Net Position: ", Font = new Font("Segoe UI", 10, FontStyle.Bold) };

            summaryTable.Rows.Add(CreateInfoRow("Total Receivables:", _customerTotalLabel));
            summaryTable.Rows.Add(CreateInfoRow("Total Payables:", _supplierTotalLabel));

            var sepRow = new XRTableRow { HeightF = 2 };
            sepRow.Cells.Add(new XRTableCell { Borders = BorderSide.None });
            sepRow.Cells.Add(new XRTableCell { Borders = BorderSide.None });
            summaryTable.Rows.Add(sepRow);

            summaryTable.Rows.Add(CreateTotalRow("Net Position:", _grandTotalLabel));

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
        /// Populates the report with outstanding data.
        /// </summary>
        public async Task PopulateAsync(DateTime asOfDate, CancellationToken cancellationToken = default)
        {
            // Use GetAllCustomerOutstandingAsync / GetAllSupplierOutstandingAsync
            // since the per-customer variants require a customerId that this report doesn't have.
            _customerOutstandings = (await _outstandingService!.GetAllCustomerOutstandingAsync(cancellationToken) ?? new List<CustomerOutstanding>()).ToList()!;
            _supplierOutstandings = (await _outstandingService!.GetAllSupplierOutstandingAsync(cancellationToken) ?? new List<SupplierOutstanding>()).ToList()!;

            // Filter by asOfDate: keep only records with TransactionDate <= asOfDate
            _customerOutstandings = _customerOutstandings
                .Where(co => co.TransactionDate <= asOfDate)
                .ToList();
            _supplierOutstandings = _supplierOutstandings
                .Where(so => so.TransactionDate <= asOfDate)
                .ToList();

            // Update header
            _asOfDateLabel.Text = $"As of: {FormatDate(asOfDate)}";

            // Populate customer table
            decimal customerTotal = 0;
            foreach (var co in _customerOutstandings)
            {
                var customer = await _customerService!.GetByIdAsync(co.CustomerId, cancellationToken);
                var row = CreateDetailRow(new[]
                {
                    customer?.Name ?? $"Customer #{co.CustomerId}",
                    co.SaleId > 0 ? $"INV-{co.SaleId:D6}" : "N/A",
                    FormatDate(co.TransactionDate),
                    FormatDate(co.TransactionDate.AddDays(30)), // Due date estimate
                    FormatCurrency(co.DebitAmount),
                    FormatCurrency(co.CreditAmount),
                    FormatCurrency(co.Balance)
                });
                _customerTable.Rows.Add(row);
                customerTotal += co.Balance;
            }

            // Add customer total row
            var customerTotalRow = CreateTotalRow("Customer Total:", new XRLabel { Text = FormatCurrency(customerTotal), Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = Color.Green });
            _customerTable.Rows.Add(customerTotalRow);

            // Populate supplier table
            decimal supplierTotal = 0;
            foreach (var so in _supplierOutstandings)
            {
                var supplier = await _supplierService!.GetByIdAsync(so.SupplierId, cancellationToken);
                var row = CreateDetailRow(new[]
                {
                    supplier?.Name ?? $"Supplier #{so.SupplierId}",
                    so.PurchaseId > 0 ? $"PO-{so.PurchaseId:D6}" : "N/A",
                    FormatDate(so.TransactionDate),
                    FormatDate(so.TransactionDate.AddDays(30)),
                    FormatCurrency(so.DebitAmount),
                    FormatCurrency(so.CreditAmount),
                    FormatCurrency(so.Balance)
                });
                _supplierTable.Rows.Add(row);
                supplierTotal += so.Balance;
            }

            // Add supplier total row
            var supplierTotalRow = CreateTotalRow("Supplier Total:", new XRLabel { Text = FormatCurrency(supplierTotal), Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = Color.Red });
            _supplierTable.Rows.Add(supplierTotalRow);

            // Update footer
            _customerTotalLabel.Text = $"Total Receivables: {FormatCurrency(customerTotal)}";
            _supplierTotalLabel.Text = $"Total Payables: {FormatCurrency(supplierTotal)}";

            var netPosition = customerTotal - supplierTotal;
            _grandTotalLabel.Text = $"Net Position: {FormatCurrency(netPosition)}";
            _grandTotalLabel.ForeColor = netPosition >= 0 ? Color.Green : Color.Red;
        }

        public async Task<byte[]> GenerateOutstandingAsync(
            DateTime asOfDate,
            CancellationToken cancellationToken = default)
        {
            await PopulateAsync(asOfDate, cancellationToken);

            using var stream = new System.IO.MemoryStream();
            this.ExportToPdf(stream);
            return stream.ToArray();
        }
    }
}
