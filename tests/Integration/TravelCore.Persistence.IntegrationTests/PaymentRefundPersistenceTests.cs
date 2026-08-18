using Microsoft.EntityFrameworkCore;
using NodaTime;
using TravelCore.Modules.Payment.Contracts;
using TravelCore.Modules.Payment.Domain;
using TravelCore.Modules.Payment.Infrastructure;
using TravelCore.Modules.Payment.Infrastructure.Services;
using Xunit;
using PaymentAggregate = TravelCore.Modules.Payment.Domain.Payment;
using MoneyValue = TravelCore.Money.Money;

namespace TravelCore.Persistence.IntegrationTests;

[Collection(nameof(PaymentMigrationLifecycleCollection))]
public sealed class PaymentRefundPersistenceTests
{
    private static readonly Instant Now = Instant.FromUtc(2026, 8, 18, 22, 0);
    private readonly PaymentMigrationLifecycleContainerFixture _postgres;

    public PaymentRefundPersistenceTests(PaymentMigrationLifecycleContainerFixture postgres)
    {
        _postgres = postgres;
    }

    [Fact]
    public async Task Concurrent_Compensation_Converges_To_One_Refund()
    {
        var ct = TestContext.Current.CancellationToken;
        await using (var migrate = _postgres.CreateDbContext())
        {
            await PaymentMigrator.MigrateAsync(migrate, ct);
        }

        var payment = await SeedSucceededAsync(ct);
        async Task<RefundId> CreateAsync()
        {
            await using var db = _postgres.CreateDbContext();
            var service = new RefundGetOrCreateService(db, new FixedClock(Now.Plus(Duration.FromMinutes(3))));
            return (await service.GetOrCreateAsync(payment.Id, ct)).Id;
        }

        var results = await Task.WhenAll(CreateAsync(), CreateAsync());
        Assert.Equal(results[0], results[1]);
        await using var verify = _postgres.CreateDbContext();
        Assert.Equal(1, await verify.Refunds.CountAsync(x => x.PaymentId == payment.Id, ct));
    }

    [Fact]
    public async Task Concurrent_Retry_Creates_At_Most_One_Active_RefundAttempt()
    {
        var ct = TestContext.Current.CancellationToken;
        await using (var migrate = _postgres.CreateDbContext())
        {
            await PaymentMigrator.MigrateAsync(migrate, ct);
        }

        var payment = await SeedSucceededAsync(ct);
        RefundId refundId;
        await using (var db = _postgres.CreateDbContext())
        {
            var refund = Refund.CreateForSucceededPayment(
                await db.Payments.Include(x => x.ExecutionSnapshot).SingleAsync(x => x.Id == payment.Id, ct),
                Now.Plus(Duration.FromMinutes(3)));
            var first = refund.CreateAttempt(Now.Plus(Duration.FromMinutes(3)));
            refund.RecordAttemptFailure(first.Id, Now.Plus(Duration.FromMinutes(4)));
            db.Refunds.Add(refund);
            await db.SaveChangesAsync(ct);
            refundId = refund.Id;
        }

        async Task TryCreateAttemptAsync()
        {
            await using var db = _postgres.CreateDbContext();
            var refund = await db.Refunds.Include(x => x.Attempts).SingleAsync(x => x.Id == refundId, ct);
            try
            {
                refund.CreateAttempt(Now.Plus(Duration.FromMinutes(5)));
                await db.SaveChangesAsync(ct);
            }
            catch (InvalidOperationException)
            {
            }
            catch (DbUpdateException)
            {
            }
        }

        await Task.WhenAll(TryCreateAttemptAsync(), TryCreateAttemptAsync());
        await using var verify = _postgres.CreateDbContext();
        var loaded = await verify.Refunds.Include(x => x.Attempts).SingleAsync(x => x.Id == refundId, ct);
        Assert.Equal(1, loaded.Attempts.Count(a => a.IsActive));
        Assert.Equal(RefundStatus.Pending, loaded.Status);
    }

    [Fact]
    public async Task Refund_Success_And_Outbox_Commit_Atomically()
    {
        var ct = TestContext.Current.CancellationToken;
        await using (var migrate = _postgres.CreateDbContext())
        {
            await PaymentMigrator.MigrateAsync(migrate, ct);
        }

        var payment = await SeedSucceededAsync(ct);
        RefundId refundId;
        await using (var db = _postgres.CreateDbContext())
        {
            var refund = CreatePendingRefund(
                await db.Payments.Include(x => x.ExecutionSnapshot).SingleAsync(x => x.Id == payment.Id, ct));
            refundId = refund.Id;
            db.Refunds.Add(refund);
            await db.SaveChangesAsync(ct);
            ApplyRefundSuccess(db, refund, Now.Plus(Duration.FromMinutes(5)));
            await db.SaveChangesAsync(ct);
        }

        await using (var db = _postgres.CreateDbContext())
        {
            Assert.Equal(RefundStatus.Succeeded, (await db.Refunds.SingleAsync(x => x.Id == refundId, ct)).Status);
            var outbox = await db.OutboxMessages.SingleAsync(x => x.Id == refundId.Value, ct);
            Assert.Equal(RefundSuccessOutboxBoundary.MessageType, outbox.MessageType);
            Assert.Null(outbox.ProcessedAt);
            Assert.Equal(PaymentStatus.Succeeded, (await db.Payments.SingleAsync(x => x.Id == payment.Id, ct)).Status);
        }
    }

    [Fact]
    public async Task Rolled_Back_Refund_Success_Commits_Neither_Refund_Nor_Outbox()
    {
        var ct = TestContext.Current.CancellationToken;
        await using (var migrate = _postgres.CreateDbContext())
        {
            await PaymentMigrator.MigrateAsync(migrate, ct);
        }

        var payment = await SeedSucceededAsync(ct);
        RefundId refundId;
        await using (var db = _postgres.CreateDbContext())
        {
            var refund = CreatePendingRefund(
                await db.Payments.Include(x => x.ExecutionSnapshot).SingleAsync(x => x.Id == payment.Id, ct));
            refundId = refund.Id;
            db.Refunds.Add(refund);
            await db.SaveChangesAsync(ct);
        }

        await using (var db = _postgres.CreateDbContext())
        {
            var refund = await db.Refunds
                .Include(x => x.Attempts)
                .Include(x => x.Amount)
                .SingleAsync(x => x.Id == refundId, ct);
            await using var tx = await db.Database.BeginTransactionAsync(ct);
            ApplyRefundSuccess(db, refund, Now.Plus(Duration.FromMinutes(5)));
            await db.SaveChangesAsync(ct);
            await tx.RollbackAsync(ct);
        }

        await using (var db = _postgres.CreateDbContext())
        {
            Assert.Equal(RefundStatus.Pending, (await db.Refunds.SingleAsync(x => x.Id == refundId, ct)).Status);
            Assert.Equal(0, await db.OutboxMessages.CountAsync(x => x.Id == refundId.Value, ct));
        }
    }

    private async Task<PaymentAggregate> SeedSucceededAsync(CancellationToken ct)
    {
        await using var db = _postgres.CreateDbContext();
        var payment = PaymentAggregate.Create(new BookingReference(Guid.CreateVersion7()), Now);
        payment.BindExecutionSnapshot(Guid.CreateVersion7(), new MoneyValue(110m, "IRR"), Now);
        var attempt = payment.CreateAttempt(Now);
        payment.RecordProviderInitiation(
            attempt.Id,
            Now.Plus(Duration.FromMinutes(1)),
            new ProviderKey("test"),
            new ProviderRequestReference("req-" + payment.Id.Value.ToString("N")[..12]),
            new ProviderTransactionReference("txn-" + payment.Id.Value.ToString("N")[..12]));
        payment.RecordAuthoritativeCollectionSuccess(attempt.Id, Now.Plus(Duration.FromMinutes(2)));
        db.Payments.Add(payment);
        await db.SaveChangesAsync(ct);
        return payment;
    }

    private static Refund CreatePendingRefund(PaymentAggregate payment)
    {
        var refund = Refund.CreateForSucceededPayment(payment, Now.Plus(Duration.FromMinutes(3)));
        var attempt = refund.CreateAttempt(Now.Plus(Duration.FromMinutes(3)));
        refund.RecordProviderInitiation(
            attempt.Id,
            Now.Plus(Duration.FromMinutes(4)),
            new ProviderKey("test"),
            new ProviderRequestReference("rref-" + refund.Id.Value.ToString("N")[..12]),
            new ProviderTransactionReference("rtxn-" + refund.Id.Value.ToString("N")[..12]));
        return refund;
    }

    private static void ApplyRefundSuccess(PaymentDbContext db, Refund refund, Instant now)
    {
        var status = VerifiedRefundOutcomeApplier.ApplyVerification(
            refund,
            refund.Attempts.Single(),
            new PaymentVerificationResult
            {
                Outcome = ProviderVerificationOutcome.Succeeded,
                ProviderKey = new ProviderKey("test"),
                ReportedAmount = 110m,
                ReportedCurrencyCode = "IRR",
            },
            now);
        RefundSucceededOutboxWriter.EnqueueIfSucceeded(db, refund, now, status);
    }

    private sealed class FixedClock : IClock
    {
        private readonly Instant _instant;
        public FixedClock(Instant instant) => _instant = instant;
        public Instant GetCurrentInstant() => _instant;
    }
}
