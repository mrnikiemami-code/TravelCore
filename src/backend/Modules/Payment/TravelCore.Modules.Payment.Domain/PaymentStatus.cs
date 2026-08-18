namespace TravelCore.Modules.Payment.Domain;

/// <summary>
/// Logical Payment collection lifecycle (TC-P20-T002 / P20-R2).
/// Failed attempts do not add PaymentStatus.Failed. Refunded is not a PaymentStatus.
/// </summary>
public enum PaymentStatus
{
    Pending = 1,
    Succeeded = 2,
}
