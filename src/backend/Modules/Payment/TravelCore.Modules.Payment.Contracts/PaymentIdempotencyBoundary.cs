namespace TravelCore.Modules.Payment.Contracts;

/// <summary>
/// P20-R4 idempotency and reconciliation locks.
/// </summary>
public static class PaymentIdempotencyBoundary
{
    public const string OneBookingOneLogicalPayment = "Booking 1 -> 1 logical Payment";
    public const string RetryIsAttemptNotPayment = "Retry = PaymentAttempt, not new Payment";
    public const string ExactlyOnceExternalPayment = "NOT ASSUMED";
    public const string AmbiguousIsNotFailedAttempt = "Unknown/Ambiguous provider outcome != PaymentAttempt.Failed";
    public const string ReconciliationIsNotSettlement = "Reconciliation != Settlement";
    public const string ReconciliationIsNotAccounting = "Reconciliation != Accounting";
    public const bool UniqueLogicalPaymentPerBookingImplemented = true;
    public const bool ProcessLocalIdempotencyAuthorityImplemented = false;
    public const bool AutomaticRetryOnAmbiguityImplemented = false;
    public const bool AutomaticProviderFailoverImplemented = false;
    public const bool ReconciliationSchedulerImplemented = false;
}
