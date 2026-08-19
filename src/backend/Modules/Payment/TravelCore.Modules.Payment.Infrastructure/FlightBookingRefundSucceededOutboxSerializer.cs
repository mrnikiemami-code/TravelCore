using System.Text.Json;
using NodaTime;
using NodaTime.Text;
using TravelCore.Modules.Payment.Contracts;

namespace TravelCore.Modules.Payment.Infrastructure;

internal static class FlightBookingRefundSucceededOutboxSerializer
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);
    private static readonly InstantPattern InstantIso = InstantPattern.ExtendedIso;

    public static string Serialize(FlightBookingRefundSucceededIntegrationEvent message)
    {
        ArgumentNullException.ThrowIfNull(message);
        return JsonSerializer.Serialize(
            new Payload(
                message.RefundId,
                message.PaymentId,
                message.FlightBookingId,
                InstantIso.Format(message.OccurredAt),
                message.Amount,
                message.CurrencyCode),
            Json);
    }

    public static FlightBookingRefundSucceededIntegrationEvent Deserialize(string payload)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(payload);
        var dto = JsonSerializer.Deserialize<Payload>(payload, Json)
            ?? throw new InvalidOperationException("FlightBooking RefundSucceeded outbox payload was empty.");
        var occurred = InstantIso.Parse(dto.OccurredAt).Value;
        return new FlightBookingRefundSucceededIntegrationEvent(
            dto.RefundId,
            dto.PaymentId,
            dto.FlightBookingId,
            occurred,
            dto.Amount,
            dto.CurrencyCode);
    }

    private sealed record Payload(
        Guid RefundId,
        Guid PaymentId,
        Guid FlightBookingId,
        string OccurredAt,
        decimal Amount,
        string CurrencyCode);
}
