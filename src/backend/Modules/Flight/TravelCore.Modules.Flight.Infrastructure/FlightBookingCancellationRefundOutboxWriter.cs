using NodaTime;
using TravelCore.Modules.Flight.Domain;
using TravelCore.Modules.Payment.Contracts;

namespace TravelCore.Modules.Flight.Infrastructure;

internal static class FlightBookingCancellationRefundOutboxWriter
{
    public static void Enqueue(
        FlightDbContext db,
        FlightBookingCancellation cancellation,
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

        var message = new FlightBookingCancellationRefundRequiredIntegrationEvent(
            cancellation.Id.Value,
            cancellation.FlightBookingId.Value,
            cancellation.PaymentId,
            now);

        db.OutboxMessages.Add(
            FlightOutboxMessage.Create(
                cancellation.Id.Value,
                now,
                FlightBookingCancellationRefundOutboxBoundary.MessageType,
                FlightBookingCancellationRefundOutboxSerializer.Serialize(message)));
    }
}
