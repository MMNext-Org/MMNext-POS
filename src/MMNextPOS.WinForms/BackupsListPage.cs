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
    /// List page for backup settings with create/edit/delete and run/history/restore
    /// actions inside the backup editor.
    /// </summary>
    public class BackupsListPage : ListPage<BackupSetting, IBackupService>
    {
        public BackupsListPage(IBackupService service, IServiceProvider serviceProvider)
            : base(service, serviceProvider)
        {
        }

        protected override string GetPageTitle() => "Backups";

        protected override void ConfigureColumns(GridView view)
        {
            view.Columns.Clear();
            view.Columns.AddRange(new[]
            {
                new GridColumn { FieldName = "Id", Caption = "ID", Width = 50, Visible = true },
                new GridColumn { FieldName = "Name", Caption = "Name", Width = 170, Visible = true },
                new GridColumn { FieldName = "Frequency", Caption = "Frequency", Width = 90, Visible = true },
                new GridColumn { FieldName = "StorageType", Caption = "Storage", Width = 100, Visible = true },
                new GridColumn { FieldName = "BackupPath", Caption = "Backup Path", Width = 200, Visible = true },
                new GridColumn { FieldName = "RetentionDays", Caption = "Retention (d)", Width = 90, Visible = true },
                new GridColumn { FieldName = "LastRunAt", Caption = "Last Run", Width = 130, Visible = true },
                new GridColumn { FieldName = "LastStatus", Caption = "Last Status", Width = 100, Visible = true },
                new GridColumn { FieldName = "IsActive", Caption = "Active", Width = 60, Visible = true }
            });
        }

        protected override async Task<IEnumerable<BackupSetting>> GetItemsAsync(CancellationToken cancellationToken)
        {
            return await _service.GetAllAsync(cancellationToken);
        }

        protected override async Task OnNewAsync()
        {
            var entity = new BackupSetting();
            using var form = new BackupEditForm(entity, _service);
            if (form.ShowDialog(this) != DialogResult.OK)
                return;

            form.SaveEntityData(entity);
            await RunAsync(async ct =>
            {
                await _service.AddAsync(entity, ct);
                await LoadAsync(ct);
            });
        }

        protected override async Task OnEditAsync(BackupSetting entity)
        {
            using var form = new BackupEditForm(entity, _service);
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