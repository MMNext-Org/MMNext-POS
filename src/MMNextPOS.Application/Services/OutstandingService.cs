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
        public async Task<IReadOnlyList<CustomerOutstanding>> GetCustomerOutstandingAsync(int customerId, CancellationToken cancellationToken = default)
        {
            var all = await _customerRepo.GetAllAsync(cancellationToken).ConfigureAwait(false);
            return all.Where(o => o.CustomerId == customerId).ToList();
        }

        public async Task<IReadOnlyList<CustomerOutstanding>> GetAllCustomerOutstandingAsync(CancellationToken cancellationToken = default)
        {
            var all = await _customerRepo.GetAllAsync(cancellationToken).ConfigureAwait(false);
            return all.ToList();
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
        public async Task<IReadOnlyList<SupplierOutstanding>> GetSupplierOutstandingAsync(int supplierId, CancellationToken cancellationToken = default)
        {
            var all = await _supplierRepo.GetAllAsync(cancellationToken).ConfigureAwait(false);
            return all.Where(o => o.SupplierId == supplierId).ToList();
        }

        public async Task<IReadOnlyList<SupplierOutstanding>> GetAllSupplierOutstandingAsync(CancellationToken cancellationToken = default)
        {
            var all = await _supplierRepo.GetAllAsync(cancellationToken).ConfigureAwait(false);
            return all.ToList();
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

        // Payment application and clearance
        public async Task<CustomerOutstanding> ApplyCustomerPaymentAsync(int customerId, decimal amount, int? saleId, int? paymentId, string? description, CancellationToken cancellationToken = default)
        {
            if (amount <= 0m)
            {
                throw new ArgumentException("Payment amount must be positive.", nameof(amount));
            }

            var allOutstanding = await GetCustomerOutstandingAsync(customerId, cancellationToken).ConfigureAwait(false);
            var openOutstanding = allOutstanding.Where(o => o.Status == "Open" && o.Balance > 0).OrderBy(o => o.TransactionDate).ToList();

            decimal remainingAmount = amount;
            CustomerOutstanding? lastUpdated = null;

            foreach (var outstanding in openOutstanding)
            {
                if (remainingAmount <= 0) break;

                var appliedAmount = Math.Min(remainingAmount, outstanding.Balance);
                var oldBalance = outstanding.Balance;
                var oldStatus = outstanding.Status;

                outstanding.CreditAmount += appliedAmount;
                outstanding.Balance -= appliedAmount;
                outstanding.Description = (outstanding.Description ?? "") + $" | Payment applied: {appliedAmount:C2}";

                if (outstanding.Balance <= 0)
                {
                    outstanding.Balance = 0;
                    outstanding.Status = "Cleared";
                }

                await UpdateCustomerOutstandingAsync(outstanding, cancellationToken).ConfigureAwait(false);
                lastUpdated = outstanding;
                remainingAmount -= appliedAmount;
            }

            // If there's overpayment, create a credit outstanding record
            if (remainingAmount > 0)
            {
                var creditOutstanding = new CustomerOutstanding
                {
                    CustomerId = customerId,
                    SaleId = saleId,
                    TransactionDate = DateTime.UtcNow,
                    DebitAmount = 0m,
                    CreditAmount = remainingAmount,
                    Balance = -remainingAmount, // Negative balance = credit
                    Description = description ?? $"Overpayment credit (payment {amount:C2}, applied {amount - remainingAmount:C2})",
                    Status = "Credit"
                };
                lastUpdated = await AddCustomerOutstandingAsync(creditOutstanding, cancellationToken).ConfigureAwait(false);
            }

            return lastUpdated ?? throw new InvalidOperationException("No outstanding record was updated or created.");
        }

        public async Task ClearCustomerAccountAsync(int customerId, int? clearedByUserId, string? reason, CancellationToken cancellationToken = default)
        {
            var allOutstanding = await GetCustomerOutstandingAsync(customerId, cancellationToken).ConfigureAwait(false);
            var openOutstanding = allOutstanding.Where(o => o.Status == "Open" && o.Balance > 0).ToList();

            if (!openOutstanding.Any())
            {
                throw new InvalidOperationException($"No open outstanding balance for customer {customerId} to clear.");
            }

            foreach (var outstanding in openOutstanding)
            {
                var oldBalance = outstanding.Balance;
                var oldStatus = outstanding.Status;

                outstanding.CreditAmount += outstanding.Balance;
                outstanding.Balance = 0;
                outstanding.Status = "Cleared";
                outstanding.Description = (outstanding.Description ?? "") + $" | Cleared: {reason ?? "Manual clearance"}";

                await UpdateCustomerOutstandingAsync(outstanding, cancellationToken).ConfigureAwait(false);
            }

            // Also create a clearance audit record
            await _auditService.LogAsync(
                entityName: nameof(CustomerOutstanding),
                entityId: 0,
                action: "ClearAccount",
                oldValues: null,
                newValues: new { CustomerId = customerId, ClearedByUserId = clearedByUserId, Reason = reason, ClearedCount = openOutstanding.Count },
                userId: clearedByUserId,
                userName: null,
                description: $"Customer {customerId} account cleared: {openOutstanding.Count} outstanding records zeroed",
                cancellationToken: cancellationToken).ConfigureAwait(false);
        }
    }
}
