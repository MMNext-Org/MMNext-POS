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
using MMNextPOS.Infrastructure;
using MMNextPOS.Infrastructure.Repositories;

namespace MMNextPOS.WinForms
{
    /// <summary>
    /// List page for Stock Transfers entity using the generic repository.
    /// </summary>
    public partial class StockTransfersListPage : ListPage<StockTransfer, IStockTransferService>
    {
        public StockTransfersListPage(IStockTransferService service, IServiceProvider serviceProvider)
            : base(service, serviceProvider)
        {
        }

        protected override string GetPageTitle() => "Stock Transfers";

        protected override void ConfigureColumns(GridView view)
        {
            view.Columns.Clear();
            view.Columns.AddRange(new[]
            {
                new GridColumn { FieldName = "Id", Caption = "ID", Width = 60, Visible = true },
                new GridColumn { FieldName = "TransferNo", Caption = "Transfer #", Width = 150, Visible = true },
                new GridColumn { FieldName = "FromLocationId", Caption = "From Location", Width = 120, Visible = true },
                new GridColumn { FieldName = "ToLocationId", Caption = "To Location", Width = 120, Visible = true },
                new GridColumn { FieldName = "TransferDate", Caption = "Transfer Date", Width = 120, Visible = true, DisplayFormat = { FormatString = "g", FormatType = DevExpress.Utils.FormatType.DateTime } },
                new GridColumn { FieldName = "ReceivedDate", Caption = "Received Date", Width = 120, Visible = true, DisplayFormat = { FormatString = "g", FormatType = DevExpress.Utils.FormatType.DateTime } },
                new GridColumn { FieldName = "Status", Caption = "Status", Width = 100, Visible = true },
                new GridColumn { FieldName = "Notes", Caption = "Notes", Width = 200, Visible = true }
            });
        }

        protected override async Task<IEnumerable<StockTransfer>> GetItemsAsync(CancellationToken cancellationToken)
        {
            return await _service.GetTransfersAsync(cancellationToken: cancellationToken);
        }

        protected override async Task OnNewAsync()
        {
            var locationRepo = _serviceProvider.GetService(typeof(ILocationRepository)) as ILocationRepository;
            var productRepo = _serviceProvider.GetService(typeof(IProductRepository)) as IProductRepository;
            var detailRepo = _serviceProvider.GetService(typeof(IStockTransferDetailRepository)) as IStockTransferDetailRepository;
            var unitOfWork = _serviceProvider.GetService(typeof(IUnitOfWork)) as IUnitOfWork;
            var auditService = _serviceProvider.GetService(typeof(IAuditService)) as MMNextPOS.Application.Services.IAuditService;

            if (locationRepo == null || productRepo == null || detailRepo == null || unitOfWork == null || auditService == null)
            {
                ShowError("Transfer services not available.");
                return;
            }

            var transferNo = await _service.GenerateTransferNumberAsync(CancellationToken.None);
            var transfer = new StockTransfer
            {
                TransferNo = transferNo,
                Status = "Draft",
                TransferDate = DateTime.UtcNow
            };

            using var form = new StockTransferEditForm(transfer, _service, locationRepo, productRepo, detailRepo, unitOfWork, auditService);
            if (form.ShowDialog() == DialogResult.OK)
            {
                await LoadAsync();
            }
        }

        protected override async Task OnEditAsync(StockTransfer entity)
        {
            var locationRepo = _serviceProvider.GetService(typeof(ILocationRepository)) as ILocationRepository;
            var productRepo = _serviceProvider.GetService(typeof(IProductRepository)) as IProductRepository;
            var detailRepo = _serviceProvider.GetService(typeof(IStockTransferDetailRepository)) as IStockTransferDetailRepository;
            var unitOfWork = _serviceProvider.GetService(typeof(IUnitOfWork)) as IUnitOfWork;
            var auditService = _serviceProvider.GetService(typeof(IAuditService)) as MMNextPOS.Application.Services.IAuditService;

            if (locationRepo == null || productRepo == null || detailRepo == null || unitOfWork == null || auditService == null)
            {
                ShowError("Transfer services not available.");
                return;
            }

            using var form = new StockTransferEditForm(entity, _service, locationRepo, productRepo, detailRepo, unitOfWork, auditService);
            if (form.ShowDialog() == DialogResult.OK)
            {
                await LoadAsync();
            }
        }

        protected override async Task DeleteAsync(int id, CancellationToken cancellationToken)
        {
            await _service.CancelTransferAsync(id, 1, "User deleted", cancellationToken);
        }
    }
}
