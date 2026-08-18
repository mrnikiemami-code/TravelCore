using System.Text.Json;
using NodaTime;
using NodaTime.Text;
using TravelCore.Modules.Payment.Contracts;

namespace TravelCore.Modules.Booking.Infrastructure;

internal static class BookingCompensationOutboxSerializer
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);
    private static readonly InstantPattern InstantIso = InstantPattern.ExtendedIso;

    public static string Serialize(BookingPaymentCompensationRequiredIntegrationEvent message)
    {
        ArgumentNullException.ThrowIfNull(message);
        return JsonSerializer.Serialize(
            new Payload(
                message.BookingId,
                message.PaymentId,
                message.RecoveryReason,
                InstantIso.Format(message.OccurredAt)),
            Json);
    }

    public static BookingPaymentCompensationRequiredIntegrationEvent Deserialize(string payload)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(payload);
        var dto = JsonSerializer.Deserialize<Payload>(payload, Json)
            ?? throw new InvalidOperationException("Compensation-required outbox payload was empty.");
        var occurred = InstantIso.Parse(dto.OccurredAt).Value;
        return new BookingPaymentCompensationRequiredIntegrationEvent(
            dto.BookingId,
            dto.PaymentId,
            dto.RecoveryReason,
            occurred);
    }

    private sealed record Payload(
        Guid BookingId,
        Guid PaymentId,
        string RecoveryReason,
        string OccurredAt);
}
