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
    /// List page for Expenses entity using the generic ListPage base class.
    /// </summary>
    public partial class ExpensesListPage : ListPage<Expense, IExpenseService>
    {
        public ExpensesListPage(IExpenseService service, IServiceProvider serviceProvider)
            : base(service, serviceProvider)
        {
        }

        protected override string GetPageTitle() => "Expenses";

        protected override void ConfigureColumns(GridView view)
        {
            view.Columns.Clear();
            view.Columns.AddRange(new[]
            {
                new GridColumn { FieldName = "Id", Caption = "ID", Width = 60, Visible = true },
                new GridColumn { FieldName = "ExpenseNo", Caption = "Expense #", Width = 120, Visible = true },
                new GridColumn { FieldName = "ExpenseTypeId", Caption = "Type", Width = 100, Visible = true },
                new GridColumn { FieldName = "ExpenseDate", Caption = "Date", Width = 120, Visible = true, DisplayFormat = { FormatString = "g", FormatType = DevExpress.Utils.FormatType.DateTime } },
                new GridColumn { FieldName = "Amount", Caption = "Amount", Width = 120, Visible = true, DisplayFormat = { FormatString = "c2", FormatType = DevExpress.Utils.FormatType.Numeric } },
                new GridColumn { FieldName = "PaymentMethod", Caption = "Method", Width = 100, Visible = true },
                new GridColumn { FieldName = "LocationId", Caption = "Location", Width = 100, Visible = true },
                new GridColumn { FieldName = "VendorId", Caption = "Vendor", Width = 100, Visible = true },
                new GridColumn { FieldName = "Description", Caption = "Description", Width = 200, Visible = true },
                new GridColumn { FieldName = "Status", Caption = "Status", Width = 100, Visible = true }
            });
        }

        protected override async Task<IEnumerable<Expense>> GetItemsAsync(CancellationToken cancellationToken)
        {
            return await _service.GetAllAsync(cancellationToken);
        }

        protected override async Task OnEditAsync(Expense entity)
        {
            // TODO: Implement ExpenseEditForm
            await Task.CompletedTask;
        }

        protected override async Task DeleteAsync(int id, CancellationToken cancellationToken)
        {
            await _service.DeleteAsync(id, cancellationToken);
        }
    }
}