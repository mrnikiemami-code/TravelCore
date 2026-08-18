using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using NodaTime;
using TravelCore.Modules.Booking.Contracts;
using TravelCore.Modules.Booking.Domain;
using TravelCore.Modules.Booking.Infrastructure;
using TravelCore.Modules.Booking.Infrastructure.Services;
using TravelCore.Modules.Payment.Contracts;
using TravelCore.Modules.Pricing.Contracts;
using Xunit;
using BookingAggregate = TravelCore.Modules.Booking.Domain.Booking;

namespace TravelCore.Modules.Booking.UnitTests;

public sealed class BookingCompensationTests
{
    private static readonly Instant Now = Instant.FromUtc(2026, 8, 18, 21, 0);
    private static readonly TourDepartureReference Departure =
        new(Guid.Parse("0198b3e0-0000-7000-8000-000000000810"));

    [Fact]
    public async Task Recovery_Issue_Produces_Compensation_Required_Intent_Without_Amount()
    {
        var issue = BookingConfirmationRecoveryIssue.Create(
            BookingId.New(),
            Guid.CreateVersion7(),
            BookingConfirmationRecoveryReason.ExpiredHold,
            Now);
        using var db = CreateDb();
        BookingCompensationOutboxWriter.Enqueue(db, issue, Now);
        await db.SaveChangesAsync();
        var row = Assert.Single(db.OutboxMessages);
        Assert.Equal(issue.PaymentId, row.Id);
        Assert.Equal(BookingCompensationOutboxBoundary.MessageType, row.MessageType);
        var evt = BookingCompensationOutboxSerializer.Deserialize(row.Payload);
        Assert.Equal(issue.BookingId.Value, evt.BookingId);
        Assert.Equal(issue.PaymentId, evt.PaymentId);
        Assert.Equal(nameof(BookingConfirmationRecoveryReason.ExpiredHold), evt.RecoveryReason);
        Assert.DoesNotContain("amount", row.Payload, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("110", row.Payload, StringComparison.Ordinal);
        Assert.False(BookingCompensationOutboxBoundary.EventAmountIsAuthoritative);
        Assert.Equal("RecoveryIssue != Refund", BookingOrchestrationBoundary.RecoveryIssueIsNotRefund);
        Assert.True(BookingOrchestrationBoundary.CompensationRequiredOutboxImplemented);
        Assert.True(BookingOrchestrationBoundary.RefundSucceededConsumerImplemented);
        Assert.False(BookingOrchestrationBoundary.ConfirmedToCancelledImplemented);
        Assert.Equal("RefundSucceeded != BookingCancelled", BookingOrchestrationBoundary.RefundSucceededIsNotBookingCancelled);
        Assert.False(RefundSuccessOutboxBoundary.EventMeansBookingCancelled);
    }

    [Fact]
    public void Confirmed_Booking_Cannot_Be_Cancelled()
    {
        var booking = EligiblePending();
        booking.ConfirmFromAuthoritativePaymentSuccess(Now.Plus(Duration.FromMinutes(1)));
        Assert.Throws<InvalidOperationException>(() => booking.CancelPending(Now.Plus(Duration.FromMinutes(2))));
        Assert.Equal(BookingStatus.Confirmed, booking.Status);
    }

    [Fact]
    public void Consumed_Hold_Cannot_Be_Released()
    {
        var bookingId = BookingId.New();
        var hold = CapacityHold.Create(
            bookingId,
            Departure,
            1,
            5,
            Now,
            Now.Plus(Duration.FromMinutes(30)),
            "hold-" + bookingId.Value);
        hold.Consume(Now.Plus(Duration.FromMinutes(1)));
        Assert.Equal(CapacityHoldStatus.Consumed, hold.Status);
        Assert.Throws<InvalidOperationException>(() => hold.Release(Now.Plus(Duration.FromMinutes(2))));
    }

    [Fact]
    public async Task RefundSucceeded_On_Confirmed_Does_Not_Cancel()
    {
        await using var db = CreateDb();
        var booking = EligiblePending();
        booking.ConfirmFromAuthoritativePaymentSuccess(Now.Plus(Duration.FromMinutes(1)));
        db.Bookings.Add(booking);
        await db.SaveChangesAsync();
        var handler = new BookingRefundSucceededIntegrationHandler(
            db,
            new BookingCancellationService(db),
            new FixedClock(Now.Plus(Duration.FromMinutes(2))));

        await handler.HandleAsync(new RefundSucceededIntegrationEvent(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            booking.Id.Value,
            Now.Plus(Duration.FromMinutes(2)),
            110m,
            "USD"));

        Assert.Equal(BookingStatus.Confirmed, (await db.Bookings.SingleAsync()).Status);
        Assert.Equal(BookingRefundInvariantIssueKind.ConfirmedBooking, (await db.RefundInvariantIssues.SingleAsync()).Kind);
        Assert.Equal(1, await db.RefundSuccessInbox.CountAsync());
    }

    [Fact]
    public async Task RefundSucceeded_Pending_With_Active_Hold_Cancels_And_Releases()
    {
        await using var db = await SeedPendingWithHoldAsync(CapacityHoldStatus.Active);
        var bookingId = (await db.Bookings.SingleAsync()).Id;
        await HandleRefundAsync(db, bookingId);

        var booking = await db.Bookings.SingleAsync();
        var hold = await db.CapacityHolds.SingleAsync();
        var account = await db.DepartureCapacityAccounts.SingleAsync();
        Assert.Equal(BookingStatus.Cancelled, booking.Status);
        Assert.Equal(CapacityHoldStatus.Released, hold.Status);
        Assert.Equal(0, account.ActiveSeats);
    }

    [Fact]
    public async Task RefundSucceeded_Pending_With_Expired_Hold_Cancels_And_Keeps_Expired()
    {
        await using var db = await SeedPendingWithHoldAsync(CapacityHoldStatus.Expired);
        var bookingId = (await db.Bookings.SingleAsync()).Id;
        await HandleRefundAsync(db, bookingId);

        Assert.Equal(BookingStatus.Cancelled, (await db.Bookings.SingleAsync()).Status);
        Assert.Equal(CapacityHoldStatus.Expired, (await db.CapacityHolds.SingleAsync()).Status);
    }

    [Fact]
    public async Task RefundSucceeded_Pending_With_Released_Hold_Cancels_And_Keeps_Released()
    {
        await using var db = await SeedPendingWithHoldAsync(CapacityHoldStatus.Released);
        var bookingId = (await db.Bookings.SingleAsync()).Id;
        await HandleRefundAsync(db, bookingId);

        Assert.Equal(BookingStatus.Cancelled, (await db.Bookings.SingleAsync()).Status);
        Assert.Equal(CapacityHoldStatus.Released, (await db.CapacityHolds.SingleAsync()).Status);
    }

    [Fact]
    public async Task RefundSucceeded_On_Already_Cancelled_Is_Idempotent()
    {
        await using var db = CreateDb();
        var booking = EligiblePending();
        booking.CancelPending(Now.Plus(Duration.FromMinutes(1)));
        db.Bookings.Add(booking);
        await db.SaveChangesAsync();

        var message = new RefundSucceededIntegrationEvent(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            booking.Id.Value,
            Now.Plus(Duration.FromMinutes(2)),
            110m,
            "USD");
        var handler = new BookingRefundSucceededIntegrationHandler(
            db,
            new BookingCancellationService(db),
            new FixedClock(Now.Plus(Duration.FromMinutes(2))));
        await handler.HandleAsync(message);
        await handler.HandleAsync(message);

        Assert.Equal(BookingStatus.Cancelled, (await db.Bookings.SingleAsync()).Status);
        Assert.Equal(1, await db.RefundSuccessInbox.CountAsync());
        Assert.Equal(0, await db.RefundInvariantIssues.CountAsync());
    }

    [Fact]
    public async Task Recovery_On_Expired_Hold_Writes_Compensation_Outbox_Atomically()
    {
        await using var db = await SeedPendingWithHoldAsync(CapacityHoldStatus.Expired);
        var booking = await db.Bookings.SingleAsync();
        var paymentId = Guid.CreateVersion7();
        await new BookingPaymentConfirmationService(db, new FakeEvidence(booking.Id.Value, paymentId))
            .ConfirmIfEligibleAsync(booking.Id, Now.Plus(Duration.FromMinutes(40)));

        Assert.Equal(BookingStatus.Pending, (await db.Bookings.SingleAsync()).Status);
        var issue = await db.ConfirmationRecoveryIssues.SingleAsync();
        Assert.Equal(BookingConfirmationRecoveryReason.ExpiredHold, issue.Reason);
        var outbox = await db.OutboxMessages.SingleAsync();
        Assert.Equal(paymentId, outbox.Id);
        Assert.Equal(BookingCompensationOutboxBoundary.MessageType, outbox.MessageType);
    }

    private static async Task HandleRefundAsync(BookingDbContext db, BookingId bookingId)
    {
        var handler = new BookingRefundSucceededIntegrationHandler(
            db,
            new BookingCancellationService(db),
            new FixedClock(Now.Plus(Duration.FromMinutes(2))));
        await handler.HandleAsync(new RefundSucceededIntegrationEvent(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            bookingId.Value,
            Now.Plus(Duration.FromMinutes(2)),
            110m,
            "USD"));
    }

    private static async Task<BookingDbContext> SeedPendingWithHoldAsync(CapacityHoldStatus status)
    {
        var db = CreateDb();
        var booking = EligiblePending();
        db.Bookings.Add(booking);
        var account = DepartureCapacityAccount.Create(Departure);
        account.Reserve(1, 5);
        var hold = CapacityHold.Create(
            booking.Id,
            Departure,
            1,
            5,
            Now,
            Now.Plus(Duration.FromMinutes(30)),
            "hold-" + booking.Id.Value);
        if (status == CapacityHoldStatus.Expired)
        {
            hold.Expire(Now.Plus(Duration.FromMinutes(31)));
            account.ReleaseActive(1);
        }
        else if (status == CapacityHoldStatus.Released)
        {
            hold.Release(Now.Plus(Duration.FromMinutes(1)));
            account.ReleaseActive(1);
        }

        db.DepartureCapacityAccounts.Add(account);
        db.CapacityHolds.Add(hold);
        await db.SaveChangesAsync();
        return db;
    }

    private sealed class FakeEvidence : IPaymentSuccessEvidenceQuery
    {
        private readonly PaymentSuccessEvidenceRead _next;

        public FakeEvidence(Guid bookingId, Guid paymentId)
        {
            _next = new PaymentSuccessEvidenceRead(paymentId, bookingId, "Succeeded", 110m, "USD", true);
        }

        public Task<PaymentSuccessEvidenceRead?> GetByBookingIdAsync(
            Guid bookingId,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<PaymentSuccessEvidenceRead?>(_next);
    }

    private static BookingAggregate EligiblePending()
    {
        var booking = BookingAggregate.Create(Departure, Now);
        booking.SetContact(BookingContactSnapshot.Create("Pay User", "pay@example.com"));
        booking.AddPassenger("Pay", "User", TravelerCategory.Adult, 1);
        booking.AcceptQuote(
            AuthoritativeQuoteFacts.Create(
                PricingQuoteReference.From(Guid.CreateVersion7()),
                Guid.CreateVersion7(),
                BookingOwnershipBoundary.InitialTarget,
                Departure.LogicalId,
                Now.Minus(Duration.FromMinutes(10)),
                Now.Plus(Duration.FromHours(6)),
                [
                    new AuthoritativeQuoteComponentFact(
                        BookingMonetaryComponentKind.Base,
                        new TravelCore.Money.Money(110m, "USD"),
                        0,
                        "BASE",
                        "Base")
                ]),
            Now);
        return booking;
    }

    private static BookingDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<BookingDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;
        return new BookingDbContext(options);
    }

    private sealed class FixedClock : IClock
    {
        private readonly Instant _instant;
        public FixedClock(Instant instant) => _instant = instant;
        public Instant GetCurrentInstant() => _instant;
    }
}
