namespace TravelCore.Modules.Payment.Contracts;

/// <summary>
/// Provider-neutral verification/status-query result. Reported amount is evidence only;
/// executable mismatch enforcement is deferred to P20-R5.
/// </summary>
public sealed class PaymentVerificationResult
{
    public required ProviderVerificationOutcome Outcome { get; init; }

    public required ProviderKey ProviderKey { get; init; }

    public ProviderRequestReference? RequestReference { get; init; }

    public ProviderTransactionReference? TransactionReference { get; init; }

    public decimal? ReportedAmount { get; init; }

    public string? ReportedCurrencyCode { get; init; }
}
