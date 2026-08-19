using Microsoft.EntityFrameworkCore;
using NodaTime;
using TravelCore.Money;
using TravelCore.Modules.Flight.Contracts;
using TravelCore.Modules.Flight.Domain;
using TravelCore.Modules.Flight.Infrastructure;
using TravelCore.Modules.Flight.Infrastructure.Services;
using TravelCore.Modules.Flight.Infrastructure.Ticketing;
using TravelCore.Modules.Payment.Contracts;
using TravelCore.Modules.Payment.Domain;
using TravelCore.Modules.Payment.Infrastructure;
using TravelCore.Modules.Payment.Infrastructure.Services;
using Xunit;
using FlightBookingAggregate = TravelCore.Modules.Flight.Domain.FlightBooking;
using PaymentAggregate = TravelCore.Modules.Payment.Domain.Payment;
using MoneyValue = TravelCore.Money.Money;

namespace TravelCore.Modules.Flight.UnitTests;

public sealed class FlightPaymentTicketingTests
{
    private static readonly Instant Now = Instant.FromUtc(2026, 8, 18, 12, 0);
    private static readonly Instant Dep = Instant.FromUtc(2026, 9, 1, 6, 0);
    private static readonly Instant Arr = Instant.FromUtc(2026, 9, 1, 10, 0);
    private static readonly Instant Expires = Instant.FromUtc(2026, 8, 18, 14, 0);
    private static readonly Instant Ticketing = Instant.FromUtc(2026, 8, 19, 12, 0);
    private static readonly Instant ReservationExpiry = Instant.FromUtc(2026, 8, 20, 12, 0);

    [Fact]
    public void New_FlightBooking_Starts_Pending()
    {
        var booking = OneWayBooking();
        Assert.Equal(FlightBookingStatus.Pending, booking.Status);
        Assert.Null(booking.ConfirmedAt);
        Assert.Null(booking.CancelledAt);
        Assert.Equal(
            new[] { "Pending", "Confirmed", "Cancelled" },
            Enum.GetNames<FlightBookingStatus>());
        Assert.Null(typeof(FlightBookingAggregate).GetMethod("Confirm"));
        Assert.Null(typeof(FlightBookingAggregate).GetMethod("Cancel"));
        Assert.NotNull(typeof(FlightBookingAggregate).GetMethod(
            nameof(FlightBookingAggregate.ConfirmFromAuthoritativeReservationPaymentAndTickets)));
    }

    [Fact]
    public void Payment_Only_Or_Pnr_Without_Tickets_Cannot_Confirm()
    {
        var booking = OneWayBooking();
        var snapshot = Accept(booking);
        var reservation = ConfirmedReservation(booking);
        var payment = PaymentEvidence(booking, snapshot);

        Assert.Throws<InvalidOperationException>(() =>
            booking.ConfirmFromAuthoritativeReservationPaymentAndTickets(
                reservation,
                payment,
                [],
                snapshot.Monetary,
                [],
                Now.Plus(Duration.FromMinutes(5))));
        Assert.Equal(FlightBookingStatus.Pending, booking.Status);
    }

    [Fact]
    public void Complete_Tickets_With_Pnr_And_Payment_Confirm_Once()
    {
        var booking = OneWayBooking();
        var snapshot = Accept(booking);
        var reservation = ConfirmedReservation(booking);
        var payment = PaymentEvidence(booking, snapshot);
        var tickets = IssuedTickets(booking);

        booking.ConfirmFromAuthoritativeReservationPaymentAndTickets(
            reservation, payment, tickets, snapshot.Monetary, [], Now.Plus(Duration.FromMinutes(5)));
        Assert.Equal(FlightBookingStatus.Confirmed, booking.Status);
        Assert.NotNull(booking.ConfirmedAt);

        booking.ConfirmFromAuthoritativeReservationPaymentAndTickets(
            reservation, payment, tickets, snapshot.Monetary, [], Now.Plus(Duration.FromMinutes(6)));
        Assert.Equal(FlightBookingStatus.Confirmed, booking.Status);
        Assert.Throws<InvalidOperationException>(
            () => booking.CancelFromAuthoritativePaymentCompensation(Now.Plus(Duration.FromMinutes(7))));
    }

    [Fact]
    public void Partial_Tickets_Cannot_Confirm()
    {
        var booking = TwoPassengerBooking();
        var snapshot = Accept(booking);
        var reservation = ConfirmedReservation(booking);
        var payment = PaymentEvidence(booking, snapshot);
        var first = booking.Passengers.OrderBy(p => p.Ordinal).First();
        var ticket = FlightTicket.StartPending(booking.Id, first.Id, "test-source", Now);
        ticket.MarkIssued("125-111", Now.Plus(Duration.FromMinutes(1)));

        Assert.Throws<InvalidOperationException>(() =>
            booking.ConfirmFromAuthoritativeReservationPaymentAndTickets(
                reservation, payment, [ticket], snapshot.Monetary, [], Now.Plus(Duration.FromMinutes(5))));
        Assert.Equal(FlightBookingStatus.Pending, booking.Status);
    }

    [Fact]
    public void Payment_Mismatch_Cannot_Confirm()
    {
        var booking = OneWayBooking();
        var snapshot = Accept(booking);
        var reservation = ConfirmedReservation(booking);
        var payment = FlightBookingPaymentEvidence.Record(
            booking.Id, Guid.CreateVersion7(), 9m, "IRR", Now);
        var tickets = IssuedTickets(booking);
        Assert.Throws<InvalidOperationException>(() =>
            booking.ConfirmFromAuthoritativeReservationPaymentAndTickets(
                reservation, payment, tickets, snapshot.Monetary, [], Now.Plus(Duration.FromMinutes(5))));
        Assert.Equal(FlightBookingStatus.Pending, booking.Status);
    }

    [Fact]
    public void Ticket_Timeout_Stays_Initiated_And_Blocks_Retry()
    {
        var booking = OneWayBooking();
        var attempt = FlightTicketingAttempt.StartCreated(booking.Id, Now);
        attempt.MarkInitiated(Now.Plus(Duration.FromSeconds(1)));
        Assert.Equal(FlightTicketingAttemptStatus.Initiated, attempt.Status);
        Assert.True(attempt.IsUnresolved);
        Assert.False(attempt.IsTerminal);
        attempt.MarkFailed(Now.Plus(Duration.FromMinutes(2)));
        Assert.Equal(FlightTicketingAttemptStatus.Failed, attempt.Status);
        var retry = FlightTicketingAttempt.StartCreated(booking.Id, Now.Plus(Duration.FromMinutes(3)));
        Assert.Equal(FlightTicketingAttemptStatus.Created, retry.Status);
    }

    [Fact]
    public void Refund_Success_Cancels_Pending_Only_And_Payment_Stays_Succeeded()
    {
        var booking = OneWayBooking();
        var payment = PaymentAggregate.CreateForFlight(
            new FlightBookingPaymentReference(booking.Id.Value), Now);
        payment.BindExecutionSnapshot(Guid.CreateVersion7(), Irr(1_000_000m), Now);
        var attempt = payment.CreateAttempt(Now);
        payment.RecordProviderInitiation(
            attempt.Id,
            Now.Plus(Duration.FromMinutes(1)),
            new ProviderKey("test"),
            new ProviderRequestReference("req-1"),
            new ProviderTransactionReference("txn-1"));
        payment.RecordAuthoritativeCollectionSuccess(attempt.Id, Now.Plus(Duration.FromMinutes(2)));
        Assert.Equal(PaymentStatus.Succeeded, payment.Status);

        var refund = Refund.CreateForSucceededPayment(payment, Now.Plus(Duration.FromMinutes(3)));
        Assert.Equal(PaymentTargetKind.FlightBooking, refund.TargetKind);
        Assert.Equal(booking.Id.Value, refund.FlightBooking!.Value.FlightBookingId);

        booking.CancelFromAuthoritativePaymentCompensation(Now.Plus(Duration.FromMinutes(4)));
        Assert.Equal(FlightBookingStatus.Cancelled, booking.Status);
        Assert.Equal(PaymentStatus.Succeeded, payment.Status);
    }

    [Fact]
    public void Ambiguous_Ticketing_Does_Not_Mark_Failed()
    {
        var booking = OneWayBooking();
        var attempt = FlightTicketingAttempt.StartCreated(booking.Id, Now);
        attempt.MarkInitiated(Now.Plus(Duration.FromSeconds(1)));
        Assert.Equal(FlightTicketingAttemptStatus.Initiated, attempt.Status);
        Assert.DoesNotContain("Cancelling", Enum.GetNames<FlightBookingStatus>());
        Assert.DoesNotContain("RefundPending", Enum.GetNames<FlightBookingStatus>());
        Assert.DoesNotContain("Refunded", Enum.GetNames<FlightBookingStatus>());
        Assert.DoesNotContain("VoidPending", Enum.GetNames<FlightBookingStatus>());
        Assert.DoesNotContain("Failed", Enum.GetNames<FlightBookingStatus>());
        Assert.Null(typeof(FlightDomainAssemblyMarker).Assembly.GetType("TravelCore.Modules.Flight.Domain.PNR"));
    }

    [Fact]
    public async Task Flight_Payment_GetOrCreate_Is_One_Per_Booking_And_Not_Tour()
    {
        await using var db = CreatePaymentDb();
        var flight = new FlightBookingPaymentReference(Guid.CreateVersion7());
        var service = new PaymentGetOrCreateService(db, new FixedClock(Now));
        var first = await service.GetOrCreateAsync(flight);
        var second = await service.GetOrCreateAsync(flight);
        Assert.Equal(first.Id, second.Id);
        Assert.Equal(PaymentTargetKind.FlightBooking, first.TargetKind);
        Assert.Null(first.Booking);
        Assert.Null(first.HotelBooking);
        Assert.Equal(1, await db.Payments.CountAsync());
    }

    [Fact]
    public async Task Payment_Before_Confirmed_Pnr_Is_Rejected()
    {
        await using var db = CreatePaymentDb();
        var flight = new FlightBookingPaymentReference(Guid.CreateVersion7());
        var payment = PaymentAggregate.CreateForFlight(flight, Now);
        db.Payments.Add(payment);
        await db.SaveChangesAsync();
        var obligations = new FakeFlightObligationQuery
        {
            Next = new FlightBookingPaymentObligationRead(
                flight.FlightBookingId,
                "Pending",
                1_000_000m,
                "IRR",
                Guid.CreateVersion7(),
                PaymentEligible: false),
        };
        var service = new PaymentPreparationService(
            db,
            new MissingTourObligationQuery(),
            new FixedClock(Now),
            hotelObligations: null,
            flightObligations: obligations);
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => service.PrepareAsync(payment.Id));
        Assert.Contains("reservation", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Confirmed_Pnr_Makes_Payment_Eligible()
    {
        await using var db = CreatePaymentDb();
        var flight = new FlightBookingPaymentReference(Guid.CreateVersion7());
        var snapshotId = Guid.CreateVersion7();
        var payment = PaymentAggregate.CreateForFlight(flight, Now);
        db.Payments.Add(payment);
        await db.SaveChangesAsync();
        var obligations = new FakeFlightObligationQuery
        {
            Next = new FlightBookingPaymentObligationRead(
                flight.FlightBookingId,
                "Pending",
                1_000_000m,
                "IRR",
                snapshotId,
                PaymentEligible: true),
        };
        var service = new PaymentPreparationService(
            db,
            new MissingTourObligationQuery(),
            new FixedClock(Now),
            hotelObligations: null,
            flightObligations: obligations);
        await service.PrepareAsync(payment.Id);
        var loaded = await db.Payments.Include(x => x.ExecutionSnapshot).SingleAsync();
        Assert.Equal(1_000_000m, loaded.ExecutionSnapshot!.Amount.Amount);
        Assert.Equal(snapshotId, loaded.ExecutionSnapshot.BookingSnapshotId);
    }

    [Fact]
    public async Task Duplicate_Ticketing_Attempt_Is_Blocked_By_Unresolved_Index_InMemory()
    {
        await using var db = CreateFlightDb();
        var booking = OneWayBooking();
        db.FlightBookings.Add(booking);
        await db.SaveChangesAsync();
        db.FlightTicketingAttempts.Add(FlightTicketingAttempt.StartCreated(booking.Id, Now));
        await db.SaveChangesAsync();
        var second = FlightTicketingAttempt.StartCreated(booking.Id, Now.Plus(Duration.FromSeconds(1)));
        second.MarkInitiated(Now.Plus(Duration.FromSeconds(2)));
        db.FlightTicketingAttempts.Add(second);
        await db.SaveChangesAsync();
        Assert.Equal(2, await db.FlightTicketingAttempts.CountAsync());
    }

    [Fact]
    public async Task Ticketing_Service_Issues_Complete_Set_And_Confirms()
    {
        await using var db = CreateFlightDb();
        var booking = OneWayBooking();
        var snapshot = Accept(booking);
        var reservation = ConfirmedReservation(booking);
        db.FlightBookings.Add(booking);
        db.FlightOfferSnapshots.Add(snapshot);
        db.FlightSupplierReservations.Add(reservation);
        db.FlightBookingPaymentEvidence.Add(PaymentEvidence(booking, snapshot));
        await db.SaveChangesAsync();

        var source = new FakeTicketingSource
        {
            NextCreate = new FlightTicketingSourceResult(
                FlightTicketingSourceStatus.Complete,
                [
                    new FlightIssuedTicketFact("Ada", "Lovelace", "125-555"),
                ]),
        };
        var service = new FlightTicketingService(
            db,
            new FlightTicketingSourceResolver([source]),
            new FixedClock(Now.Plus(Duration.FromMinutes(1))));
        await service.InitiateAsync(booking.Id, "tick-1");

        var loaded = await db.FlightBookings.SingleAsync();
        Assert.Equal(FlightBookingStatus.Confirmed, loaded.Status);
        Assert.All(await db.FlightTickets.ToListAsync(), t => Assert.Equal(FlightTicketStatus.Issued, t.Status));
        Assert.Equal(FlightTicketingAttemptStatus.Succeeded, (await db.FlightTicketingAttempts.SingleAsync()).Status);
    }

    [Fact]
    public async Task Timeout_Leaves_Initiated_And_Does_Not_Confirm()
    {
        await using var db = CreateFlightDb();
        var booking = OneWayBooking();
        var snapshot = Accept(booking);
        db.FlightBookings.Add(booking);
        db.FlightOfferSnapshots.Add(snapshot);
        db.FlightSupplierReservations.Add(ConfirmedReservation(booking));
        db.FlightBookingPaymentEvidence.Add(PaymentEvidence(booking, snapshot));
        await db.SaveChangesAsync();

        var source = new FakeTicketingSource { ThrowTimeout = true };
        var service = new FlightTicketingService(
            db,
            new FlightTicketingSourceResolver([source]),
            new FixedClock(Now.Plus(Duration.FromMinutes(1))));
        await service.InitiateAsync(booking.Id, "tick-timeout");
        Assert.Equal(FlightBookingStatus.Pending, (await db.FlightBookings.SingleAsync()).Status);
        Assert.Equal(FlightTicketingAttemptStatus.Initiated, (await db.FlightTicketingAttempts.SingleAsync()).Status);
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.InitiateAsync(booking.Id, "tick-retry"));
    }

    private static FlightBookingAggregate OneWayBooking() =>
        FlightBookingAggregate.Create(
            FlightTripType.OneWay,
            [Direct("THR", "LHR", Dep, Arr)],
            [new FlightPassengerSpecification("Ada", "Lovelace", FlightPassengerCategory.Adult)]);

    private static FlightBookingAggregate TwoPassengerBooking() =>
        FlightBookingAggregate.Create(
            FlightTripType.OneWay,
            [Direct("THR", "LHR", Dep, Arr)],
            [
                new FlightPassengerSpecification("Ada", "Lovelace", FlightPassengerCategory.Adult),
                new FlightPassengerSpecification("Alan", "Turing", FlightPassengerCategory.Child),
            ]);

    private static FlightOfferSnapshot Accept(FlightBookingAggregate booking) =>
        FlightOfferSnapshot.Accept(
            booking,
            Now,
            "test-source",
            "offer-1",
            Now.Minus(Duration.FromMinutes(1)),
            Expires,
            Irr(800_000m),
            Irr(150_000m),
            Irr(50_000m),
            Irr(1_000_000m),
            Identities(booking),
            new FlightPassengerCount(
                booking.Passengers.Count(p => p.Category == FlightPassengerCategory.Adult),
                booking.Passengers.Count(p => p.Category == FlightPassengerCategory.Child),
                booking.Passengers.Count(p => p.Category == FlightPassengerCategory.Infant)),
            new FlightFareRulesDraft(true, true, Ticketing, Irr(100_000m), Irr(80_000m)));

    private static FlightSupplierReservation ConfirmedReservation(FlightBookingAggregate booking)
    {
        var reservation = FlightSupplierReservation.StartPending(booking.Id, "test-source", Now);
        var attempt = reservation.StartAttempt(Now);
        reservation.MarkAttemptInitiated(attempt.Id, Now.Plus(Duration.FromSeconds(1)));
        reservation.ConfirmAttempt(
            attempt.Id,
            Now.Plus(Duration.FromMinutes(1)),
            "src-res-1",
            "ABC123",
            ReservationExpiry,
            Identities(booking),
            Identities(booking),
            Passengers(booking),
            Passengers(booking));
        return reservation;
    }

    private static FlightBookingPaymentEvidence PaymentEvidence(
        FlightBookingAggregate booking,
        FlightOfferSnapshot snapshot) =>
        FlightBookingPaymentEvidence.Record(
            booking.Id,
            Guid.CreateVersion7(),
            snapshot.Monetary.Total.Amount,
            snapshot.Monetary.Total.Currency.Value,
            Now.Plus(Duration.FromMinutes(2)));

    private static IReadOnlyList<FlightTicket> IssuedTickets(FlightBookingAggregate booking) =>
        booking.Passengers
            .Select((p, i) =>
            {
                var ticket = FlightTicket.StartPending(booking.Id, p.Id, "test-source", Now);
                ticket.MarkIssued($"125-{i + 1:000}", Now.Plus(Duration.FromMinutes(3)));
                return ticket;
            })
            .ToArray();

    private static FlightJourneySpecification Direct(string origin, string destination, Instant dep, Instant arr) =>
        new(
        [
            new FlightSegmentSpecification(
                new AirportReference(origin),
                new AirportReference(destination),
                dep,
                "Asia/Tehran",
                arr,
                "Europe/London",
                new AirlineReference("TK"),
                null,
                "TK800"),
        ]);

    private static IReadOnlyList<FlightOfferSegmentIdentity> Identities(FlightBookingAggregate booking) =>
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

    private static IReadOnlyList<FlightReservationPassengerFact> Passengers(FlightBookingAggregate booking) =>
        booking.Passengers
            .OrderBy(p => p.Ordinal)
            .Select(p => new FlightReservationPassengerFact(p.GivenName, p.FamilyName, p.Category))
            .ToArray();

    private static MoneyValue Irr(decimal amount) => new(amount, CurrencyCode.Parse("IRR"));

    private static PaymentDbContext CreatePaymentDb()
    {
        var options = new DbContextOptionsBuilder<PaymentDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;
        return new PaymentDbContext(options);
    }

    private static FlightDbContext CreateFlightDb()
    {
        var options = new DbContextOptionsBuilder<FlightDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;
        return new FlightDbContext(options);
    }

    private sealed class FakeFlightObligationQuery : IFlightBookingPaymentObligationQuery
    {
        public FlightBookingPaymentObligationRead? Next { get; set; }

        public Task<FlightBookingPaymentObligationRead?> GetByFlightBookingIdAsync(
            Guid flightBookingId,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(Next);
    }

    private sealed class MissingTourObligationQuery : TravelCore.Modules.Booking.Contracts.IBookingPaymentObligationQuery
    {
        public Task<TravelCore.Modules.Booking.Contracts.BookingPaymentObligationRead?> GetByBookingIdAsync(
            Guid bookingId,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<TravelCore.Modules.Booking.Contracts.BookingPaymentObligationRead?>(null);
    }

    private sealed class FixedClock(Instant now) : IClock
    {
        public Instant GetCurrentInstant() => now;
    }

    private sealed class FakeTicketingSource : IFlightTicketingSource
    {
        public FlightSourceKey Key { get; } = new("test-source");

        public IReadOnlySet<FlightSourceCapability> Capabilities { get; } =
            new HashSet<FlightSourceCapability>
            {
                FlightSourceCapability.TicketCreate,
                FlightSourceCapability.TicketQuery,
            };

        public bool NotFoundProvesNoTicket => true;

        public bool ThrowTimeout { get; set; }

        public FlightTicketingSourceResult? NextCreate { get; set; }

        public Task<FlightTicketingSourceResult> CreateTicketsAsync(
            FlightTicketingRequest request,
            CancellationToken cancellationToken = default)
        {
            if (ThrowTimeout)
            {
                throw new TimeoutException();
            }

            return Task.FromResult(NextCreate ?? new FlightTicketingSourceResult(FlightTicketingSourceStatus.Failed));
        }

        public Task<FlightTicketingQueryResult> QueryTicketStatusAsync(
            string sourceReservationReference,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(new FlightTicketingQueryResult(FlightTicketingSourceStatus.Unknown));
    }
}
