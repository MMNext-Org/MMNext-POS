using System;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using MMNextPOS.Domain.Models;

namespace MMNextPOS.Infrastructure.Repositories
{
    /// <summary>
    /// Implementation of <see cref="IVerificationCodeRepository"/> using Dapper and MySQL.
    /// </summary>
    public class VerificationCodeRepository : RepositoryBase, IVerificationCodeRepository
    {
        private const string TableName = "VerificationCodes";

        public VerificationCodeRepository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public async Task<string> GenerateCodeAsync(int userId, CancellationToken ct)
        {
            var code = Guid.NewGuid().ToString("N")[..6]; // 6-character alphanumeric
            var createdAt = DateTime.UtcNow;
            var expiresAt = createdAt.AddMinutes(10);

            var sql = $@"
                INSERT INTO {TableName} (UserId, Code, CreatedAt, ExpiresAt)
                VALUES (@UserId, @Code, @CreatedAt, @ExpiresAt)";

            await _unitOfWork.Connection.ExecuteAsync(sql, new
            {
                UserId = userId,
                Code = code,
                CreatedAt = createdAt,
                ExpiresAt = expiresAt
            }, transaction: _unitOfWork.Transaction);

            return code;
        }

        public async Task<bool> ValidateCodeAsync(int userId, string code, CancellationToken ct)
        {
            var sql = $@"
                SELECT 1
                FROM {TableName}
                WHERE UserId = @UserId
                  AND Code = @Code
                  AND ExpiresAt > @Now";

            var parameters = new { userId, code, Now = DateTime.UtcNow };
            var rows = await _unitOfWork.Connection.ExecuteAsync(sql, parameters, transaction: _unitOfWork.Transaction);
            return rows > 0;
        }

        public async Task DeleteExpiredAsync(CancellationToken ct)
        {
            var sql = $@"
                DELETE FROM {TableName}
                WHERE ExpiresAt < @Now";

            var parameters = new { Now = DateTime.UtcNow };
            await _unitOfWork.Connection.ExecuteAsync(sql, parameters, transaction: _unitOfWork.Transaction);
        }
    }
}