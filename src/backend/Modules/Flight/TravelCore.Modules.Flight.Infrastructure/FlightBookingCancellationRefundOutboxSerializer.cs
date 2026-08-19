using System.Text.Json;
using NodaTime;
using NodaTime.Text;
using TravelCore.Modules.Payment.Contracts;

namespace TravelCore.Modules.Flight.Infrastructure;

internal static class FlightBookingCancellationRefundOutboxSerializer
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);
    private static readonly InstantPattern InstantIso = InstantPattern.ExtendedIso;

    public static string Serialize(FlightBookingCancellationRefundRequiredIntegrationEvent message)
    {
        ArgumentNullException.ThrowIfNull(message);
        return JsonSerializer.Serialize(
            new Payload(
                message.FlightBookingCancellationId,
                message.FlightBookingId,
                message.PaymentId,
                InstantIso.Format(message.OccurredAt)),
            Json);
    }

    public static FlightBookingCancellationRefundRequiredIntegrationEvent Deserialize(string payload)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(payload);
        var dto = JsonSerializer.Deserialize<Payload>(payload, Json)
            ?? throw new InvalidOperationException("Flight cancellation refund-required outbox payload was empty.");
        var occurred = InstantIso.Parse(dto.OccurredAt).Value;
        return new FlightBookingCancellationRefundRequiredIntegrationEvent(
            dto.FlightBookingCancellationId,
            dto.FlightBookingId,
            dto.PaymentId,
            occurred);
    }

    private sealed record Payload(
        Guid FlightBookingCancellationId,
        Guid FlightBookingId,
        Guid PaymentId,
        string OccurredAt);
}
