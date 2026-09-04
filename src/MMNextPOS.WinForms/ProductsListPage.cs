using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraGrid.Views.Grid;
using MMNextPOS.Application.Services;
using MMNextPOS.Domain.Models;

namespace MMNextPOS.WinForms
{
    /// <summary>
    /// List page for Products entity using the generic ListPage base class.
    /// </summary>
    public partial class ProductsListPage : ListPage<Product, IProductService>
    {
        public ProductsListPage(IProductService service, IServiceProvider serviceProvider)
            : base(service, serviceProvider)
        {
        }

        protected override string GetPageTitle() => "Products";

        protected override void ConfigureColumns(GridView view)
        {
            view.Columns.Clear();
            view.Columns.AddRange(new[]
            {
                new GridColumn { FieldName = "Id", Caption = "ID", Width = 60, Visible = true },
                new GridColumn { FieldName = "Sku", Caption = "SKU", Width = 150, Visible = true },
                new GridColumn { FieldName = "Name", Caption = "Product Name", Width = 250, Visible = true },
                new GridColumn { FieldName = "Price", Caption = "Price", Width = 100, Visible = true },
                new GridColumn { FieldName = "StockQuantity", Caption = "Stock", Width = 80, Visible = true },
                new GridColumn { FieldName = "IsActive", Caption = "Active", Width = 70, Visible = true }
            });
        }

        protected override async Task<IEnumerable<Product>> GetItemsAsync(CancellationToken cancellationToken)
        {
            return await _service.GetAllAsync(cancellationToken);
        }

        protected override async Task OnEditAsync(Product entity)
        {
            // Edit handled by derived forms
        }

        protected override async Task DeleteAsync(int id, CancellationToken cancellationToken)
        {
            // Delete handled by derived forms
        }
    }
}
