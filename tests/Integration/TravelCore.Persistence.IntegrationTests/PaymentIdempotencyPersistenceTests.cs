using Microsoft.EntityFrameworkCore;
using NodaTime;
using TravelCore.Modules.Payment.Contracts;
using TravelCore.Modules.Payment.Domain;
using TravelCore.Modules.Payment.Infrastructure;
using TravelCore.Modules.Payment.Infrastructure.Services;
using Xunit;
using PaymentAggregate = TravelCore.Modules.Payment.Domain.Payment;

namespace TravelCore.Persistence.IntegrationTests;

[Collection(nameof(PaymentMigrationLifecycleCollection))]
public sealed class PaymentIdempotencyPersistenceTests
{
    private readonly PaymentMigrationLifecycleContainerFixture _postgres;

    public PaymentIdempotencyPersistenceTests(PaymentMigrationLifecycleContainerFixture postgres)
    {
        _postgres = postgres;
    }

    [Fact]
    public async Task Concurrent_GetOrCreate_Converges_To_One_Payment()
    {
        var ct = TestContext.Current.CancellationToken;
        await using (var migrate = _postgres.CreateDbContext())
        {
            await PaymentMigrator.MigrateAsync(migrate, ct);
        }

        var booking = new BookingReference(Guid.Parse("0198b3e0-0000-7000-8000-000000000551"));
        var clock = new FixedClock(Instant.FromUtc(2026, 8, 18, 12, 30));

        async Task<PaymentId> CreateAsync()
        {
            await using var db = _postgres.CreateDbContext();
            var service = new PaymentGetOrCreateService(db, clock);
            var payment = await service.GetOrCreateAsync(booking, ct);
            return payment.Id;
        }

        var results = await Task.WhenAll(CreateAsync(), CreateAsync());
        Assert.Equal(results[0], results[1]);

        await using var verify = _postgres.CreateDbContext();
        Assert.Equal(1, await verify.Payments.CountAsync(x => x.Booking == booking, ct));
    }

    [Fact]
    public async Task Concurrent_Retry_Creates_At_Most_One_Active_Attempt()
    {
        var ct = TestContext.Current.CancellationToken;
        await using (var migrate = _postgres.CreateDbContext())
        {
            await PaymentMigrator.MigrateAsync(migrate, ct);
        }

        var booking = new BookingReference(Guid.Parse("0198b3e0-0000-7000-8000-000000000552"));
        PaymentId paymentId;
        await using (var db = _postgres.CreateDbContext())
        {
            var payment = PaymentAggregate.Create(booking, Instant.FromUtc(2026, 8, 18, 12, 31));
            var first = payment.CreateAttempt(Instant.FromUtc(2026, 8, 18, 12, 31));
            payment.RecordAttemptFailure(first.Id, Instant.FromUtc(2026, 8, 18, 12, 32));
            db.Payments.Add(payment);
            await db.SaveChangesAsync(ct);
            paymentId = payment.Id;
        }

        async Task TryCreateAttemptAsync()
        {
            await using var db = _postgres.CreateDbContext();
            var payment = await db.Payments.Include(x => x.Attempts).SingleAsync(x => x.Id == paymentId, ct);
            try
            {
                payment.CreateAttempt(Instant.FromUtc(2026, 8, 18, 12, 33));
                await db.SaveChangesAsync(ct);
            }
            catch (DbUpdateException)
            {
                // Unique active-attempt index is the authority.
            }
        }

        await Task.WhenAll(TryCreateAttemptAsync(), TryCreateAttemptAsync());
        await using var verify = _postgres.CreateDbContext();
        var loaded = await verify.Payments.Include(x => x.Attempts).SingleAsync(x => x.Id == paymentId, ct);
        Assert.Equal(1, loaded.Attempts.Count(a => a.IsActive));
        Assert.Equal(PaymentStatus.Pending, loaded.Status);
    }

    [Fact]
    public async Task Success_Vs_Retry_Leaves_No_Active_Attempt_After_Success()
    {
        var ct = TestContext.Current.CancellationToken;
        await using (var migrate = _postgres.CreateDbContext())
        {
            await PaymentMigrator.MigrateAsync(migrate, ct);
        }

        var booking = new BookingReference(Guid.Parse("0198b3e0-0000-7000-8000-000000000553"));
        PaymentId paymentId;
        PaymentAttemptId firstId;
        await using (var db = _postgres.CreateDbContext())
        {
            var payment = PaymentAggregate.Create(booking, Instant.FromUtc(2026, 8, 18, 12, 34));
            var first = payment.CreateAttempt(Instant.FromUtc(2026, 8, 18, 12, 34));
            db.Payments.Add(payment);
            await db.SaveChangesAsync(ct);
            paymentId = payment.Id;
            firstId = first.Id;
        }

        async Task SucceedAsync()
        {
            await using var db = _postgres.CreateDbContext();
            var payment = await db.Payments.Include(x => x.Attempts).SingleAsync(x => x.Id == paymentId, ct);
            try
            {
                payment.RecordAuthoritativeCollectionSuccess(firstId, Instant.FromUtc(2026, 8, 18, 12, 35));
                await db.SaveChangesAsync(ct);
            }
            catch (InvalidOperationException)
            {
            }
            catch (DbUpdateException)
            {
            }
        }

        async Task RetryAsync()
        {
            await using var db = _postgres.CreateDbContext();
            var payment = await db.Payments.Include(x => x.Attempts).SingleAsync(x => x.Id == paymentId, ct);
            try
            {
                payment.RecordAttemptFailure(firstId, Instant.FromUtc(2026, 8, 18, 12, 35));
                payment.CreateAttempt(Instant.FromUtc(2026, 8, 18, 12, 36));
                await db.SaveChangesAsync(ct);
            }
            catch (InvalidOperationException)
            {
            }
            catch (DbUpdateException)
            {
            }
        }

        await Task.WhenAll(SucceedAsync(), RetryAsync());
        await using var verify = _postgres.CreateDbContext();
        var loaded = await verify.Payments.Include(x => x.Attempts).SingleAsync(x => x.Id == paymentId, ct);
        if (loaded.Status == PaymentStatus.Succeeded)
        {
            Assert.DoesNotContain(loaded.Attempts, a => a.IsActive);
            Assert.Equal(1, loaded.Attempts.Count(a => a.Status == PaymentAttemptStatus.Succeeded));
        }
        else
        {
            Assert.Equal(PaymentStatus.Pending, loaded.Status);
            Assert.True(loaded.Attempts.Count(a => a.IsActive) <= 1);
        }
    }

    [Fact]
    public async Task Idempotency_And_Reconciliation_Issue_RoundTrip()
    {
        var ct = TestContext.Current.CancellationToken;
        await using (var migrate = _postgres.CreateDbContext())
        {
            await PaymentMigrator.MigrateAsync(migrate, ct);
        }

        var booking = new BookingReference(Guid.Parse("0198b3e0-0000-7000-8000-000000000554"));
        await using (var db = _postgres.CreateDbContext())
        {
            var payment = PaymentAggregate.Create(booking, Instant.FromUtc(2026, 8, 18, 12, 40));
            var attempt = payment.CreateAttempt(Instant.FromUtc(2026, 8, 18, 12, 40));
            db.Payments.Add(payment);
            db.InitiationIdempotency.Add(
                PaymentInitiationIdempotencyRecord.Create(
                    payment.Id,
                    "key-roundtrip",
                    attempt.Id,
                    Instant.FromUtc(2026, 8, 18, 12, 40)));
            db.ReconciliationIssues.Add(
                PaymentReconciliationIssue.Create(
                    payment.Id,
                    attempt.Id,
                    PaymentReconciliationIssueKind.ContradictoryProviderState,
                    Instant.FromUtc(2026, 8, 18, 12, 41)));
            await db.SaveChangesAsync(ct);
        }

        await using (var db = _postgres.CreateDbContext())
        {
            Assert.Equal("key-roundtrip", (await db.InitiationIdempotency.SingleAsync(ct)).IdempotencyKey);
            Assert.Equal(
                PaymentReconciliationIssueKind.ContradictoryProviderState,
                (await db.ReconciliationIssues.SingleAsync(ct)).Kind);
            Assert.Equal(1, await db.Payments.CountAsync(x => x.Booking == booking, ct));
        }
    }

    private sealed class FixedClock : IClock
    {
        private readonly Instant _instant;
        public FixedClock(Instant instant) => _instant = instant;
        public Instant GetCurrentInstant() => _instant;
    }
}
