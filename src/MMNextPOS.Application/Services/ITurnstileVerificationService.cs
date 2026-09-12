using System.Threading.Tasks;

namespace MMNextPOS.Application.Services
{
    /// <summary>
    /// Interface for Cloudflare Turnstile verification service.
    /// Provides server-side verification of Turnstile tokens to protect
    /// admin forms and API endpoints from bot submissions.
    /// </summary>
    public interface ITurnstileVerificationService
    {
        /// <summary>
        /// Verifies a Cloudflare Turnstile token.
        /// </summary>
        /// <param name="token">The Turnstile response token from the client.</param>
        /// <returns>True if the token is valid, false otherwise.</returns>
        Task<bool> VerifyAsync(string token);
    }
}