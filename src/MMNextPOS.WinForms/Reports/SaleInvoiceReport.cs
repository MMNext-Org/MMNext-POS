using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DevExpress.XtraPrinting;
using DevExpress.Drawing.Printing;
using DevExpress.XtraReports.UI;
using System.Windows.Forms;
using MMNextPOS.Application.Services;
using MMNextPOS.Domain.Models;

namespace MMNextPOS.WinForms.Reports
{
    /// <summary>
    /// Sale Invoice Report - detailed A4 invoice for a sale transaction.
    /// </summary>
    public class SaleInvoiceReport : BaseReport
    {
        // Non-nullable, constructor-injected service references (shadow the
        // nullable protected fields on BaseReport on purpose).
        private new readonly ISalesService _salesService;
        private new readonly IProductService _productService;
        private new readonly ICustomerService _customerService;
        private new readonly ISettingService _settingService;

        // Header controls
        private XRLabel _invoiceNumberLabel = null!;
        private XRLabel _invoiceDateLabel = null!;
        private XRLabel _dueDateLabel = null!;
        private XRLabel _customerNameLabel = null!;
        private XRLabel _customerAddressLabel = null!;
        private XRLabel _customerPhoneLabel = null!;
        private XRLabel _customerEmailLabel = null!;
        private XRTable _detailTable = null!;

        // Footer controls
        private XRLabel _subtotalLabel = null!;
        private XRLabel _discountLabel = null!;
        private XRLabel _taxLabel = null!;
        private XRLabel _totalLabel = null!;
        private XRLabel _amountPaidLabel = null!;
        private XRLabel _balanceDueLabel = null!;
        private XRLabel _notesLabel = null!;
        private XRLabel _termsLabel = null!;

        private Sale? _currentSale;
        private List<SaleDetail> _saleDetails = new();

        public SaleInvoiceReport(
            ISalesService salesService,
            IProductService productService,
            ICustomerService customerService,
            ISettingService settingService)
        {
            _salesService = salesService ?? throw new ArgumentNullException(nameof(salesService));
            _productService = productService ?? throw new ArgumentNullException(nameof(productService));
            _customerService = customerService ?? throw new ArgumentNullException(nameof(customerService));
            _settingService = settingService ?? throw new ArgumentNullException(nameof(settingService));

            InitializeReport();
        }

        private void InitializeReport()
        {
            Name = "rptSaleInvoice";
            DisplayName = "Sale Invoice";
            PageWidth = 827;
            PageHeight = 1169;
            Margins = new Margins(50, 50, 50, 50);
            PaperKind = DXPaperKind.A4;
            Font = new Font("Segoe UI", 9.75f);

            var headerBand = new ReportHeaderBand { HeightF = 280 };
            var detailBand = new DetailBand { HeightF = 25 };
            var footerBand = new ReportFooterBand { HeightF = 200 };
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
            float leftMargin = 0;
            float leftHalf = 363.5f;
            float rightHalf = 363.5f;

            // Company Header (Left side)
            var companyPanel = CreateCompanyHeader(0, leftHalf);
            companyPanel.LocationF = new PointF(leftMargin, yPos);
            band.Controls.Add(companyPanel);
            yPos += 90;

            // Invoice Title (Right side)
            var titleLabel = new XRLabel
            {
                Text = "TAX INVOICE",
                Font = new Font("Segoe UI", 20, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 122, 204),
                LocationF = new PointF(rightHalf, 20),
                SizeF = new SizeF(rightHalf - 20, 40),
                TextAlignment = TextAlignment.MiddleRight
            };
            band.Controls.Add(titleLabel);

            // Invoice Number & Dates
            var infoTable = new XRTable
            {
                LocationF = new PointF(rightHalf, 70),
                SizeF = new SizeF(rightHalf - 20, 80),
                Font = new Font("Segoe UI", 9)
            };

            _invoiceNumberLabel = new XRLabel { Text = "Invoice #: ", Font = new Font("Segoe UI", 9) };
            _invoiceDateLabel = new XRLabel { Text = "Date: ", Font = new Font("Segoe UI", 9) };
            _dueDateLabel = new XRLabel { Text = "Due Date: ", Font = new Font("Segoe UI", 9) };

            infoTable.Rows.Add(CreateInfoRow("Invoice #:", _invoiceNumberLabel));
            infoTable.Rows.Add(CreateInfoRow("Date:", _invoiceDateLabel));
            infoTable.Rows.Add(CreateInfoRow("Due Date:", _dueDateLabel));

            band.Controls.Add(infoTable);
            yPos += 100;

            // Bill To section
            var customerPanel = new XRPanel
            {
                LocationF = new PointF(0, yPos),
                SizeF = new SizeF(leftHalf, 100),
                Borders = BorderSide.All,
                BorderColor = Color.LightGray,
                BorderWidth = 1
            };

            var billToLabel = new XRLabel
            {
                Text = "BILL TO:",
                Font = new Font("Segoe UI", 8, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 122, 204),
                LocationF = new PointF(5, 5),
                SizeF = new SizeF(leftHalf - 10, 20),
                TextAlignment = TextAlignment.MiddleLeft
            };
            customerPanel.Controls.Add(billToLabel);

            _customerNameLabel = new XRLabel
            {
                Text = "Customer Name",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                LocationF = new PointF(5, 25),
                SizeF = new SizeF(leftHalf - 10, 22),
                TextAlignment = TextAlignment.MiddleLeft
            };
            customerPanel.Controls.Add(_customerNameLabel);

            _customerAddressLabel = new XRLabel
            {
                Text = "Address",
                Font = new Font("Segoe UI", 8),
                LocationF = new PointF(5, 47),
                SizeF = new SizeF(leftHalf - 10, 22),
                TextAlignment = TextAlignment.MiddleLeft
            };
            customerPanel.Controls.Add(_customerAddressLabel);

            _customerPhoneLabel = new XRLabel
            {
                Text = "Phone: ",
                Font = new Font("Segoe UI", 8),
                LocationF = new PointF(5, 69),
                SizeF = new SizeF(leftHalf - 10, 22),
                TextAlignment = TextAlignment.MiddleLeft
            };
            customerPanel.Controls.Add(_customerPhoneLabel);

            _customerEmailLabel = new XRLabel
            {
                Text = "Email: ",
                Font = new Font("Segoe UI", 8),
                LocationF = new PointF(5, 91),
                SizeF = new SizeF(leftHalf - 10, 22),
                TextAlignment = TextAlignment.MiddleLeft
            };
            customerPanel.Controls.Add(_customerEmailLabel);

            band.Controls.Add(customerPanel);

            // Sales info section
            var salesPanel = new XRPanel
            {
                LocationF = new PointF(rightHalf, yPos),
                SizeF = new SizeF(rightHalf - 20, 100),
                Borders = BorderSide.All,
                BorderColor = Color.LightGray,
                BorderWidth = 1
            };

            var salesLabel = new XRLabel
            {
                Text = "SALES INFO:",
                Font = new Font("Segoe UI", 8, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 122, 204),
                LocationF = new PointF(5, 5),
                SizeF = new SizeF(rightHalf - 30, 20),
                TextAlignment = TextAlignment.MiddleLeft
            };
            salesPanel.Controls.Add(salesLabel);

            var salespersonLabel = new XRLabel
            {
                Text = "Salesperson: ",
                Font = new Font("Segoe UI", 9),
                LocationF = new PointF(5, 30),
                SizeF = new SizeF(rightHalf - 30, 20),
                TextAlignment = TextAlignment.MiddleLeft
            };
            salesPanel.Controls.Add(salespersonLabel);

            var paymentTermsLabel = new XRLabel
            {
                Text = "Payment Terms: Net 30",
                Font = new Font("Segoe UI", 9),
                LocationF = new PointF(5, 55),
                SizeF = new SizeF(rightHalf - 30, 20),
                TextAlignment = TextAlignment.MiddleLeft
            };
            salesPanel.Controls.Add(paymentTermsLabel);

            var referenceLabel = new XRLabel
            {
                Text = "Reference: ",
                Font = new Font("Segoe UI", 9),
                LocationF = new PointF(5, 80),
                SizeF = new SizeF(rightHalf - 30, 20),
                TextAlignment = TextAlignment.MiddleLeft
            };
            salesPanel.Controls.Add(referenceLabel);

            band.Controls.Add(salesPanel);

            yPos += 110;

            // Detail table header
            _detailTable = new XRTable
            {
                LocationF = new PointF(0, yPos),
                SizeF = new SizeF(727, 30),
                Font = new Font("Segoe UI", 9)
            };
            _detailTable.Rows.Add(CreateHeaderRow(new[] { "#", "Description", "Qty", "Unit Price", "Disc.", "Tax", "Total" }));
            band.Controls.Add(_detailTable);
        }

        private new XRTableRow CreateInfoRow(string label, XRLabel valueLabel)
        {
            var row = new XRTableRow { HeightF = 25 };

            var labelCell = new XRTableCell
            {
                Text = label,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Borders = BorderSide.None,
                TextAlignment = TextAlignment.MiddleRight,
                Padding = new PaddingInfo(5, 10, 3, 3),
                WidthF = 100
            };

            var valueCell = new XRTableCell
            {
                Controls = { valueLabel },
                Borders = BorderSide.None,
                TextAlignment = TextAlignment.MiddleLeft,
                Padding = new PaddingInfo(5, 5, 3, 3)
            };

            row.Cells.Add(labelCell);
            row.Cells.Add(valueCell);
            return row;
        }

        private XRTableRow CreateHeaderRow(string[] columns)
        {
            var row = new XRTableRow { HeightF = 30 };

            foreach (var col in columns)
            {
                var cell = new XRTableCell
                {
                    Text = col,
                    Font = new Font("Segoe UI", 9, FontStyle.Bold),
                    BackColor = Color.FromArgb(0, 122, 204),
                    ForeColor = Color.White,
                    Borders = BorderSide.All,
                    BorderColor = Color.White,
                    BorderWidth = 1,
                    Padding = new PaddingInfo(5, 5, 3, 3),
                    WidthF = 100
                };
                row.Cells.Add(cell);
            }

            return row;
        }

        private void BuildDetail(DetailBand band)
        {
            var detailTable = new XRTable
            {
                LocationF = new PointF(0, 0),
                SizeF = new SizeF(727, 25),
                Font = new Font("Segoe UI", 9)
            };
            detailTable.Rows.Add(CreateHeaderRow(new[] { "#", "Description", "Qty", "Unit Price", "Disc.", "Tax", "Total" }));
            band.Controls.Add(detailTable);
            _detailTable = detailTable;
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

            // Totals table (right-aligned)
            var totalsTable = new XRTable
            {
                LocationF = new PointF(rightAlign, yPos),
                SizeF = new SizeF(300, 100),
                Font = new Font("Segoe UI", 9)
            };

            _subtotalLabel = new XRLabel { Text = "Subtotal: ", Font = new Font("Segoe UI", 9) };
            _discountLabel = new XRLabel { Text = "Discount: ", Font = new Font("Segoe UI", 9) };
            _taxLabel = new XRLabel { Text = "Tax: ", Font = new Font("Segoe UI", 9) };
            _totalLabel = new XRLabel { Text = "Total: ", Font = new Font("Segoe UI", 10, FontStyle.Bold) };

            totalsTable.Rows.Add(CreateInfoRow("Subtotal:", _subtotalLabel));
            totalsTable.Rows.Add(CreateInfoRow("Discount:", _discountLabel));
            totalsTable.Rows.Add(CreateInfoRow("Tax:", _taxLabel));
            totalsTable.Rows.Add(CreateTotalRow("Total:", _totalLabel));

            band.Controls.Add(totalsTable);
            yPos += 110;

            // Amount Paid / Balance Due
            var paymentTable = new XRTable
            {
                LocationF = new PointF(rightAlign, yPos),
                SizeF = new SizeF(300, 60),
                Font = new Font("Segoe UI", 9)
            };

            _amountPaidLabel = new XRLabel { Text = "Amount Paid: ", Font = new Font("Segoe UI", 9) };
            _balanceDueLabel = new XRLabel { Text = "Balance Due: ", Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = Color.FromArgb(200, 80, 80) };

            totalsTable.Rows.Add(CreateInfoRow("Amount Paid:", _amountPaidLabel));
            totalsTable.Rows.Add(CreateTotalRow("Balance Due:", _balanceDueLabel));

            band.Controls.Add(paymentTable);
            yPos += 70;

            // Notes
            var notesLabel = new XRLabel
            {
                Text = "Notes:",
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                LocationF = new PointF(0, yPos),
                SizeF = new SizeF(width, 20),
                TextAlignment = TextAlignment.MiddleLeft
            };
            band.Controls.Add(notesLabel);
            yPos += 22;

            _notesLabel = new XRLabel
            {
                Text = "Thank you for your business!",
                Font = new Font("Segoe UI", 9),
                LocationF = new PointF(0, yPos),
                SizeF = new SizeF(width, 40),
                TextAlignment = TextAlignment.MiddleLeft,
                Multiline = true
            };
            band.Controls.Add(_notesLabel);
            yPos += 45;

            // Terms
            var termsLabel = new XRLabel
            {
                Text = "Terms: Payment due within 30 days. Late payments may incur interest charges.",
                Font = new Font("Segoe UI", 7),
                ForeColor = Color.Gray,
                LocationF = new PointF(0, yPos),
                SizeF = new SizeF(width, 30),
                TextAlignment = TextAlignment.MiddleLeft,
                Multiline = true
            };
            band.Controls.Add(termsLabel);
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
        /// Populates the report with sale data.
        /// </summary>
        public async Task PopulateAsync(int saleId, CancellationToken cancellationToken = default)
        {
            _currentSale = await _salesService.GetByIdAsync(saleId, cancellationToken);
            if (_currentSale == null)
                throw new InvalidOperationException($"Sale #{saleId} not found");

            // Populate header
            _invoiceNumberLabel.Text = $"Invoice #: INV-{_currentSale.Id:D6}";
            _invoiceDateLabel.Text = $"Date: {FormatDate(_currentSale.SaleDate)}";
            _dueDateLabel.Text = $"Due Date: {FormatDate(_currentSale.SaleDate.AddDays(30))}";

            // Customer info
            var customer = _currentSale.CustomerId > 0 ? await _customerService.GetByIdAsync(_currentSale.CustomerId, cancellationToken) : null;
            if (customer != null)
            {
                _customerNameLabel.Text = customer.Name;
                _customerAddressLabel.Text = customer.Address ?? "";
                _customerPhoneLabel.Text = $"Phone: {customer.Phone ?? "N/A"}";
                _customerEmailLabel.Text = $"Email: {customer.Email ?? "N/A"}";
            }
            else
            {
                _customerNameLabel.Text = "Walk-in Customer";
                _customerAddressLabel.Text = "";
                _customerPhoneLabel.Text = "Phone: N/A";
                _customerEmailLabel.Text = "Email: N/A";
            }

            // TODO: Populate detail rows from sale details
            // This would require ISaleDetailRepository or extending ISalesService

            // Update footer
            var netAmount = _currentSale.TotalAmount;
            _subtotalLabel.Text = $"Subtotal: {FormatCurrency(_currentSale.TotalAmount)}";
            _discountLabel.Text = $"Discount: 0";
            _taxLabel.Text = $"Tax: 0";
            _totalLabel.Text = $"Total: {FormatCurrency(_currentSale.TotalAmount)}";
            _amountPaidLabel.Text = $"Amount Paid: 0";
            _balanceDueLabel.Text = $"Balance Due: {FormatCurrency(_currentSale.TotalAmount)}";
            _notesLabel.Text = "Thank you for your business!";
            _termsLabel.Text = "Terms: Payment due within 30 days. Late payments may incur interest charges.";
        }

        /// <summary>
        /// Generates the complete invoice.
        /// </summary>
        public async Task<byte[]> GenerateInvoiceAsync(int saleId, CancellationToken cancellationToken = default)
        {
            await PopulateAsync(saleId, cancellationToken);

            using var stream = new System.IO.MemoryStream();
            this.ExportToPdf(stream);
            return stream.ToArray();
        }
    }
}
