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
    public class SaleReceiptService : ISaleReceiptService
    {
        private readonly ISaleReceiptRepository _receiptRepo;
        private readonly ISaleReceiptDetailRepository _detailRepo;
        private readonly IAuditService _auditService;

        public SaleReceiptService(
            ISaleReceiptRepository receiptRepo,
            ISaleReceiptDetailRepository detailRepo,
            IAuditService auditService)
        {
            _receiptRepo = receiptRepo ?? throw new ArgumentNullException(nameof(receiptRepo));
            _detailRepo = detailRepo ?? throw new ArgumentNullException(nameof(detailRepo));
            _auditService = auditService ?? throw new ArgumentNullException(nameof(auditService));
        }

        public Task<SaleReceipt?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return _receiptRepo.GetByIdAsync(id, cancellationToken);
        }

        public Task<SaleReceipt?> GetBySaleIdAsync(int saleId, CancellationToken cancellationToken = default)
        {
            return _receiptRepo.GetBySaleIdAsync(saleId, cancellationToken);
        }

        public async Task<IReadOnlyList<SaleReceipt>> GetByDateRangeAsync(DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default)
        {
            return await _receiptRepo.GetByDateRangeAsync(fromDate, toDate, cancellationToken);
        }

        public async Task<SaleReceipt> CreateReceiptAsync(SaleReceipt receipt, IEnumerable<SaleReceiptDetail> details, CancellationToken cancellationToken = default)
        {
            var result = await _receiptRepo.AddWithDetailsAsync(receipt, details, cancellationToken).ConfigureAwait(false);
            await _auditService.LogAsync(nameof(SaleReceipt), result.Id, "Create", null, result, 1, "System", $"Created sale receipt {result.ReceiptNo}", cancellationToken).ConfigureAwait(false);
            return result;
        }

        public async Task<SaleReceipt> UpdateReceiptAsync(SaleReceipt receipt, CancellationToken cancellationToken = default)
        {
            var existing = await _receiptRepo.GetByIdAsync(receipt.Id, cancellationToken).ConfigureAwait(false);
            await _receiptRepo.UpdateAsync(receipt, cancellationToken).ConfigureAwait(false);
            await _auditService.LogAsync(nameof(SaleReceipt), receipt.Id, "Update", existing, receipt, 1, "System", $"Updated sale receipt {receipt.ReceiptNo}", cancellationToken).ConfigureAwait(false);
            return receipt;
        }

        public async Task DeleteReceiptAsync(int id, CancellationToken cancellationToken = default)
        {
            var existing = await _receiptRepo.GetByIdAsync(id, cancellationToken).ConfigureAwait(false);
            await _receiptRepo.DeleteAsync(id, cancellationToken).ConfigureAwait(false);
            await _auditService.LogAsync(nameof(SaleReceipt), id, "Delete", existing, null, 1, "System", $"Deleted sale receipt {existing?.ReceiptNo ?? id.ToString()}", cancellationToken).ConfigureAwait(false);
        }

        public async Task<IReadOnlyList<SaleReceiptDetail>> GetReceiptDetailsAsync(int receiptId, CancellationToken cancellationToken = default)
        {
            return await _detailRepo.GetByReceiptIdAsync(receiptId, cancellationToken);
        }
    }
}