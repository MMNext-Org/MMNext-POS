using Dapper;
using MMNextPOS.Domain.Models;
using MySqlConnector;

namespace MMNextPOS.Infrastructure.Repositories
{
    public interface IJournalVoucherRepository : IRepository<JournalVoucher>
    {
        /// <summary>
        /// Get header by voucher number (e.g. JV-2026-000001).
        /// </summary>
        Task<JournalVoucher?> GetByVoucherNoAsync(string voucherNo, CancellationToken cancellationToken = default);
    }

    public class JournalVoucherRepository : GenericRepository<JournalVoucher>, IJournalVoucherRepository
    {
        public JournalVoucherRepository(IUnitOfWork unitOfWork)
            : base(unitOfWork, "JournalVouchers")
        {
        }

        public async Task<JournalVoucher?> GetByVoucherNoAsync(string voucherNo, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(voucherNo))
                return null;

            var sql = $"SELECT * FROM JournalVouchers WHERE VoucherNo = @VoucherNo AND IsDeleted = 0";
            return await Connection.QuerySingleOrDefaultAsync<JournalVoucher>(
                new CommandDefinition(sql, new { VoucherNo = voucherNo }, transaction: Transaction,
                    cancellationToken: cancellationToken)).ConfigureAwait(false);
        }
    }

    public interface IJournalVoucherDetailRepository : IRepository<JournalVoucherDetail>
    {
        /// <summary>
        /// Get all details for a given voucher header.
        /// </summary>
        Task<IReadOnlyList<JournalVoucherDetail>> GetByVoucherIdAsync(int voucherId, CancellationToken cancellationToken = default);
    }

    public class JournalVoucherDetailRepository : GenericRepository<JournalVoucherDetail>, IJournalVoucherDetailRepository
    {
        public JournalVoucherDetailRepository(IUnitOfWork unitOfWork)
            : base(unitOfWork, "JournalVoucherDetails")
        {
        }

        public async Task<IReadOnlyList<JournalVoucherDetail>> GetByVoucherIdAsync(int voucherId, CancellationToken cancellationToken = default)
        {
            var sql = $"SELECT * FROM JournalVoucherDetails WHERE JournalVoucherId = @VoucherId AND IsDeleted = 0 ORDER BY LineNo";
            return (await Connection.QueryAsync<JournalVoucherDetail>(
                new CommandDefinition(sql, new { VoucherId = voucherId }, transaction: Transaction,
                    cancellationToken: cancellationToken))).AsList();
        }
    }
}
