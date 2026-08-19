using NodaTime;

namespace TravelCore.Modules.Payment.Contracts;

/// <summary>
/// FlightBooking-published trigger that a confirmed cancellation requires a full Refund.
/// Amount is omitted; PaymentExecutionSnapshot is refund authority (P22-R7).
/// Distinct from FlightBookingPaymentCompensationRequiredIntegrationEvent (R6).
/// </summary>
public sealed record FlightBookingCancellationRefundRequiredIntegrationEvent(
    Guid FlightBookingCancellationId,
    Guid FlightBookingId,
    Guid PaymentId,
    Instant OccurredAt);

public interface IFlightBookingCancellationRefundRequiredHandler
{
    Task HandleAsync(
        FlightBookingCancellationRefundRequiredIntegrationEvent message,
        CancellationToken cancellationToken = default);
}

public static class FlightBookingCancellationRefundOutboxBoundary
{
    public const string MessageType = "FlightBookingCancellationRefundRequiredIntegrationEvent";
    public const string DeliverySemantics = "at-least-once";
    public const string LocalEffectSemantics = "idempotent/effectively-once";
    public const bool TransactionalOutboxImplemented = true;
    public const bool EventAmountIsAuthoritative = false;
}
