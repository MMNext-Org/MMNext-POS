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
    /// List page for SalesReturns entity using the generic ListPage base class.
    /// </summary>
    public partial class SalesReturnsListPage : ListPage<SalesReturn, ISalesReturnService>
    {
        public SalesReturnsListPage(ISalesReturnService service, IServiceProvider serviceProvider)
            : base(service, serviceProvider)
        {
        }

        protected override string GetPageTitle() => "Sales Returns";

        protected override void ConfigureColumns(GridView view)
        {
            view.Columns.Clear();
            view.Columns.AddRange(new[]
            {
                new GridColumn { FieldName = "Id", Caption = "ID", Width = 60, Visible = true },
                new GridColumn { FieldName = "ReturnNo", Caption = "Return #", Width = 120, Visible = true },
                new GridColumn { FieldName = "SaleId", Caption = "Original Sale", Width = 100, Visible = true },
                new GridColumn { FieldName = "CustomerId", Caption = "Customer", Width = 100, Visible = true },
                new GridColumn { FieldName = "ReturnDate", Caption = "Date", Width = 120, Visible = true, DisplayFormat = { FormatString = "g", FormatType = DevExpress.Utils.FormatType.DateTime } },
                new GridColumn { FieldName = "TotalAmount", Caption = "Amount", Width = 120, Visible = true, DisplayFormat = { FormatString = "c2", FormatType = DevExpress.Utils.FormatType.Numeric } },
                new GridColumn { FieldName = "Reason", Caption = "Reason", Width = 200, Visible = true },
                new GridColumn { FieldName = "Status", Caption = "Status", Width = 100, Visible = true }
            });
        }

        protected override async Task<IEnumerable<SalesReturn>> GetItemsAsync(CancellationToken cancellationToken)
        {
            return await _service.GetAllAsync(cancellationToken);
        }

        protected override async Task OnEditAsync(SalesReturn entity)
        {
            // TODO: Implement SalesReturnEditForm
            await Task.CompletedTask;
        }

        protected override async Task DeleteAsync(int id, CancellationToken cancellationToken)
        {
            await _service.DeleteAsync(id, cancellationToken);
        }
    }
}
