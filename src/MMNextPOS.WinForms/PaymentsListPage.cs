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
    /// List page for Payments entity using the generic ListPage base class.
    /// </summary>
    public partial class PaymentsListPage : ListPage<Payment, IPaymentService>
    {
        public PaymentsListPage(IPaymentService service, IServiceProvider serviceProvider)
            : base(service, serviceProvider)
        {
        }

        protected override string GetPageTitle() => "Payments";

        protected override void ConfigureColumns(GridView view)
        {
            view.Columns.Clear();
            view.Columns.AddRange(new[]
            {
                new GridColumn { FieldName = "Id", Caption = "ID", Width = 60, Visible = true },
                new GridColumn { FieldName = "PaymentNo", Caption = "Payment #", Width = 120, Visible = true },
                new GridColumn { FieldName = "PaymentType", Caption = "Type", Width = 100, Visible = true },
                new GridColumn { FieldName = "Method", Caption = "Method", Width = 100, Visible = true },
                new GridColumn { FieldName = "Amount", Caption = "Amount", Width = 120, Visible = true, DisplayFormat = { FormatString = "c2", FormatType = DevExpress.Utils.FormatType.Numeric } },
                new GridColumn { FieldName = "PaymentDate", Caption = "Date", Width = 120, Visible = true, DisplayFormat = { FormatString = "g", FormatType = DevExpress.Utils.FormatType.DateTime } },
                new GridColumn { FieldName = "SaleId", Caption = "Sale", Width = 100, Visible = true },
                new GridColumn { FieldName = "PurchaseId", Caption = "Purchase", Width = 100, Visible = true },
                new GridColumn { FieldName = "CustomerId", Caption = "Customer", Width = 100, Visible = true },
                new GridColumn { FieldName = "SupplierId", Caption = "Supplier", Width = 100, Visible = true },
                new GridColumn { FieldName = "Status", Caption = "Status", Width = 100, Visible = true }
            });
        }

        protected override async Task<IEnumerable<Payment>> GetItemsAsync(CancellationToken cancellationToken)
        {
            return await _service.GetAllAsync(cancellationToken);
        }

        protected override async Task OnEditAsync(Payment entity)
        {
            // Open the payment edit form
            using var form = new PaymentEditForm(entity);
            if (form.ShowDialog() == DialogResult.OK)
            {
                await _service.UpdateAsync(entity, CancellationToken);
            }
        }

        protected override async Task DeleteAsync(int id, CancellationToken cancellationToken)
        {
            await _service.DeleteAsync(id, cancellationToken);
        }
    }
}
