namespace TravelCore.Modules.Payment.Contracts;

/// <summary>
/// Adapter-owned callback verification. Core must ignore unverified envelopes (P20-R3).
/// </summary>
public sealed class PaymentCallbackVerification
{
    public required bool IsVerified { get; init; }

    public PaymentVerificationResult? Result { get; init; }

    public static PaymentCallbackVerification Unverified() => new()
    {
        IsVerified = false,
        Result = null,
    };

    public static PaymentCallbackVerification Verified(PaymentVerificationResult result)
    {
        ArgumentNullException.ThrowIfNull(result);
        return new PaymentCallbackVerification
        {
            IsVerified = true,
            Result = result,
        };
    }
}
