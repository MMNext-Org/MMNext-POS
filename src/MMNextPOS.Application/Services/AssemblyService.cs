using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MMNextPOS.Domain.Models;
using MMNextPOS.Infrastructure;
using MMNextPOS.Infrastructure.Repositories;
using MMNextPOS.Application.Services;

namespace MMNextPOS.Application.Services
{
    public class AssemblyService : IAssemblyService
    {
        private readonly IAssemblyRepository _assemblyRepo;
        private readonly IAssemblyDetailRepository _detailRepo;
        private readonly IProductRepository _productRepo;
        private readonly IStockMovementService _stockMovementService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAuditService _auditService;

        public AssemblyService(
            IAssemblyRepository assemblyRepo,
            IAssemblyDetailRepository detailRepo,
            IProductRepository productRepo,
            IStockMovementService stockMovementService,
            IUnitOfWork unitOfWork,
            IAuditService auditService)
        {
            _assemblyRepo = assemblyRepo ?? throw new ArgumentNullException(nameof(assemblyRepo));
            _detailRepo = detailRepo ?? throw new ArgumentNullException(nameof(detailRepo));
            _productRepo = productRepo ?? throw new ArgumentNullException(nameof(productRepo));
            _stockMovementService = stockMovementService ?? throw new ArgumentNullException(nameof(stockMovementService));
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _auditService = auditService ?? throw new ArgumentNullException(nameof(auditService));
        }

        // CRUD Operations
        public async Task<Assembly?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _assemblyRepo.GetByIdAsync(id, cancellationToken);
        }

        public async Task<IReadOnlyList<Assembly>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _assemblyRepo.GetAllAsync(cancellationToken);
        }

        public async Task<Assembly> CreateAsync(Assembly assembly, IEnumerable<AssemblyDetail> components, int userId, CancellationToken cancellationToken = default)
        {
            if (assembly == null) throw new ArgumentNullException(nameof(assembly));
            if (components == null || !components.Any())
                throw new ArgumentException("Assembly must have at least one component.", nameof(components));

            await _unitOfWork.BeginTransactionAsync(cancellationToken);

            try
            {
                // Calculate total cost
                assembly.TotalCost = components.Sum(c => c.Quantity * c.UnitCost);
                assembly.AssemblyDate = DateTime.UtcNow;
                assembly.Status = "Active";
                assembly.CreatedByUserId = userId;

                var created = await _assemblyRepo.AddAsync(assembly, cancellationToken);

                foreach (var detail in components)
                {
                    detail.AssemblyId = created.Id;
                    detail.LineTotal = detail.Quantity * detail.UnitCost;
                    await _detailRepo.AddAsync(detail, cancellationToken);
                }

                await _auditService.LogAsync(nameof(Assembly), created.Id, "Create", null, created, userId, "System",
                    $"Created assembly {assembly.AssemblyNo} with {components.Count()} components", cancellationToken);

                await _unitOfWork.CommitAsync(cancellationToken);
                return created;
            }
            catch
            {
                await _unitOfWork.RollbackAsync(cancellationToken);
                throw;
            }
        }

        public async Task<Assembly> UpdateAsync(Assembly assembly, IEnumerable<AssemblyDetail> components, int userId, CancellationToken cancellationToken = default)
        {
            if (assembly == null) throw new ArgumentNullException(nameof(assembly));

            await _unitOfWork.BeginTransactionAsync(cancellationToken);

            try
            {
                var existing = await _assemblyRepo.GetByIdAsync(assembly.Id, cancellationToken);
                if (existing == null)
                    throw new KeyNotFoundException($"Assembly {assembly.Id} not found");

                // Update costs
                assembly.TotalCost = components?.Sum(c => c.Quantity * c.UnitCost) ?? 0m;
                await _assemblyRepo.UpdateAsync(assembly, cancellationToken);

                // Update details - delete existing and re-add
                if (components != null)
                {
                    var existingDetails = await _detailRepo.GetAllAsync(cancellationToken);
                    var currentDetails = existingDetails.Where(d => d.AssemblyId == assembly.Id).ToList();
                    foreach (var d in currentDetails)
                    {
                        await _detailRepo.DeleteAsync(d.Id, cancellationToken);
                    }

                    foreach (var detail in components)
                    {
                        detail.AssemblyId = assembly.Id;
                        detail.LineTotal = detail.Quantity * detail.UnitCost;
                        await _detailRepo.AddAsync(detail, cancellationToken);
                    }
                }

                await _auditService.LogAsync(nameof(Assembly), assembly.Id, "Update", existing, assembly, userId, "System",
                    $"Updated assembly {assembly.AssemblyNo}", cancellationToken);

                await _unitOfWork.CommitAsync(cancellationToken);
                return assembly;
            }
            catch
            {
                await _unitOfWork.RollbackAsync(cancellationToken);
                throw;
            }
        }

        public async Task DeleteAsync(int id, int userId, CancellationToken cancellationToken = default)
        {
            var existing = await _assemblyRepo.GetByIdAsync(id, cancellationToken);
            if (existing == null)
                throw new KeyNotFoundException($"Assembly {id} not found");

            await _auditService.LogAsync(nameof(Assembly), id, "Delete", existing, null, userId, "System",
                $"Deleted assembly {existing.AssemblyNo}", cancellationToken);
            await _assemblyRepo.DeleteAsync(id, cancellationToken);
        }

        // Assembly Operations
        public async Task<Assembly> BuildAssemblyAsync(int assemblyId, int quantity, int userId, CancellationToken cancellationToken = default)
        {
            if (quantity <= 0)
                throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be positive");

            var assembly = await _assemblyRepo.GetByIdAsync(assemblyId, cancellationToken);
            if (assembly == null)
                throw new KeyNotFoundException($"Assembly {assemblyId} not found");

            var details = await _detailRepo.GetAllAsync(cancellationToken);
            var components = details.Where(d => d.AssemblyId == assemblyId).ToList();

            await _unitOfWork.BeginTransactionAsync(cancellationToken);

            try
            {
                // Check component stock availability
                foreach (var component in components)
                {
                    var product = await _productRepo.GetByIdAsync(component.ComponentProductId, cancellationToken);
                    if (product == null)
                        throw new ValidationException($"Component product {component.ComponentProductId} not found");

                    var requiredQty = component.Quantity * quantity;
                    if (product.StockQuantity < requiredQty)
                        throw new ValidationException($"Insufficient stock for component {product.Name}. Required: {requiredQty}, Available: {product.StockQuantity}");
                }

                // Create stock movement for components (decrement)
                var componentDetails = components.Select(c => new SaleDetail
                {
                    ProductId = c.ComponentProductId,
                    Quantity = c.Quantity * quantity,
                    UnitPrice = c.UnitCost
                }).ToList();

                await _stockMovementService.AddAssemblyMovementAsync(assemblyId, components, userId, cancellationToken);

                // Increment stock for the output product
                var outputSuccess = await _productRepo.TryIncrementStockAsync(assembly.OutputProductId, quantity * assembly.OutputQuantity, userId, $"Assembly {assembly.AssemblyNo}", cancellationToken);
                if (!outputSuccess)
                    throw new ValidationException($"Failed to increment stock for assembly output {assembly.OutputProductId}");

                // Update assembly costs
                assembly.TotalCost = components.Sum(c => c.Quantity * c.UnitCost * quantity);
                await _assemblyRepo.UpdateAsync(assembly, cancellationToken);

                await _auditService.LogAsync(nameof(Assembly), assemblyId, "Build", null, assembly, userId, "System",
                    $"Built {quantity} units of assembly {assembly.AssemblyNo}", cancellationToken);

                await _unitOfWork.CommitAsync(cancellationToken);
                return assembly;
            }
            catch
            {
                await _unitOfWork.RollbackAsync(cancellationToken);
                throw;
            }
        }

        public async Task<Assembly> DeassembleAsync(int assemblyId, int quantity, int userId, CancellationToken cancellationToken = default)
        {
            if (quantity <= 0)
                throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be positive");

            var assembly = await _assemblyRepo.GetByIdAsync(assemblyId, cancellationToken);
            if (assembly == null)
                throw new KeyNotFoundException($"Assembly {assemblyId} not found");

            var details = await _detailRepo.GetAllAsync(cancellationToken);
            var components = details.Where(d => d.AssemblyId == assemblyId).ToList();

            await _unitOfWork.BeginTransactionAsync(cancellationToken);

            try
            {
                // Create stock movement for components (increment back)
                var componentDetails = components.Select(c => new SaleDetail
                {
                    ProductId = c.ComponentProductId,
                    Quantity = c.Quantity * quantity,
                    UnitPrice = c.UnitCost
                }).ToList();

                await _stockMovementService.AddDeassemblyMovementAsync(assemblyId, components, userId, cancellationToken);

                // Increment stock for each component
                foreach (var component in components)
                {
                    var componentQty = component.Quantity * quantity;
                    var success = await _productRepo.TryIncrementStockAsync(component.ComponentProductId, componentQty, userId, $"Deassembly {assembly.AssemblyNo}", cancellationToken);
                    if (!success)
                        throw new ValidationException($"Failed to increment stock for component {component.ComponentProductId}");
                }

                // Decrement stock for the output product
                var outputQty = quantity * assembly.OutputQuantity;
                var outputSuccess = await _productRepo.TryDecrementStockAsync(assembly.OutputProductId, outputQty, userId, $"Deassembly {assembly.AssemblyNo}", cancellationToken);
                if (!outputSuccess)
                {
                    var outputProduct = await _productRepo.GetByIdAsync(assembly.OutputProductId, cancellationToken);
                    throw new ValidationException($"Insufficient stock for assembly output {outputProduct?.Name}");
                }

                await _auditService.LogAsync(nameof(Assembly), assemblyId, "Deassemble", null, assembly, userId, "System",
                    $"Deassembled {quantity} units of assembly {assembly.AssemblyNo}", cancellationToken);

                await _unitOfWork.CommitAsync(cancellationToken);
                return assembly;
            }
            catch
            {
                await _unitOfWork.RollbackAsync(cancellationToken);
                throw;
            }
        }

        public async Task<IReadOnlyList<AssemblyDetail>> GetAssemblyDetailsAsync(int assemblyId, CancellationToken cancellationToken = default)
        {
            var all = await _detailRepo.GetAllAsync(cancellationToken);
            return all.Where(d => d.AssemblyId == assemblyId).ToList();
        }

        public async Task<decimal> CalculateAssemblyCostAsync(int assemblyId, CancellationToken cancellationToken = default)
        {
            var details = await GetAssemblyDetailsAsync(assemblyId, cancellationToken);
            return details.Sum(d => d.Quantity * d.UnitCost);
        }

        public async Task<AssemblyCostVariance> GetCostVarianceAsync(int assemblyId, CancellationToken cancellationToken = default)
        {
            var assembly = await _assemblyRepo.GetByIdAsync(assemblyId, cancellationToken);
            if (assembly == null)
                throw new KeyNotFoundException($"Assembly {assemblyId} not found");

            var details = await GetAssemblyDetailsAsync(assemblyId, cancellationToken);
            var standardCost = details.Sum(d => d.Quantity * d.UnitCost);

            // For now, actual cost = standard cost (in real implementation, could vary by batch, supplier, etc.)
            // In a real scenario, you'd track actual costs per component purchase/receipt
            var actualCost = standardCost; // Placeholder - could calculate from recent purchases

            return new AssemblyCostVariance
            {
                AssemblyId = assemblyId,
                StandardCost = standardCost,
                ActualCost = actualCost
            };
        }
    }
}
