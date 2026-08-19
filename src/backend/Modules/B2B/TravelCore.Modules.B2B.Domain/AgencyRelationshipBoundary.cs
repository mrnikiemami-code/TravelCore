namespace TravelCore.Modules.B2B.Domain;

/// <summary>
/// P24-R2: Agency is a B2B business concept linked to Party-owned organization/agency identity — not an Identity silo.
/// </summary>
public static class AgencyRelationshipBoundary
{
    public const string AgencyIsBusinessConcept = "Agency is a business concept, not Identity";
    public const string AgencyUsersAreAccessSubjects = "Agency users are Access subjects";
    public const string AgencyOrganizationOwnedByParty = "Agency organization relationship belongs to Party";
    public const string B2BCommercialLayer = "B2B";
    public const string PartyIdentityOwner = "Party";
    public const string AccessAuthorizationOwner = "Access";
    public const string IdentityCredentialOwner = "Identity";
    public const string BookingExecutionOwner = "Booking";
    public const string PaymentExecutionOwner = "Payment";

    public const bool B2BOwnsPartyOrganizationData = false;
    public const bool B2BOwnsIdentityCredentials = false;
    public const bool B2BOwnsAccessAuthorization = false;
    public const bool B2BOwnsAgencyAggregate = false;
    public const bool AgencyPersistenceImplemented = false;
    public const bool AgencyCrudImplemented = false;
    public const bool AgencyRegistrationFlowImplemented = false;
    public const bool BookingRelationImplemented = false;
    public const bool PaymentRelationImplemented = false;
    public const bool PublicApiImplemented = false;
    public const bool FrontendImplemented = false;
}
