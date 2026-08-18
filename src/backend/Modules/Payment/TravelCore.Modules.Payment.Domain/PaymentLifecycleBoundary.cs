namespace TravelCore.Modules.Payment.Domain;

/// <summary>
/// P20-R2 lifecycle semantics. Authoritative provider verification remains deferred (P20-R3).
/// </summary>
public static class PaymentLifecycleBoundary
{
    public const string PaymentIsNotPaymentAttempt = "Payment != PaymentAttempt";
    public const string PaymentStatusIsNotPaymentAttemptStatus = "PaymentStatus != PaymentAttemptStatus";
    public const string FailedAttemptIsNotFailedPayment = "Failed PaymentAttempt != Failed Payment";
    public const string PaymentSucceededIsNotBookingConfirmed = "PaymentSucceeded != BookingConfirmed";
    public const string CreatedIsNotProviderCreated = "Created != Provider payment created successfully";
    public const bool PaymentFailedStatusImplemented = false;
    public const bool PaymentRefundedStatusImplemented = false;
    public const bool PaymentCancelledStatusImplemented = false;
    public const bool PaymentExpiredStatusImplemented = false;
    public const bool CallerControlledSuccessImplemented = false;
    public const bool PublicSuccessEndpointImplemented = false;
    public const bool ProviderAdapterImplemented = false;
    public const bool RefundImplemented = false;
    public const bool BookingConfirmImplemented = false;
}
