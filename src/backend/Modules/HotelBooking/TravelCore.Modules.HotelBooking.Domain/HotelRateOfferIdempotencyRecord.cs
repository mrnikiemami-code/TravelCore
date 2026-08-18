using NodaTime;

namespace TravelCore.Modules.HotelBooking.Domain;

public sealed class HotelRateOfferIdempotencyRecord
{
    public const int KeyMaxLength = 128;

    private HotelRateOfferIdempotencyRecord()
    {
        IdempotencyKey = string.Empty;
    }

    public HotelRateOfferIdempotencyRecord(
        HotelBookingId hotelBookingId,
        string idempotencyKey,
        HotelRateOfferSnapshotId snapshotId,
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
        SnapshotId = snapshotId;
        CreatedAt = createdAt;
    }

    public HotelBookingId HotelBookingId { get; private set; }

    public string IdempotencyKey { get; private set; }

    public HotelRateOfferSnapshotId SnapshotId { get; private set; }

    public Instant CreatedAt { get; private set; }
}
