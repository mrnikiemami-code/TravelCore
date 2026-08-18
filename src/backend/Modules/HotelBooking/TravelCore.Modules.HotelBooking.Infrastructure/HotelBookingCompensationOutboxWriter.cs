using NodaTime;
using TravelCore.Modules.HotelBooking.Domain;
using TravelCore.Modules.Payment.Contracts;

namespace TravelCore.Modules.HotelBooking.Infrastructure;

internal static class HotelBookingCompensationOutboxWriter
{
    public static void Enqueue(
        HotelBookingDbContext db,
        HotelBookingPaymentCompensationEvidence evidence,
        Instant now)
    {
        ArgumentNullException.ThrowIfNull(db);
        ArgumentNullException.ThrowIfNull(evidence);

        if (db.OutboxMessages.Local.Any(x => x.Id == evidence.PaymentId)
            || db.OutboxMessages.Any(x => x.Id == evidence.PaymentId))
        {
            return;
        }

        var message = new HotelBookingPaymentCompensationRequiredIntegrationEvent(
            evidence.HotelBookingId.Value,
            evidence.PaymentId,
            evidence.Reason.ToString(),
            now);

        db.OutboxMessages.Add(
            HotelBookingOutboxMessage.Create(
                evidence.PaymentId,
                now,
                HotelBookingCompensationOutboxBoundary.MessageType,
                HotelBookingCompensationOutboxSerializer.Serialize(message)));
    }
}
