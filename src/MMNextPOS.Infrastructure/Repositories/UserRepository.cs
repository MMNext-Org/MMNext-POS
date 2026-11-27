using System.Threading;
using System.Threading.Tasks;
using Dapper;
using MMNextPOS.Domain.Models;

namespace MMNextPOS.Infrastructure.Repositories
{
    public class UserRepository : GenericRepository<User>, IUserRepository
    {
        public UserRepository(IUnitOfWork unitOfWork)
            : base(unitOfWork, "Users")
        {
        }

        public async Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
        {
            const string sql = @"
SELECT *
FROM Users
WHERE IsDeleted = 0
  AND LOWER(Username) = LOWER(@Username)
ORDER BY Id
LIMIT 1";

            var command = new CommandDefinition(
                sql,
                new { Username = username },
                transaction: Transaction,
                cancellationToken: cancellationToken);

            return await Connection.QuerySingleOrDefaultAsync<User>(command).ConfigureAwait(false);
        }
    }
}
