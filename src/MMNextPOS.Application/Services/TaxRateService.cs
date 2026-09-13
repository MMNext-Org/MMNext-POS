using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MMNextPOS.Domain.Models;
using MMNextPOS.Infrastructure.Repositories;

namespace MMNextPOS.Application.Services
{
    public class TaxRateService : ITaxRateService
    {
        private readonly ICustomerRepository _customerRepo;
        private readonly ITaxRepository _taxRepo;

        public TaxRateService(
            ICustomerRepository customerRepo,
            ITaxRepository taxRepo)
        {
            _customerRepo = customerRepo ?? throw new ArgumentNullException(nameof(customerRepo));
            _taxRepo = taxRepo ?? throw new ArgumentNullException(nameof(taxRepo));
        }

        public async Task<decimal> GetTaxRateForCustomerAsync(int customerId, CancellationToken cancellationToken = default)
        {
            var customer = await _customerRepo.GetByIdAsync(customerId, cancellationToken).ConfigureAwait(false);
            if (customer == null)
            {
                return await GetDefaultTaxRateAsync(cancellationToken).ConfigureAwait(false);
            }

            // Check if customer has a specific tax type assigned
            if (customer.CustomerType != null)
            {
                var taxType = customer.CustomerType switch
                {
                    "Wholesale" => CustomerTaxType.Wholesale,
                    "TaxExempt" => CustomerTaxType.TaxExempt,
                    "Government" => CustomerTaxType.Government,
                    "Export" => CustomerTaxType.Export,
                    _ => CustomerTaxType.Retail
                };

                return taxType switch
                {
                    CustomerTaxType.Wholesale => 0m,
                    CustomerTaxType.TaxExempt => 0m,
                    CustomerTaxType.Government => 0m,
                    CustomerTaxType.Export => 0m,
                    CustomerTaxType.Retail => await GetDefaultTaxRateAsync(cancellationToken).ConfigureAwait(false),
                    _ => await GetDefaultTaxRateAsync(cancellationToken).ConfigureAwait(false)
                };
            }

            return await GetDefaultTaxRateAsync(cancellationToken).ConfigureAwait(false);
        }

        public async Task<decimal> GetTaxRateForCategoryAsync(int categoryId, CancellationToken cancellationToken = default)
        {
            // Category doesn't have TaxId in current model, fall back to default
            return await GetDefaultTaxRateAsync(cancellationToken).ConfigureAwait(false);
        }

        public async Task<decimal> GetDefaultTaxRateAsync(CancellationToken cancellationToken = default)
        {
            var taxes = await _taxRepo.GetAllAsync(cancellationToken).ConfigureAwait(false);
            var firstTax = taxes.FirstOrDefault(t => t.IsActive);

            if (firstTax != null)
            {
                // Rate is already stored as decimal (e.g., 0.05 for 5%)
                return firstTax.Rate;
            }

            return 0m; // No tax configured
        }
    }
}
