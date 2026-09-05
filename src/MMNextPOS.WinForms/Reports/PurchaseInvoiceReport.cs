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
    /// Purchase Invoice Report - detailed A4 invoice for a purchase transaction.
    /// </summary>
    public class PurchaseInvoiceReport : BaseReport
    {

        // Header controls
        private XRLabel _poNumberLabel = null!;
        private XRLabel _invoiceDateLabel = null!;
        private XRLabel _dueDateLabel = null!;
        private XRLabel _supplierNameLabel = null!;
        private XRLabel _supplierAddressLabel = null!;
        private XRLabel _supplierPhoneLabel = null!;
        private XRLabel _supplierEmailLabel = null!;
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

        private Purchase? _currentPurchase;
        private List<PurchaseDetail> _purchaseDetails = new();

        public PurchaseInvoiceReport(
            IPurchaseService purchaseService,
            IProductService productService,
            ISupplierService supplierService,
            ISettingService settingService)
        {
            _purchaseService = purchaseService ?? throw new ArgumentNullException(nameof(purchaseService));
            _productService = productService ?? throw new ArgumentNullException(nameof(productService));
            _supplierService = supplierService ?? throw new ArgumentNullException(nameof(supplierService));
            _settingService = settingService ?? throw new ArgumentNullException(nameof(settingService));

            InitializeReport();
        }

        private void InitializeReport()
        {
            Name = "rptPurchaseInvoice";
            DisplayName = "Purchase Invoice";
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
            float width = 727;
            float leftMargin = 0;
            float rightHalf = width / 2;
            float leftHalf = width / 2;

            // Company Header
            var companyPanel = CreateCompanyHeader(0, leftHalf);
            companyPanel.LocationF = new PointF(leftMargin, yPos);
            band.Controls.Add(companyPanel);
            yPos += 90;

            // Invoice Title
            var titleLabel = new XRLabel
            {
                Text = "PURCHASE ORDER",
                Font = new Font("Segoe UI", 20, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 122, 204),
                LocationF = new PointF(rightHalf, 20),
                SizeF = new SizeF(rightHalf - 20, 40),
                TextAlignment = TextAlignment.MiddleRight
            };
            band.Controls.Add(titleLabel);

            // PO Number & Dates
            var infoTable = new XRTable
            {
                LocationF = new PointF(rightHalf, 70),
                SizeF = new SizeF(rightHalf - 20, 80),
                Font = new Font("Segoe UI", 9)
            };

            _poNumberLabel = new XRLabel { Text = "PO #: ", Font = new Font("Segoe UI", 9) };
            _invoiceDateLabel = new XRLabel { Text = "Order Date: ", Font = new Font("Segoe UI", 9) };
            _dueDateLabel = new XRLabel { Text = "Expected Date: ", Font = new Font("Segoe UI", 9) };

            infoTable.Rows.Add(CreateInfoRow("PO #:", _poNumberLabel));
            infoTable.Rows.Add(CreateInfoRow("Order Date:", _invoiceDateLabel));
            infoTable.Rows.Add(CreateInfoRow("Expected:", _dueDateLabel));

            band.Controls.Add(infoTable);
            yPos += 100;

            // Supplier Info
            var supplierPanel = new XRPanel
            {
                LocationF = new PointF(leftMargin, yPos),
                SizeF = new SizeF(leftHalf, 100),
                Borders = BorderSide.All,
                BorderColor = Color.LightGray,
                BorderWidth = 1
            };

            var supplierLabel = new XRLabel
            {
                Text = "SUPPLIER:",
                Font = new Font("Segoe UI", 8, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 122, 204),
                LocationF = new PointF(5, 5),
                SizeF = new SizeF(leftHalf - 10, 20),
                TextAlignment = TextAlignment.MiddleLeft
            };
            supplierPanel.Controls.Add(supplierLabel);

            _supplierNameLabel = new XRLabel
            {
                Text = "Supplier Name",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                LocationF = new PointF(5, 25),
                SizeF = new SizeF(leftHalf - 10, 22),
                TextAlignment = TextAlignment.MiddleLeft
            };
            supplierPanel.Controls.Add(_supplierNameLabel);

            _supplierAddressLabel = new XRLabel
            {
                Text = "Address",
                Font = new Font("Segoe UI", 8),
                LocationF = new PointF(5, 47),
                SizeF = new SizeF(leftHalf - 10, 22),
                TextAlignment = TextAlignment.MiddleLeft
            };
            supplierPanel.Controls.Add(_supplierAddressLabel);

            _supplierPhoneLabel = new XRLabel
            {
                Text = "Phone: ",
                Font = new Font("Segoe UI", 8),
                LocationF = new PointF(5, 69),
                SizeF = new SizeF(leftHalf - 10, 22),
                TextAlignment = TextAlignment.MiddleLeft
            };
            supplierPanel.Controls.Add(_supplierPhoneLabel);

            _supplierEmailLabel = new XRLabel
            {
                Text = "Email: ",
                Font = new Font("Segoe UI", 8),
                LocationF = new PointF(5, 91),
                SizeF = new SizeF(leftHalf - 10, 22),
                TextAlignment = TextAlignment.MiddleLeft
            };
            supplierPanel.Controls.Add(_supplierEmailLabel);

            band.Controls.Add(supplierPanel);

            // Order Info
            var orderPanel = new XRPanel
            {
                LocationF = new PointF(rightHalf, yPos),
                SizeF = new SizeF(rightHalf - 20, 100),
                Borders = BorderSide.All,
                BorderColor = Color.LightGray,
                BorderWidth = 1
            };

            var orderLabel = new XRLabel
            {
                Text = "ORDER INFO:",
                Font = new Font("Segoe UI", 8, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 122, 204),
                LocationF = new PointF(5, 5),
                SizeF = new SizeF(rightHalf - 30, 20),
                TextAlignment = TextAlignment.MiddleLeft
            };
            orderPanel.Controls.Add(orderLabel);

            var buyerLabel = new XRLabel
            {
                Text = "Buyer: ",
                Font = new Font("Segoe UI", 9),
                LocationF = new PointF(5, 30),
                SizeF = new SizeF(rightHalf - 30, 20),
                TextAlignment = TextAlignment.MiddleLeft
            };
            orderPanel.Controls.Add(buyerLabel);

            var paymentTermsLabel = new XRLabel
            {
                Text = "Payment Terms: Net 30",
                Font = new Font("Segoe UI", 9),
                LocationF = new PointF(5, 55),
                SizeF = new SizeF(rightHalf - 30, 20),
                TextAlignment = TextAlignment.MiddleLeft
            };
            orderPanel.Controls.Add(paymentTermsLabel);

            var referenceLabel = new XRLabel
            {
                Text = "Reference: ",
                Font = new Font("Segoe UI", 9),
                LocationF = new PointF(5, 80),
                SizeF = new SizeF(rightHalf - 30, 20),
                TextAlignment = TextAlignment.MiddleLeft
            };
            orderPanel.Controls.Add(referenceLabel);

            band.Controls.Add(orderPanel);

            yPos += 110;

            // Detail table header
            _detailTable = new XRTable
            {
                LocationF = new PointF(leftMargin, yPos),
                SizeF = new SizeF(width, 30),
                Font = new Font("Segoe UI", 9)
            };
            _detailTable.Rows.Add(CreateHeaderRow(new[] { "#", "Product", "Qty", "Unit Cost", "Disc.", "Tax", "Total" }));
            band.Controls.Add(_detailTable);
        }


        private void BuildDetail(DetailBand band)
        {
            var detailTable = new XRTable
            {
                LocationF = new PointF(0, 0),
                SizeF = new SizeF(727, 25),
                Font = new Font("Segoe UI", 9)
            };
            detailTable.Rows.Add(CreateDetailRow(new[] { "", "", "", "", "", "", "" }));
            band.Controls.Add(detailTable);
            _detailTable = detailTable;
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

            // Totals table (right-aligned)
            var totalsTable = new XRTable
            {
                LocationF = new PointF(rightAlign, yPos),
                SizeF = new SizeF(250, 100),
                Font = new Font("Segoe UI", 9)
            };

            _subtotalLabel = new XRLabel { Text = "Subtotal: ", Font = new Font("Segoe UI", 9) };
            _discountLabel = new XRLabel { Text = "Discount: ", Font = new Font("Segoe UI", 9) };
            _taxLabel = new XRLabel { Text = "Tax: ", Font = new Font("Segoe UI", 9) };
            _totalLabel = new XRLabel { Text = "Total: ", Font = new Font("Segoe UI", 10, FontStyle.Bold) };

            totalsTable.Rows.Add(CreateInfoRow("Subtotal:", _subtotalLabel));
            totalsTable.Rows.Add(CreateInfoRow("Discount:", _discountLabel));
            totalsTable.Rows.Add(CreateInfoRow("Tax:", _taxLabel));
            totalsTable.Rows.Add(CreateTotalRow("TOTAL:", _totalLabel));

            band.Controls.Add(totalsTable);
            yPos += 110;

            // Amount Paid / Balance Due
            var paymentTable = new XRTable
            {
                LocationF = new PointF(rightAlign, yPos),
                SizeF = new SizeF(250, 60),
                Font = new Font("Segoe UI", 9)
            };

            _amountPaidLabel = new XRLabel { Text = "Amount Paid: ", Font = new Font("Segoe UI", 9) };
            _balanceDueLabel = new XRLabel { Text = "Balance Due: ", Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = Color.FromArgb(200, 80, 80) };

            paymentTable.Rows.Add(CreateInfoRow("Amount Paid:", _amountPaidLabel));
            paymentTable.Rows.Add(CreateTotalRow("Balance Due:", _balanceDueLabel));

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
                Text = "Please deliver by the expected date.",
                Font = new Font("Segoe UI", 9),
                LocationF = new PointF(0, yPos),
                SizeF = new SizeF(width, 40),
                TextAlignment = TextAlignment.MiddleLeft,
                Multiline = true
            };
            band.Controls.Add(_notesLabel);
            yPos += 45;

            // Terms
            _termsLabel = new XRLabel
            {
                Text = "Terms: All goods remain property of supplier until full payment received.",
                Font = new Font("Segoe UI", 7),
                ForeColor = Color.Gray,
                LocationF = new PointF(0, yPos),
                SizeF = new SizeF(width, 30),
                TextAlignment = TextAlignment.MiddleLeft,
                Multiline = true
            };
            band.Controls.Add(_termsLabel);
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
        /// Populates the report with purchase data.
        /// </summary>
        public async Task PopulateAsync(int purchaseId, CancellationToken cancellationToken = default)
        {
            _currentPurchase = await _purchaseService.GetByIdAsync(purchaseId, cancellationToken);
            if (_currentPurchase == null)
                throw new InvalidOperationException($"Purchase #{purchaseId} not found");

            // Populate header
            _poNumberLabel.Text = $"PO #: PO-{_currentPurchase.Id:D6}";
            _invoiceDateLabel.Text = $"Order Date: {FormatDate(_currentPurchase.PurchaseDate)}";
            _dueDateLabel.Text = $"Expected Date: {FormatDate(_currentPurchase.PurchaseDate.AddDays(30))}";

            // Supplier info
            var supplier = await _supplierService.GetByIdAsync(_currentPurchase.SupplierId, cancellationToken);
            if (supplier != null)
            {
                _supplierNameLabel.Text = supplier.Name;
                _supplierAddressLabel.Text = supplier.Address ?? "";
                _supplierPhoneLabel.Text = $"Phone: {supplier.Phone ?? "N/A"}";
                _supplierEmailLabel.Text = $"Email: {supplier.Email ?? "N/A"}";
            }

            // TODO: Populate detail rows from purchase details
        }

        public async Task<byte[]> GeneratePurchaseInvoiceAsync(int purchaseId, CancellationToken cancellationToken = default)
        {
            await PopulateAsync(purchaseId, cancellationToken);
            
            using var stream = new System.IO.MemoryStream();
            this.ExportToPdf(stream);
            return stream.ToArray();
        }
}
}