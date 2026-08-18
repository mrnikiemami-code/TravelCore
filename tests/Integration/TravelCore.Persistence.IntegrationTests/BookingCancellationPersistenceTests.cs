using Microsoft.EntityFrameworkCore;
using NodaTime;
using TravelCore.Modules.Booking.Contracts;
using TravelCore.Modules.Booking.Domain;
using TravelCore.Modules.Booking.Infrastructure;
using TravelCore.Modules.Booking.Infrastructure.Services;
using Xunit;
using BookingAggregate = TravelCore.Modules.Booking.Domain.Booking;

namespace TravelCore.Persistence.IntegrationTests;

[Collection(nameof(BookingMigrationLifecycleCollection))]
public sealed class BookingCancellationPersistenceTests
{
    private readonly BookingMigrationLifecycleContainerFixture _postgres;

    public BookingCancellationPersistenceTests(BookingMigrationLifecycleContainerFixture postgres)
    {
        _postgres = postgres;
    }

    [Fact]
    public async Task Pending_Cancel_Without_Hold_Succeeds_And_Keeps_People_And_Money()
    {
        var ct = TestContext.Current.CancellationToken;
        await using (var db = _postgres.CreateDbContext())
        {
            await BookingMigrator.MigrateAsync(db, ct);
        }

        var departure = new TourDepartureReference(Guid.CreateVersion7());
        var now = Instant.FromUtc(2026, 8, 18, 16, 0);
        BookingId id;
        await using (var db = _postgres.CreateDbContext())
        {
            var booking = BookingAggregate.Create(departure, now);
            booking.SetContact(BookingContactSnapshot.Create("S", "s@example.com"));
            booking.AddPassenger("A", "One", TravelerCategory.Adult, null);
            booking.AcceptQuote(QuoteFacts(departure.LogicalId, now), now);
            db.Bookings.Add(booking);
            await db.SaveChangesAsync(ct);
            id = booking.Id;
        }

        await using (var db = _postgres.CreateDbContext())
        {
            await new BookingCancellationService(db).CancelPendingAsync(id, now.Plus(Duration.FromMinutes(1)), ct);
        }

        await using (var db = _postgres.CreateDbContext())
        {
            var loaded = await db.Bookings
                .Include(x => x.Passengers)
                .Include(x => x.MonetarySnapshot)
                .ThenInclude(x => x!.Components)
                .SingleAsync(x => x.Id == id, ct);
            Assert.Equal(BookingStatus.Cancelled, loaded.Status);
            Assert.Equal("S", loaded.Contact!.DisplayName);
            Assert.Equal(1, loaded.PassengerCount);
            Assert.NotNull(loaded.MonetarySnapshot);
        }
    }

    [Fact]
    public async Task Pending_Cancel_Releases_Active_Hold_Once_Even_When_Retried_Or_Concurrent()
    {
        var ct = TestContext.Current.CancellationToken;
        await using (var db = _postgres.CreateDbContext())
        {
            await BookingMigrator.MigrateAsync(db, ct);
        }

        var departure = new TourDepartureReference(Guid.CreateVersion7());
        var now = Instant.FromUtc(2026, 8, 18, 16, 10);
        var expires = now.Plus(Duration.FromMinutes(5));
        BookingId id;
        await using (var db = _postgres.CreateDbContext())
        {
            var booking = BookingAggregate.Create(departure, now);
            db.Bookings.Add(booking);
            await db.SaveChangesAsync(ct);
            id = booking.Id;
        }

        await using (var db = _postgres.CreateDbContext())
        {
            await new BookingCapacityService(db).HoldAsync(
                new HoldCapacityCommand(id, 1, 1, expires, now, "cancel-hold-" + id.Value),
                ct);
        }

        var first = CancelOnNewContextAsync(id, now.Plus(Duration.FromMinutes(1)), ct);
        var second = CancelOnNewContextAsync(id, now.Plus(Duration.FromMinutes(2)), ct);
        await Task.WhenAll(first, second);

        await using (var db = _postgres.CreateDbContext())
        {
            await new BookingCancellationService(db).CancelPendingAsync(id, now.Plus(Duration.FromMinutes(3)), ct);
            var booking = await db.Bookings.SingleAsync(x => x.Id == id, ct);
            var hold = await db.CapacityHolds.SingleAsync(x => x.BookingId == id, ct);
            var account = await db.DepartureCapacityAccounts.SingleAsync(x => x.TourDeparture == departure, ct);
            Assert.Equal(BookingStatus.Cancelled, booking.Status);
            Assert.Equal(CapacityHoldStatus.Released, hold.Status);
            Assert.Equal(0, account.EffectiveSeats);
        }
    }

    [Fact]
    public async Task Consumed_Hold_Is_Not_Released_On_Pending_Cancel()
    {
        var ct = TestContext.Current.CancellationToken;
        await using (var db = _postgres.CreateDbContext())
        {
            await BookingMigrator.MigrateAsync(db, ct);
        }

        var departure = new TourDepartureReference(Guid.CreateVersion7());
        var now = Instant.FromUtc(2026, 8, 18, 16, 20);
        var expires = now.Plus(Duration.FromMinutes(5));
        BookingId id;
        await using (var db = _postgres.CreateDbContext())
        {
            var booking = BookingAggregate.Create(departure, now);
            db.Bookings.Add(booking);
            await db.SaveChangesAsync(ct);
            id = booking.Id;
        }

        CapacityHoldId holdId;
        await using (var db = _postgres.CreateDbContext())
        {
            var hold = await new BookingCapacityService(db).HoldAsync(
                new HoldCapacityCommand(id, 1, 1, expires, now, "cancel-consumed-" + id.Value),
                ct);
            holdId = hold.Id;
        }

        await using (var db = _postgres.CreateDbContext())
        {
            await new BookingCapacityService(db).ConsumeAsync(holdId, now.Plus(Duration.FromMinutes(1)), ct);
        }

        await using (var db = _postgres.CreateDbContext())
        {
            await new BookingCancellationService(db).CancelPendingAsync(id, now.Plus(Duration.FromMinutes(2)), ct);
        }

        await using (var db = _postgres.CreateDbContext())
        {
            var hold = await db.CapacityHolds.SingleAsync(x => x.Id == holdId, ct);
            var account = await db.DepartureCapacityAccounts.SingleAsync(x => x.TourDeparture == departure, ct);
            Assert.Equal(CapacityHoldStatus.Consumed, hold.Status);
            Assert.Equal(1, account.EffectiveSeats);
            Assert.Equal(BookingStatus.Cancelled, (await db.Bookings.SingleAsync(x => x.Id == id, ct)).Status);
        }
    }

    private Task CancelOnNewContextAsync(BookingId id, Instant now, CancellationToken ct)
    {
        return Task.Run(async () =>
        {
            await using var db = _postgres.CreateDbContext();
            await new BookingCancellationService(db).CancelPendingAsync(id, now, ct);
        }, ct);
    }

    private static AuthoritativeQuoteFacts QuoteFacts(Guid departureId, Instant now) =>
        AuthoritativeQuoteFacts.Create(
            PricingQuoteReference.From(Guid.CreateVersion7()),
            Guid.CreateVersion7(),
            BookingOwnershipBoundary.InitialTarget,
            departureId,
            now.Minus(Duration.FromMinutes(5)),
            now.Plus(Duration.FromHours(2)),
            [
                new AuthoritativeQuoteComponentFact(
                    BookingMonetaryComponentKind.Base,
                    new TravelCore.Money.Money(20m, "IRR"),
                    0,
                    "BASE",
                    "Base")
            ]);
}
