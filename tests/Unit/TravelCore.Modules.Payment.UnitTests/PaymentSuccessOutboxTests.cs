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

public sealed class PaymentSuccessOutboxTests
{
    private static readonly Instant Now = Instant.FromUtc(2026, 8, 18, 18, 0);
    private static readonly BookingReference Booking =
        new(Guid.Parse("0198b3e0-0000-7000-8000-000000000701"));
    private static readonly Guid SnapshotId = Guid.Parse("0198b3e0-0000-7000-8000-000000000702");
    private static readonly ProviderKey TestKey = new("test");

    [Fact]
    public void PaymentSucceeded_Event_Is_Trigger_Not_Booking_Confirmation()
    {
        Assert.True(PaymentSuccessOutboxBoundary.TransactionalOutboxImplemented);
        Assert.False(PaymentSuccessOutboxBoundary.EventMeansBookingConfirmed);
        Assert.Equal("at-least-once", PaymentSuccessOutboxBoundary.DeliverySemantics);
        Assert.Equal("PaymentSucceeded != BookingConfirmed", PaymentOwnershipBoundary.PaymentSucceededIsNotBookingConfirmed);
        Assert.False(PaymentOwnershipBoundary.BookingConfirmImplemented);
        Assert.False(PaymentOwnershipBoundary.SharedDbContextImplemented);
    }

    [Fact]
    public async Task Verified_Success_Writes_One_Outbox_Event_Without_Pii()
    {
        await using var db = CreateDb();
        var fake = MatchingFake();
        var payment = await SeedInitiatedAsync(db, fake);
        var processor = new PaymentCallbackProcessor(db, new PaymentProviderResolver([fake]), new FixedClock(Now.Plus(Duration.FromMinutes(2))));

        await processor.ProcessAsync(VerifiedEnvelope());

        var row = Assert.Single(db.OutboxMessages);
        Assert.Equal(payment.Id.Value, row.Id);
        Assert.Equal(PaymentSuccessOutboxBoundary.MessageType, row.MessageType);
        Assert.Null(row.ProcessedAt);
        Assert.DoesNotContain("pay@example.com", row.Payload, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Pay User", row.Payload, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("passport", row.Payload, StringComparison.OrdinalIgnoreCase);
        var evt = PaymentSucceededOutboxSerializer.Deserialize(row.Payload);
        Assert.Equal(payment.Id.Value, evt.PaymentId);
        Assert.Equal(Booking.BookingId, evt.BookingId);
        Assert.Equal(110m, evt.Amount);
        Assert.Equal("IRR", evt.CurrencyCode);
        Assert.Equal(PaymentStatus.Succeeded, (await db.Payments.SingleAsync()).Status);
    }

    [Fact]
    public async Task Duplicate_Verified_Success_Writes_One_Logical_Event()
    {
        await using var db = CreateDb();
        var fake = MatchingFake();
        await SeedInitiatedAsync(db, fake);
        var processor = new PaymentCallbackProcessor(db, new PaymentProviderResolver([fake]), new FixedClock(Now.Plus(Duration.FromMinutes(2))));
        await processor.ProcessAsync(VerifiedEnvelope());
        await processor.ProcessAsync(VerifiedEnvelope());

        Assert.Equal(1, await db.OutboxMessages.CountAsync());
        Assert.Equal(PaymentStatus.Succeeded, (await db.Payments.SingleAsync()).Status);
    }

    [Fact]
    public async Task Attempt_Failure_Does_Not_Write_Success_Outbox()
    {
        await using var db = CreateDb();
        var fake = MatchingFake();
        fake.NextVerification = ProviderVerificationOutcome.Failed;
        await SeedInitiatedAsync(db, fake);
        var processor = new PaymentCallbackProcessor(db, new PaymentProviderResolver([fake]), new FixedClock(Now.Plus(Duration.FromMinutes(2))));
        await processor.ProcessAsync(VerifiedEnvelope());

        Assert.Empty(db.OutboxMessages);
        Assert.Equal(PaymentStatus.Pending, (await db.Payments.SingleAsync()).Status);
    }

    [Fact]
    public async Task Dispatcher_Leaves_Pending_When_Consumer_Throws()
    {
        await using var db = CreateDb();
        var fake = MatchingFake();
        await SeedInitiatedAsync(db, fake);
        await new PaymentCallbackProcessor(db, new PaymentProviderResolver([fake]), new FixedClock(Now.Plus(Duration.FromMinutes(2))))
            .ProcessAsync(VerifiedEnvelope());
        var dispatcher = new PaymentSuccessOutboxDispatcher(db, new ThrowingHandler(), new FixedClock(Now.Plus(Duration.FromMinutes(3))));

        await Assert.ThrowsAsync<InvalidOperationException>(() => dispatcher.DispatchPendingAsync());
        Assert.Null((await db.OutboxMessages.SingleAsync()).ProcessedAt);
    }

    [Fact]
    public async Task Dispatcher_Marks_Processed_After_Handler_Succeeds()
    {
        await using var db = CreateDb();
        var fake = MatchingFake();
        var payment = await SeedInitiatedAsync(db, fake);
        await new PaymentCallbackProcessor(db, new PaymentProviderResolver([fake]), new FixedClock(Now.Plus(Duration.FromMinutes(2))))
            .ProcessAsync(VerifiedEnvelope());
        var handler = new RecordingHandler();
        var processedAt = Now.Plus(Duration.FromMinutes(4));
        var dispatcher = new PaymentSuccessOutboxDispatcher(db, handler, new FixedClock(processedAt));

        Assert.Equal(1, await dispatcher.DispatchPendingAsync());
        Assert.Equal(processedAt, (await db.OutboxMessages.SingleAsync()).ProcessedAt);
        Assert.Equal(payment.Id.Value, handler.Last!.PaymentId);
        Assert.Equal(0, await dispatcher.DispatchPendingAsync());
    }

    private static async Task<PaymentAggregate> SeedInitiatedAsync(PaymentDbContext db, FakePaymentProviderGateway fake)
    {
        var payment = PaymentAggregate.Create(Booking, Now);
        payment.BindExecutionSnapshot(SnapshotId, new MoneyValue(110m, "IRR"), Now);
        var attempt = payment.CreateAttempt(Now);
        payment.RecordProviderInitiation(
            attempt.Id,
            Now.Plus(Duration.FromMinutes(1)),
            TestKey,
            fake.RequestReference,
            fake.TransactionReference);
        db.Payments.Add(payment);
        await db.SaveChangesAsync();
        return payment;
    }

    private static FakePaymentProviderGateway MatchingFake() =>
        new(TestKey)
        {
            NextVerification = ProviderVerificationOutcome.Succeeded,
            ReportedAmount = 110m,
            ReportedCurrencyCode = "IRR",
        };

    private static PaymentCallbackEnvelope VerifiedEnvelope() =>
        new()
        {
            ProviderKey = TestKey,
            Headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                [FakePaymentProviderGateway.VerifiedHeaderName] = "true",
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

    private sealed class ThrowingHandler : IPaymentSucceededIntegrationHandler
    {
        public Task HandleAsync(PaymentSucceededIntegrationEvent message, CancellationToken cancellationToken = default) =>
            throw new InvalidOperationException("transient-consumer-failure");
    }

    private sealed class RecordingHandler : IPaymentSucceededIntegrationHandler
    {
        public PaymentSucceededIntegrationEvent? Last { get; private set; }

        public Task HandleAsync(PaymentSucceededIntegrationEvent message, CancellationToken cancellationToken = default)
        {
            Last = message;
            return Task.CompletedTask;
        }
    }
}
