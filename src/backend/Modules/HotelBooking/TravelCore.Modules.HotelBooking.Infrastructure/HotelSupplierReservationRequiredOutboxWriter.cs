using System.Text.Json;
using NodaTime;
using NodaTime.Text;
using TravelCore.Modules.HotelBooking.Domain;

namespace TravelCore.Modules.HotelBooking.Infrastructure;

internal static class HotelSupplierReservationRequiredOutboxBoundary
{
    public const string MessageType = "HotelSupplierReservationRequired";
}

internal sealed record HotelSupplierReservationRequiredWorkItem(
    Guid HotelBookingId,
    Guid PaymentId,
    Instant OccurredAt);

internal static class HotelSupplierReservationRequiredOutboxWriter
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);
    private static readonly InstantPattern InstantIso = InstantPattern.ExtendedIso;

    public static void Enqueue(
        HotelBookingDbContext db,
        HotelBookingId hotelBookingId,
        Guid paymentId,
        Instant now)
    {
        ArgumentNullException.ThrowIfNull(db);
        if (paymentId == Guid.Empty)
        {
            throw new ArgumentException("PaymentId cannot be empty.", nameof(paymentId));
        }

        if (db.OutboxMessages.Local.Any(x => x.Id == hotelBookingId.Value)
            || db.OutboxMessages.Any(x => x.Id == hotelBookingId.Value))
        {
            return;
        }

        var payload = JsonSerializer.Serialize(
            new Payload(hotelBookingId.Value, paymentId, InstantIso.Format(now)),
            Json);
        db.OutboxMessages.Add(
            HotelBookingOutboxMessage.Create(
                hotelBookingId.Value,
                now,
                HotelSupplierReservationRequiredOutboxBoundary.MessageType,
                payload));
    }

    public static HotelSupplierReservationRequiredWorkItem Deserialize(string payload)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(payload);
        var dto = JsonSerializer.Deserialize<Payload>(payload, Json)
            ?? throw new InvalidOperationException("Reservation-required outbox payload was empty.");
        return new HotelSupplierReservationRequiredWorkItem(
            dto.HotelBookingId,
            dto.PaymentId,
            InstantIso.Parse(dto.OccurredAt).Value);
    }

    private sealed record Payload(Guid HotelBookingId, Guid PaymentId, string OccurredAt);
}
