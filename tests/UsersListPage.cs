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
    /// List page for Users entity using the generic ListPage base class.
    /// </summary>
    public partial class UsersListPage : ListPage<User, IUserService>
    {
        public UsersListPage(IUserService service, IServiceProvider serviceProvider)
            : base(service, serviceProvider)
        {
        }

        protected override string GetPageTitle() => "Users";

        protected override void ConfigureColumns(GridView view)
        {
            view.Columns.Clear();
            view.Columns.AddRange(new[]
            {
                new GridColumn { FieldName = "Id", Caption = "ID", Width = 60, Visible = true },
                new GridColumn { FieldName = "Username", Caption = "Username", Width = 150, Visible = true },
                new GridColumn { FieldName = "FullName", Caption = "Full Name", Width = 200, Visible = true },
                new GridColumn { FieldName = "Email", Caption = "Email", Width = 200, Visible = true },
                new GridColumn { FieldName = "Phone", Caption = "Phone", Width = 120, Visible = true },
                new GridColumn { FieldName = "IsActive", Caption = "Active", Width = 70, Visible = true },
                new GridColumn { FieldName = "LastLoginAt", Caption = "Last Login", Width = 150, Visible = true, DisplayFormat = { FormatString = "g", FormatType = DevExpress.Utils.FormatType.DateTime } }
            });
        }

        protected override async Task<IEnumerable<User>> GetItemsAsync(CancellationToken cancellationToken)
        {
            return await _service.GetAllAsync(cancellationToken);
        }

        protected override async Task OnEditAsync(User entity)
        {
            // TODO: Implement UserEditForm
            await Task.CompletedTask;
        }

        protected override async Task DeleteAsync(int id, CancellationToken cancellationToken)
        {
            await _service.DeleteAsync(id, cancellationToken);
        }
    }
}
