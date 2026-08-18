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
public sealed class BookingCompensationPersistenceTests
{
    private readonly BookingMigrationLifecycleContainerFixture _postgres;

    public BookingCompensationPersistenceTests(BookingMigrationLifecycleContainerFixture postgres)
    {
        _postgres = postgres;
    }

    [Fact]
    public async Task Recovery_And_Compensation_Outbox_Commit_Atomically()
    {
        var ct = TestContext.Current.CancellationToken;
        var ctx = await SeedExpiredAsync(ct);
        await using (var db = _postgres.CreateDbContext())
        {
            await new BookingPaymentConfirmationService(db, SuccessEvidence(ctx))
                .ConfirmIfEligibleAsync(ctx.BookingId, ctx.Now.Plus(Duration.FromMinutes(2)), ct);
        }

        await using (var db = _postgres.CreateDbContext())
        {
            var issue = await db.ConfirmationRecoveryIssues.SingleAsync(x => x.BookingId == ctx.BookingId, ct);
            var outbox = await db.OutboxMessages.SingleAsync(x => x.Id == ctx.PaymentId, ct);
            Assert.Equal(BookingConfirmationRecoveryReason.ExpiredHold, issue.Reason);
            Assert.Equal(BookingCompensationOutboxBoundary.MessageType, outbox.MessageType);
            Assert.Null(outbox.ProcessedAt);
            Assert.Equal(BookingStatus.Pending, (await db.Bookings.SingleAsync(x => x.Id == ctx.BookingId, ct)).Status);
        }
    }

    [Fact]
    public async Task Rolled_Back_Recovery_Commits_Neither_Issue_Nor_Outbox()
    {
        var ct = TestContext.Current.CancellationToken;
        var ctx = await SeedExpiredAsync(ct);
        await using (var db = _postgres.CreateDbContext())
        {
            await using var tx = await db.Database.BeginTransactionAsync(ct);
            var issue = BookingConfirmationRecoveryIssue.Create(
                ctx.BookingId,
                ctx.PaymentId,
                BookingConfirmationRecoveryReason.ExpiredHold,
                ctx.Now.Plus(Duration.FromMinutes(2)));
            db.ConfirmationRecoveryIssues.Add(issue);
            BookingCompensationOutboxWriter.Enqueue(db, issue, ctx.Now.Plus(Duration.FromMinutes(2)));
            await db.SaveChangesAsync(ct);
            await tx.RollbackAsync(ct);
        }

        await using (var db = _postgres.CreateDbContext())
        {
            Assert.Equal(0, await db.ConfirmationRecoveryIssues.CountAsync(x => x.BookingId == ctx.BookingId, ct));
            Assert.Equal(0, await db.OutboxMessages.CountAsync(x => x.Id == ctx.PaymentId, ct));
        }
    }

    private async Task<SeededBooking> SeedExpiredAsync(CancellationToken ct)
    {
        await using (var db = _postgres.CreateDbContext())
        {
            await BookingMigrator.MigrateAsync(db, ct);
        }

        var departure = new TourDepartureReference(Guid.CreateVersion7());
        var now = Instant.FromUtc(2026, 8, 18, 22, 0);
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
                    now.Plus(Duration.FromMinutes(1)),
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
                    new TravelCore.Money.Money(110m, "USD"),
                    0,
                    "BASE",
                    "Base")
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
}
