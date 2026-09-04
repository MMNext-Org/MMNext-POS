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
using MMNextPOS.Domain.Models;
using MMNextPOS.Infrastructure.Repositories;

namespace MMNextPOS.WinForms
{
    /// <summary>
    /// List page for PurchaseReturns entity using the generic repository.
    /// </summary>
    public partial class PurchaseReturnsListPage : ListPage<PurchaseReturn, IPurchaseReturnRepository>
    {
        public PurchaseReturnsListPage(IPurchaseReturnRepository service, IServiceProvider serviceProvider)
            : base(service, serviceProvider)
        {
        }

        protected override string GetPageTitle() => "Purchase Returns";

        protected override void ConfigureColumns(GridView view)
        {
            view.Columns.Clear();
            view.Columns.AddRange(new[]
            {
                new GridColumn { FieldName = "Id", Caption = "ID", Width = 60, Visible = true },
                new GridColumn { FieldName = "ReturnNo", Caption = "Return #", Width = 120, Visible = true },
                new GridColumn { FieldName = "PurchaseId", Caption = "Original Purchase", Width = 120, Visible = true },
                new GridColumn { FieldName = "SupplierId", Caption = "Supplier", Width = 100, Visible = true },
                new GridColumn { FieldName = "ReturnDate", Caption = "Date", Width = 120, Visible = true, DisplayFormat = { FormatString = "g", FormatType = DevExpress.Utils.FormatType.DateTime } },
                new GridColumn { FieldName = "TotalAmount", Caption = "Amount", Width = 120, Visible = true, DisplayFormat = { FormatString = "c2", FormatType = DevExpress.Utils.FormatType.Numeric } },
                new GridColumn { FieldName = "Reason", Caption = "Reason", Width = 200, Visible = true },
                new GridColumn { FieldName = "Status", Caption = "Status", Width = 100, Visible = true }
            });
        }

        protected override async Task<IEnumerable<PurchaseReturn>> GetItemsAsync(CancellationToken cancellationToken)
        {
            return await _service.GetAllAsync(cancellationToken);
        }

        protected override async Task OnEditAsync(PurchaseReturn entity)
        {
            // TODO: Implement PurchaseReturnEditForm
            await Task.CompletedTask;
        }

        protected override async Task DeleteAsync(int id, CancellationToken cancellationToken)
        {
            await _service.DeleteAsync(id, cancellationToken);
        }
    }
}