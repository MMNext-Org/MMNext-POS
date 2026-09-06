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
    /// List page for Discounts entity using the generic ListPage base class.
    /// </summary>
    public partial class DiscountsListPage : ListPage<Discount, IDiscountService>
    {
        public DiscountsListPage(IDiscountService service, IServiceProvider serviceProvider)
            : base(service, serviceProvider)
        {
        }

        protected override string GetPageTitle() => "Discounts";

        protected override void ConfigureColumns(GridView view)
        {
            view.Columns.Clear();
            view.Columns.AddRange(new[]
            {
                new GridColumn { FieldName = "Id", Caption = "ID", Width = 60, Visible = true },
                new GridColumn { FieldName = "Code", Caption = "Code", Width = 100, Visible = true },
                new GridColumn { FieldName = "Name", Caption = "Name", Width = 200, Visible = true },
                new GridColumn { FieldName = "Rate", Caption = "Rate", Width = 100, Visible = true, DisplayFormat = { FormatString = "p2", FormatType = DevExpress.Utils.FormatType.Numeric } },
                new GridColumn { FieldName = "MinimumAmount", Caption = "Min Amount", Width = 100, Visible = true, DisplayFormat = { FormatString = "c2", FormatType = DevExpress.Utils.FormatType.Numeric } },
                new GridColumn { FieldName = "MaximumAmount", Caption = "Max Amount", Width = 100, Visible = true, DisplayFormat = { FormatString = "c2", FormatType = DevExpress.Utils.FormatType.Numeric } },
                new GridColumn { FieldName = "IsActive", Caption = "Active", Width = 70, Visible = true },
                new GridColumn { FieldName = "DisplayOrder", Caption = "Display Order", Width = 100, Visible = true }
            });
        }

        protected override async Task<IEnumerable<Discount>> GetItemsAsync(CancellationToken cancellationToken)
        {
            return await _service.GetAllAsync(cancellationToken);
        }

        protected override async Task OnEditAsync(Discount entity)
        {
            // TODO: Implement DiscountEditForm
            await Task.CompletedTask;
        }

        protected override async Task DeleteAsync(int id, CancellationToken cancellationToken)
        {
            await _service.DeleteAsync(id, cancellationToken);
        }
    }
}
