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
        private readonly IInvoiceNumberGenerator _invoiceNumberGenerator;

        public JournalVoucherService(
            IJournalVoucherRepository voucherRepo,
            IJournalVoucherDetailRepository detailRepo,
            IAuditService auditService,
            IUnitOfWork unitOfWork,
            IInvoiceNumberGenerator invoiceNumberGenerator)
        {
            _voucherRepo = voucherRepo ?? throw new ArgumentNullException(nameof(voucherRepo));
            _detailRepo = detailRepo ?? throw new ArgumentNullException(nameof(detailRepo));
            _auditService = auditService ?? throw new ArgumentNullException(nameof(auditService));
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _invoiceNumberGenerator = invoiceNumberGenerator ?? throw new ArgumentNullException(nameof(invoiceNumberGenerator));
        }

        public async Task<JournalVoucher?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _voucherRepo.GetByIdAsync(id, cancellationToken);
        }

        public async Task<JournalVoucher?> GetByIdWithDetailsAsync(int id, CancellationToken cancellationToken = default)
        {
            var voucher = await _voucherRepo.GetByIdAsync(id, cancellationToken);
            if (voucher == null)
                return null;

            voucher.Details = (await _detailRepo.GetByVoucherIdAsync(id, cancellationToken))
                .OrderBy(d => d.LineNo)
                .ToList();

            return voucher;
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

            // Require at least 2 lines for double-entry
            var details = (voucher.Details ?? Enumerable.Empty<JournalVoucherDetail>()).ToList();
            if (details.Count < 2)
                throw new InvalidOperationException("Journal voucher must have at least 2 lines (debit and credit).");

            // Validate each line: exactly one of debit/credit must be positive
            foreach (var d in details)
            {
                if (string.IsNullOrWhiteSpace(d.AccountCode))
                    throw new InvalidOperationException($"Line {d.LineNo}: AccountCode is required.");
                if (string.IsNullOrWhiteSpace(d.AccountName))
                    throw new InvalidOperationException($"Line {d.LineNo}: AccountName is required.");
                if (d.DebitAmount < 0 || d.CreditAmount < 0)
                    throw new InvalidOperationException($"Line {d.LineNo}: DebitAmount and CreditAmount cannot be negative.");
                if (d.DebitAmount > 0 && d.CreditAmount > 0)
                    throw new InvalidOperationException($"Line {d.LineNo}: cannot have both DebitAmount and CreditAmount.");
                if (d.DebitAmount == 0 && d.CreditAmount == 0)
                    throw new InvalidOperationException($"Line {d.LineNo}: must have either DebitAmount or CreditAmount.");
            }

            if (voucher.VoucherDate == default)
                throw new InvalidOperationException("VoucherDate is required.");

            // Calculate totals from lines (do not trust caller-supplied totals)
            var totalDebit = details.Sum(d => d.DebitAmount);
            var totalCredit = details.Sum(d => d.CreditAmount);
            if (Math.Abs(totalDebit - totalCredit) > 0.0001m)
                throw new InvalidOperationException("Journal voucher must balance: total debit must equal total credit.");

            voucher.Status = "Draft";
            voucher.TotalDebit = totalDebit;
            voucher.TotalCredit = totalCredit;

            await _unitOfWork.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                // Reserve voucher number inside the transaction so concurrent
                // callers cannot receive the same number (see IInvoiceNumberGenerator).
                if (string.IsNullOrWhiteSpace(voucher.VoucherNo))
                {
                    voucher.VoucherNo = await _invoiceNumberGenerator.NextAsync("JV", cancellationToken)
                        .ConfigureAwait(false);
                }

                voucher.VoucherNo = voucher.VoucherNo.Trim();
                var saved = await _voucherRepo.AddAsync(voucher, cancellationToken).ConfigureAwait(false);

                foreach (var detail in details)
                {
                    detail.JournalVoucherId = saved.Id;
                    detail.LineNo = details.IndexOf(detail);
                    await _detailRepo.AddAsync(detail, cancellationToken).ConfigureAwait(false);
                }

                await _auditService.LogAsync(
                    entityName: nameof(JournalVoucher),
                    entityId: saved.Id,
                    action: "Create",
                    oldValues: null,
                    newValues: new { saved.VoucherNo, saved.VoucherDate, saved.TotalDebit, saved.TotalCredit },
                    cancellationToken: cancellationToken).ConfigureAwait(false);

                await _unitOfWork.CommitAsync(cancellationToken).ConfigureAwait(false);

                return saved;
            }
            catch
            {
                await _unitOfWork.RollbackAsync(cancellationToken).ConfigureAwait(false);
                throw;
            }
        }

        public async Task UpdateAsync(JournalVoucher voucher, CancellationToken cancellationToken = default)
        {
            if (voucher == null)
                throw new ArgumentNullException(nameof(voucher));

            var existing = await _voucherRepo.GetByIdAsync(voucher.Id, cancellationToken)
                ?? throw new InvalidOperationException($"Journal voucher {voucher.Id} not found.");

            if (existing.Status != "Draft")
                throw new InvalidOperationException("Only Draft vouchers can be updated.");

            // Capture old values BEFORE mutation
            var oldVoucherNo = existing.VoucherNo;
            var oldTotalDebit = existing.TotalDebit;
            var oldTotalCredit = existing.TotalCredit;

            var details = (voucher.Details ?? Enumerable.Empty<JournalVoucherDetail>()).ToList();
            if (details.Count < 2)
                throw new InvalidOperationException("Journal voucher must have at least 2 lines (debit and credit).");

            foreach (var d in details)
            {
                if (string.IsNullOrWhiteSpace(d.AccountCode))
                    throw new InvalidOperationException($"Line {d.LineNo}: AccountCode is required.");
                if (string.IsNullOrWhiteSpace(d.AccountName))
                    throw new InvalidOperationException($"Line {d.LineNo}: AccountName is required.");
                if (d.DebitAmount < 0 || d.CreditAmount < 0)
                    throw new InvalidOperationException($"Line {d.LineNo}: DebitAmount and CreditAmount cannot be negative.");
                if (d.DebitAmount > 0 && d.CreditAmount > 0)
                    throw new InvalidOperationException($"Line {d.LineNo}: cannot have both DebitAmount and CreditAmount.");
                if (d.DebitAmount == 0 && d.CreditAmount == 0)
                    throw new InvalidOperationException($"Line {d.LineNo}: must have either DebitAmount or CreditAmount.");
            }

            if (voucher.VoucherDate == default)
                throw new InvalidOperationException("VoucherDate is required.");

            var totalDebit = details.Sum(d => d.DebitAmount);
            var totalCredit = details.Sum(d => d.CreditAmount);
            if (Math.Abs(totalDebit - totalCredit) > 0.0001m)
                throw new InvalidOperationException("Journal voucher must balance: total debit must equal total credit.");

            existing.VoucherNo = string.IsNullOrWhiteSpace(voucher.VoucherNo) ? existing.VoucherNo : voucher.VoucherNo.Trim();
            existing.VoucherDate = voucher.VoucherDate;
            existing.Description = voucher.Description;
            existing.ReferenceNo = voucher.ReferenceNo;
            existing.TotalDebit = totalDebit;
            existing.TotalCredit = totalCredit;

            await _unitOfWork.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                await _voucherRepo.UpdateAsync(existing, cancellationToken).ConfigureAwait(false);

                // Replace all details: soft-delete old, insert new
                var oldDetails = await _detailRepo.GetByVoucherIdAsync(existing.Id, cancellationToken).ConfigureAwait(false);
                foreach (var oldDetail in oldDetails)
                {
                    await _detailRepo.DeleteAsync(oldDetail.Id, cancellationToken).ConfigureAwait(false);
                }

                foreach (var detail in details)
                {
                    detail.JournalVoucherId = existing.Id;
                    detail.LineNo = details.IndexOf(detail);
                    await _detailRepo.AddAsync(detail, cancellationToken).ConfigureAwait(false);
                }

                await _auditService.LogAsync(
                    entityName: nameof(JournalVoucher),
                    entityId: existing.Id,
                    action: "Update",
                    oldValues: new { VoucherNo = oldVoucherNo, TotalDebit = oldTotalDebit, TotalCredit = oldTotalCredit },
                    newValues: new { VoucherNo = existing.VoucherNo, TotalDebit = existing.TotalDebit, TotalCredit = existing.TotalCredit },
                    cancellationToken: cancellationToken).ConfigureAwait(false);

                await _unitOfWork.CommitAsync(cancellationToken).ConfigureAwait(false);
            }
            catch
            {
                await _unitOfWork.RollbackAsync(cancellationToken).ConfigureAwait(false);
                throw;
            }
        }

        public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var existing = await _voucherRepo.GetByIdAsync(id, cancellationToken)
                ?? throw new InvalidOperationException($"Journal voucher {id} not found.");

            if (existing.Status != "Draft")
                throw new InvalidOperationException("Only Draft vouchers can be deleted.");

            var oldValues = new { existing.VoucherNo, existing.TotalDebit, existing.TotalCredit };

            await _unitOfWork.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                // Soft-delete all details first
                var details = await _detailRepo.GetByVoucherIdAsync(id, cancellationToken).ConfigureAwait(false);
                foreach (var detail in details)
                {
                    await _detailRepo.DeleteAsync(detail.Id, cancellationToken).ConfigureAwait(false);
                }

                await _voucherRepo.DeleteAsync(id, cancellationToken).ConfigureAwait(false);

                await _auditService.LogAsync(
                    entityName: nameof(JournalVoucher),
                    entityId: id,
                    action: "Delete",
                    oldValues: oldValues,
                    newValues: null,
                    cancellationToken: cancellationToken).ConfigureAwait(false);

                await _unitOfWork.CommitAsync(cancellationToken).ConfigureAwait(false);
            }
            catch
            {
                await _unitOfWork.RollbackAsync(cancellationToken).ConfigureAwait(false);
                throw;
            }
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
            await _voucherRepo.UpdateAsync(voucher, cancellationToken).ConfigureAwait(false);

            await _auditService.LogAsync(
                entityName: nameof(JournalVoucher),
                entityId: id,
                action: "Post",
                oldValues: new { Status = "Draft" },
                newValues: new { Status = "Posted" },
                cancellationToken: cancellationToken).ConfigureAwait(false);

            return voucher;
        }

        public async Task<JournalVoucher> VoidAsync(int id, CancellationToken cancellationToken = default)
        {
            var voucher = await _voucherRepo.GetByIdAsync(id, cancellationToken)
                ?? throw new InvalidOperationException($"Journal voucher {id} not found.");

            if (voucher.Status == "Voided")
                throw new InvalidOperationException("Voucher is already voided.");

            // Capture prior status BEFORE mutation
            var priorStatus = voucher.Status;

            voucher.Status = "Voided";
            await _voucherRepo.UpdateAsync(voucher, cancellationToken).ConfigureAwait(false);

            await _auditService.LogAsync(
                entityName: nameof(JournalVoucher),
                entityId: id,
                action: "Void",
                oldValues: new { Status = priorStatus },
                newValues: new { Status = "Voided" },
                cancellationToken: cancellationToken).ConfigureAwait(false);

            return voucher;
        }
    }
}
