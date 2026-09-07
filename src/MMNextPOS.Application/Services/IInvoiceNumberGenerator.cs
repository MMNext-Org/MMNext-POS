using System.Threading;
using System.Threading.Tasks;

namespace MMNextPOS.Application.Services
{
    /// <summary>
    /// Generates monotonic, unique, per-year invoice numbers.
    /// Implementations must participate in the active <see cref="MMNextPOS.Infrastructure.IUnitOfWork"/>
    /// transaction so that two concurrent calls cannot issue the same number.
    /// </summary>
    public interface IInvoiceNumberGenerator
    {
        /// <summary>
        /// Reserves and returns the next invoice number for the current year.
        /// Format produced by <see cref="InvoiceNumberFormat"/>: PREFIX-YYYY-NNNNNN.
        /// </summary>
        /// <param name="prefix">Short alpha prefix, e.g. "INV" or "QT".</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        Task<string> NextAsync(string prefix, CancellationToken cancellationToken = default);
    }
}
