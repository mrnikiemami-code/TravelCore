using NodaTime;

namespace TravelCore.Modules.Payment.Contracts;

/// <summary>
/// Flight-published trigger that a successful Payment cannot complete the FlightBooking.
/// Amount is omitted; PaymentExecutionSnapshot is refund authority (P22-R6).
/// </summary>
public sealed record FlightBookingPaymentCompensationRequiredIntegrationEvent(
    Guid FlightBookingId,
    Guid PaymentId,
    string RecoveryReason,
    Instant OccurredAt);

public interface IFlightBookingPaymentCompensationRequiredHandler
{
    Task HandleAsync(
        FlightBookingPaymentCompensationRequiredIntegrationEvent message,
        CancellationToken cancellationToken = default);
}

public static class FlightBookingCompensationOutboxBoundary
{
    public const string MessageType = "FlightBookingPaymentCompensationRequiredIntegrationEvent";
    public const string DeliverySemantics = "at-least-once";
    public const string LocalEffectSemantics = "idempotent/effectively-once";
    public const bool TransactionalOutboxImplemented = true;
    public const bool EventAmountIsAuthoritative = false;
}
