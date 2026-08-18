using NodaTime;

namespace TravelCore.Modules.Payment.Contracts;

/// <summary>
/// HotelBooking-published trigger that a confirmed cancellation requires a full Refund.
/// Amount is omitted; PaymentExecutionSnapshot is refund authority (P21-R7).
/// Distinct from HotelBookingPaymentCompensationRequiredIntegrationEvent (R6).
/// </summary>
public sealed record HotelBookingCancellationRefundRequiredIntegrationEvent(
    Guid HotelBookingCancellationId,
    Guid HotelBookingId,
    Guid PaymentId,
    Instant OccurredAt);

public interface IHotelBookingCancellationRefundRequiredHandler
{
    Task HandleAsync(
        HotelBookingCancellationRefundRequiredIntegrationEvent message,
        CancellationToken cancellationToken = default);
}

public static class HotelBookingCancellationRefundOutboxBoundary
{
    public const string MessageType = "HotelBookingCancellationRefundRequiredIntegrationEvent";
    public const string DeliverySemantics = "at-least-once";
    public const string LocalEffectSemantics = "idempotent/effectively-once";
    public const bool TransactionalOutboxImplemented = true;
    public const bool EventAmountIsAuthoritative = false;
}
