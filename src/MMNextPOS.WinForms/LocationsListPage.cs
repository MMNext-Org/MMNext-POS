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
    /// List page for Locations entity using the generic ListPage base class.
    /// </summary>
    public partial class LocationsListPage : ListPage<Location, ILocationService>
    {
        public LocationsListPage(ILocationService service, IServiceProvider serviceProvider)
            : base(service, serviceProvider)
        {
        }

        protected override string GetPageTitle() => "Locations";

        protected override void ConfigureColumns(GridView view)
        {
            view.Columns.Clear();
            view.Columns.AddRange(new[]
            {
                new GridColumn { FieldName = "Id", Caption = "ID", Width = 60, Visible = true },
                new GridColumn { FieldName = "Code", Caption = "Code", Width = 100, Visible = true },
                new GridColumn { FieldName = "Name", Caption = "Name", Width = 200, Visible = true },
                new GridColumn { FieldName = "Address", Caption = "Address", Width = 200, Visible = true },
                new GridColumn { FieldName = "City", Caption = "City", Width = 120, Visible = true },
                new GridColumn { FieldName = "Phone", Caption = "Phone", Width = 120, Visible = true },
                new GridColumn { FieldName = "Email", Caption = "Email", Width = 180, Visible = true },
                new GridColumn { FieldName = "IsActive", Caption = "Active", Width = 70, Visible = true },
                new GridColumn { FieldName = "IsHeadquarter", Caption = "HQ", Width = 60, Visible = true },
                new GridColumn { FieldName = "DisplayOrder", Caption = "Display Order", Width = 100, Visible = true }
            });
        }

        protected override async Task<IEnumerable<Location>> GetItemsAsync(CancellationToken cancellationToken)
        {
            return await _service.GetAllAsync(cancellationToken);
        }

        protected override async Task OnEditAsync(Location entity)
        {
            // TODO: Implement LocationEditForm
            await Task.CompletedTask;
        }

        protected override async Task DeleteAsync(int id, CancellationToken cancellationToken)
        {
            await _service.DeleteAsync(id, cancellationToken);
        }
    }
}
