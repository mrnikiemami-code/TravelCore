namespace TravelCore.Modules.Payment.Domain;

/// <summary>
/// Concrete execution-attempt lifecycle (TC-P20-T002 / P20-R2).
/// Not PaymentStatus. Not a provider-specific gateway status.
/// </summary>
public enum PaymentAttemptStatus
{
    Created = 1,
    Initiated = 2,
    Succeeded = 3,
    Failed = 4,
}
