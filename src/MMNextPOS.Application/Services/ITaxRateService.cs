using System;
using System.Threading;
using System.Threading.Tasks;

namespace MMNextPOS.Application.Services
{
    /// <summary>
    /// Service for determining tax rates based on customer type, jurisdiction, or product category.
    /// </summary>
    public interface ITaxRateService
    {
        /// <summary>
        /// Gets the applicable tax rate for a customer.
        /// </summary>
        /// <param name="customerId">The customer ID.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The tax rate as a decimal (e.g., 0.05m for 5%). Returns 0 if no tax applies.</returns>
        Task<decimal> GetTaxRateForCustomerAsync(int customerId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the applicable tax rate for a product category.
        /// </summary>
        /// <param name="categoryId">The product category ID.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The tax rate as a decimal.</returns>
        Task<decimal> GetTaxRateForCategoryAsync(int categoryId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the default tax rate when no specific rule applies.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The default tax rate as a decimal.</returns>
        Task<decimal> GetDefaultTaxRateAsync(CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Customer type enumeration for tax jurisdiction.
    /// </summary>
    public enum CustomerTaxType
    {
        /// <summary>Standard retail customer</summary>
        Retail = 0,
        /// <summary>Wholesale/B2B customer</summary>
        Wholesale = 1,
        /// <summary>Tax-exempt customer</summary>
        TaxExempt = 2,
        /// <summary>Government customer</summary>
        Government = 3,
        /// <summary>Export customer (zero-rated)</summary>
        Export = 4
    }
}
