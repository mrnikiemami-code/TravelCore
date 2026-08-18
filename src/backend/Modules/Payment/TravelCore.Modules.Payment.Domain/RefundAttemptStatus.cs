namespace TravelCore.Modules.Payment.Domain;

/// <summary>
/// Concrete refund-execution attempt lifecycle (TC-P20-T006 / P20-R6).
/// Not RefundStatus. Not PaymentAttemptStatus. Not a provider gateway status.
/// </summary>
public enum RefundAttemptStatus
{
    Created = 1,
    Initiated = 2,
    Succeeded = 3,
    Failed = 4,
}
