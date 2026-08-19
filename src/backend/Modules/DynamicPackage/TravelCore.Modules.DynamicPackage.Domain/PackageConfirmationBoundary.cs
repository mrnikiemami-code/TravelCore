namespace TravelCore.Modules.DynamicPackage.Domain;

/// <summary>
/// P23-T007: confirmation boundary posture for DynamicPackage.
/// This phase only defines the "confirmed" meaning and consistency constraints.
/// No persistence, no public APIs, no reservation/payment execution is implemented here.
/// </summary>
public static class PackageConfirmationBoundary
{
    public const bool ConfirmationModelImplemented = true;

    /// <summary>
    /// Confirmation is not a distributed transaction.
    /// </summary>
    public const bool DistributedTransactionAllowed = false;

    public const bool SagaImplemented = false;

    public const bool CompensationImplemented = false;
}

