namespace TravelCore.Modules.TripPlanner.Contracts;

/// <summary>
/// P18-R3: TripPlanner identity/contact boundaries vs Identity, Party, and CRM.
/// </summary>
public static class TripPlannerIdentityBoundary
{
    public const string AnonymousFirstPosture = "Anonymous-first TripIntent";
    public const string TripIntentCreationRequiresAccount = "TripIntent creation must not require Identity Account creation";
    public const string PlannerActorReferenceNotEqualIdentityAccount = "PlannerActorReference != Identity Account entity";
    public const string LeadContactSnapshotNotEqualParty = "LeadContactSnapshot != Party";
    public const string LeadContactSnapshotNotEqualIdentityAccount = "LeadContactSnapshot != Identity Account";
    public const string LeadContactSnapshotNotEqualCustomerMaster = "LeadContactSnapshot != Customer Master";
    public const string ContactEmailNotDomainIdentity = "Contact Email != Domain Identity";
    public const string DraftAccessTokenNotGlobalIdentity = "Draft access token != global identity";
    public const bool AnonymousTripIntentSupported = true;
    public const bool AuthenticatedAssociationOptional = true;
    public const bool IdentityAuthorityDuplicated = false;
    public const bool PartyMasterDuplicated = false;
    public const bool PersistentAnonymousUserPlatform = false;
    public const bool LeadContactSnapshotImplemented = true;
    public const bool ConsentModelImplemented = false;
}
