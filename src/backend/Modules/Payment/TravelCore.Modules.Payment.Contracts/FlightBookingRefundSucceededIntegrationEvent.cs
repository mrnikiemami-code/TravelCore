using NodaTime;

namespace TravelCore.Modules.Payment.Contracts;

/// <summary>
/// Trigger only: full Refund succeeded for a FlightBooking Payment. Not customer cancellation (P22-R6).
/// </summary>
public sealed record FlightBookingRefundSucceededIntegrationEvent(
    Guid RefundId,
    Guid PaymentId,
    Guid FlightBookingId,
    Instant OccurredAt,
    decimal Amount,
    string CurrencyCode);

public interface IFlightBookingRefundSucceededIntegrationHandler
{
    Task HandleAsync(
        FlightBookingRefundSucceededIntegrationEvent message,
        CancellationToken cancellationToken = default);
}

public static class FlightBookingRefundSuccessOutboxBoundary
{
    public const string MessageType = "FlightBookingRefundSucceededIntegrationEvent";
    public const string DeliverySemantics = "at-least-once";
    public const string LocalEffectSemantics = "idempotent/effectively-once";
    public const bool TransactionalOutboxImplemented = true;
    public const bool EventMeansFlightBookingCancelled = false;
}
