using Microsoft.EntityFrameworkCore;
using NodaTime;
using TravelCore.Modules.HotelBooking.Contracts;
using TravelCore.Modules.HotelBooking.Domain;
using Stay = TravelCore.Modules.HotelBooking.Domain.HotelBooking;

namespace TravelCore.Modules.HotelBooking.Infrastructure.Services;

public sealed class HotelAvailabilityHoldService
{
    public const string UnconfiguredSourceKey = "unconfigured";

    private readonly HotelBookingDbContext _db;
    private readonly IHotelAvailabilitySourceResolver _resolver;
    private readonly IClock _clock;

    public HotelAvailabilityHoldService(
        HotelBookingDbContext db,
        IHotelAvailabilitySourceResolver resolver,
        IClock clock)
    {
        _db = db;
        _resolver = resolver;
        _clock = clock;
    }

    public async Task<HotelAvailabilityHold> AcquireAsync(
        HotelBookingId hotelBookingId,
        string idempotencyKey,
        AvailabilitySourceKey? requestedSourceKey = null,
        CancellationToken cancellationToken = default)
    {
        var now = _clock.GetCurrentInstant();
        var existingIdempotency = await _db.HotelAvailabilityHoldIdempotency
            .SingleOrDefaultAsync(
                x => x.HotelBookingId == hotelBookingId && x.IdempotencyKey == idempotencyKey.Trim(),
                cancellationToken);
        if (existingIdempotency is not null)
        {
            return await LoadHoldAsync(existingIdempotency.HoldId, cancellationToken);
        }

        var unresolved = await _db.HotelAvailabilityHolds
            .Include(x => x.Rooms)
            .Where(x => x.HotelBookingId == hotelBookingId
                && (x.Status == HotelAvailabilityHoldStatus.Requested
                    || x.Status == HotelAvailabilityHoldStatus.Active))
            .ToListAsync(cancellationToken);
        if (unresolved.Count > 0)
        {
            throw new InvalidOperationException(
                "An unresolved Requested/Active HotelAvailabilityHold blocks another acquisition.");
        }

        var booking = await _db.HotelBookings
            .Include(x => x.Rooms)
            .ThenInclude(x => x.Guests)
            .SingleAsync(x => x.Id == hotelBookingId, cancellationToken);

        var sourceKey = requestedSourceKey ?? new AvailabilitySourceKey(UnconfiguredSourceKey);
        if (requestedSourceKey is { } explicitKey && _resolver.Resolve(explicitKey) is null
            && explicitKey.Value != UnconfiguredSourceKey)
        {
            throw new InvalidOperationException("Availability source selection is server-controlled.");
        }

        var configured = _resolver.ListConfiguredKeys();
        if (configured.Count == 1)
        {
            sourceKey = configured[0];
        }
        else if (configured.Count > 1)
        {
            throw new InvalidOperationException("Automatic supplier routing/failover is not implemented.");
        }

        var hold = HotelAvailabilityHold.StartRequested(
            booking.Id,
            sourceKey.Value,
            now,
            booking.Rooms.Select(r => r.Id).ToArray());

        _db.HotelAvailabilityHolds.Add(hold);
        _db.HotelAvailabilityHoldIdempotency.Add(
            new HotelAvailabilityHoldIdempotencyRecord(booking.Id, idempotencyKey, hold.Id, now));
        await _db.SaveChangesAsync(cancellationToken);

        var source = _resolver.Resolve(new AvailabilitySourceKey(hold.SourceKey));
        if (source is null)
        {
            return hold;
        }

        HotelAvailabilityHoldSourceResult result;
        try
        {
            result = await source.CreateHoldAsync(ToRequest(booking), cancellationToken);
        }
        catch (TaskCanceledException)
        {
            return hold;
        }
        catch (TimeoutException)
        {
            return hold;
        }

        ApplySourceResult(hold, result, now);
        await _db.SaveChangesAsync(cancellationToken);
        return hold;
    }

    public async Task<HotelAvailabilityHold> RecheckAsync(
        HotelAvailabilityHoldId holdId,
        CancellationToken cancellationToken = default)
    {
        var now = _clock.GetCurrentInstant();
        var hold = await LoadHoldAsync(holdId, cancellationToken);
        hold.ApplyLocalExpiryIfDue(now);
        if (hold.IsTerminal)
        {
            await _db.SaveChangesAsync(cancellationToken);
            return hold;
        }

        var source = _resolver.Resolve(new AvailabilitySourceKey(hold.SourceKey));
        if (source is null || string.IsNullOrWhiteSpace(hold.SourceHoldReference))
        {
            await _db.SaveChangesAsync(cancellationToken);
            return hold;
        }

        var query = await source.QueryHoldStatusAsync(hold.SourceHoldReference, cancellationToken);
        switch (query.Status)
        {
            case HotelAvailabilityHoldQueryStatus.Active when hold.Status == HotelAvailabilityHoldStatus.Requested:
                if (query.ExpiresAt is { } expires
                    && hold.Rooms.All(r => r.SelectionReference is not null))
                {
                    hold.Activate(
                        now,
                        expires,
                        hold.SourceHoldReference,
                        hold.Rooms.ToDictionary(r => r.RoomReservationId, r => r.SelectionReference!));
                }

                break;
            case HotelAvailabilityHoldQueryStatus.Released:
                hold.Release(now);
                break;
            case HotelAvailabilityHoldQueryStatus.Expired:
                hold.Expire(now);
                break;
            case HotelAvailabilityHoldQueryStatus.PendingUnknown:
            case HotelAvailabilityHoldQueryStatus.NotFound:
                break;
        }

        await _db.SaveChangesAsync(cancellationToken);
        return hold;
    }

    public async Task<HotelAvailabilityHold> ReleaseAsync(
        HotelAvailabilityHoldId holdId,
        CancellationToken cancellationToken = default)
    {
        var now = _clock.GetCurrentInstant();
        var hold = await LoadHoldAsync(holdId, cancellationToken);
        if (hold.Status == HotelAvailabilityHoldStatus.Released)
        {
            return hold;
        }

        var source = _resolver.Resolve(new AvailabilitySourceKey(hold.SourceKey));
        if (source is not null && !string.IsNullOrWhiteSpace(hold.SourceHoldReference))
        {
            await source.ReleaseHoldAsync(hold.SourceHoldReference, cancellationToken);
        }

        hold.Release(now);
        await _db.SaveChangesAsync(cancellationToken);
        return hold;
    }

    private async Task<HotelAvailabilityHold> LoadHoldAsync(
        HotelAvailabilityHoldId holdId,
        CancellationToken cancellationToken) =>
        await _db.HotelAvailabilityHolds
            .Include(x => x.Rooms)
            .SingleAsync(x => x.Id == holdId, cancellationToken);

    private static HotelAvailabilityRequest ToRequest(Stay booking) =>
        new(
            booking.Id.Value,
            booking.Place.PlaceId,
            booking.CheckInDate,
            booking.CheckOutDate,
            booking.Rooms.Select(room => new HotelAvailabilityRoomRequest(
                room.Id.Value,
                room.AdultCount,
                room.Guests
                    .Where(g => g.Category == HotelGuestCategory.Child)
                    .Select(g => g.AgeAtCheckIn!.Value.Years)
                    .ToArray()))
            .ToArray());

    private static void ApplySourceResult(
        HotelAvailabilityHold hold,
        HotelAvailabilityHoldSourceResult result,
        Instant now)
    {
        if (result.Outcome is HotelAvailabilitySourceOutcome.Timeout
            or HotelAvailabilitySourceOutcome.Unknown
            or HotelAvailabilitySourceOutcome.Unavailable
            or HotelAvailabilitySourceOutcome.Partial)
        {
            return;
        }

        if (result.Outcome != HotelAvailabilitySourceOutcome.Complete
            || result.ExpiresAt is null
            || string.IsNullOrWhiteSpace(result.SourceHoldReference))
        {
            return;
        }

        var selections = result.Rooms.ToDictionary(
            room => RoomReservationId.From(room.RoomReservationId),
            room => room.SelectionReference);
        if (selections.Count != hold.Rooms.Count)
        {
            return;
        }

        hold.Activate(now, result.ExpiresAt.Value, result.SourceHoldReference, selections);
    }
}
