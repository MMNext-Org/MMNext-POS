using System;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.DependencyInjection;
using MySqlConnector;
using MMNextPOS.Infrastructure.Repositories;
using Xunit;

namespace MMNextPOS.Infrastructure.Tests
{
    [Collection(nameof(MySqlContainerCollection))]
    public sealed class UserRepositoryIntegrationTests
    {
        private readonly MySqlContainerFixture _fixture;

        public UserRepositoryIntegrationTests(MySqlContainerFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task GetByUsernameAsync_FindsCaseInsensitiveUsernameAndExcludesDeletedRows()
        {
            await _fixture.ResetDatabaseAsync();
            await _fixture.ServiceProvider.GetRequiredService<DatabaseInitializer>().InitializeAsync();

            var username = $"lookup-{Guid.NewGuid():N}";
            var deletedUsername = $"deleted-{Guid.NewGuid():N}";
            const string passwordHash = "test-password-hash";

            const string insertUserSql = @"
INSERT INTO Users (Username, PasswordHash, Email, FullName, IsActive, IsDeleted)
VALUES (@Username, @PasswordHash, @Email, @FullName, @IsActive, @IsDeleted)";

            await using (var connection = new MySqlConnection(_fixture.ConnectionString))
            {
                await connection.OpenAsync();
                await connection.ExecuteAsync(insertUserSql, new
                {
                    Username = username,
                    PasswordHash = passwordHash,
                    Email = $"{username}@test.local",
                    FullName = "Lookup Test User",
                    IsActive = true,
                    IsDeleted = false
                });
                await connection.ExecuteAsync(insertUserSql, new
                {
                    Username = deletedUsername,
                    PasswordHash = passwordHash,
                    Email = $"{deletedUsername}@test.local",
                    FullName = "Deleted Lookup Test User",
                    IsActive = true,
                    IsDeleted = true
                });
            }

            using var scope = _fixture.ServiceProvider.CreateScope();
            var repository = scope.ServiceProvider.GetRequiredService<IUserRepository>();

            var found = await repository.GetByUsernameAsync(username.ToUpperInvariant());
            var deleted = await repository.GetByUsernameAsync(deletedUsername);

            Assert.NotNull(found);
            Assert.Equal(username, found.Username);
            Assert.Equal(passwordHash, found.PasswordHash);
            Assert.Null(deleted);
        }
    }
}
