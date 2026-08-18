using System.Text.Json;
using NodaTime;
using NodaTime.Text;
using TravelCore.Modules.Payment.Contracts;

namespace TravelCore.Modules.Payment.Infrastructure;

internal static class PaymentSucceededOutboxSerializer
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);
    private static readonly InstantPattern InstantIso = InstantPattern.ExtendedIso;

    public static string Serialize(PaymentSucceededIntegrationEvent message)
    {
        ArgumentNullException.ThrowIfNull(message);
        return JsonSerializer.Serialize(
            new Payload(
                message.PaymentId,
                message.BookingId,
                InstantIso.Format(message.OccurredAt),
                message.Amount,
                message.CurrencyCode),
            Json);
    }

    public static PaymentSucceededIntegrationEvent Deserialize(string payload)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(payload);
        var dto = JsonSerializer.Deserialize<Payload>(payload, Json)
            ?? throw new InvalidOperationException("PaymentSucceeded outbox payload was empty.");
        var occurred = InstantIso.Parse(dto.OccurredAt).Value;
        return new PaymentSucceededIntegrationEvent(
            dto.PaymentId,
            dto.BookingId,
            occurred,
            dto.Amount,
            dto.CurrencyCode);
    }

    private sealed record Payload(
        Guid PaymentId,
        Guid BookingId,
        string OccurredAt,
        decimal Amount,
        string CurrencyCode);
}
