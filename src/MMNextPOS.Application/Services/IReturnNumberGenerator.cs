using System.Threading;
using System.Threading.Tasks;

namespace MMNextPOS.Application.Services
{
    /// <summary>
    /// Generates monotonic, unique, per-year return numbers.
    /// Implementations must participate in the active <see cref="MMNextPOS.Infrastructure.IUnitOfWork"/>
    /// transaction so that two concurrent calls cannot issue the same number.
    /// </summary>
    public interface IReturnNumberGenerator
    {
        /// <summary>
        /// Reserves and returns the next return number for the current year.
        /// Format produced by <see cref="ReturnNumberFormat"/>: PREFIX-YYYY-NNNNNN.
        /// </summary>
        /// <param name="prefix">Short alpha prefix, e.g. "RET" or "RFD".</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        Task<string> NextAsync(string prefix, CancellationToken cancellationToken = default);
    }
}