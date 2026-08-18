namespace TravelCore.Modules.Payment.Domain;

/// <summary>
/// Minimal operational kinds for Payment/provider divergence (P20-R4). Not a ticket workflow.
/// </summary>
public enum PaymentReconciliationIssueKind
{
    ContradictoryProviderState = 1,
    UnknownProviderTransaction = 2,
}
