namespace TravelCore.Modules.Payment.Domain;

/// <summary>
/// P20-R6: Refund is a distinct Payment-owned full-return obligation.
/// </summary>
public static class PaymentRefundBoundary
{
    public const string PaymentIsNotRefund = "Payment != Refund";
    public const string PaymentSucceededIsNotRefundSucceeded = "PaymentSucceeded != RefundSucceeded";
    public const string RefundSucceededIsNotBookingCancelled = "RefundSucceeded != BookingCancelled";
    public const string RefundIsNotBookingCancellationPolicy = "Refund != Booking cancellation policy";
    public const string RefundIsNotSettlement = "Refund != Settlement";
    public const string RefundIsNotAccountingLedger = "Refund != Accounting Ledger";
    public const string RefundAttemptIsNotRefund = "RefundAttempt != Refund";
    public const string FailedRefundAttemptIsNotFailedRefund = "Failed RefundAttempt != Failed logical Refund";
    public const bool PaymentRefundedStatusImplemented = false;
    public const bool PartialRefundImplemented = false;
    public const bool MultipleIndependentRefundsImplemented = false;
    public const bool ConfirmedBookingCancellationImplemented = false;
    public const bool ConsumedHoldReversalImplemented = false;
    public const bool PublicRefundApiImplemented = false;
    public const bool RefundAggregateImplemented = true;
}
