using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using TravelCore.Money;
using TravelCore.Modules.Flight.Contracts;
using TravelCore.Modules.Flight.Domain;
using TravelCore.Modules.Flight.Infrastructure;
using TravelCore.Modules.Flight.Infrastructure.Cancellations;
using TravelCore.Modules.Flight.Infrastructure.Services;
using TravelCore.Modules.Payment.Contracts;
using Xunit;
using FlightBookingAggregate = TravelCore.Modules.Flight.Domain.FlightBooking;
using MoneyValue = TravelCore.Money.Money;

namespace TravelCore.Persistence.IntegrationTests;

[Collection(nameof(FlightMigrationLifecycleCollection))]
public sealed class FlightBookingCancellationPersistenceTests
{
    private static readonly Instant T0 = Instant.FromUtc(2026, 8, 18, 12, 0);
    private static readonly Instant Dep = Instant.FromUtc(2026, 9, 1, 6, 0);
    private static readonly Instant Arr = Instant.FromUtc(2026, 9, 1, 10, 0);
    private static readonly Instant Expires = Instant.FromUtc(2026, 8, 18, 14, 0);
    private static readonly Instant Ticketing = Instant.FromUtc(2026, 8, 19, 12, 0);
    private static readonly Instant ReservationExpiry = Instant.FromUtc(2026, 8, 20, 12, 0);
    private readonly FlightMigrationLifecycleContainerFixture _postgres;

    public FlightBookingCancellationPersistenceTests(FlightMigrationLifecycleContainerFixture postgres)
    {
        _postgres = postgres;
    }

    [Fact]
    public async Task Full_Refund_Path_Commits_Cancellation_Outbox_Atomically()
    {
        var ct = TestContext.Current.CancellationToken;
        await using (var migrate = _postgres.CreateDbContext())
        {
            await FlightMigrator.MigrateAsync(migrate, ct);
        }

        FlightBookingId bookingId;
        var source = SucceedingSource();
        await using (var db = _postgres.CreateDbContext())
        {
            var seed = await SeedConfirmedAsync(db, ct);
            bookingId = seed.Booking.Id;
            var result = await CreateService(db, source).RequestAsync(seed.Booking.Id, "cancel-1", ct);
            Assert.Equal(FlightBookingCancellationRequestOutcome.Accepted, result.Outcome);
            Assert.Equal(1, source.QuoteCalls);
            Assert.Equal(1, source.ReverseTicketCalls);
            Assert.Equal(1, source.CancelReservationCalls);
        }

        await using (var db = _postgres.CreateDbContext())
        {
            var booking = await db.FlightBookings.SingleAsync(x => x.Id == bookingId, ct);
            var reservation = await db.FlightSupplierReservations.SingleAsync(x => x.FlightBookingId == bookingId, ct);
            var cancellation = await db.FlightBookingCancellations
                .Include(x => x.Attempts)
                .SingleAsync(x => x.FlightBookingId == bookingId, ct);
            Assert.Equal(FlightBookingStatus.Cancelled, booking.Status);
            Assert.Equal(FlightSupplierReservationStatus.Cancelled, reservation.Status);
            Assert.Equal(FlightBookingCancellationStatus.RefundPending, cancellation.Status);
            Assert.Equal(2, cancellation.Attempts.Count);
            Assert.All(cancellation.Attempts, a => Assert.Equal(FlightSupplierReversalAttemptStatus.Succeeded, a.Status));
            Assert.All(
                await db.FlightTickets.Where(x => x.FlightBookingId == bookingId).ToListAsync(ct),
                t => Assert.Equal(FlightTicketStatus.Voided, t.Status));
            Assert.Equal(
                1,
                await db.OutboxMessages.CountAsync(
                    x => x.Id == cancellation.Id.Value
                        && x.MessageType == FlightBookingCancellationRefundOutboxBoundary.MessageType,
                    ct));
            Assert.Equal(1, await db.FlightBookingCancellationIdempotency.CountAsync(x => x.FlightBookingId == bookingId, ct));
        }
    }

    [Fact]
    public async Task Concurrent_Requests_Create_One_Cancellation_And_Unconfigured_Does_Not_Fabricate()
    {
        var ct = TestContext.Current.CancellationToken;
        await using (var migrate = _postgres.CreateDbContext())
        {
            await FlightMigrator.MigrateAsync(migrate, ct);
        }

        FlightBookingId bookingId;
        await using (var db = _postgres.CreateDbContext())
        {
            var seed = await SeedConfirmedAsync(db, ct);
            bookingId = seed.Booking.Id;
        }

        var source = SucceedingSource();
        await using (var dbA = _postgres.CreateDbContext())
        await using (var dbB = _postgres.CreateDbContext())
        {
            var serviceA = CreateService(dbA, source);
            var serviceB = CreateService(dbB, source);
            var first = serviceA.RequestAsync(bookingId, "key-a", ct);
            var second = serviceB.RequestAsync(bookingId, "key-b", ct);
            await Task.WhenAll(first, second);
            Assert.Equal(FlightBookingCancellationRequestOutcome.Accepted, first.Result.Outcome);
            Assert.Equal(FlightBookingCancellationRequestOutcome.Accepted, second.Result.Outcome);
        }

        await using (var db = _postgres.CreateDbContext())
        {
            Assert.Equal(1, await db.FlightBookingCancellations.CountAsync(x => x.FlightBookingId == bookingId, ct));
            var cancellation = await db.FlightBookingCancellations
                .Include(x => x.Attempts)
                .SingleAsync(x => x.FlightBookingId == bookingId, ct);
            Assert.True(cancellation.Attempts.Count >= 2);
        }

        await using (var noneDb = _postgres.CreateDbContext())
        {
            var other = await SeedConfirmedAsync(noneDb, ct);
            var none = new FlightBookingCancellationService(
                noneDb,
                new FlightCancellationSourceResolver([]),
                new FixedClock(T0));
            var unconfigured = await none.RequestAsync(other.Booking.Id, "cancel-none", ct);
            Assert.Equal(FlightBookingCancellationRequestOutcome.UnconfiguredCancellationSource, unconfigured.Outcome);
            Assert.Equal(0, await noneDb.FlightBookingCancellations.CountAsync(x => x.FlightBookingId == other.Booking.Id, ct));
        }

        await using (var db = _postgres.CreateDbContext())
        {
            var conn = db.Database.GetDbConnection();
            await db.Database.OpenConnectionAsync(ct);
            Assert.Equal(0, await ScalarIntAsync(conn, """
                SELECT COUNT(*)::int
                FROM information_schema.table_constraints tc
                JOIN information_schema.constraint_column_usage ccu
                  ON tc.constraint_schema = ccu.constraint_schema
                 AND tc.constraint_name = ccu.constraint_name
                WHERE tc.table_schema = 'flight'
                  AND tc.constraint_type = 'FOREIGN KEY'
                  AND ccu.table_schema IN ('payment', 'booking', 'hotel_booking');
                """, ct));
        }
    }

    private static FlightBookingCancellationService CreateService(
        FlightDbContext db,
        IFlightCancellationSource source) =>
        new(db, new FlightCancellationSourceResolver([source]), new FixedClock(T0));

    private static FakeFlightCancellationSource SucceedingSource() =>
        new()
        {
            NextQuote = new FlightCancellationQuoteResult(
                FlightCancellationQuoteSourceOutcome.Complete,
                new MoneyValue(0m, CurrencyCode.Parse("IRR")),
                partialRefundRequired: false,
                FlightSupplierReversalKind.TicketVoid),
            NextReverse = new FlightTicketReversalSourceResult(FlightTicketReversalSourceOutcome.Voided),
            NextCancel = new FlightReservationCancelSourceResult(FlightReservationCancelSourceOutcome.Succeeded),
        };

    private static async Task<ConfirmedSeed> SeedConfirmedAsync(
        FlightDbContext db,
        CancellationToken cancellationToken)
    {
        var booking = FlightBookingAggregate.Create(
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
        var irr = CurrencyCode.Parse("IRR");
        var identities = booking.Journeys
            .SelectMany(j => j.Segments.Select(s => new FlightOfferSegmentIdentity(
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
        var passengers = booking.Passengers
            .Select(p => new FlightReservationPassengerFact(p.GivenName, p.FamilyName, p.Category))
            .ToArray();
        var snapshot = FlightOfferSnapshot.Accept(
            booking,
            T0,
            "test-source",
            $"offer-{booking.Id.Value:N}",
            T0.Minus(Duration.FromMinutes(1)),
            Expires,
            new MoneyValue(800_000m, irr),
            new MoneyValue(150_000m, irr),
            new MoneyValue(50_000m, irr),
            new MoneyValue(1_000_000m, irr),
            identities,
            new FlightPassengerCount(1, 0, 0),
            new FlightFareRulesDraft(true, true, Ticketing, new MoneyValue(0m, irr), new MoneyValue(80_000m, irr)));
        var reservation = FlightSupplierReservation.StartPending(booking.Id, "test-source", T0);
        var attempt = reservation.StartAttempt(T0);
        reservation.MarkAttemptInitiated(attempt.Id, T0.Plus(Duration.FromSeconds(1)));
        reservation.ConfirmAttempt(
            attempt.Id,
            T0.Plus(Duration.FromMinutes(1)),
            $"src-res-{booking.Id.Value:N}",
            "ABC123",
            ReservationExpiry,
            identities,
            identities,
            passengers,
            passengers);
        var ticket = FlightTicket.StartPending(booking.Id, booking.Passengers[0].Id, "test-source", T0);
        ticket.MarkIssued($"125-{booking.Id.Value:N}", T0.Plus(Duration.FromMinutes(3)));
        var paymentId = Guid.CreateVersion7();
        var evidence = FlightBookingPaymentEvidence.Record(
            booking.Id,
            paymentId,
            1_000_000m,
            "IRR",
            T0.Plus(Duration.FromMinutes(2)));
        booking.ConfirmFromAuthoritativeReservationPaymentAndTickets(
            reservation,
            evidence,
            [ticket],
            snapshot.Monetary,
            [],
            T0.Plus(Duration.FromMinutes(5)));
        db.FlightBookings.Add(booking);
        db.FlightOfferSnapshots.Add(snapshot);
        db.FlightSupplierReservations.Add(reservation);
        db.FlightTickets.Add(ticket);
        db.FlightBookingPaymentEvidence.Add(evidence);
        await db.SaveChangesAsync(cancellationToken);
        return new ConfirmedSeed(booking, paymentId);
    }

    private static async Task<int> ScalarIntAsync(DbConnection conn, string sql, CancellationToken cancellationToken)
    {
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        var result = await cmd.ExecuteScalarAsync(cancellationToken);
        return Convert.ToInt32(result);
    }

    private sealed record ConfirmedSeed(FlightBookingAggregate Booking, Guid PaymentId);

    private sealed class FakeFlightCancellationSource : IFlightCancellationSource
    {
        public FlightSourceKey Key { get; } = new("test-source");

        public IReadOnlySet<FlightSourceCapability> Capabilities { get; } =
            new HashSet<FlightSourceCapability>
            {
                FlightSourceCapability.CancellationQuote,
                FlightSourceCapability.ReservationCancel,
                FlightSourceCapability.TicketVoid,
                FlightSourceCapability.TicketRefund,
                FlightSourceCapability.CancellationQuery,
            };

        public int QuoteCalls { get; private set; }

        public int CancelReservationCalls { get; private set; }

        public int ReverseTicketCalls { get; private set; }

        public FlightCancellationQuoteResult? NextQuote { get; set; }

        public FlightReservationCancelSourceResult? NextCancel { get; set; }

        public FlightTicketReversalSourceResult? NextReverse { get; set; }

        public Task<FlightCancellationQuoteResult> QuoteCancellationAsync(
            FlightCancellationQuoteRequest request,
            CancellationToken cancellationToken = default)
        {
            QuoteCalls++;
            return Task.FromResult(NextQuote
                ?? new FlightCancellationQuoteResult(FlightCancellationQuoteSourceOutcome.Unknown));
        }

        public Task<FlightReservationCancelSourceResult> CancelReservationAsync(
            FlightReservationCancelRequest request,
            CancellationToken cancellationToken = default)
        {
            CancelReservationCalls++;
            return Task.FromResult(NextCancel
                ?? new FlightReservationCancelSourceResult(FlightReservationCancelSourceOutcome.Unknown));
        }

        public Task<FlightTicketReversalSourceResult> ReverseTicketAsync(
            FlightTicketReversalRequest request,
            CancellationToken cancellationToken = default)
        {
            ReverseTicketCalls++;
            return Task.FromResult(NextReverse
                ?? new FlightTicketReversalSourceResult(FlightTicketReversalSourceOutcome.Unknown));
        }

        public Task<FlightCancellationQueryResult> QueryCancellationStatusAsync(
            FlightCancellationQueryRequest request,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(new FlightCancellationQueryResult(FlightCancellationQueryStatus.PendingUnknown));

        public Task<FlightTicketReversalQueryResult> QueryTicketReversalStatusAsync(
            FlightTicketReversalQueryRequest request,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(new FlightTicketReversalQueryResult(
                request.TicketId,
                FlightTicketReversalQueryStatus.PendingUnknown));
    }

    private sealed class FixedClock(Instant now) : IClock
    {
        public Instant GetCurrentInstant() => now;
    }
}
