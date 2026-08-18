using Microsoft.EntityFrameworkCore;
using NodaTime;
using TravelCore.Modules.Payment.Contracts;
using TravelCore.Modules.Payment.Domain;
using TravelCore.Modules.Payment.Infrastructure;
using TravelCore.Modules.Payment.Infrastructure.Providers;
using TravelCore.Modules.Payment.Infrastructure.Services;
using TravelCore.Modules.Payment.UnitTests.Fakes;
using Xunit;
using PaymentAggregate = TravelCore.Modules.Payment.Domain.Payment;
using MoneyValue = TravelCore.Money.Money;

namespace TravelCore.Modules.Payment.UnitTests;

public sealed class PaymentRefundTests
{
    private static readonly Instant Now = Instant.FromUtc(2026, 8, 18, 20, 0);
    private static readonly BookingReference Booking =
        new(Guid.Parse("0198b3e0-0000-7000-8000-000000000801"));
    private static readonly Guid SnapshotId = Guid.Parse("0198b3e0-0000-7000-8000-000000000802");
    private static readonly ProviderKey TestKey = new("test");

    [Fact]
    public void Succeeded_Payment_Creates_Full_Refund_From_Snapshot()
    {
        var payment = SucceededPayment();
        var refund = Refund.CreateForSucceededPayment(payment, Now.Plus(Duration.FromMinutes(1)));

        Assert.Equal(RefundStatus.Pending, refund.Status);
        Assert.Equal(payment.Id, refund.PaymentId);
        Assert.Equal(110m, refund.Amount.Amount);
        Assert.Equal("IRR", refund.Amount.Currency.Value);
        Assert.Equal(payment.Booking, refund.Booking);
        Assert.Empty(refund.Attempts);
        Assert.Equal(PaymentStatus.Succeeded, payment.Status);
        Assert.Equal(
            new[] { RefundStatus.Pending, RefundStatus.Succeeded },
            Enum.GetValues<RefundStatus>());
        Assert.Equal(
            new[]
            {
                RefundAttemptStatus.Created,
                RefundAttemptStatus.Initiated,
                RefundAttemptStatus.Succeeded,
                RefundAttemptStatus.Failed,
            },
            Enum.GetValues<RefundAttemptStatus>());
        Assert.DoesNotContain("Failed", Enum.GetNames<RefundStatus>());
        Assert.DoesNotContain("Refunded", Enum.GetNames<PaymentStatus>());
        Assert.Equal("Payment != Refund", PaymentRefundBoundary.PaymentIsNotRefund);
        Assert.Equal("PaymentSucceeded != RefundSucceeded", PaymentRefundBoundary.PaymentSucceededIsNotRefundSucceeded);
        Assert.False(PaymentRefundBoundary.PartialRefundImplemented);
        Assert.False(PaymentRefundBoundary.PublicRefundApiImplemented);
        Assert.False(PaymentRefundBoundary.PaymentRefundedStatusImplemented);
    }

    [Fact]
    public void Pending_Payment_Cannot_Create_Refund()
    {
        var payment = PaymentAggregate.Create(Booking, Now);
        payment.BindExecutionSnapshot(SnapshotId, new MoneyValue(110m, "IRR"), Now);
        Assert.Throws<InvalidOperationException>(() => Refund.CreateForSucceededPayment(payment, Now));
    }

    [Fact]
    public void Refund_Without_Snapshot_Is_Rejected()
    {
        var payment = PaymentAggregate.Create(Booking, Now);
        var attempt = payment.CreateAttempt(Now);
        payment.RecordAuthoritativeCollectionSuccess(attempt.Id, Now.Plus(Duration.FromMinutes(1)));
        Assert.Throws<InvalidOperationException>(() => Refund.CreateForSucceededPayment(payment, Now.Plus(Duration.FromMinutes(2))));
    }

    [Fact]
    public async Task GetOrCreate_Returns_Same_RefundId()
    {
        await using var db = CreateDb();
        var payment = await SeedSucceededAsync(db);
        var service = new RefundGetOrCreateService(db, new FixedClock(Now.Plus(Duration.FromMinutes(3))));
        var first = await service.GetOrCreateAsync(payment.Id);
        var second = await service.GetOrCreateAsync(payment.Id);
        Assert.Equal(first.Id, second.Id);
        Assert.Equal(RefundStatus.Pending, first.Status);
        Assert.Equal(1, await db.Refunds.CountAsync());
    }

    [Fact]
    public void First_Attempt_Starts_Created()
    {
        var refund = Refund.CreateForSucceededPayment(SucceededPayment(), Now);
        var attempt = refund.CreateAttempt(Now.Plus(Duration.FromSeconds(1)));
        Assert.Equal(RefundAttemptStatus.Created, attempt.Status);
        Assert.True(attempt.IsActive);
        Assert.False(attempt.IsTerminal);
    }

    [Fact]
    public void Failed_Attempt_Leaves_Refund_Pending()
    {
        var refund = Refund.CreateForSucceededPayment(SucceededPayment(), Now);
        var attempt = refund.CreateAttempt(Now.Plus(Duration.FromSeconds(1)));
        refund.RecordAttemptFailure(attempt.Id, Now.Plus(Duration.FromSeconds(2)));
        Assert.Equal(RefundAttemptStatus.Failed, attempt.Status);
        Assert.Equal(RefundStatus.Pending, refund.Status);
        Assert.Equal("Failed RefundAttempt != Failed logical Refund", PaymentRefundBoundary.FailedRefundAttemptIsNotFailedRefund);
    }

    [Fact]
    public void Retry_After_Definitive_Failure_Creates_New_Attempt()
    {
        var refund = Refund.CreateForSucceededPayment(SucceededPayment(), Now);
        var first = refund.CreateAttempt(Now.Plus(Duration.FromSeconds(1)));
        refund.RecordAttemptFailure(first.Id, Now.Plus(Duration.FromSeconds(2)));
        var second = refund.CreateAttempt(Now.Plus(Duration.FromSeconds(3)));
        Assert.NotEqual(first.Id, second.Id);
        Assert.Equal(RefundAttemptStatus.Created, second.Status);
    }

    [Fact]
    public void Unresolved_Attempt_Blocks_Retry()
    {
        var refund = Refund.CreateForSucceededPayment(SucceededPayment(), Now);
        refund.CreateAttempt(Now.Plus(Duration.FromSeconds(1)));
        Assert.Throws<InvalidOperationException>(() => refund.CreateAttempt(Now.Plus(Duration.FromSeconds(2))));
    }

    [Fact]
    public void Ambiguous_Initiation_Does_Not_Fail_Attempt()
    {
        var refund = Refund.CreateForSucceededPayment(SucceededPayment(), Now);
        var attempt = refund.CreateAttempt(Now.Plus(Duration.FromSeconds(1)));
        refund.RecordAmbiguousProviderInitiation(
            attempt.Id,
            Now.Plus(Duration.FromSeconds(2)),
            TestKey,
            new ProviderRequestReference("ref-amb"),
            null);
        Assert.Equal(RefundAttemptStatus.Created, attempt.Status);
        Assert.Equal(RefundStatus.Pending, refund.Status);
        Assert.Throws<InvalidOperationException>(() => refund.CreateAttempt(Now.Plus(Duration.FromSeconds(3))));
    }

    [Fact]
    public void Success_Prevents_More_Attempts_And_Leaves_Payment_Succeeded()
    {
        var payment = SucceededPayment();
        var refund = Refund.CreateForSucceededPayment(payment, Now);
        var attempt = refund.CreateAttempt(Now.Plus(Duration.FromSeconds(1)));
        refund.RecordAuthoritativeRefundSuccess(attempt.Id, Now.Plus(Duration.FromSeconds(2)));
        Assert.Equal(RefundStatus.Succeeded, refund.Status);
        Assert.Equal(RefundAttemptStatus.Succeeded, attempt.Status);
        Assert.Equal(PaymentStatus.Succeeded, payment.Status);
        Assert.Throws<InvalidOperationException>(() => refund.CreateAttempt(Now.Plus(Duration.FromSeconds(3))));
    }

    [Fact]
    public void Provider_Amount_Mismatch_Does_Not_Succeed_Refund()
    {
        var refund = Refund.CreateForSucceededPayment(SucceededPayment(), Now);
        var attempt = refund.CreateAttempt(Now.Plus(Duration.FromSeconds(1)));
        var status = VerifiedRefundOutcomeApplier.ApplyVerification(
            refund,
            attempt,
            new PaymentVerificationResult
            {
                Outcome = ProviderVerificationOutcome.Succeeded,
                ProviderKey = TestKey,
                ReportedAmount = 99m,
                ReportedCurrencyCode = "IRR",
            },
            Now.Plus(Duration.FromSeconds(2)));
        Assert.Equal(VerificationApplyStatus.AmountMismatch, status);
        Assert.Equal(RefundStatus.Pending, refund.Status);
        Assert.Equal(RefundAttemptStatus.Created, attempt.Status);
    }

    [Fact]
    public void Provider_Currency_Mismatch_Does_Not_Succeed_Refund()
    {
        var refund = Refund.CreateForSucceededPayment(SucceededPayment(), Now);
        var attempt = refund.CreateAttempt(Now.Plus(Duration.FromSeconds(1)));
        var status = VerifiedRefundOutcomeApplier.ApplyVerification(
            refund,
            attempt,
            new PaymentVerificationResult
            {
                Outcome = ProviderVerificationOutcome.Succeeded,
                ProviderKey = TestKey,
                ReportedAmount = 110m,
                ReportedCurrencyCode = "USD",
            },
            Now.Plus(Duration.FromSeconds(2)));
        Assert.Equal(VerificationApplyStatus.CurrencyMismatch, status);
        Assert.Equal(RefundStatus.Pending, refund.Status);
    }

    [Fact]
    public void Duplicate_Verified_Refund_Success_Is_Idempotent()
    {
        var refund = Refund.CreateForSucceededPayment(SucceededPayment(), Now);
        var attempt = refund.CreateAttempt(Now.Plus(Duration.FromSeconds(1)));
        var result = new PaymentVerificationResult
        {
            Outcome = ProviderVerificationOutcome.Succeeded,
            ProviderKey = TestKey,
            ReportedAmount = 110m,
            ReportedCurrencyCode = "IRR",
        };
        var first = VerifiedRefundOutcomeApplier.ApplyVerification(
            refund, attempt, result, Now.Plus(Duration.FromSeconds(2)));
        var second = VerifiedRefundOutcomeApplier.ApplyVerification(
            refund, attempt, result, Now.Plus(Duration.FromSeconds(3)));
        Assert.Equal(VerificationApplyStatus.Applied, first);
        Assert.Equal(VerificationApplyStatus.Unchanged, second);
        Assert.Equal(RefundStatus.Succeeded, refund.Status);
        Assert.Equal(1, refund.Attempts.Count(x => x.Status == RefundAttemptStatus.Succeeded));
    }

    [Fact]
    public async Task Compensation_Does_Not_Trust_Event_Amount()
    {
        await using var db = CreateDb();
        var payment = await SeedSucceededAsync(db);
        var fake = MatchingRefundFake();
        var handler = new BookingPaymentCompensationRequiredHandler(
            db,
            new RefundGetOrCreateService(db, new FixedClock(Now.Plus(Duration.FromMinutes(3)))),
            new RefundInitiationService(db, new PaymentProviderResolver([fake]), new FixedClock(Now.Plus(Duration.FromMinutes(3)))),
            new FixedClock(Now.Plus(Duration.FromMinutes(3))));

        await handler.HandleAsync(new BookingPaymentCompensationRequiredIntegrationEvent(
            Booking.BookingId,
            payment.Id.Value,
            "ExpiredHold",
            Now.Plus(Duration.FromMinutes(2))));

        var refund = await db.Refunds.Include(x => x.Attempts).SingleAsync();
        Assert.Equal(110m, refund.Amount.Amount);
        Assert.Equal("IRR", refund.Amount.Currency.Value);
        Assert.Equal(RefundStatus.Pending, refund.Status);
        Assert.Equal(RefundAttemptStatus.Initiated, refund.Attempts.Single().Status);
        Assert.Equal(PaymentStatus.Succeeded, (await db.Payments.SingleAsync()).Status);
        Assert.False(BookingCompensationOutboxBoundary.EventAmountIsAuthoritative);
    }

    [Fact]
    public async Task Refund_Success_Writes_Outbox_Without_Pii_And_Keeps_Payment_Succeeded()
    {
        await using var db = CreateDb();
        var payment = await SeedSucceededAsync(db);
        var refund = Refund.CreateForSucceededPayment(payment, Now.Plus(Duration.FromMinutes(3)));
        var attempt = refund.CreateAttempt(Now.Plus(Duration.FromMinutes(3)));
        refund.RecordProviderInitiation(
            attempt.Id,
            Now.Plus(Duration.FromMinutes(3)),
            TestKey,
            new ProviderRequestReference("ref-refund-1"),
            new ProviderTransactionReference("txn-refund-1"));
        db.Refunds.Add(refund);
        await db.SaveChangesAsync();

        var fake = MatchingRefundFake();
        var processor = new PaymentCallbackProcessor(
            db,
            new PaymentProviderResolver([fake]),
            new FixedClock(Now.Plus(Duration.FromMinutes(4))));
        await processor.ProcessAsync(VerifiedRefundEnvelope());

        var row = Assert.Single(await db.OutboxMessages
            .Where(x => x.MessageType == RefundSuccessOutboxBoundary.MessageType)
            .ToListAsync());
        Assert.Equal(refund.Id.Value, row.Id);
        Assert.DoesNotContain("pay@example.com", row.Payload, StringComparison.OrdinalIgnoreCase);
        var evt = RefundSucceededOutboxSerializer.Deserialize(row.Payload);
        Assert.Equal(refund.Id.Value, evt.RefundId);
        Assert.Equal(payment.Id.Value, evt.PaymentId);
        Assert.Equal(110m, evt.Amount);
        Assert.Equal("IRR", evt.CurrencyCode);
        Assert.Equal(PaymentStatus.Succeeded, (await db.Payments.SingleAsync()).Status);
        Assert.Equal(RefundStatus.Succeeded, (await db.Refunds.SingleAsync()).Status);
        Assert.False(RefundSuccessOutboxBoundary.EventMeansBookingCancelled);
    }

    [Fact]
    public void Gateway_Exposes_Provider_Neutral_Refund_Ports()
    {
        var names = typeof(IPaymentProviderGateway).GetMethods().Select(m => m.Name).ToArray();
        Assert.Contains("InitiateRefundAsync", names);
        Assert.Contains("VerifyRefundAsync", names);
        Assert.Contains("QueryRefundStatusAsync", names);
        Assert.Equal("NONE", PaymentProviderTrustBoundary.NamedProviderSelected);
        Assert.False(PaymentOwnershipBoundary.ProviderSdkImplemented);
    }

    private static PaymentAggregate SucceededPayment()
    {
        var payment = PaymentAggregate.Create(Booking, Now);
        payment.BindExecutionSnapshot(SnapshotId, new MoneyValue(110m, "IRR"), Now);
        var attempt = payment.CreateAttempt(Now);
        payment.RecordProviderInitiation(
            attempt.Id,
            Now.Plus(Duration.FromMinutes(1)),
            TestKey,
            new ProviderRequestReference("req-pay-1"),
            new ProviderTransactionReference("txn-pay-1"));
        payment.RecordAuthoritativeCollectionSuccess(attempt.Id, Now.Plus(Duration.FromMinutes(2)));
        return payment;
    }

    private static async Task<PaymentAggregate> SeedSucceededAsync(PaymentDbContext db)
    {
        var payment = SucceededPayment();
        db.Payments.Add(payment);
        await db.SaveChangesAsync();
        return payment;
    }

    private static FakePaymentProviderGateway MatchingRefundFake() =>
        new(TestKey)
        {
            NextRefundInitiation = PaymentInitiationOutcome.Initiated,
            NextRefundVerification = ProviderVerificationOutcome.Succeeded,
            ReportedRefundAmount = 110m,
            ReportedRefundCurrencyCode = "IRR",
        };

    private static PaymentCallbackEnvelope VerifiedRefundEnvelope() =>
        new()
        {
            ProviderKey = TestKey,
            Headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                [FakePaymentProviderGateway.VerifiedHeaderName] = "true",
                [PaymentCallbackKinds.HeaderName] = PaymentCallbackKinds.Refund,
            },
        };

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
