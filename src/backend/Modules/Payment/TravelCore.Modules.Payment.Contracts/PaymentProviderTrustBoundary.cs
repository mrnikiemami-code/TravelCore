namespace TravelCore.Modules.Payment.Contracts;

/// <summary>
/// P20-R3 trust and provider-neutrality locks.
/// </summary>
public static class PaymentProviderTrustBoundary
{
    public const string BrowserReturnIsNotPaymentSuccess = "BrowserReturn != PaymentSuccess";
    public const string UnverifiedCallbackIsNotPaymentSuccess = "UnverifiedCallback != PaymentSuccess";
    public const string ClientSuccessFlagIsNotPaymentSuccess = "ClientSuccessFlag != PaymentSuccess";
    public const string ProviderRedirectIsNotPaymentSuccess = "ProviderRedirect != PaymentSuccess";
    public const string ProviderReferenceIsNotPaymentId = "ProviderReference != PaymentId";
    public const string ProviderReferenceIsNotPaymentAttemptId = "ProviderReference != PaymentAttemptId";
    public const string NetworkTimeoutIsNotAttemptFailed = "NetworkTimeout != PaymentAttemptFailed";
    public const string NamedProviderSelected = "NONE";
    public const string AmountMismatchEnforcement = "DeferredToR5";
    public const bool ProviderPortImplemented = true;
    public const bool NamedProductionAdapterImplemented = false;
    public const bool ProductionFakeProviderRegistered = false;
    public const bool AmountMismatchEnforcementImplemented = false;
}
