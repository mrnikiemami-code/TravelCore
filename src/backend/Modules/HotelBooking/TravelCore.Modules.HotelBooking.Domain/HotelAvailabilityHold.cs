using NodaTime;

namespace TravelCore.Modules.HotelBooking.Domain;

/// <summary>
/// HotelBooking-owned temporary multi-room availability hold (TC-P21-T003 / P21-R3).
/// Not inventory authority, confirmation, Payment, or a named supplier reservation.
/// </summary>
public sealed class HotelAvailabilityHold
{
    public const int SourceKeyMaxLength = 64;
    public const int SourceHoldReferenceMaxLength = 128;

    private readonly List<HotelAvailabilityHoldRoom> _rooms = [];

    private HotelAvailabilityHold()
    {
        SourceKey = string.Empty;
    }

    private HotelAvailabilityHold(
        HotelAvailabilityHoldId id,
        HotelBookingId hotelBookingId,
        string sourceKey,
        Instant requestedAt)
    {
        Id = id;
        HotelBookingId = hotelBookingId;
        SourceKey = sourceKey;
        Status = HotelAvailabilityHoldStatus.Requested;
        RequestedAt = requestedAt;
        Version = 0;
    }

    public HotelAvailabilityHoldId Id { get; private set; }

    public HotelBookingId HotelBookingId { get; private set; }

    public string SourceKey { get; private set; }

    public HotelAvailabilityHoldStatus Status { get; private set; }

    public string? SourceHoldReference { get; private set; }

    public Instant RequestedAt { get; private set; }

    public Instant? ActivatedAt { get; private set; }

    public Instant? ExpiresAt { get; private set; }

    public Instant? ReleasedAt { get; private set; }

    public Instant? ExpiredAt { get; private set; }

    public long Version { get; private set; }

    public IReadOnlyList<HotelAvailabilityHoldRoom> Rooms => _rooms;

    public bool IsUnresolved =>
        Status is HotelAvailabilityHoldStatus.Requested or HotelAvailabilityHoldStatus.Active;

    public bool IsTerminal =>
        Status is HotelAvailabilityHoldStatus.Released or HotelAvailabilityHoldStatus.Expired;

    public static HotelAvailabilityHold StartRequested(
        HotelBookingId hotelBookingId,
        string sourceKey,
        Instant requestedAt,
        IReadOnlyList<RoomReservationId> rooms)
    {
        ArgumentNullException.ThrowIfNull(rooms);
        if (requestedAt == default)
        {
            throw new ArgumentException("RequestedAt cannot be default.", nameof(requestedAt));
        }

        if (string.IsNullOrWhiteSpace(sourceKey))
        {
            throw new ArgumentException("SourceKey is required.", nameof(sourceKey));
        }

        var normalizedKey = sourceKey.Trim().ToLowerInvariant();
        if (normalizedKey.Length > SourceKeyMaxLength)
        {
            throw new ArgumentException($"SourceKey max length is {SourceKeyMaxLength}.", nameof(sourceKey));
        }

        if (rooms.Count == 0)
        {
            throw new ArgumentException("Hold must cover at least one RoomReservation.", nameof(rooms));
        }

        if (rooms.Distinct().Count() != rooms.Count)
        {
            throw new ArgumentException("Hold rooms must be unique.", nameof(rooms));
        }

        var hold = new HotelAvailabilityHold(
            HotelAvailabilityHoldId.New(),
            hotelBookingId,
            normalizedKey,
            requestedAt);

        foreach (var roomId in rooms)
        {
            hold._rooms.Add(new HotelAvailabilityHoldRoom(hold.Id, roomId));
        }

        return hold;
    }

    public void Activate(
        Instant now,
        Instant expiresAt,
        string sourceHoldReference,
        IReadOnlyDictionary<RoomReservationId, string> roomSelections)
    {
        EnsureClock(now);
        ArgumentNullException.ThrowIfNull(roomSelections);
        EnsureRequested();

        if (string.IsNullOrWhiteSpace(sourceHoldReference))
        {
            throw new ArgumentException("Active hold requires a source hold reference.", nameof(sourceHoldReference));
        }

        var trimmedRef = sourceHoldReference.Trim();
        if (trimmedRef.Length > SourceHoldReferenceMaxLength)
        {
            throw new ArgumentException(
                $"Source hold reference max length is {SourceHoldReferenceMaxLength}.",
                nameof(sourceHoldReference));
        }

        if (expiresAt <= now)
        {
            throw new ArgumentException("ExpiresAt must be later than activation time.", nameof(expiresAt));
        }

        if (roomSelections.Count != _rooms.Count
            || _rooms.Any(room => !roomSelections.ContainsKey(room.RoomReservationId)))
        {
            throw new InvalidOperationException(
                "Active hold must cover every RoomReservation; partial source success cannot become Active.");
        }

        foreach (var room in _rooms)
        {
            room.AssignSelection(roomSelections[room.RoomReservationId]);
        }

        SourceHoldReference = trimmedRef;
        Status = HotelAvailabilityHoldStatus.Active;
        ActivatedAt = now;
        ExpiresAt = expiresAt;
        IncrementVersion();
    }

    public void Release(Instant now)
    {
        EnsureClock(now);
        if (Status == HotelAvailabilityHoldStatus.Released)
        {
            return;
        }

        if (Status == HotelAvailabilityHoldStatus.Expired)
        {
            throw new InvalidOperationException("Expired hold cannot be released.");
        }

        Status = HotelAvailabilityHoldStatus.Released;
        ReleasedAt = now;
        IncrementVersion();
    }

    public void Expire(Instant now)
    {
        EnsureClock(now);
        if (Status == HotelAvailabilityHoldStatus.Expired)
        {
            return;
        }

        if (Status == HotelAvailabilityHoldStatus.Released)
        {
            throw new InvalidOperationException("Released hold cannot expire.");
        }

        Status = HotelAvailabilityHoldStatus.Expired;
        ExpiredAt = now;
        IncrementVersion();
    }

    public void ApplyLocalExpiryIfDue(Instant now)
    {
        if (Status == HotelAvailabilityHoldStatus.Active
            && ExpiresAt is { } expires
            && now >= expires)
        {
            Expire(now);
        }
    }

    private void EnsureRequested()
    {
        if (Status != HotelAvailabilityHoldStatus.Requested)
        {
            throw new InvalidOperationException(
                $"Hold in status {Status} cannot be activated.");
        }
    }

    private static void EnsureClock(Instant now)
    {
        if (now == default)
        {
            throw new ArgumentException("Instant cannot be default.", nameof(now));
        }
    }

    private void IncrementVersion() => Version++;
}
