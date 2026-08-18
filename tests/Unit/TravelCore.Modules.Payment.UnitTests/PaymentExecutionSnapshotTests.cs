using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NodaTime;
using TravelCore.Modules.Booking.Contracts;
using TravelCore.Modules.Payment.Contracts;
using TravelCore.Modules.Payment.Domain;
using TravelCore.Modules.Payment.Infrastructure;
using TravelCore.Modules.Payment.Infrastructure.Options;
using TravelCore.Modules.Payment.Infrastructure.Providers;
using TravelCore.Modules.Payment.Infrastructure.Services;
using TravelCore.Modules.Payment.UnitTests.Fakes;
using Xunit;
using PaymentAggregate = TravelCore.Modules.Payment.Domain.Payment;
using MoneyValue = TravelCore.Money.Money;

namespace TravelCore.Modules.Payment.UnitTests;

public sealed class PaymentExecutionSnapshotTests
{
    private static readonly Instant Now = Instant.FromUtc(2026, 8, 18, 15, 0);
    private static readonly BookingReference Booking =
        new(Guid.Parse("0198b3e0-0000-7000-8000-000000000550"));
    private static readonly Guid SnapshotId = Guid.Parse("0198b3e0-0000-7000-8000-000000000551");
    private static readonly ProviderKey TestKey = new("test");

    [Fact]
    public void Bind_Creates_Immutable_Snapshot_And_Leaves_Payment_Pending()
    {
        var payment = PaymentAggregate.Create(Booking, Now);
        payment.BindExecutionSnapshot(SnapshotId, new MoneyValue(110m, "IRR"), Now);

        Assert.NotNull(payment.ExecutionSnapshot);
        Assert.Equal(SnapshotId, payment.ExecutionSnapshot.BookingSnapshotId);
        Assert.Equal(110m, payment.ExecutionSnapshot.Amount.Amount);
        Assert.Equal("IRR", payment.ExecutionSnapshot.Amount.Currency.Value);
        Assert.Equal(Now, payment.ExecutionSnapshot.CapturedAt);
        Assert.Equal(PaymentStatus.Pending, payment.Status);
        Assert.Null(payment.SucceededAt);
    }

    [Fact]
    public void Repeated_Same_Obligation_Bind_Is_Idempotent()
    {
        var payment = PaymentAggregate.Create(Booking, Now);
        payment.BindExecutionSnapshot(SnapshotId, new MoneyValue(110m, "IRR"), Now);
        var first = payment.ExecutionSnapshot;
        payment.BindExecutionSnapshot(SnapshotId, new MoneyValue(110m, "IRR"), Now.Plus(Duration.FromMinutes(1)));

        Assert.Same(first, payment.ExecutionSnapshot);
        Assert.Equal(110m, payment.ExecutionSnapshot!.Amount.Amount);
        Assert.Equal(Now, payment.ExecutionSnapshot.CapturedAt);
    }

    [Fact]
    public void Different_Obligation_Cannot_Overwrite_Snapshot()
    {
        var payment = PaymentAggregate.Create(Booking, Now);
        payment.BindExecutionSnapshot(SnapshotId, new MoneyValue(110m, "IRR"), Now);

        Assert.Throws<InvalidOperationException>(() =>
            payment.BindExecutionSnapshot(Guid.CreateVersion7(), new MoneyValue(110m, "IRR"), Now));
        Assert.Throws<InvalidOperationException>(() =>
            payment.BindExecutionSnapshot(SnapshotId, new MoneyValue(999m, "IRR"), Now));
        Assert.Throws<InvalidOperationException>(() =>
            payment.BindExecutionSnapshot(SnapshotId, new MoneyValue(110m, "USD"), Now));
        Assert.Equal(110m, payment.ExecutionSnapshot!.Amount.Amount);
        Assert.Equal("IRR", payment.ExecutionSnapshot.Amount.Currency.Value);
    }

    [Fact]
    public void Provider_Amount_Mismatch_Does_Not_Succeed_Payment()
    {
        var payment = PrepareInitiated();
        var status = VerifiedProviderOutcomeApplier.ApplyVerification(
            payment,
            payment.Attempts.Single(),
            new PaymentVerificationResult
            {
                Outcome = ProviderVerificationOutcome.Succeeded,
                ProviderKey = TestKey,
                ReportedAmount = 999m,
                ReportedCurrencyCode = "IRR",
            },
            Now.Plus(Duration.FromMinutes(2)));

        Assert.Equal(VerificationApplyStatus.AmountMismatch, status);
        Assert.Equal(PaymentStatus.Pending, payment.Status);
        Assert.Equal(PaymentAttemptStatus.Initiated, payment.Attempts.Single().Status);
    }

    [Fact]
    public void Provider_Currency_Mismatch_Does_Not_Succeed_Payment()
    {
        var payment = PrepareInitiated();
        var status = VerifiedProviderOutcomeApplier.ApplyVerification(
            payment,
            payment.Attempts.Single(),
            new PaymentVerificationResult
            {
                Outcome = ProviderVerificationOutcome.Succeeded,
                ProviderKey = TestKey,
                ReportedAmount = 110m,
                ReportedCurrencyCode = "USD",
            },
            Now.Plus(Duration.FromMinutes(2)));

        Assert.Equal(VerificationApplyStatus.CurrencyMismatch, status);
        Assert.Equal(PaymentStatus.Pending, payment.Status);
        Assert.NotEqual(PaymentAttemptStatus.Succeeded, payment.Attempts.Single().Status);
    }

    [Fact]
    public void Omitted_Provider_Amount_Or_Currency_Does_Not_Succeed_When_Snapshot_Exists()
    {
        var payment = PrepareInitiated();
        var missingAmount = VerifiedProviderOutcomeApplier.ApplyVerification(
            payment,
            payment.Attempts.Single(),
            new PaymentVerificationResult
            {
                Outcome = ProviderVerificationOutcome.Succeeded,
                ProviderKey = TestKey,
                ReportedCurrencyCode = "IRR",
            },
            Now.Plus(Duration.FromMinutes(2)));
        Assert.Equal(VerificationApplyStatus.AmountMismatch, missingAmount);
        Assert.Equal(PaymentStatus.Pending, payment.Status);

        var missingCurrency = VerifiedProviderOutcomeApplier.ApplyVerification(
            payment,
            payment.Attempts.Single(),
            new PaymentVerificationResult
            {
                Outcome = ProviderVerificationOutcome.Succeeded,
                ProviderKey = TestKey,
                ReportedAmount = 110m,
            },
            Now.Plus(Duration.FromMinutes(3)));
        Assert.Equal(VerificationApplyStatus.CurrencyMismatch, missingCurrency);
        Assert.Equal(PaymentStatus.Pending, payment.Status);
    }

    [Fact]
    public void Matching_Provider_Success_Marks_Payment_And_Attempt_Succeeded()
    {
        var payment = PrepareInitiated();
        var status = VerifiedProviderOutcomeApplier.ApplyVerification(
            payment,
            payment.Attempts.Single(),
            new PaymentVerificationResult
            {
                Outcome = ProviderVerificationOutcome.Succeeded,
                ProviderKey = TestKey,
                ReportedAmount = 110m,
                ReportedCurrencyCode = "IRR",
            },
            Now.Plus(Duration.FromMinutes(2)));

        Assert.Equal(VerificationApplyStatus.Applied, status);
        Assert.Equal(PaymentStatus.Succeeded, payment.Status);
        Assert.Equal(PaymentAttemptStatus.Succeeded, payment.Attempts.Single().Status);
    }

    [Fact]
    public async Task Initiation_Without_Snapshot_Is_Rejected()
    {
        await using var db = CreateDb();
        var payment = PaymentAggregate.Create(Booking, Now);
        db.Payments.Add(payment);
        await db.SaveChangesAsync();
        var service = new PaymentInitiationService(
            db,
            new PaymentProviderResolver([new FakePaymentProviderGateway(TestKey)]),
            Options.Create(new PaymentProviderOptions { DefaultProviderKey = "test" }),
            new FixedClock(Now),
            new PaymentGetOrCreateService(db, new FixedClock(Now)));

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => service.InitiateAsync(payment.Id));
        Assert.Equal("PaymentExecutionSnapshot must be prepared before initiation.", ex.Message);
        var reloaded = await db.Payments.Include(x => x.Attempts).SingleAsync();
        Assert.Equal(PaymentStatus.Pending, reloaded.Status);
        Assert.Empty(reloaded.Attempts);
    }

    [Fact]
    public async Task Preparation_Is_Idempotent_And_Rejects_Different_Obligation()
    {
        await using var db = CreateDb();
        var payment = PaymentAggregate.Create(Booking, Now);
        db.Payments.Add(payment);
        await db.SaveChangesAsync();
        var obligations = new FakeObligationQuery
        {
            Next = new BookingPaymentObligationRead(
                Booking.BookingId,
                "Pending",
                110m,
                "IRR",
                SnapshotId,
                PaymentEligible: true),
        };
        var service = new PaymentPreparationService(db, obligations, new FixedClock(Now));
        await service.PrepareAsync(payment.Id);
        await service.PrepareAsync(payment.Id);
        var first = await db.Payments.Include(x => x.ExecutionSnapshot).SingleAsync();
        Assert.Equal(110m, first.ExecutionSnapshot!.Amount.Amount);
        Assert.Equal(PaymentStatus.Pending, first.Status);

        obligations.Next = obligations.Next with { Amount = 999m };
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.PrepareAsync(payment.Id));
        var reloaded = await db.Payments.Include(x => x.ExecutionSnapshot).SingleAsync();
        Assert.Equal(110m, reloaded.ExecutionSnapshot!.Amount.Amount);
    }

    private static PaymentAggregate PrepareInitiated()
    {
        var payment = PaymentAggregate.Create(Booking, Now);
        payment.BindExecutionSnapshot(SnapshotId, new MoneyValue(110m, "IRR"), Now);
        var attempt = payment.CreateAttempt(Now);
        payment.RecordProviderInitiation(
            attempt.Id,
            Now.Plus(Duration.FromMinutes(1)),
            TestKey,
            new ProviderRequestReference("req-snap"),
            new ProviderTransactionReference("txn-snap"));
        return payment;
    }

    private static PaymentDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<PaymentDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;
        return new PaymentDbContext(options);
    }

    private sealed class FakeObligationQuery : IBookingPaymentObligationQuery
    {
        public BookingPaymentObligationRead? Next { get; set; }

        public Task<BookingPaymentObligationRead?> GetByBookingIdAsync(
            Guid bookingId,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(Next);
    }

    private sealed class FixedClock : IClock
    {
        private readonly Instant _instant;

        public FixedClock(Instant instant) => _instant = instant;

        public Instant GetCurrentInstant() => _instant;
    }
}
