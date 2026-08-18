using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using Npgsql;
using TravelCore.Modules.HotelBooking.Contracts;
using TravelCore.Modules.HotelBooking.Domain;
using TravelCore.Modules.Payment.Contracts;
using Stay = TravelCore.Modules.HotelBooking.Domain.HotelBooking;

namespace TravelCore.Modules.HotelBooking.Infrastructure.Services;

/// <summary>
/// Public HotelBooking initiation, authorized reads, availability/rate progression, and R7 cancellation (P21-R8).
/// Initiation creates Pending only. Sources and Payment provider remain server-controlled.
/// </summary>
public sealed class PublicHotelBookingSurfaceService :
    IPublicHotelBookingInitiationService,
    IPublicHotelBookingReadService,
    IPublicHotelBookingJourneyService
{
    private readonly HotelBookingDbContext _db;
    private readonly IHotelPlaceCatalogLookup _places;
    private readonly IHotelAvailabilitySourceResolver _availability;
    private readonly IHotelRateOfferSourceResolver _rates;
    private readonly HotelAvailabilityHoldService _holds;
    private readonly HotelRateOfferAcceptanceService _rateOffers;
    private readonly HotelBookingCancellationService _cancellations;
    private readonly IPublicHotelBookingPaymentService _payments;
    private readonly IClock _clock;

    public PublicHotelBookingSurfaceService(
        HotelBookingDbContext db,
        IHotelPlaceCatalogLookup places,
        IHotelAvailabilitySourceResolver availability,
        IHotelRateOfferSourceResolver rates,
        HotelAvailabilityHoldService holds,
        HotelRateOfferAcceptanceService rateOffers,
        HotelBookingCancellationService cancellations,
        IPublicHotelBookingPaymentService payments,
        IClock clock)
    {
        ArgumentNullException.ThrowIfNull(db);
        ArgumentNullException.ThrowIfNull(places);
        ArgumentNullException.ThrowIfNull(availability);
        ArgumentNullException.ThrowIfNull(rates);
        ArgumentNullException.ThrowIfNull(holds);
        ArgumentNullException.ThrowIfNull(rateOffers);
        ArgumentNullException.ThrowIfNull(cancellations);
        ArgumentNullException.ThrowIfNull(payments);
        ArgumentNullException.ThrowIfNull(clock);
        _db = db;
        _places = places;
        _availability = availability;
        _rates = rates;
        _holds = holds;
        _rateOffers = rateOffers;
        _cancellations = cancellations;
        _payments = payments;
        _clock = clock;
    }

    public async Task<PublicHotelBookingInitiationResponse> InitiateAsync(
        PublicHotelBookingInitiationRequest request,
        Guid? actorId,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.Contact);
        ArgumentNullException.ThrowIfNull(request.Rooms);

        if (request.PlaceId == Guid.Empty)
        {
            throw new ArgumentException("PlaceId cannot be empty.", nameof(request));
        }

        var idempotencyKey = HotelBookingPublicIdempotencyRecord.NormalizeIdempotencyKey(
            request.IdempotencyKey ?? string.Empty);
        var rooms = ParseRooms(request.Rooms);
        var contact = HotelBookingContactSnapshot.Create(request.Contact.Email, request.Contact.Phone);
        var checkIn = ToLocalDate(request.CheckInDate);
        var checkOut = ToLocalDate(request.CheckOutDate);

        var existing = await FindByIdempotencyAsync(idempotencyKey, cancellationToken);
        if (existing is not null)
        {
            return existing;
        }

        if (!await _places.IsActiveHotelPlaceAsync(request.PlaceId, cancellationToken))
        {
            throw new InvalidOperationException("Active Hotel Place was not found.");
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

        var booking = Stay.Create(
            new HotelPlaceReference(request.PlaceId),
            checkIn,
            checkOut,
            contact,
            rooms);
        if (actorId is { } id)
        {
            booking.AttachActorAccount(id);
        }

        var rawToken = HotelBookingAccessToken.CreateRaw();
        _db.HotelBookings.Add(booking);
        _db.AccessCredentials.Add(
            HotelBookingAccessCredential.Create(booking.Id, HotelBookingAccessToken.Hash(rawToken), now));
        _db.PublicIdempotency.Add(
            HotelBookingPublicIdempotencyRecord.Create(idempotencyKey, booking.Id, now));

        try
        {
            await _db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (IsUniqueViolation(ex))
        {
            _db.ChangeTracker.Clear();
            await tx.RollbackAsync(cancellationToken);
            return await FindByIdempotencyAsync(idempotencyKey, cancellationToken)
                ?? throw new InvalidOperationException("Concurrent HotelBooking initiation did not converge.");
        }

        await tx.CommitAsync(cancellationToken);
        return PublicHotelBookingMapper.ToInitiation(booking, rawToken);
    }

    public async Task<PublicHotelBookingRead?> GetAuthorizedAsync(
        Guid hotelBookingId,
        string? accessToken,
        Guid? actorId,
        CancellationToken cancellationToken = default)
    {
        var booking = await LoadAuthorizedAsync(hotelBookingId, accessToken, actorId, cancellationToken);
        return booking is null ? null : await ComposeReadAsync(booking, cancellationToken);
    }

    public async Task<PublicHotelBookingProgressResult> RequestAvailabilityAsync(
        Guid hotelBookingId,
        string? accessToken,
        Guid? actorId,
        string? idempotencyKey,
        CancellationToken cancellationToken = default)
    {
        var booking = await LoadAuthorizedAsync(hotelBookingId, accessToken, actorId, cancellationToken)
            ?? throw new UnauthorizedAccessException();
        if (_availability.ListConfiguredKeys().Count == 0)
        {
            return new PublicHotelBookingProgressResult(
                PublicHotelBookingJourneyStatus.SourceUnavailable,
                await ComposeReadAsync(booking, cancellationToken));
        }

        var key = string.IsNullOrWhiteSpace(idempotencyKey)
            ? $"availability:{booking.Id.Value:D}"
            : HotelBookingPublicIdempotencyRecord.NormalizeIdempotencyKey(idempotencyKey);
        await _holds.AcquireAsync(booking.Id, key, requestedSourceKey: null, cancellationToken);
        booking = await LoadBookingAsync(booking.Id, cancellationToken)
            ?? throw new InvalidOperationException("HotelBooking was not found.");
        return new PublicHotelBookingProgressResult(
            PublicHotelBookingJourneyStatus.Completed,
            await ComposeReadAsync(booking, cancellationToken));
    }

    public async Task<PublicHotelBookingProgressResult> RequestRateOfferAsync(
        Guid hotelBookingId,
        string? accessToken,
        Guid? actorId,
        string? idempotencyKey,
        CancellationToken cancellationToken = default)
    {
        var booking = await LoadAuthorizedAsync(hotelBookingId, accessToken, actorId, cancellationToken)
            ?? throw new UnauthorizedAccessException();
        if (_rates.ListConfiguredKeys().Count == 0)
        {
            return new PublicHotelBookingProgressResult(
                PublicHotelBookingJourneyStatus.SourceUnavailable,
                await ComposeReadAsync(booking, cancellationToken));
        }

        var key = string.IsNullOrWhiteSpace(idempotencyKey)
            ? $"rate:{booking.Id.Value:D}"
            : HotelBookingPublicIdempotencyRecord.NormalizeIdempotencyKey(idempotencyKey);
        await _rateOffers.AcceptAsync(booking.Id, key, requestedSourceKey: null, cancellationToken);
        booking = await LoadBookingAsync(booking.Id, cancellationToken)
            ?? throw new InvalidOperationException("HotelBooking was not found.");
        return new PublicHotelBookingProgressResult(
            PublicHotelBookingJourneyStatus.Completed,
            await ComposeReadAsync(booking, cancellationToken));
    }

    public async Task<PublicHotelBookingCancellationCommandResult?> RequestCancellationAsync(
        Guid hotelBookingId,
        string? accessToken,
        Guid? actorId,
        string? idempotencyKey,
        CancellationToken cancellationToken = default)
    {
        var booking = await LoadAuthorizedAsync(hotelBookingId, accessToken, actorId, cancellationToken);
        if (booking is null)
        {
            return null;
        }

        var key = string.IsNullOrWhiteSpace(idempotencyKey)
            ? $"cancel:{booking.Id.Value:D}"
            : HotelBookingPublicIdempotencyRecord.NormalizeIdempotencyKey(idempotencyKey);
        var result = await _cancellations.RequestAsync(booking.Id, key, cancellationToken);
        booking = await LoadBookingAsync(booking.Id, cancellationToken)
            ?? throw new InvalidOperationException("HotelBooking was not found.");
        return new PublicHotelBookingCancellationCommandResult(
            result.Outcome.ToString(),
            await ComposeReadAsync(booking, cancellationToken));
    }

    private async Task<PublicHotelBookingRead> ComposeReadAsync(
        Stay booking,
        CancellationToken cancellationToken)
    {
        var hold = await _db.HotelAvailabilityHolds
            .AsNoTracking()
            .Include(x => x.Rooms)
            .Where(x => x.HotelBookingId == booking.Id)
            .OrderByDescending(x => x.RequestedAt)
            .FirstOrDefaultAsync(cancellationToken);
        var offer = await _db.HotelRateOfferSnapshots
            .AsNoTracking()
            .Include(x => x.Monetary)
            .Include(x => x.CancellationPolicy)
            .ThenInclude(x => x.Rules)
            .SingleOrDefaultAsync(x => x.HotelBookingId == booking.Id, cancellationToken);
        var reservation = await _db.HotelSupplierReservations
            .AsNoTracking()
            .Where(x => x.HotelBookingId == booking.Id)
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);
        var cancellation = await _db.HotelBookingCancellations
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.HotelBookingId == booking.Id, cancellationToken);
        var issues = await _db.HotelBookingReconciliationIssues
            .AsNoTracking()
            .Where(x => x.HotelBookingId == booking.Id)
            .ToListAsync(cancellationToken);
        PublicPaymentRead? payment = null;
        try
        {
            payment = await _payments.GetByHotelBookingIdAsync(booking.Id.Value, cancellationToken);
        }
        catch (InvalidOperationException)
        {
            // Payment row may be absent until first scoped read; keep booking readable.
        }

        return PublicHotelBookingMapper.ToRead(
            new PublicHotelBookingFacts(
                booking,
                hold,
                offer,
                reservation,
                cancellation,
                issues,
                payment,
                _clock.GetCurrentInstant()));
    }

    private async Task<Stay?> LoadAuthorizedAsync(
        Guid hotelBookingId,
        string? accessToken,
        Guid? actorId,
        CancellationToken cancellationToken)
    {
        if (hotelBookingId == Guid.Empty)
        {
            return null;
        }

        var id = HotelBookingId.From(hotelBookingId);
        if (!await IsAuthorizedAsync(id, accessToken, actorId, cancellationToken))
        {
            return null;
        }

        return await LoadBookingAsync(id, cancellationToken);
    }

    private async Task<bool> IsAuthorizedAsync(
        HotelBookingId hotelBookingId,
        string? accessToken,
        Guid? actorId,
        CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(accessToken))
        {
            var hash = HotelBookingAccessToken.Hash(accessToken);
            var match = await _db.AccessCredentials
                .AsNoTracking()
                .AnyAsync(x => x.HotelBookingId == hotelBookingId && x.TokenHash == hash, cancellationToken);
            if (match)
            {
                return true;
            }
        }

        if (actorId is { } id)
        {
            var booking = await _db.HotelBookings
                .AsNoTracking()
                .SingleOrDefaultAsync(x => x.Id == hotelBookingId, cancellationToken);
            return booking?.ActorAccountId == id;
        }

        return false;
    }

    private Task<Stay?> LoadBookingAsync(HotelBookingId hotelBookingId, CancellationToken cancellationToken) =>
        _db.HotelBookings
            .Include(x => x.Rooms)
            .ThenInclude(x => x.Guests)
            .SingleOrDefaultAsync(x => x.Id == hotelBookingId, cancellationToken);

    private async Task<PublicHotelBookingInitiationResponse?> FindByIdempotencyAsync(
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

        var booking = await LoadBookingAsync(record.HotelBookingId, cancellationToken)
            ?? throw new InvalidOperationException("Idempotent HotelBooking was not found.");
        return PublicHotelBookingMapper.ToInitiation(booking, rawAccessToken: null);
    }

    private Task AcquireIdempotencyLockAsync(string idempotencyKey, CancellationToken cancellationToken)
    {
        if (_db.Database.ProviderName is null
            || !_db.Database.ProviderName.Contains("Npgsql", StringComparison.OrdinalIgnoreCase))
        {
            return Task.CompletedTask;
        }

        var digest = SHA256.HashData(Encoding.UTF8.GetBytes("hotel-booking:" + idempotencyKey));
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

    private static LocalDate ToLocalDate(DateOnly date) => new(date.Year, date.Month, date.Day);

    private static IReadOnlyList<RoomReservationSpecification> ParseRooms(
        IReadOnlyList<PublicHotelBookingRoomInput> rooms)
    {
        if (rooms.Count == 0)
        {
            throw new ArgumentException("At least one room is required.", nameof(rooms));
        }

        var parsed = new List<RoomReservationSpecification>(rooms.Count);
        foreach (var room in rooms)
        {
            ArgumentNullException.ThrowIfNull(room);
            ArgumentNullException.ThrowIfNull(room.Guests);
            var guests = new List<HotelBookingGuestSpecification>(room.Guests.Count);
            foreach (var guest in room.Guests)
            {
                if (!Enum.TryParse<HotelGuestCategory>(guest.Category, ignoreCase: true, out var category)
                    || !Enum.IsDefined(category))
                {
                    throw new ArgumentException("HotelGuestCategory is not controlled.", nameof(rooms));
                }

                guests.Add(
                    new HotelBookingGuestSpecification(
                        guest.GivenName,
                        guest.FamilyName,
                        category,
                        guest.IsLeadGuest,
                        guest.AgeAtCheckInYears));
            }

            parsed.Add(new RoomReservationSpecification(guests));
        }

        return parsed;
    }
}
