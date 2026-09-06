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
    /// List page for Email Settings entity using the generic ListPage base class.
    /// </summary>
    public partial class EmailSettingsListPage : ListPage<EmailSetting, IEmailSettingService>
    {
        public EmailSettingsListPage(IEmailSettingService service, IServiceProvider serviceProvider)
            : base(service, serviceProvider)
        {
        }

        protected override string GetPageTitle() => "Email Settings";

        protected override void ConfigureColumns(GridView view)
        {
            view.Columns.Clear();
            view.Columns.AddRange(new[]
            {
                new GridColumn { FieldName = "Id", Caption = "ID", Width = 60, Visible = true },
                new GridColumn { FieldName = "SmtpHost", Caption = "SMTP Host", Width = 200, Visible = true },
                new GridColumn { FieldName = "SmtpPort", Caption = "Port", Width = 80, Visible = true },
                new GridColumn { FieldName = "SmtpUsername", Caption = "Username", Width = 150, Visible = true },
                new GridColumn { FieldName = "FromAddress", Caption = "From Address", Width = 200, Visible = true },
                new GridColumn { FieldName = "FromName", Caption = "From Name", Width = 150, Visible = true },
                new GridColumn { FieldName = "EnableTls", Caption = "TLS", Width = 60, Visible = true },
                new GridColumn { FieldName = "IsActive", Caption = "Active", Width = 70, Visible = true }
            });
        }

        protected override async Task<IEnumerable<EmailSetting>> GetItemsAsync(CancellationToken cancellationToken)
        {
            return await _service.GetAllAsync(cancellationToken);
        }

        protected override async Task OnEditAsync(EmailSetting entity)
        {
            // TODO: Implement EmailSettingEditForm
            await Task.CompletedTask;
        }

        protected override async Task DeleteAsync(int id, CancellationToken cancellationToken)
        {
            await _service.DeleteAsync(id, cancellationToken);
        }
    }
}
