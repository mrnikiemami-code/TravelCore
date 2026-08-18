using NodaTime;

namespace TravelCore.Modules.Booking.Domain;

/// <summary>
/// Public initiation idempotency binding (TC-P19-T008). One key maps to one Booking.
/// </summary>
public sealed class BookingPublicIdempotencyRecord
{
    public const int KeyMaxLength = 128;

    private BookingPublicIdempotencyRecord()
    {
        IdempotencyKey = null!;
    }

    private BookingPublicIdempotencyRecord(string idempotencyKey, BookingId bookingId, Instant createdAt)
    {
        IdempotencyKey = idempotencyKey;
        BookingId = bookingId;
        CreatedAt = createdAt;
    }

    public string IdempotencyKey { get; private set; }

    public BookingId BookingId { get; private set; }

    public Instant CreatedAt { get; private set; }

    public static BookingPublicIdempotencyRecord Create(string idempotencyKey, BookingId bookingId, Instant now)
    {
        var key = CapacityHold.NormalizeIdempotencyKey(idempotencyKey);
        if (key.Length > KeyMaxLength)
        {
            throw new ArgumentException($"Idempotency key cannot exceed {KeyMaxLength} characters.", nameof(idempotencyKey));
        }

        if (bookingId.Value == Guid.Empty)
        {
            throw new ArgumentException("BookingId cannot be empty.", nameof(bookingId));
        }

        if (now == default)
        {
            throw new ArgumentException("CreatedAt cannot be default.", nameof(now));
        }

        return new BookingPublicIdempotencyRecord(key, bookingId, now);
    }
}
