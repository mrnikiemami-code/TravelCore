namespace TravelCore.Hardening;

/// <summary>
/// P29 security vs domain authorization separation. Domain modules retain authorization facts; Hardening declares review posture only.
/// </summary>
public static class HardeningDomainAuthorizationInteractionBoundary
{
    public const string DomainAuthorizationFactsRemainInDomain =
        "Domain authorization facts remain in domain modules";
    public const string AccessModuleOwnsPermissionModel = "Access module owns permission model facts";
    public const string IdentityModuleOwnsIdentityFacts = "Identity module owns identity facts";
    public const string HardeningDoesNotReplaceDomainAuthorization =
        "Hardening != Domain authorization replacement";
    public const string HardeningDoesNotOwnBusinessRules = "Hardening != business rule owner";

    public const bool DomainAuthorizationInteractionBoundaryImplemented = true;
    public const bool IdentityModuleReferenceRequired = false;
    public const bool AccessModuleReferenceRequired = false;
    public const bool CrossModuleAuthorizationMegaTableImplemented = false;
    public const bool CentralizedPermissionEngineImplemented = false;
}
