using System.Text.Json;
using NodaTime;
using NodaTime.Text;
using TravelCore.Modules.Payment.Contracts;

namespace TravelCore.Modules.Payment.Infrastructure;

internal static class HotelBookingPaymentSucceededOutboxSerializer
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);
    private static readonly InstantPattern InstantIso = InstantPattern.ExtendedIso;

    public static string Serialize(HotelBookingPaymentSucceededIntegrationEvent message)
    {
        ArgumentNullException.ThrowIfNull(message);
        return JsonSerializer.Serialize(
            new Payload(
                message.PaymentId,
                message.HotelBookingId,
                InstantIso.Format(message.OccurredAt),
                message.Amount,
                message.CurrencyCode),
            Json);
    }

    public static HotelBookingPaymentSucceededIntegrationEvent Deserialize(string payload)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(payload);
        var dto = JsonSerializer.Deserialize<Payload>(payload, Json)
            ?? throw new InvalidOperationException("HotelBooking PaymentSucceeded outbox payload was empty.");
        var occurred = InstantIso.Parse(dto.OccurredAt).Value;
        return new HotelBookingPaymentSucceededIntegrationEvent(
            dto.PaymentId,
            dto.HotelBookingId,
            occurred,
            dto.Amount,
            dto.CurrencyCode);
    }

    private sealed record Payload(
        Guid PaymentId,
        Guid HotelBookingId,
        string OccurredAt,
        decimal Amount,
        string CurrencyCode);
}
