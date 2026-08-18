namespace TravelCore.Modules.Booking.Domain;

/// <summary>
/// P19-R2 lifecycle semantics. Executable confirmation remains deferred (P19-R6).
/// </summary>
public static class BookingLifecycleBoundary
{
    public const string PendingMeaning = "Booking transaction exists; not finally confirmed";
    public const bool PendingImpliesCapacityHeld = false;
    public const bool PendingImpliesPaymentPending = false;
    public const bool PendingImpliesQuoteValid = false;
    public const bool UnrestrictedConfirmationImplemented = false;
    public const bool ConfirmedToCancelledImplemented = false;
    public const bool ExpiredStatusImplemented = false;
    public const bool AwaitingPaymentStatusImplemented = false;
    public const bool PaidStatusImplemented = false;
    public const bool RefundedStatusImplemented = false;
    public const string ConfirmedIsNotPaymentSucceeded = "Confirmed != PaymentSucceeded";
    public const string CancelledIsNotRefunded = "Cancelled != Refunded";
    public const string BookingStatusIsNotPaymentStatus = "BookingStatus != PaymentStatus";
    public const string BookingStatusIsNotCapacityStatus = "BookingStatus != CapacityStatus";
    public const string BookingStatusIsNotQuoteStatus = "BookingStatus != QuoteStatus";
}
