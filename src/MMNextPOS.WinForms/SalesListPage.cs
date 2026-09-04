using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraGrid.Views.Grid;
using MMNextPOS.Application.Services;
using MMNextPOS.Domain.Models;

namespace MMNextPOS.WinForms
{
    /// <summary>
    /// List page for Sales entity using the generic ListPage base class.
    /// </summary>
    public partial class SalesListPage : ListPage<Sale, ISalesService>
    {
        public SalesListPage(ISalesService service, IServiceProvider serviceProvider)
            : base(service, serviceProvider)
        {
        }

        protected override string GetPageTitle() => "Sales";

        protected override void ConfigureColumns(GridView view)
        {
            view.Columns.Clear();
            view.Columns.AddRange(new[]
            {
                new GridColumn { FieldName = "Id", Caption = "Sale #", Width = 80, Visible = true },
                new GridColumn { FieldName = "CustomerName", Caption = "Customer", Width = 200, Visible = true },
                new GridColumn { FieldName = "SaleDate", Caption = "Date", Width = 150, Visible = true },
                new GridColumn { FieldName = "TotalAmount", Caption = "Total", Width = 120, Visible = true }
            });
        }

        protected override async Task<IEnumerable<Sale>> GetItemsAsync(CancellationToken cancellationToken)
        {
            return await _service.GetRecentSalesAsync(100, cancellationToken);
        }

        protected override async Task OnEditAsync(Sale entity)
        {
            // Edit handled by derived forms
        }

        protected override async Task DeleteAsync(int id, CancellationToken cancellationToken)
        {
            // Delete handled by derived forms
        }
    }
}
