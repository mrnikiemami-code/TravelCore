using NodaTime;

namespace TravelCore.Modules.Flight.Domain;

public sealed class FlightTicketingIdempotencyRecord
{
    public const int KeyMaxLength = 128;

    private FlightTicketingIdempotencyRecord()
    {
        IdempotencyKey = string.Empty;
    }

    public FlightTicketingIdempotencyRecord(
        FlightBookingId flightBookingId,
        string idempotencyKey,
        FlightTicketingAttemptId attemptId,
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
        AttemptId = attemptId;
        CreatedAt = createdAt;
    }

    public FlightBookingId FlightBookingId { get; private set; }

    public string IdempotencyKey { get; private set; }

    public FlightTicketingAttemptId AttemptId { get; private set; }

    public Instant CreatedAt { get; private set; }
}
