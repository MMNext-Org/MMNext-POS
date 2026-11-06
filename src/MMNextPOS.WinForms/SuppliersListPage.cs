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
    /// List page for Suppliers entity using the generic ListPage base class.
    /// </summary>
    public partial class SuppliersListPage : ListPage<Supplier, ISupplierService>
    {
        public SuppliersListPage(ISupplierService service, IServiceProvider serviceProvider)
            : base(service, serviceProvider)
        {
        }

        /// <summary>
        /// Adds the "Import CSV" toolbar button (Admin/Manager only - controlled by MainForm).
        /// Opens the bulk supplier import dialog and reloads the grid on success.
        /// </summary>
        public void AddImportButton()
        {
            AppendToolbarButton("Import CSV", async (s, e) => await OpenImportDialogAsync());
        }

        private async Task OpenImportDialogAsync()
        {
            using var dialog = new SupplierImportDialog(_service);
            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
                await LoadAsync();
            }
        }

        protected override string GetPageTitle() => "Suppliers";

        protected override void ConfigureColumns(GridView view)
        {
            view.Columns.Clear();
            view.Columns.AddRange(new[]
            {
                new GridColumn { FieldName = "Id", Caption = "ID", Width = 60, Visible = true },
                new GridColumn { FieldName = "Code", Caption = "Code", Width = 100, Visible = true },
                new GridColumn { FieldName = "Name", Caption = "Name", Width = 200, Visible = true },
                new GridColumn { FieldName = "ContactPerson", Caption = "Contact Person", Width = 150, Visible = true },
                new GridColumn { FieldName = "Phone", Caption = "Phone", Width = 120, Visible = true },
                new GridColumn { FieldName = "Email", Caption = "Email", Width = 180, Visible = true },
                new GridColumn { FieldName = "City", Caption = "City", Width = 120, Visible = true },
                new GridColumn { FieldName = "CreditLimit", Caption = "Credit Limit", Width = 120, Visible = true, DisplayFormat = { FormatString = "c2", FormatType = DevExpress.Utils.FormatType.Numeric } },
                new GridColumn { FieldName = "PaymentTermDays", Caption = "Payment Terms (Days)", Width = 120, Visible = true },
                new GridColumn { FieldName = "IsActive", Caption = "Active", Width = 70, Visible = true }
            });
        }

        protected override async Task<IEnumerable<Supplier>> GetItemsAsync(CancellationToken cancellationToken)
        {
            return await _service.GetAllAsync(cancellationToken);
        }

        protected override async Task OnNewAsync()
        {
            using var dialog = _serviceProvider.GetRequiredService<SupplierEditForm>();
            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
                var entity = new Supplier();
                dialog.SaveEntityData(entity);
                await _service.AddAsync(entity, CancellationToken);
                await LoadAsync();
            }
        }

        protected override async Task OnEditAsync(Supplier entity)
        {
            using var dialog = _serviceProvider.GetRequiredService<SupplierEditForm>();
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
