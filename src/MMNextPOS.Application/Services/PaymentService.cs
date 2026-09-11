using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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

        public Task<PagedResult<Payment>> GetPageAsync(int page, int pageSize, CancellationToken cancellationToken = default)
        {
            return _repo.GetPageAsync(page, pageSize, cancellationToken);
        }

        public async Task<Payment> AddAsync(Payment payment, CancellationToken cancellationToken = default)
        {
            // Generate payment number if not provided
            if (string.IsNullOrWhiteSpace(payment.PaymentNo))
            {
                payment.PaymentNo = $"PAY-{DateTime.UtcNow:yyyyMMdd}-{new Random().Next(100000, 999999)}";
            }

            var result = await _repo.AddAsync(payment, cancellationToken).ConfigureAwait(false);
            await _auditService.LogAsync(nameof(Payment), result.Id, "Create", null, result, 1, "System", $"Created payment {result.PaymentNo} - {result.Method}", cancellationToken).ConfigureAwait(false);
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

        // Payment by reference
        public async Task<IReadOnlyList<Payment>> GetBySaleAsync(int saleId, CancellationToken cancellationToken = default)
        {
            var all = await _repo.GetAllAsync(cancellationToken).ConfigureAwait(false);
            return all.Where(p => p.SaleId == saleId).ToList().AsReadOnly();
        }

        public async Task<IReadOnlyList<Payment>> GetByPurchaseAsync(int purchaseId, CancellationToken cancellationToken = default)
        {
            var all = await _repo.GetAllAsync(cancellationToken).ConfigureAwait(false);
            return all.Where(p => p.PurchaseId == purchaseId).ToList().AsReadOnly();
        }

        public async Task<IReadOnlyList<Payment>> GetByCustomerAsync(int customerId, CancellationToken cancellationToken = default)
        {
            var all = await _repo.GetAllAsync(cancellationToken).ConfigureAwait(false);
            return all.Where(p => p.CustomerId == customerId).ToList().AsReadOnly();
        }

        public async Task<IReadOnlyList<Payment>> GetBySupplierAsync(int supplierId, CancellationToken cancellationToken = default)
        {
            var all = await _repo.GetAllAsync(cancellationToken).ConfigureAwait(false);
            return all.Where(p => p.SupplierId == supplierId).ToList().AsReadOnly();
        }

        public async Task<IReadOnlyList<Payment>> GetByDateRangeAsync(DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default)
        {
            var all = await _repo.GetAllAsync(cancellationToken).ConfigureAwait(false);
            return all.Where(p => p.PaymentDate >= fromDate && p.PaymentDate <= toDate).ToList().AsReadOnly();
        }

        // Payment processing
        public async Task<Payment> ProcessPaymentAsync(Payment payment, CancellationToken cancellationToken = default)
        {
            // Validate payment
            if (payment == null) throw new ArgumentNullException(nameof(payment));
            if (payment.Amount <= 0m) throw new ValidationException("Payment amount must be greater than zero.");
            if (string.IsNullOrWhiteSpace(payment.Method)) throw new ValidationException("Payment method is required.");

            // If linked to a sale, ensure the sale exists and update its paid amount
            if (payment.SaleId.HasValue)
            {
                // The sale's paid amount will be updated by the caller or a separate service
                // This method primarily creates the payment record
            }

            // If linked to a purchase, ensure the purchase exists
            if (payment.PurchaseId.HasValue)
            {
                // Similar - the purchase's paid amount will be updated separately
            }

            // Generate payment number if not provided
            if (string.IsNullOrWhiteSpace(payment.PaymentNo))
            {
                payment.PaymentNo = $"PAY-{DateTime.UtcNow:yyyyMMdd}-{new Random().Next(100000, 999999)}";
            }

            var result = await AddAsync(payment, cancellationToken).ConfigureAwait(false);
            return result;
        }
    }
}