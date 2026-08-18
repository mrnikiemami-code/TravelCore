using NodaTime;
using TravelCore.Modules.Flight.Contracts;
using TravelCore.Modules.Flight.Domain;
using TravelCore.Modules.Flight.Infrastructure.Search;
using Xunit;

namespace TravelCore.Modules.Flight.UnitTests;

public sealed class FlightSearchAvailabilityTests
{
    private static readonly LocalDate Departure = new(2026, 9, 1);
    private static readonly LocalDate Return = new(2026, 9, 8);
    private static readonly Instant Observed = Instant.FromUtc(2026, 8, 18, 12, 0);
    private static readonly Instant Dep = Instant.FromUtc(2026, 9, 1, 6, 0);
    private static readonly Instant Arr = Instant.FromUtc(2026, 9, 1, 10, 0);
    private static readonly Instant RetDep = Instant.FromUtc(2026, 9, 8, 8, 0);
    private static readonly Instant RetArr = Instant.FromUtc(2026, 9, 8, 12, 0);

    [Fact]
    public async Task OneWay_Search_Request_Is_Accepted()
    {
        var source = FakeFlightSource.Available("alpha");
        var service = CreateService(source);
        var result = await service.SearchAsync(OneWayRequest(), source.Key);
        Assert.Equal(FlightSearchCompletion.Complete, result.Completion);
        Assert.Single(result.Options);
        Assert.Equal(FlightTripType.OneWay, result.Options[0].TripType);
        Assert.Equal("alpha", result.Options[0].SourceKey.Value);
        Assert.Equal("opt-1", result.Options[0].SourceOptionReference);
        Assert.Equal(Observed, result.Options[0].ObservedAt);
    }

    [Fact]
    public async Task RoundTrip_Search_Request_Is_Accepted()
    {
        var source = FakeFlightSource.Available("alpha");
        var service = CreateService(source);
        var result = await service.SearchAsync(RoundTripRequest(), source.Key);
        Assert.Equal(2, result.Options[0].Journeys.Count);
    }

    [Fact]
    public void MultiCity_TripType_Is_Unavailable()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new FlightSearchRequest(
                new AirportReference("IKA"),
                new AirportReference("IST"),
                (FlightTripType)3,
                Departure,
                new FlightPassengerCount(1)));
    }

    [Fact]
    public void Search_Requires_Adult_And_Rejects_Negative_Counts()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new FlightPassengerCount(0));
        Assert.Throws<ArgumentOutOfRangeException>(() => new FlightPassengerCount(1, childCount: -1));
        Assert.Throws<ArgumentOutOfRangeException>(() => new FlightPassengerCount(1, infantCount: -1));
        var passengers = new FlightPassengerCount(1, 1, 1);
        Assert.Equal(3, passengers.TotalCount);
        Assert.Null(typeof(FlightSearchRequest).GetProperty("GivenName"));
        Assert.Null(typeof(FlightSearchRequest).GetProperty("BirthDate"));
        Assert.Null(typeof(FlightSearchRequest).GetProperty("Passport"));
        Assert.Null(typeof(FlightSearchRequest).GetProperty("Nationality"));
        Assert.Null(typeof(FlightSearchRequest).GetProperty("FamilyName"));
    }

    [Fact]
    public async Task Connecting_Search_Result_Is_Accepted()
    {
        var source = FakeFlightSource.Connecting("alpha");
        var service = CreateService(source);
        var result = await service.SearchAsync(OneWayRequest(), source.Key);
        Assert.Equal(2, result.Options[0].Journeys[0].Segments.Count);
        Assert.Equal("AYT", result.Options[0].Journeys[0].Segments[0].Destination.IataCode);
    }

    [Fact]
    public async Task Available_Outcome_Is_Returned()
    {
        var source = FakeFlightSource.Available("alpha");
        var service = CreateService(source);
        var result = await service.CheckAvailabilityAsync(AvailabilityRequest(source.Key, "opt-1"));
        Assert.Equal(FlightOfferAvailabilityOutcome.Available, result.Outcome);
    }

    [Fact]
    public async Task Unavailable_Outcome_Is_Returned()
    {
        var source = FakeFlightSource.Unavailable("alpha");
        var service = CreateService(source);
        var result = await service.CheckAvailabilityAsync(AvailabilityRequest(source.Key, "opt-1"));
        Assert.Equal(FlightOfferAvailabilityOutcome.Unavailable, result.Outcome);
    }

    [Fact]
    public async Task Changed_Outcome_Is_Returned()
    {
        var source = FakeFlightSource.Changed("alpha");
        var service = CreateService(source);
        var result = await service.CheckAvailabilityAsync(AvailabilityRequest(source.Key, "opt-1"));
        Assert.Equal(FlightOfferAvailabilityOutcome.Changed, result.Outcome);
        Assert.NotNull(result.CurrentOption);
    }

    [Fact]
    public async Task Search_Timeout_Returns_Unknown_Not_Empty_Fabrication()
    {
        var source = FakeFlightSource.Timeout("alpha");
        var service = CreateService(source);
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();
        var result = await service.SearchAsync(OneWayRequest(), source.Key, cts.Token);
        Assert.Equal(FlightSearchCompletion.Unknown, result.Completion);
        Assert.Empty(result.Options);
    }

    [Fact]
    public async Task Availability_Timeout_Returns_Unknown_Not_Unavailable()
    {
        var source = FakeFlightSource.Timeout("alpha");
        var service = CreateService(source);
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();
        var result = await service.CheckAvailabilityAsync(AvailabilityRequest(source.Key, "opt-1"), cts.Token);
        Assert.Equal(FlightOfferAvailabilityOutcome.Unknown, result.Outcome);
        Assert.NotEqual(FlightOfferAvailabilityOutcome.Unavailable, result.Outcome);
    }

    [Fact]
    public async Task Cross_Source_Validation_Is_Rejected()
    {
        var liar = FakeFlightSource.CrossSource("alpha", "beta");
        var service = CreateService(liar);
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.CheckAvailabilityAsync(AvailabilityRequest(liar.Key, "opt-1")));
    }

    [Fact]
    public async Task Multiple_Sources_Without_Explicit_Key_Are_Rejected()
    {
        var service = CreateService(FakeFlightSource.Available("alpha"), FakeFlightSource.Available("beta"));
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.SearchAsync(OneWayRequest()));
    }

    [Fact]
    public async Task Unknown_Or_Disabled_Source_Is_Rejected()
    {
        var disabled = FakeFlightSource.Disabled("alpha");
        var service = CreateService(disabled);
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.SearchAsync(OneWayRequest(), disabled.Key));
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.CheckAvailabilityAsync(AvailabilityRequest(new FlightSourceKey("missing"), "opt-1")));
    }

    [Fact]
    public void Duplicate_SourceKey_Is_Rejected_At_Resolver()
    {
        var a = FakeFlightSource.Available("alpha");
        var b = FakeFlightSource.Available("alpha");
        Assert.Throws<ArgumentException>(() =>
            new FlightSearchSourceResolver([a, b]));
        Assert.Throws<ArgumentException>(() =>
            new FlightOfferAvailabilitySourceResolver([a, b]));
    }

    [Fact]
    public async Task Zero_Source_Search_Is_Safe_Empty_Complete()
    {
        var service = CreateService();
        var result = await service.SearchAsync(OneWayRequest());
        Assert.Equal(FlightSearchCompletion.Complete, result.Completion);
        Assert.Null(result.SourceKey);
        Assert.Empty(result.Options);
    }

    [Fact]
    public void Search_Result_Is_Not_FlightBooking_Or_Accepted_Offer()
    {
        Assert.NotEqual(typeof(FlightBooking), typeof(FlightSearchResult));
        Assert.NotEqual(typeof(FlightBooking), typeof(FlightSearchOption));
        Assert.Null(typeof(FlightLiveSearchService).Assembly.GetType("TravelCore.Modules.Flight.Domain.FlightOfferSnapshot"));
        Assert.Null(typeof(FlightLiveSearchService).Assembly.GetType("TravelCore.Modules.Flight.Domain.FlightBookingMonetarySnapshot"));
        Assert.Null(typeof(FlightLiveSearchService).Assembly.GetType("TravelCore.Modules.Flight.Domain.FlightAvailabilityHold"));
        Assert.Null(typeof(FlightLiveSearchService).Assembly.GetType("TravelCore.Modules.Flight.Domain.SeatInventory"));
    }

    private static FlightLiveSearchService CreateService(params FakeFlightSource[] sources)
    {
        return new FlightLiveSearchService(
            new FlightSearchSourceResolver(sources),
            new FlightOfferAvailabilitySourceResolver(sources),
            new FixedClock(Observed));
    }

    private static FlightSearchRequest OneWayRequest() =>
        new(
            new AirportReference("ika"),
            new AirportReference("IST"),
            FlightTripType.OneWay,
            Departure,
            new FlightPassengerCount(1, 1, 0));

    private static FlightSearchRequest RoundTripRequest() =>
        new(
            new AirportReference("IKA"),
            new AirportReference("IST"),
            FlightTripType.RoundTrip,
            Departure,
            new FlightPassengerCount(1),
            Return);

    private static FlightOfferAvailabilityRequest AvailabilityRequest(FlightSourceKey sourceKey, string optionRef) =>
        new(sourceKey, optionRef, new FlightPassengerCount(1));

    private sealed class FixedClock(Instant instant) : IClock
    {
        public Instant GetCurrentInstant() => instant;
    }

    private sealed class FakeFlightSource : IFlightSearchSource, IFlightOfferAvailabilitySource
    {
        private readonly string _mode;

        private FakeFlightSource(string key, string mode, params FlightSourceCapability[] capabilities)
        {
            Key = new FlightSourceKey(key);
            _mode = mode;
            Capabilities = capabilities.ToHashSet();
        }

        public FlightSourceKey Key { get; }

        public IReadOnlySet<FlightSourceCapability> Capabilities { get; }

        public static FakeFlightSource Available(string key) =>
            new(key, "available", FlightSourceCapability.Search, FlightSourceCapability.AvailabilityCheck);

        public static FakeFlightSource Unavailable(string key) =>
            new(key, "unavailable", FlightSourceCapability.Search, FlightSourceCapability.AvailabilityCheck);

        public static FakeFlightSource Changed(string key) =>
            new(key, "changed", FlightSourceCapability.Search, FlightSourceCapability.AvailabilityCheck);

        public static FakeFlightSource Timeout(string key) =>
            new(key, "timeout", FlightSourceCapability.Search, FlightSourceCapability.AvailabilityCheck);

        public static FakeFlightSource Connecting(string key) =>
            new(key, "connecting", FlightSourceCapability.Search, FlightSourceCapability.AvailabilityCheck);

        public static FakeFlightSource Disabled(string key) =>
            new(key, "disabled");

        public static FakeFlightSource CrossSource(string key, string otherKey) =>
            new(key, $"cross:{otherKey}", FlightSourceCapability.Search, FlightSourceCapability.AvailabilityCheck);

        public Task<FlightSearchResult> SearchAsync(FlightSearchRequest request, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var option = _mode == "connecting" ? ConnectingOption(request) : Option(request, "opt-1");
            return Task.FromResult(FlightSearchResult.Complete(Key, Observed, [option]));
        }

        public Task<FlightOfferAvailabilityResult> CheckAvailabilityAsync(
            FlightOfferAvailabilityRequest request,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (_mode.StartsWith("cross:", StringComparison.Ordinal))
            {
                var other = new FlightSourceKey(_mode["cross:".Length..]);
                return Task.FromResult(new FlightOfferAvailabilityResult(
                    FlightOfferAvailabilityOutcome.Available,
                    other,
                    request.SourceOptionReference,
                    Observed));
            }

            if (request.SourceKey.Value != Key.Value)
            {
                throw new InvalidOperationException("Cross-source availability validation is forbidden.");
            }

            var outcome = _mode switch
            {
                "unavailable" => FlightOfferAvailabilityOutcome.Unavailable,
                "changed" => FlightOfferAvailabilityOutcome.Changed,
                _ => FlightOfferAvailabilityOutcome.Available,
            };

            FlightSearchOption? current = outcome == FlightOfferAvailabilityOutcome.Changed
                ? Option(OneWayRequest(), "opt-2")
                : null;

            return Task.FromResult(new FlightOfferAvailabilityResult(
                outcome,
                Key,
                request.SourceOptionReference,
                Observed,
                currentOption: current));
        }

        private FlightSearchOption Option(FlightSearchRequest request, string optionRef)
        {
            var outbound = new FlightSearchJourney(
                1,
                [Segment(1, request.Origin, request.Destination, Dep, Arr)]);
            IReadOnlyList<FlightSearchJourney> journeys = request.TripType == FlightTripType.RoundTrip
                ? [outbound, new FlightSearchJourney(2, [Segment(1, request.Destination, request.Origin, RetDep, RetArr)])]
                : [outbound];
            return new FlightSearchOption(Key, optionRef, request.TripType, journeys, Observed);
        }

        private FlightSearchOption ConnectingOption(FlightSearchRequest request)
        {
            var via = new AirportReference("AYT");
            var mid = Instant.FromUtc(2026, 9, 1, 8, 0);
            var journey = new FlightSearchJourney(
                1,
                [
                    Segment(1, request.Origin, via, Dep, mid),
                    Segment(2, via, request.Destination, mid.Plus(Duration.FromMinutes(45)), Arr),
                ]);
            return new FlightSearchOption(Key, "opt-1", FlightTripType.OneWay, [journey], Observed);
        }

        private static FlightSearchSegment Segment(
            int ordinal,
            AirportReference origin,
            AirportReference destination,
            Instant departureAt,
            Instant arrivalAt) =>
            new(
                ordinal,
                origin,
                destination,
                new AirlineReference("TK"),
                departureAt,
                "Asia/Tehran",
                arrivalAt,
                "Europe/Istanbul",
                new AirlineReference("TK"),
                "123");
    }
}
