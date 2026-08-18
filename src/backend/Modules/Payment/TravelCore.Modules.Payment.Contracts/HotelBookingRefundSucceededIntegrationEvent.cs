using NodaTime;

namespace TravelCore.Modules.Payment.Contracts;

/// <summary>
/// Trigger only: Payment-owned full Refund succeeded for a HotelBooking target.
/// Not HotelBooking cancellation and not Tour RefundSucceededIntegrationEvent (P21-R6).
/// </summary>
public sealed record HotelBookingRefundSucceededIntegrationEvent(
    Guid RefundId,
    Guid PaymentId,
    Guid HotelBookingId,
    Instant OccurredAt,
    decimal Amount,
    string CurrencyCode);

public interface IHotelBookingRefundSucceededIntegrationHandler
{
    Task HandleAsync(
        HotelBookingRefundSucceededIntegrationEvent message,
        CancellationToken cancellationToken = default);
}

public static class HotelBookingRefundSuccessOutboxBoundary
{
    public const string MessageType = "HotelBookingRefundSucceededIntegrationEvent";
    public const string DeliverySemantics = "at-least-once";
    public const string LocalEffectSemantics = "idempotent/effectively-once";
    public const bool TransactionalOutboxImplemented = true;
    public const bool EventMeansHotelBookingCancelled = false;
}
