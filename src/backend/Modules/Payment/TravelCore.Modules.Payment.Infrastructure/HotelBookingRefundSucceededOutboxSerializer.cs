using System.Text.Json;
using NodaTime;
using NodaTime.Text;
using TravelCore.Modules.Payment.Contracts;

namespace TravelCore.Modules.Payment.Infrastructure;

internal static class HotelBookingRefundSucceededOutboxSerializer
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);
    private static readonly InstantPattern InstantIso = InstantPattern.ExtendedIso;

    public static string Serialize(HotelBookingRefundSucceededIntegrationEvent message)
    {
        ArgumentNullException.ThrowIfNull(message);
        return JsonSerializer.Serialize(
            new Payload(
                message.RefundId,
                message.PaymentId,
                message.HotelBookingId,
                InstantIso.Format(message.OccurredAt),
                message.Amount,
                message.CurrencyCode),
            Json);
    }

    public static HotelBookingRefundSucceededIntegrationEvent Deserialize(string payload)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(payload);
        var dto = JsonSerializer.Deserialize<Payload>(payload, Json)
            ?? throw new InvalidOperationException("HotelBooking RefundSucceeded outbox payload was empty.");
        var occurred = InstantIso.Parse(dto.OccurredAt).Value;
        return new HotelBookingRefundSucceededIntegrationEvent(
            dto.RefundId,
            dto.PaymentId,
            dto.HotelBookingId,
            occurred,
            dto.Amount,
            dto.CurrencyCode);
    }

    private sealed record Payload(
        Guid RefundId,
        Guid PaymentId,
        Guid HotelBookingId,
        string OccurredAt,
        decimal Amount,
        string CurrencyCode);
}
