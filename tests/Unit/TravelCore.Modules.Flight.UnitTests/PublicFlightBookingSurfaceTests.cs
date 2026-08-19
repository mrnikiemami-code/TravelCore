using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using NodaTime;
using TravelCore.Modules.Booking.Contracts;
using TravelCore.Modules.Flight.Contracts;
using TravelCore.Modules.Flight.Domain;
using TravelCore.Modules.Flight.Infrastructure;
using TravelCore.Modules.Flight.Infrastructure.Cancellations;
using TravelCore.Modules.Flight.Infrastructure.Reservations;
using TravelCore.Modules.Flight.Infrastructure.Search;
using TravelCore.Modules.Flight.Infrastructure.Services;
using TravelCore.Modules.HotelBooking.Contracts;
using TravelCore.Modules.Payment.Contracts;
using Xunit;
using FlightBookingAggregate = TravelCore.Modules.Flight.Domain.FlightBooking;

namespace TravelCore.Modules.Flight.UnitTests;

public sealed class PublicFlightBookingSurfaceTests
{
    private static readonly Instant Observed = Instant.FromUtc(2026, 8, 18, 12, 0);
    private static readonly Instant Dep = Instant.FromUtc(2026, 9, 1, 6, 0);
    private static readonly Instant Arr = Instant.FromUtc(2026, 9, 1, 10, 0);
    private static readonly Instant ConnDep = Instant.FromUtc(2026, 9, 1, 12, 0);
    private static readonly Instant ConnArr = Instant.FromUtc(2026, 9, 1, 16, 0);
    private static readonly Instant RetDep = Instant.FromUtc(2026, 9, 8, 8, 0);
    private static readonly Instant RetArr = Instant.FromUtc(2026, 9, 8, 12, 0);

    [Fact]
    public async Task Zero_Source_Search_Is_Truthful_Unavailable()
    {
        await using var db = CreateDb();
        var surface = CreateSurface(db);
        var result = await surface.SearchAsync(OneWaySearch());
        Assert.False(result.SourceConfigured);
        Assert.Empty(result.Options);
        Assert.Equal(nameof(FlightSearchCompletion.Complete), result.Completion);
        Assert.Contains("not currently available", result.SafeMessage, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Search_Returns_OneWay_RoundTrip_And_Connecting()
    {
        await using var db = CreateDb();
        var oneWay = await CreateSurface(db, FakeFlightSource.Direct("alpha")).SearchAsync(OneWaySearch());
        Assert.True(oneWay.SourceConfigured);
        Assert.Single(oneWay.Options);
        Assert.Equal("OneWay", oneWay.Options[0].TripType);
        Assert.Single(oneWay.Options[0].Journeys);
        Assert.Single(oneWay.Options[0].Journeys[0].Segments);

        var roundTrip = await CreateSurface(db, FakeFlightSource.Direct("alpha")).SearchAsync(RoundTripSearch());
        Assert.Equal(2, roundTrip.Options[0].Journeys.Count);

        var connecting = await CreateSurface(db, FakeFlightSource.Connecting("alpha")).SearchAsync(OneWaySearch());
        Assert.Equal(2, connecting.Options[0].Journeys[0].Segments.Count);
        Assert.Equal("AYT", connecting.Options[0].Journeys[0].Segments[0].DestinationIata);
    }

    [Fact]
    public async Task Initiation_Is_Idempotent_And_Returns_Raw_Token_Once()
    {
        await using var db = CreateDb();
        var surface = CreateSurface(db);
        var request = InitiationRequest("key-1");
        var first = await surface.InitiateAsync(request, actorId: null);
        var replay = await surface.InitiateAsync(request, actorId: null);
        Assert.Equal(first.FlightBookingId, replay.FlightBookingId);
        Assert.False(string.IsNullOrWhiteSpace(first.AccessToken));
        Assert.True(first.AccessTokenIssued);
        Assert.Null(replay.AccessToken);
        Assert.False(replay.AccessTokenIssued);
        Assert.Equal("Pending", first.Status);
        Assert.False(first.Confirmed);
        var hash = await db.AccessCredentials.Select(x => x.TokenHash).SingleAsync();
        Assert.NotEqual(first.AccessToken, hash);
        Assert.Equal(64, hash.Length);
    }

    [Fact]
    public async Task Missing_Token_Does_Not_Authorize_Read()
    {
        await using var db = CreateDb();
        var surface = CreateSurface(db);
        var created = await surface.InitiateAsync(InitiationRequest("key-2"), actorId: null);
        var missing = await surface.GetAuthorizedAsync(created.FlightBookingId, accessToken: null, actorId: null);
        var wrong = await surface.GetAuthorizedAsync(created.FlightBookingId, "not-the-token", actorId: null);
        var ok = await surface.GetAuthorizedAsync(created.FlightBookingId, created.AccessToken, actorId: null);
        Assert.Null(missing);
        Assert.Null(wrong);
        Assert.NotNull(ok);
        Assert.False(ok!.Confirmed);
        Assert.All(ok.Passengers, p => Assert.False(string.IsNullOrWhiteSpace(p.GivenName)));
        Assert.Null(typeof(PublicFlightPassengerRead).GetProperty("BirthDate"));
        Assert.Null(typeof(PublicFlightPassengerRead).GetProperty("Passport"));
    }

    [Fact]
    public async Task Zero_Offer_And_Reservation_Sources_Are_Unavailable()
    {
        await using var db = CreateDb();
        var surface = CreateSurface(db);
        var created = await surface.InitiateAsync(InitiationRequest("key-3"), actorId: null);
        var offer = await surface.AcceptOfferAsync(created.FlightBookingId, created.AccessToken, null, null);
        Assert.Equal(PublicFlightBookingJourneyStatus.SourceUnavailable, offer.Status);
        var reservation = await surface.RequestReservationAsync(
            created.FlightBookingId, created.AccessToken, null, null);
        Assert.Equal(PublicFlightBookingJourneyStatus.SourceUnavailable, reservation.Status);
        Assert.Equal(0, await db.FlightOfferSnapshots.CountAsync());
        Assert.Equal(0, await db.FlightSupplierReservations.CountAsync());
    }

    [Fact]
    public void Presentation_Keeps_Pnr_Payment_Ticket_And_Booking_Distinct()
    {
        var booking = OneWayBooking();
        var reservation = FlightSupplierReservation.StartPending(booking.Id, "test-source", Observed);
        var attempt = reservation.StartAttempt(Observed);
        reservation.MarkAttemptInitiated(attempt.Id, Observed.Plus(NodaTime.Duration.FromSeconds(1)));
        reservation.ConfirmAttempt(
            attempt.Id,
            Observed.Plus(NodaTime.Duration.FromMinutes(1)),
            "src-res-1",
            "ABC123",
            Observed.Plus(NodaTime.Duration.FromDays(2)),
            Identities(booking),
            Identities(booking),
            Passengers(booking),
            Passengers(booking));
        var pendingTicket = FlightTicket.StartPending(booking.Id, booking.Passengers[0].Id, "test-source", Observed);
        var payment = new PublicPaymentRead(
            Guid.CreateVersion7(),
            "Succeeded",
            1_000_000m,
            "IRR",
            false,
            "Succeeded",
            null,
            "Succeeded",
            null);
        var read = PublicFlightBookingMapper.ToRead(
            new PublicFlightBookingFacts(
                booking,
                Offer: null,
                reservation,
                [pendingTicket],
                Cancellation: null,
                [],
                payment,
                Observed));
        Assert.Equal(PublicFlightBookingPresentationStates.TicketingPending, read.PresentationState);
        Assert.False(read.Confirmed);
        Assert.Equal("Pending", read.Status);
        Assert.Equal(
            PublicFlightBookingPresentationStates.ReservationConfirmed,
            read.Reservation!.PresentationStatus);
        Assert.Equal("Pending", read.Tickets[0].Status);
        Assert.Null(read.Tickets[0].TicketNumber);
    }

    [Fact]
    public void Partial_Tickets_Are_Not_Confirmed()
    {
        var booking = TwoPassengerBooking();
        var issued = FlightTicket.StartPending(booking.Id, booking.Passengers[0].Id, "test-source", Observed);
        issued.MarkIssued("125-001", Observed.Plus(NodaTime.Duration.FromMinutes(1)));
        var pending = FlightTicket.StartPending(booking.Id, booking.Passengers[1].Id, "test-source", Observed);
        var payment = new PublicPaymentRead(
            Guid.CreateVersion7(),
            "Succeeded",
            1_000_000m,
            "IRR",
            false,
            "Succeeded",
            null,
            "Succeeded",
            null);
        var read = PublicFlightBookingMapper.ToRead(
            new PublicFlightBookingFacts(
                booking,
                Offer: null,
                Reservation: null,
                [issued, pending],
                Cancellation: null,
                [],
                payment,
                Observed));
        Assert.False(read.Confirmed);
        Assert.Equal(PublicFlightBookingPresentationStates.TicketingPending, read.PresentationState);
        Assert.Equal("Issued", read.Tickets.Single(t => t.PassengerId == booking.Passengers[0].Id.Value).Status);
        Assert.Equal("125-001", read.Tickets.Single(t => t.PassengerId == booking.Passengers[0].Id.Value).TicketNumber);
    }

    [Fact]
    public void Access_Token_Is_Independent_Of_Tour_And_Hotel()
    {
        Assert.NotEqual(
            PublicFlightBookingCompositionBoundary.AccessTokenHeader,
            PublicBookingCompositionBoundary.AccessTokenHeader);
        Assert.NotEqual(
            PublicFlightBookingCompositionBoundary.AccessTokenHeader,
            PublicHotelBookingCompositionBoundary.AccessTokenHeader);
        var raw = FlightBookingAccessToken.CreateRaw();
        Assert.NotEqual(raw, FlightBookingAccessToken.Hash(raw));
    }

    private static PublicFlightBookingSurfaceService CreateSurface(
        FlightDbContext db,
        params FakeFlightSource[] sources)
    {
        var searchResolver = new FlightSearchSourceResolver(sources);
        var availabilityResolver = new FlightOfferAvailabilitySourceResolver(sources);
        var clock = new FixedClock(Observed);
        return new PublicFlightBookingSurfaceService(
            db,
            new FlightLiveSearchService(searchResolver, availabilityResolver, clock),
            searchResolver,
            new FlightOfferSourceResolver([]),
            new FlightReservationSourceResolver([]),
            new FlightOfferAcceptanceService(db, new FlightOfferSourceResolver([]), clock),
            new FlightSupplierReservationService(db, new FlightReservationSourceResolver([]), clock),
            new FlightBookingCancellationService(db, new FlightCancellationSourceResolver([]), clock),
            new MissingPaymentService(),
            clock);
    }

    private static FlightDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<FlightDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;
        return new FlightDbContext(options);
    }

    private static PublicFlightSearchRequest OneWaySearch() =>
        new("IKA", "IST", "OneWay", new DateOnly(2026, 9, 1), null, 1, 0, 0);

    private static PublicFlightSearchRequest RoundTripSearch() =>
        new("IKA", "IST", "RoundTrip", new DateOnly(2026, 9, 1), new DateOnly(2026, 9, 8), 1, 0, 0);

    private static PublicFlightBookingInitiationRequest InitiationRequest(string key) =>
        new(
            "OneWay",
            [
                new PublicFlightJourneyInput(
                [
                    new PublicFlightSegmentInput(
                        "THR",
                        "LHR",
                        Dep.ToDateTimeOffset(),
                        "Asia/Tehran",
                        Arr.ToDateTimeOffset(),
                        "Europe/London",
                        "TK",
                        null,
                        "TK800"),
                ]),
            ],
            [new PublicFlightPassengerInput("Ada", "Lovelace", "Adult")],
            key);

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

    private sealed class FixedClock(Instant instant) : IClock
    {
        public Instant GetCurrentInstant() => instant;
    }

    private sealed class MissingPaymentService : IPublicFlightBookingPaymentService
    {
        public Task<PublicPaymentRead> GetByFlightBookingIdAsync(
            Guid flightBookingId,
            CancellationToken cancellationToken = default) =>
            throw new InvalidOperationException("Payment is absent.");

        public Task<PublicPaymentCommandResult> InitiateForFlightBookingAsync(
            Guid flightBookingId,
            string? idempotencyKey,
            CancellationToken cancellationToken = default) =>
            throw new InvalidOperationException("Payment is absent.");
    }

    private sealed class FakeFlightSource : IFlightSearchSource, IFlightOfferAvailabilitySource
    {
        private readonly string _mode;

        private FakeFlightSource(string key, string mode)
        {
            Key = new FlightSourceKey(key);
            _mode = mode;
            Capabilities = new HashSet<FlightSourceCapability>
            {
                FlightSourceCapability.Search,
                FlightSourceCapability.AvailabilityCheck,
            };
        }

        public FlightSourceKey Key { get; }

        public IReadOnlySet<FlightSourceCapability> Capabilities { get; }

        public static FakeFlightSource Direct(string key) => new(key, "direct");

        public static FakeFlightSource Connecting(string key) => new(key, "connecting");

        public Task<FlightSearchResult> SearchAsync(
            FlightSearchRequest request,
            CancellationToken cancellationToken = default)
        {
            var outbound = _mode == "connecting"
                ? new FlightSearchJourney(
                    1,
                    [
                        Segment(1, request.Origin, new AirportReference("AYT"), Dep, Arr),
                        Segment(2, new AirportReference("AYT"), request.Destination, ConnDep, ConnArr),
                    ])
                : new FlightSearchJourney(
                    1,
                    [Segment(1, request.Origin, request.Destination, Dep, Arr)]);
            var journeys = request.TripType == FlightTripType.RoundTrip
                ? new[]
                {
                    outbound,
                    new FlightSearchJourney(
                        2,
                        [Segment(1, request.Destination, request.Origin, RetDep, RetArr)]),
                }
                : new[] { outbound };
            var option = new FlightSearchOption(
                Key,
                "opt-1",
                request.TripType,
                journeys,
                Observed);
            return Task.FromResult(FlightSearchResult.Complete(Key, Observed, [option]));
        }

        public Task<FlightOfferAvailabilityResult> CheckAvailabilityAsync(
            FlightOfferAvailabilityRequest request,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(new FlightOfferAvailabilityResult(
                FlightOfferAvailabilityOutcome.Available,
                request.SourceKey,
                request.SourceOptionReference,
                Observed));

        private static FlightSearchSegment Segment(
            int ordinal,
            AirportReference origin,
            AirportReference destination,
            Instant dep,
            Instant arr) =>
            new(ordinal, origin, destination, new AirlineReference("TK"), dep, "Asia/Tehran", arr, "Europe/Istanbul");
    }
}
