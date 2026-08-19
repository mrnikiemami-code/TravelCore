using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using Npgsql;
using TravelCore.Modules.Flight.Contracts;
using TravelCore.Modules.Flight.Domain;
using TravelCore.Modules.Payment.Contracts;
using FlightBookingAggregate = TravelCore.Modules.Flight.Domain.FlightBooking;

namespace TravelCore.Modules.Flight.Infrastructure.Services;

/// <summary>
/// Public FlightBooking search, initiation, authorized reads, offer/reservation progression, and R7 cancellation (P22-R8).
/// Initiation creates Pending only. Sources and Payment provider remain server-controlled.
/// </summary>
public sealed class PublicFlightBookingSurfaceService :
    IPublicFlightBookingSearchService,
    IPublicFlightBookingInitiationService,
    IPublicFlightBookingReadService,
    IPublicFlightBookingJourneyService
{
    private readonly FlightDbContext _db;
    private readonly FlightLiveSearchService _search;
    private readonly IFlightSearchSourceResolver _searchResolver;
    private readonly IFlightOfferSourceResolver _offers;
    private readonly IFlightReservationSourceResolver _reservations;
    private readonly FlightOfferAcceptanceService _offerAcceptance;
    private readonly FlightSupplierReservationService _reservationService;
    private readonly FlightBookingCancellationService _cancellations;
    private readonly IPublicFlightBookingPaymentService _payments;
    private readonly IClock _clock;

    public PublicFlightBookingSurfaceService(
        FlightDbContext db,
        FlightLiveSearchService search,
        IFlightSearchSourceResolver searchResolver,
        IFlightOfferSourceResolver offers,
        IFlightReservationSourceResolver reservations,
        FlightOfferAcceptanceService offerAcceptance,
        FlightSupplierReservationService reservationService,
        FlightBookingCancellationService cancellations,
        IPublicFlightBookingPaymentService payments,
        IClock clock)
    {
        ArgumentNullException.ThrowIfNull(db);
        ArgumentNullException.ThrowIfNull(search);
        ArgumentNullException.ThrowIfNull(searchResolver);
        ArgumentNullException.ThrowIfNull(offers);
        ArgumentNullException.ThrowIfNull(reservations);
        ArgumentNullException.ThrowIfNull(offerAcceptance);
        ArgumentNullException.ThrowIfNull(reservationService);
        ArgumentNullException.ThrowIfNull(cancellations);
        ArgumentNullException.ThrowIfNull(payments);
        ArgumentNullException.ThrowIfNull(clock);
        _db = db;
        _search = search;
        _searchResolver = searchResolver;
        _offers = offers;
        _reservations = reservations;
        _offerAcceptance = offerAcceptance;
        _reservationService = reservationService;
        _cancellations = cancellations;
        _payments = payments;
        _clock = clock;
    }

    public async Task<PublicFlightSearchResultRead> SearchAsync(
        PublicFlightSearchRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var configured = _searchResolver.ListConfiguredKeys().Count > 0;
        var domainRequest = ToSearchRequest(request);
        var result = await _search.SearchAsync(domainRequest, sourceKey: null, cancellationToken);
        return PublicFlightBookingMapper.ToSearch(result, configured);
    }

    public async Task<PublicFlightBookingInitiationResponse> InitiateAsync(
        PublicFlightBookingInitiationRequest request,
        Guid? actorId,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.Journeys);
        ArgumentNullException.ThrowIfNull(request.Passengers);

        var idempotencyKey = FlightBookingPublicIdempotencyRecord.NormalizeIdempotencyKey(
            request.IdempotencyKey ?? string.Empty);
        if (!Enum.TryParse<FlightTripType>(request.TripType, ignoreCase: true, out var tripType)
            || !Enum.IsDefined(tripType))
        {
            throw new ArgumentException("FlightTripType is not controlled. MultiCity is DEFERRED.", nameof(request));
        }

        var journeys = ParseJourneys(request.Journeys);
        var passengers = ParsePassengers(request.Passengers);

        var existing = await FindByIdempotencyAsync(idempotencyKey, cancellationToken);
        if (existing is not null)
        {
            return existing;
        }

        var now = _clock.GetCurrentInstant();
        await using var tx = await _db.Database.BeginTransactionAsync(cancellationToken);
        await AcquireIdempotencyLockAsync(idempotencyKey, cancellationToken);

        var raced = await FindByIdempotencyAsync(idempotencyKey, cancellationToken);
        if (raced is not null)
        {
            await tx.CommitAsync(cancellationToken);
            return raced;
        }

        var booking = FlightBookingAggregate.Create(tripType, journeys, passengers);
        if (actorId is { } id)
        {
            booking.AttachActorAccount(id);
        }

        var rawToken = FlightBookingAccessToken.CreateRaw();
        _db.FlightBookings.Add(booking);
        _db.AccessCredentials.Add(
            FlightBookingAccessCredential.Create(booking.Id, FlightBookingAccessToken.Hash(rawToken), now));
        _db.PublicIdempotency.Add(
            FlightBookingPublicIdempotencyRecord.Create(idempotencyKey, booking.Id, now));

        try
        {
            await _db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (IsUniqueViolation(ex))
        {
            _db.ChangeTracker.Clear();
            await tx.RollbackAsync(cancellationToken);
            return await FindByIdempotencyAsync(idempotencyKey, cancellationToken)
                ?? throw new InvalidOperationException("Concurrent FlightBooking initiation did not converge.");
        }

        await tx.CommitAsync(cancellationToken);
        return PublicFlightBookingMapper.ToInitiation(booking, rawToken);
    }

    public async Task<PublicFlightBookingRead?> GetAuthorizedAsync(
        Guid flightBookingId,
        string? accessToken,
        Guid? actorId,
        CancellationToken cancellationToken = default)
    {
        var booking = await LoadAuthorizedAsync(flightBookingId, accessToken, actorId, cancellationToken);
        return booking is null ? null : await ComposeReadAsync(booking, cancellationToken);
    }

    public async Task<PublicFlightBookingProgressResult> AcceptOfferAsync(
        Guid flightBookingId,
        string? accessToken,
        Guid? actorId,
        string? idempotencyKey,
        CancellationToken cancellationToken = default)
    {
        var booking = await LoadAuthorizedAsync(flightBookingId, accessToken, actorId, cancellationToken)
            ?? throw new UnauthorizedAccessException();
        if (_offers.ListConfiguredKeys().Count == 0)
        {
            return new PublicFlightBookingProgressResult(
                PublicFlightBookingJourneyStatus.SourceUnavailable,
                await ComposeReadAsync(booking, cancellationToken));
        }

        var key = string.IsNullOrWhiteSpace(idempotencyKey)
            ? $"offer:{booking.Id.Value:D}"
            : FlightBookingPublicIdempotencyRecord.NormalizeIdempotencyKey(idempotencyKey);
        try
        {
            await _offerAcceptance.AcceptAsync(
                booking.Id,
                key,
                requestedSourceKey: null,
                previouslyObservedTotal: null,
                sourceOfferReference: null,
                cancellationToken);
        }
        catch (InvalidOperationException ex)
        {
            booking = await LoadBookingAsync(booking.Id, cancellationToken)
                ?? booking;
            return new PublicFlightBookingProgressResult(
                PublicFlightBookingMapper.MapOfferException(ex),
                await ComposeReadAsync(booking, cancellationToken));
        }

        booking = await LoadBookingAsync(booking.Id, cancellationToken)
            ?? throw new InvalidOperationException("FlightBooking was not found.");
        return new PublicFlightBookingProgressResult(
            PublicFlightBookingJourneyStatus.Completed,
            await ComposeReadAsync(booking, cancellationToken));
    }

    public async Task<PublicFlightBookingProgressResult> RequestReservationAsync(
        Guid flightBookingId,
        string? accessToken,
        Guid? actorId,
        string? idempotencyKey,
        CancellationToken cancellationToken = default)
    {
        var booking = await LoadAuthorizedAsync(flightBookingId, accessToken, actorId, cancellationToken)
            ?? throw new UnauthorizedAccessException();
        if (_reservations.ListConfiguredKeys().Count == 0)
        {
            return new PublicFlightBookingProgressResult(
                PublicFlightBookingJourneyStatus.SourceUnavailable,
                await ComposeReadAsync(booking, cancellationToken));
        }

        var key = string.IsNullOrWhiteSpace(idempotencyKey)
            ? $"reservation:{booking.Id.Value:D}"
            : FlightBookingPublicIdempotencyRecord.NormalizeIdempotencyKey(idempotencyKey);
        try
        {
            await _reservationService.InitiateAsync(booking.Id, key, requestedSourceKey: null, cancellationToken);
        }
        catch (InvalidOperationException ex)
        {
            booking = await LoadBookingAsync(booking.Id, cancellationToken)
                ?? booking;
            var status = ex.Message.Contains("unconfigured", StringComparison.OrdinalIgnoreCase)
                || ex.Message.Contains("cannot be fabricated", StringComparison.OrdinalIgnoreCase)
                ? PublicFlightBookingJourneyStatus.SourceUnavailable
                : PublicFlightBookingJourneyStatus.Ineligible;
            return new PublicFlightBookingProgressResult(status, await ComposeReadAsync(booking, cancellationToken));
        }

        booking = await LoadBookingAsync(booking.Id, cancellationToken)
            ?? throw new InvalidOperationException("FlightBooking was not found.");
        return new PublicFlightBookingProgressResult(
            PublicFlightBookingJourneyStatus.Completed,
            await ComposeReadAsync(booking, cancellationToken));
    }

    public async Task<PublicFlightBookingCancellationCommandResult?> RequestCancellationAsync(
        Guid flightBookingId,
        string? accessToken,
        Guid? actorId,
        string? idempotencyKey,
        CancellationToken cancellationToken = default)
    {
        var booking = await LoadAuthorizedAsync(flightBookingId, accessToken, actorId, cancellationToken);
        if (booking is null)
        {
            return null;
        }

        var key = string.IsNullOrWhiteSpace(idempotencyKey)
            ? $"cancel:{booking.Id.Value:D}"
            : FlightBookingPublicIdempotencyRecord.NormalizeIdempotencyKey(idempotencyKey);
        var result = await _cancellations.RequestAsync(booking.Id, key, cancellationToken);
        booking = await LoadBookingAsync(booking.Id, cancellationToken)
            ?? throw new InvalidOperationException("FlightBooking was not found.");
        return new PublicFlightBookingCancellationCommandResult(
            result.Outcome.ToString(),
            await ComposeReadAsync(booking, cancellationToken));
    }

    private async Task<PublicFlightBookingRead> ComposeReadAsync(
        FlightBookingAggregate booking,
        CancellationToken cancellationToken)
    {
        var offer = await _db.FlightOfferSnapshots
            .AsNoTracking()
            .Include(x => x.Monetary)
            .Include(x => x.FareRules)
            .ThenInclude(x => x.Baggage)
            .SingleOrDefaultAsync(x => x.FlightBookingId == booking.Id, cancellationToken);
        var reservation = await _db.FlightSupplierReservations
            .AsNoTracking()
            .Where(x => x.FlightBookingId == booking.Id)
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);
        var tickets = await _db.FlightTickets
            .AsNoTracking()
            .Where(x => x.FlightBookingId == booking.Id)
            .ToListAsync(cancellationToken);
        var cancellation = await _db.FlightBookingCancellations
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.FlightBookingId == booking.Id, cancellationToken);
        var issues = await _db.FlightReconciliationIssues
            .AsNoTracking()
            .Where(x => x.FlightBookingId == booking.Id)
            .ToListAsync(cancellationToken);
        PublicPaymentRead? payment = null;
        try
        {
            payment = await _payments.GetByFlightBookingIdAsync(booking.Id.Value, cancellationToken);
        }
        catch (InvalidOperationException)
        {
            // Payment row may be absent until first scoped read; keep booking readable.
        }

        return PublicFlightBookingMapper.ToRead(
            new PublicFlightBookingFacts(
                booking,
                offer,
                reservation,
                tickets,
                cancellation,
                issues,
                payment,
                _clock.GetCurrentInstant()));
    }

    private async Task<FlightBookingAggregate?> LoadAuthorizedAsync(
        Guid flightBookingId,
        string? accessToken,
        Guid? actorId,
        CancellationToken cancellationToken)
    {
        if (flightBookingId == Guid.Empty)
        {
            return null;
        }

        var id = FlightBookingId.From(flightBookingId);
        if (!await IsAuthorizedAsync(id, accessToken, actorId, cancellationToken))
        {
            return null;
        }

        return await LoadBookingAsync(id, cancellationToken);
    }

    private async Task<bool> IsAuthorizedAsync(
        FlightBookingId flightBookingId,
        string? accessToken,
        Guid? actorId,
        CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(accessToken))
        {
            var hash = FlightBookingAccessToken.Hash(accessToken);
            var match = await _db.AccessCredentials
                .AsNoTracking()
                .AnyAsync(x => x.FlightBookingId == flightBookingId && x.TokenHash == hash, cancellationToken);
            if (match)
            {
                return true;
            }
        }

        if (actorId is { } id)
        {
            var booking = await _db.FlightBookings
                .AsNoTracking()
                .SingleOrDefaultAsync(x => x.Id == flightBookingId, cancellationToken);
            return booking?.ActorAccountId == id;
        }

        return false;
    }

    private Task<FlightBookingAggregate?> LoadBookingAsync(
        FlightBookingId flightBookingId,
        CancellationToken cancellationToken) =>
        _db.FlightBookings
            .Include(x => x.Journeys)
            .ThenInclude(x => x.Segments)
            .Include(x => x.Passengers)
            .SingleOrDefaultAsync(x => x.Id == flightBookingId, cancellationToken);

    private async Task<PublicFlightBookingInitiationResponse?> FindByIdempotencyAsync(
        string idempotencyKey,
        CancellationToken cancellationToken)
    {
        var record = await _db.PublicIdempotency
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.IdempotencyKey == idempotencyKey, cancellationToken);
        if (record is null)
        {
            return null;
        }

        var booking = await LoadBookingAsync(record.FlightBookingId, cancellationToken)
            ?? throw new InvalidOperationException("Idempotent FlightBooking was not found.");
        return PublicFlightBookingMapper.ToInitiation(booking, rawAccessToken: null);
    }

    private Task AcquireIdempotencyLockAsync(string idempotencyKey, CancellationToken cancellationToken)
    {
        if (_db.Database.ProviderName is null
            || !_db.Database.ProviderName.Contains("Npgsql", StringComparison.OrdinalIgnoreCase))
        {
            return Task.CompletedTask;
        }

        var digest = SHA256.HashData(Encoding.UTF8.GetBytes("flight-booking:" + idempotencyKey));
        var key1 = BitConverter.ToInt32(digest, 0);
        var key2 = BitConverter.ToInt32(digest, 4);
        return _db.Database.ExecuteSqlInterpolatedAsync(
            $"SELECT pg_advisory_xact_lock({key1}, {key2})",
            cancellationToken);
    }

    private static bool IsUniqueViolation(DbUpdateException exception)
    {
        for (Exception? current = exception; current is not null; current = current.InnerException)
        {
            if (current is PostgresException postgres
                && postgres.SqlState == PostgresErrorCodes.UniqueViolation)
            {
                return true;
            }
        }

        return false;
    }

    private static FlightSearchRequest ToSearchRequest(PublicFlightSearchRequest request)
    {
        if (!Enum.TryParse<FlightTripType>(request.TripType, ignoreCase: true, out var tripType)
            || !Enum.IsDefined(tripType))
        {
            throw new ArgumentException("FlightTripType is not controlled. MultiCity is DEFERRED.", nameof(request));
        }

        return new FlightSearchRequest(
            new AirportReference(request.OriginIata),
            new AirportReference(request.DestinationIata),
            tripType,
            ToLocalDate(request.DepartureDate),
            new FlightPassengerCount(request.AdultCount, request.ChildCount, request.InfantCount),
            request.ReturnDate is { } ret ? ToLocalDate(ret) : null);
    }

    private static LocalDate ToLocalDate(DateOnly date) => new(date.Year, date.Month, date.Day);

    private static IReadOnlyList<FlightJourneySpecification> ParseJourneys(
        IReadOnlyList<PublicFlightJourneyInput> journeys)
    {
        if (journeys.Count == 0)
        {
            throw new ArgumentException("At least one journey is required.", nameof(journeys));
        }

        var parsed = new List<FlightJourneySpecification>(journeys.Count);
        foreach (var journey in journeys)
        {
            ArgumentNullException.ThrowIfNull(journey);
            ArgumentNullException.ThrowIfNull(journey.Segments);
            if (journey.Segments.Count == 0)
            {
                throw new ArgumentException("Each journey requires at least one segment.", nameof(journeys));
            }

            var segments = new List<FlightSegmentSpecification>(journey.Segments.Count);
            foreach (var segment in journey.Segments)
            {
                ArgumentNullException.ThrowIfNull(segment);
                segments.Add(
                    new FlightSegmentSpecification(
                        new AirportReference(segment.OriginIata),
                        new AirportReference(segment.DestinationIata),
                        Instant.FromDateTimeOffset(segment.DepartureAt),
                        segment.DepartureTimeZoneId,
                        Instant.FromDateTimeOffset(segment.ArrivalAt),
                        segment.ArrivalTimeZoneId,
                        new AirlineReference(segment.MarketingCarrierIata),
                        string.IsNullOrWhiteSpace(segment.OperatingCarrierIata)
                            ? null
                            : new AirlineReference(segment.OperatingCarrierIata),
                        segment.FlightNumber));
            }

            parsed.Add(new FlightJourneySpecification(segments));
        }

        return parsed;
    }

    private static IReadOnlyList<FlightPassengerSpecification> ParsePassengers(
        IReadOnlyList<PublicFlightPassengerInput> passengers)
    {
        if (passengers.Count == 0)
        {
            throw new ArgumentException("At least one passenger is required.", nameof(passengers));
        }

        var parsed = new List<FlightPassengerSpecification>(passengers.Count);
        foreach (var passenger in passengers)
        {
            ArgumentNullException.ThrowIfNull(passenger);
            if (!Enum.TryParse<FlightPassengerCategory>(passenger.Category, ignoreCase: true, out var category)
                || !Enum.IsDefined(category))
            {
                throw new ArgumentException("FlightPassengerCategory is not controlled.", nameof(passengers));
            }

            parsed.Add(new FlightPassengerSpecification(passenger.GivenName, passenger.FamilyName, category));
        }

        return parsed;
    }
}
