using Microsoft.EntityFrameworkCore;
using NodaTime;
using TravelCore.Modules.AgencyMarketplace.Contracts;
using TravelCore.Modules.Booking.Contracts;
using TravelCore.Modules.Booking.Domain;
using TravelCore.Modules.Pricing.Contracts;
using TravelCore.Modules.Tour.Contracts;
using BookingAggregate = TravelCore.Modules.Booking.Domain.Booking;

namespace TravelCore.Modules.Booking.Infrastructure.Services;

/// <summary>
/// Public Pending Booking initiation and authorized reads (TC-P19-T008 / P19-R8; P38-T005).
/// Direct consumer path by default. Optional AgencyOfferId is server-validated — not client-forged SourceKind.
/// Quote issuance is a separate Pricing transaction; Booking work is one Booking transaction.
/// </summary>
public sealed class PublicBookingSurfaceService : IPublicBookingInitiationService, IPublicBookingReadService
{
    private readonly BookingDbContext _db;
    private readonly ITourDeparturePublicQuery _departures;
    private readonly IAuthoritativeQuoteIssuer _quotes;
    private readonly IAgencyOriginContextQuery _origin;
    private readonly IClock _clock;

    public PublicBookingSurfaceService(
        BookingDbContext db,
        ITourDeparturePublicQuery departures,
        IAuthoritativeQuoteIssuer quotes,
        IAgencyOriginContextQuery origin,
        IClock clock)
    {
        ArgumentNullException.ThrowIfNull(db);
        ArgumentNullException.ThrowIfNull(departures);
        ArgumentNullException.ThrowIfNull(quotes);
        ArgumentNullException.ThrowIfNull(origin);
        ArgumentNullException.ThrowIfNull(clock);
        _db = db;
        _departures = departures;
        _quotes = quotes;
        _origin = origin;
        _clock = clock;
    }

    public async Task<PublicBookingInitiationResponse> InitiateAsync(
        PublicBookingInitiationRequest request,
        Guid? actorId,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.Contact);
        ArgumentNullException.ThrowIfNull(request.Passengers);

        if (request.TourDepartureId == Guid.Empty)
        {
            throw new ArgumentException("TourDepartureId cannot be empty.", nameof(request));
        }

        RejectForgedAgencySource(request.SourceKind);
        var idempotencyKey = RequireIdempotencyKey(request.IdempotencyKey);
        var passengers = ParsePassengers(request.Passengers);
        var contact = BookingContactSnapshot.Create(
            request.Contact.DisplayName,
            request.Contact.Email,
            request.Contact.Phone);

        var existing = await FindByIdempotencyAsync(idempotencyKey, cancellationToken);
        if (existing is not null)
        {
            return existing;
        }

        var departure = await _departures.GetPublishedByIdAsync(request.TourDepartureId, cancellationToken)
            ?? throw new InvalidOperationException("Published TourDeparture was not found.");
        if (departure.Capacity.MaximumPax is not int maxPax || maxPax <= 0)
        {
            throw new InvalidOperationException("Published TourDeparture has no authoritative MaximumPax.");
        }

        var source = await ResolveSourceAsync(request.AgencyOfferId, departure, cancellationToken);

        var now = _clock.GetCurrentInstant();
        var quote = await _quotes.IssueForTourDepartureAsync(
            request.TourDepartureId,
            now.ToDateTimeOffset(),
            commercialContextAgencyOfferId: request.AgencyOfferId is Guid offerId && offerId != Guid.Empty
                ? offerId
                : null,
            cancellationToken)
            ?? throw new InvalidOperationException("Authoritative Quote could not be issued because no Price exists.");

        var quoteExpires = Instant.FromDateTimeOffset(quote.ExpiresAt);
        if (quoteExpires <= now)
        {
            throw new InvalidOperationException("Expired Quote cannot be newly accepted as BookingMonetarySnapshot.");
        }

        var facts = BookingQuoteService.MapFacts(quote);
        var tourDeparture = new TourDepartureReference(request.TourDepartureId);
        var seatCount = passengers.Count;

        await using var tx = await _db.Database.BeginTransactionAsync(cancellationToken);
        await AcquireDepartureLockAsync(request.TourDepartureId, cancellationToken);

        var raced = await FindByIdempotencyAsync(idempotencyKey, cancellationToken);
        if (raced is not null)
        {
            await tx.CommitAsync(cancellationToken);
            return raced;
        }

        var booking = BookingAggregate.Create(tourDeparture, now, source);
        booking.SetContact(contact);
        if (actorId is { } id)
        {
            booking.SetActorReference(new BookingActorReference(id));
        }

        _db.Bookings.Add(booking);

        var account = await LoadOrCreateAccountAsync(tourDeparture, cancellationToken);
        account.Reserve(seatCount, maxPax);

        var hold = CapacityHold.Create(
            booking.Id,
            tourDeparture,
            seatCount,
            maxPax,
            now,
            quoteExpires,
            idempotencyKey);
        _db.CapacityHolds.Add(hold);

        foreach (var passenger in passengers)
        {
            booking.AddPassenger(passenger.GivenName, passenger.FamilyName, passenger.Category, seatCount);
        }

        booking.AcceptQuote(facts, now);

        var rawToken = BookingAccessToken.CreateRaw();
        _db.AccessCredentials.Add(
            BookingAccessCredential.Create(booking.Id, BookingAccessToken.Hash(rawToken), now));
        _db.PublicIdempotency.Add(
            BookingPublicIdempotencyRecord.Create(idempotencyKey, booking.Id, now));

        await _db.SaveChangesAsync(cancellationToken);
        await tx.CommitAsync(cancellationToken);
        return PublicBookingMapper.ToInitiation(booking, hold, rawToken);
    }

    public async Task<PublicBookingRead?> GetAuthorizedAsync(
        Guid bookingId,
        string? accessToken,
        Guid? actorId,
        CancellationToken cancellationToken = default)
    {
        if (bookingId == Guid.Empty)
        {
            return null;
        }

        var booking = await LoadBookingAsync(BookingId.From(bookingId), cancellationToken);
        if (booking is null)
        {
            return null;
        }

        if (!await IsAuthorizedAsync(booking.Id, accessToken, actorId, cancellationToken))
        {
            return null;
        }

        var hold = await _db.CapacityHolds
            .AsNoTracking()
            .Where(x => x.BookingId == booking.Id)
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        return PublicBookingMapper.ToRead(booking, hold);
    }

    private async Task<PublicBookingInitiationResponse?> FindByIdempotencyAsync(
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

        var booking = await LoadBookingAsync(record.BookingId, cancellationToken)
            ?? throw new InvalidOperationException("Idempotent Booking was not found.");
        var hold = await _db.CapacityHolds
            .AsNoTracking()
            .Where(x => x.BookingId == booking.Id)
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);
        return PublicBookingMapper.ToInitiation(booking, hold, rawAccessToken: null);
    }

    private async Task<bool> IsAuthorizedAsync(
        BookingId bookingId,
        string? accessToken,
        Guid? actorId,
        CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(accessToken))
        {
            var hash = BookingAccessToken.Hash(accessToken);
            var match = await _db.AccessCredentials
                .AsNoTracking()
                .AnyAsync(x => x.BookingId == bookingId && x.TokenHash == hash, cancellationToken);
            if (match)
            {
                return true;
            }
        }

        if (actorId is { } id)
        {
            var booking = await _db.Bookings
                .AsNoTracking()
                .SingleOrDefaultAsync(x => x.Id == bookingId, cancellationToken);
            return booking?.ActorReference is { ActorId: var actor } && actor == id;
        }

        return false;
    }

    private Task<BookingAggregate?> LoadBookingAsync(BookingId bookingId, CancellationToken cancellationToken) =>
        _db.Bookings
            .Include(x => x.Passengers)
            .Include(x => x.MonetarySnapshot)
            .ThenInclude(x => x!.Components)
            .SingleOrDefaultAsync(x => x.Id == bookingId, cancellationToken);

    private async Task<DepartureCapacityAccount> LoadOrCreateAccountAsync(
        TourDepartureReference departure,
        CancellationToken cancellationToken)
    {
        var existing = await _db.DepartureCapacityAccounts
            .SingleOrDefaultAsync(x => x.TourDeparture == departure, cancellationToken);
        if (existing is not null)
        {
            return existing;
        }

        var created = DepartureCapacityAccount.Create(departure);
        _db.DepartureCapacityAccounts.Add(created);
        return created;
    }

    private Task AcquireDepartureLockAsync(Guid tourDepartureId, CancellationToken cancellationToken)
    {
        var bytes = tourDepartureId.ToByteArray();
        var key1 = BitConverter.ToInt32(bytes, 0);
        var key2 = BitConverter.ToInt32(bytes, 4);
        return _db.Database.ExecuteSqlInterpolatedAsync(
            $"SELECT pg_advisory_xact_lock({key1}, {key2})",
            cancellationToken);
    }

    private async Task<BookingSourceContext> ResolveSourceAsync(
        Guid? agencyOfferId,
        PublishedDeparturePublicSummary departure,
        CancellationToken cancellationToken)
    {
        if (agencyOfferId is null || agencyOfferId == Guid.Empty)
        {
            return BookingSourceContext.Direct();
        }

        var offer = await _origin.GetOfferAsync(agencyOfferId.Value, cancellationToken)
            ?? throw new InvalidOperationException("AgencyOffer was not found.");

        if (!RelatedAgencyOfferPublicEligibility.IsOfferPubliclyEligible(
                offer.PublicationStatus,
                offer.Visibility,
                offer.OfferStatus,
                offer.SalesChannel)
            || !RelatedAgencyOfferPublicEligibility.IsAgencyPubliclyEligible(
                offer.AgencyProfileStatus,
                offer.AgencyPublicListingEnabled))
        {
            throw new InvalidOperationException("AgencyOffer is not publicly eligible for booking selection.");
        }

        if (offer.TourProductId != departure.TourProductId)
        {
            throw new InvalidOperationException("AgencyOffer TourProduct must match the selected TourDeparture product.");
        }

        EnsureDepartureInOfferScope(offer, departure.Id);

        return BookingSourceContext.ForAgency(
            new AgencyProfileReference(offer.AgencyProfileId),
            new AgencyOfferReference(offer.AgencyOfferId));
    }

    private static void EnsureDepartureInOfferScope(AgencyOriginOfferFacts offer, Guid departureId)
    {
        if (string.Equals(offer.DepartureScopeMode, "All", StringComparison.Ordinal))
        {
            return;
        }

        if (string.Equals(offer.DepartureScopeMode, "Listed", StringComparison.Ordinal))
        {
            if (offer.DepartureScopeIds.Count == 0 || !offer.DepartureScopeIds.Contains(departureId))
            {
                throw new InvalidOperationException(
                    "AgencyOffer listed departure scope must include the Booking TourDeparture target.");
            }

            return;
        }

        if (offer.ReferencedTourDepartureId is Guid scoped && scoped != departureId)
        {
            throw new InvalidOperationException(
                "AgencyOffer referenced TourDeparture must match the Booking TourDeparture target.");
        }
    }

    private static void RejectForgedAgencySource(string? sourceKind)
    {
        if (string.IsNullOrWhiteSpace(sourceKind))
        {
            return;
        }

        if (!string.Equals(sourceKind.Trim(), nameof(BookingSourceKind.Direct), StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException(
                "Public Booking initiation rejects client-forged Agency SourceKind. Pass AgencyOfferId for server validation.",
                nameof(sourceKind));
        }
    }

    private static string RequireIdempotencyKey(string? idempotencyKey)
    {
        if (string.IsNullOrWhiteSpace(idempotencyKey))
        {
            throw new ArgumentException("Idempotency key is required.", nameof(idempotencyKey));
        }

        return CapacityHold.NormalizeIdempotencyKey(idempotencyKey);
    }

    private static IReadOnlyList<(string GivenName, string FamilyName, TravelerCategory Category)> ParsePassengers(
        IReadOnlyList<PublicBookingPassengerInput> passengers)
    {
        if (passengers.Count == 0)
        {
            throw new ArgumentException("At least one passenger is required.", nameof(passengers));
        }

        var parsed = new List<(string, string, TravelerCategory)>(passengers.Count);
        foreach (var passenger in passengers)
        {
            if (!Enum.TryParse<TravelerCategory>(passenger.Category, ignoreCase: true, out var category)
                || !Enum.IsDefined(category))
            {
                throw new ArgumentException("TravelerCategory is not controlled.", nameof(passengers));
            }

            parsed.Add((passenger.GivenName, passenger.FamilyName, category));
        }

        return parsed;
    }
}
