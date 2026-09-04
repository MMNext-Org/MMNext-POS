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
    public class PurchaseReceiptService : IPurchaseReceiptService
    {
        private readonly IPurchaseReceiptRepository _receiptRepo;
        private readonly IPurchaseReceiptDetailRepository _detailRepo;
        private readonly IAuditService _auditService;

        public PurchaseReceiptService(
            IPurchaseReceiptRepository receiptRepo,
            IPurchaseReceiptDetailRepository detailRepo,
            IAuditService auditService)
        {
            _receiptRepo = receiptRepo ?? throw new ArgumentNullException(nameof(receiptRepo));
            _detailRepo = detailRepo ?? throw new ArgumentNullException(nameof(detailRepo));
            _auditService = auditService ?? throw new ArgumentNullException(nameof(auditService));
        }

        public Task<PurchaseReceipt?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return _receiptRepo.GetByIdAsync(id, cancellationToken);
        }

        public Task<PurchaseReceipt?> GetByPurchaseIdAsync(int purchaseId, CancellationToken cancellationToken = default)
        {
            return _receiptRepo.GetByPurchaseIdAsync(purchaseId, cancellationToken);
        }

        public async Task<IReadOnlyList<PurchaseReceipt>> GetByDateRangeAsync(DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default)
        {
            return await _receiptRepo.GetByDateRangeAsync(fromDate, toDate, cancellationToken);
        }

        public async Task<PurchaseReceipt> CreateReceiptAsync(PurchaseReceipt receipt, IEnumerable<PurchaseReceiptDetail> details, CancellationToken cancellationToken = default)
        {
            var result = await _receiptRepo.AddWithDetailsAsync(receipt, details, cancellationToken).ConfigureAwait(false);
            await _auditService.LogAsync(nameof(PurchaseReceipt), result.Id, "Create", null, result, 1, "System", $"Created purchase receipt {result.ReceiptNo}", cancellationToken).ConfigureAwait(false);
            return result;
        }

        public async Task<PurchaseReceipt> UpdateReceiptAsync(PurchaseReceipt receipt, CancellationToken cancellationToken = default)
        {
            var existing = await _receiptRepo.GetByIdAsync(receipt.Id, cancellationToken).ConfigureAwait(false);
            await _receiptRepo.UpdateAsync(receipt, cancellationToken).ConfigureAwait(false);
            await _auditService.LogAsync(nameof(PurchaseReceipt), receipt.Id, "Update", existing, receipt, 1, "System", $"Updated purchase receipt {receipt.ReceiptNo}", cancellationToken).ConfigureAwait(false);
            return receipt;
        }

        public async Task DeleteReceiptAsync(int id, CancellationToken cancellationToken = default)
        {
            var existing = await _receiptRepo.GetByIdAsync(id, cancellationToken).ConfigureAwait(false);
            await _receiptRepo.DeleteAsync(id, cancellationToken).ConfigureAwait(false);
            await _auditService.LogAsync(nameof(PurchaseReceipt), id, "Delete", existing, null, 1, "System", $"Deleted purchase receipt {existing?.ReceiptNo ?? id.ToString()}", cancellationToken).ConfigureAwait(false);
        }

        public async Task<IReadOnlyList<PurchaseReceiptDetail>> GetReceiptDetailsAsync(int receiptId, CancellationToken cancellationToken = default)
        {
            return await _detailRepo.GetByReceiptIdAsync(receiptId, cancellationToken);
        }
    }
}