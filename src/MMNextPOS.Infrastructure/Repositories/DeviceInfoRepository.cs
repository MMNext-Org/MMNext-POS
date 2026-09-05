using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using MMNextPOS.Domain.Models;

namespace MMNextPOS.Infrastructure.Repositories
{
    public class DeviceInfoRepository : GenericRepository<DeviceInfo>, IDeviceInfoRepository
    {
        public DeviceInfoRepository(IUnitOfWork unitOfWork)
            : base(unitOfWork, "DeviceInfos")
        {
        }

        public async Task<DeviceInfo?> GetByFingerprintAsync(string fingerprint, CancellationToken cancellationToken = default)
        {
            const string sql = "SELECT * FROM DeviceInfos WHERE DeviceFingerprint = @Fingerprint AND IsDeleted = 0 LIMIT 1";
            return await Connection.QueryFirstOrDefaultAsync<DeviceInfo>(
                new CommandDefinition(sql, new { Fingerprint = fingerprint }, Transaction, cancellationToken: cancellationToken))
                .ConfigureAwait(false);
        }

        public async Task<int> CountActiveAsync(CancellationToken cancellationToken = default)
        {
            const string sql = "SELECT COUNT(*) FROM DeviceInfos WHERE IsActive = 1 AND IsDeleted = 0";
            return await Connection.ExecuteScalarAsync<int>(
                new CommandDefinition(sql, null, Transaction, cancellationToken: cancellationToken))
                .ConfigureAwait(false);
        }
    }
}
