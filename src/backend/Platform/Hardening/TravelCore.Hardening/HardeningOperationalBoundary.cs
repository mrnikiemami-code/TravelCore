namespace TravelCore.Hardening;

/// <summary>
/// P29 operational hardening posture. Production readiness boundaries without APM/secret-manager/CI product.
/// </summary>
public static class HardeningOperationalBoundary
{
    public const string NoFakeSecurityClaims = "Fake security/compliance claims are NOT ALLOWED";
    public const string NoProductionHardeningProductInT008 = "No production hardening product in T008";
    public const string OperationalReadinessBoundaryOnly = "Operational readiness is boundary-only";
    public const string InternalOpsPosture = "BOUNDARY ONLY";
    public const string RunbooksAreDocumentationPosture = "Operational runbooks are documentation posture";

    public const bool OperationalBoundaryImplemented = true;
    public const bool ApmVendorProductImplemented = false;
    public const bool SecretManagerIntegrationImplemented = false;
    public const bool CiPipelineYamlImplemented = false;
    public const bool PublicHardeningApiImplemented = false;
}
