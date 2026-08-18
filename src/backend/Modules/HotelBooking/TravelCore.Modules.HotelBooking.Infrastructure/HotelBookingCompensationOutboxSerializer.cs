using System.Text.Json;
using NodaTime;
using NodaTime.Text;
using TravelCore.Modules.Payment.Contracts;

namespace TravelCore.Modules.HotelBooking.Infrastructure;

internal static class HotelBookingCompensationOutboxSerializer
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);
    private static readonly InstantPattern InstantIso = InstantPattern.ExtendedIso;

    public static string Serialize(HotelBookingPaymentCompensationRequiredIntegrationEvent message)
    {
        ArgumentNullException.ThrowIfNull(message);
        return JsonSerializer.Serialize(
            new Payload(
                message.HotelBookingId,
                message.PaymentId,
                message.RecoveryReason,
                InstantIso.Format(message.OccurredAt)),
            Json);
    }

    public static HotelBookingPaymentCompensationRequiredIntegrationEvent Deserialize(string payload)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(payload);
        var dto = JsonSerializer.Deserialize<Payload>(payload, Json)
            ?? throw new InvalidOperationException("Hotel compensation-required outbox payload was empty.");
        var occurred = InstantIso.Parse(dto.OccurredAt).Value;
        return new HotelBookingPaymentCompensationRequiredIntegrationEvent(
            dto.HotelBookingId,
            dto.PaymentId,
            dto.RecoveryReason,
            occurred);
    }

    private sealed record Payload(
        Guid HotelBookingId,
        Guid PaymentId,
        string RecoveryReason,
        string OccurredAt);
}
