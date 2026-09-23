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
    /// List page for Expense Types entity using the generic ListPage base class.
    /// </summary>
    public partial class ExpenseTypesListPage : ListPage<ExpenseType, IExpenseTypeService>
    {
        public ExpenseTypesListPage(IExpenseTypeService service, IServiceProvider serviceProvider)
            : base(service, serviceProvider)
        {
        }

        protected override string GetPageTitle() => "Expense Types";

        protected override void ConfigureColumns(GridView view)
        {
            view.Columns.Clear();
            view.Columns.AddRange(new[]
            {
                new GridColumn { FieldName = "Id", Caption = "ID", Width = 60, Visible = true },
                new GridColumn { FieldName = "Code", Caption = "Code", Width = 100, Visible = true },
                new GridColumn { FieldName = "Name", Caption = "Name", Width = 200, Visible = true },
                new GridColumn { FieldName = "Description", Caption = "Description", Width = 250, Visible = true },
                new GridColumn { FieldName = "IsActive", Caption = "Active", Width = 70, Visible = true },
                new GridColumn { FieldName = "DisplayOrder", Caption = "Display Order", Width = 100, Visible = true }
            });
        }

        protected override async Task<IEnumerable<ExpenseType>> GetItemsAsync(CancellationToken cancellationToken)
        {
            return await _service.GetAllAsync(cancellationToken);
        }

        protected override async Task OnNewAsync()
        {
            using var dialog = _serviceProvider.GetRequiredService<ExpenseTypeEditForm>();
            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
                var entity = new ExpenseType();
                dialog.SaveEntityData(entity);
                await _service.AddAsync(entity, CancellationToken);
                await LoadAsync();
            }
        }

        protected override async Task OnEditAsync(ExpenseType entity)
        {
            using var dialog = _serviceProvider.GetRequiredService<ExpenseTypeEditForm>();
            dialog.LoadEntityData(entity);
            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
                dialog.SaveEntityData(entity);
                await _service.UpdateAsync(entity, CancellationToken);
                await LoadAsync();
            }
        }

        protected override async Task DeleteAsync(int id, CancellationToken cancellationToken)
        {
            await _service.DeleteAsync(id, cancellationToken);
        }
    }
}
