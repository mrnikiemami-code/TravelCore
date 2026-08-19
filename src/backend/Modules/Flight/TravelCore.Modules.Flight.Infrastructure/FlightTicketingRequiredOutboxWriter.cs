using System.Text.Json;
using NodaTime;
using NodaTime.Text;
using TravelCore.Modules.Flight.Domain;

namespace TravelCore.Modules.Flight.Infrastructure;

internal static class FlightTicketingRequiredOutboxBoundary
{
    public const string MessageType = "FlightTicketingRequired";
}

internal sealed record FlightTicketingRequiredWorkItem(
    Guid FlightBookingId,
    Guid PaymentId,
    Instant OccurredAt);

internal static class FlightTicketingRequiredOutboxWriter
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);
    private static readonly InstantPattern InstantIso = InstantPattern.ExtendedIso;

    public static void Enqueue(
        FlightDbContext db,
        FlightBookingId flightBookingId,
        Guid paymentId,
        Instant now)
    {
        ArgumentNullException.ThrowIfNull(db);
        if (paymentId == Guid.Empty)
        {
            throw new ArgumentException("PaymentId cannot be empty.", nameof(paymentId));
        }

        var id = flightBookingId.Value;
        if (db.OutboxMessages.Local.Any(x => x.Id == id)
            || db.OutboxMessages.Any(x => x.Id == id))
        {
            return;
        }

        var payload = JsonSerializer.Serialize(
            new Payload(flightBookingId.Value, paymentId, InstantIso.Format(now)),
            Json);
        db.OutboxMessages.Add(
            FlightOutboxMessage.Create(
                id,
                now,
                FlightTicketingRequiredOutboxBoundary.MessageType,
                payload));
    }

    public static FlightTicketingRequiredWorkItem Deserialize(string payload)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(payload);
        var dto = JsonSerializer.Deserialize<Payload>(payload, Json)
            ?? throw new InvalidOperationException("Ticketing-required outbox payload was empty.");
        return new FlightTicketingRequiredWorkItem(
            dto.FlightBookingId,
            dto.PaymentId,
            InstantIso.Parse(dto.OccurredAt).Value);
    }

    private sealed record Payload(Guid FlightBookingId, Guid PaymentId, string OccurredAt);
}
