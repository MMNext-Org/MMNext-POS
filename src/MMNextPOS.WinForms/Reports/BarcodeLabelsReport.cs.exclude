using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Printing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DevExpress.XtraPrinting;
using DevExpress.Drawing.Printing;
using DevExpress.XtraReports.UI;
using MMNextPOS.Application.Services;
using MMNextPOS.Domain.Models;

namespace MMNextPOS.WinForms.Reports
{
    /// <summary>
    /// Barcode Labels Report - prints barcode labels for products.
    /// Supports multiple labels per page (Avery 5160 / 2-5/8" x 1" or custom sizes).
    /// </summary>
    public class BarcodeLabelsReport : BaseReport
    {

        // Controls
        private XRLabel _filterLabel = null!;
        private XRTable _labelsTable = null!;

        private List<Product> _products = new();
        private int _labelsPerRow = 3;
        private int _labelWidth = 240; // hundredths of inch (2.4")
        private int _labelHeight = 100; // hundredths of inch (1.0")
        private int _horizontalGap = 5;
        private int _verticalGap = 5;

        public BarcodeLabelsReport(IProductService productService)
        {
            _productService = productService ?? throw new ArgumentNullException(nameof(productService));

            InitializeReport();
        }

        private void InitializeReport()
        {
            Name = "rptBarcodeLabels";
            DisplayName = "Barcode Labels";
            PageWidth = 827;
            PageHeight = 1169;
            Margins = new Margins(25, 25, 25, 25); // Tight margins for labels
            PaperKind = DXPaperKind.A4;
            Font = new Font("Segoe UI", 8);

            var headerBand = new ReportHeaderBand { HeightF = 60 };
            var detailBand = new DetailBand { HeightF = 110 }; // Height of one label row
            var pageFooterBand = new PageFooterBand { HeightF = 30 };

            Bands.Add(headerBand);
            Bands.Add(detailBand);
            Bands.Add(pageFooterBand);

            BuildHeader(headerBand);
            BuildDetail(detailBand);
            BuildPageFooter(pageFooterBand);
        }

        private void BuildHeader(ReportHeaderBand band)
        {
            float yPos = 10;

            // Title and filter info
            var titleLabel = new XRLabel
            {
                Text = "Barcode Labels",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 122, 204),
                LocationF = new PointF(0, yPos),
                SizeF = new SizeF(300, 25),
                TextAlignment = TextAlignment.MiddleLeft
            };
            band.Controls.Add(titleLabel);

            _filterLabel = new XRLabel
            {
                Text = "Filter: All Products",
                Font = new Font("Segoe UI", 8),
                ForeColor = Color.Gray,
                LocationF = new PointF(320, yPos + 5),
                SizeF = new SizeF(450, 20),
                TextAlignment = TextAlignment.MiddleRight
            };
            band.Controls.Add(_filterLabel);
        }

        private void BuildDetail(DetailBand band)
        {
            // Create a table to hold labels in a grid
            _labelsTable = new XRTable
            {
                LocationF = new PointF(0, 0),
                SizeF = new SizeF(777, _labelHeight + _verticalGap),
                Font = new Font("Segoe UI", 7)
            };

            // Create rows for labels
            for (int row = 0; row < 1; row++) // Single row per detail band iteration
            {
                var tableRow = new XRTableRow { HeightF = _labelHeight };
                
                for (int col = 0; col < _labelsPerRow; col++)
                {
                    var cell = new XRTableCell
                    {
                        WidthF = _labelWidth,
                        Borders = BorderSide.All,
                        BorderColor = Color.LightGray,
                        BorderWidth = 1,
                        Padding = new PaddingInfo(5, 5, 5, 5),
                        TextAlignment = TextAlignment.TopLeft
                    };
                    
                    // Label content will be populated in BeforePrint
                    var labelPanel = new XRPanel
                    {
                        LocationF = new PointF(0, 0),
                        SizeF = new SizeF(_labelWidth - 10, _labelHeight - 10),
                        Borders = BorderSide.None
                    };
                    
                    // Product name
                    var nameLabel = new XRLabel
                    {
                        Name = $"lblName_{col}",
                        Font = new Font("Segoe UI", 7, FontStyle.Bold),
                        LocationF = new PointF(0, 0),
                        SizeF = new SizeF(_labelWidth - 10, 20),
                        TextAlignment = TextAlignment.MiddleCenter,
                        WordWrap = true,
                        Multiline = true
                    };
                    labelPanel.Controls.Add(nameLabel);

                    // Barcode
                    var barcode = new XRBarCode
                    {
                        Name = $"barcode_{col}",
                        Symbology = new DevExpress.XtraPrinting.BarCode.Code128Generator(),
                        Module = 1.5f,
                        LocationF = new PointF(5, 22),
                        SizeF = new SizeF(_labelWidth - 20, 40),
                        TextAlignment = TextAlignment.MiddleCenter,
                        AutoModule = true,
                        ShowText = true,
                        Font = new Font("Consolas", 6)
                    };
                    labelPanel.Controls.Add(barcode);

                    // SKU
                    var skuLabel = new XRLabel
                    {
                        Name = $"lblSku_{col}",
                        Font = new Font("Segoe UI", 6),
                        ForeColor = Color.Gray,
                        LocationF = new PointF(0, 65),
                        SizeF = new SizeF(_labelWidth - 10, 15),
                        TextAlignment = TextAlignment.MiddleCenter
                    };
                    labelPanel.Controls.Add(skuLabel);

                    // Price
                    var priceLabel = new XRLabel
                    {
                        Name = $"lblPrice_{col}",
                        Font = new Font("Segoe UI", 8, FontStyle.Bold),
                        LocationF = new PointF(0, 80),
                        SizeF = new SizeF(_labelWidth - 10, 15),
                        TextAlignment = TextAlignment.MiddleCenter
                    };
                    labelPanel.Controls.Add(priceLabel);

                    cell.Controls.Add(labelPanel);
                    tableRow.Cells.Add(cell);
                }
                
                _labelsTable.Rows.Add(tableRow);
            }

            band.Controls.Add(_labelsTable);
        }

        private void BuildPageFooter(PageFooterBand band)
        {
            var printDate = new XRLabel
            {
                Text = $"Printed: {DateTime.Now:yyyy-MM-dd HH:mm:ss}",
                Font = new Font("Segoe UI", 6),
                ForeColor = Color.Gray,
                LocationF = new PointF(0, 5),
                SizeF = new SizeF(300, 15),
                TextAlignment = TextAlignment.MiddleLeft
            };
            band.Controls.Add(printDate);
        }

        /// <summary>
        /// Populates the report with product data.
        /// </summary>
        public async Task PopulateAsync(
            List<int>? productIds = null, 
            int? categoryId = null, 
            bool activeOnly = true, 
            CancellationToken cancellationToken = default)
        {
            _products = (await _productService.GetAllAsync(cancellationToken)).ToList();

            if (activeOnly)
                _products = _products.Where(p => p.IsActive && !p.IsDeleted).ToList();

            if (categoryId.HasValue)
                _products = _products.Where(p => p.CategoryId == categoryId.Value).ToList();

            if (productIds != null && productIds.Any())
                _products = _products.Where(p => productIds.Contains(p.Id)).ToList();

            _filterLabel.Text = $"Products: {_products.Count} | Labels per row: {_labelsPerRow}";
        }

        protected override void OnBeforePrint(System.ComponentModel.CancelEventArgs e)
        {
            base.OnBeforePrint(e);

            // This is a complex report that needs custom data binding
            // For label reports, we typically use a different approach
            // This would be implemented with a custom data source
        }

        /// <summary>
        /// Generates barcode labels for multiple products.
        /// Note: This requires custom data binding for proper label layout.
        /// </summary>
        public async Task<byte[]> GenerateBarcodeLabelsAsync(
            List<int>? productIds = null,
            int? categoryId = null,
            bool activeOnly = true,
            CancellationToken cancellationToken = default)
        {
            await PopulateAsync(productIds, categoryId, activeOnly, cancellationToken);
            
            using var stream = new System.IO.MemoryStream();
            this.ExportToPdf(stream);
            return stream.ToArray();
        }
    }
}