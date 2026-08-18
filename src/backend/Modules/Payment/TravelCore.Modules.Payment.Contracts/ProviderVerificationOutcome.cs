namespace TravelCore.Modules.Payment.Contracts;

/// <summary>
/// Provider-neutral verification/status outcome. Not a PaymentStatus and not a provider enum (P20-R3).
/// </summary>
public enum ProviderVerificationOutcome
{
    Succeeded = 1,
    Failed = 2,
    PendingUnknown = 3,
}
