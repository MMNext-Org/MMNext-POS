using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MMNextPOS.Application.Services;
using MMNextPOS.Domain.Models;
using MMNextPOS.Infrastructure.Repositories;

namespace MMNextPOS.Application.Services
{
    public class PaymentVoucherService : IPaymentVoucherService
    {
        private readonly IPaymentVoucherRepository _repo;
        private readonly IAuditService _auditService;

        public PaymentVoucherService(
            IPaymentVoucherRepository repo,
            IAuditService auditService)
        {
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
            _auditService = auditService ?? throw new ArgumentNullException(nameof(auditService));
        }

        public Task<PaymentVoucher?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return _repo.GetByIdAsync(id, cancellationToken);
        }

        public Task<PaymentVoucher?> GetByVoucherNoAsync(string voucherNo, CancellationToken cancellationToken = default)
        {
            return _repo.GetByVoucherNoAsync(voucherNo, cancellationToken);
        }

        public async Task<IReadOnlyList<PaymentVoucher>> GetByDateRangeAsync(DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default)
        {
            return await _repo.GetByDateRangeAsync(fromDate, toDate, cancellationToken);
        }

        public async Task<IReadOnlyList<PaymentVoucher>> GetByCustomerAsync(int customerId, CancellationToken cancellationToken = default)
        {
            return await _repo.GetByCustomerAsync(customerId, cancellationToken);
        }

        public async Task<IReadOnlyList<PaymentVoucher>> GetBySupplierAsync(int supplierId, CancellationToken cancellationToken = default)
        {
            return await _repo.GetBySupplierAsync(supplierId, cancellationToken);
        }

        public async Task<IReadOnlyList<PaymentVoucher>> GetBySaleAsync(int saleId, CancellationToken cancellationToken = default)
        {
            return await _repo.GetBySaleAsync(saleId, cancellationToken);
        }

        public async Task<IReadOnlyList<PaymentVoucher>> GetByPurchaseAsync(int purchaseId, CancellationToken cancellationToken = default)
        {
            return await _repo.GetByPurchaseAsync(purchaseId, cancellationToken);
        }

        public async Task<PaymentVoucher> AddAsync(PaymentVoucher voucher, CancellationToken cancellationToken = default)
        {
            var result = await _repo.AddAsync(voucher, cancellationToken).ConfigureAwait(false);
            await _auditService.LogAsync(nameof(PaymentVoucher), result.Id, "Create", null, result, 1, "System", $"Created payment voucher {result.VoucherNo}", cancellationToken).ConfigureAwait(false);
            return result;
        }

        public async Task UpdateAsync(PaymentVoucher voucher, CancellationToken cancellationToken = default)
        {
            var existing = await _repo.GetByIdAsync(voucher.Id, cancellationToken).ConfigureAwait(false);
            await _repo.UpdateAsync(voucher, cancellationToken).ConfigureAwait(false);
            await _auditService.LogAsync(nameof(PaymentVoucher), voucher.Id, "Update", existing, voucher, 1, "System", $"Updated payment voucher {voucher.VoucherNo}", cancellationToken).ConfigureAwait(false);
        }

        public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var existing = await _repo.GetByIdAsync(id, cancellationToken).ConfigureAwait(false);
            await _repo.DeleteAsync(id, cancellationToken).ConfigureAwait(false);
            await _auditService.LogAsync(nameof(PaymentVoucher), id, "Delete", existing, null, 1, "System", $"Deleted payment voucher {existing?.VoucherNo ?? id.ToString()}", cancellationToken).ConfigureAwait(false);
        }
    }
}