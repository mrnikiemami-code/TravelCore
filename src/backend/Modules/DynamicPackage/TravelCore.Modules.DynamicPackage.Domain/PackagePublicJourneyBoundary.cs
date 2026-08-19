namespace TravelCore.Modules.DynamicPackage.Domain;

/// <summary>
/// P23-T008: DynamicPackage public journey boundary posture.
/// This phase defines what the public journey concept may do (and may not do)
/// without creating production endpoints, suppliers, or real payment flows.
/// </summary>
public static class PackagePublicJourneyBoundary
{
    /// <summary>
    /// Allowed: package discovery concepts and transient selection/review boundaries.
    /// </summary>
    public const bool DiscoveryConceptAllowed = true;

    /// <summary>
    /// Allowed: transient composition selection + review boundary.
    /// </summary>
    public const bool TransientCompositionSelectionAllowed = true;

    /// <summary>
    /// Not allowed: generic CRUD, operational mutation.
    /// </summary>
    public const bool OperationalMutationAllowed = false;

    /// <summary>
    /// Token strategy: do NOT reuse component tokens.
    /// </summary>
    public const bool ReuseFlightToken = false;

    public const bool ReuseHotelToken = false;

    /// <summary>
    /// SEO posture: transactional pages are noindex; discovery pages can be indexed.
    /// </summary>
    public const bool DiscoveryPagesMayIndex = true;

    public const bool TransactionalPagesNoIndex = true;

    /// <summary>
    /// No distributed transactions.
    /// </summary>
    public const bool DistributedTransactionAllowed = false;
}

