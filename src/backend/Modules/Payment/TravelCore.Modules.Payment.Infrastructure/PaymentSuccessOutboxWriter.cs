using NodaTime;
using TravelCore.Modules.Payment.Contracts;
using TravelCore.Modules.Payment.Domain;
using TravelCore.Modules.Payment.Infrastructure.Services;
using PaymentAggregate = TravelCore.Modules.Payment.Domain.Payment;

namespace TravelCore.Modules.Payment.Infrastructure;

internal static class PaymentSuccessOutboxWriter
{
    public static void EnqueueIfSucceeded(
        PaymentDbContext db,
        PaymentAggregate payment,
        Instant now,
        VerificationApplyStatus status)
    {
        ArgumentNullException.ThrowIfNull(db);
        ArgumentNullException.ThrowIfNull(payment);

        if (payment.Status != PaymentStatus.Succeeded
            || payment.ExecutionSnapshot is null
            || status is not (VerificationApplyStatus.Applied or VerificationApplyStatus.Unchanged))
        {
            return;
        }

        if (db.OutboxMessages.Local.Any(x => x.Id == payment.Id.Value)
            || db.OutboxMessages.Any(x => x.Id == payment.Id.Value))
        {
            return;
        }

        if (payment.HotelBooking is { } hotel)
        {
            var hotelMessage = new HotelBookingPaymentSucceededIntegrationEvent(
                payment.Id.Value,
                hotel.HotelBookingId,
                now,
                payment.ExecutionSnapshot.Amount.Amount,
                payment.ExecutionSnapshot.Amount.Currency.Value);

            db.OutboxMessages.Add(
                PaymentOutboxMessage.Create(
                    payment.Id.Value,
                    now,
                    HotelBookingPaymentSuccessOutboxBoundary.MessageType,
                    HotelBookingPaymentSucceededOutboxSerializer.Serialize(hotelMessage)));
            return;
        }

        var message = new PaymentSucceededIntegrationEvent(
            payment.Id.Value,
            payment.Booking?.BookingId
                ?? throw new InvalidOperationException("Tour Payment is missing BookingReference."),
            now,
            payment.ExecutionSnapshot.Amount.Amount,
            payment.ExecutionSnapshot.Amount.Currency.Value);

        db.OutboxMessages.Add(
            PaymentOutboxMessage.Create(
                payment.Id.Value,
                now,
                PaymentSuccessOutboxBoundary.MessageType,
                PaymentSucceededOutboxSerializer.Serialize(message)));
    }
}
