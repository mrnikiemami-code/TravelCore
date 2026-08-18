using NodaTime;

namespace TravelCore.Modules.Payment.Contracts;

/// <summary>
/// Booking-published trigger that a successful Payment cannot be confirmed. Amount is omitted (P20-R6).
/// </summary>
public sealed record BookingPaymentCompensationRequiredIntegrationEvent(
    Guid BookingId,
    Guid PaymentId,
    string RecoveryReason,
    Instant OccurredAt);

public interface IBookingPaymentCompensationRequiredHandler
{
    Task HandleAsync(
        BookingPaymentCompensationRequiredIntegrationEvent message,
        CancellationToken cancellationToken = default);
}

public static class BookingCompensationOutboxBoundary
{
    public const string MessageType = "BookingPaymentCompensationRequiredIntegrationEvent";
    public const string DeliverySemantics = "at-least-once";
    public const string LocalEffectSemantics = "idempotent/effectively-once";
    public const bool TransactionalOutboxImplemented = true;
    public const bool EventAmountIsAuthoritative = false;
}
