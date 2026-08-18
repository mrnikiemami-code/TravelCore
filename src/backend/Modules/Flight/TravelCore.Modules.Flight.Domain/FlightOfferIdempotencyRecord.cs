using NodaTime;

namespace TravelCore.Modules.Flight.Domain;

public sealed class FlightOfferIdempotencyRecord
{
    public const int KeyMaxLength = 128;

    private FlightOfferIdempotencyRecord()
    {
        IdempotencyKey = string.Empty;
    }

    public FlightOfferIdempotencyRecord(
        FlightBookingId flightBookingId,
        string idempotencyKey,
        FlightOfferSnapshotId snapshotId,
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

        FlightBookingId = flightBookingId;
        IdempotencyKey = trimmed;
        SnapshotId = snapshotId;
        CreatedAt = createdAt;
    }

    public FlightBookingId FlightBookingId { get; private set; }

    public string IdempotencyKey { get; private set; }

    public FlightOfferSnapshotId SnapshotId { get; private set; }

    public Instant CreatedAt { get; private set; }
}
