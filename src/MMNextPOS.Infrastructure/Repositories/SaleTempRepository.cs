using MMNextPOS.Domain.Models;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Dapper;
using MySqlConnector;

namespace MMNextPOS.Infrastructure.Repositories
{
    public class SaleTempRepository : GenericRepository<SaleTemp>, ISaleTempRepository
    {
        public SaleTempRepository(IUnitOfWork unitOfWork)
            : base(unitOfWork, "SaleTemps")
        {
        }

public async Task<IReadOnlyList<SaleTemp>> GetDraftsByStatusAsync(string status, CancellationToken cancellationToken = default)
        {
            var results = await Connection.QueryAsync<SaleTemp>(
                new CommandDefinition("SELECT * FROM SaleTemps WHERE Status = @Status", new { Status = status }, Transaction, cancellationToken: cancellationToken));
            return results.AsList();
        }
    }
}