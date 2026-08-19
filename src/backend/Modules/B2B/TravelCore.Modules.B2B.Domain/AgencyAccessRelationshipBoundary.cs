namespace TravelCore.Modules.B2B.Domain;

/// <summary>
/// P24-R3: Agency membership connects to Access subjects only — B2B does not own users, authentication, or authorization.
/// </summary>
public static class AgencyAccessRelationshipBoundary
{
    public const string MembershipIntentOwner = "B2B";
    public const string UserIdentityOwner = "Identity";
    public const string AuthorizationOwner = "Access";
    public const string OrganizationRelationshipOwner = "Party";

    public const string AgencyUsersAreAccessSubjects = "Agency users are Access subjects";
    public const string AgencyMembersAreNotIdentityCredentials = "Agency members are not Identity credentials";
    public const string MembershipIsNotAuthorization = "Membership intent is not authorization";

    public const bool B2BOwnsUsers = false;
    public const bool B2BOwnsAuthentication = false;
    public const bool B2BOwnsAuthorization = false;
    public const bool B2BOwnsAccessPolicies = false;
    public const bool B2BOwnsRoles = false;
    public const bool B2BOwnsPermissions = false;
    public const bool B2BOwnsInvitationFlow = false;
    public const bool AgencyMemberTableImplemented = false;
    public const bool UserTableImplemented = false;
    public const bool RoleTableImplemented = false;
    public const bool PermissionTableImplemented = false;
    public const bool MembershipPersistenceImplemented = false;
    public const bool AuthenticationChangesImplemented = false;
    public const bool AuthorizationChangesImplemented = false;
    public const bool PublicApiImplemented = false;
    public const bool FrontendImplemented = false;
}
