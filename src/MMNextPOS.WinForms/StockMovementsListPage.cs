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
    /// List page for Stock Movements entity using the IInventoryService.
    /// </summary>
    public partial class StockMovementsListPage : ListPage<StockMovement, IInventoryService>
    {
        public StockMovementsListPage(IInventoryService service, IServiceProvider serviceProvider)
            : base(service, serviceProvider)
        {
        }

        protected override string GetPageTitle() => "Stock Movements";

        protected override void ConfigureColumns(GridView view)
        {
            view.Columns.Clear();
            view.Columns.AddRange(new[]
            {
                new GridColumn { FieldName = "Id", Caption = "ID", Width = 60, Visible = true },
                new GridColumn { FieldName = "MovementNo", Caption = "Movement #", Width = 150, Visible = true },
                new GridColumn { FieldName = "MovementType", Caption = "Type", Width = 120, Visible = true },
                new GridColumn { FieldName = "MovementDate", Caption = "Date", Width = 120, Visible = true, DisplayFormat = { FormatString = "g", FormatType = DevExpress.Utils.FormatType.DateTime } },
                new GridColumn { FieldName = "LocationId", Caption = "Location", Width = 100, Visible = true },
                new GridColumn { FieldName = "SupplierId", Caption = "Supplier", Width = 100, Visible = true },
                new GridColumn { FieldName = "CustomerId", Caption = "Customer", Width = 100, Visible = true },
                new GridColumn { FieldName = "Reason", Caption = "Reason", Width = 200, Visible = true },
                new GridColumn { FieldName = "Status", Caption = "Status", Width = 100, Visible = true }
            });
        }

        protected override async Task<IEnumerable<StockMovement>> GetItemsAsync(CancellationToken cancellationToken)
        {
            return await _service.GetStockMovementsAsync(null, null, cancellationToken);
        }

        protected override async Task OnNewAsync()
        {
            using var form = CreateEditForm(new StockMovement());
            if (form.ShowDialog(this) == DialogResult.OK)
            {
                await LoadAsync();
            }
        }

        protected override async Task OnEditAsync(StockMovement entity)
        {
            using var form = CreateEditForm(entity);
            if (form.ShowDialog(this) == DialogResult.OK)
            {
                await LoadAsync();
            }
        }

        private StockMovementEditForm CreateEditForm(StockMovement movement)
        {
            return new StockMovementEditForm(
                _serviceProvider.GetRequiredService<IInventoryService>(),
                _serviceProvider.GetRequiredService<IProductService>(),
                _serviceProvider.GetRequiredService<ICustomerService>(),
                _serviceProvider.GetRequiredService<ISupplierService>(),
                movement);
        }

        protected override async Task DeleteAsync(int id, CancellationToken cancellationToken)
        {
            await _service.DeleteStockMovementAsync(id, cancellationToken);
        }
    }
}
