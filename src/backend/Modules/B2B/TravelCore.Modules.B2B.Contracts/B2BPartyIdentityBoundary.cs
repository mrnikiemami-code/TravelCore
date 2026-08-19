namespace TravelCore.Modules.B2B.Contracts;

/// <summary>
/// P24-R1 logical-reference readiness. Party remains identity SoR; Access remains authorization SoR; Identity remains credential SoR.
/// B2B may later reference logical Party/Access identifiers only — no Party/Access aggregate ownership in T001.
/// </summary>
public static class B2BPartyIdentityBoundary
{
    public const string IdentitySourceModule = "Party";
    public const string AccessSubjectModule = "Access";
    public const string CredentialModule = "Identity";
    public const string CommercialLayerModule = "B2B";
}
