using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.DependencyInjection;
using MySqlConnector;
using MMNextPOS.Application.Services;
using MMNextPOS.Domain.Models;
using MMNextPOS.Infrastructure;
using MMNextPOS.Infrastructure.Repositories;
using Xunit;

namespace MMNextPOS.Infrastructure.Tests
{
    /// <summary>
    /// Integration tests for Journal Voucher repositories and service against a
    /// real MySQL testcontainer. Validates the 017 migration, transaction
    /// atomicity, and detail lifecycle.
    /// </summary>
    [Collection(nameof(MySqlContainerCollection))]
    public class JournalVoucherIntegrationTests
    {
        private readonly MySqlContainerFixture _fixture;

        public JournalVoucherIntegrationTests(MySqlContainerFixture fixture)
        {
            _fixture = fixture;
        }

        private static JournalVoucher NewVoucher(string voucherNo = "JV-IT-001") =>
            new()
            {
                VoucherNo = voucherNo,
                VoucherDate = new DateTime(2026, 10, 4),
                Description = "Integration test voucher",
                Status = "Draft",
                Details = new List<JournalVoucherDetail>
                {
                    new()
                    {
                        AccountCode = "1000", AccountName = "Cash",
                        DebitAmount = 250m, CreditAmount = 0m, LineNo = 0
                    },
                    new()
                    {
                        AccountCode = "4000", AccountName = "Sales",
                        DebitAmount = 0m, CreditAmount = 250m, LineNo = 1
                    }
                }
            };

        private async Task<int> SeedVoucherAsync(JournalVoucher voucher)
        {
            using var scope = _fixture.ServiceProvider.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<IJournalVoucherService>();
            var saved = await service.AddAsync(voucher);
            return saved.Id;
        }

        [Fact]
        public async Task Tables_Exist_AfterMigrationRun()
        {
            await using var conn = new MySqlConnection(_fixture.ConnectionString);
            await conn.OpenAsync();

            const string sql = @"
SELECT TABLE_NAME
FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_SCHEMA = DATABASE()
AND TABLE_NAME IN ('JournalVouchers', 'JournalVoucherDetails')";

            var tables = (await conn.QueryAsync<string>(sql)).ToHashSet();

            Assert.Contains("JournalVouchers", tables);
            Assert.Contains("JournalVoucherDetails", tables);
        }

        [Fact]
        public async Task Add_PersistsHeaderAndDetails_CanReadBack()
        {
            var id = await SeedVoucherAsync(NewVoucher("JV-IT-ADD-001"));

            using var scope = _fixture.ServiceProvider.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<IJournalVoucherService>();

            var voucher = await service.GetByIdWithDetailsAsync(id);
            Assert.NotNull(voucher);
            Assert.Equal("JV-IT-ADD-001", voucher.VoucherNo);
            Assert.Equal("Draft", voucher.Status);
            Assert.Equal(250m, voucher.TotalDebit);
            Assert.Equal(250m, voucher.TotalCredit);

            var details = voucher.Details?.OrderBy(d => d.LineNo).ToList();
            Assert.NotNull(details);
            Assert.Equal(2, details.Count);
            Assert.Equal("1000", details[0].AccountCode);
            Assert.Equal(250m, details[0].DebitAmount);
            Assert.Equal("4000", details[1].AccountCode);
            Assert.Equal(250m, details[1].CreditAmount);
        }

        [Fact]
        public async Task Update_ReplacesDetails_AndTotalsRecalculated()
        {
            var id = await SeedVoucherAsync(NewVoucher("JV-IT-UPD-001"));

            using var scope = _fixture.ServiceProvider.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<IJournalVoucherService>();

            var existing = await service.GetByIdWithDetailsAsync(id);
            Assert.NotNull(existing);

            existing.VoucherNo = "JV-IT-UPD-001-REV";
            existing.TotalDebit = 500m;
            existing.TotalCredit = 500m;
            existing.Details = new List<JournalVoucherDetail>
            {
                new()
                {
                    AccountCode = "1100", AccountName = "Bank",
                    DebitAmount = 500m, CreditAmount = 0m, LineNo = 0
                },
                new()
                {
                    AccountCode = "4100", AccountName = "Sales Rev",
                    DebitAmount = 0m, CreditAmount = 500m, LineNo = 1
                }
            };

            await service.UpdateAsync(existing);

            var reloaded = await service.GetByIdWithDetailsAsync(id);
            Assert.NotNull(reloaded);
            Assert.Equal("JV-IT-UPD-001-REV", reloaded.VoucherNo);
            Assert.Equal(500m, reloaded.TotalDebit);
            Assert.Equal(500m, reloaded.TotalCredit);

            var details = reloaded.Details?.OrderBy(d => d.LineNo).ToList();
            Assert.NotNull(details);
            Assert.Equal(2, details.Count);
            Assert.DoesNotContain(details, d => d.AccountCode == "1000");
            Assert.DoesNotContain(details, d => d.AccountCode == "4000");
            Assert.Contains(details, d => d.AccountCode == "1100");
            Assert.Contains(details, d => d.AccountCode == "4100");
        }

        [Fact]
        public async Task Delete_SoftDeletesHeaderAndDetails()
        {
            var id = await SeedVoucherAsync(NewVoucher("JV-IT-DEL-001"));

            using var scope = _fixture.ServiceProvider.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<IJournalVoucherService>();
            await service.DeleteAsync(id);

            // GetByIdAsync filters IsDeleted = 0 â†’ should not find it
            var voucher = await service.GetByIdAsync(id);
            Assert.Null(voucher);

            // Direct DB check: both header and details have IsDeleted = 1
            await using var conn = new MySqlConnection(_fixture.ConnectionString);
            await conn.OpenAsync();

            var headerDeleted = await conn.ExecuteScalarAsync<int?>(
                "SELECT IsDeleted FROM JournalVouchers WHERE Id = @Id", new { Id = id });
            Assert.Equal(1, headerDeleted);

            var detailDeletedCount = await conn.ExecuteScalarAsync<int>(
                "SELECT COUNT(*) FROM JournalVoucherDetails WHERE JournalVoucherId = @VoucherId AND IsDeleted = 1",
                new { VoucherId = id });
            Assert.Equal(2, detailDeletedCount);
        }

        [Fact]
        public async Task DuplicateVoucherNo_ThrowsUniqueKeyException()
        {
            await SeedVoucherAsync(NewVoucher("JV-IT-DUP-001"));

            await Assert.ThrowsAnyAsync<Exception>(async () =>
            {
                using var scope = _fixture.ServiceProvider.CreateScope();
                var service = scope.ServiceProvider.GetRequiredService<IJournalVoucherService>();
                await service.AddAsync(NewVoucher("JV-IT-DUP-001"));
            });
        }

        [Fact]
        public async Task DetailWriteFails_RollsBackHeader()
        {
            // Use an account code longer than VARCHAR(50) to trigger a data truncation error
            var badVoucher = NewVoucher("JV-IT-ROLL-001");
            badVoucher.Details = new List<JournalVoucherDetail>
            {
                new()
                {
                    AccountCode = new string('A', 200), // exceeds VARCHAR(50)
                    AccountName = "Cash",
                    DebitAmount = 100m, CreditAmount = 0m, LineNo = 0
                },
                new()
                {
                    AccountCode = "4000", AccountName = "Sales",
                    DebitAmount = 0m, CreditAmount = 100m, LineNo = 1
                }
            };

            await Assert.ThrowsAnyAsync<Exception>(async () =>
            {
                using var scope = _fixture.ServiceProvider.CreateScope();
                var service = scope.ServiceProvider.GetRequiredService<IJournalVoucherService>();
                await service.AddAsync(badVoucher);
            });

            // Header should NOT exist (rolled back): no voucher with this number
            using var scope = _fixture.ServiceProvider.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<IJournalVoucherService>();
            var all = await service.GetAllAsync();
            Assert.DoesNotContain(all, v => v.VoucherNo == "JV-IT-ROLL-001");
        }

        [Fact]
        public async Task MigrationReRun_IsIdempotent()
        {
            using var scope = _fixture.ServiceProvider.CreateScope();
            var migrationRunner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();

            var result = await migrationRunner.RunMigrationsAsync();

            Assert.True(result.Success);
            Assert.Equal(0, result.MigrationsApplied); // all already applied
            Assert.Equal(0, result.MigrationsFailed);
        }
    }
}

