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
    public class PaymentService : IPaymentService
    {
        private readonly IPaymentRepository _repo;
        private readonly IAuditService _auditService;

        public PaymentService(IPaymentRepository repo, IAuditService auditService)
        {
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
            _auditService = auditService ?? throw new ArgumentNullException(nameof(auditService));
        }

        public Task<Payment?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return _repo.GetByIdAsync(id, cancellationToken);
        }

        public Task<IReadOnlyList<Payment>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return _repo.GetAllAsync(cancellationToken);
        }

        public async Task<PagedResult<Payment>> GetPageAsync(int page, int pageSize, CancellationToken cancellationToken = default)
        {
            return await _repo.GetPageAsync(page, pageSize, cancellationToken);
        }

        public async Task<Payment> AddAsync(Payment payment, CancellationToken cancellationToken = default)
        {
            var result = await _repo.AddAsync(payment, cancellationToken).ConfigureAwait(false);
            await _auditService.LogAsync(nameof(Payment), result.Id, "Create", null, result, 1, "System", $"Created payment {result.PaymentNo}", cancellationToken).ConfigureAwait(false);
            return result;
        }

        public async Task UpdateAsync(Payment payment, CancellationToken cancellationToken = default)
        {
            var existing = await _repo.GetByIdAsync(payment.Id, cancellationToken).ConfigureAwait(false);
            await _repo.UpdateAsync(payment, cancellationToken).ConfigureAwait(false);
            await _auditService.LogAsync(nameof(Payment), payment.Id, "Update", existing, payment, 1, "System", $"Updated payment {payment.PaymentNo}", cancellationToken).ConfigureAwait(false);
        }

        public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var existing = await _repo.GetByIdAsync(id, cancellationToken).ConfigureAwait(false);
            await _repo.DeleteAsync(id, cancellationToken).ConfigureAwait(false);
            await _auditService.LogAsync(nameof(Payment), id, "Delete", existing, null, 1, "System", $"Deleted payment {existing?.PaymentNo ?? id.ToString()}", cancellationToken).ConfigureAwait(false);
        }

        public async Task<IReadOnlyList<Payment>> GetByCustomerAsync(int customerId, CancellationToken cancellationToken = default)
        {
            var all = await _repo.GetAllAsync(cancellationToken);
            return all.Where(p => p.CustomerId == customerId).ToList().AsReadOnly();
        }

        public async Task<IReadOnlyList<Payment>> GetBySupplierAsync(int supplierId, CancellationToken cancellationToken = default)
        {
            var all = await _repo.GetAllAsync(cancellationToken);
            return all.Where(p => p.SupplierId == supplierId).ToList().AsReadOnly();
        }

        public async Task<IReadOnlyList<Payment>> GetByDateRangeAsync(DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default)
        {
            var all = await _repo.GetAllAsync(cancellationToken);
            return all.Where(p => p.PaymentDate >= fromDate && p.PaymentDate <= toDate).ToList().AsReadOnly();
        }
    }
}
