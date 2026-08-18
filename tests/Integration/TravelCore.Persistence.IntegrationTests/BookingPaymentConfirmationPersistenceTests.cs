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
public sealed class BookingPaymentConfirmationPersistenceTests
{
    private readonly BookingMigrationLifecycleContainerFixture _postgres;

    public BookingPaymentConfirmationPersistenceTests(BookingMigrationLifecycleContainerFixture postgres)
    {
        _postgres = postgres;
    }

    [Fact]
    public async Task Valid_Payment_Success_Confirms_Booking_And_Consumes_Hold()
    {
        var ct = TestContext.Current.CancellationToken;
        var ctx = await SeedEligibleAsync(ct);
        var evidence = SuccessEvidence(ctx);
        await using (var db = _postgres.CreateDbContext())
        {
            await new BookingPaymentConfirmationService(db, evidence)
                .ConfirmIfEligibleAsync(ctx.BookingId, ctx.Now.Plus(Duration.FromMinutes(1)), ct);
        }

        await using (var db = _postgres.CreateDbContext())
        {
            var booking = await db.Bookings.SingleAsync(x => x.Id == ctx.BookingId, ct);
            var hold = await db.CapacityHolds.SingleAsync(x => x.BookingId == ctx.BookingId, ct);
            var account = await db.DepartureCapacityAccounts.SingleAsync(x => x.TourDeparture == ctx.Departure, ct);
            Assert.Equal(BookingStatus.Confirmed, booking.Status);
            Assert.Equal(CapacityHoldStatus.Consumed, hold.Status);
            Assert.Equal(1, account.ConsumedSeats);
            Assert.Equal(0, account.ActiveSeats);
            Assert.Equal(0, await db.ConfirmationRecoveryIssues.CountAsync(x => x.BookingId == ctx.BookingId, ct));
        }
    }

    [Fact]
    public async Task Duplicate_Payment_Success_Confirms_Once()
    {
        var ct = TestContext.Current.CancellationToken;
        var ctx = await SeedEligibleAsync(ct);
        var evidence = SuccessEvidence(ctx);
        var later = ctx.Now.Plus(Duration.FromMinutes(1));
        await using (var db = _postgres.CreateDbContext())
        {
            var service = new BookingPaymentConfirmationService(db, evidence);
            await service.ConfirmIfEligibleAsync(ctx.BookingId, later, ct);
        }

        await using (var db = _postgres.CreateDbContext())
        {
            await new BookingPaymentConfirmationService(db, evidence)
                .ConfirmIfEligibleAsync(ctx.BookingId, later.Plus(Duration.FromMinutes(1)), ct);
        }

        await using (var db = _postgres.CreateDbContext())
        {
            Assert.Equal(1, await db.CapacityHolds.CountAsync(x => x.BookingId == ctx.BookingId && x.Status == CapacityHoldStatus.Consumed, ct));
            Assert.Equal(BookingStatus.Confirmed, (await db.Bookings.SingleAsync(x => x.Id == ctx.BookingId, ct)).Status);
            Assert.Equal(0, await db.ConfirmationRecoveryIssues.CountAsync(x => x.BookingId == ctx.BookingId, ct));
        }
    }

    [Fact]
    public async Task Expired_Hold_After_Payment_Does_Not_Confirm()
    {
        var ct = TestContext.Current.CancellationToken;
        var ctx = await SeedEligibleAsync(ct, holdMinutes: 1);
        await using (var db = _postgres.CreateDbContext())
        {
            var hold = await db.CapacityHolds.SingleAsync(x => x.BookingId == ctx.BookingId, ct);
            await new BookingCapacityService(db).ExpireAsync(hold.Id, ctx.Now.Plus(Duration.FromMinutes(2)), ct);
        }

        await using (var db = _postgres.CreateDbContext())
        {
            await new BookingPaymentConfirmationService(db, SuccessEvidence(ctx))
                .ConfirmIfEligibleAsync(ctx.BookingId, ctx.Now.Plus(Duration.FromMinutes(2)), ct);
        }

        await AssertRecoveryAsync(ctx, BookingConfirmationRecoveryReason.ExpiredHold, CapacityHoldStatus.Expired, ct);
    }

    [Fact]
    public async Task Released_Hold_After_Payment_Does_Not_Confirm_Or_Resurrect()
    {
        var ct = TestContext.Current.CancellationToken;
        var ctx = await SeedEligibleAsync(ct);
        await using (var db = _postgres.CreateDbContext())
        {
            var hold = await db.CapacityHolds.SingleAsync(x => x.BookingId == ctx.BookingId, ct);
            await new BookingCapacityService(db).ReleaseAsync(hold.Id, ctx.Now.Plus(Duration.FromMinutes(1)), ct);
        }

        await using (var db = _postgres.CreateDbContext())
        {
            await new BookingPaymentConfirmationService(db, SuccessEvidence(ctx))
                .ConfirmIfEligibleAsync(ctx.BookingId, ctx.Now.Plus(Duration.FromMinutes(1)), ct);
        }

        await AssertRecoveryAsync(ctx, BookingConfirmationRecoveryReason.ReleasedHold, CapacityHoldStatus.Released, ct);
        await using (var db = _postgres.CreateDbContext())
        {
            var account = await db.DepartureCapacityAccounts.SingleAsync(x => x.TourDeparture == ctx.Departure, ct);
            Assert.Equal(0, account.EffectiveSeats);
        }
    }

    [Fact]
    public async Task Cancelled_Booking_After_Payment_Does_Not_Reopen()
    {
        var ct = TestContext.Current.CancellationToken;
        var ctx = await SeedEligibleAsync(ct);
        await using (var db = _postgres.CreateDbContext())
        {
            await new BookingCancellationService(db)
                .CancelPendingAsync(ctx.BookingId, ctx.Now.Plus(Duration.FromMinutes(1)), ct);
        }

        await using (var db = _postgres.CreateDbContext())
        {
            await new BookingPaymentConfirmationService(db, SuccessEvidence(ctx))
                .ConfirmIfEligibleAsync(ctx.BookingId, ctx.Now.Plus(Duration.FromMinutes(2)), ct);
        }

        await using (var db = _postgres.CreateDbContext())
        {
            var booking = await db.Bookings.SingleAsync(x => x.Id == ctx.BookingId, ct);
            var issue = await db.ConfirmationRecoveryIssues.SingleAsync(x => x.BookingId == ctx.BookingId, ct);
            Assert.Equal(BookingStatus.Cancelled, booking.Status);
            Assert.Equal(BookingConfirmationRecoveryReason.CancelledBooking, issue.Reason);
            Assert.Equal(ctx.PaymentId, issue.PaymentId);
        }
    }

    [Fact]
    public async Task Amount_Mismatch_At_Booking_Consumer_Does_Not_Confirm()
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
        await using (var db = _postgres.CreateDbContext())
        {
            await new BookingPaymentConfirmationService(db, evidence)
                .ConfirmIfEligibleAsync(ctx.BookingId, ctx.Now.Plus(Duration.FromMinutes(1)), ct);
        }

        await AssertRecoveryAsync(ctx, BookingConfirmationRecoveryReason.MonetaryMismatch, CapacityHoldStatus.Active, ct);
    }

    [Fact]
    public async Task Currency_Mismatch_At_Booking_Consumer_Does_Not_Confirm()
    {
        var ct = TestContext.Current.CancellationToken;
        var ctx = await SeedEligibleAsync(ct);
        var evidence = new FakePaymentEvidence
        {
            Next = new PaymentSuccessEvidenceRead(
                ctx.PaymentId,
                ctx.BookingId.Value,
                "Succeeded",
                110m,
                "EUR",
                true),
        };
        await using (var db = _postgres.CreateDbContext())
        {
            await new BookingPaymentConfirmationService(db, evidence)
                .ConfirmIfEligibleAsync(ctx.BookingId, ctx.Now.Plus(Duration.FromMinutes(1)), ct);
        }

        await AssertRecoveryAsync(ctx, BookingConfirmationRecoveryReason.MonetaryMismatch, CapacityHoldStatus.Active, ct);
    }

    [Fact]
    public async Task Concurrent_Duplicate_Success_Does_Not_Double_Consume()
    {
        var ct = TestContext.Current.CancellationToken;
        var ctx = await SeedEligibleAsync(ct);
        var later = ctx.Now.Plus(Duration.FromMinutes(1));
        var first = ConfirmOnNewContextAsync(ctx, later, ct);
        var second = ConfirmOnNewContextAsync(ctx, later, ct);
        await Task.WhenAll(first, second);

        await using var db = _postgres.CreateDbContext();
        Assert.Equal(BookingStatus.Confirmed, (await db.Bookings.SingleAsync(x => x.Id == ctx.BookingId, ct)).Status);
        Assert.Equal(1, await db.CapacityHolds.CountAsync(x => x.BookingId == ctx.BookingId && x.Status == CapacityHoldStatus.Consumed, ct));
        Assert.Equal(1, (await db.DepartureCapacityAccounts.SingleAsync(x => x.TourDeparture == ctx.Departure, ct)).ConsumedSeats);
    }

    [Fact]
    public async Task Cancellation_Wins_Race_Cannot_Become_Confirmed()
    {
        var ct = TestContext.Current.CancellationToken;
        var ctx = await SeedEligibleAsync(ct);
        var later = ctx.Now.Plus(Duration.FromMinutes(1));
        var cancel = Task.Run(async () =>
        {
            try
            {
                await using var db = _postgres.CreateDbContext();
                await new BookingCancellationService(db).CancelPendingAsync(ctx.BookingId, later, ct);
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("Confirmed Booking cancellation", StringComparison.Ordinal))
            {
                // Confirmation won the advisory lock; Confirmed cancellation remains R6.
            }
        }, ct);
        var confirm = ConfirmOnNewContextAsync(ctx, later, ct);
        await Task.WhenAll(cancel, confirm);

        await using var db = _postgres.CreateDbContext();
        var booking = await db.Bookings.SingleAsync(x => x.Id == ctx.BookingId, ct);
        var hold = await db.CapacityHolds.SingleAsync(x => x.BookingId == ctx.BookingId, ct);
        Assert.True(booking.Status is BookingStatus.Cancelled or BookingStatus.Confirmed);
        Assert.False(booking.Status == BookingStatus.Confirmed && hold.Status == CapacityHoldStatus.Expired);
        if (booking.Status == BookingStatus.Cancelled)
        {
            Assert.NotEqual(CapacityHoldStatus.Consumed, hold.Status);
            Assert.Equal(
                BookingConfirmationRecoveryReason.CancelledBooking,
                (await db.ConfirmationRecoveryIssues.SingleAsync(x => x.BookingId == ctx.BookingId, ct)).Reason);
        }
    }

    private async Task ConfirmOnNewContextAsync(SeededBooking ctx, Instant now, CancellationToken ct)
    {
        await using var db = _postgres.CreateDbContext();
        await new BookingPaymentConfirmationService(db, SuccessEvidence(ctx))
            .ConfirmIfEligibleAsync(ctx.BookingId, now, ct);
    }

    private async Task AssertRecoveryAsync(
        SeededBooking ctx,
        BookingConfirmationRecoveryReason reason,
        CapacityHoldStatus holdStatus,
        CancellationToken ct)
    {
        await using var db = _postgres.CreateDbContext();
        var booking = await db.Bookings.SingleAsync(x => x.Id == ctx.BookingId, ct);
        var hold = await db.CapacityHolds.SingleAsync(x => x.BookingId == ctx.BookingId, ct);
        var issue = await db.ConfirmationRecoveryIssues.SingleAsync(x => x.BookingId == ctx.BookingId, ct);
        Assert.Equal(BookingStatus.Pending, booking.Status);
        Assert.Equal(holdStatus, hold.Status);
        Assert.Equal(reason, issue.Reason);
        Assert.Equal(ctx.PaymentId, issue.PaymentId);
        Assert.Null(typeof(BookingAggregate).GetMethod("Confirm"));
    }

    private async Task<SeededBooking> SeedEligibleAsync(CancellationToken ct, int holdMinutes = 30)
    {
        await using (var db = _postgres.CreateDbContext())
        {
            await BookingMigrator.MigrateAsync(db, ct);
        }

        var departure = new TourDepartureReference(Guid.CreateVersion7());
        var now = Instant.FromUtc(2026, 8, 18, 17, 0);
        var paymentId = Guid.CreateVersion7();
        BookingId bookingId;
        Guid snapshotId;
        await using (var db = _postgres.CreateDbContext())
        {
            var booking = BookingAggregate.Create(departure, now);
            booking.SetContact(BookingContactSnapshot.Create("Pay User", "pay@example.com"));
            booking.AddPassenger("Pay", "User", TravelerCategory.Adult, 1);
            booking.AcceptQuote(Facts(departure.LogicalId, now), now);
            db.Bookings.Add(booking);
            await db.SaveChangesAsync(ct);
            bookingId = booking.Id;
            snapshotId = booking.MonetarySnapshot!.Id.Value;
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

        return new SeededBooking(bookingId, departure, paymentId, snapshotId, now);
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

    private sealed record SeededBooking(
        BookingId BookingId,
        TourDepartureReference Departure,
        Guid PaymentId,
        Guid SnapshotId,
        Instant Now);

    private sealed class FakePaymentEvidence : IPaymentSuccessEvidenceQuery
    {
        public PaymentSuccessEvidenceRead? Next { get; set; }

        public Task<PaymentSuccessEvidenceRead?> GetByBookingIdAsync(
            Guid bookingId,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(Next);
    }
}
