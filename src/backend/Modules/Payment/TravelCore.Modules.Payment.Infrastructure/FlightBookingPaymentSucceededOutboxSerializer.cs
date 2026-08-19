using System.Text.Json;
using NodaTime;
using NodaTime.Text;
using TravelCore.Modules.Payment.Contracts;

namespace TravelCore.Modules.Payment.Infrastructure;

internal static class FlightBookingPaymentSucceededOutboxSerializer
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);
    private static readonly InstantPattern InstantIso = InstantPattern.ExtendedIso;

    public static string Serialize(FlightBookingPaymentSucceededIntegrationEvent message)
    {
        ArgumentNullException.ThrowIfNull(message);
        return JsonSerializer.Serialize(
            new Payload(
                message.PaymentId,
                message.FlightBookingId,
                InstantIso.Format(message.OccurredAt),
                message.Amount,
                message.CurrencyCode),
            Json);
    }

    public static FlightBookingPaymentSucceededIntegrationEvent Deserialize(string payload)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(payload);
        var dto = JsonSerializer.Deserialize<Payload>(payload, Json)
            ?? throw new InvalidOperationException("FlightBooking PaymentSucceeded outbox payload was empty.");
        var occurred = InstantIso.Parse(dto.OccurredAt).Value;
        return new FlightBookingPaymentSucceededIntegrationEvent(
            dto.PaymentId,
            dto.FlightBookingId,
            occurred,
            dto.Amount,
            dto.CurrencyCode);
    }

    private sealed record Payload(
        Guid PaymentId,
        Guid FlightBookingId,
        string OccurredAt,
        decimal Amount,
        string CurrencyCode);
}
