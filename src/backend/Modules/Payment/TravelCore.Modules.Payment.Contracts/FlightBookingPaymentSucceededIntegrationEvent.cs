using NodaTime;

namespace TravelCore.Modules.Payment.Contracts;

/// <summary>
/// Trigger only: Payment succeeded for a FlightBooking target. Not FlightBooking confirmation (P22-R6).
/// Distinct from Tour/Hotel events so consumers never infer the wrong target.
/// </summary>
public sealed record FlightBookingPaymentSucceededIntegrationEvent(
    Guid PaymentId,
    Guid FlightBookingId,
    Instant OccurredAt,
    decimal Amount,
    string CurrencyCode);

public interface IFlightBookingPaymentSucceededIntegrationHandler
{
    Task HandleAsync(
        FlightBookingPaymentSucceededIntegrationEvent message,
        CancellationToken cancellationToken = default);
}

public static class FlightBookingPaymentSuccessOutboxBoundary
{
    public const string MessageType = "FlightBookingPaymentSucceededIntegrationEvent";
    public const string DeliverySemantics = "at-least-once";
    public const string LocalEffectSemantics = "idempotent/effectively-once";
    public const bool TransactionalOutboxImplemented = true;
    public const bool EventMeansFlightBookingConfirmed = false;
}
