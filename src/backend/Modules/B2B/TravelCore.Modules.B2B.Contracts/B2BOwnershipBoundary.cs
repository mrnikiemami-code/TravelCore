namespace TravelCore.Modules.B2B.Contracts;

/// <summary>
/// P24-R1: B2B is the independent agency-commerce orchestration owner (schema <c>b2b</c>).
/// Agency business posture lives here; Identity credentials, Access authorization, and Party identity remain unchanged.
/// </summary>
public static class B2BOwnershipBoundary
{
    public const string OwnerModule = "B2B";
    public const string SchemaName = "b2b";
    public const string IdentityConvention = "UUIDv7";
    public const string MoneyModel = "TravelCore.Money";
    public const string TemporalModel = "NodaTime";

    public const string B2BIsNotIdentity = "B2B != Identity";
    public const string B2BIsNotAccess = "B2B != Access";
    public const string B2BIsNotParty = "B2B != Party";
    public const string B2BIsNotBooking = "B2B != Booking";
    public const string B2BIsNotPayment = "B2B != Payment";
    public const string B2BIsNotAgencyMarketplace = "B2B != AgencyMarketplace";
    public const string AgencyIsBusinessConceptNotIdentity = "Agency is a business concept, not Identity";
    public const string AgencyUsersAreAccessSubjects = "Agency users are Access subjects";
    public const string AgencyOrganizationBelongsToParty = "Agency organization relationship belongs to Party";

    public const string IdentityOwner = "Identity";
    public const string AccessOwner = "Access";
    public const string PartyOwner = "Party";
    public const string BookingOwner = "Booking";
    public const string PaymentOwner = "Payment";
    public const string AgencyMarketplaceOwner = "AgencyMarketplace";

    public const bool OwnsIdentityCredentials = false;
    public const bool OwnsAccessAuthorization = false;
    public const bool OwnsPartyIdentity = false;
    public const bool OwnsBookingExecution = false;
    public const bool OwnsPaymentExecution = false;
    public const bool OwnsAgencyMarketplaceCommercialLayer = false;
    public const bool GenericBookingAbstractionImplemented = false;
    public const bool SeparateB2BModuleImplemented = true;
    public const bool SeparateB2BSchemaImplemented = true;
    public const bool AgencyEntityImplemented = false;
    public const bool AgencyReferenceBoundaryImplemented = true;
    public const bool AgencyMembershipBoundaryImplemented = true;
    public const bool AgencyRelationshipBoundaryImplemented = true;
    public const bool AgencyMemberReferenceImplemented = true;
    public const bool AgencyAccessRelationshipBoundaryImplemented = true;
    public const bool AgencyUserEntityImplemented = false;
    public const bool OwnsUsers = false;
    public const bool OwnsAuthentication = false;
    public const bool OwnsAuthorizationPolicies = false;
    public const bool OwnsInvitationFlow = false;
    public const bool ContractEntityImplemented = false;
    public const bool CommissionEntityImplemented = false;
    public const bool CreditLimitEntityImplemented = false;
    public const bool WalletImplemented = false;
    public const bool SettlementImplemented = false;
    public const bool PaymentTargetAdded = false;
    public const bool PublicApiImplemented = false;
    public const bool FrontendImplemented = false;
    public const bool SharedDbContextImplemented = false;
    public const bool PeerSchemaForeignKeyImplemented = false;
    public const bool BookingPersistenceDependencyImplemented = false;
    public const bool PaymentPersistenceDependencyImplemented = false;
    public const bool IdentityPersistenceDependencyImplemented = false;
    public const bool ProductTablesImplemented = false;
}
