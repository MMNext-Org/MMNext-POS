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
    public class LinkedStockService : ILinkedStockService
    {
        private readonly ILinkedStockRepository _linkedStockRepo;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAuditService _auditService;

        public LinkedStockService(
            ILinkedStockRepository linkedStockRepo,
            IUnitOfWork unitOfWork,
            IAuditService auditService)
        {
            _linkedStockRepo = linkedStockRepo;
            _unitOfWork = unitOfWork;
            _auditService = auditService;
        }

        public async Task<LinkedStock?> GetLinkedStockByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _linkedStockRepo.GetByIdAsync(id, cancellationToken);
        }

        public async Task<IReadOnlyList<LinkedStock>> GetLinkedStocksAsync(int? productId = null, string? linkType = null, CancellationToken cancellationToken = default)
        {
            var all = await _linkedStockRepo.GetAllAsync(cancellationToken);
            var filtered = all;

            if (productId.HasValue)
                filtered = filtered.Where(l => l.PrimaryProductId == productId.Value).ToList();

            if (!string.IsNullOrEmpty(linkType))
                filtered = filtered.Where(l => l.LinkType == linkType).ToList();

            var ordered = filtered.OrderBy(l => l.PrimaryProductId).ToList();
            return ordered.AsReadOnly();

        async Task<PagedResult<LinkedStock>> GetLinkedStocksPageAsync(int page, int pageSize, int? productId = null, string? linkType = null, CancellationToken cancellationToken = default)
                {
                    var all = await _linkedStockRepo.GetAllAsync(cancellationToken);
                    var filtered = all;

                    if (productId.HasValue)
                        filtered = filtered.Where(l => l.PrimaryProductId == productId.Value).ToList();

                    if (!string.IsNullOrEmpty(linkType))
                        filtered = filtered.Where(l => l.LinkType == linkType).ToList();

                    // Apply paging to the filtered list.
                    return await _linkedStockRepo.GetPageAsync(page, pageSize, cancellationToken);
                        }

        public async Task<LinkedStock> AddLinkedStockAsync(LinkedStock linkedStock, CancellationToken cancellationToken = default)
        {
            var result = await _linkedStockRepo.AddAsync(linkedStock, cancellationToken);

            await _auditService.LogAsync(
                "LinkedStock", 
                result.Id, 
                "Create", 
                null, 
                linkedStock, 
                1, 
                "System", 
                $"Created linked stock {result.Id}", 
                cancellationToken);

            return result;
        }

        public async Task UpdateLinkedStockAsync(LinkedStock linkedStock, CancellationToken cancellationToken = default)
        {
            var existing = await _linkedStockRepo.GetByIdAsync(linkedStock.Id, cancellationToken);
            if (existing == null)
            {
                throw new KeyNotFoundException($"LinkedStock {linkedStock.Id} not found");
            }

            await _linkedStockRepo.UpdateAsync(linkedStock, cancellationToken);

            await _auditService.LogAsync(
                "LinkedStock", 
                linkedStock.Id, 
                "Update", 
                existing, 
                linkedStock, 
                1, 
                "System", 
                $"Updated linked stock {linkedStock.Id}", 
                cancellationToken);
        }

        public async Task DeleteLinkedStockAsync(int id, CancellationToken cancellationToken = default)
        {
            var existing = await _linkedStockRepo.GetByIdAsync(id, cancellationToken);
            if (existing == null)
            {
                throw new KeyNotFoundException($"LinkedStock {id} not found");
            }

            await _linkedStockRepo.DeleteAsync(id, cancellationToken);

            await _auditService.LogAsync(
                "LinkedStock", 
                id, 
                "Delete", 
                existing, 
                null, 
                1, 
                "System", 
                $"Deleted linked stock {id}", 
                cancellationToken);
        }
    }
}