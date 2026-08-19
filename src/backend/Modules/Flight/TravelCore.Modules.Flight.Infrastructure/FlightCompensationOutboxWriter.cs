using System.Text.Json;
using NodaTime;
using NodaTime.Text;
using TravelCore.Modules.Flight.Domain;
using TravelCore.Modules.Payment.Contracts;

namespace TravelCore.Modules.Flight.Infrastructure;

internal static class FlightCompensationOutboxWriter
{
    public static void Enqueue(
        FlightDbContext db,
        FlightBookingPaymentCompensationEvidence evidence,
        Instant now)
    {
        ArgumentNullException.ThrowIfNull(db);
        ArgumentNullException.ThrowIfNull(evidence);

        if (db.OutboxMessages.Local.Any(x => x.Id == evidence.PaymentId)
            || db.OutboxMessages.Any(x => x.Id == evidence.PaymentId))
        {
            return;
        }

        var message = new FlightBookingPaymentCompensationRequiredIntegrationEvent(
            evidence.FlightBookingId.Value,
            evidence.PaymentId,
            evidence.Reason.ToString(),
            now);

        db.OutboxMessages.Add(
            FlightOutboxMessage.Create(
                evidence.PaymentId,
                now,
                FlightBookingCompensationOutboxBoundary.MessageType,
                FlightCompensationOutboxSerializer.Serialize(message)));
    }
}

internal static class FlightCompensationOutboxSerializer
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);
    private static readonly InstantPattern InstantIso = InstantPattern.ExtendedIso;

    public static string Serialize(FlightBookingPaymentCompensationRequiredIntegrationEvent message)
    {
        ArgumentNullException.ThrowIfNull(message);
        return JsonSerializer.Serialize(
            new Payload(
                message.FlightBookingId,
                message.PaymentId,
                message.RecoveryReason,
                InstantIso.Format(message.OccurredAt)),
            Json);
    }

    public static FlightBookingPaymentCompensationRequiredIntegrationEvent Deserialize(string payload)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(payload);
        var dto = JsonSerializer.Deserialize<Payload>(payload, Json)
            ?? throw new InvalidOperationException("Flight compensation-required outbox payload was empty.");
        var occurred = InstantIso.Parse(dto.OccurredAt).Value;
        return new FlightBookingPaymentCompensationRequiredIntegrationEvent(
            dto.FlightBookingId,
            dto.PaymentId,
            dto.RecoveryReason,
            occurred);
    }

    private sealed record Payload(
        Guid FlightBookingId,
        Guid PaymentId,
        string RecoveryReason,
        string OccurredAt);
}
