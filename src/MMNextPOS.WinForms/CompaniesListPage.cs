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
    /// List page for Companies entity using the generic ListPage base class.
    /// </summary>
    public partial class CompaniesListPage : ListPage<Company, ICompanyService>
    {
        public CompaniesListPage(ICompanyService service, IServiceProvider serviceProvider)
            : base(service, serviceProvider)
        {
        }

        protected override string GetPageTitle() => "Companies";

        protected override void ConfigureColumns(GridView view)
        {
            view.Columns.Clear();
            view.Columns.AddRange(new[]
            {
                new GridColumn { FieldName = "Id", Caption = "ID", Width = 60, Visible = true },
                new GridColumn { FieldName = "Code", Caption = "Code", Width = 80, Visible = true },
                new GridColumn { FieldName = "Name", Caption = "Name", Width = 200, Visible = true },
                new GridColumn { FieldName = "RegistrationNumber", Caption = "Reg. Number", Width = 150, Visible = true },
                new GridColumn { FieldName = "TaxId", Caption = "Tax ID", Width = 120, Visible = true },
                new GridColumn { FieldName = "City", Caption = "City", Width = 120, Visible = true },
                new GridColumn { FieldName = "Phone", Caption = "Phone", Width = 120, Visible = true },
                new GridColumn { FieldName = "Email", Caption = "Email", Width = 180, Visible = true },
                new GridColumn { FieldName = "IsActive", Caption = "Active", Width = 70, Visible = true }
            });
        }

        protected override async Task<IEnumerable<Company>> GetItemsAsync(CancellationToken cancellationToken)
        {
            return await _service.GetAllAsync(cancellationToken);
        }

        protected override async Task OnEditAsync(Company entity)
        {
            // TODO: Implement CompanyEditForm
            await Task.CompletedTask;
        }

        protected override async Task DeleteAsync(int id, CancellationToken cancellationToken)
        {
            await _service.DeleteAsync(id, cancellationToken);
        }
    }
}
