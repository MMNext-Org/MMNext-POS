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
    public class PurchaseReturnService : IPurchaseReturnService
    {
        private readonly IPurchaseReturnRepository _repo;
        private readonly IPurchaseReturnDetailRepository _detailRepo;
        private readonly IProductRepository _productRepo;
        private readonly IAuditService _auditService;

        public PurchaseReturnService(
            IPurchaseReturnRepository repo,
            IPurchaseReturnDetailRepository detailRepo,
            IProductRepository productRepo,
            IAuditService auditService)
        {
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
            _detailRepo = detailRepo ?? throw new ArgumentNullException(nameof(detailRepo));
            _productRepo = productRepo ?? throw new ArgumentNullException(nameof(productRepo));
            _auditService = auditService ?? throw new ArgumentNullException(nameof(auditService));
        }

        public Task<PurchaseReturn?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return _repo.GetByIdAsync(id, cancellationToken);
        }

        public Task<IReadOnlyList<PurchaseReturn>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return _repo.GetAllAsync(cancellationToken);
        }

        public async Task<PagedResult<PurchaseReturn>> GetPageAsync(int page, int pageSize, CancellationToken cancellationToken = default)
        {
            return await _repo.GetPageAsync(page, pageSize, cancellationToken);
        }

        public async Task<PurchaseReturn> AddAsync(PurchaseReturn purchaseReturn, CancellationToken cancellationToken = default)
        {
            var result = await _repo.AddAsync(purchaseReturn, cancellationToken).ConfigureAwait(false);
            await _auditService.LogAsync(nameof(PurchaseReturn), result.Id, "Create", null, result, 1, "System", $"Created purchase return {result.ReturnNo}", cancellationToken).ConfigureAwait(false);
            return result;
        }

        public async Task UpdateAsync(PurchaseReturn purchaseReturn, CancellationToken cancellationToken = default)
        {
            var existing = await _repo.GetByIdAsync(purchaseReturn.Id, cancellationToken).ConfigureAwait(false);
            await _repo.UpdateAsync(purchaseReturn, cancellationToken).ConfigureAwait(false);
            await _auditService.LogAsync(nameof(PurchaseReturn), purchaseReturn.Id, "Update", existing, purchaseReturn, 1, "System", $"Updated purchase return {purchaseReturn.ReturnNo}", cancellationToken).ConfigureAwait(false);
        }

        public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var existing = await _repo.GetByIdAsync(id, cancellationToken).ConfigureAwait(false);
            await _repo.DeleteAsync(id, cancellationToken).ConfigureAwait(false);
            await _auditService.LogAsync(nameof(PurchaseReturn), id, "Delete", existing, null, 1, "System", $"Deleted purchase return {existing?.ReturnNo ?? id.ToString()}", cancellationToken).ConfigureAwait(false);
        }

        // Purchase Return Details
        public async Task<IReadOnlyList<PurchaseReturnDetail>> GetPurchaseReturnDetailsAsync(int purchaseReturnId, CancellationToken cancellationToken = default)
        {
            var all = await _detailRepo.GetAllAsync(cancellationToken);
            return all.Where(d => d.PurchaseReturnId == purchaseReturnId).ToList();
        }

        public async Task<PurchaseReturnDetail> AddPurchaseReturnDetailAsync(PurchaseReturnDetail detail, CancellationToken cancellationToken = default)
        {
            var result = await _detailRepo.AddAsync(detail, cancellationToken).ConfigureAwait(false);
            await _auditService.LogAsync(nameof(PurchaseReturnDetail), result.Id, "Create", null, result, 1, "System", $"Created purchase return detail", cancellationToken).ConfigureAwait(false);
            return result;
        }

        public async Task UpdatePurchaseReturnDetailAsync(PurchaseReturnDetail detail, CancellationToken cancellationToken = default)
        {
            var existing = await _detailRepo.GetByIdAsync(detail.Id, cancellationToken).ConfigureAwait(false);
            await _detailRepo.UpdateAsync(detail, cancellationToken).ConfigureAwait(false);
            await _auditService.LogAsync(nameof(PurchaseReturnDetail), detail.Id, "Update", existing, detail, 1, "System", $"Updated purchase return detail", cancellationToken).ConfigureAwait(false);
        }

        public async Task DeletePurchaseReturnDetailAsync(int id, CancellationToken cancellationToken = default)
        {
            var existing = await _detailRepo.GetByIdAsync(id, cancellationToken).ConfigureAwait(false);
            await _detailRepo.DeleteAsync(id, cancellationToken).ConfigureAwait(false);
            await _auditService.LogAsync(nameof(PurchaseReturnDetail), id, "Delete", existing, null, 1, "System", $"Deleted purchase return detail {existing?.Id ?? id}", cancellationToken).ConfigureAwait(false);
        }

        // Purchase Return with details
        public async Task<PurchaseReturn> CreatePurchaseReturnAsync(PurchaseReturn returnOrder, IEnumerable<PurchaseReturnDetail> details, CancellationToken cancellationToken = default)
        {
            var result = await _repo.AddAsync(returnOrder, cancellationToken).ConfigureAwait(false);

            foreach (var detail in details)
            {
                detail.PurchaseReturnId = result.Id;
                await _detailRepo.AddAsync(detail, cancellationToken).ConfigureAwait(false);
            }

            await _auditService.LogAsync(nameof(PurchaseReturn), result.Id, "Create", null, returnOrder, 1, "System", $"Created purchase return {result.ReturnNo} with details", cancellationToken).ConfigureAwait(false);
            return result;
        }

        public async Task<PurchaseReturn> UpdatePurchaseReturnAsync(PurchaseReturn returnOrder, IEnumerable<PurchaseReturnDetail> details, CancellationToken cancellationToken = default)
        {
            var existing = await _repo.GetByIdAsync(returnOrder.Id, cancellationToken).ConfigureAwait(false);
            await _repo.UpdateAsync(returnOrder, cancellationToken).ConfigureAwait(false);

            var existingDetails = await _detailRepo.GetAllAsync(cancellationToken).ConfigureAwait(false);
            var returnDetails = existingDetails.Where(d => d.PurchaseReturnId == returnOrder.Id).ToList();
            foreach (var detail in returnDetails)
            {
                await _detailRepo.DeleteAsync(detail.Id, cancellationToken).ConfigureAwait(false);
            }

            foreach (var detail in details)
            {
                detail.PurchaseReturnId = returnOrder.Id;
                await _detailRepo.AddAsync(detail, cancellationToken).ConfigureAwait(false);
            }

            await _auditService.LogAsync(nameof(PurchaseReturn), returnOrder.Id, "Update", null, returnOrder, 1, "System", $"Updated purchase return {returnOrder.ReturnNo} with details", cancellationToken).ConfigureAwait(false);
            return returnOrder;
        }

        // Stock validation
        public async Task<int> GetAvailableStockAsync(int productId, CancellationToken cancellationToken = default)
        {
            var product = await _productRepo.GetByIdAsync(productId, cancellationToken).ConfigureAwait(false);
            return product?.StockQuantity ?? 0;
        }

        public async Task<bool> HasSufficientStockAsync(int productId, int quantity, CancellationToken cancellationToken = default)
        {
            var available = await GetAvailableStockAsync(productId, cancellationToken).ConfigureAwait(false);
            return available >= quantity;
        }
    }
}
