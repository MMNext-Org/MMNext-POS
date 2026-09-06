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
    /// List page for Languages entity using the generic ListPage base class.
    /// </summary>
    public partial class LanguagesListPage : ListPage<Language, ILanguageService>
    {
        public LanguagesListPage(ILanguageService service, IServiceProvider serviceProvider)
            : base(service, serviceProvider)
        {
        }

        protected override string GetPageTitle() => "Languages";

        protected override void ConfigureColumns(GridView view)
        {
            view.Columns.Clear();
            view.Columns.AddRange(new[]
            {
                new GridColumn { FieldName = "Id", Caption = "ID", Width = 60, Visible = true },
                new GridColumn { FieldName = "Code", Caption = "Code", Width = 80, Visible = true },
                new GridColumn { FieldName = "Name", Caption = "Name", Width = 150, Visible = true },
                new GridColumn { FieldName = "NativeName", Caption = "Native Name", Width = 150, Visible = true },
                new GridColumn { FieldName = "CultureCode", Caption = "Culture Code", Width = 100, Visible = true },
                new GridColumn { FieldName = "FlagIcon", Caption = "Flag", Width = 60, Visible = true },
                new GridColumn { FieldName = "DisplayOrder", Caption = "Display Order", Width = 100, Visible = true },
                new GridColumn { FieldName = "IsDefault", Caption = "Default", Width = 70, Visible = true },
                new GridColumn { FieldName = "IsActive", Caption = "Active", Width = 70, Visible = true },
                new GridColumn { FieldName = "IsRTL", Caption = "RTL", Width = 50, Visible = true }
            });
        }

        protected override async Task<IEnumerable<Language>> GetItemsAsync(CancellationToken cancellationToken)
        {
            return await _service.GetAllAsync(cancellationToken);
        }

        protected override async Task OnEditAsync(Language entity)
        {
            using var dialog = _serviceProvider.GetRequiredService<LanguageEditForm>();
            dialog.LoadEntityData(entity);
            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
                dialog.SaveEntityData(entity);
                await _service.UpdateAsync(entity, CancellationToken.None);
                await LoadAsync();
            }
        }

        protected override async Task DeleteAsync(int id, CancellationToken cancellationToken)
        {
            await _service.DeleteAsync(id, cancellationToken);
        }

        protected override async Task OnNewAsync()
        {
            using var dialog = _serviceProvider.GetRequiredService<LanguageEditForm>();
            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
                var language = new Language();
                dialog.SaveEntityData(language);
                await _service.AddAsync(language, CancellationToken.None);
                await LoadAsync();
            }
        }
    }
}
