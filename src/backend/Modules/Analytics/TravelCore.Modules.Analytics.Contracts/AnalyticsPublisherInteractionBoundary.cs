namespace TravelCore.Modules.Analytics.Contracts;

/// <summary>
/// P27-R4 publisher interaction and opaque reference semantics. Analytics must not become PII SoR.
/// </summary>
public static class AnalyticsPublisherInteractionBoundary
{
    public const string PublisherEmitsSemanticFactsOnly = "Publishers emit semantic product events only";
    public const string AnalyticsDoesNotOwnPii = "Analytics != PII Source of Record";
    public const string OpaqueResourceReferenceRequired = "Resource references must be opaque ids/contracts";
    public const string BookingPartyRemainIdentitySoR = "Booking/Party remain identity SoR";
    public const string SearchRemainRankingSoR = "Search remains ranking SoR";
    public const string DispatchOwner = "Analytics";

    public const bool PublisherInteractionBoundaryImplemented = true;
    public const bool OpaqueReferenceSemanticsImplemented = true;
    public const bool PiiPersistenceImplemented = false;
    public const bool IdentityGraphImplemented = false;
    public const bool PublicApiImplemented = false;
}
