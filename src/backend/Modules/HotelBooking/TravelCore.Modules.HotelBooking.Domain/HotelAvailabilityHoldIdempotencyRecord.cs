using NodaTime;

namespace TravelCore.Modules.HotelBooking.Domain;

public sealed class HotelAvailabilityHoldIdempotencyRecord
{
    public const int KeyMaxLength = 128;

    private HotelAvailabilityHoldIdempotencyRecord()
    {
        IdempotencyKey = string.Empty;
    }

    public HotelAvailabilityHoldIdempotencyRecord(
        HotelBookingId hotelBookingId,
        string idempotencyKey,
        HotelAvailabilityHoldId holdId,
        Instant createdAt)
    {
        if (string.IsNullOrWhiteSpace(idempotencyKey))
        {
            throw new ArgumentException("Idempotency key is required.", nameof(idempotencyKey));
        }

        var trimmed = idempotencyKey.Trim();
        if (trimmed.Length > KeyMaxLength)
        {
            throw new ArgumentException($"Idempotency key max length is {KeyMaxLength}.", nameof(idempotencyKey));
        }

        if (createdAt == default)
        {
            throw new ArgumentException("CreatedAt cannot be default.", nameof(createdAt));
        }

        HotelBookingId = hotelBookingId;
        IdempotencyKey = trimmed;
        HoldId = holdId;
        CreatedAt = createdAt;
    }

    public HotelBookingId HotelBookingId { get; private set; }

    public string IdempotencyKey { get; private set; }

    public HotelAvailabilityHoldId HoldId { get; private set; }

    public Instant CreatedAt { get; private set; }
}
