namespace TravelCore.Modules.Payment.Contracts;

/// <summary>
/// Provider-neutral initiation result. Secrets must not appear here (P20-R3).
/// </summary>
public sealed class PaymentInitiationResult
{
    public required PaymentInitiationOutcome Outcome { get; init; }

    public required ProviderKey ProviderKey { get; init; }

    public ProviderRequestReference? RequestReference { get; init; }

    public ProviderTransactionReference? TransactionReference { get; init; }

    public Uri? RedirectUri { get; init; }
}
