using Microsoft.EntityFrameworkCore;
using NodaTime;
using TravelCore.Money;
using TravelCore.Modules.Flight.Contracts;
using TravelCore.Modules.Flight.Domain;
using TravelCore.Modules.Flight.Infrastructure;
using TravelCore.Modules.Flight.Infrastructure.Search;
using TravelCore.Modules.Flight.Infrastructure.Services;
using Xunit;
using MoneyValue = TravelCore.Money.Money;

namespace TravelCore.Persistence.IntegrationTests;

[Collection(nameof(FlightMigrationLifecycleCollection))]
public sealed class FlightOfferSnapshotPersistenceTests
{
    private static readonly Instant T0 = Instant.FromUtc(2026, 8, 18, 12, 0);
    private static readonly Instant Dep = Instant.FromUtc(2026, 9, 1, 6, 0);
    private static readonly Instant Arr = Instant.FromUtc(2026, 9, 1, 10, 0);
    private static readonly Instant Expires = Instant.FromUtc(2026, 8, 18, 14, 0);
    private static readonly Instant Ticketing = Instant.FromUtc(2026, 8, 19, 12, 0);
    private readonly FlightMigrationLifecycleContainerFixture _postgres;

    public FlightOfferSnapshotPersistenceTests(FlightMigrationLifecycleContainerFixture postgres)
    {
        _postgres = postgres;
    }

    [Fact]
    public async Task Offer_Money_FareRules_Baggage_And_Provenance_RoundTrip_Without_Float_Or_Peer_Fk()
    {
        var ct = TestContext.Current.CancellationToken;
        await using (var migrate = _postgres.CreateDbContext())
        {
            await FlightMigrator.MigrateAsync(migrate, ct);
        }

        FlightOfferSnapshotId snapshotId;
        FlightBookingId bookingId;

        await using (var db = _postgres.CreateDbContext())
        {
            var booking = CreateBooking();
            bookingId = booking.Id;
            db.FlightBookings.Add(booking);
            var snapshot = AcceptOffer(booking, "offer-persist");
            db.FlightOfferSnapshots.Add(snapshot);
            snapshotId = snapshot.Id;
            await db.SaveChangesAsync(ct);
        }

        await using (var db = _postgres.CreateDbContext())
        {
            var loaded = await db.FlightOfferSnapshots
                .Include(x => x.Monetary)
                .ThenInclude(x => x.CategoryFares)
                .Include(x => x.FareRules)
                .ThenInclude(x => x.Baggage)
                .SingleAsync(x => x.Id == snapshotId, ct);

            Assert.Equal(bookingId, loaded.FlightBookingId);
            Assert.Equal("test-source", loaded.SourceKey);
            Assert.Equal("offer-persist", loaded.SourceOfferReference);
            Assert.Equal(T0.Minus(Duration.FromMinutes(1)), loaded.QuotedAt);
            Assert.Equal(Expires, loaded.OfferExpiresAt);
            Assert.Equal(1_000_000m, loaded.Monetary.Total.Amount);
            Assert.Equal(800_000m, loaded.Monetary.BaseFare.Amount);
            Assert.Equal("IRR", loaded.Monetary.CurrencyCode.Value);
            Assert.Equal(typeof(decimal), loaded.Monetary.Total.Amount.GetType());
            Assert.Single(loaded.Monetary.CategoryFares);
            Assert.Equal(FlightPassengerCategory.Adult, loaded.Monetary.CategoryFares[0].Category);
            Assert.True(loaded.FareRules.Refundable);
            Assert.Equal(Ticketing, loaded.FareRules.TicketingDeadline);
            Assert.NotEqual(loaded.OfferExpiresAt, loaded.FareRules.TicketingDeadline);
            Assert.Equal(100_000m, loaded.FareRules.CancelPenalty!.Amount);
            Assert.Single(loaded.FareRules.Baggage);
            Assert.Equal(23m, loaded.FareRules.Baggage[0].Weight);
            Assert.Equal("KG", loaded.FareRules.Baggage[0].Unit);
            Assert.Equal("Y", loaded.BookingClass);

            var conn = db.Database.GetDbConnection();
            await db.Database.OpenConnectionAsync(ct);
            await using var typeCmd = conn.CreateCommand();
            typeCmd.CommandText = """
                SELECT COUNT(*)::int
                FROM information_schema.columns
                WHERE table_schema = 'flight'
                  AND table_name IN (
                        'flight_offer_snapshots',
                        'flight_booking_monetary_snapshots',
                        'flight_passenger_category_fare_snapshots',
                        'flight_fare_rule_snapshots',
                        'flight_baggage_allowance_snapshots',
                        'flight_offer_idempotency')
                  AND data_type IN ('double precision', 'real');
                """;
            Assert.Equal(0, Convert.ToInt32(await typeCmd.ExecuteScalarAsync(ct)));

            await using var uniqueCmd = conn.CreateCommand();
            uniqueCmd.CommandText = """
                SELECT COUNT(*)::int
                FROM pg_indexes
                WHERE schemaname = 'flight'
                  AND indexname IN (
                        'ux_flight_offer_snapshots_flight_booking_id',
                        'ux_flight_offer_snapshots_source_offer',
                        'ux_flight_booking_monetary_snapshots_flight_booking_id');
                """;
            Assert.Equal(3, Convert.ToInt32(await uniqueCmd.ExecuteScalarAsync(ct)));

            await using var fkCmd = conn.CreateCommand();
            fkCmd.CommandText = """
                SELECT COUNT(*)::int
                FROM information_schema.table_constraints tc
                JOIN information_schema.constraint_column_usage ccu
                  ON tc.constraint_schema = ccu.constraint_schema
                 AND tc.constraint_name = ccu.constraint_name
                WHERE tc.table_schema = 'flight'
                  AND tc.constraint_type = 'FOREIGN KEY'
                  AND ccu.table_schema IN (
                        'place', 'booking', 'payment', 'pricing', 'tour',
                        'party', 'identity', 'agency_marketplace', 'search', 'hotel_booking');
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
            await FlightMigrator.MigrateAsync(migrate, ct);
        }

        await using var db = _postgres.CreateDbContext();
        var booking = CreateBooking();
        db.FlightBookings.Add(booking);
        db.FlightOfferSnapshots.Add(AcceptOffer(booking, "offer-a"));
        await db.SaveChangesAsync(ct);

        db.FlightOfferSnapshots.Add(AcceptOffer(booking, "offer-b"));
        await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync(ct));
    }

    [Fact]
    public async Task Same_Offer_Acceptance_Is_Idempotent_And_Unconfigured_Source_Does_Not_Fabricate()
    {
        var ct = TestContext.Current.CancellationToken;
        await using (var migrate = _postgres.CreateDbContext())
        {
            await FlightMigrator.MigrateAsync(migrate, ct);
        }

        FlightBookingId bookingId;
        await using (var db = _postgres.CreateDbContext())
        {
            var booking = CreateBooking();
            bookingId = booking.Id;
            db.FlightBookings.Add(booking);
            var snapshot = AcceptOffer(booking, "offer-idemp");
            db.FlightOfferSnapshots.Add(snapshot);
            db.FlightOfferIdempotency.Add(
                new FlightOfferIdempotencyRecord(booking.Id, "key-1", snapshot.Id, T0));
            await db.SaveChangesAsync(ct);
        }

        await using (var db = _postgres.CreateDbContext())
        {
            var emptyResolver = new FlightOfferSourceResolver([]);
            var service = new FlightOfferAcceptanceService(db, emptyResolver, new FixedClock(T0));
            var loaded = await service.AcceptAsync(bookingId, "key-1", cancellationToken: ct);
            Assert.Equal("offer-idemp", loaded.SourceOfferReference);
            Assert.Equal(1_000_000m, loaded.Monetary.Total.Amount);

            var unconfigured = await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.AcceptAsync(bookingId, "key-2", cancellationToken: ct));
            Assert.Contains("unconfigured", unconfigured.Message, StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public async Task Configured_Source_Accepts_And_Timeout_Does_Not_Commit()
    {
        var ct = TestContext.Current.CancellationToken;
        await using (var migrate = _postgres.CreateDbContext())
        {
            await FlightMigrator.MigrateAsync(migrate, ct);
        }

        FlightBookingId bookingId;
        await using (var db = _postgres.CreateDbContext())
        {
            var booking = CreateBooking();
            bookingId = booking.Id;
            db.FlightBookings.Add(booking);
            await db.SaveChangesAsync(ct);
        }

        await using (var db = _postgres.CreateDbContext())
        {
            var booking = await db.FlightBookings
                .Include(x => x.Journeys)
                .ThenInclude(x => x.Segments)
                .Include(x => x.Passengers)
                .SingleAsync(x => x.Id == bookingId, ct);
            var source = new FakeOfferSource(booking);
            var service = new FlightOfferAcceptanceService(
                db,
                new FlightOfferSourceResolver([source]),
                new FixedClock(T0));
            var accepted = await service.AcceptAsync(bookingId, "key-live", cancellationToken: ct);
            Assert.Equal("offer-live", accepted.SourceOfferReference);
            Assert.Equal(1_000_000m, accepted.Monetary.Total.Amount);
        }

        await using (var db = _postgres.CreateDbContext())
        {
            var timeoutSource = new TimeoutOfferSource();
            var service = new FlightOfferAcceptanceService(
                db,
                new FlightOfferSourceResolver([timeoutSource]),
                new FixedClock(T0));
            var timeout = await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.AcceptAsync(bookingId, "key-timeout", cancellationToken: ct));
            Assert.Contains("Unknown", timeout.Message, StringComparison.Ordinal);
            Assert.Equal(1, await db.FlightOfferSnapshots.CountAsync(x => x.FlightBookingId == bookingId, ct));
        }
    }

    private static FlightBooking CreateBooking() =>
        FlightBooking.Create(
            FlightTripType.OneWay,
            [
                new FlightJourneySpecification(
                [
                    new FlightSegmentSpecification(
                        new AirportReference("THR"),
                        new AirportReference("LHR"),
                        Dep,
                        "Asia/Tehran",
                        Arr,
                        "Europe/London",
                        new AirlineReference("TK"),
                        null,
                        "TK800"),
                ]),
            ],
            [new FlightPassengerSpecification("Ada", "Lovelace", FlightPassengerCategory.Adult)]);

    private static FlightOfferSnapshot AcceptOffer(FlightBooking booking, string sourceOfferReference)
    {
        var identities = booking.Journeys
            .OrderBy(j => j.Ordinal)
            .SelectMany(j => j.Segments
                .OrderBy(s => s.Ordinal)
                .Select(s => new FlightOfferSegmentIdentity(
                    j.Ordinal,
                    s.Ordinal,
                    s.Origin,
                    s.Destination,
                    s.DepartureAt,
                    s.ArrivalAt,
                    s.MarketingCarrier,
                    s.OperatingCarrier,
                    s.FlightNumber)))
            .ToArray();
        var passengers = new FlightPassengerCount(1);
        return FlightOfferSnapshot.Accept(
            booking,
            T0,
            "test-source",
            sourceOfferReference,
            T0.Minus(Duration.FromMinutes(1)),
            Expires,
            Irr(800_000m),
            Irr(150_000m),
            Irr(50_000m),
            Irr(1_000_000m),
            identities,
            passengers,
            new FlightFareRulesDraft(true, true, Ticketing, Irr(100_000m), Irr(80_000m)),
            categoryFares: [new FlightPassengerCategoryFareLine(FlightPassengerCategory.Adult, 1, Irr(1_000_000m))],
            baggage: [new FlightBaggageAllowanceDraft(1, 23m, "KG", "CHECKED", FlightPassengerCategory.Adult)],
            cabin: "Economy",
            bookingClass: "Y",
            fareBasis: "YOW",
            fareFamily: "ECO");
    }

    private static MoneyValue Irr(decimal amount) => new(amount, CurrencyCode.Parse("IRR"));

    private sealed class FixedClock(Instant now) : IClock
    {
        public Instant GetCurrentInstant() => now;
    }

    private sealed class FakeOfferSource(FlightBooking booking) : IFlightOfferSource
    {
        public FlightSourceKey Key { get; } = new("test-source");

        public IReadOnlySet<FlightSourceCapability> Capabilities { get; } =
            new HashSet<FlightSourceCapability> { FlightSourceCapability.OfferRevalidation };

        public Task<FlightOfferSourceResult> GetOfferAsync(
            FlightOfferRequest request,
            CancellationToken cancellationToken = default)
        {
            var identities = booking.Journeys
                .OrderBy(j => j.Ordinal)
                .SelectMany(j => j.Segments
                    .OrderBy(s => s.Ordinal)
                    .Select(s => new FlightOfferSegmentIdentity(
                        j.Ordinal,
                        s.Ordinal,
                        s.Origin,
                        s.Destination,
                        s.DepartureAt,
                        s.ArrivalAt,
                        s.MarketingCarrier,
                        s.OperatingCarrier,
                        s.FlightNumber)))
                .ToArray();
            return Task.FromResult(FlightOfferSourceResult.Available(
                Key,
                "offer-live",
                T0.Minus(Duration.FromMinutes(1)),
                Expires,
                Irr(800_000m),
                Irr(150_000m),
                Irr(50_000m),
                Irr(1_000_000m),
                identities,
                new FlightPassengerCount(1),
                new FlightFareRulesFact(true, true, Ticketing, Irr(100_000m), Irr(80_000m)),
                T0,
                cabin: "Economy",
                bookingClass: "Y"));
        }
    }

    private sealed class TimeoutOfferSource : IFlightOfferSource
    {
        public FlightSourceKey Key { get; } = new("test-source");

        public IReadOnlySet<FlightSourceCapability> Capabilities { get; } =
            new HashSet<FlightSourceCapability> { FlightSourceCapability.OfferRevalidation };

        public Task<FlightOfferSourceResult> GetOfferAsync(
            FlightOfferRequest request,
            CancellationToken cancellationToken = default) =>
            throw new OperationCanceledException();
    }
}
