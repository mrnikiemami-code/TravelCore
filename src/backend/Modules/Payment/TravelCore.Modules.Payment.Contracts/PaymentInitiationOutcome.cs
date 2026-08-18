namespace TravelCore.Modules.Payment.Contracts;

/// <summary>
/// Provider-neutral initiation outcome. Network ambiguity is not definitive failure (P20-R3).
/// </summary>
public enum PaymentInitiationOutcome
{
    Initiated = 1,
    DefinitiveFailure = 2,
    Unknown = 3,
}
