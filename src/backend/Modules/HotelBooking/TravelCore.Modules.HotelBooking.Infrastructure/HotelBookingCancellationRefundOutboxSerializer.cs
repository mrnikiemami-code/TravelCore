using System.Text.Json;
using NodaTime;
using NodaTime.Text;
using TravelCore.Modules.Payment.Contracts;

namespace TravelCore.Modules.HotelBooking.Infrastructure;

internal static class HotelBookingCancellationRefundOutboxSerializer
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);
    private static readonly InstantPattern InstantIso = InstantPattern.ExtendedIso;

    public static string Serialize(HotelBookingCancellationRefundRequiredIntegrationEvent message)
    {
        ArgumentNullException.ThrowIfNull(message);
        return JsonSerializer.Serialize(
            new Payload(
                message.HotelBookingCancellationId,
                message.HotelBookingId,
                message.PaymentId,
                InstantIso.Format(message.OccurredAt)),
            Json);
    }

    public static HotelBookingCancellationRefundRequiredIntegrationEvent Deserialize(string payload)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(payload);
        var dto = JsonSerializer.Deserialize<Payload>(payload, Json)
            ?? throw new InvalidOperationException("Hotel cancellation refund-required outbox payload was empty.");
        var occurred = InstantIso.Parse(dto.OccurredAt).Value;
        return new HotelBookingCancellationRefundRequiredIntegrationEvent(
            dto.HotelBookingCancellationId,
            dto.HotelBookingId,
            dto.PaymentId,
            occurred);
    }

    private sealed record Payload(
        Guid HotelBookingCancellationId,
        Guid HotelBookingId,
        Guid PaymentId,
        string OccurredAt);
}
