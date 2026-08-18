using NodaTime;

namespace TravelCore.Modules.Payment.Contracts;

/// <summary>
/// Trigger only: Payment monetary collection succeeded. Not Booking confirmation (P20-R5).
/// </summary>
public sealed record PaymentSucceededIntegrationEvent(
    Guid PaymentId,
    Guid BookingId,
    Instant OccurredAt,
    decimal Amount,
    string CurrencyCode);

public interface IPaymentSucceededIntegrationHandler
{
    Task HandleAsync(PaymentSucceededIntegrationEvent message, CancellationToken cancellationToken = default);
}

public static class PaymentSuccessOutboxBoundary
{
    public const string MessageType = "PaymentSucceededIntegrationEvent";
    public const string DeliverySemantics = "at-least-once";
    public const string LocalEffectSemantics = "idempotent/effectively-once";
    public const bool TransactionalOutboxImplemented = true;
    public const bool EventMeansBookingConfirmed = false;
}
