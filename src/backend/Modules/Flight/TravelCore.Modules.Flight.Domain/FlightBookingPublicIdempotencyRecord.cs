using NodaTime;

namespace TravelCore.Modules.Flight.Domain;

/// <summary>
/// Public initiation idempotency binding (TC-P22-T008). One key maps to one FlightBooking.
/// Database-backed; not process-local.
/// </summary>
public sealed class FlightBookingPublicIdempotencyRecord
{
    public const int KeyMaxLength = 128;

    private FlightBookingPublicIdempotencyRecord()
    {
        IdempotencyKey = null!;
    }

    private FlightBookingPublicIdempotencyRecord(
        string idempotencyKey,
        FlightBookingId flightBookingId,
        Instant createdAt)
    {
        IdempotencyKey = idempotencyKey;
        FlightBookingId = flightBookingId;
        CreatedAt = createdAt;
    }

    public string IdempotencyKey { get; private set; }

    public FlightBookingId FlightBookingId { get; private set; }

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

    public static FlightBookingPublicIdempotencyRecord Create(
        string idempotencyKey,
        FlightBookingId flightBookingId,
        Instant now)
    {
        var key = NormalizeIdempotencyKey(idempotencyKey);
        if (flightBookingId.Value == Guid.Empty)
        {
            throw new ArgumentException("FlightBookingId cannot be empty.", nameof(flightBookingId));
        }

        if (now == default)
        {
            throw new ArgumentException("CreatedAt cannot be default.", nameof(now));
        }

        return new FlightBookingPublicIdempotencyRecord(key, flightBookingId, now);
    }
}
