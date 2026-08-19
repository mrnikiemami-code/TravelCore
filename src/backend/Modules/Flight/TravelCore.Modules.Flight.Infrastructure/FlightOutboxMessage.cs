using NodaTime;

namespace TravelCore.Modules.Flight.Infrastructure;

public sealed class FlightOutboxMessage
{
    private FlightOutboxMessage()
    {
        MessageType = null!;
        Payload = null!;
    }

    private FlightOutboxMessage(Guid id, Instant occurredAt, string messageType, string payload)
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

    public static FlightOutboxMessage Create(Guid id, Instant occurredAt, string messageType, string payload)
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
        return new FlightOutboxMessage(id, occurredAt, messageType, payload);
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
