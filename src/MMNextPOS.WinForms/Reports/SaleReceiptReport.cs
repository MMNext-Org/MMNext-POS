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
    /// Sale Receipt Report - prints a receipt for a single sale transaction.
    /// </summary>
    public class SaleReceiptReport : BaseReport
    {

        public SaleReceiptReport(ISalesService salesService, IProductService productService, ICustomerService customerService)
        {
            _salesService = salesService ?? throw new ArgumentNullException(nameof(salesService));
            _productService = productService ?? throw new ArgumentNullException(nameof(productService));
            _customerService = customerService ?? throw new ArgumentNullException(nameof(customerService));

            InitializeReport();
        }

        private void InitializeReport()
        {
            Name = "rptSaleReceipt";
            DisplayName = "Sale Receipt";
            PageWidth = 300; // 3-inch thermal receipt width (approx 300 hundredths of inch)
            PageHeight = 1169;
            Margins = new Margins(10, 10, 10, 10);
            PaperKind = DXPaperKind.Custom;
            Font = new Font("Consolas", 8);
            
            // Create bands
            var headerBand = new ReportHeaderBand { HeightF = 180 };
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

            // Company Name
            var companyLabel = new XRLabel
            {
                Text = "MMNext POS",
                Font = new Font("Consolas", 10, FontStyle.Bold),
                LocationF = new PointF(0, yPos),
                SizeF = new SizeF(280, 20),
                TextAlignment = TextAlignment.MiddleCenter
            };
            band.Controls.Add(companyLabel);
            yPos += 22;

            // Company Slogan
            var sloganLabel = new XRLabel
            {
                Text = "Modern Point of Sale",
                Font = new Font("Consolas", 7),
                ForeColor = Color.Gray,
                LocationF = new PointF(0, yPos),
                SizeF = new SizeF(280, 15),
                TextAlignment = TextAlignment.MiddleCenter
            };
            band.Controls.Add(sloganLabel);
            yPos += 18;

            // Separator
            var sep1 = new XRLine
            {
                LocationF = new PointF(0, yPos),
                SizeF = new SizeF(280, 2),
                ForeColor = Color.Black
            };
            band.Controls.Add(sep1);
            yPos += 5;

            // Sale Info Labels (will be populated at runtime)
            _saleCodeLabel = new XRLabel
            {
                Text = "Sale #: ",
                Font = new Font("Consolas", 8),
                LocationF = new PointF(0, yPos),
                SizeF = new SizeF(280, 15),
                TextAlignment = TextAlignment.MiddleLeft
            };
            band.Controls.Add(_saleCodeLabel);
            yPos += 17;

            _dateLabel = new XRLabel
            {
                Text = "Date: ",
                Font = new Font("Consolas", 8),
                LocationF = new PointF(0, yPos),
                SizeF = new SizeF(280, 15),
                TextAlignment = TextAlignment.MiddleLeft
            };
            band.Controls.Add(_dateLabel);
            yPos += 17;

            _customerLabel = new XRLabel
            {
                Text = "Customer: ",
                Font = new Font("Consolas", 8),
                LocationF = new PointF(0, yPos),
                SizeF = new SizeF(280, 15),
                TextAlignment = TextAlignment.MiddleLeft
            };
            band.Controls.Add(_customerLabel);
            yPos += 17;

            _cashierLabel = new XRLabel
            {
                Text = "Cashier: ",
                Font = new Font("Consolas", 8),
                LocationF = new PointF(0, yPos),
                SizeF = new SizeF(280, 15),
                TextAlignment = TextAlignment.MiddleLeft
            };
            band.Controls.Add(_cashierLabel);
            yPos += 5;

            // Separator
            var sep2 = new XRLine
            {
                LocationF = new PointF(0, yPos),
                SizeF = new SizeF(280, 2),
                ForeColor = Color.Black
            };
            band.Controls.Add(sep2);
            yPos += 5;

            // Column headers
            var headerTable = new XRTable
            {
                LocationF = new PointF(0, yPos),
                SizeF = new SizeF(280, 20),
                Font = new Font("Consolas", 7, FontStyle.Bold)
            };
            headerTable.Rows.Add(CreateHeaderRow(new[] { "Item", "Qty", "Price", "Total" }));
            band.Controls.Add(headerTable);
        }

        private XRLabel _saleCodeLabel = null!;
        private XRLabel _dateLabel = null!;
        private XRLabel _customerLabel = null!;
        private XRLabel _cashierLabel = null!;

        private void BuildDetail(DetailBand band)
        {
            // Detail row will be populated at runtime via BeforePrint
            var detailTable = new XRTable
            {
                LocationF = new PointF(0, 0),
                SizeF = new SizeF(280, 25),
                Font = new Font("Consolas", 7)
            };
            detailTable.Rows.Add(CreateDetailRow(new[] { "", "", "", "" }));
            band.Controls.Add(detailTable);

            // Store reference for BeforePrint
            _detailTable = detailTable;
        }

        private XRTable _detailTable = null!;

        private void BuildFooter(ReportFooterBand band)
        {
            float yPos = 5;

            // Separator
            var sep = new XRLine
            {
                LocationF = new PointF(0, yPos),
                SizeF = new SizeF(280, 2),
                ForeColor = Color.Black
            };
            band.Controls.Add(sep);
            yPos += 5;

            // Totals
            _subtotalLabel = new XRLabel
            {
                Text = "Subtotal: ",
                Font = new Font("Consolas", 8),
                LocationF = new PointF(0, yPos),
                SizeF = new SizeF(280, 17),
                TextAlignment = TextAlignment.MiddleRight
            };
            band.Controls.Add(_subtotalLabel);
            yPos += 17;

            _discountLabel = new XRLabel
            {
                Text = "Discount: ",
                Font = new Font("Consolas", 8),
                LocationF = new PointF(0, yPos),
                SizeF = new SizeF(280, 17),
                TextAlignment = TextAlignment.MiddleRight
            };
            band.Controls.Add(_discountLabel);
            yPos += 17;

            _taxLabel = new XRLabel
            {
                Text = "Tax: ",
                Font = new Font("Consolas", 8),
                LocationF = new PointF(0, yPos),
                SizeF = new SizeF(280, 17),
                TextAlignment = TextAlignment.MiddleRight
            };
            band.Controls.Add(_taxLabel);
            yPos += 17;

            // Total line
            var totalLine = new XRLine
            {
                LocationF = new PointF(0, yPos),
                SizeF = new SizeF(280, 2),
                ForeColor = Color.Black
            };
            band.Controls.Add(totalLine);
            yPos += 5;

            _totalLabel = new XRLabel
            {
                Text = "TOTAL: ",
                Font = new Font("Consolas", 10, FontStyle.Bold),
                LocationF = new PointF(0, yPos),
                SizeF = new SizeF(280, 22),
                TextAlignment = TextAlignment.MiddleRight
            };
            band.Controls.Add(_totalLabel);
            yPos += 22;

            _paidLabel = new XRLabel
            {
                Text = "Paid: ",
                Font = new Font("Consolas", 8),
                LocationF = new PointF(0, yPos),
                SizeF = new SizeF(280, 17),
                TextAlignment = TextAlignment.MiddleRight
            };
            band.Controls.Add(_paidLabel);
            yPos += 17;

            _changeLabel = new XRLabel
            {
                Text = "Change: ",
                Font = new Font("Consolas", 8),
                LocationF = new PointF(0, yPos),
                SizeF = new SizeF(280, 17),
                TextAlignment = TextAlignment.MiddleRight
            };
            band.Controls.Add(_changeLabel);
            yPos += 10;

            // Separator
            var sep2 = new XRLine
            {
                LocationF = new PointF(0, yPos),
                SizeF = new SizeF(280, 1),
                ForeColor = Color.Gray
            };
            band.Controls.Add(sep2);
            yPos += 5;

            // Thank you message
            var thanksLabel = new XRLabel
            {
                Text = "Thank You for Shopping!",
                Font = new Font("Consolas", 8, FontStyle.Bold),
                LocationF = new PointF(0, yPos),
                SizeF = new SizeF(280, 17),
                TextAlignment = TextAlignment.MiddleCenter
            };
            band.Controls.Add(thanksLabel);
            yPos += 17;

            var visitLabel = new XRLabel
            {
                Text = "Please Visit Again",
                Font = new Font("Consolas", 7),
                ForeColor = Color.Gray,
                LocationF = new PointF(0, yPos),
                SizeF = new SizeF(280, 15),
                TextAlignment = TextAlignment.MiddleCenter
            };
            band.Controls.Add(visitLabel);
        }

        private XRLabel _subtotalLabel = null!;
        private XRLabel _discountLabel = null!;
        private XRLabel _taxLabel = null!;
        private XRLabel _totalLabel = null!;
        private XRLabel _paidLabel = null!;
        private XRLabel _changeLabel = null!;

        private Sale? _currentSale;
        private List<SaleDetail> _saleDetails = new();

        /// <summary>
        /// Populates the report with sale data.
        /// </summary>
        public async Task PopulateAsync(int saleId, CancellationToken cancellationToken = default)
        {
            _currentSale = await _salesService!.GetByIdAsync(saleId, cancellationToken);
            if (_currentSale == null)
                throw new InvalidOperationException($"Sale #{saleId} not found");

            // Populate header
            _saleCodeLabel.Text = $"Sale #: {_currentSale.Id}";
            _dateLabel.Text = $"Date: {FormatDateTime(_currentSale.SaleDate)}";

            var customer = _currentSale.CustomerId > 0 ? await _customerService!.GetByIdAsync(_currentSale.CustomerId, cancellationToken) : null;
            _customerLabel.Text = $"Customer: {customer?.Name ?? "Walk-in"}";
            
            _cashierLabel.Text = $"Cashier: User #{_currentSale.Id}"; // Would need user service

            // Get sale details
            // Note: This would need ISaleDetailRepository or extend ISalesService
            // For now, we'll use the sale's total amount
        }

        protected override void OnBeforePrint(System.ComponentModel.CancelEventArgs e)
        {
            base.OnBeforePrint(e);
            
            // This would be called during report generation to populate detail rows
            // For thermal receipt, we typically build the entire report programmatically
        }

        /// <summary>
        /// Generates the complete receipt programmatically.
        /// </summary>
        public async Task<byte[]> GenerateReceiptAsync(int saleId, CancellationToken cancellationToken = default)
        {
            await PopulateAsync(saleId, cancellationToken);
            
            using var stream = new System.IO.MemoryStream();
            this.ExportToPdf(stream);
            return stream.ToArray();
        }
    }
}