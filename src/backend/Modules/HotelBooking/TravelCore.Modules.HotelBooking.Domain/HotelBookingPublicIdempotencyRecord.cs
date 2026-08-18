using NodaTime;

namespace TravelCore.Modules.HotelBooking.Domain;

/// <summary>
/// Public initiation idempotency binding (TC-P21-T008). One key maps to one HotelBooking.
/// Database-backed; not process-local.
/// </summary>
public sealed class HotelBookingPublicIdempotencyRecord
{
    public const int KeyMaxLength = 128;

    private HotelBookingPublicIdempotencyRecord()
    {
        IdempotencyKey = null!;
    }

    private HotelBookingPublicIdempotencyRecord(
        string idempotencyKey,
        HotelBookingId hotelBookingId,
        Instant createdAt)
    {
        IdempotencyKey = idempotencyKey;
        HotelBookingId = hotelBookingId;
        CreatedAt = createdAt;
    }

    public string IdempotencyKey { get; private set; }

    public HotelBookingId HotelBookingId { get; private set; }

    public Instant CreatedAt { get; private set; }

    public static string NormalizeIdempotencyKey(string idempotencyKey)
    {
        if (string.IsNullOrWhiteSpace(idempotencyKey))
        {
            throw new ArgumentException("Idempotency key is required.", nameof(idempotencyKey));
        }

        var trimmed = idempotencyKey.Trim();
        if (trimmed.Length > KeyMaxLength)
        {
            throw new ArgumentException(
                $"Idempotency key cannot exceed {KeyMaxLength} characters.",
                nameof(idempotencyKey));
        }

        return trimmed;
    }

    public static HotelBookingPublicIdempotencyRecord Create(
        string idempotencyKey,
        HotelBookingId hotelBookingId,
        Instant now)
    {
        var key = NormalizeIdempotencyKey(idempotencyKey);
        if (hotelBookingId.Value == Guid.Empty)
        {
            throw new ArgumentException("HotelBookingId cannot be empty.", nameof(hotelBookingId));
        }

        if (now == default)
        {
            throw new ArgumentException("CreatedAt cannot be default.", nameof(now));
        }

        return new HotelBookingPublicIdempotencyRecord(key, hotelBookingId, now);
    }
}
