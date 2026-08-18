namespace TravelCore.Modules.Payment.Domain;

/// <summary>
/// Logical full-return obligation lifecycle (TC-P20-T006 / P20-R6).
/// Failed attempts do not add RefundStatus.Failed. Not PaymentStatus.
/// </summary>
public enum RefundStatus
{
    Pending = 1,
    Succeeded = 2,
}
