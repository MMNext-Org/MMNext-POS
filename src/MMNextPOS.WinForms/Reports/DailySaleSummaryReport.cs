using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DevExpress.XtraPrinting;
using DevExpress.XtraReports.UI;
using DevExpress.Drawing.Printing;
using MMNextPOS.Application.Services;
using MMNextPOS.Domain.Models;

namespace MMNextPOS.WinForms.Reports
{
    /// <summary>
    /// Daily Sale Summary Report - simple list of sales for a date with totals.
    /// </summary>
    public class DailySaleSummaryReport : BaseReport
    {
        private XRTable? _detailTable;
        private XRLabel? _reportDateLabel;
        private XRLabel? _totalSalesLabel;
        private XRLabel? _totalAmountLabel;
        private XRLabel? _totalTransactionsLabel;
        private XRLabel? _cashAmountLabel;
        private XRLabel? _creditAmountLabel;

        public DailySaleSummaryReport(ISalesService salesService, IProductService productService, ICustomerService customerService)
        {
            _salesService = salesService ?? throw new ArgumentNullException(nameof(salesService));
            _productService = productService ?? throw new ArgumentNullException(nameof(productService));
            _customerService = customerService ?? throw new ArgumentNullException(nameof(customerService));

            InitializeReport();
        }

        private void InitializeReport()
        {
            Name = "rptDailySaleSummary";
            DisplayName = "Daily Sale Summary";
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
            var companyHeader = CreateCompanyHeader(yPos, 727);
            band.Controls.Add(companyHeader);
            yPos += 85;

            // Report title
            var titleLabel = CreateReportTitle("Daily Sale Summary", yPos, 727);
            band.Controls.Add(titleLabel);
            yPos += 35;

            // Report date parameter
            _reportDateLabel = new XRLabel
            {
                Text = "Date: ",
                Font = new Font("Segoe UI", 10),
                LocationF = new PointF(0, yPos),
                SizeF = new SizeF(727, 22),
                TextAlignment = TextAlignment.MiddleLeft
            };
            band.Controls.Add(_reportDateLabel);
            yPos += 25;

            // Separator
            var sep = new XRLine
            {
                LocationF = new PointF(0, yPos),
                SizeF = new SizeF(727, 2),
                ForeColor = Color.Black
            };
            band.Controls.Add(sep);
            yPos += 5;

            // Column headers
            var headerTable = new XRTable
            {
                LocationF = new PointF(0, yPos),
                SizeF = new SizeF(727, 25)
            };
            headerTable.Rows.Add(CreateHeaderRow(new[] { "Sale #", "Time", "Customer", "Items", "Payment", "Total" }));
            band.Controls.Add(headerTable);
        }

        private void BuildDetail(DetailBand band)
        {
            _detailTable = new XRTable
            {
                LocationF = new PointF(0, 0),
                SizeF = new SizeF(727, 25),
                Font = new Font("Segoe UI", 9)
            };
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
                SizeF = new SizeF(727, 2),
                ForeColor = Color.Black
            };
            band.Controls.Add(sep);
            yPos += 10;

            // Summary totals
            _totalSalesLabel = new XRLabel
            {
                Text = "Total Sales: ",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                LocationF = new PointF(0, yPos),
                SizeF = new SizeF(200, 22),
                TextAlignment = TextAlignment.MiddleLeft
            };
            band.Controls.Add(_totalSalesLabel);

            _totalAmountLabel = new XRLabel
            {
                Text = "Total Amount: ",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                LocationF = new PointF(220, yPos),
                SizeF = new SizeF(200, 22),
                TextAlignment = TextAlignment.MiddleRight
            };
            band.Controls.Add(_totalAmountLabel);
            yPos += 25;

            _totalTransactionsLabel = new XRLabel
            {
                Text = "Transactions: ",
                Font = new Font("Segoe UI", 9),
                LocationF = new PointF(0, yPos),
                SizeF = new SizeF(200, 20),
                TextAlignment = TextAlignment.MiddleLeft
            };
            band.Controls.Add(_totalTransactionsLabel);

            _cashAmountLabel = new XRLabel
            {
                Text = "Cash: ",
                Font = new Font("Segoe UI", 9),
                LocationF = new PointF(220, yPos),
                SizeF = new SizeF(200, 20),
                TextAlignment = TextAlignment.MiddleRight
            };
            band.Controls.Add(_cashAmountLabel);
            yPos += 22;

            _creditAmountLabel = new XRLabel
            {
                Text = "Credit: ",
                Font = new Font("Segoe UI", 9),
                LocationF = new PointF(440, yPos),
                SizeF = new SizeF(200, 20),
                TextAlignment = TextAlignment.MiddleRight
            };
            band.Controls.Add(_creditAmountLabel);
        }

        /// <summary>
        /// Populates the report with sales data for a specific date.
        /// </summary>
        public async Task PopulateAsync(DateTime reportDate, CancellationToken cancellationToken = default)
        {
            // Get all sales for the date (inclusive range)
            var fromDate = reportDate.Date;
            var toDate = fromDate.AddDays(1);

            var allSales = await _salesService!.GetAllAsync(fromDate, toDate, null, null, null, cancellationToken);
            var sales = allSales.Where(s => s.SaleDate >= fromDate && s.SaleDate < toDate).ToList();

            if (_reportDateLabel != null)
                _reportDateLabel.Text = $"Date: {fromDate:yyyy-MM-dd}";

            // Clear existing detail rows (keep header row)
            if (_detailTable != null)
            {
                while (_detailTable.Rows.Count > 1)
                {
                    _detailTable.Rows.RemoveAt(_detailTable.Rows.Count - 1);
                }
            }

            decimal grandTotal = 0m;
            int totalItems = 0;
            decimal cashTotal = 0m;
            decimal creditTotal = 0m;

            foreach (var sale in sales.OrderBy(s => s.SaleDate))
            {
                var customer = sale.CustomerId > 0
                    ? await _customerService!.GetByIdAsync(sale.CustomerId, cancellationToken)
                    : null;

                var saleDetails = await _salesService.GetSaleDetailsAsync(sale.Id, cancellationToken);
                int itemCount = saleDetails.Sum(d => d.Quantity);
                totalItems += itemCount;

                // Determine payment type (simplified - use status or payment method)
                string paymentType = "Cash"; // Default assumption
                decimal saleAmount = sale.TotalAmount;

                if (sale.Status == "Voided" || sale.Status == "Returned")
                {
                    paymentType = "Void/Return";
                }

                // For this spike, assume cash if customerId=0, credit otherwise
                if (sale.CustomerId == 0)
                    cashTotal += saleAmount;
                else
                    creditTotal += saleAmount;

                grandTotal += saleAmount;

                var row = new XRTableRow { HeightF = 25 };
                var cell0 = new XRTableCell { Text = sale.Id.ToString(), TextAlignment = TextAlignment.MiddleCenter, Padding = new PaddingInfo(3) };
                var cell1 = new XRTableCell { Text = sale.SaleDate.ToString("HH:mm"), TextAlignment = TextAlignment.MiddleCenter, Padding = new PaddingInfo(3) };
                var cell2 = new XRTableCell { Text = customer?.Name ?? "Walk-in", TextAlignment = TextAlignment.MiddleLeft, Padding = new PaddingInfo(5, 3, 3, 3) };
                var cell3 = new XRTableCell { Text = itemCount.ToString(), TextAlignment = TextAlignment.MiddleCenter, Padding = new PaddingInfo(3) };
                var cell4 = new XRTableCell { Text = paymentType, TextAlignment = TextAlignment.MiddleCenter, Padding = new PaddingInfo(3) };
                var cell5 = new XRTableCell { Text = FormatCurrency(saleAmount), TextAlignment = TextAlignment.MiddleRight, Padding = new PaddingInfo(3, 10, 3, 3) };

                row.Cells.Add(cell0);
                row.Cells.Add(cell1);
                row.Cells.Add(cell2);
                row.Cells.Add(cell3);
                row.Cells.Add(cell4);
                row.Cells.Add(cell5);

                _detailTable?.Rows.Add(row);
            }

            if (_totalSalesLabel != null)
                _totalSalesLabel.Text = $"Total Sales: {sales.Count}";
            if (_totalAmountLabel != null)
                _totalAmountLabel.Text = $"Total Amount: {FormatCurrency(grandTotal)}";
            if (_totalTransactionsLabel != null)
                _totalTransactionsLabel.Text = $"Total Items: {totalItems}";
            if (_cashAmountLabel != null)
                _cashAmountLabel.Text = $"Cash: {FormatCurrency(cashTotal)}";
            if (_creditAmountLabel != null)
                _creditAmountLabel.Text = $"Credit: {FormatCurrency(creditTotal)}";
        }

        /// <summary>
        /// Generates the daily sale summary as PDF bytes.
        /// </summary>
        public async Task<byte[]> GenerateSummaryAsync(DateTime reportDate, CancellationToken cancellationToken = default)
        {
            await PopulateAsync(reportDate, cancellationToken);

            using var stream = new System.IO.MemoryStream();
            this.ExportToPdf(stream);
            return stream.ToArray();
        }
    }
}
