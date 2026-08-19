using NodaTime;
using TravelCore.Money;
using TravelCore.Modules.Flight.Contracts;
using TravelCore.Modules.Flight.Domain;
using Xunit;
using MoneyValue = TravelCore.Money.Money;

namespace TravelCore.Modules.Flight.UnitTests;

public sealed class FlightSupplierReservationTests
{
    private static readonly Instant Now = Instant.FromUtc(2026, 8, 18, 12, 0);
    private static readonly Instant Dep = Instant.FromUtc(2026, 9, 1, 6, 0);
    private static readonly Instant Arr = Instant.FromUtc(2026, 9, 1, 10, 0);
    private static readonly Instant ConnDep = Instant.FromUtc(2026, 9, 1, 12, 0);
    private static readonly Instant ConnArr = Instant.FromUtc(2026, 9, 1, 16, 0);
    private static readonly Instant Expires = Instant.FromUtc(2026, 8, 18, 14, 0);
    private static readonly Instant Ticketing = Instant.FromUtc(2026, 8, 19, 12, 0);
    private static readonly Instant ReservationExpiry = Instant.FromUtc(2026, 8, 20, 12, 0);

    [Fact]
    public void Reservation_Statuses_Have_No_Failed_And_No_Pnr_Type()
    {
        Assert.Equal(
            new[] { "Pending", "Confirmed", "Expired", "Cancelled" },
            Enum.GetNames<FlightSupplierReservationStatus>());
        Assert.DoesNotContain("Failed", Enum.GetNames<FlightSupplierReservationStatus>());
        Assert.Equal(
            new[] { "Created", "Initiated", "Confirmed", "Failed" },
            Enum.GetNames<FlightSupplierReservationAttemptStatus>());
        Assert.Null(typeof(FlightDomainAssemblyMarker).Assembly.GetType("TravelCore.Modules.Flight.Domain.PNR"));
        Assert.NotNull(typeof(FlightDomainAssemblyMarker).Assembly.GetType("TravelCore.Modules.Flight.Domain.FlightBookingStatus"));
        Assert.NotNull(typeof(FlightDomainAssemblyMarker).Assembly.GetType("TravelCore.Modules.Flight.Domain.FlightTicket"));
        Assert.True(FlightOwnershipBoundary.FlightBookingStatusImplemented);
        Assert.False(FlightReservationOwnershipBoundary.PnrTypeImplemented);
        Assert.False(FlightReservationOwnershipBoundary.PaymentRequiredForReservation);
        Assert.True(FlightReservationOwnershipBoundary.TicketImplemented);
        Assert.Equal(FlightSourceCapability.ReservationCreate, (FlightSourceCapability)4);
        Assert.Equal(FlightSourceCapability.ReservationQuery, (FlightSourceCapability)5);
        Assert.Equal(FlightSourceCapability.TicketCreate, (FlightSourceCapability)6);
        Assert.Equal(FlightSourceCapability.TicketQuery, (FlightSourceCapability)7);
        Assert.Equal(
            "OfferExpiresAt != ReservationExpiresAt",
            FlightReservationOwnershipBoundary.OfferExpiryIsNotReservationExpiry);
        Assert.Equal(
            "TicketingDeadline != ReservationExpiresAt",
            FlightReservationOwnershipBoundary.TicketingDeadlineIsNotReservationExpiry);
    }

    [Fact]
    public void Complete_Reservation_Confirms_Without_Payment_Or_Ticket()
    {
        var booking = OneWayBooking();
        var snapshot = Accept(booking);
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

        Assert.Equal(FlightSupplierReservationStatus.Confirmed, reservation.Status);
        Assert.Equal(FlightSupplierReservationAttemptStatus.Confirmed, attempt.Status);
        Assert.Equal("src-res-1", reservation.SourceReservationReference);
        Assert.Equal("ABC123", reservation.ReservationLocator);
        Assert.Equal(ReservationExpiry, reservation.ReservationExpiresAt);
        Assert.NotEqual(snapshot.OfferExpiresAt, reservation.ReservationExpiresAt);
        Assert.NotEqual(snapshot.FareRules.TicketingDeadline, reservation.ReservationExpiresAt);
        Assert.Equal(7, reservation.Id.Value.Version);
        Assert.NotEqual(booking.Id.Value, reservation.Id.Value);
        Assert.NotEqual(booking.Id.Value.ToString("D"), reservation.ReservationLocator);
        Assert.Throws<InvalidOperationException>(() => reservation.StartAttempt(Now.Plus(Duration.FromMinutes(2))));
    }

    [Fact]
    public void Timeout_Leaves_Initiated_Pending_And_Blocks_Retry_Until_Failed()
    {
        var booking = OneWayBooking();
        var reservation = FlightSupplierReservation.StartPending(booking.Id, "test-source", Now);
        var attempt = reservation.StartAttempt(Now);
        reservation.MarkAttemptInitiated(attempt.Id, Now.Plus(Duration.FromSeconds(1)));

        Assert.Equal(FlightSupplierReservationStatus.Pending, reservation.Status);
        Assert.Equal(FlightSupplierReservationAttemptStatus.Initiated, attempt.Status);
        Assert.True(attempt.IsUnresolved);
        Assert.False(attempt.IsTerminal);
        var blocked = Assert.Throws<InvalidOperationException>(
            () => reservation.StartAttempt(Now.Plus(Duration.FromMinutes(1))));
        Assert.Contains("unresolved", blocked.Message, StringComparison.OrdinalIgnoreCase);

        reservation.FailAttempt(attempt.Id, Now.Plus(Duration.FromMinutes(2)));
        Assert.Equal(FlightSupplierReservationAttemptStatus.Failed, attempt.Status);
        Assert.Equal(FlightSupplierReservationStatus.Pending, reservation.Status);
        var retry = reservation.StartAttempt(Now.Plus(Duration.FromMinutes(3)));
        Assert.Equal(FlightSupplierReservationAttemptStatus.Created, retry.Status);
    }

    [Fact]
    public void Connecting_Itinerary_And_MultiPassenger_Must_Be_Complete_To_Confirm()
    {
        var booking = ConnectingMultiPassenger();
        var reservation = FlightSupplierReservation.StartPending(booking.Id, "test-source", Now);
        var attempt = reservation.StartAttempt(Now);
        reservation.ConfirmAttempt(
            attempt.Id,
            Now.Plus(Duration.FromMinutes(1)),
            "src-conn",
            "CONN99",
            null,
            Identities(booking),
            Identities(booking),
            Passengers(booking),
            Passengers(booking));
        Assert.Equal(FlightSupplierReservationStatus.Confirmed, reservation.Status);
        Assert.Equal(2, Identities(booking).Count);
        Assert.Equal(2, Passengers(booking).Count);
    }

    [Fact]
    public void Partial_Passenger_Or_Itinerary_Cannot_Confirm()
    {
        var booking = ConnectingMultiPassenger();
        var reservation = FlightSupplierReservation.StartPending(booking.Id, "test-source", Now);
        var attempt = reservation.StartAttempt(Now);
        var passengerEx = Assert.Throws<InvalidOperationException>(() =>
            reservation.ConfirmAttempt(
                attempt.Id,
                Now.Plus(Duration.FromMinutes(1)),
                "src-partial-pax",
                "PARTP",
                null,
                Identities(booking),
                Identities(booking),
                [Passengers(booking)[0]],
                Passengers(booking)));
        Assert.Contains("passenger", passengerEx.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(FlightSupplierReservationStatus.Pending, reservation.Status);

        var itineraryEx = Assert.Throws<InvalidOperationException>(() =>
            reservation.ConfirmAttempt(
                attempt.Id,
                Now.Plus(Duration.FromMinutes(1)),
                "src-partial-seg",
                "PARTS",
                null,
                [Identities(booking)[0]],
                Identities(booking),
                Passengers(booking),
                Passengers(booking)));
        Assert.Contains("itinerary", itineraryEx.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(FlightSupplierReservationStatus.Pending, reservation.Status);
    }

    [Fact]
    public void Monetary_And_Currency_Mismatch_Do_Not_Mutate_Snapshot()
    {
        var booking = OneWayBooking();
        var snapshot = Accept(booking);
        var originalTotal = snapshot.Monetary.Total.Amount;
        var originalCurrency = snapshot.Monetary.CurrencyCode.Value;
        var kinds = FlightReservationReconciliation.CollectIssues(
            Identities(booking),
            Passengers(booking),
            Identities(booking),
            Passengers(booking),
            snapshot.SourceOfferReference,
            snapshot.SourceOfferReference,
            snapshot.Monetary.Total,
            Irr(1_250_000m));
        Assert.Contains(FlightReconciliationIssueKind.MonetaryMismatch, kinds);

        var currencyKinds = FlightReservationReconciliation.CollectIssues(
            Identities(booking),
            Passengers(booking),
            Identities(booking),
            Passengers(booking),
            snapshot.SourceOfferReference,
            snapshot.SourceOfferReference,
            snapshot.Monetary.Total,
            new MoneyValue(1_000_000m, CurrencyCode.Parse("USD")));
        Assert.Contains(FlightReconciliationIssueKind.CurrencyMismatch, currencyKinds);

        var offerKinds = FlightReservationReconciliation.CollectIssues(
            Identities(booking),
            Passengers(booking),
            Identities(booking),
            Passengers(booking),
            snapshot.SourceOfferReference,
            "other-offer",
            snapshot.Monetary.Total,
            Irr(1_000_000m));
        Assert.Contains(FlightReconciliationIssueKind.OfferMismatch, offerKinds);

        Assert.Equal(originalTotal, snapshot.Monetary.Total.Amount);
        Assert.Equal(originalCurrency, snapshot.Monetary.CurrencyCode.Value);
        Assert.Equal(1_000_000m, snapshot.Monetary.Total.Amount);
    }

    [Fact]
    public void Authoritative_Expiry_And_Contradictory_Cancel_Stay_Terminal()
    {
        var booking = OneWayBooking();
        var reservation = FlightSupplierReservation.StartPending(booking.Id, "test-source", Now);
        var attempt = reservation.StartAttempt(Now);
        reservation.ConfirmAttempt(
            attempt.Id,
            Now.Plus(Duration.FromMinutes(1)),
            "src-exp",
            "EXP123",
            ReservationExpiry,
            Identities(booking),
            Identities(booking),
            Passengers(booking),
            Passengers(booking));

        reservation.ExpireFromSource(Now.Plus(Duration.FromHours(1)));
        Assert.Equal(FlightSupplierReservationStatus.Expired, reservation.Status);
        Assert.NotNull(reservation.ExpiredAt);
        var contradiction = Assert.Throws<InvalidOperationException>(
            () => reservation.CancelFromAuthoritativeSource(Now.Plus(Duration.FromHours(2))));
        Assert.Contains("contradictory", contradiction.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(FlightSupplierReservationStatus.Expired, reservation.Status);
    }

    [Fact]
    public void Internal_Identities_Cannot_Be_Used_As_Pnr_Or_Source_Reference()
    {
        var booking = OneWayBooking();
        var reservation = FlightSupplierReservation.StartPending(booking.Id, "test-source", Now);
        var attempt = reservation.StartAttempt(Now);
        Assert.Throws<ArgumentException>(() =>
            reservation.ConfirmAttempt(
                attempt.Id,
                Now.Plus(Duration.FromMinutes(1)),
                booking.Id.Value.ToString("D"),
                "ABC123",
                null,
                Identities(booking),
                Identities(booking),
                Passengers(booking),
                Passengers(booking)));
        Assert.Throws<ArgumentException>(() =>
            reservation.ConfirmAttempt(
                attempt.Id,
                Now.Plus(Duration.FromMinutes(1)),
                "src-ok",
                reservation.Id.Value.ToString("D"),
                null,
                Identities(booking),
                Identities(booking),
                Passengers(booking),
                Passengers(booking)));
        Assert.Equal(FlightSupplierReservationStatus.Pending, reservation.Status);
    }

    private static FlightBooking OneWayBooking() =>
        FlightBooking.Create(
            FlightTripType.OneWay,
            [Direct("THR", "LHR", Dep, Arr)],
            [new FlightPassengerSpecification("Ada", "Lovelace", FlightPassengerCategory.Adult)]);

    private static FlightBooking ConnectingMultiPassenger() =>
        FlightBooking.Create(
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

    private static FlightOfferSnapshot Accept(FlightBooking booking) =>
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
}
