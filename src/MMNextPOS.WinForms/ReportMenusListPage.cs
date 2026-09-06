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
    /// List page for Report Menus entity using the generic ListPage base class.
    /// </summary>
    public partial class ReportMenusListPage : ListPage<ReportMenus, IReportMenusService>
    {
        public ReportMenusListPage(IReportMenusService service, IServiceProvider serviceProvider)
            : base(service, serviceProvider)
        {
        }

        protected override string GetPageTitle() => "Report Menus";

        protected override void ConfigureColumns(GridView view)
        {
            view.Columns.Clear();
            view.Columns.AddRange(new[]
            {
                new GridColumn { FieldName = "Id", Caption = "ID", Width = 60, Visible = true },
                new GridColumn { FieldName = "Code", Caption = "Code", Width = 100, Visible = true },
                new GridColumn { FieldName = "Name", Caption = "Name", Width = 200, Visible = true },
                new GridColumn { FieldName = "ParentCode", Caption = "Parent Code", Width = 100, Visible = true },
                new GridColumn { FieldName = "FormName", Caption = "Form Name", Width = 150, Visible = true },
                new GridColumn { FieldName = "AssemblyName", Caption = "Assembly", Width = 150, Visible = true },
                new GridColumn { FieldName = "IconName", Caption = "Icon", Width = 100, Visible = true },
                new GridColumn { FieldName = "DisplayOrder", Caption = "Display Order", Width = 100, Visible = true },
                new GridColumn { FieldName = "IsVisible", Caption = "Visible", Width = 70, Visible = true },
                new GridColumn { FieldName = "IsReport", Caption = "Is Report", Width = 80, Visible = true },
                new GridColumn { FieldName = "ReportFileName", Caption = "Report File", Width = 150, Visible = true }
            });
        }

        protected override async Task<IEnumerable<ReportMenus>> GetItemsAsync(CancellationToken cancellationToken)
        {
            return await _service.GetAllAsync(cancellationToken);
        }

        protected override async Task OnEditAsync(ReportMenus entity)
        {
            // TODO: Implement ReportMenusEditForm
            await Task.CompletedTask;
        }

        protected override async Task DeleteAsync(int id, CancellationToken cancellationToken)
        {
            await _service.DeleteAsync(id, cancellationToken);
        }
    }
}
