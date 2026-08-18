using NodaTime;
using TravelCore.Modules.HotelBooking.Domain;
using TravelCore.Modules.Payment.Contracts;

namespace TravelCore.Modules.HotelBooking.Infrastructure;

internal static class HotelBookingCancellationRefundOutboxWriter
{
    public static void Enqueue(
        HotelBookingDbContext db,
        HotelBookingCancellation cancellation,
        Instant now)
    {
        ArgumentNullException.ThrowIfNull(db);
        ArgumentNullException.ThrowIfNull(cancellation);

        if (!cancellation.RequiresFullRefund)
        {
            return;
        }

        if (db.OutboxMessages.Local.Any(x => x.Id == cancellation.Id.Value)
            || db.OutboxMessages.Any(x => x.Id == cancellation.Id.Value))
        {
            return;
        }

        var message = new HotelBookingCancellationRefundRequiredIntegrationEvent(
            cancellation.Id.Value,
            cancellation.HotelBookingId.Value,
            cancellation.PaymentId,
            now);

        db.OutboxMessages.Add(
            HotelBookingOutboxMessage.Create(
                cancellation.Id.Value,
                now,
                HotelBookingCancellationRefundOutboxBoundary.MessageType,
                HotelBookingCancellationRefundOutboxSerializer.Serialize(message)));
    }
}
