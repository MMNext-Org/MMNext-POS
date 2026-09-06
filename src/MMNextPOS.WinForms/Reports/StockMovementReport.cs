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
    /// Stock Movement Journal Report - lists all stock movements with filtering.
    /// </summary>
    public class StockMovementReport : BaseReport
    {

        // Controls
        private XRLabel _periodLabel = null!;
        private XRLabel _filterLabel = null!;
        private XRTable _detailTable = null!;
        private XRLabel _totalInLabel = null!;
        private XRLabel _totalOutLabel = null!;
        private XRLabel _netMovementLabel = null!;

        private List<StockMovement> _movements = new();

        public StockMovementReport(
            IInventoryService inventoryService,
            ILocationService locationService,
            IProductService productService)
        {
            _inventoryService = inventoryService ?? throw new ArgumentNullException(nameof(inventoryService));
            _locationService = locationService ?? throw new ArgumentNullException(nameof(locationService));
            _productService = productService ?? throw new ArgumentNullException(nameof(productService));

            InitializeReport();
        }

        private void InitializeReport()
        {
            Name = "rptStockMovement";
            DisplayName = "Stock Movement Journal";
            PageWidth = 827;
            PageHeight = 1169;
            Margins = new Margins(50, 50, 50, 50);
            PaperKind = DXPaperKind.A4;
            Font = new Font("Segoe UI", 9.75f);

            var headerBand = new ReportHeaderBand { HeightF = 130 };
            var detailBand = new DetailBand { HeightF = 30 };
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
            var titleLabel = CreateReportTitle("Stock Movement Journal", yPos, width);
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
            _filterLabel = new XRLabel { Text = "Filter: All Movements", Font = new Font("Segoe UI", 9) };

            paramTable.Rows.Add(CreateInfoRow("Period:", _periodLabel));
            paramTable.Rows.Add(CreateInfoRow("Filter:", _filterLabel));

            band.Controls.Add(paramTable);
        }


        private void BuildDetail(DetailBand band)
        {
            _detailTable = new XRTable
            {
                LocationF = new PointF(0, 0),
                SizeF = new SizeF(727, 30),
                Font = new Font("Segoe UI", 8)
            };
            _detailTable.Rows.Add(CreateDetailRow(new[]
            {
                "Date", "Type", "Ref #", "Product", "Location", "Qty In", "Qty Out", "Balance", "Reason"
            }));
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

            // Summary
            var summaryTable = new XRTable
            {
                LocationF = new PointF(rightAlign, yPos),
                SizeF = new SizeF(250, 80),
                Font = new Font("Segoe UI", 9)
            };

            _totalInLabel = new XRLabel { Text = "Total In: ", Font = new Font("Segoe UI", 9) };
            _totalOutLabel = new XRLabel { Text = "Total Out: ", Font = new Font("Segoe UI", 9) };
            _netMovementLabel = new XRLabel { Text = "Net Movement: ", Font = new Font("Segoe UI", 9) };

            summaryTable.Rows.Add(CreateInfoRow("Total In:", _totalInLabel));
            summaryTable.Rows.Add(CreateInfoRow("Total Out:", _totalOutLabel));
            summaryTable.Rows.Add(CreateTotalRow("Net Movement:", _netMovementLabel));

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
        /// Populates the report with stock movement data.
        /// </summary>
        public async Task PopulateAsync(
            DateTime fromDate,
            DateTime toDate,
            string? movementType = null,
            int? productId = null,
            int? locationId = null,
            CancellationToken cancellationToken = default)
        {
            // Note: IInventoryService.GetStockMovementsAsync returns all movements;
            // we filter in memory below.
            _movements = (await _inventoryService!.GetStockMovementsAsync(locationId, movementType, cancellationToken) ?? new List<StockMovement>()).ToList()!;

            // Apply filters
            _movements = _movements.Where(m => m.MovementDate >= fromDate && m.MovementDate <= toDate).ToList();

            if (!string.IsNullOrEmpty(movementType))
                _movements = _movements.Where(m => m.MovementType.Equals(movementType, StringComparison.OrdinalIgnoreCase)).ToList();

            if (productId.HasValue)
                _movements = _movements.Where(m => m.ProductId == productId.Value).ToList();

            if (locationId.HasValue)
                _movements = _movements.Where(m => m.LocationId == locationId.Value).ToList();

            // Update header
            _periodLabel.Text = $"Period: {FormatDate(fromDate)} to {FormatDate(toDate)}";
            var filters = new List<string>();
            if (!string.IsNullOrEmpty(movementType)) filters.Add($"Type: {movementType}");
            if (productId.HasValue) filters.Add($"Product ID: {productId.Value}");
            if (locationId.HasValue) filters.Add($"Location ID: {locationId.Value}");
            _filterLabel.Text = $"Filter: {(filters.Any() ? string.Join(", ", filters) : "All Movements")}";

            // Populate detail rows
            int totalIn = 0;
            int totalOut = 0;
            foreach (var movement in _movements.OrderBy(m => m.MovementDate))
            {
                var product = movement.ProductId.HasValue ? await _productService!.GetByIdAsync(movement.ProductId.Value, cancellationToken) : null;
                var location = movement.LocationId.HasValue ? await _locationService!.GetByIdAsync(movement.LocationId.Value, cancellationToken) : null;

                bool isIn = movement.MovementType.Equals("Receive", StringComparison.OrdinalIgnoreCase) ||
                           movement.MovementType.Equals("Purchase", StringComparison.OrdinalIgnoreCase) ||
                           movement.MovementType.Equals("Transfer In", StringComparison.OrdinalIgnoreCase) ||
                           movement.MovementType.Equals("Return", StringComparison.OrdinalIgnoreCase) ||
                           movement.MovementType.Equals("Adjustment", StringComparison.OrdinalIgnoreCase);

                int qtyIn = isIn ? movement.Quantity : 0;
                int qtyOut = !isIn ? movement.Quantity : 0;

                totalIn += qtyIn;
                totalOut += qtyOut;

                var row = CreateDetailRow(new[]
                {
                    FormatDateTime(movement.MovementDate),
                    movement.MovementType,
                    movement.Id.ToString(),
                    product?.Name ?? $"Product #{movement.ProductId ?? 0}",
                    location?.Name ?? "N/A",
                    qtyIn > 0 ? qtyIn.ToString() : "",
                    qtyOut > 0 ? qtyOut.ToString() : "",
                    "N/A", // Running balance would need calculation
                    movement.Reason ?? ""
                });
                _detailTable.Rows.Add(row);
            }

            // Update footer
            _totalInLabel.Text = $"Total In: {totalIn:N0}";
            _totalOutLabel.Text = $"Total Out: {totalOut:N0}";
            var net = totalIn - totalOut;
            _netMovementLabel.Text = $"Net Movement: {net:+#;-#;0}";
            _netMovementLabel.ForeColor = net >= 0 ? Color.Green : Color.Red;
        }

        public async Task<byte[]> GenerateStockMovementAsync(
            DateTime fromDate,
            DateTime toDate,
            string? movementType = null,
            int? productId = null,
            int? locationId = null,
            CancellationToken cancellationToken = default)
        {
            await PopulateAsync(fromDate, toDate, movementType, productId, locationId, cancellationToken);

            using var stream = new System.IO.MemoryStream();
            this.ExportToPdf(stream);
            return stream.ToArray();
        }
    }
}
