using NodaTime;

namespace TravelCore.Modules.HotelBooking.Domain;

public sealed class HotelBookingCancellationIdempotencyRecord
{
    public const int KeyMaxLength = 128;

    private HotelBookingCancellationIdempotencyRecord()
    {
        IdempotencyKey = string.Empty;
    }

    public HotelBookingCancellationIdempotencyRecord(
        HotelBookingId hotelBookingId,
        string idempotencyKey,
        HotelBookingCancellationId cancellationId,
        HotelSupplierCancellationAttemptId? attemptId,
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
        CancellationId = cancellationId;
        AttemptId = attemptId;
        CreatedAt = createdAt;
    }

    public HotelBookingId HotelBookingId { get; private set; }

    public string IdempotencyKey { get; private set; }

    public HotelBookingCancellationId CancellationId { get; private set; }

    public HotelSupplierCancellationAttemptId? AttemptId { get; private set; }

    public Instant CreatedAt { get; private set; }
}
