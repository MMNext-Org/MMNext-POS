using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraGrid.Views.Grid;
using Microsoft.Extensions.DependencyInjection;
using MMNextPOS.Application.Services;
using MMNextPOS.Domain.Models;

namespace MMNextPOS.WinForms
{
    /// <summary>
    /// List page for SaleTemps entity using the generic ListPage base class.
    /// </summary>
    public partial class SaleTempsListPage : ListPage<SaleTemp, ISaleTempService>
    {
        public SaleTempsListPage(ISaleTempService service, IServiceProvider serviceProvider)
            : base(service, serviceProvider)
        {
        }

        protected override string GetPageTitle() => "Sale Temporaries (Drafts)";

        protected override void ConfigureColumns(GridView view)
        {
            view.Columns.Clear();
            view.Columns.AddRange(new[]
            {
                new GridColumn { FieldName = "Id", Caption = "ID", Width = 60, Visible = true },
                new GridColumn { FieldName = "CustomerName", Caption = "Customer", Width = 180, Visible = true },
                new GridColumn { FieldName = "CustomerPhone", Caption = "Phone", Width = 120, Visible = true },
                new GridColumn { FieldName = "SaleDate", Caption = "Date", Width = 120, Visible = true, DisplayFormat = { FormatString = "g", FormatType = DevExpress.Utils.FormatType.DateTime } },
                new GridColumn { FieldName = "TotalAmount", Caption = "Total", Width = 120, Visible = true, DisplayFormat = { FormatString = "c2", FormatType = DevExpress.Utils.FormatType.Numeric } },
                new GridColumn { FieldName = "DiscountAmount", Caption = "Discount", Width = 100, Visible = true, DisplayFormat = { FormatString = "c2", FormatType = DevExpress.Utils.FormatType.Numeric } },
                new GridColumn { FieldName = "TaxAmount", Caption = "Tax", Width = 100, Visible = true, DisplayFormat = { FormatString = "c2", FormatType = DevExpress.Utils.FormatType.Numeric } },
                new GridColumn { FieldName = "NetAmount", Caption = "Net", Width = 120, Visible = true, DisplayFormat = { FormatString = "c2", FormatType = DevExpress.Utils.FormatType.Numeric } },
                new GridColumn { FieldName = "Status", Caption = "Status", Width = 100, Visible = true },
                new GridColumn { FieldName = "Notes", Caption = "Notes", Width = 200, Visible = true }
            });
        }

        protected override async Task<IEnumerable<SaleTemp>> GetItemsAsync(CancellationToken cancellationToken)
        {
            return await _service.GetAllAsync(cancellationToken);
        }

        protected override async Task OnEditAsync(SaleTemp entity)
        {
            // TODO: Implement SaleTempEditForm
            await Task.CompletedTask;
        }

        protected override async Task DeleteAsync(int id, CancellationToken cancellationToken)
        {
            await _service.DeleteAsync(id, cancellationToken);
        }
    }
}
