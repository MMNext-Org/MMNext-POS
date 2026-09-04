using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MMNextPOS.Application.Services;
using MMNextPOS.Domain.Models;
using MMNextPOS.Infrastructure.Repositories;

namespace MMNextPOS.Application.Services
{
    public class InvoiceService : IInvoiceService
    {
        private readonly IInvoiceRepository _repo;
        private readonly IAuditService _auditService;

        public InvoiceService(IInvoiceRepository repo, IAuditService auditService)
        {
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
            _auditService = auditService ?? throw new ArgumentNullException(nameof(auditService));
        }

        public Task<Invoice?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return _repo.GetByIdAsync(id, cancellationToken);
        }

        public Task<IReadOnlyList<Invoice>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return _repo.GetAllAsync(cancellationToken);
        }

        public async Task<Invoice> AddAsync(Invoice invoice, CancellationToken cancellationToken = default)
        {
            var result = await _repo.AddAsync(invoice, cancellationToken).ConfigureAwait(false);
            await _auditService.LogAsync(nameof(Invoice), result.Id, "Create", null, result, 1, "System", $"Created invoice {result.InvoiceNo}", cancellationToken).ConfigureAwait(false);
            return result;
        }

        public async Task UpdateAsync(Invoice invoice, CancellationToken cancellationToken = default)
        {
            var existing = await _repo.GetByIdAsync(invoice.Id, cancellationToken).ConfigureAwait(false);
            await _repo.UpdateAsync(invoice, cancellationToken).ConfigureAwait(false);
            await _auditService.LogAsync(nameof(Invoice), invoice.Id, "Update", existing, invoice, 1, "System", $"Updated invoice {invoice.InvoiceNo}", cancellationToken).ConfigureAwait(false);
        }

        public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var existing = await _repo.GetByIdAsync(id, cancellationToken).ConfigureAwait(false);
            await _repo.DeleteAsync(id, cancellationToken).ConfigureAwait(false);
            await _auditService.LogAsync(nameof(Invoice), id, "Delete", existing, null, 1, "System", $"Deleted invoice {existing?.InvoiceNo ?? id.ToString()}", cancellationToken).ConfigureAwait(false);
        }
    }
}
