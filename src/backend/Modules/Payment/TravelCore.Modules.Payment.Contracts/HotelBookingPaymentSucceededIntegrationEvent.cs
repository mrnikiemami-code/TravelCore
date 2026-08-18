using NodaTime;

namespace TravelCore.Modules.Payment.Contracts;

/// <summary>
/// Trigger only: Payment succeeded for a HotelBooking target. Not HotelBooking confirmation (P21-R6).
/// Distinct from <see cref="PaymentSucceededIntegrationEvent"/> so Tour consumers never infer HotelBooking.
/// </summary>
public sealed record HotelBookingPaymentSucceededIntegrationEvent(
    Guid PaymentId,
    Guid HotelBookingId,
    Instant OccurredAt,
    decimal Amount,
    string CurrencyCode);

public interface IHotelBookingPaymentSucceededIntegrationHandler
{
    Task HandleAsync(
        HotelBookingPaymentSucceededIntegrationEvent message,
        CancellationToken cancellationToken = default);
}

public static class HotelBookingPaymentSuccessOutboxBoundary
{
    public const string MessageType = "HotelBookingPaymentSucceededIntegrationEvent";
    public const string DeliverySemantics = "at-least-once";
    public const string LocalEffectSemantics = "idempotent/effectively-once";
    public const bool TransactionalOutboxImplemented = true;
    public const bool EventMeansHotelBookingConfirmed = false;
}
