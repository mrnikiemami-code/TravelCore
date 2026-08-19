namespace TravelCore.Modules.DynamicPackage.Domain;

/// <summary>
/// Transient (non-persistent, non-transactional) confirmation evidence for a DynamicPackage.
/// This is NOT a saga instance and does not implement any reservation/payment execution.
///
/// Scope (P23-T007):
/// - Define when a composed Flight+Hotel package is considered "confirmed" (boundary only)
/// - Maintain currency consistency via PackageMonetarySnapshot (ADR 0003)
/// </summary>
public sealed class TransientPackageConfirmation
{
    private TransientPackageConfirmation(
        TransientPackageCandidate candidate,
        PackageMonetarySnapshot monetary)
    {
        Candidate = candidate ?? throw new ArgumentNullException(nameof(candidate));
        Monetary = monetary ?? throw new ArgumentNullException(nameof(monetary));
        Confirmed = true;
    }

    public TransientPackageCandidate Candidate { get; }

    public PackageMonetarySnapshot Monetary { get; }

    public bool Confirmed { get; }

    public static TransientPackageConfirmation ConfirmedPackage(
        TransientPackageCandidate candidate,
        PackageMonetarySnapshot monetary)
    {
        return new TransientPackageConfirmation(candidate, monetary);
    }
}

