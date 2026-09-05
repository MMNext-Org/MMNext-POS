using System.Threading;
using System.Threading.Tasks;
using Dapper;
using MMNextPOS.Domain.Models;

namespace MMNextPOS.Infrastructure.Repositories
{
    public class LicenseInfoRepository : GenericRepository<LicenseInfo>, ILicenseInfoRepository
    {
        public LicenseInfoRepository(IUnitOfWork unitOfWork)
            : base(unitOfWork, "LicenseInfos")
        {
        }

        public async Task<LicenseInfo?> GetCurrentAsync(CancellationToken cancellationToken = default)
        {
            const string sql = @"SELECT * FROM LicenseInfos WHERE IsActivated = 1 AND IsDeleted = 0 ORDER BY ActivatedDate DESC LIMIT 1";
            var result = await Connection.QueryFirstOrDefaultAsync<LicenseInfo>(
                new CommandDefinition(sql, null, Transaction, cancellationToken: cancellationToken)).ConfigureAwait(false);
            return result;
        }

        public async Task<LicenseInfo?> GetByLicenseKeyAsync(string licenseKey, CancellationToken cancellationToken = default)
        {
            const string sql = @"SELECT * FROM LicenseInfos WHERE LicenseKey = @LicenseKey AND IsDeleted = 0";
            var result = await Connection.QueryFirstOrDefaultAsync<LicenseInfo>(
                new CommandDefinition(sql, new { LicenseKey = licenseKey }, Transaction, cancellationToken: cancellationToken)).ConfigureAwait(false);
            return result;
        }
    }
}
