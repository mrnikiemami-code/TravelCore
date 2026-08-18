using NodaTime;

namespace TravelCore.Modules.Payment.Contracts;

/// <summary>
/// Trigger only: Payment-owned full Refund succeeded. Not Booking cancellation (P20-R6).
/// </summary>
public sealed record RefundSucceededIntegrationEvent(
    Guid RefundId,
    Guid PaymentId,
    Guid BookingId,
    Instant OccurredAt,
    decimal Amount,
    string CurrencyCode);

public interface IRefundSucceededIntegrationHandler
{
    Task HandleAsync(RefundSucceededIntegrationEvent message, CancellationToken cancellationToken = default);
}

public static class RefundSuccessOutboxBoundary
{
    public const string MessageType = "RefundSucceededIntegrationEvent";
    public const string DeliverySemantics = "at-least-once";
    public const string LocalEffectSemantics = "idempotent/effectively-once";
    public const bool TransactionalOutboxImplemented = true;
    public const bool EventMeansBookingCancelled = false;
    public const string RefundSucceededIsNotBookingCancelled = "RefundSucceeded != BookingCancelled";
}
