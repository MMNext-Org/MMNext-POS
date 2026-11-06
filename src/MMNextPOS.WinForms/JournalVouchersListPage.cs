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
    /// List page for Journal Voucher (double-entry accounting) entity.
    /// </summary>
    public partial class JournalVouchersListPage : ListPage<JournalVoucher, IJournalVoucherService>
    {
        public JournalVouchersListPage(IJournalVoucherService service, IServiceProvider serviceProvider)
            : base(service, serviceProvider)
        {
        }

        protected override string GetPageTitle() => "Journal Vouchers";

        protected override void ConfigureColumns(GridView view)
        {
            view.Columns.Clear();
            view.Columns.AddRange(new[]
            {
                new GridColumn { FieldName = "VoucherNo", Caption = "Voucher #", Width = 140, Visible = true },
                new GridColumn { FieldName = "VoucherDate", Caption = "Date", Width = 120, Visible = true,
                    DisplayFormat = { FormatString = "g", FormatType = DevExpress.Utils.FormatType.DateTime } },
                new GridColumn { FieldName = "ReferenceNo", Caption = "Reference", Width = 120, Visible = true },
                new GridColumn { FieldName = "Description", Caption = "Description", Width = 260, Visible = true },
                new GridColumn { FieldName = "Status", Caption = "Status", Width = 90, Visible = true },
                new GridColumn { FieldName = "TotalDebit", Caption = "Debit", Width = 130, Visible = true,
                    DisplayFormat = { FormatString = "c2", FormatType = DevExpress.Utils.FormatType.Numeric } },
                new GridColumn { FieldName = "TotalCredit", Caption = "Credit", Width = 130, Visible = true,
                    DisplayFormat = { FormatString = "c2", FormatType = DevExpress.Utils.FormatType.Numeric } }
            });
        }

        protected override async Task<IEnumerable<JournalVoucher>> GetItemsAsync(CancellationToken cancellationToken)
        {
            return await _service.GetAllAsync(cancellationToken);
        }

        protected override IQueryable<JournalVoucher> ApplyStatusFilter(IQueryable<JournalVoucher> query, string status)
        {
            if (string.Equals(status, "All", StringComparison.OrdinalIgnoreCase))
                return query;

            return query.Where(v => v.Status == status);
        }

        protected override async Task OnNewAsync()
        {
            var voucher = new JournalVoucher
            {
                VoucherDate = DateTime.Today,
                Details = new List<JournalVoucherDetail>()
            };

            using var dialog = _serviceProvider.GetRequiredService<JournalVoucherEditForm>();
            dialog.LoadEntityData(voucher);
            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
                dialog.SaveEntityData(voucher);
                await _service.AddAsync(voucher);
                await LoadAsync();
            }
        }

        protected override async Task OnEditAsync(JournalVoucher entity)
        {
            if (entity.Status != "Draft")
            {
                XtraMessageBox.Show(this, "Only Draft vouchers can be edited.", "Edit Voucher",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var voucher = await _service.GetByIdWithDetailsAsync(entity.Id);
            if (voucher == null)
            {
                XtraMessageBox.Show(this, "Voucher not found.", "Edit Voucher",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using var dialog = _serviceProvider.GetRequiredService<JournalVoucherEditForm>();
            dialog.LoadEntityData(voucher);
            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
                dialog.SaveEntityData(voucher);
                await _service.UpdateAsync(voucher);
                await LoadAsync();
            }
        }

        protected override async Task DeleteAsync(int id, CancellationToken cancellationToken)
        {
            var voucher = await _service.GetByIdAsync(id, cancellationToken);
            if (voucher == null || voucher.Status != "Draft")
            {
                XtraMessageBox.Show(this, "Only Draft vouchers can be deleted.", "Delete Voucher",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (XtraMessageBox.Show(this, "Delete this journal voucher?", "Confirm Delete",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                return;
            }

            await _service.DeleteAsync(id, cancellationToken);
            await LoadAsync();
        }
    }
}
