using NodaTime;

namespace TravelCore.Modules.Payment.Contracts;

/// <summary>
/// HotelBooking-published trigger that a successful Payment cannot complete the stay.
/// Amount is omitted; PaymentExecutionSnapshot is refund authority (P21-R6).
/// </summary>
public sealed record HotelBookingPaymentCompensationRequiredIntegrationEvent(
    Guid HotelBookingId,
    Guid PaymentId,
    string RecoveryReason,
    Instant OccurredAt);

public interface IHotelBookingPaymentCompensationRequiredHandler
{
    Task HandleAsync(
        HotelBookingPaymentCompensationRequiredIntegrationEvent message,
        CancellationToken cancellationToken = default);
}

public static class HotelBookingCompensationOutboxBoundary
{
    public const string MessageType = "HotelBookingPaymentCompensationRequiredIntegrationEvent";
    public const string DeliverySemantics = "at-least-once";
    public const string LocalEffectSemantics = "idempotent/effectively-once";
    public const bool TransactionalOutboxImplemented = true;
    public const bool EventAmountIsAuthoritative = false;
}
