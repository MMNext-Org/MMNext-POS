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
    /// List page for Themes entity using the generic ListPage base class.
    /// </summary>
    public partial class ThemesListPage : ListPage<Theme, IThemeService>
    {
        public ThemesListPage(IThemeService service, IServiceProvider serviceProvider)
            : base(service, serviceProvider)
        {
        }

        protected override string GetPageTitle() => "Themes";

        protected override void ConfigureColumns(GridView view)
        {
            view.Columns.Clear();
            view.Columns.AddRange(new[]
            {
                new GridColumn { FieldName = "Id", Caption = "ID", Width = 60, Visible = true },
                new GridColumn { FieldName = "Code", Caption = "Code", Width = 100, Visible = true },
                new GridColumn { FieldName = "Name", Caption = "Name", Width = 150, Visible = true },
                new GridColumn { FieldName = "PrimaryColor", Caption = "Primary Color", Width = 100, Visible = true },
                new GridColumn { FieldName = "SecondaryColor", Caption = "Secondary Color", Width = 110, Visible = true },
                new GridColumn { FieldName = "AccentColor", Caption = "Accent Color", Width = 100, Visible = true },
                new GridColumn { FieldName = "BackgroundColor", Caption = "Background", Width = 100, Visible = true },
                new GridColumn { FieldName = "TextColor", Caption = "Text Color", Width = 100, Visible = true },
                new GridColumn { FieldName = "FontFamily", Caption = "Font Family", Width = 120, Visible = true },
                new GridColumn { FieldName = "FontSize", Caption = "Font Size", Width = 80, Visible = true },
                new GridColumn { FieldName = "IsDefault", Caption = "Default", Width = 70, Visible = true },
                new GridColumn { FieldName = "IsActive", Caption = "Active", Width = 70, Visible = true }
            });
        }

        protected override async Task<IEnumerable<Theme>> GetItemsAsync(CancellationToken cancellationToken)
        {
            return await _service.GetAllAsync(cancellationToken);
        }

        protected override async Task OnEditAsync(Theme entity)
        {
            using var dialog = _serviceProvider.GetRequiredService<ThemeEditForm>();
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
            using var dialog = _serviceProvider.GetRequiredService<ThemeEditForm>();
            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
                var theme = new Theme();
                dialog.SaveEntityData(theme);
                await _service.AddAsync(theme, CancellationToken.None);
                await LoadAsync();
            }
        }
    }
}