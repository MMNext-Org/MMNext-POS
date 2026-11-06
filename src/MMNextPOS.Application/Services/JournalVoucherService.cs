using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MMNextPOS.Domain.Models;
using MMNextPOS.Infrastructure;
using MMNextPOS.Infrastructure.Repositories;

namespace MMNextPOS.Application.Services
{
    public class JournalVoucherService : IJournalVoucherService
    {
        private readonly IJournalVoucherRepository _voucherRepo;
        private readonly IJournalVoucherDetailRepository _detailRepo;
        private readonly IAuditService _auditService;
        private readonly IUnitOfWork _unitOfWork;

        public JournalVoucherService(
            IJournalVoucherRepository voucherRepo,
            IJournalVoucherDetailRepository detailRepo,
            IAuditService auditService,
            IUnitOfWork unitOfWork)
        {
            _voucherRepo = voucherRepo ?? throw new ArgumentNullException(nameof(voucherRepo));
            _detailRepo = detailRepo ?? throw new ArgumentNullException(nameof(detailRepo));
            _auditService = auditService ?? throw new ArgumentNullException(nameof(auditService));
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        public async Task<JournalVoucher?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _voucherRepo.GetByIdAsync(id, cancellationToken);
        }

        public async Task<IReadOnlyList<JournalVoucher>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _voucherRepo.GetAllAsync(cancellationToken);
        }

        public Task<PagedResult<JournalVoucher>> GetPageAsync(int page, int pageSize, CancellationToken cancellationToken = default)
        {
            return _voucherRepo.GetPageAsync(page, pageSize, cancellationToken);
        }

        public async Task<JournalVoucher> AddAsync(JournalVoucher voucher, CancellationToken cancellationToken = default)
        {
            if (voucher == null)
                throw new ArgumentNullException(nameof(voucher));

            if (Math.Abs(voucher.TotalDebit - voucher.TotalCredit) > 0.0001m)
                throw new InvalidOperationException("Journal voucher must balance: TotalDebit == TotalCredit.");

            voucher.Status = "Draft";

            var saved = await _voucherRepo.AddAsync(voucher, cancellationToken);

            foreach (var detail in saved.Details)
            {
                detail.JournalVoucherId = saved.Id;
                await _detailRepo.AddAsync(detail, cancellationToken);
            }

            await _auditService.LogAsync(
                entityName: nameof(JournalVoucher),
                entityId: saved.Id,
                action: "Create",
                oldValues: null,
                newValues: new { saved.VoucherNo, saved.VoucherDate, saved.TotalDebit, saved.TotalCredit },
                cancellationToken: cancellationToken);

            return saved;
        }

        public async Task UpdateAsync(JournalVoucher voucher, CancellationToken cancellationToken = default)
        {
            if (voucher == null)
                throw new ArgumentNullException(nameof(voucher));

            var existing = await _voucherRepo.GetByIdAsync(voucher.Id, cancellationToken)
                ?? throw new InvalidOperationException($"Journal voucher {voucher.Id} not found.");

            if (existing.Status != "Draft")
                throw new InvalidOperationException("Only Draft vouchers can be updated.");

            if (Math.Abs(voucher.TotalDebit - voucher.TotalCredit) > 0.0001m)
                throw new InvalidOperationException("Journal voucher must balance: TotalDebit == TotalCredit.");

            existing.VoucherNo = voucher.VoucherNo;
            existing.VoucherDate = voucher.VoucherDate;
            existing.Description = voucher.Description;
            existing.ReferenceNo = voucher.ReferenceNo;
            existing.TotalDebit = voucher.TotalDebit;
            existing.TotalCredit = voucher.TotalCredit;

            await _voucherRepo.UpdateAsync(existing, cancellationToken);

            // Replace all details
            var oldDetails = await _detailRepo.GetAllAsync(cancellationToken);
            foreach (var oldDetail in oldDetails.Where(d => d.JournalVoucherId == existing.Id))
            {
                await _detailRepo.DeleteAsync(oldDetail.Id, cancellationToken);
            }

            foreach (var detail in voucher.Details)
            {
                detail.JournalVoucherId = existing.Id;
                await _detailRepo.AddAsync(detail, cancellationToken);
            }

            await _auditService.LogAsync(
                entityName: nameof(JournalVoucher),
                entityId: existing.Id,
                action: "Update",
                oldValues: new { existing.VoucherNo, existing.TotalDebit, existing.TotalCredit },
                newValues: new { voucher.VoucherNo, voucher.TotalDebit, voucher.TotalCredit },
                cancellationToken: cancellationToken);
        }

        public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var existing = await _voucherRepo.GetByIdAsync(id, cancellationToken)
                ?? throw new InvalidOperationException($"Journal voucher {id} not found.");

            if (existing.Status != "Draft")
                throw new InvalidOperationException("Only Draft vouchers can be deleted.");

            await _voucherRepo.DeleteAsync(id, cancellationToken);

            await _auditService.LogAsync(
                entityName: nameof(JournalVoucher),
                entityId: id,
                action: "Delete",
                oldValues: new { existing.VoucherNo, existing.TotalDebit, existing.TotalCredit },
                newValues: null,
                cancellationToken: cancellationToken);
        }

        public async Task<JournalVoucher> PostAsync(int id, CancellationToken cancellationToken = default)
        {
            var voucher = await _voucherRepo.GetByIdAsync(id, cancellationToken)
                ?? throw new InvalidOperationException($"Journal voucher {id} not found.");

            if (voucher.Status != "Draft")
                throw new InvalidOperationException("Only Draft vouchers can be posted.");

            if (Math.Abs(voucher.TotalDebit - voucher.TotalCredit) > 0.0001m)
                throw new InvalidOperationException("Voucher must balance before posting.");

            voucher.Status = "Posted";
            await _voucherRepo.UpdateAsync(voucher, cancellationToken);

            await _auditService.LogAsync(
                entityName: nameof(JournalVoucher),
                entityId: id,
                action: "Post",
                oldValues: new { Status = "Draft" },
                newValues: new { Status = "Posted" },
                cancellationToken: cancellationToken);

            return voucher;
        }

        public async Task<JournalVoucher> VoidAsync(int id, CancellationToken cancellationToken = default)
        {
            var voucher = await _voucherRepo.GetByIdAsync(id, cancellationToken)
                ?? throw new InvalidOperationException($"Journal voucher {id} not found.");

            if (voucher.Status == "Voided")
                throw new InvalidOperationException("Voucher is already voided.");

            voucher.Status = "Voided";
            await _voucherRepo.UpdateAsync(voucher, cancellationToken);

            await _auditService.LogAsync(
                entityName: nameof(JournalVoucher),
                entityId: id,
                action: "Void",
                oldValues: new { Status = voucher.Status },
                newValues: new { Status = "Voided" },
                cancellationToken: cancellationToken);

            return voucher;
        }
    }
}
