namespace TravelCore.Modules.CommercialFinance.Contracts;

/// <summary>
/// P39 idempotency posture: strict idempotent consumption per source event (Q12 derived).
/// No automatic Payment event handlers in T006 skeleton.
/// </summary>
public static class CommercialFinanceIdempotencyBoundary
{
    public const string StrictSourceEventConsumption =
        "One obligation-side consumption record per source event correlation key";
    public const string PaymentSucceededHandlerImplemented = "NOT IMPLEMENTED";
    public const string RefundHandlerImplemented = "NOT IMPLEMENTED";
    public const bool AutomaticPaymentEventHandlersImplemented = false;
    public const bool EventConsumptionPersistenceImplemented = true;
}
