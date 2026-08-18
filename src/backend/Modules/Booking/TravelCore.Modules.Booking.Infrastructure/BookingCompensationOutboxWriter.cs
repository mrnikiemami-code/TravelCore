using NodaTime;
using TravelCore.Modules.Booking.Domain;
using TravelCore.Modules.Payment.Contracts;

namespace TravelCore.Modules.Booking.Infrastructure;

internal static class BookingCompensationOutboxWriter
{
    public static void Enqueue(
        BookingDbContext db,
        BookingConfirmationRecoveryIssue issue,
        Instant now)
    {
        ArgumentNullException.ThrowIfNull(db);
        ArgumentNullException.ThrowIfNull(issue);

        if (db.OutboxMessages.Local.Any(x => x.Id == issue.PaymentId)
            || db.OutboxMessages.Any(x => x.Id == issue.PaymentId))
        {
            return;
        }

        var message = new BookingPaymentCompensationRequiredIntegrationEvent(
            issue.BookingId.Value,
            issue.PaymentId,
            issue.Reason.ToString(),
            now);

        db.OutboxMessages.Add(
            BookingOutboxMessage.Create(
                issue.PaymentId,
                now,
                BookingCompensationOutboxBoundary.MessageType,
                BookingCompensationOutboxSerializer.Serialize(message)));
    }
}
