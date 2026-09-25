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
using MMNextPOS.Application.Services;
using MMNextPOS.Application.Utilities;
using MMNextPOS.Domain.Models;
using MMNextPOS.WinForms.Services;

namespace MMNextPOS.WinForms.Reports
{
    /// <summary>
    /// Sale Receipt Report – 3‑inch thermal receipt localized for Myanmar and English.
    /// </summary>
    public class SaleReceiptReport : BaseReport
    {
        private readonly ITranslationService _translationService;

        public SaleReceiptReport(ISalesService salesService, IProductService productService, ICustomerService customerService, ITranslationService translationService)
        {
            _salesService = salesService ?? throw new ArgumentNullException(nameof(salesService));
            _productService = productService ?? throw new ArgumentNullException(nameof(productService));
            _customerService = customerService ?? throw new ArgumentNullException(nameof(customerService));
            _translationService = translationService ?? throw new ArgumentNullException(nameof(translationService));

            InitializeReport();
        }

        private void InitializeReport()
        {
            Name = "rptSaleReceipt";
            DisplayName = "Sale Receipt (Bilingual)";
            PageWidth = 300; 
            PageHeight = 1169;
            Margins = new Margins(10, 10, 10, 10);
            PaperKind = DXPaperKind.Custom;
            
            Font = _translationService.CurrentLanguage == LanguageType.Myanmar 
                ? FontHelper.CreateMyanmarFont(8) 
                : new Font("Segoe UI", 8);

            var headerBand = new ReportHeaderBand { HeightF = 180 };
            var detailBand = new DetailBand { HeightF = 25 };
            var footerBand = new ReportFooterBand { HeightF = 110 };

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

            var companyLabel = new XRLabel
            {
                Text = "MMNext POS",
                Font = _translationService.CurrentLanguage == LanguageType.Myanmar 
                    ? FontHelper.CreateMyanmarFontBold(10) 
                    : new Font("Segoe UI", 10, FontStyle.Bold),
                LocationF = new PointF(0, yPos),
                SizeF = new SizeF(280, 20),
                TextAlignment = TextAlignment.MiddleCenter
            };
            band.Controls.Add(companyLabel);
            yPos += 22;

            var sloganLabel = new XRLabel
            {
                Text = " Modern Point of Sale ",
                Font = _translationService.CurrentLanguage == LanguageType.Myanmar 
                    ? FontHelper.CreateMyanmarFont(7) 
                    : new Font("Segoe UI", 7),
                ForeColor = Color.Gray,
                LocationF = new PointF(0, yPos),
                SizeF = new SizeF(280, 15),
                TextAlignment = TextAlignment.MiddleCenter
            };
            band.Controls.Add(sloganLabel);
            yPos += 18;

            var sep1 = new XRLine
            {
                LocationF = new PointF(0, yPos),
                SizeF = new SizeF(280, 2),
                ForeColor = Color.Black
            };
            band.Controls.Add(sep1);
            yPos += 5;

            _saleCodeLabel = new XRLabel
            {
                Text = $"{_translationService.GetText("ReceiptNo")}: ",
                Font = _translationService.CurrentLanguage == LanguageType.Myanmar 
                    ? FontHelper.CreateMyanmarFont(8) 
                    : new Font("Segoe UI", 8),
                LocationF = new PointF(0, yPos),
                SizeF = new SizeF(280, 15),
                TextAlignment = TextAlignment.MiddleLeft
            };
            band.Controls.Add(_saleCodeLabel);
            yPos += 17;

            _dateLabel = new XRLabel
            {
                Text = $"{_translationService.GetText("Date")}: ",
                Font = _translationService.CurrentLanguage == LanguageType.Myanmar 
                    ? FontHelper.CreateMyanmarFont(8) 
                    : new Font("Segoe UI", 8),
                LocationF = new PointF(0, yPos),
                SizeF = new SizeF(280, 15),
                TextAlignment = TextAlignment.MiddleLeft
            };
            band.Controls.Add(_dateLabel);
            yPos += 17;

            _customerLabel = new XRLabel
            {
                Text = $"{_translationService.GetText("Customer")}: ",
                Font = _translationService.CurrentLanguage == LanguageType.Myanmar 
                    ? FontHelper.CreateMyanmarFont(8) 
                    : new Font("Segoe UI", 8),
                LocationF = new PointF(0, yPos),
                SizeF = new SizeF(280, 15),
                TextAlignment = TextAlignment.MiddleLeft
            };
            band.Controls.Add(_customerLabel);
            yPos += 17;

            _cashierLabel = new XRLabel
            {
                Text = $"{_translationService.GetText("Advance")}: ",
                Font = _translationService.CurrentLanguage == LanguageType.Myanmar 
                    ? FontHelper.CreateMyanmarFont(8) 
                    : new Font("Segoe UI", 8),
                LocationF = new PointF(0, yPos),
                SizeF = new SizeF(280, 15),
                TextAlignment = TextAlignment.MiddleLeft
            };
            band.Controls.Add(_cashierLabel);
            yPos += 5;

            var sep2 = new XRLine
            {
                LocationF = new PointF(0, yPos),
                SizeF = new SizeF(280, 2),
                ForeColor = Color.Black
            };
            band.Controls.Add(sep2);
            yPos += 5;

            var headerTable = new XRTable
            {
                LocationF = new PointF(0, yPos),
                SizeF = new SizeF(280, 20),
                Font = _translationService.CurrentLanguage == LanguageType.Myanmar 
                    ? FontHelper.CreateMyanmarFontBold(7) 
                    : new Font("Segoe UI", 7, FontStyle.Bold)
            };
            headerTable.Rows.Add(CreateHeaderRow(new[]
            {
                _translationService.GetText("Item"),
                _translationService.GetText("Qty"),
                _translationService.GetText("Price"),
                _translationService.GetText("Total")
            }));
            band.Controls.Add(headerTable);
        }

        private XRLabel _saleCodeLabel = null!;
        private XRLabel _dateLabel = null!;
        private XRLabel _customerLabel = null!;
        private XRLabel _cashierLabel = null!;

        private void BuildDetail(DetailBand band)
        {
            var detailTable = new XRTable
            {
                LocationF = new PointF(0, 0),
                SizeF = new SizeF(280, 25),
                Font = _translationService.CurrentLanguage == LanguageType.Myanmar 
                    ? FontHelper.CreateMyanmarFont(7) 
                    : new Font("Segoe UI", 7)
            };
            detailTable.Rows.Add(CreateDetailRow(new[] { "", "", "", "" }));
            band.Controls.Add(detailTable);
            _detailTable = detailTable;
        }

        private XRTable _detailTable = null!;

        private void BuildFooter(ReportFooterBand band)
        {
            float yPos = 5;

            var sep = new XRLine
            {
                LocationF = new PointF(0, yPos),
                SizeF = new SizeF(280, 2),
                ForeColor = Color.Black
            };
            band.Controls.Add(sep);
            yPos += 5;

            _subtotalLabel = new XRLabel
            {
                Text = $"{_translationService.GetText("Total")}: ",
                Font = _translationService.CurrentLanguage == LanguageType.Myanmar 
                    ? FontHelper.CreateMyanmarFont(8) 
                    : new Font("Segoe UI", 8),
                LocationF = new PointF(0, yPos),
                SizeF = new SizeF(280, 17),
                TextAlignment = TextAlignment.MiddleRight
            };
            band.Controls.Add(_subtotalLabel);
            yPos += 17;

            _discountLabel = new XRLabel
            {
                Text = $"{_translationService.GetText("Discount")}: ",
                Font = _translationService.CurrentLanguage == LanguageType.Myanmar 
                    ? FontHelper.CreateMyanmarFont(8) 
                    : new Font("Segoe UI", 8),
                LocationF = new PointF(0, yPos),
                SizeF = new SizeF(280, 17),
                TextAlignment = TextAlignment.MiddleRight
            };
            band.Controls.Add(_discountLabel);
            yPos += 17;

            _taxLabel = new XRLabel
            {
                Text = $"{_translationService.GetText("Tax")}: ",
                Font = _translationService.CurrentLanguage == LanguageType.Myanmar 
                    ? FontHelper.CreateMyanmarFont(8) 
                    : new Font("Segoe UI", 8),
                LocationF = new PointF(0, yPos),
                SizeF = new SizeF(280, 17),
                TextAlignment = TextAlignment.MiddleRight
            };
            band.Controls.Add(_taxLabel);
            yPos += 17;

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
                Text = $"{_translationService.GetText("Total")}: ",
                Font = _translationService.CurrentLanguage == LanguageType.Myanmar 
                    ? FontHelper.CreateMyanmarFontBold(10) 
                    : new Font("Segoe UI", 10, FontStyle.Bold),
                LocationF = new PointF(0, yPos),
                SizeF = new SizeF(280, 22),
                TextAlignment = TextAlignment.MiddleRight
            };
            band.Controls.Add(_totalLabel);
            yPos += 22;

            _paidLabel = new XRLabel
            {
                Text = $"{_translationService.GetText("Paid")}: ",
                Font = _translationService.CurrentLanguage == LanguageType.Myanmar 
                    ? FontHelper.CreateMyanmarFont(8) 
                    : new Font("Segoe UI", 8),
                LocationF = new PointF(0, yPos),
                SizeF = new SizeF(280, 17),
                TextAlignment = TextAlignment.MiddleRight
            };
            band.Controls.Add(_paidLabel);
            yPos += 17;

            _changeLabel = new XRLabel
            {
                Text = $"{_translationService.GetText("Change")}: ",
                Font = _translationService.CurrentLanguage == LanguageType.Myanmar 
                    ? FontHelper.CreateMyanmarFont(8) 
                    : new Font("Segoe UI", 8),
                LocationF = new PointF(0, yPos),
                SizeF = new SizeF(280, 17),
                TextAlignment = TextAlignment.MiddleRight
            };
            band.Controls.Add(_changeLabel);
            yPos += 10;

            var sep2 = new XRLine
            {
                LocationF = new PointF(0, yPos),
                SizeF = new SizeF(280, 1),
                ForeColor = Color.Gray
            };
            band.Controls.Add(sep2);
            yPos += 5;

            var thanksLabel = new XRLabel
            {
                Text = _translationService.GetText("ThankYou"),
                Font = _translationService.CurrentLanguage == LanguageType.Myanmar 
                    ? FontHelper.CreateMyanmarFontBold(8) 
                    : new Font("Segoe UI", 8, FontStyle.Bold),
                LocationF = new PointF(0, yPos),
                SizeF = new SizeF(280, 17),
                TextAlignment = TextAlignment.MiddleCenter
            };
            band.Controls.Add(thanksLabel);
            yPos += 17;

            var visitLabel = new XRLabel
            {
                Text = _translationService.GetText("VisitAgain"),
                Font = _translationService.CurrentLanguage == LanguageType.Myanmar 
                    ? FontHelper.CreateMyanmarFont(7) 
                    : new Font("Segoe UI", 7),
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

        public async Task PopulateAsync(int saleId, CancellationToken cancellationToken = default)
        {
            _currentSale = await _salesService!.GetByIdAsync(saleId, cancellationToken);
            if (_currentSale == null)
                throw new InvalidOperationException($"Sale #{saleId} not found");

            _saleCodeLabel.Text = $"{_translationService.GetText("ReceiptNo")}: {_currentSale.Id}";
            _dateLabel.Text = $"{_translationService.GetText("Date")}: {FormatDateTime(_currentSale.SaleDate)}";

            var customer = _currentSale.CustomerId > 0
                ? await _customerService!.GetByIdAsync(_currentSale.CustomerId, cancellationToken)
                : null;
            
            var custName = customer?.Name ?? "Walk-in";
            _customerLabel.Text = $"{_translationService.GetText("Customer")}: {custName}";

            _cashierLabel.Text = $"{_translationService.GetText("Advance")}: User #{_currentSale.Id}";

            _saleDetails = (await _salesService.GetSaleDetailsAsync(_currentSale.Id, cancellationToken)).ToList();
            foreach (var detail in _saleDetails)
            {
                var product = await _productService!.GetByIdAsync(detail.ProductId, cancellationToken);
                var row = new XRTableRow { HeightF = 25 };
                
                var prodName = product?.Name ?? $"Product {detail.ProductId}";

                var font = _translationService.CurrentLanguage == LanguageType.Myanmar 
                    ? FontHelper.CreateMyanmarFont(8) 
                    : new Font("Segoe UI", 8);

                var cell0 = new XRTableCell { Text = detail.Quantity.ToString(), Font = font, TextAlignment = TextAlignment.MiddleCenter };
                var cell1 = new XRTableCell { Text = prodName, Font = font, TextAlignment = TextAlignment.MiddleLeft };
                var cell2 = new XRTableCell { Text = detail.Quantity.ToString(), Font = font, TextAlignment = TextAlignment.MiddleCenter };
                var cell3 = new XRTableCell { Text = FormatCurrency(detail.UnitPrice), Font = font, TextAlignment = TextAlignment.MiddleRight };
                var cell4 = new XRTableCell { Text = FormatCurrency(detail.DiscountAmount), Font = font, TextAlignment = TextAlignment.MiddleRight };
                var cell5 = new XRTableCell { Text = FormatCurrency(detail.TaxAmount), Font = font, TextAlignment = TextAlignment.MiddleRight };
                var cell6 = new XRTableCell { Text = FormatCurrency(detail.Quantity * detail.UnitPrice - detail.DiscountAmount + detail.TaxAmount), Font = font, TextAlignment = TextAlignment.MiddleRight };

                row.Cells.Add(cell0);
                row.Cells.Add(cell1);
                row.Cells.Add(cell2);
                row.Cells.Add(cell3);
                row.Cells.Add(cell4);
                row.Cells.Add(cell5);
                row.Cells.Add(cell6);
                _detailTable.Rows.Add(row);
            }

            var subtotal = _saleDetails.Sum(d => d.Quantity * d.UnitPrice);
            var discount = _saleDetails.Sum(d => d.DiscountAmount);
            var tax = _saleDetails.Sum(d => d.TaxAmount);
            var total = subtotal - discount + tax;

            _subtotalLabel.Text = $"{_translationService.GetText("Total")}: {FormatCurrency(subtotal)}";
            _discountLabel.Text = $"{_translationService.GetText("Discount")}: {FormatCurrency(discount)}";
            _taxLabel.Text = $"{_translationService.GetText("Tax")}: {FormatCurrency(tax)}";
            _totalLabel.Text = $"{_translationService.GetText("Total")}: {FormatCurrency(total)}";
        }

        public async Task<byte[]> GenerateReceiptAsync(int saleId, CancellationToken cancellationToken = default)
        {
            await PopulateAsync(saleId, cancellationToken);
            using var stream = new System.IO.MemoryStream();
            this.ExportToPdf(stream);
            return stream.ToArray();
        }
    }
}
