using Microsoft.EntityFrameworkCore;
using NodaTime;
using TravelCore.Money;
using TravelCore.Modules.HotelBooking.Contracts;
using TravelCore.Modules.HotelBooking.Domain;
using TravelCore.Modules.HotelBooking.Infrastructure;
using TravelCore.Modules.HotelBooking.Infrastructure.Rates;
using TravelCore.Modules.HotelBooking.Infrastructure.Services;
using Xunit;
using MoneyValue = TravelCore.Money.Money;
using Stay = TravelCore.Modules.HotelBooking.Domain.HotelBooking;

namespace TravelCore.Persistence.IntegrationTests;

[Collection(nameof(HotelBookingMigrationLifecycleCollection))]
public sealed class HotelRateOfferSnapshotPersistenceTests
{
    private static readonly Instant T0 = Instant.FromUtc(2026, 8, 18, 12, 0);
    private readonly HotelBookingMigrationLifecycleContainerFixture _postgres;

    public HotelRateOfferSnapshotPersistenceTests(HotelBookingMigrationLifecycleContainerFixture postgres)
    {
        _postgres = postgres;
    }

    [Fact]
    public async Task Offer_Rooms_Money_And_Cancellation_RoundTrip_Without_Float_Or_Peer_Fk()
    {
        var ct = TestContext.Current.CancellationToken;
        await using (var migrate = _postgres.CreateDbContext())
        {
            await HotelBookingMigrator.MigrateAsync(migrate, ct);
        }

        HotelRateOfferSnapshotId snapshotId;
        HotelBookingId bookingId;

        await using (var db = _postgres.CreateDbContext())
        {
            var booking = CreateBooking();
            bookingId = booking.Id;
            db.HotelBookings.Add(booking);
            var snapshot = AcceptOffer(booking, "offer-persist", 1_000_000m);
            db.HotelRateOfferSnapshots.Add(snapshot);
            snapshotId = snapshot.Id;
            await db.SaveChangesAsync(ct);
        }

        await using (var db = _postgres.CreateDbContext())
        {
            var loaded = await db.HotelRateOfferSnapshots
                .Include(x => x.Rooms)
                .Include(x => x.Monetary)
                .ThenInclude(x => x.Charges)
                .Include(x => x.CancellationPolicy)
                .ThenInclude(x => x.Rules)
                .SingleAsync(x => x.Id == snapshotId, ct);

            Assert.Equal(bookingId, loaded.HotelBookingId);
            Assert.Equal(2, loaded.Rooms.Count);
            Assert.Equal(1_000_000m, loaded.Monetary.Total.Amount);
            Assert.Equal("IRR", loaded.Monetary.CurrencyCode.Value);
            Assert.Equal(typeof(decimal), loaded.Monetary.Total.Amount.GetType());
            Assert.Equal(2, loaded.CancellationPolicy.Rules.Count);
            Assert.Equal("Asia/Tehran", loaded.CancellationPolicy.PropertyTimeZoneId);
            Assert.Equal("test-source", loaded.SourceKey);
            Assert.Equal("offer-persist", loaded.SourceOfferReference);
            Assert.Single(loaded.Monetary.Charges);
            Assert.Equal("TAX", loaded.Monetary.Charges[0].Code);

            var conn = db.Database.GetDbConnection();
            await db.Database.OpenConnectionAsync(ct);
            await using var typeCmd = conn.CreateCommand();
            typeCmd.CommandText = """
                SELECT COUNT(*)::int
                FROM information_schema.columns
                WHERE table_schema = 'hotel_booking'
                  AND table_name IN (
                        'hotel_rate_offer_snapshots',
                        'hotel_room_rate_snapshots',
                        'hotel_booking_monetary_snapshots',
                        'hotel_charge_component_snapshots',
                        'hotel_cancellation_policy_snapshots',
                        'hotel_cancellation_penalty_rules')
                  AND data_type IN ('double precision', 'real');
                """;
            Assert.Equal(0, Convert.ToInt32(await typeCmd.ExecuteScalarAsync(ct)));

            await using var fkCmd = conn.CreateCommand();
            fkCmd.CommandText = """
                SELECT COUNT(*)::int
                FROM information_schema.table_constraints tc
                JOIN information_schema.constraint_column_usage ccu
                  ON tc.constraint_schema = ccu.constraint_schema
                 AND tc.constraint_name = ccu.constraint_name
                WHERE tc.table_schema = 'hotel_booking'
                  AND tc.constraint_type = 'FOREIGN KEY'
                  AND ccu.table_schema IN (
                        'place', 'booking', 'payment', 'pricing', 'tour',
                        'party', 'identity', 'agency_marketplace', 'search');
                """;
            Assert.Equal(0, Convert.ToInt32(await fkCmd.ExecuteScalarAsync(ct)));
        }
    }

    [Fact]
    public async Task Unique_Index_Allows_Only_One_Accepted_Offer_Per_Booking()
    {
        var ct = TestContext.Current.CancellationToken;
        await using (var migrate = _postgres.CreateDbContext())
        {
            await HotelBookingMigrator.MigrateAsync(migrate, ct);
        }

        await using var db = _postgres.CreateDbContext();
        var booking = CreateBooking();
        db.HotelBookings.Add(booking);
        db.HotelRateOfferSnapshots.Add(AcceptOffer(booking, "offer-a", 1_000_000m));
        await db.SaveChangesAsync(ct);

        db.HotelRateOfferSnapshots.Add(AcceptOffer(booking, "offer-b", 1_000_000m));
        await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync(ct));
    }

    [Fact]
    public async Task Same_Offer_Acceptance_Is_Idempotent_And_Unconfigured_Source_Does_Not_Fabricate()
    {
        var ct = TestContext.Current.CancellationToken;
        await using (var migrate = _postgres.CreateDbContext())
        {
            await HotelBookingMigrator.MigrateAsync(migrate, ct);
        }

        HotelBookingId bookingId;
        await using (var db = _postgres.CreateDbContext())
        {
            var booking = CreateBooking();
            bookingId = booking.Id;
            db.HotelBookings.Add(booking);
            var snapshot = AcceptOffer(booking, "offer-idemp", 1_000_000m);
            db.HotelRateOfferSnapshots.Add(snapshot);
            db.HotelRateOfferIdempotency.Add(
                new HotelRateOfferIdempotencyRecord(booking.Id, "key-1", snapshot.Id, T0));
            await db.SaveChangesAsync(ct);
        }

        await using (var db = _postgres.CreateDbContext())
        {
            var emptyResolver = new HotelRateOfferSourceResolver([]);
            var service = new HotelRateOfferAcceptanceService(db, emptyResolver, new FixedClock(T0));
            var loaded = await service.AcceptAsync(bookingId, "key-1", cancellationToken: ct);
            Assert.Equal("offer-idemp", loaded.SourceOfferReference);
            Assert.Equal(1_000_000m, loaded.Monetary.Total.Amount);

            var unconfigured = await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.AcceptAsync(bookingId, "key-2", cancellationToken: ct));
            Assert.Contains("unconfigured", unconfigured.Message, StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public async Task Expired_Offer_Cannot_Be_Committed()
    {
        var ct = TestContext.Current.CancellationToken;
        await using (var migrate = _postgres.CreateDbContext())
        {
            await HotelBookingMigrator.MigrateAsync(migrate, ct);
        }

        await using var db = _postgres.CreateDbContext();
        var booking = CreateBooking();
        db.HotelBookings.Add(booking);
        await db.SaveChangesAsync(ct);

        Assert.Throws<InvalidOperationException>(() =>
            HotelRateOfferSnapshot.Accept(
                booking,
                T0,
                booking.Place,
                booking.CheckInDate,
                booking.CheckOutDate,
                "test-source",
                "expired-offer",
                T0.Minus(Duration.FromMinutes(10)),
                T0.Minus(Duration.FromSeconds(1)),
                new MoneyValue(1_000_000m, CurrencyCode.Parse("IRR")),
                booking.Rooms.Select(r => new HotelRoomRateLine(r.Id, new MoneyValue(500_000m, CurrencyCode.Parse("IRR")))).ToArray(),
                [new HotelCancellationPenaltyRuleDraft(T0, null, new MoneyValue(1_000_000m, CurrencyCode.Parse("IRR")))]));

        Assert.Equal(0, await db.HotelRateOfferSnapshots.CountAsync(ct));
    }

    private static Stay CreateBooking() =>
        Stay.Create(
            new HotelPlaceReference(Guid.Parse("0198b3e0-0000-7000-8000-000000000021")),
            new LocalDate(2026, 8, 18),
            new LocalDate(2026, 8, 20),
            HotelBookingContactSnapshot.Create(email: "lead@example.com"),
            [
                new RoomReservationSpecification(
                [
                    new HotelBookingGuestSpecification("Ada", "Lovelace", HotelGuestCategory.Adult, true),
                ]),
                new RoomReservationSpecification(
                [
                    new HotelBookingGuestSpecification("Alan", "Turing", HotelGuestCategory.Adult, false),
                ]),
            ]);

    private static HotelRateOfferSnapshot AcceptOffer(Stay booking, string offerRef, decimal total)
    {
        var irr = CurrencyCode.Parse("IRR");
        var rooms = booking.Rooms.OrderBy(r => r.Ordinal).ToArray();
        return HotelRateOfferSnapshot.Accept(
            booking,
            T0,
            booking.Place,
            booking.CheckInDate,
            booking.CheckOutDate,
            "test-source",
            offerRef,
            T0.Minus(Duration.FromMinutes(1)),
            T0.Plus(Duration.FromHours(2)),
            new MoneyValue(total, irr),
            [
                new HotelRoomRateLine(rooms[0].Id, new MoneyValue(400_000m, irr), "sel-1", "rate-1", "BB"),
                new HotelRoomRateLine(rooms[1].Id, new MoneyValue(600_000m, irr), "sel-2", "rate-2", "BB"),
            ],
            [
                new HotelCancellationPenaltyRuleDraft(T0, T0.Plus(Duration.FromDays(1)), new MoneyValue(0m, irr)),
                new HotelCancellationPenaltyRuleDraft(T0.Plus(Duration.FromDays(1)), null, new MoneyValue(total, irr)),
            ],
            charges:
            [
                new HotelChargeComponentLine("TAX", new MoneyValue(90_000m, irr)),
            ],
            propertyTimeZoneId: "Asia/Tehran");
    }

    private sealed class FixedClock(Instant now) : IClock
    {
        public Instant GetCurrentInstant() => now;
    }
}
