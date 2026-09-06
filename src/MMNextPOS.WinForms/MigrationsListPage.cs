using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraGrid.Views.Grid;
using Microsoft.Extensions.DependencyInjection;
using MMNextPOS.Application.Services;
using MMNextPOS.Domain.Models;

namespace MMNextPOS.WinForms
{
    /// <summary>
    /// List page for data migration definitions with create/edit/delete and
    /// run/cancel/history actions inside the migration editor.
    /// </summary>
    public class MigrationsListPage : ListPage<DataMigration, IMigrationService>
    {
        public MigrationsListPage(IMigrationService service, IServiceProvider serviceProvider)
            : base(service, serviceProvider)
        {
        }

        protected override string GetPageTitle() => "Data Migrations";

        protected override void ConfigureColumns(GridView view)
        {
            view.Columns.Clear();
            view.Columns.AddRange(new[]
            {
                new GridColumn { FieldName = "Id", Caption = "ID", Width = 50, Visible = true },
                new GridColumn { FieldName = "Name", Caption = "Name", Width = 170, Visible = true },
                new GridColumn { FieldName = "SourceType", Caption = "Source", Width = 90, Visible = true },
                new GridColumn { FieldName = "TargetType", Caption = "Target", Width = 90, Visible = true },
                new GridColumn { FieldName = "ScheduleType", Caption = "Schedule", Width = 90, Visible = true },
                new GridColumn { FieldName = "Status", Caption = "Status", Width = 90, Visible = true },
                new GridColumn { FieldName = "ProcessedRecords", Caption = "Processed", Width = 90, Visible = true },
                new GridColumn { FieldName = "FailedRecords", Caption = "Failed", Width = 70, Visible = true },
                new GridColumn { FieldName = "LastRunAt", Caption = "Last Run", Width = 130, Visible = true },
                new GridColumn { FieldName = "IsActive", Caption = "Active", Width = 60, Visible = true }
            });
        }

        protected override async Task<IEnumerable<DataMigration>> GetItemsAsync(CancellationToken cancellationToken)
        {
            return await _service.GetAllAsync(cancellationToken);
        }

        protected override async Task OnNewAsync()
        {
            var entity = new DataMigration();
            using var form = new MigrationEditForm(entity, _service);
            if (form.ShowDialog(this) != DialogResult.OK)
                return;

            form.SaveEntityData(entity);
            await RunAsync(async ct =>
            {
                await _service.AddAsync(entity, ct);
                await LoadAsync(ct);
            });
        }

        protected override async Task OnEditAsync(DataMigration entity)
        {
            using var form = new MigrationEditForm(entity, _service);
            if (form.ShowDialog(this) != DialogResult.OK)
                return;

            form.SaveEntityData(entity);
            await RunAsync(async ct =>
            {
                await _service.UpdateAsync(entity, ct);
                await LoadAsync(ct);
            });
        }

        protected override async Task DeleteAsync(int id, CancellationToken cancellationToken)
        {
            await _service.DeleteAsync(id, cancellationToken);
        }
    }
}
