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
    /// List page for Stock Transfers entity using the generic repository.
    /// </summary>
    public partial class StockTransfersListPage : ListPage<StockTransfer, IStockTransferRepository>
    {
        public StockTransfersListPage(IStockTransferRepository service, IServiceProvider serviceProvider)
            : base(service, serviceProvider)
        {
        }

        protected override string GetPageTitle() => "Stock Transfers";

        protected override void ConfigureColumns(GridView view)
        {
            view.Columns.Clear();
            view.Columns.AddRange(new[]
            {
                new GridColumn { FieldName = "Id", Caption = "ID", Width = 60, Visible = true },
                new GridColumn { FieldName = "TransferNo", Caption = "Transfer #", Width = 150, Visible = true },
                new GridColumn { FieldName = "FromLocationId", Caption = "From Location", Width = 120, Visible = true },
                new GridColumn { FieldName = "ToLocationId", Caption = "To Location", Width = 120, Visible = true },
                new GridColumn { FieldName = "TransferDate", Caption = "Transfer Date", Width = 120, Visible = true, DisplayFormat = { FormatString = "g", FormatType = DevExpress.Utils.FormatType.DateTime } },
                new GridColumn { FieldName = "ReceivedDate", Caption = "Received Date", Width = 120, Visible = true, DisplayFormat = { FormatString = "g", FormatType = DevExpress.Utils.FormatType.DateTime } },
                new GridColumn { FieldName = "Status", Caption = "Status", Width = 100, Visible = true },
                new GridColumn { FieldName = "Notes", Caption = "Notes", Width = 200, Visible = true }
            });
        }

        protected override async Task<IEnumerable<StockTransfer>> GetItemsAsync(CancellationToken cancellationToken)
        {
            return await _service.GetAllAsync(cancellationToken);
        }

        protected override async Task OnEditAsync(StockTransfer entity)
        {
            // TODO: Implement StockTransferEditForm
            await Task.CompletedTask;
        }

        protected override async Task DeleteAsync(int id, CancellationToken cancellationToken)
        {
            await _service.DeleteAsync(id, cancellationToken);
        }
    }
}
