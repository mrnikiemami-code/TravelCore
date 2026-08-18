using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NodaTime;
using TravelCore.Modules.Payment.Contracts;
using TravelCore.Modules.Payment.Domain;
using TravelCore.Modules.Payment.Infrastructure;
using TravelCore.Modules.Payment.Infrastructure.Options;
using TravelCore.Modules.Payment.Infrastructure.Providers;
using TravelCore.Modules.Payment.Infrastructure.Services;
using TravelCore.Modules.Payment.UnitTests.Fakes;
using Xunit;
using PaymentAggregate = TravelCore.Modules.Payment.Domain.Payment;

namespace TravelCore.Modules.Payment.UnitTests;

public sealed class PaymentIdempotencyTests
{
    private static readonly Instant Now = Instant.FromUtc(2026, 8, 18, 12, 0);
    private static readonly BookingReference Booking =
        new(Guid.Parse("0198b3e0-0000-7000-8000-000000000440"));
    private static readonly ProviderKey TestKey = new("test");

    [Fact]
    public async Task Repeated_GetOrCreate_Returns_Same_PaymentId()
    {
        await using var db = CreateDb();
        var clock = new FixedClock(Now);
        var service = new PaymentGetOrCreateService(db, clock);
        var first = await service.GetOrCreateAsync(Booking);
        var second = await service.GetOrCreateAsync(Booking);
        Assert.Equal(first.Id, second.Id);
        Assert.Equal(1, await db.Payments.CountAsync());
        Assert.Equal("Booking 1 -> 1 logical Payment", PaymentIdempotencyBoundary.OneBookingOneLogicalPayment);
        Assert.Equal("Retry = PaymentAttempt, not new Payment", PaymentIdempotencyBoundary.RetryIsAttemptNotPayment);
        Assert.Equal("NOT ASSUMED", PaymentIdempotencyBoundary.ExactlyOnceExternalPayment);
        Assert.False(PaymentIdempotencyBoundary.ProcessLocalIdempotencyAuthorityImplemented);
        Assert.False(PaymentIdempotencyBoundary.AutomaticRetryOnAmbiguityImplemented);
    }

    [Fact]
    public void Failed_Attempt_Allows_Explicit_New_Attempt_On_Same_Payment()
    {
        var payment = PaymentAggregate.Create(Booking, Now);
        var first = payment.CreateAttempt(Now);
        payment.RecordAttemptFailure(first.Id, Instant.FromUtc(2026, 8, 18, 12, 1));
        var second = payment.CreateAttempt(Instant.FromUtc(2026, 8, 18, 12, 2));
        Assert.Equal(payment.Id, payment.Id);
        Assert.Equal(2, payment.Attempts.Count);
        Assert.NotEqual(first.Id, second.Id);
        Assert.Equal(PaymentStatus.Pending, payment.Status);
    }

    [Fact]
    public async Task Same_Initiation_Idempotency_Key_Recovers_Same_Attempt()
    {
        await using var db = CreateDb();
        var fake = new FakePaymentProviderGateway(TestKey);
        var clock = new FixedClock(Now);
        var payment = PaymentAggregate.Create(Booking, Now);
        db.Payments.Add(payment);
        await db.SaveChangesAsync();
        var service = CreateInitiation(db, fake, clock);
        var first = await service.InitiateAsync(payment.Id, "init-1");
        var second = await service.InitiateAsync(payment.Id, "init-1");
        var reloaded = await db.Payments.Include(x => x.Attempts).SingleAsync();
        Assert.Equal(PaymentInitiationOutcome.Initiated, first.Outcome);
        Assert.Equal(first.RequestReference, second.RequestReference);
        Assert.Single(reloaded.Attempts);
    }

    [Fact]
    public async Task Different_Retry_Key_After_Failure_Creates_New_Attempt()
    {
        await using var db = CreateDb();
        var fake = new FakePaymentProviderGateway(TestKey)
        {
            NextInitiation = PaymentInitiationOutcome.DefinitiveFailure,
        };
        var clock = new FixedClock(Now);
        var payment = PaymentAggregate.Create(Booking, Now);
        db.Payments.Add(payment);
        await db.SaveChangesAsync();
        var service = CreateInitiation(db, fake, clock);
        await service.InitiateAsync(payment.Id, "fail-1");
        fake.NextInitiation = PaymentInitiationOutcome.Initiated;
        await service.InitiateAsync(payment.Id, "retry-2");
        var reloaded = await db.Payments.Include(x => x.Attempts).SingleAsync();
        Assert.Equal(2, reloaded.Attempts.Count);
        Assert.Equal(1, reloaded.Attempts.Count(a => a.Status == PaymentAttemptStatus.Failed));
        Assert.Equal(1, reloaded.Attempts.Count(a => a.IsActive));
    }

    [Fact]
    public async Task Unresolved_Attempt_Blocks_Different_Retry_Key()
    {
        await using var db = CreateDb();
        var fake = new FakePaymentProviderGateway(TestKey) { ThrowOnInitiate = true };
        var payment = PaymentAggregate.Create(Booking, Now);
        db.Payments.Add(payment);
        await db.SaveChangesAsync();
        var service = CreateInitiation(db, fake, new FixedClock(Now));
        await service.InitiateAsync(payment.Id, "amb-1");
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.InitiateAsync(payment.Id, "amb-2"));
        var reloaded = await db.Payments.Include(x => x.Attempts).SingleAsync();
        Assert.Single(reloaded.Attempts);
        Assert.Equal(PaymentAttemptStatus.Created, reloaded.Attempts.Single().Status);
        Assert.Equal(PaymentStatus.Pending, reloaded.Status);
    }

    [Fact]
    public async Task Succeeded_Payment_Blocks_Another_Attempt()
    {
        await using var db = CreateDb();
        var payment = PaymentAggregate.Create(Booking, Now);
        var attempt = payment.CreateAttempt(Now);
        payment.RecordAuthoritativeCollectionSuccess(attempt.Id, Instant.FromUtc(2026, 8, 18, 12, 5));
        db.Payments.Add(payment);
        await db.SaveChangesAsync();
        var service = CreateInitiation(db, new FakePaymentProviderGateway(TestKey), new FixedClock(Now));
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => service.InitiateAsync(payment.Id, "too-late"));
        Assert.Equal("Payment already succeeded.", ex.Message);
    }

    [Fact]
    public async Task Reconciliation_Succeeded_Converges_Idempotently()
    {
        await using var db = CreateDb();
        var fake = new FakePaymentProviderGateway(TestKey)
        {
            NextVerification = ProviderVerificationOutcome.Succeeded,
        };
        var payment = PaymentAggregate.Create(Booking, Now);
        var attempt = payment.CreateAttempt(Now);
        payment.RecordProviderInitiation(
            attempt.Id,
            Instant.FromUtc(2026, 8, 18, 12, 6),
            TestKey,
            fake.RequestReference,
            fake.TransactionReference);
        db.Payments.Add(payment);
        await db.SaveChangesAsync();
        var recheck = new PaymentAttemptRecheckService(db, new PaymentProviderResolver([fake]), new FixedClock(Now));
        await recheck.RecheckAsync(attempt.Id);
        await recheck.RecheckAsync(attempt.Id);
        var reloaded = await db.Payments.Include(x => x.Attempts).SingleAsync();
        Assert.Equal(PaymentStatus.Succeeded, reloaded.Status);
        Assert.Equal(1, reloaded.Attempts.Count(a => a.Status == PaymentAttemptStatus.Succeeded));
        Assert.Empty(db.ReconciliationIssues);
    }

    [Fact]
    public async Task Reconciliation_Failed_Leaves_Payment_Pending()
    {
        await using var db = CreateDb();
        var fake = new FakePaymentProviderGateway(TestKey)
        {
            NextVerification = ProviderVerificationOutcome.Failed,
        };
        var payment = PaymentAggregate.Create(Booking, Now);
        var attempt = payment.CreateAttempt(Now);
        payment.RecordProviderInitiation(
            attempt.Id,
            Instant.FromUtc(2026, 8, 18, 12, 7),
            TestKey,
            fake.RequestReference,
            fake.TransactionReference);
        db.Payments.Add(payment);
        await db.SaveChangesAsync();
        var recheck = new PaymentAttemptRecheckService(db, new PaymentProviderResolver([fake]), new FixedClock(Now));
        await recheck.RecheckAsync(attempt.Id);
        var reloaded = await db.Payments.Include(x => x.Attempts).SingleAsync();
        Assert.Equal(PaymentAttemptStatus.Failed, reloaded.Attempts.Single().Status);
        Assert.Equal(PaymentStatus.Pending, reloaded.Status);
    }

    [Fact]
    public async Task Reconciliation_Pending_Leaves_Unresolved()
    {
        await using var db = CreateDb();
        var fake = new FakePaymentProviderGateway(TestKey)
        {
            NextVerification = ProviderVerificationOutcome.PendingUnknown,
        };
        var payment = PaymentAggregate.Create(Booking, Now);
        var attempt = payment.CreateAttempt(Now);
        payment.RecordProviderInitiation(
            attempt.Id,
            Instant.FromUtc(2026, 8, 18, 12, 8),
            TestKey,
            fake.RequestReference,
            fake.TransactionReference);
        db.Payments.Add(payment);
        await db.SaveChangesAsync();
        var recheck = new PaymentAttemptRecheckService(db, new PaymentProviderResolver([fake]), new FixedClock(Now));
        await recheck.RecheckAsync(attempt.Id);
        var reloaded = await db.Payments.Include(x => x.Attempts).SingleAsync();
        Assert.Equal(PaymentAttemptStatus.Initiated, reloaded.Attempts.Single().Status);
        Assert.Equal(PaymentStatus.Pending, reloaded.Status);
        Assert.Empty(db.ReconciliationIssues);
    }

    [Fact]
    public async Task Contradictory_Terminal_Evidence_Does_Not_Flip_And_Records_Issue()
    {
        await using var db = CreateDb();
        var fake = new FakePaymentProviderGateway(TestKey)
        {
            NextVerification = ProviderVerificationOutcome.Succeeded,
        };
        var payment = PaymentAggregate.Create(Booking, Now);
        var attempt = payment.CreateAttempt(Now);
        payment.RecordProviderInitiation(
            attempt.Id,
            Instant.FromUtc(2026, 8, 18, 12, 9),
            TestKey,
            fake.RequestReference,
            fake.TransactionReference);
        payment.RecordAttemptFailure(attempt.Id, Instant.FromUtc(2026, 8, 18, 12, 10));
        db.Payments.Add(payment);
        await db.SaveChangesAsync();
        var recheck = new PaymentAttemptRecheckService(db, new PaymentProviderResolver([fake]), new FixedClock(Now));
        await recheck.RecheckAsync(attempt.Id);
        var reloaded = await db.Payments.Include(x => x.Attempts).SingleAsync();
        Assert.Equal(PaymentAttemptStatus.Failed, reloaded.Attempts.Single().Status);
        Assert.Equal(PaymentStatus.Pending, reloaded.Status);
        Assert.Equal(PaymentReconciliationIssueKind.ContradictoryProviderState, db.ReconciliationIssues.Single().Kind);
    }

    private static PaymentInitiationService CreateInitiation(
        PaymentDbContext db,
        FakePaymentProviderGateway fake,
        FixedClock clock) =>
        new(
            db,
            new PaymentProviderResolver([fake]),
            Options.Create(new PaymentProviderOptions { DefaultProviderKey = "test" }),
            clock,
            new PaymentGetOrCreateService(db, clock));

    private static PaymentDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<PaymentDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;
        return new PaymentDbContext(options);
    }

    private sealed class FixedClock : IClock
    {
        private readonly Instant _instant;
        public FixedClock(Instant instant) => _instant = instant;
        public Instant GetCurrentInstant() => _instant;
    }
}
