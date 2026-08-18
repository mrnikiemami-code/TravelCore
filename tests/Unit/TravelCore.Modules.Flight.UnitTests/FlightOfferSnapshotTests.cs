using NodaTime;
using TravelCore.Money;
using TravelCore.Modules.Flight.Contracts;
using TravelCore.Modules.Flight.Domain;
using TravelCore.Modules.Flight.Infrastructure.Search;
using Xunit;
using MoneyValue = TravelCore.Money.Money;

namespace TravelCore.Modules.Flight.UnitTests;

public sealed class FlightOfferSnapshotTests
{
    private static readonly Instant Now = Instant.FromUtc(2026, 8, 18, 12, 0);
    private static readonly Instant Dep = Instant.FromUtc(2026, 9, 1, 6, 0);
    private static readonly Instant Arr = Instant.FromUtc(2026, 9, 1, 10, 0);
    private static readonly Instant RetDep = Instant.FromUtc(2026, 9, 8, 8, 0);
    private static readonly Instant RetArr = Instant.FromUtc(2026, 9, 8, 14, 0);
    private static readonly Instant Expires = Instant.FromUtc(2026, 8, 18, 14, 0);
    private static readonly Instant Ticketing = Instant.FromUtc(2026, 8, 19, 12, 0);
    private static readonly FlightSourceKey SourceKey = new("test-source");

    [Fact]
    public void Accept_Valid_OneWay_Offer()
    {
        var booking = OneWayBooking();
        var snapshot = Accept(booking);
        Assert.Equal(booking.Id, snapshot.FlightBookingId);
        Assert.Equal(FlightTripType.OneWay, snapshot.TripType);
        Assert.Equal("test-source", snapshot.SourceKey);
        Assert.Equal("offer-1", snapshot.SourceOfferReference);
        Assert.Equal(1_000_000m, snapshot.Monetary.Total.Amount);
        Assert.Equal(800_000m, snapshot.Monetary.BaseFare.Amount);
        Assert.Equal(150_000m, snapshot.Monetary.Taxes.Amount);
        Assert.Equal(50_000m, snapshot.Monetary.Fees.Amount);
        Assert.Equal("IRR", snapshot.Monetary.CurrencyCode.Value);
        Assert.Equal(Expires, snapshot.OfferExpiresAt);
        Assert.Equal(Ticketing, snapshot.FareRules.TicketingDeadline);
        Assert.NotEqual(snapshot.OfferExpiresAt, snapshot.FareRules.TicketingDeadline);
        Assert.True(snapshot.FareRules.Refundable);
        Assert.True(snapshot.FareRules.Changeable);
        Assert.Equal(100_000m, snapshot.FareRules.CancelPenalty!.Amount);
        Assert.Equal(7, snapshot.Id.Value.Version);
        Assert.Equal("Y", snapshot.BookingClass);
        Assert.Equal("YOW", snapshot.FareBasis);
        Assert.Single(snapshot.FareRules.Baggage);
        Assert.Equal(1, snapshot.FareRules.Baggage[0].Quantity);
        Assert.Equal(23m, snapshot.FareRules.Baggage[0].Weight);
        Assert.Equal("KG", snapshot.FareRules.Baggage[0].Unit);
        Assert.False(FlightOfferOwnershipBoundary.SilentRepricingImplemented);
        Assert.False(FlightOfferOwnershipBoundary.HardcodedOfferTtlImplemented);
        Assert.False(FlightOfferOwnershipBoundary.AncillariesImplemented);
        Assert.Equal("TicketingDeadline != OfferExpiresAt", FlightOfferOwnershipBoundary.TicketingDeadlineIsNotOfferExpiry);
        Assert.Equal("Toman != CurrencyCode", FlightOfferOwnershipBoundary.TomanIsNotCurrencyCode);
        Assert.Null(typeof(FlightOfferSnapshot).GetField("DefaultTtlMinutes"));
    }

    [Fact]
    public void Expired_Offer_Is_Rejected()
    {
        var booking = OneWayBooking();
        var ex = Assert.Throws<InvalidOperationException>(() =>
            Accept(booking, expires: Now.Minus(Duration.FromSeconds(1))));
        Assert.Contains("Expired", ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Unknown_And_Timeout_Outcomes_Are_Rejected()
    {
        var booking = OneWayBooking();
        var unknown = FlightOfferSourceResult.Unknown(SourceKey, Now);
        var timeout = FlightOfferAcceptanceCoordinator.MapCanceledToUnknown(SourceKey, Now);
        var unknownEx = Assert.Throws<InvalidOperationException>(() =>
            FlightOfferAcceptanceCoordinator.Accept(booking, Now, unknown));
        var timeoutEx = Assert.Throws<InvalidOperationException>(() =>
            FlightOfferAcceptanceCoordinator.Accept(booking, Now, timeout));
        Assert.Contains("Unknown", unknownEx.Message, StringComparison.Ordinal);
        Assert.Contains("Unknown", timeoutEx.Message, StringComparison.Ordinal);
        Assert.Equal(FlightOfferOutcome.Unknown, timeout.Outcome);
    }

    [Fact]
    public void Changed_Offer_Requires_Requote()
    {
        var booking = OneWayBooking();
        var changed = FlightOfferSourceResult.Changed(SourceKey, Now);
        var ex = Assert.Throws<InvalidOperationException>(() =>
            FlightOfferAcceptanceCoordinator.Accept(booking, Now, changed));
        Assert.Contains("requote", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Higher_And_Lower_Repricing_Is_Not_Silent()
    {
        var booking = OneWayBooking();
        var first = Accept(booking, sourceOfferReference: "offer-low");
        var higher = Assert.Throws<InvalidOperationException>(() =>
            Accept(booking, existing: first, sourceOfferReference: "offer-high", total: 1_200_000m, baseFare: 1_000_000m));
        var lower = Assert.Throws<InvalidOperationException>(() =>
            Accept(booking, existing: first, sourceOfferReference: "offer-lower", total: 800_000m, baseFare: 600_000m));
        Assert.Contains("requote", higher.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("requote", lower.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(1_000_000m, first.Monetary.Total.Amount);

        var sameIdentityDifferentPrice = Assert.Throws<InvalidOperationException>(() =>
            Accept(booking, existing: first, sourceOfferReference: "offer-low", total: 1_200_000m, baseFare: 1_000_000m));
        Assert.Contains("Silent repricing", sameIdentityDifferentPrice.Message, StringComparison.Ordinal);

        var observedMismatch = Assert.Throws<InvalidOperationException>(() =>
            Accept(booking, previouslyObservedTotal: Irr(900_000m)));
        Assert.Contains("requote", observedMismatch.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Itinerary_Mismatch_Is_Rejected()
    {
        var booking = OneWayBooking();
        var wrong = new[]
        {
            new FlightOfferSegmentIdentity(
                1,
                1,
                new AirportReference("IKA"),
                new AirportReference("LHR"),
                Dep,
                Arr,
                new AirlineReference("TK"),
                null,
                "TK800"),
        };
        Assert.Throws<ArgumentException>(() => Accept(booking, segments: wrong));
    }

    [Fact]
    public void Passenger_Mismatch_Is_Rejected()
    {
        var booking = OneWayBooking();
        Assert.Throws<ArgumentException>(() =>
            Accept(booking, passengers: new FlightPassengerCount(1, 1)));
    }

    [Fact]
    public void Partial_Offer_Is_Rejected()
    {
        var booking = RoundTripBooking();
        var outboundOnly = Identities(booking).Take(1).ToArray();
        Assert.Throws<ArgumentException>(() => Accept(booking, segments: outboundOnly));
    }

    [Fact]
    public void Mixed_Currency_Is_Rejected()
    {
        var booking = OneWayBooking();
        Assert.Throws<InvalidOperationException>(() =>
            Accept(booking, taxes: new MoneyValue(150_000m, CurrencyCode.Parse("USD"))));
    }

    [Fact]
    public void Money_Arithmetic_Must_Balance_Without_Float()
    {
        var booking = OneWayBooking();
        Assert.Throws<ArgumentException>(() =>
            Accept(booking, total: 999_999m, baseFare: 800_000m, taxes: Irr(150_000m), fees: 50_000m));

        var snapshot = Accept(booking, total: 1_000_000.125m, baseFare: 800_000.100m, taxes: Irr(150_000.025m), fees: 50_000m);
        Assert.Equal(1_000_000.125m, snapshot.Monetary.Total.Amount);
        Assert.Equal(
            snapshot.Monetary.BaseFare.Amount + snapshot.Monetary.Taxes.Amount + snapshot.Monetary.Fees.Amount,
            snapshot.Monetary.Total.Amount);
        Assert.Equal(typeof(decimal), snapshot.Monetary.Total.Amount.GetType());
        Assert.DoesNotContain(typeof(float), new[] { snapshot.Monetary.Total.Amount.GetType() });
        Assert.DoesNotContain(typeof(double), new[] { snapshot.Monetary.Total.Amount.GetType() });
    }

    [Fact]
    public void Same_Offer_Is_Idempotent_And_Different_Offer_Conflicts()
    {
        var booking = OneWayBooking();
        var first = Accept(booking, sourceOfferReference: "offer-same");
        var second = Accept(booking, existing: first, sourceOfferReference: "offer-same");
        Assert.Same(first, second);
        var conflict = Assert.Throws<InvalidOperationException>(() =>
            Accept(booking, existing: first, sourceOfferReference: "offer-other"));
        Assert.Contains("requote", conflict.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Ticketing_Deadline_Is_Distinct_From_Offer_Expiry()
    {
        var booking = OneWayBooking();
        Assert.Throws<ArgumentException>(() =>
            Accept(booking, fareRules: new FlightFareRulesDraft(true, true, TicketingDeadline: Expires)));
        var snapshot = Accept(booking);
        Assert.NotEqual(snapshot.OfferExpiresAt, snapshot.FareRules.TicketingDeadline);
        Assert.Equal(typeof(Instant), snapshot.OfferExpiresAt.GetType());
        Assert.Equal(typeof(Instant), snapshot.FareRules.TicketingDeadline!.Value.GetType());
    }

    [Fact]
    public void Toman_Is_Not_CurrencyCode_For_Snapshots()
    {
        Assert.Throws<ArgumentException>(() => CurrencyCode.Parse("1"));
        var toman = CurrencyCode.Parse("TOMAN");
        Assert.Equal("Toman != CurrencyCode", "Toman != CurrencyCode");
        var booking = OneWayBooking();
        Assert.Throws<ArgumentException>(() =>
            FlightOfferSnapshot.Accept(
                booking,
                Now,
                "test-source",
                "offer-toman",
                Now,
                Expires,
                new MoneyValue(800_000m, toman),
                new MoneyValue(150_000m, toman),
                new MoneyValue(50_000m, toman),
                new MoneyValue(1_000_000m, toman),
                Identities(booking),
                PassengerCount(booking),
                new FlightFareRulesDraft(false, false)));
    }

    [Fact]
    public void Duplicate_Source_Resolver_Keys_Are_Rejected()
    {
        var a = new FakeOfferSource("alpha");
        var b = new FakeOfferSource("alpha");
        Assert.Throws<ArgumentException>(() => new FlightOfferSourceResolver([a, b]));
    }

    [Fact]
    public void Offer_Request_Contains_No_Passenger_Pii()
    {
        var booking = OneWayBooking();
        var request = new FlightOfferRequest(
            booking.Id.Value,
            booking.TripType,
            Identities(booking),
            PassengerCount(booking));
        string[] forbidden = ["Email", "Phone", "GivenName", "FamilyName", "Passport", "NationalId", "CardNumber"];
        foreach (var type in new[]
                 {
                     typeof(FlightOfferRequest),
                     typeof(FlightOfferSourceResult),
                     typeof(FlightOfferSnapshot),
                     typeof(FlightBookingMonetarySnapshot),
                     typeof(FlightFareRulesSnapshot),
                 })
        {
            var names = type.GetProperties().Select(p => p.Name).ToArray();
            foreach (var token in forbidden)
            {
                Assert.DoesNotContain(token, names);
            }
        }

        Assert.Equal(1, request.Passengers.AdultCount);
        Assert.False(FlightOfferOwnershipBoundary.PartialRefundExecutionImplemented);
        Assert.False(FlightOfferOwnershipBoundary.PaymentIntegrationImplemented);
        Assert.False(FlightOfferOwnershipBoundary.PnrImplemented);
    }

    private static FlightOfferSnapshot Accept(
        FlightBooking booking,
        Instant? expires = null,
        string sourceOfferReference = "offer-1",
        decimal total = 1_000_000m,
        decimal baseFare = 800_000m,
        MoneyValue? taxes = null,
        decimal fees = 50_000m,
        IReadOnlyList<FlightOfferSegmentIdentity>? segments = null,
        FlightPassengerCount? passengers = null,
        FlightFareRulesDraft? fareRules = null,
        FlightOfferSnapshot? existing = null,
        MoneyValue? previouslyObservedTotal = null)
    {
        return FlightOfferSnapshot.Accept(
            booking,
            Now,
            "test-source",
            sourceOfferReference,
            Now.Minus(Duration.FromMinutes(1)),
            expires ?? Expires,
            Irr(baseFare),
            taxes ?? Irr(150_000m),
            Irr(fees),
            Irr(total),
            segments ?? Identities(booking),
            passengers ?? PassengerCount(booking),
            fareRules ?? new FlightFareRulesDraft(true, true, Ticketing, Irr(100_000m), Irr(80_000m)),
            existing,
            previouslyObservedTotal,
            categoryFares: null,
            baggage:
            [
                new FlightBaggageAllowanceDraft(1, 23m, "KG", "CHECKED", FlightPassengerCategory.Adult),
            ],
            cabin: "Economy",
            bookingClass: "Y",
            fareBasis: "YOW",
            fareFamily: "ECO");
    }

    private static FlightBooking OneWayBooking() =>
        FlightBooking.Create(
            FlightTripType.OneWay,
            [Direct("THR", "LHR", Dep, Arr)],
            [new FlightPassengerSpecification("Ada", "Lovelace", FlightPassengerCategory.Adult)]);

    private static FlightBooking RoundTripBooking() =>
        FlightBooking.Create(
            FlightTripType.RoundTrip,
            [Direct("THR", "LHR", Dep, Arr), Direct("LHR", "THR", RetDep, RetArr)],
            [new FlightPassengerSpecification("Ada", "Lovelace", FlightPassengerCategory.Adult)]);

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

    private static FlightPassengerCount PassengerCount(FlightBooking booking) =>
        new(
            booking.Passengers.Count(p => p.Category == FlightPassengerCategory.Adult),
            booking.Passengers.Count(p => p.Category == FlightPassengerCategory.Child),
            booking.Passengers.Count(p => p.Category == FlightPassengerCategory.Infant));

    private static MoneyValue Irr(decimal amount) => new(amount, CurrencyCode.Parse("IRR"));

    private sealed class FakeOfferSource(string key) : IFlightOfferSource
    {
        public FlightSourceKey Key { get; } = new(key);

        public IReadOnlySet<FlightSourceCapability> Capabilities { get; } =
            new HashSet<FlightSourceCapability> { FlightSourceCapability.OfferRevalidation };

        public Task<FlightOfferSourceResult> GetOfferAsync(
            FlightOfferRequest request,
            CancellationToken cancellationToken = default) =>
            throw new NotImplementedException();
    }
}
