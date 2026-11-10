using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MMNextPOS.Application.Services;
using MMNextPOS.Domain.Models;
using MMNextPOS.Infrastructure;
using MMNextPOS.Infrastructure.Repositories;

namespace MMNextPOS.Application.Services
{
    public class PurchaseReturnService : IPurchaseReturnService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPurchaseReturnRepository _repo;
        private readonly IPurchaseReturnDetailRepository _detailRepo;
        private readonly IProductRepository _productRepo;
        private readonly IAuditService _auditService;

        public PurchaseReturnService(
            IUnitOfWork unitOfWork,
            IPurchaseReturnRepository repo,
            IPurchaseReturnDetailRepository detailRepo,
            IProductRepository productRepo,
            IAuditService auditService)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
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
            await _unitOfWork.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                var result = await _repo.AddAsync(purchaseReturn, cancellationToken).ConfigureAwait(false);
                await _auditService.LogAsync(nameof(PurchaseReturn), result.Id, "Create", null, result, 1, "System", $"Created purchase return {result.ReturnNo}", cancellationToken).ConfigureAwait(false);
                await _unitOfWork.CommitAsync(cancellationToken).ConfigureAwait(false);
                return result;
            }
            catch
            {
                await _unitOfWork.RollbackAsync(cancellationToken).ConfigureAwait(false);
                throw;
            }
        }

        public async Task UpdateAsync(PurchaseReturn purchaseReturn, CancellationToken cancellationToken = default)
        {
            await _unitOfWork.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                var existing = await _repo.GetByIdAsync(purchaseReturn.Id, cancellationToken).ConfigureAwait(false);
                await _repo.UpdateAsync(purchaseReturn, cancellationToken).ConfigureAwait(false);
                await _auditService.LogAsync(nameof(PurchaseReturn), purchaseReturn.Id, "Update", existing, purchaseReturn, 1, "System", $"Updated purchase return {purchaseReturn.ReturnNo}", cancellationToken).ConfigureAwait(false);
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
            await _unitOfWork.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                var existing = await _repo.GetByIdAsync(id, cancellationToken).ConfigureAwait(false);
                await _repo.DeleteAsync(id, cancellationToken).ConfigureAwait(false);
                await _auditService.LogAsync(nameof(PurchaseReturn), id, "Delete", existing, null, 1, "System", $"Deleted purchase return {existing?.ReturnNo ?? id.ToString()}", cancellationToken).ConfigureAwait(false);
                await _unitOfWork.CommitAsync(cancellationToken).ConfigureAwait(false);
            }
            catch
            {
                await _unitOfWork.RollbackAsync(cancellationToken).ConfigureAwait(false);
                throw;
            }
        }

        // Purchase Return Details
        public async Task<IReadOnlyList<PurchaseReturnDetail>> GetPurchaseReturnDetailsAsync(int purchaseReturnId, CancellationToken cancellationToken = default)
        {
            var purchaseReturn = await _unitOfWork.GetPurchaseReturnByIdAsync(purchaseReturnId, cancellationToken).ConfigureAwait(false);
            return purchaseReturn?.PurchaseReturnDetails?.ToList() ?? new List<PurchaseReturnDetail>();
        }

        public async Task<PurchaseReturnDetail> AddPurchaseReturnDetailAsync(PurchaseReturnDetail detail, CancellationToken cancellationToken = default)
        {
            await _unitOfWork.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                var result = await _detailRepo.AddAsync(detail, cancellationToken).ConfigureAwait(false);
                await _auditService.LogAsync(nameof(PurchaseReturnDetail), result.Id, "Create", null, result, 1, "System", $"Created purchase return detail", cancellationToken).ConfigureAwait(false);
                await _unitOfWork.CommitAsync(cancellationToken).ConfigureAwait(false);
                return result;
            }
            catch
            {
                await _unitOfWork.RollbackAsync(cancellationToken).ConfigureAwait(false);
                throw;
            }
        }

        public async Task UpdatePurchaseReturnDetailAsync(PurchaseReturnDetail detail, CancellationToken cancellationToken = default)
        {
            await _unitOfWork.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                var existing = await _detailRepo.GetByIdAsync(detail.Id, cancellationToken).ConfigureAwait(false);
                await _detailRepo.UpdateAsync(detail, cancellationToken).ConfigureAwait(false);
                await _auditService.LogAsync(nameof(PurchaseReturnDetail), detail.Id, "Update", existing, detail, 1, "System", $"Updated purchase return detail", cancellationToken).ConfigureAwait(false);
                await _unitOfWork.CommitAsync(cancellationToken).ConfigureAwait(false);
            }
            catch
            {
                await _unitOfWork.RollbackAsync(cancellationToken).ConfigureAwait(false);
                throw;
            }
        }

        public async Task DeletePurchaseReturnDetailAsync(int id, CancellationToken cancellationToken = default)
        {
            await _unitOfWork.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                var existing = await _detailRepo.GetByIdAsync(id, cancellationToken).ConfigureAwait(false);
                await _detailRepo.DeleteAsync(id, cancellationToken).ConfigureAwait(false);
                await _auditService.LogAsync(nameof(PurchaseReturnDetail), id, "Delete", existing, null, 1, "System", $"Deleted purchase return detail {existing?.Id ?? id}", cancellationToken).ConfigureAwait(false);
                await _unitOfWork.CommitAsync(cancellationToken).ConfigureAwait(false);
            }
            catch
            {
                await _unitOfWork.RollbackAsync(cancellationToken).ConfigureAwait(false);
                throw;
            }
        }

        // Purchase Return with details
        public async Task<PurchaseReturn> CreatePurchaseReturnAsync(PurchaseReturn returnOrder, IEnumerable<PurchaseReturnDetail> details, CancellationToken cancellationToken = default)
        {
            await _unitOfWork.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                var result = await _repo.AddAsync(returnOrder, cancellationToken).ConfigureAwait(false);

                foreach (var detail in details)
                {
                    detail.PurchaseReturnId = result.Id;
                    await _detailRepo.AddAsync(detail, cancellationToken).ConfigureAwait(false);

                    var incremented = await _productRepo.TryIncrementStockAsync(detail.ProductId, detail.Quantity, 1, "PurchaseReturn", cancellationToken).ConfigureAwait(false);
                    if (!incremented)
                    {
                        throw new InvalidOperationException($"Product {detail.ProductId} not found for stock restoration.");
                    }
                }

                await _auditService.LogAsync(nameof(PurchaseReturn), result.Id, "Create", null, returnOrder, 1, "System", $"Created purchase return {result.ReturnNo} with details", cancellationToken).ConfigureAwait(false);
                await _unitOfWork.CommitAsync(cancellationToken).ConfigureAwait(false);
                return result;
            }
            catch
            {
                await _unitOfWork.RollbackAsync(cancellationToken).ConfigureAwait(false);
                throw;
            }
        }

        public async Task<PurchaseReturn> UpdatePurchaseReturnAsync(PurchaseReturn returnOrder, IEnumerable<PurchaseReturnDetail> details, CancellationToken cancellationToken = default)
        {
            await _unitOfWork.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                var existing = await _repo.GetByIdAsync(returnOrder.Id, cancellationToken).ConfigureAwait(false);
                await _repo.UpdateAsync(returnOrder, cancellationToken).ConfigureAwait(false);

                var existingDetails = await _unitOfWork.GetPurchaseReturnByIdAsync(returnOrder.Id, cancellationToken).ConfigureAwait(false);
                var returnDetails = existingDetails?.PurchaseReturnDetails?.ToList() ?? new List<PurchaseReturnDetail>();
                foreach (var detail in returnDetails)
                {
                    await _detailRepo.DeleteAsync(detail.Id, cancellationToken).ConfigureAwait(false);
                }

                foreach (var detail in details)
                {
                    detail.PurchaseReturnId = returnOrder.Id;
                    await _detailRepo.AddAsync(detail, cancellationToken).ConfigureAwait(false);

                    var incremented = await _productRepo.TryIncrementStockAsync(detail.ProductId, detail.Quantity, 1, "PurchaseReturnUpdate", cancellationToken).ConfigureAwait(false);
                    if (!incremented)
                    {
                        throw new InvalidOperationException($"Product {detail.ProductId} not found for stock restoration.");
                    }
                }

                await _auditService.LogAsync(nameof(PurchaseReturn), returnOrder.Id, "Update", null, returnOrder, 1, "System", $"Updated purchase return {returnOrder.ReturnNo} with details", cancellationToken).ConfigureAwait(false);
                await _unitOfWork.CommitAsync(cancellationToken).ConfigureAwait(false);
                return returnOrder;
            }
            catch
            {
                await _unitOfWork.RollbackAsync(cancellationToken).ConfigureAwait(false);
                throw;
            }
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
