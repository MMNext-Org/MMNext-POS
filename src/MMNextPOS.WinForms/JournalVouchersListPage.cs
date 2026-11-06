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
            // Restrict status filter to voucher-relevant values (base populates a generic list).
            _statusFilter.Properties.Items.Clear();
            _statusFilter.Properties.Items.AddRange(new[] { "All", "Draft", "Posted", "Voided" });
            _statusFilter.SelectedIndex = 0;

            // Workflow buttons: Post / Void (single-row status updates, no transaction needed).
            AppendToolbarButton("Post", PostSelected_Click, 910, 90);
            AppendToolbarButton("Void", VoidSelected_Click, 1010, 90);
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

        private void PostSelected_Click(object? sender, EventArgs e) => _ = PostSelectedAsync();
        private void VoidSelected_Click(object? sender, EventArgs e) => _ = VoidSelectedAsync();

        private async Task PostSelectedAsync()
        {
            if (_gridView.FocusedRowHandle < 0) return;
            var entity = _gridView.GetRow(_gridView.FocusedRowHandle) as JournalVoucher;
            if (entity == null) return;

            if (entity.Status != "Draft")
            {
                ShowError("Only Draft vouchers can be posted.");
                return;
            }
            if (!ShowConfirm("Post this journal voucher?")) return;

            try
            {
                await _service.PostAsync(entity.Id);
                await LoadAsync();
            }
            catch (Exception ex)
            {
                ShowError($"Post failed: {ex.Message}");
            }
        }

        private async Task VoidSelectedAsync()
        {
            if (_gridView.FocusedRowHandle < 0) return;
            var entity = _gridView.GetRow(_gridView.FocusedRowHandle) as JournalVoucher;
            if (entity == null) return;

            if (entity.Status == "Voided")
            {
                ShowError("Voucher is already voided.");
                return;
            }
            if (!ShowConfirm("Void this journal voucher?")) return;

            try
            {
                await _service.VoidAsync(entity.Id);
                await LoadAsync();
            }
            catch (Exception ex)
            {
                ShowError($"Void failed: {ex.Message}");
            }
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
                ShowError("Only Draft vouchers can be edited.");
                return;
            }

            var voucher = await _service.GetByIdWithDetailsAsync(entity.Id);
            if (voucher == null)
            {
                ShowError("Voucher not found.");
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

        protected override async Task OnDeleteAsync()
        {
            if (_gridView.FocusedRowHandle < 0) return;
            var entity = _gridView.GetRow(_gridView.FocusedRowHandle) as JournalVoucher;
            if (entity == null) return;

            if (entity.Status != "Draft")
            {
                ShowError("Only Draft vouchers can be deleted.");
                return;
            }

            await base.OnDeleteAsync();
        }

        protected override async Task DeleteAsync(int id, CancellationToken cancellationToken)
        {
            var voucher = await _service.GetByIdAsync(id, cancellationToken);
            if (voucher == null || voucher.Status != "Draft") return;

            await _service.DeleteAsync(id, cancellationToken);
        }
    }
}
