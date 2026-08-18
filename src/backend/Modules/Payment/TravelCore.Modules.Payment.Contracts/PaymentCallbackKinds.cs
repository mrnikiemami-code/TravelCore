namespace TravelCore.Modules.Payment.Contracts;

/// <summary>
/// Distinguishes collection vs refund on the shared provider callback without a client success flag (P20-R6).
/// </summary>
public static class PaymentCallbackKinds
{
    public const string HeaderName = "X-TravelCore-Callback-Kind";
    public const string Payment = "payment";
    public const string Refund = "refund";
}
