namespace TravelCore.Modules.Payment.Domain;

/// <summary>
/// P20-R2 lifecycle semantics with P20-R3 trusted provider-verification boundary.
/// </summary>
public static class PaymentLifecycleBoundary
{
    public const string PaymentIsNotPaymentAttempt = "Payment != PaymentAttempt";
    public const string PaymentStatusIsNotPaymentAttemptStatus = "PaymentStatus != PaymentAttemptStatus";
    public const string FailedAttemptIsNotFailedPayment = "Failed PaymentAttempt != Failed Payment";
    public const string PaymentSucceededIsNotBookingConfirmed = "PaymentSucceeded != BookingConfirmed";
    public const string CreatedIsNotProviderCreated = "Created != Provider payment created successfully";
    public const string BrowserReturnIsNotPaymentSuccess = "BrowserReturn != PaymentSuccess";
    public const string UnverifiedCallbackIsNotPaymentSuccess = "UnverifiedCallback != PaymentSuccess";
    public const string ClientSuccessFlagIsNotPaymentSuccess = "ClientSuccessFlag != PaymentSuccess";
    public const string ProviderRedirectIsNotPaymentSuccess = "ProviderRedirect != PaymentSuccess";
    public const string ProviderReferenceIsNotPaymentId = "ProviderReference != PaymentId";
    public const string ProviderReferenceIsNotPaymentAttemptId = "ProviderReference != PaymentAttemptId";
    public const string NetworkTimeoutIsNotAttemptFailed = "NetworkTimeout != PaymentAttemptFailed";
    public const bool PaymentFailedStatusImplemented = false;
    public const bool PaymentRefundedStatusImplemented = false;
    public const bool PaymentCancelledStatusImplemented = false;
    public const bool PaymentExpiredStatusImplemented = false;
    public const bool CallerControlledSuccessImplemented = false;
    public const bool PublicSuccessEndpointImplemented = false;
    public const bool ProviderAdapterImplemented = false;
    public const bool ProviderPortImplemented = true;
    public const bool RefundImplemented = true;
    public const bool BookingConfirmImplemented = false;
}
