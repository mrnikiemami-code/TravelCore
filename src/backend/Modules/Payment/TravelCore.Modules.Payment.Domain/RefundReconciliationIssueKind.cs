namespace TravelCore.Modules.Payment.Domain;

/// <summary>
/// Operational refund/provider divergence. Not settlement or accounting (P20-R6).
/// </summary>
public enum RefundReconciliationIssueKind
{
    ContradictoryProviderState = 1,
    UnknownProviderTransaction = 2,
    AmountMismatch = 3,
    CurrencyMismatch = 4,
}
