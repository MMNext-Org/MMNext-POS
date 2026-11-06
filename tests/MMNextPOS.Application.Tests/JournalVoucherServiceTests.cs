using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MMNextPOS.Application.Services;
using MMNextPOS.Domain.Models;
using MMNextPOS.Infrastructure;
using MMNextPOS.Infrastructure.Repositories;
using Moq;
using Xunit;

namespace MMNextPOS.Application.Tests
{
    public class JournalVoucherServiceTests
    {
        private static readonly CancellationToken Ct = CancellationToken.None;

        private static JournalVoucher NewVoucher(string voucherNo = "JV-2026-000001", bool posted = false)
        {
            return new JournalVoucher
            {
                VoucherNo = voucherNo,
                VoucherDate = new DateTime(2026, 10, 4),
                Description = "Test voucher",
                Status = posted ? "Posted" : "Draft",
                TotalDebit = 100m,
                TotalCredit = 100m,
                Details = new List<JournalVoucherDetail>
                {
                    new JournalVoucherDetail
                    {
                        AccountCode = "1000", AccountName = "Cash", DebitAmount = 100m, CreditAmount = 0m, LineNo = 0
                    },
                    new JournalVoucherDetail
                    {
                        AccountCode = "2000", AccountName = "Revenue", DebitAmount = 0m, CreditAmount = 100m, LineNo = 1
                    }
                }
            };
        }

        private static Mock<IJournalVoucherRepository> MockVoucherRepo(JournalVoucher? existing = null)
        {
            var repo = new Mock<IJournalVoucherRepository>();
            repo.Setup(r => r.GetByIdAsync(It.IsAny<int>(), Ct))
                .ReturnsAsync(existing);
            repo.Setup(r => r.AddAsync(It.IsAny<JournalVoucher>(), Ct))
                .Returns((JournalVoucher v, CancellationToken _) =>
                {
                    v.Id = 42;
                    return Task.FromResult(v);
                });
            repo.Setup(r => r.UpdateAsync(It.IsAny<JournalVoucher>(), Ct))
                .Returns(Task.CompletedTask);
            repo.Setup(r => r.DeleteAsync(It.IsAny<int>(), Ct))
                .Returns(Task.CompletedTask);
            return repo;
        }

        private static Mock<IJournalVoucherDetailRepository> MockDetailRepo()
        {
            var repo = new Mock<IJournalVoucherDetailRepository>();
            repo.Setup(r => r.AddAsync(It.IsAny<JournalVoucherDetail>(), Ct))
                .Returns((JournalVoucherDetail d, CancellationToken _) => Task.FromResult(d));
            repo.Setup(r => r.DeleteAsync(It.IsAny<int>(), Ct))
                .Returns(Task.CompletedTask);
            repo.Setup(r => r.GetByVoucherIdAsync(It.IsAny<int>(), Ct))
                .ReturnsAsync(new List<JournalVoucherDetail>());
            return repo;
        }

        private static Mock<IUnitOfWork> MockUnitOfWork()
        {
            var uow = new Mock<IUnitOfWork>();
            uow.Setup(u => u.BeginTransactionAsync(Ct)).Returns(Task.CompletedTask);
            uow.Setup(u => u.CommitAsync(Ct)).Returns(Task.CompletedTask);
            uow.Setup(u => u.RollbackAsync(Ct)).Returns(Task.CompletedTask);
            return uow;
        }

        private static Mock<IInvoiceNumberGenerator> MockNumberGenerator(string nextNo = "JV-2026-000001")
        {
            var gen = new Mock<IInvoiceNumberGenerator>();
            gen.Setup(g => g.NextAsync("JV", Ct)).ReturnsAsync(nextNo);
            return gen;
        }

        private static JournalVoucherService CreateService(
            JournalVoucher? existing = null,
            string? nextVoucherNo = null,
            Action<Mock<IJournalVoucherDetailRepository>>? detailSetup = null)
        {
            var voucherRepo = MockVoucherRepo(existing);
            var detailRepo = MockDetailRepo();
            detailSetup?.Invoke(detailRepo);
            var audit = new Mock<IAuditService>();
            var uow = MockUnitOfWork();
            var gen = MockNumberGenerator(nextVoucherNo ?? "JV-2026-000001");

            return new JournalVoucherService(
                voucherRepo.Object,
                detailRepo.Object,
                audit.Object,
                uow.Object,
                gen.Object);
        }

        [Fact]
        public async Task AddAsync_UnbalancedLines_Throws()
        {
            var v = NewVoucher();
            v.Details = new List<JournalVoucherDetail>
            {
                new() { AccountCode = "1000", AccountName = "Cash", DebitAmount = 100m, CreditAmount = 0m, LineNo = 0 },
                new() { AccountCode = "2000", AccountName = "Revenue", DebitAmount = 0m, CreditAmount = 50m, LineNo = 1 }
            };

            var service = CreateService();

            await Assert.ThrowsAsync<InvalidOperationException>(() => service.AddAsync(v, Ct));
        }

        [Fact]
        public async Task AddAsync_SingleLine_Throws()
        {
            var v = NewVoucher();
            v.Details = new List<JournalVoucherDetail>
            {
                new() { AccountCode = "1000", AccountName = "Cash", DebitAmount = 100m, CreditAmount = 0m, LineNo = 0 }
            };

            var service = CreateService();

            await Assert.ThrowsAsync<InvalidOperationException>(() => service.AddAsync(v, Ct));
        }

        [Fact]
        public async Task AddAsync_BothDebitAndCreditOnLine_Throws()
        {
            var v = NewVoucher();
            v.Details.ElementAt(0).DebitAmount = 50m;
            v.Details.ElementAt(0).CreditAmount = 50m;

            var service = CreateService();

            await Assert.ThrowsAsync<InvalidOperationException>(() => service.AddAsync(v, Ct));
        }

        [Fact]
        public async Task AddAsync_NeitherDebitNorCredit_Throws()
        {
            var v = NewVoucher();
            v.Details.ElementAt(0).DebitAmount = 0m;
            v.Details.ElementAt(0).CreditAmount = 0m;

            var service = CreateService();

            await Assert.ThrowsAsync<InvalidOperationException>(() => service.AddAsync(v, Ct));
        }

        [Fact]
        public async Task AddAsync_Valid_VoucherNoAutoGenerated_CommitCalled()
        {
            var voucherRepo = MockVoucherRepo();
            var detailRepo = MockDetailRepo();
            var audit = new Mock<IAuditService>();
            var uow = MockUnitOfWork();
            var gen = MockNumberGenerator("JV-2026-000007");

            var service = new JournalVoucherService(
                voucherRepo.Object, detailRepo.Object, audit.Object, uow.Object, gen.Object);

            var v = new JournalVoucher
            {
                VoucherNo = "", // blank -> auto-generate
                VoucherDate = new DateTime(2026, 10, 4),
                Details = new List<JournalVoucherDetail>
                {
                    new() { AccountCode = "1000", AccountName = "Cash", DebitAmount = 100m, CreditAmount = 0m, LineNo = 0 },
                    new() { AccountCode = "2000", AccountName = "Revenue", DebitAmount = 0m, CreditAmount = 100m, LineNo = 1 }
                }
            };

            var result = await service.AddAsync(v, Ct);

            Assert.Equal("JV-2026-000007", result.VoucherNo);
            Assert.Equal("Draft", result.Status);
            Assert.Equal(100m, result.TotalDebit);
            Assert.Equal(100m, result.TotalCredit);
            uow.Verify(u => u.BeginTransactionAsync(Ct), Times.Once);
            uow.Verify(u => u.CommitAsync(Ct), Times.Once);
            uow.Verify(u => u.RollbackAsync(Ct), Times.Never);
        }

        [Fact]
        public async Task AddAsync_DetailInsertFails_RollbackCalled()
        {
            var voucherRepo = MockVoucherRepo();
            var detailRepo = MockDetailRepo();
            detailRepo.Setup(r => r.AddAsync(It.IsAny<JournalVoucherDetail>(), Ct))
                .ThrowsAsync(new InvalidOperationException("DB write failed"));
            var audit = new Mock<IAuditService>();
            var uow = MockUnitOfWork();
            var gen = MockNumberGenerator();

            var service = new JournalVoucherService(
                voucherRepo.Object, detailRepo.Object, audit.Object, uow.Object, gen.Object);

            var v = new JournalVoucher
            {
                Details = new List<JournalVoucherDetail>
                {
                    new() { AccountCode = "1000", AccountName = "Cash", DebitAmount = 100m, CreditAmount = 0m, LineNo = 0 },
                    new() { AccountCode = "2000", AccountName = "Revenue", DebitAmount = 0m, CreditAmount = 100m, LineNo = 1 }
                }
            };

            await Assert.ThrowsAsync<InvalidOperationException>(() => service.AddAsync(v, Ct));

            uow.Verify(u => u.BeginTransactionAsync(Ct), Times.Once);
            uow.Verify(u => u.RollbackAsync(Ct), Times.Once);
            uow.Verify(u => u.CommitAsync(Ct), Times.Never);
        }

        [Fact]
        public async Task UpdateAsync_NonDraft_Throws()
        {
            var existing = NewVoucher(posted: true);
            existing.Id = 1;
            var service = CreateService(existing: existing);

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                service.UpdateAsync(NewVoucher(voucherNo: "JV-EDIT"), Ct));
        }

        [Fact]
        public async Task UpdateAsync_CapturesOldValuesBeforeMutation()
        {
            var existing = NewVoucher(voucherNo: "JV-OLD");
            existing.Id = 1;
            var audit = new Mock<IAuditService>();
            object? capturedOld = null;

            var voucherRepo = MockVoucherRepo(existing);
            var detailRepo = MockDetailRepo();
            var uow = MockUnitOfWork();
            var gen = MockNumberGenerator();

            var service = new JournalVoucherService(
                voucherRepo.Object, detailRepo.Object, audit.Object, uow.Object, gen.Object);

            audit.Setup(a => a.LogAsync(
                    It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(),
                    It.IsAny<object?>(), It.IsAny<object?>(), It.IsAny<int?>(), It.IsAny<string?>(),
                    It.IsAny<string?>(), Ct))
                .Callback<string, int, string, object?, object?, int?, string?, string?, CancellationToken>(
                    (_, _, _, oldValues, _, _, _, _, _) => capturedOld = oldValues);

            var update = NewVoucher(voucherNo: "JV-NEW");
            update.Id = 1;
            update.TotalDebit = 200m;
            update.TotalCredit = 200m;
            update.Details = new List<JournalVoucherDetail>
            {
                new() { AccountCode = "1000", AccountName = "Cash", DebitAmount = 200m, CreditAmount = 0m, LineNo = 0 },
                new() { AccountCode = "2000", AccountName = "Revenue", DebitAmount = 0m, CreditAmount = 200m, LineNo = 1 }
            };

            await service.UpdateAsync(update, Ct);

            Assert.NotNull(capturedOld);
            var oldProps = capturedOld!.GetType().GetProperties();
            var voucherNoProp = oldProps.FirstOrDefault(p => p.Name == "VoucherNo");
            Assert.NotNull(voucherNoProp);
            Assert.Equal("JV-OLD", voucherNoProp.GetValue(capturedOld, null)?.ToString());
        }

        [Fact]
        public async Task UpdateAsync_ReplacesDetails()
        {
            var existing = NewVoucher();
            existing.Id = 1;
            var detailRepo = MockDetailRepo();
            detailRepo.Setup(r => r.GetByVoucherIdAsync(1, Ct))
                .ReturnsAsync(new List<JournalVoucherDetail>
                {
                    new() { Id = 10, JournalVoucherId = 1, AccountCode = "OLD", AccountName = "Old", DebitAmount = 50m, CreditAmount = 0m, LineNo = 0 }
                });
            var audit = new Mock<IAuditService>();
            var uow = MockUnitOfWork();
            var gen = MockNumberGenerator();
            var voucherRepo = MockVoucherRepo(existing);

            var service = new JournalVoucherService(
                voucherRepo.Object, detailRepo.Object, audit.Object, uow.Object, gen.Object);

            var update = NewVoucher();
            update.Id = 1;
            update.Details = new List<JournalVoucherDetail>
            {
                new() { AccountCode = "NEW", AccountName = "New", DebitAmount = 100m, CreditAmount = 0m, LineNo = 0 },
                new() { AccountCode = "REV", AccountName = "Rev", DebitAmount = 0m, CreditAmount = 100m, LineNo = 1 }
            };

            await service.UpdateAsync(update, Ct);

            detailRepo.Verify(r => r.DeleteAsync(10, Ct), Times.Once); // old detail removed
            detailRepo.Verify(r => r.AddAsync(It.Is<JournalVoucherDetail>(d => d.AccountCode == "NEW"), Ct), Times.Once);
            detailRepo.Verify(r => r.AddAsync(It.Is<JournalVoucherDetail>(d => d.AccountCode == "REV"), Ct), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_NonDraft_Throws()
        {
            var existing = NewVoucher(posted: true);
            existing.Id = 1;
            var service = CreateService(existing: existing);

            await Assert.ThrowsAsync<InvalidOperationException>(() => service.DeleteAsync(1, Ct));
        }

        [Fact]
        public async Task DeleteAsync_SoftDeletesDetailsToo()
        {
            var existing = NewVoucher();
            existing.Id = 1;
            var detailRepo = MockDetailRepo();
            detailRepo.Setup(r => r.GetByVoucherIdAsync(1, Ct))
                .ReturnsAsync(new List<JournalVoucherDetail>
                {
                    new() { Id = 10, JournalVoucherId = 1 },
                    new() { Id = 11, JournalVoucherId = 1 }
                });
            var audit = new Mock<IAuditService>();
            var uow = MockUnitOfWork();
            var gen = MockNumberGenerator();
            var voucherRepo = MockVoucherRepo(existing);

            var service = new JournalVoucherService(
                voucherRepo.Object, detailRepo.Object, audit.Object, uow.Object, gen.Object);

            await service.DeleteAsync(1, Ct);

            detailRepo.Verify(r => r.DeleteAsync(10, Ct), Times.Once);
            detailRepo.Verify(r => r.DeleteAsync(11, Ct), Times.Once);
            voucherRepo.Verify(r => r.DeleteAsync(1, Ct), Times.Once);
        }

        [Fact]
        public async Task VoidAsync_CapturesPriorStatusNotNewStatus()
        {
            var existing = NewVoucher();
            existing.Id = 1;
            existing.Status = "Posted";
            var audit = new Mock<IAuditService>();
            object? capturedOld = null;
            object? capturedNew = null;

            var voucherRepo = MockVoucherRepo(existing);
            var uow = MockUnitOfWork();
            var gen = MockNumberGenerator();

            var service = new JournalVoucherService(
                voucherRepo.Object, MockDetailRepo().Object, audit.Object, uow.Object, gen.Object);

            audit.Setup(a => a.LogAsync(
                    It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(),
                    It.IsAny<object?>(), It.IsAny<object?>(), It.IsAny<int?>(), It.IsAny<string?>(),
                    It.IsAny<string?>(), Ct))
                .Callback<string, int, string, object?, object?, int?, string?, string?, CancellationToken>(
                    (_, _, _, oldValues, newValues, _, _, _, _) =>
                    {
                        capturedOld = oldValues;
                        capturedNew = newValues;
                    });

            await service.VoidAsync(1, Ct);

            Assert.NotNull(capturedOld);
            var oldStatus = capturedOld!.GetType().GetProperty("Status")?.GetValue(capturedOld, null)?.ToString();
            Assert.Equal("Posted", oldStatus);

            Assert.NotNull(capturedNew);
            var newStatus = capturedNew!.GetType().GetProperty("Status")?.GetValue(capturedNew, null)?.ToString();
            Assert.Equal("Voided", newStatus);
        }

        [Fact]
        public async Task GetByIdWithDetailsAsync_ReturnsDetailsOrderedByLineNo()
        {
            var voucher = NewVoucher();
            voucher.Id = 1;
            var detailRepo = MockDetailRepo();
            detailRepo.Setup(r => r.GetByVoucherIdAsync(1, Ct))
                .ReturnsAsync(new List<JournalVoucherDetail>
                {
                    new() { Id = 2, JournalVoucherId = 1, AccountCode = "B", AccountName = "B", LineNo = 1 },
                    new() { Id = 1, JournalVoucherId = 1, AccountCode = "A", AccountName = "A", LineNo = 0 }
                });
            var audit = new Mock<IAuditService>();
            var uow = MockUnitOfWork();
            var gen = MockNumberGenerator();
            var voucherRepo = MockVoucherRepo(voucher);

            var service = new JournalVoucherService(
                voucherRepo.Object, detailRepo.Object, audit.Object, uow.Object, gen.Object);

            var result = await service.GetByIdWithDetailsAsync(1, Ct);

            Assert.NotNull(result);
            Assert.Equal(2, result.Details.Count());
            Assert.Equal("A", result.Details.First().AccountCode);
            Assert.Equal("B", result.Details.Last().AccountCode);
        }
    }
}
