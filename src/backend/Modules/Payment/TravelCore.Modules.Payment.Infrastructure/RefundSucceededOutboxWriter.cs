using NodaTime;
using TravelCore.Modules.Payment.Contracts;
using TravelCore.Modules.Payment.Domain;
using TravelCore.Modules.Payment.Infrastructure.Services;

namespace TravelCore.Modules.Payment.Infrastructure;

internal static class RefundSucceededOutboxWriter
{
    public static void EnqueueIfSucceeded(
        PaymentDbContext db,
        Refund refund,
        Instant now,
        VerificationApplyStatus status)
    {
        ArgumentNullException.ThrowIfNull(db);
        ArgumentNullException.ThrowIfNull(refund);

        if (refund.Status != RefundStatus.Succeeded
            || status is not (VerificationApplyStatus.Applied or VerificationApplyStatus.Unchanged))
        {
            return;
        }

        if (db.OutboxMessages.Local.Any(x => x.Id == refund.Id.Value)
            || db.OutboxMessages.Any(x => x.Id == refund.Id.Value))
        {
            return;
        }

        var message = new RefundSucceededIntegrationEvent(
            refund.Id.Value,
            refund.PaymentId.Value,
            refund.Booking.BookingId,
            now,
            refund.Amount.Amount,
            refund.Amount.Currency.Value);

        db.OutboxMessages.Add(
            PaymentOutboxMessage.Create(
                refund.Id.Value,
                now,
                RefundSuccessOutboxBoundary.MessageType,
                RefundSucceededOutboxSerializer.Serialize(message)));
    }
}
