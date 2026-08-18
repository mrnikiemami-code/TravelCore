using NodaTime;

namespace TravelCore.Modules.Flight.Domain;

public sealed class FlightSupplierReservationIdempotencyRecord
{
    public const int KeyMaxLength = 128;

    private FlightSupplierReservationIdempotencyRecord()
    {
        IdempotencyKey = string.Empty;
    }

    public FlightSupplierReservationIdempotencyRecord(
        FlightSupplierReservationId reservationId,
        string idempotencyKey,
        FlightSupplierReservationAttemptId attemptId,
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

        ReservationId = reservationId;
        IdempotencyKey = trimmed;
        AttemptId = attemptId;
        CreatedAt = createdAt;
    }

    public FlightSupplierReservationId ReservationId { get; private set; }

    public string IdempotencyKey { get; private set; }

    public FlightSupplierReservationAttemptId AttemptId { get; private set; }

    public Instant CreatedAt { get; private set; }
}
