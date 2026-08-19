namespace TravelCore.Modules.DynamicPackage.Domain;

/// <summary>
/// Transient orchestration plan for a DynamicPackage composition.
/// Documents the coordination sequence without implementing it.
/// DynamicPackage coordinates FlightBooking + HotelBooking + Payment lifecycles
/// but does NOT own their execution.
///
/// Orchestration pattern: choreography via outbox/inbox (existing TravelCore pattern).
/// No distributed transactions. No centralized saga orchestrator.
///
/// Failure boundaries (documented, not implemented):
/// - Flight success / Hotel failure → compensate Flight
/// - Hotel success / Flight failure → compensate Hotel
/// - Payment success / component failure → refund via Payment compensation
/// - Timeouts → component-owned timeout handling
/// </summary>
public sealed class PackageOrchestrationPlan
{
    private PackageOrchestrationPlan(
        TransientPackageCandidate candidate,
        PackageMonetarySnapshot monetary)
    {
        Candidate = candidate;
        Monetary = monetary;
    }

    public TransientPackageCandidate Candidate { get; }

    public PackageMonetarySnapshot Monetary { get; }

    /// <summary>
    /// Coordination is choreography-based (outbox/inbox), not centralized orchestration.
    /// </summary>
    public const string CoordinationPattern = "Choreography (outbox/inbox)";

    /// <summary>
    /// No distributed transactions allowed.
    /// </summary>
    public const bool DistributedTransactionAllowed = false;

    /// <summary>
    /// Saga not implemented in this phase.
    /// </summary>
    public const bool SagaImplemented = false;

    /// <summary>
    /// Compensation logic not implemented in this phase.
    /// </summary>
    public const bool CompensationImplemented = false;

    public static PackageOrchestrationPlan Create(
        TransientPackageCandidate candidate,
        PackageMonetarySnapshot monetary)
    {
        ArgumentNullException.ThrowIfNull(candidate);
        ArgumentNullException.ThrowIfNull(monetary);
        return new PackageOrchestrationPlan(candidate, monetary);
    }
}
