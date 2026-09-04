using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MMNextPOS.Domain.Models;
using MMNextPOS.Infrastructure.Repositories;

namespace MMNextPOS.Application.Services
{
    public class OutstandingService : IOutstandingService
    {
        private readonly ICustomerOutstandingRepository _customerRepo;
        private readonly ISupplierOutstandingRepository _supplierRepo;
        private readonly IAuditService _auditService;

        public OutstandingService(
            ICustomerOutstandingRepository customerRepo,
            ISupplierOutstandingRepository supplierRepo,
            IAuditService auditService)
        {
            _customerRepo = customerRepo ?? throw new ArgumentNullException(nameof(customerRepo));
            _supplierRepo = supplierRepo ?? throw new ArgumentNullException(nameof(supplierRepo));
            _auditService = auditService ?? throw new ArgumentNullException(nameof(auditService));
        }

        // Customer outstanding methods
        public Task<IReadOnlyList<CustomerOutstanding>> GetCustomerOutstandingAsync(int customerId, CancellationToken cancellationToken = default)
        {
            return _customerRepo.GetAllAsync(cancellationToken)
                .ContinueWith(t =>
                {
                    if (t.IsFaulted) throw t.Exception?.Flatten().InnerException ?? new InvalidOperationException("Failed to load customer outstanding.");
                    return (IReadOnlyList<CustomerOutstanding>)t.Result.Where(o => o.CustomerId == customerId).ToList();
                }, cancellationToken);
        }

        public Task<IReadOnlyList<CustomerOutstanding>> GetAllCustomerOutstandingAsync(CancellationToken cancellationToken = default)
        {
            return _customerRepo.GetAllAsync(cancellationToken)
                .ContinueWith(t =>
                {
                    if (t.IsFaulted) throw t.Exception?.Flatten().InnerException ?? new InvalidOperationException("Failed to load customer outstanding.");
                    return (IReadOnlyList<CustomerOutstanding>)t.Result.ToList();
                }, cancellationToken);
        }

        public async Task<CustomerOutstanding> AddCustomerOutstandingAsync(CustomerOutstanding outstanding, CancellationToken cancellationToken = default)
        {
            var result = await _customerRepo.AddAsync(outstanding, cancellationToken).ConfigureAwait(false);
            await _auditService.LogAsync(nameof(CustomerOutstanding), result.Id, "Create", null, result, 1, "System", $"Created customer outstanding for customer {outstanding.CustomerId}", cancellationToken).ConfigureAwait(false);
            return result;
        }

        public async Task UpdateCustomerOutstandingAsync(CustomerOutstanding outstanding, CancellationToken cancellationToken = default)
        {
            var existing = await _customerRepo.GetByIdAsync(outstanding.Id, cancellationToken).ConfigureAwait(false);
            await _customerRepo.UpdateAsync(outstanding, cancellationToken).ConfigureAwait(false);
            await _auditService.LogAsync(nameof(CustomerOutstanding), outstanding.Id, "Update", existing, outstanding, 1, "System", $"Updated customer outstanding for customer {outstanding.CustomerId}", cancellationToken).ConfigureAwait(false);
        }

        public async Task DeleteCustomerOutstandingAsync(int id, CancellationToken cancellationToken = default)
        {
            var existing = await _customerRepo.GetByIdAsync(id, cancellationToken).ConfigureAwait(false);
            await _customerRepo.DeleteAsync(id, cancellationToken).ConfigureAwait(false);
            await _auditService.LogAsync(nameof(CustomerOutstanding), id, "Delete", existing, null, 1, "System", $"Deleted customer outstanding {existing?.Id ?? id}", cancellationToken).ConfigureAwait(false);
        }

        // Supplier outstanding methods
        public Task<IReadOnlyList<SupplierOutstanding>> GetSupplierOutstandingAsync(int supplierId, CancellationToken cancellationToken = default)
        {
            return _supplierRepo.GetAllAsync(cancellationToken)
                .ContinueWith(t =>
                {
                    if (t.IsFaulted) throw t.Exception?.Flatten().InnerException ?? new InvalidOperationException("Failed to load supplier outstanding.");
                    return (IReadOnlyList<SupplierOutstanding>)t.Result.Where(o => o.SupplierId == supplierId).ToList();
                }, cancellationToken);
        }

        public Task<IReadOnlyList<SupplierOutstanding>> GetAllSupplierOutstandingAsync(CancellationToken cancellationToken = default)
        {
            return _supplierRepo.GetAllAsync(cancellationToken)
                .ContinueWith(t =>
                {
                    if (t.IsFaulted) throw t.Exception?.Flatten().InnerException ?? new InvalidOperationException("Failed to load supplier outstanding.");
                    return (IReadOnlyList<SupplierOutstanding>)t.Result.ToList();
                }, cancellationToken);
        }

        public async Task<SupplierOutstanding> AddSupplierOutstandingAsync(SupplierOutstanding outstanding, CancellationToken cancellationToken = default)
        {
            var result = await _supplierRepo.AddAsync(outstanding, cancellationToken).ConfigureAwait(false);
            await _auditService.LogAsync(nameof(SupplierOutstanding), result.Id, "Create", null, result, 1, "System", $"Created supplier outstanding for supplier {outstanding.SupplierId}", cancellationToken).ConfigureAwait(false);
            return result;
        }

        public async Task UpdateSupplierOutstandingAsync(SupplierOutstanding outstanding, CancellationToken cancellationToken = default)
        {
            var existing = await _supplierRepo.GetByIdAsync(outstanding.Id, cancellationToken).ConfigureAwait(false);
            await _supplierRepo.UpdateAsync(outstanding, cancellationToken).ConfigureAwait(false);
            await _auditService.LogAsync(nameof(SupplierOutstanding), outstanding.Id, "Update", existing, outstanding, 1, "System", $"Updated supplier outstanding for supplier {outstanding.SupplierId}", cancellationToken).ConfigureAwait(false);
        }

        public async Task DeleteSupplierOutstandingAsync(int id, CancellationToken cancellationToken = default)
        {
            var existing = await _supplierRepo.GetByIdAsync(id, cancellationToken).ConfigureAwait(false);
            await _supplierRepo.DeleteAsync(id, cancellationToken).ConfigureAwait(false);
            await _auditService.LogAsync(nameof(SupplierOutstanding), id, "Delete", existing, null, 1, "System", $"Deleted supplier outstanding {existing?.Id ?? id}", cancellationToken).ConfigureAwait(false);
        }
    }
}
