namespace TravelCore.Modules.Payment.Contracts;

/// <summary>
/// Trusted server-side verification/status query. Not a browser return payload (P20-R3).
/// </summary>
public sealed record PaymentVerificationRequest(
    ProviderKey ProviderKey,
    ProviderRequestReference? RequestReference,
    ProviderTransactionReference? TransactionReference);
