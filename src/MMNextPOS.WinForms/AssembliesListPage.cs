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
    /// List page for Assemblies entity using the IInventoryService.
    /// </summary>
    public partial class AssembliesListPage : ListPage<Assembly, IInventoryService>
    {
        public AssembliesListPage(IInventoryService service, IServiceProvider serviceProvider)
            : base(service, serviceProvider)
        {
        }

        protected override string GetPageTitle() => "Assemblies (BOM)";

        protected override void ConfigureColumns(GridView view)
        {
            view.Columns.Clear();
            view.Columns.AddRange(new[]
            {
                new GridColumn { FieldName = "Id", Caption = "ID", Width = 60, Visible = true },
                new GridColumn { FieldName = "AssemblyNo", Caption = "Assembly #", Width = 150, Visible = true },
                new GridColumn { FieldName = "OutputProductId", Caption = "Output Product", Width = 120, Visible = true },
                new GridColumn { FieldName = "OutputQuantity", Caption = "Qty", Width = 80, Visible = true },
                new GridColumn { FieldName = "AssemblyDate", Caption = "Date", Width = 120, Visible = true, DisplayFormat = { FormatString = "g", FormatType = DevExpress.Utils.FormatType.DateTime } },
                new GridColumn { FieldName = "TotalCost", Caption = "Total Cost", Width = 120, Visible = true, DisplayFormat = { FormatString = "c2", FormatType = DevExpress.Utils.FormatType.Numeric } },
                new GridColumn { FieldName = "LocationId", Caption = "Location", Width = 100, Visible = true },
                new GridColumn { FieldName = "Status", Caption = "Status", Width = 100, Visible = true },
                new GridColumn { FieldName = "Notes", Caption = "Notes", Width = 200, Visible = true }
            });
        }

        protected override async Task<IEnumerable<Assembly>> GetItemsAsync(CancellationToken cancellationToken)
        {
            return await _service.GetAssembliesAsync(cancellationToken);
        }

        protected override async Task OnEditAsync(Assembly entity)
        {
            // TODO: Implement AssemblyEditForm
            await Task.CompletedTask;
        }

        protected override async Task DeleteAsync(int id, CancellationToken cancellationToken)
        {
            await _service.DeleteAssemblyAsync(id, cancellationToken);
        }
    }
}