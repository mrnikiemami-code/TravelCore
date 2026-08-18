using NodaTime;

namespace TravelCore.Modules.HotelBooking.Domain;

public sealed class HotelSupplierReservationIdempotencyRecord
{
    public const int KeyMaxLength = 128;

    private HotelSupplierReservationIdempotencyRecord()
    {
        IdempotencyKey = string.Empty;
    }

    public HotelSupplierReservationIdempotencyRecord(
        HotelSupplierReservationId reservationId,
        string idempotencyKey,
        HotelSupplierReservationAttemptId attemptId,
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

    public HotelSupplierReservationId ReservationId { get; private set; }

    public string IdempotencyKey { get; private set; }

    public HotelSupplierReservationAttemptId AttemptId { get; private set; }

    public Instant CreatedAt { get; private set; }
}
