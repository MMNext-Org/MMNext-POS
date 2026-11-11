namespace MMNextPOS.Infrastructure.Repositories
{
    /// <summary>
    /// Repository interface for managing password reset verification codes.
    /// </summary>
    public interface IVerificationCodeRepository
    {
        /// <summary>
        /// Generates a new verification code for the specified user.
        /// </summary>
        Task<string> GenerateCodeAsync(int userId, CancellationToken ct);

        /// <summary>
        /// Validates the provided verification code for the user.
        /// Returns true if the code exists, is not expired, and hasn't been used.
        /// </summary>
        Task<bool> ValidateCodeAsync(int userId, string code, CancellationToken ct);

        /// <summary>
        /// Deletes all expired verification codes.
        /// </summary>
        Task DeleteExpiredAsync(CancellationToken ct);
    }
}