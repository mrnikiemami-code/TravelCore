namespace TravelCore.Modules.DynamicPackage.Domain;

/// <summary>
/// Documents the DynamicPackage payment boundary without implementing payment flows.
///
/// Payment ownership decision: DynamicPackage does NOT introduce a new PaymentTargetKind
/// until the DynamicPackageBooking aggregate exists. Component payments (Flight, Hotel)
/// remain component-owned.
///
/// Payment authority: Package amount derived from PackageMonetarySnapshot
/// (FlightTotal + HotelTotal), same currency only, no FX.
///
/// Payment ordering (documented, not implemented):
/// - Before payment: Flight + Hotel obligations must be composed
/// - After payment: orchestration steps (reservation, ticketing) allowed
///
/// Failure boundaries (documented, not implemented):
/// - Payment success + Flight failure → Flight compensation via existing outbox
/// - Payment success + Hotel failure → Hotel compensation via existing outbox
/// - Payment timeout → component-owned timeout handling
/// - Duplicate payment event → idempotent via existing inbox pattern
/// </summary>
public static class PackagePaymentBoundary
{
    /// <summary>
    /// DynamicPackage does NOT own Payment execution.
    /// </summary>
    public const bool OwnsPaymentExecution = false;

    /// <summary>
    /// No new PaymentTargetKind introduced for DynamicPackage in this phase.
    /// Requires DynamicPackageBooking aggregate first.
    /// </summary>
    public const bool NewPaymentTargetIntroduced = false;

    /// <summary>
    /// Component payments remain component-owned (Flight, Hotel).
    /// </summary>
    public const string PaymentStrategy = "ComponentPayments";

    /// <summary>
    /// No distributed transaction allowed.
    /// </summary>
    public const bool DistributedTransactionAllowed = false;

    /// <summary>
    /// Compensation not implemented in this phase.
    /// </summary>
    public const bool CompensationImplemented = false;
}
