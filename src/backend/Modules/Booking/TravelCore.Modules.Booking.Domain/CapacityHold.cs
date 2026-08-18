using NodaTime;
using TravelCore.Modules.Booking.Contracts;

namespace TravelCore.Modules.Booking.Domain;

/// <summary>
/// Temporary Booking-owned capacity consumption for one logical TourDeparture (P19-R3).
/// Active counts against availability. Consumed remains counted. Released/Expired do not.
/// Does not confirm Booking and is not BookingStatus.
/// </summary>
public sealed class CapacityHold
{
    public const int IdempotencyKeyMaxLength = 128;

    private CapacityHold()
    {
    }

    private CapacityHold(
        CapacityHoldId id,
        BookingId bookingId,
        TourDepartureReference tourDeparture,
        int seatCount,
        int observedConfiguredCapacity,
        Instant createdAt,
        Instant expiresAt,
        string idempotencyKey)
    {
        Id = id;
        BookingId = bookingId;
        TourDeparture = tourDeparture;
        SeatCount = seatCount;
        ObservedConfiguredCapacity = observedConfiguredCapacity;
        Status = CapacityHoldStatus.Active;
        CreatedAt = createdAt;
        ExpiresAt = expiresAt;
        StatusChangedAt = createdAt;
        IdempotencyKey = idempotencyKey;
    }

    public CapacityHoldId Id { get; private set; }

    public BookingId BookingId { get; private set; }

    public TourDepartureReference TourDeparture { get; private set; }

    public int SeatCount { get; private set; }

    /// <summary>
    /// Non-authoritative observation of the Tour capacity definition used for this hold.
    /// NOT Tour Source of Truth.
    /// </summary>
    public int ObservedConfiguredCapacity { get; private set; }

    public CapacityHoldStatus Status { get; private set; }

    public Instant CreatedAt { get; private set; }

    public Instant ExpiresAt { get; private set; }

    public Instant StatusChangedAt { get; private set; }

    public string IdempotencyKey { get; private set; } = string.Empty;

    public bool IsActive => Status == CapacityHoldStatus.Active;

    public bool IsTerminal => Status is CapacityHoldStatus.Consumed
        or CapacityHoldStatus.Released
        or CapacityHoldStatus.Expired;

    public static CapacityHold Create(
        BookingId bookingId,
        TourDepartureReference tourDeparture,
        int seatCount,
        int observedConfiguredCapacity,
        Instant now,
        Instant expiresAt,
        string idempotencyKey)
    {
        if (bookingId.Value == Guid.Empty)
        {
            throw new ArgumentException("BookingId cannot be empty.", nameof(bookingId));
        }

        if (tourDeparture.LogicalId == Guid.Empty)
        {
            throw new ArgumentException("TourDeparture reference cannot be empty.", nameof(tourDeparture));
        }

        if (seatCount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(seatCount), seatCount, "SeatCount must be > 0.");
        }

        if (observedConfiguredCapacity <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(observedConfiguredCapacity),
                observedConfiguredCapacity,
                "ConfiguredCapacity must be > 0.");
        }

        if (now == default)
        {
            throw new ArgumentException("CreatedAt cannot be default.", nameof(now));
        }

        if (expiresAt <= now)
        {
            throw new ArgumentException("ExpiresAt must be greater than CreatedAt.", nameof(expiresAt));
        }

        var key = NormalizeIdempotencyKey(idempotencyKey);
        return new CapacityHold(
            CapacityHoldId.New(),
            bookingId,
            tourDeparture,
            seatCount,
            observedConfiguredCapacity,
            now,
            expiresAt,
            key);
    }

    public void Consume(Instant now)
    {
        EnsureNow(now);
        if (Status == CapacityHoldStatus.Consumed)
        {
            return;
        }

        EnsureActiveForTransition("consume");
        Status = CapacityHoldStatus.Consumed;
        StatusChangedAt = now;
    }

    public void Release(Instant now)
    {
        EnsureNow(now);
        if (Status == CapacityHoldStatus.Released)
        {
            return;
        }

        EnsureActiveForTransition("release");
        Status = CapacityHoldStatus.Released;
        StatusChangedAt = now;
    }

    public void Expire(Instant now)
    {
        EnsureNow(now);
        if (Status == CapacityHoldStatus.Expired)
        {
            return;
        }

        EnsureActiveForTransition("expire");
        if (now < ExpiresAt)
        {
            throw new InvalidOperationException("Active CapacityHold cannot expire before ExpiresAt.");
        }

        Status = CapacityHoldStatus.Expired;
        StatusChangedAt = now;
    }

    public static string NormalizeIdempotencyKey(string idempotencyKey)
    {
        if (string.IsNullOrWhiteSpace(idempotencyKey))
        {
            throw new ArgumentException("Idempotency key is required.", nameof(idempotencyKey));
        }

        var trimmed = idempotencyKey.Trim();
        if (trimmed.Length > IdempotencyKeyMaxLength)
        {
            throw new ArgumentException(
                $"Idempotency key cannot exceed {IdempotencyKeyMaxLength} characters.",
                nameof(idempotencyKey));
        }

        return trimmed;
    }

    private void EnsureActiveForTransition(string operation)
    {
        if (Status != CapacityHoldStatus.Active)
        {
            throw new InvalidOperationException(
                $"CapacityHold in status {Status} cannot {operation}. Terminal holds cannot return to Active.");
        }
    }

    private static void EnsureNow(Instant now)
    {
        if (now == default)
        {
            throw new ArgumentException("StatusChangedAt cannot be default.", nameof(now));
        }
    }
}
