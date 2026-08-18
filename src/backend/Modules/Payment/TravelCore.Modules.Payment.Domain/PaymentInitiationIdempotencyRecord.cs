using NodaTime;
using TravelCore.Modules.Payment.Domain;

namespace TravelCore.Modules.Payment.Domain;

/// <summary>
/// Payment-scoped initiation idempotency binding (P20-R4). Key is not a PaymentId or AttemptId.
/// </summary>
public sealed class PaymentInitiationIdempotencyRecord
{
    public const int KeyMaxLength = 128;

    private PaymentInitiationIdempotencyRecord()
    {
        IdempotencyKey = null!;
    }

    private PaymentInitiationIdempotencyRecord(
        PaymentId paymentId,
        string idempotencyKey,
        PaymentAttemptId attemptId,
        Instant createdAt)
    {
        PaymentId = paymentId;
        IdempotencyKey = idempotencyKey;
        AttemptId = attemptId;
        CreatedAt = createdAt;
    }

    public PaymentId PaymentId { get; private set; }

    public string IdempotencyKey { get; private set; }

    public PaymentAttemptId AttemptId { get; private set; }

    public Instant CreatedAt { get; private set; }

    public static PaymentInitiationIdempotencyRecord Create(
        PaymentId paymentId,
        string idempotencyKey,
        PaymentAttemptId attemptId,
        Instant now)
    {
        var key = Normalize(idempotencyKey);
        if (now == default)
        {
            throw new ArgumentException("CreatedAt cannot be default.", nameof(now));
        }

        return new PaymentInitiationIdempotencyRecord(paymentId, key, attemptId, now);
    }

    public static string Normalize(string idempotencyKey)
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
}
