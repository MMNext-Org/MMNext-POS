using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using DevExpress.XtraPrinting;
using DevExpress.XtraReports.UI;
using DevExpress.Drawing.Printing;
using MMNextPOS.Application.Services;
using MMNextPOS.Domain.Models;

namespace MMNextPOS.WinForms.Reports
{
    /// <summary>
    /// Base class for all MMNextPOS reports with common styling and helper methods.
    /// </summary>
    public abstract class BaseReport : XtraReport
    {
        // Service fields - initialized via InitializeServices()
        protected ISalesService? _salesService;
        protected IProductService? _productService;
        protected ICustomerService? _customerService;
        protected ISupplierService? _supplierService;
        protected IInventoryService? _inventoryService;
        protected IOutstandingService? _outstandingService;
        protected IPurchaseService? _purchaseService;
        protected ISettingService? _settingService;
        protected ILocationService? _locationService;
        protected IReportService? _reportService;

        protected BaseReport()
        {
            // Default page settings
            PageWidth = 827;  // A4 width in hundredths of inch (8.27")
            PageHeight = 1169; // A4 height in hundredths of inch (11.69")
            Margins = new Margins(50, 50, 50, 50);
            PaperKind = DXPaperKind.A4;
            
            // Default fonts
            Font = new Font("Segoe UI", 9.75f);
        }

        /// <summary>
        /// Initializes the report with DI services.
        /// </summary>
        public virtual void InitializeServices(IServiceProvider serviceProvider)
        {
        }

        /// <summary>
        /// Creates a standard company header for the report.
        /// </summary>
        protected XRControl CreateCompanyHeader(float top = 0, float width = 727)
        {
            var panel = new XRPanel
            {
                LocationF = new PointF(0, top),
                SizeF = new SizeF(width, 80),
                Borders = BorderSide.None
            };

            // Company name
            var companyLabel = new XRLabel
            {
                Text = "MMNext POS",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 122, 204),
                LocationF = new PointF(0, 5),
                SizeF = new SizeF(width, 35),
                TextAlignment = TextAlignment.MiddleLeft
            };
            panel.Controls.Add(companyLabel);

            // Company details (address, phone, etc.)
            var detailsLabel = new XRLabel
            {
                Text = "Modern Point of Sale System",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.Gray,
                LocationF = new PointF(0, 40),
                SizeF = new SizeF(width, 20),
                TextAlignment = TextAlignment.MiddleLeft
            };
            panel.Controls.Add(detailsLabel);

            // Report generated date
            var dateLabel = new XRLabel
            {
                Text = $"Printed: {DateTime.Now:yyyy-MM-dd HH:mm:ss}",
                Font = new Font("Segoe UI", 8),
                ForeColor = Color.Gray,
                LocationF = new PointF(width - 200, 60),
                SizeF = new SizeF(190, 15),
                TextAlignment = TextAlignment.MiddleRight
            };
            panel.Controls.Add(dateLabel);

            return panel;
        }

        /// <summary>
        /// Creates a standard report title.
        /// </summary>
        protected XRLabel CreateReportTitle(string title, float top, float width = 727)
        {
            return new XRLabel
            {
                Text = title,
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.FromArgb(51, 51, 51),
                LocationF = new PointF(0, top),
                SizeF = new SizeF(width, 30),
                TextAlignment = TextAlignment.MiddleCenter
            };
        }

        /// <summary>
        /// Creates a parameter display section.
        /// </summary>
        protected XRPanel CreateParameterSection(Dictionary<string, object> parameters, float top, float width = 727)
        {
            var panel = new XRPanel
            {
                LocationF = new PointF(0, top),
                SizeF = new SizeF(width, parameters.Count * 22 + 10),
                Borders = BorderSide.All,
                BorderColor = Color.LightGray,
                BorderWidth = 1
            };

            int yPos = 5;
            foreach (var param in parameters)
            {
                var label = new XRLabel
                {
                    Text = $"{param.Key}: {param.Value?.ToString() ?? "N/A"}",
                    Font = new Font("Segoe UI", 9),
                    LocationF = new PointF(10, yPos),
                    SizeF = new SizeF(width - 20, 20),
                    TextAlignment = TextAlignment.MiddleLeft
                };
                panel.Controls.Add(label);
                yPos += 22;
            }

            return panel;
        }

        /// <summary>
        /// Creates a standard detail row with borders.
        /// </summary>
        protected XRTableRow CreateDetailRow(string[] columns, Font? font = null)
        {
            var row = new XRTableRow { HeightF = 25 };
            var tableFont = font ?? new Font("Segoe UI", 9);

            foreach (var col in columns)
            {
                var cell = new XRTableCell
                {
                    Text = col,
                    Font = tableFont,
                    Borders = BorderSide.Left | BorderSide.Right | BorderSide.Bottom,
                    BorderColor = Color.LightGray,
                    BorderWidth = 1,
                    Padding = new PaddingInfo(5, 5, 3, 3),
                    TextAlignment = TextAlignment.MiddleLeft
                };
                row.Cells.Add(cell);
            }

            return row;
        }

        /// <summary>
        /// Creates a header row for tables.
        /// </summary>
        protected XRTableRow CreateHeaderRow(string[] columns, Font? font = null)
        {
            var row = new XRTableRow { HeightF = 30 };
            var tableFont = font ?? new Font("Segoe UI", 9, FontStyle.Bold);

            foreach (var col in columns)
            {
                var cell = new XRTableCell
                {
                    Text = col,
                    Font = tableFont,
                    BackColor = Color.FromArgb(0, 122, 204),
                    ForeColor = Color.White,
                    Borders = BorderSide.All,
                    BorderColor = Color.White,
                    BorderWidth = 1,
                    Padding = new PaddingInfo(5, 5, 3, 3),
                    TextAlignment = TextAlignment.MiddleCenter
                };
                row.Cells.Add(cell);
            }

            return row;
        }

        /// <summary>
        /// Creates a totals row for tables.
        /// </summary>
        protected XRTableRow CreateTotalRow(string[] columns, Font? font = null)
        {
            var row = new XRTableRow { HeightF = 30 };
            var tableFont = font ?? new Font("Segoe UI", 9, FontStyle.Bold);

            foreach (var col in columns)
            {
                var cell = new XRTableCell
                {
                    Text = col,
                    Font = tableFont,
                    BackColor = Color.FromArgb(240, 240, 240),
                    ForeColor = Color.FromArgb(51, 51, 51),
                    Borders = BorderSide.All,
                    BorderColor = Color.LightGray,
                    BorderWidth = 1,
                    Padding = new PaddingInfo(5, 5, 3, 3),
                    TextAlignment = TextAlignment.MiddleRight
                };
                row.Cells.Add(cell);
            }

            return row;
        }

        /// <summary>
        /// Formats currency values.
        /// </summary>
        protected string FormatCurrency(decimal value, string currencySymbol = "K")
        {
            return $"{currencySymbol} {value:N2}";
        }

        /// <summary>
        /// Formats date values.
        /// </summary>
        protected string FormatDate(DateTime? date, string format = "yyyy-MM-dd")
        {
            return date?.ToString(format) ?? "";
        }

        /// <summary>
        /// Formats date and time values.
        /// </summary>
        protected string FormatDateTime(DateTime? dateTime, string format = "yyyy-MM-dd HH:mm")
        {
            return dateTime?.ToString(format) ?? "";
        }

        /// <summary>
        /// Creates an info row with a label and value label (for footer summaries).
        /// </summary>
        protected XRTableRow CreateInfoRow(string label, XRLabel valueLabel)
        {
            var row = new XRTableRow { HeightF = 25 };
            
            var labelCell = new XRTableCell
            {
                Text = label,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Borders = BorderSide.None,
                TextAlignment = TextAlignment.MiddleRight,
                Padding = new PaddingInfo(5, 10, 3, 3),
                WidthF = 150
            };
            
            var valueCell = new XRTableCell
            {
                Controls = { valueLabel },
                Borders = BorderSide.None,
                TextAlignment = TextAlignment.MiddleRight,
                Padding = new PaddingInfo(5, 5, 3, 3)
            };
            
            row.Cells.Add(labelCell);
            row.Cells.Add(valueCell);
            return row;
        }

        /// <summary>
        /// Creates a total row with a label and value label (for footer summaries).
        /// </summary>
        protected XRTableRow CreateTotalRow(string label, XRLabel valueLabel)
        {
            var row = new XRTableRow { HeightF = 30 };
            
            var labelCell = new XRTableCell
            {
                Text = label,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                BackColor = Color.FromArgb(240, 240, 240),
                Borders = BorderSide.None,
                TextAlignment = TextAlignment.MiddleRight,
                Padding = new PaddingInfo(5, 10, 5, 5),
                WidthF = 150
            };
            
            var valueCell = new XRTableCell
            {
                Controls = { valueLabel },
                BackColor = Color.FromArgb(240, 240, 240),
                Borders = BorderSide.None,
                TextAlignment = TextAlignment.MiddleRight,
                Padding = new PaddingInfo(5, 5, 5, 5)
            };
            
            row.Cells.Add(labelCell);
            row.Cells.Add(valueCell);
            return row;
        }

        /// <summary>
        /// Creates an info row for parameter tables (label + value).
        /// </summary>
        protected XRTableRow CreateInfoRow(string label, XRLabel valueLabel, float labelWidth = 120)
        {
            var row = new XRTableRow { HeightF = 25 };
            
            var labelCell = new XRTableCell
            {
                Text = label,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Borders = BorderSide.None,
                TextAlignment = TextAlignment.MiddleRight,
                Padding = new PaddingInfo(5, 10, 3, 3),
                WidthF = labelWidth
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
    }
}