using Microsoft.EntityFrameworkCore;
using NodaTime;
using TravelCore.Modules.Booking.Contracts;
using TravelCore.Modules.Booking.Domain;
using TravelCore.Modules.Booking.Infrastructure;
using TravelCore.Modules.Booking.Infrastructure.Services;
using TravelCore.Modules.Payment.Contracts;
using TravelCore.Modules.Pricing.Contracts;
using Xunit;
using BookingAggregate = TravelCore.Modules.Booking.Domain.Booking;

namespace TravelCore.Persistence.IntegrationTests;

[Collection(nameof(BookingMigrationLifecycleCollection))]
public sealed class BookingPaymentSuccessConsumerPersistenceTests
{
    private readonly BookingMigrationLifecycleContainerFixture _postgres;

    public BookingPaymentSuccessConsumerPersistenceTests(BookingMigrationLifecycleContainerFixture postgres)
    {
        _postgres = postgres;
    }

    [Fact]
    public async Task Delayed_Delivery_After_Hold_Expire_Does_Not_Confirm()
    {
        var ct = TestContext.Current.CancellationToken;
        var ctx = await SeedEligibleAsync(ct, holdMinutes: 1);
        await using (var db = _postgres.CreateDbContext())
        {
            var hold = await db.CapacityHolds.SingleAsync(x => x.BookingId == ctx.BookingId, ct);
            await new BookingCapacityService(db).ExpireAsync(hold.Id, ctx.Now.Plus(Duration.FromMinutes(2)), ct);
        }

        await HandleAsync(ctx, ctx.Now.Plus(Duration.FromMinutes(2)), SuccessEvidence(ctx), ct);

        await using (var db = _postgres.CreateDbContext())
        {
            var booking = await db.Bookings.SingleAsync(x => x.Id == ctx.BookingId, ct);
            var issue = await db.ConfirmationRecoveryIssues.SingleAsync(x => x.BookingId == ctx.BookingId, ct);
            Assert.Equal(BookingStatus.Pending, booking.Status);
            Assert.Equal(BookingConfirmationRecoveryReason.ExpiredHold, issue.Reason);
            Assert.Equal(1, await db.PaymentSuccessInbox.CountAsync(x => x.PaymentId == ctx.PaymentId, ct));
        }
    }

    [Fact]
    public async Task Delayed_Delivery_After_Cancel_Does_Not_Confirm()
    {
        var ct = TestContext.Current.CancellationToken;
        var ctx = await SeedEligibleAsync(ct);
        await using (var db = _postgres.CreateDbContext())
        {
            await new BookingCancellationService(db)
                .CancelPendingAsync(ctx.BookingId, ctx.Now.Plus(Duration.FromMinutes(1)), ct);
        }

        await HandleAsync(ctx, ctx.Now.Plus(Duration.FromMinutes(2)), SuccessEvidence(ctx), ct);

        await using (var db = _postgres.CreateDbContext())
        {
            Assert.Equal(BookingStatus.Cancelled, (await db.Bookings.SingleAsync(x => x.Id == ctx.BookingId, ct)).Status);
            Assert.Equal(
                BookingConfirmationRecoveryReason.CancelledBooking,
                (await db.ConfirmationRecoveryIssues.SingleAsync(x => x.BookingId == ctx.BookingId, ct)).Reason);
            Assert.Equal(1, await db.PaymentSuccessInbox.CountAsync(x => x.PaymentId == ctx.PaymentId, ct));
        }
    }

    [Fact]
    public async Task Duplicate_Delivery_Confirms_Once_And_Is_Idempotent()
    {
        var ct = TestContext.Current.CancellationToken;
        var ctx = await SeedEligibleAsync(ct);
        var later = ctx.Now.Plus(Duration.FromMinutes(1));
        await HandleAsync(ctx, later, SuccessEvidence(ctx), ct);
        await HandleAsync(ctx, later.Plus(Duration.FromMinutes(1)), SuccessEvidence(ctx), ct);

        await using var db = _postgres.CreateDbContext();
        Assert.Equal(BookingStatus.Confirmed, (await db.Bookings.SingleAsync(x => x.Id == ctx.BookingId, ct)).Status);
        Assert.Equal(1, await db.CapacityHolds.CountAsync(x => x.BookingId == ctx.BookingId && x.Status == CapacityHoldStatus.Consumed, ct));
        Assert.Equal(1, await db.PaymentSuccessInbox.CountAsync(x => x.PaymentId == ctx.PaymentId, ct));
        Assert.Equal(0, await db.ConfirmationRecoveryIssues.CountAsync(x => x.BookingId == ctx.BookingId, ct));
    }

    [Fact]
    public async Task Consumer_Revalidates_Evidence_And_Does_Not_Trust_Event_Amount()
    {
        var ct = TestContext.Current.CancellationToken;
        var ctx = await SeedEligibleAsync(ct);
        var evidence = new FakePaymentEvidence
        {
            Next = new PaymentSuccessEvidenceRead(
                ctx.PaymentId,
                ctx.BookingId.Value,
                "Succeeded",
                999m,
                "USD",
                true),
        };
        await HandleAsync(ctx, ctx.Now.Plus(Duration.FromMinutes(1)), evidence, ct);

        await using var db = _postgres.CreateDbContext();
        Assert.Equal(BookingStatus.Pending, (await db.Bookings.SingleAsync(x => x.Id == ctx.BookingId, ct)).Status);
        Assert.Equal(
            BookingConfirmationRecoveryReason.MonetaryMismatch,
            (await db.ConfirmationRecoveryIssues.SingleAsync(x => x.BookingId == ctx.BookingId, ct)).Reason);
        Assert.Equal(1, await db.PaymentSuccessInbox.CountAsync(x => x.PaymentId == ctx.PaymentId, ct));
    }

    [Fact]
    public async Task Missing_Evidence_Leaves_Inbox_Empty_For_Retry()
    {
        var ct = TestContext.Current.CancellationToken;
        var ctx = await SeedEligibleAsync(ct);
        var evidence = new FakePaymentEvidence { Next = null };
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            HandleAsync(ctx, ctx.Now.Plus(Duration.FromMinutes(1)), evidence, ct));

        await using var db = _postgres.CreateDbContext();
        Assert.Equal(BookingStatus.Pending, (await db.Bookings.SingleAsync(x => x.Id == ctx.BookingId, ct)).Status);
        Assert.Equal(0, await db.PaymentSuccessInbox.CountAsync(x => x.PaymentId == ctx.PaymentId, ct));
    }

    private async Task HandleAsync(
        SeededBooking ctx,
        Instant now,
        IPaymentSuccessEvidenceQuery evidence,
        CancellationToken ct)
    {
        await using var db = _postgres.CreateDbContext();
        var handler = new BookingPaymentSucceededIntegrationHandler(
            db,
            new BookingPaymentConfirmationService(db, evidence),
            new FixedClock(now));
        await handler.HandleAsync(
            new PaymentSucceededIntegrationEvent(
                ctx.PaymentId,
                ctx.BookingId.Value,
                ctx.Now,
                110m,
                "USD"),
            ct);
    }

    private async Task<SeededBooking> SeedEligibleAsync(CancellationToken ct, int holdMinutes = 30)
    {
        await using (var db = _postgres.CreateDbContext())
        {
            await BookingMigrator.MigrateAsync(db, ct);
        }

        var departure = new TourDepartureReference(Guid.CreateVersion7());
        var now = Instant.FromUtc(2026, 8, 18, 20, 0);
        var paymentId = Guid.CreateVersion7();
        BookingId bookingId;
        await using (var db = _postgres.CreateDbContext())
        {
            var booking = BookingAggregate.Create(departure, now);
            booking.SetContact(BookingContactSnapshot.Create("Pay User", "pay@example.com"));
            booking.AddPassenger("Pay", "User", TravelerCategory.Adult, 1);
            booking.AcceptQuote(Facts(departure.LogicalId, now), now);
            db.Bookings.Add(booking);
            await db.SaveChangesAsync(ct);
            bookingId = booking.Id;
        }

        await using (var db = _postgres.CreateDbContext())
        {
            await new BookingCapacityService(db).HoldAsync(
                new HoldCapacityCommand(
                    bookingId,
                    1,
                    5,
                    now.Plus(Duration.FromMinutes(holdMinutes)),
                    now,
                    "hold-" + bookingId.Value),
                ct);
        }

        return new SeededBooking(bookingId, paymentId, now);
    }

    private static FakePaymentEvidence SuccessEvidence(SeededBooking ctx) =>
        new()
        {
            Next = new PaymentSuccessEvidenceRead(
                ctx.PaymentId,
                ctx.BookingId.Value,
                "Succeeded",
                110m,
                "USD",
                true),
        };

    private static AuthoritativeQuoteFacts Facts(Guid departureId, Instant now) =>
        AuthoritativeQuoteFacts.Create(
            PricingQuoteReference.From(Guid.CreateVersion7()),
            Guid.CreateVersion7(),
            BookingOwnershipBoundary.InitialTarget,
            departureId,
            now.Minus(Duration.FromMinutes(10)),
            now.Plus(Duration.FromHours(6)),
            [
                new AuthoritativeQuoteComponentFact(
                    BookingMonetaryComponentKind.Base,
                    new TravelCore.Money.Money(100m, "USD"),
                    0,
                    "BASE",
                    "Base"),
                new AuthoritativeQuoteComponentFact(
                    BookingMonetaryComponentKind.Fee,
                    new TravelCore.Money.Money(10m, "USD"),
                    1,
                    "FEE",
                    "Fee")
            ]);

    private sealed record SeededBooking(BookingId BookingId, Guid PaymentId, Instant Now);

    private sealed class FakePaymentEvidence : IPaymentSuccessEvidenceQuery
    {
        public PaymentSuccessEvidenceRead? Next { get; set; }

        public Task<PaymentSuccessEvidenceRead?> GetByBookingIdAsync(
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
