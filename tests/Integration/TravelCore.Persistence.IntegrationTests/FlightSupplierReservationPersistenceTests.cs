using Microsoft.EntityFrameworkCore;
using NodaTime;
using TravelCore.Money;
using TravelCore.Modules.Flight.Contracts;
using TravelCore.Modules.Flight.Domain;
using TravelCore.Modules.Flight.Infrastructure;
using TravelCore.Modules.Flight.Infrastructure.Reservations;
using TravelCore.Modules.Flight.Infrastructure.Services;
using Xunit;
using MoneyValue = TravelCore.Money.Money;

namespace TravelCore.Persistence.IntegrationTests;

[Collection(nameof(FlightMigrationLifecycleCollection))]
public sealed class FlightSupplierReservationPersistenceTests
{
    private static readonly Instant T0 = Instant.FromUtc(2026, 8, 18, 12, 0);
    private static readonly Instant Dep = Instant.FromUtc(2026, 9, 1, 6, 0);
    private static readonly Instant Arr = Instant.FromUtc(2026, 9, 1, 10, 0);
    private static readonly Instant ConnDep = Instant.FromUtc(2026, 9, 1, 12, 0);
    private static readonly Instant ConnArr = Instant.FromUtc(2026, 9, 1, 16, 0);
    private static readonly Instant Expires = Instant.FromUtc(2026, 8, 18, 14, 0);
    private static readonly Instant Ticketing = Instant.FromUtc(2026, 8, 19, 12, 0);
    private static readonly Instant ReservationExpiry = Instant.FromUtc(2026, 8, 20, 12, 0);
    private readonly FlightMigrationLifecycleContainerFixture _postgres;

    public FlightSupplierReservationPersistenceTests(FlightMigrationLifecycleContainerFixture postgres)
    {
        _postgres = postgres;
    }

    [Fact]
    public async Task Complete_Reservation_Confirms_Pnr_Without_Payment_Gating()
    {
        var ct = TestContext.Current.CancellationToken;
        await using (var migrate = _postgres.CreateDbContext())
        {
            await FlightMigrator.MigrateAsync(migrate, ct);
        }

        FlightBookingId bookingId;
        var source = new FakeFlightReservationSource();
        await using (var db = _postgres.CreateDbContext())
        {
            var booking = await SeedBookingWithOfferAsync(db, ct);
            bookingId = booking.Id;
            source.NextCreate = Complete(booking);
            var service = CreateService(db, source);
            var reservation = await service.InitiateAsync(booking.Id, "key-1", cancellationToken: ct);
            Assert.Equal(FlightSupplierReservationStatus.Confirmed, reservation.Status);
            Assert.Equal("ABC123", reservation.ReservationLocator);
            Assert.Equal(ReservationExpiry, reservation.ReservationExpiresAt);
            Assert.Equal(1, source.CreateCalls);
            Assert.Equal(0, await db.FlightReconciliationIssues.CountAsync(x => x.FlightBookingId == bookingId, ct));
        }

        await using (var db = _postgres.CreateDbContext())
        {
            var loaded = await db.FlightSupplierReservations
                .Include(x => x.Attempts)
                .SingleAsync(x => x.FlightBookingId == bookingId, ct);
            Assert.Equal(FlightSupplierReservationStatus.Confirmed, loaded.Status);
            Assert.Equal("ABC123", loaded.ReservationLocator);
            Assert.Single(loaded.Attempts);
            Assert.Equal(FlightSupplierReservationAttemptStatus.Confirmed, loaded.Attempts[0].Status);
            var loadedBooking = await db.FlightBookings.SingleAsync(x => x.Id == bookingId, ct);
            Assert.Equal(FlightBookingStatus.Pending, loadedBooking.Status);
            Assert.NotNull(typeof(FlightBooking).GetProperty("Status"));
        }
    }

    [Fact]
    public async Task Timeout_Leaves_Initiated_And_Same_Idempotency_Key_Converges()
    {
        var ct = TestContext.Current.CancellationToken;
        await using (var migrate = _postgres.CreateDbContext())
        {
            await FlightMigrator.MigrateAsync(migrate, ct);
        }

        var timeoutSource = new FakeFlightReservationSource { CreateException = new TimeoutException("network") };
        FlightSupplierReservationId reservationId;
        FlightBookingId bookingId;
        await using (var db = _postgres.CreateDbContext())
        {
            var booking = await SeedBookingWithOfferAsync(db, ct);
            bookingId = booking.Id;
            var service = CreateService(db, timeoutSource);
            var reservation = await service.InitiateAsync(booking.Id, "key-timeout", cancellationToken: ct);
            reservationId = reservation.Id;
            Assert.Equal(FlightSupplierReservationAttemptStatus.Initiated, reservation.Attempts.Single().Status);
            Assert.Equal(FlightSupplierReservationStatus.Pending, reservation.Status);
        }

        await using (var db = _postgres.CreateDbContext())
        {
            var service = CreateService(db, new FakeFlightReservationSource());
            var blocked = await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.InitiateAsync(bookingId, "key-retry", cancellationToken: ct));
            Assert.Contains("unresolved", blocked.Message, StringComparison.OrdinalIgnoreCase);

            var sameKey = await service.InitiateAsync(bookingId, "key-timeout", cancellationToken: ct);
            Assert.Equal(reservationId, sameKey.Id);
            Assert.Equal(FlightSupplierReservationAttemptStatus.Initiated, sameKey.Attempts.Single().Status);
        }
    }

    [Fact]
    public async Task Failed_Allows_Retry_And_Partial_Passenger_Does_Not_Confirm()
    {
        var ct = TestContext.Current.CancellationToken;
        await using (var migrate = _postgres.CreateDbContext())
        {
            await FlightMigrator.MigrateAsync(migrate, ct);
        }

        FlightBookingId bookingId;
        var failed = new FakeFlightReservationSource
        {
            NextCreate = new FlightReservationSourceResult(FlightReservationSourceOutcome.Failed),
        };
        await using (var db = _postgres.CreateDbContext())
        {
            var booking = await SeedBookingWithOfferAsync(db, ct);
            bookingId = booking.Id;
            var service = CreateService(db, failed);
            var reservation = await service.InitiateAsync(booking.Id, "key-fail", cancellationToken: ct);
            Assert.Equal(FlightSupplierReservationAttemptStatus.Failed, reservation.Attempts.Single().Status);
            Assert.Equal(FlightSupplierReservationStatus.Pending, reservation.Status);
        }

        await using (var db = _postgres.CreateDbContext())
        {
            var booking = await db.FlightBookings
                .Include(x => x.Journeys)
                .ThenInclude(x => x.Segments)
                .Include(x => x.Passengers)
                .SingleAsync(x => x.Id == bookingId, ct);
            var retry = new FakeFlightReservationSource { NextCreate = Complete(booking) };
            var service = CreateService(db, retry);
            var reservation = await service.InitiateAsync(bookingId, "key-retry", cancellationToken: ct);
            Assert.Equal(FlightSupplierReservationStatus.Confirmed, reservation.Status);
            Assert.Equal(2, reservation.Attempts.Count);
        }

        await using (var db = _postgres.CreateDbContext())
        {
            var booking = await SeedConnectingBookingAsync(db, ct);
            var partial = new FakeFlightReservationSource
            {
                NextCreate = new FlightReservationSourceResult(
                    FlightReservationSourceOutcome.Partial,
                    $"src-partial-{booking.Id.Value:N}",
                    "PART1",
                    Identities(booking),
                    [Passengers(booking)[0]],
                    Irr(1_000_000m),
                    ReservationExpiry,
                    UniqueOfferRef(booking, "offer-conn")),
            };
            var service = CreateService(db, partial);
            var reservation = await service.InitiateAsync(booking.Id, "key-partial", cancellationToken: ct);
            Assert.Equal(FlightSupplierReservationStatus.Pending, reservation.Status);
            Assert.Equal(FlightSupplierReservationAttemptStatus.Initiated, reservation.Attempts.Single(a => a.IsUnresolved).Status);
            Assert.Contains(
                await db.FlightReconciliationIssues.Where(x => x.FlightBookingId == booking.Id).ToListAsync(ct),
                i => i.Kind == FlightReconciliationIssueKind.PassengerMismatch);
            var snapshot = await db.FlightOfferSnapshots
                .Include(x => x.Monetary)
                .SingleAsync(x => x.FlightBookingId == booking.Id, ct);
            Assert.Equal(1_000_000m, snapshot.Monetary.Total.Amount);
        }
    }

    [Fact]
    public async Task Source_Mismatch_Monetary_Mismatch_And_Unconfigured_Source_Are_Rejected()
    {
        var ct = TestContext.Current.CancellationToken;
        await using (var migrate = _postgres.CreateDbContext())
        {
            await FlightMigrator.MigrateAsync(migrate, ct);
        }

        await using (var db = _postgres.CreateDbContext())
        {
            var booking = await SeedBookingWithOfferAsync(db, ct);
            var source = new FakeFlightReservationSource { NextCreate = Complete(booking, 1_250_000m) };
            var service = CreateService(db, source);
            var reservation = await service.InitiateAsync(booking.Id, "key-mismatch", cancellationToken: ct);
            Assert.Equal(FlightSupplierReservationStatus.Pending, reservation.Status);
            Assert.Contains(
                await db.FlightReconciliationIssues.Where(x => x.FlightBookingId == booking.Id).ToListAsync(ct),
                i => i.Kind == FlightReconciliationIssueKind.MonetaryMismatch);
            var snapshot = await db.FlightOfferSnapshots
                .Include(x => x.Monetary)
                .SingleAsync(x => x.FlightBookingId == booking.Id, ct);
            Assert.Equal(1_000_000m, snapshot.Monetary.Total.Amount);
            Assert.Equal("IRR", snapshot.Monetary.CurrencyCode.Value);
        }

        await using (var db = _postgres.CreateDbContext())
        {
            var booking = await SeedBookingWithOfferAsync(db, ct);
            var source = new FakeFlightReservationSource();
            var service = CreateService(db, source);
            var mismatch = await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.InitiateAsync(
                    booking.Id,
                    "key-source",
                    new FlightSourceKey("other-source"),
                    ct));
            Assert.Contains("SourceKey", mismatch.Message, StringComparison.Ordinal);
            Assert.Equal(0, source.CreateCalls);
        }

        await using (var db = _postgres.CreateDbContext())
        {
            var booking = await SeedBookingWithOfferAsync(db, ct);
            var empty = new FlightReservationSourceResolver([]);
            var service = new FlightSupplierReservationService(db, empty, new FixedClock(T0));
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.InitiateAsync(booking.Id, "key-unconfigured", cancellationToken: ct));
            Assert.Contains("unconfigured", ex.Message, StringComparison.OrdinalIgnoreCase);
            Assert.Equal(0, await db.FlightSupplierReservations.CountAsync(x => x.FlightBookingId == booking.Id, ct));
        }
    }

    [Fact]
    public async Task Recheck_Recovers_Ambiguous_Outcome_And_Expiry_Is_Authoritative()
    {
        var ct = TestContext.Current.CancellationToken;
        await using (var migrate = _postgres.CreateDbContext())
        {
            await FlightMigrator.MigrateAsync(migrate, ct);
        }

        FlightBookingId bookingId;
        FlightSupplierReservationId reservationId;
        await using (var db = _postgres.CreateDbContext())
        {
            var booking = await SeedBookingWithOfferAsync(db, ct, persistOffer: true);
            bookingId = booking.Id;
            var reservation = FlightSupplierReservation.StartPending(booking.Id, "test-source", T0);
            var attempt = reservation.StartAttempt(T0);
            reservation.MarkAttemptInitiated(attempt.Id, T0.Plus(Duration.FromSeconds(1)));
            reservation.RecordSourceCorrelation($"src-crash-{booking.Id.Value:N}", "CRASH1");
            db.FlightSupplierReservations.Add(reservation);
            await db.SaveChangesAsync(ct);
            reservationId = reservation.Id;
        }

        await using (var db = _postgres.CreateDbContext())
        {
            var booking = await db.FlightBookings
                .Include(x => x.Journeys)
                .ThenInclude(x => x.Segments)
                .Include(x => x.Passengers)
                .SingleAsync(x => x.Id == bookingId, ct);
            var source = new FakeFlightReservationSource
            {
                NextQuery = new FlightReservationQueryResult(
                    FlightReservationQueryStatus.Confirmed,
                    $"src-crash-{booking.Id.Value:N}",
                    "ABC123",
                    Identities(booking),
                    Passengers(booking),
                    Irr(1_000_000m),
                    ReservationExpiry,
                    UniqueOfferRef(booking, "offer-res")),
            };
            var service = CreateService(db, source);
            var rechecked = await service.RecheckAsync(reservationId, ct);
            Assert.Equal(FlightSupplierReservationStatus.Confirmed, rechecked.Status);
            Assert.Equal("ABC123", rechecked.ReservationLocator);
        }

        await using (var db = _postgres.CreateDbContext())
        {
            var source = new FakeFlightReservationSource
            {
                NextQuery = new FlightReservationQueryResult(FlightReservationQueryStatus.Expired),
            };
            var service = CreateService(db, source);
            var expired = await service.RecheckAsync(reservationId, ct);
            Assert.Equal(FlightSupplierReservationStatus.Expired, expired.Status);
            Assert.NotNull(expired.ExpiredAt);
        }

        await using (var db = _postgres.CreateDbContext())
        {
            var source = new FakeFlightReservationSource
            {
                NextQuery = new FlightReservationQueryResult(FlightReservationQueryStatus.Confirmed),
            };
            var service = CreateService(db, source);
            var after = await service.RecheckAsync(reservationId, ct);
            Assert.Equal(FlightSupplierReservationStatus.Expired, after.Status);
            Assert.Contains(
                await db.FlightReconciliationIssues.Where(x => x.FlightBookingId == bookingId).ToListAsync(ct),
                i => i.Kind == FlightReconciliationIssueKind.ContradictorySupplierEvidence);
        }
    }

    [Fact]
    public async Task Confirmed_Blocks_Duplicate_And_Unresolved_Attempt_Is_Unique()
    {
        var ct = TestContext.Current.CancellationToken;
        await using (var migrate = _postgres.CreateDbContext())
        {
            await FlightMigrator.MigrateAsync(migrate, ct);
        }

        FlightBookingId bookingId;
        await using (var db = _postgres.CreateDbContext())
        {
            var booking = await SeedBookingWithOfferAsync(db, ct);
            bookingId = booking.Id;
            var source = new FakeFlightReservationSource { NextCreate = Complete(booking) };
            var service = CreateService(db, source);
            var reservation = await service.InitiateAsync(booking.Id, "key-1", cancellationToken: ct);
            Assert.Equal(FlightSupplierReservationStatus.Confirmed, reservation.Status);
            var same = await service.InitiateAsync(booking.Id, "key-1", cancellationToken: ct);
            Assert.Equal(reservation.Id, same.Id);
            var blocked = await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.InitiateAsync(booking.Id, "key-2", cancellationToken: ct));
            Assert.Contains("Confirmed", blocked.Message, StringComparison.Ordinal);
        }

        await using (var db = _postgres.CreateDbContext())
        {
            var booking = await SeedBookingWithOfferAsync(db, ct);
            var reservation = FlightSupplierReservation.StartPending(booking.Id, "test-source", T0);
            reservation.StartAttempt(T0);
            db.FlightSupplierReservations.Add(reservation);
            await db.SaveChangesAsync(ct);

            await using var other = _postgres.CreateDbContext();
            var concurrentAttemptId = Guid.Parse("0198b3e0-0000-7000-8000-00000000f201");
            var unique = await Record.ExceptionAsync(() => other.Database.ExecuteSqlRawAsync(
                """
                INSERT INTO flight.flight_supplier_reservation_attempts
                    (id, flight_supplier_reservation_id, status, created_at)
                VALUES
                    ({0}, {1}, 1, TIMESTAMPTZ '2026-08-18 12:00:00+00');
                """,
                concurrentAttemptId,
                reservation.Id.Value));
            Assert.NotNull(unique);
            Assert.Contains(
                "ux_flight_supplier_reservation_attempts_one_unresolved",
                unique.Message,
                StringComparison.OrdinalIgnoreCase);
        }

        await using (var db = _postgres.CreateDbContext())
        {
            var booking = await SeedBookingWithOfferAsync(db, ct);
            db.FlightSupplierReservations.Add(FlightSupplierReservation.StartPending(booking.Id, "test-source", T0));
            await db.SaveChangesAsync(ct);
            db.FlightSupplierReservations.Add(FlightSupplierReservation.StartPending(booking.Id, "other-source", T0));
            await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync(ct));
        }
    }

    [Fact]
    public async Task NotCreated_On_Confirmed_Persists_Contradiction_Without_Flipping()
    {
        var ct = TestContext.Current.CancellationToken;
        await using (var migrate = _postgres.CreateDbContext())
        {
            await FlightMigrator.MigrateAsync(migrate, ct);
        }

        FlightSupplierReservationId reservationId;
        FlightBookingId bookingId;
        await using (var db = _postgres.CreateDbContext())
        {
            var booking = await SeedBookingWithOfferAsync(db, ct);
            bookingId = booking.Id;
            var source = new FakeFlightReservationSource { NextCreate = Complete(booking) };
            var service = CreateService(db, source);
            var reservation = await service.InitiateAsync(booking.Id, "key-1", cancellationToken: ct);
            reservationId = reservation.Id;
            Assert.Equal(FlightSupplierReservationStatus.Confirmed, reservation.Status);
        }

        await using (var db = _postgres.CreateDbContext())
        {
            var source = new FakeFlightReservationSource
            {
                NotFoundProvesNoReservation = true,
                NextQuery = new FlightReservationQueryResult(FlightReservationQueryStatus.NotCreated),
            };
            var service = CreateService(db, source);
            var rechecked = await service.RecheckAsync(reservationId, ct);
            Assert.Equal(FlightSupplierReservationStatus.Confirmed, rechecked.Status);
            Assert.Contains(
                await db.FlightReconciliationIssues.Where(x => x.FlightBookingId == bookingId).ToListAsync(ct),
                i => i.Kind == FlightReconciliationIssueKind.ContradictorySupplierEvidence);
        }
    }

    private static FlightSupplierReservationService CreateService(
        FlightDbContext db,
        IFlightReservationSource source) =>
        new(db, new FlightReservationSourceResolver([source]), new FixedClock(T0));

    private static async Task<FlightBooking> SeedBookingWithOfferAsync(
        FlightDbContext db,
        CancellationToken cancellationToken,
        bool persistOffer = true)
    {
        var booking = CreateBooking();
        db.FlightBookings.Add(booking);
        if (persistOffer)
        {
            db.FlightOfferSnapshots.Add(AcceptOffer(booking, UniqueOfferRef(booking, "offer-res")));
        }

        await db.SaveChangesAsync(cancellationToken);
        return booking;
    }

    private static async Task<FlightBooking> SeedConnectingBookingAsync(
        FlightDbContext db,
        CancellationToken cancellationToken)
    {
        var booking = FlightBooking.Create(
            FlightTripType.OneWay,
            [
                new FlightJourneySpecification(
                [
                    new FlightSegmentSpecification(
                        new AirportReference("THR"),
                        new AirportReference("IST"),
                        Dep,
                        "Asia/Tehran",
                        Arr,
                        "Europe/Istanbul",
                        new AirlineReference("TK"),
                        null,
                        "TK800"),
                    new FlightSegmentSpecification(
                        new AirportReference("IST"),
                        new AirportReference("LHR"),
                        ConnDep,
                        "Europe/Istanbul",
                        ConnArr,
                        "Europe/London",
                        new AirlineReference("TK"),
                        null,
                        "TK1980"),
                ]),
            ],
            [
                new FlightPassengerSpecification("Ada", "Lovelace", FlightPassengerCategory.Adult),
                new FlightPassengerSpecification("Alan", "Turing", FlightPassengerCategory.Child),
            ]);
        db.FlightBookings.Add(booking);
        db.FlightOfferSnapshots.Add(AcceptOffer(booking, UniqueOfferRef(booking, "offer-conn")));
        await db.SaveChangesAsync(cancellationToken);
        return booking;
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
        var passengers = new FlightPassengerCount(
            booking.Passengers.Count(p => p.Category == FlightPassengerCategory.Adult),
            booking.Passengers.Count(p => p.Category == FlightPassengerCategory.Child),
            booking.Passengers.Count(p => p.Category == FlightPassengerCategory.Infant));
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
            Identities(booking),
            passengers,
            new FlightFareRulesDraft(true, true, Ticketing, Irr(100_000m), Irr(80_000m)));
    }

    private static FlightReservationSourceResult Complete(FlightBooking booking, decimal amount = 1_000_000m) =>
        new(
            FlightReservationSourceOutcome.Complete,
            $"src-res-{booking.Id.Value:N}",
            "ABC123",
            Identities(booking),
            Passengers(booking),
            Irr(amount),
            ReservationExpiry,
            UniqueOfferRef(booking, "offer-res"));

    private static string UniqueOfferRef(FlightBooking booking, string suffix) =>
        $"{suffix}-{booking.Id.Value:N}"[..Math.Min(128, $"{suffix}-{booking.Id.Value:N}".Length)];

    private static IReadOnlyList<FlightOfferSegmentIdentity> Identities(FlightBooking booking) =>
        booking.Journeys
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

    private static IReadOnlyList<FlightReservationPassengerFact> Passengers(FlightBooking booking) =>
        booking.Passengers
            .OrderBy(p => p.Ordinal)
            .Select(p => new FlightReservationPassengerFact(p.GivenName, p.FamilyName, p.Category))
            .ToArray();

    private static MoneyValue Irr(decimal amount) => new(amount, CurrencyCode.Parse("IRR"));

    private sealed class FakeFlightReservationSource : IFlightReservationSource
    {
        public FlightSourceKey Key { get; } = new("test-source");

        public IReadOnlySet<FlightSourceCapability> Capabilities { get; } =
            new HashSet<FlightSourceCapability>
            {
                FlightSourceCapability.ReservationCreate,
                FlightSourceCapability.ReservationQuery,
            };

        public bool NotFoundProvesNoReservation { get; set; }

        public FlightReservationSourceResult? NextCreate { get; set; }

        public Exception? CreateException { get; set; }

        public FlightReservationQueryResult? NextQuery { get; set; }

        public int CreateCalls { get; private set; }

        public Task<FlightReservationSourceResult> CreateReservationAsync(
            FlightReservationRequest request,
            CancellationToken cancellationToken = default)
        {
            CreateCalls++;
            if (CreateException is not null)
            {
                throw CreateException;
            }

            return Task.FromResult(NextCreate
                ?? throw new InvalidOperationException("NextCreate is required."));
        }

        public Task<FlightReservationQueryResult> QueryReservationStatusAsync(
            string sourceReservationReference,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(NextQuery ?? new FlightReservationQueryResult(
                FlightReservationQueryStatus.PendingOrUnknown,
                sourceReservationReference));
    }

    private sealed class FixedClock(Instant now) : IClock
    {
        public Instant GetCurrentInstant() => now;
    }
}
