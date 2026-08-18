namespace TravelCore.Modules.Payment.Contracts;

/// <summary>
/// Provider-neutral verification/status-query result. When PaymentExecutionSnapshot exists,
/// omitted or mismatched amount/currency cannot produce Payment success (P20-R5).
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
