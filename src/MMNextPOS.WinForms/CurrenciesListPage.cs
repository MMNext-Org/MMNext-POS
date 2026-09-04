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
    /// List page for Currencies entity using the generic ListPage base class.
    /// </summary>
    public partial class CurrenciesListPage : ListPage<Currency, ICurrencyService>
    {
        public CurrenciesListPage(ICurrencyService service, IServiceProvider serviceProvider)
            : base(service, serviceProvider)
        {
        }

        protected override string GetPageTitle() => "Currencies";

        protected override void ConfigureColumns(GridView view)
        {
            view.Columns.Clear();
            view.Columns.AddRange(new[]
            {
                new GridColumn { FieldName = "Id", Caption = "ID", Width = 60, Visible = true },
                new GridColumn { FieldName = "Code", Caption = "Code", Width = 80, Visible = true },
                new GridColumn { FieldName = "Name", Caption = "Name", Width = 150, Visible = true },
                new GridColumn { FieldName = "Symbol", Caption = "Symbol", Width = 80, Visible = true },
                new GridColumn { FieldName = "ExchangeRate", Caption = "Exchange Rate", Width = 120, Visible = true, DisplayFormat = { FormatString = "n4", FormatType = DevExpress.Utils.FormatType.Numeric } },
                new GridColumn { FieldName = "IsActive", Caption = "Active", Width = 70, Visible = true },
                new GridColumn { FieldName = "IsDefault", Caption = "Default", Width = 70, Visible = true }
            });
        }

        protected override async Task<IEnumerable<Currency>> GetItemsAsync(CancellationToken cancellationToken)
        {
            return await _service.GetAllAsync(cancellationToken);
        }

        protected override async Task OnEditAsync(Currency entity)
        {
            // TODO: Implement CurrencyEditForm
            await Task.CompletedTask;
        }

        protected override async Task DeleteAsync(int id, CancellationToken cancellationToken)
        {
            await _service.DeleteAsync(id, cancellationToken);
        }
    }
}