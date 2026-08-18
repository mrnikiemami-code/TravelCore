using NodaTime;

namespace TravelCore.Modules.Payment.Infrastructure;

/// <summary>
/// Payment-owned module-local transactional outbox row (docs/architecture/29-module-local-transactional-outbox.md).
/// </summary>
public sealed class PaymentOutboxMessage
{
    private PaymentOutboxMessage()
    {
        MessageType = null!;
        Payload = null!;
    }

    private PaymentOutboxMessage(
        Guid id,
        Instant occurredAt,
        string messageType,
        string payload)
    {
        Id = id;
        OccurredAt = occurredAt;
        MessageType = messageType;
        Payload = payload;
    }

    public Guid Id { get; private set; }

    public Instant OccurredAt { get; private set; }

    public string MessageType { get; private set; }

    public string Payload { get; private set; }

    public Instant? ProcessedAt { get; private set; }

    public static PaymentOutboxMessage Create(
        Guid id,
        Instant occurredAt,
        string messageType,
        string payload)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Outbox id cannot be empty.", nameof(id));
        }

        if (occurredAt == default)
        {
            throw new ArgumentException("OccurredAt cannot be default.", nameof(occurredAt));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(messageType);
        ArgumentException.ThrowIfNullOrWhiteSpace(payload);

        return new PaymentOutboxMessage(id, occurredAt, messageType, payload);
    }

    public void MarkProcessed(Instant now)
    {
        if (now == default)
        {
            throw new ArgumentException("ProcessedAt cannot be default.", nameof(now));
        }

        ProcessedAt ??= now;
    }
}
