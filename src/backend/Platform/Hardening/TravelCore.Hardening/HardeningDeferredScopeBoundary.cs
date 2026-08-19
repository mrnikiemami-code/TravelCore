namespace TravelCore.Hardening;

/// <summary>
/// P29 deferred hardening scope catalog. Product implementations remain deferred unless explicitly locked by later tasks/GATE.
/// </summary>
public static class HardeningDeferredScopeBoundary
{
    public const string PenetrationTestingVendorEngagement = "DEFERRED";
    public const string SastDastVendorProducts = "DEFERRED";
    public const string SiemCentralizedLogAggregation = "DEFERRED";
    public const string HardwareSecurityModuleProduct = "DEFERRED";
    public const string ZeroTrustMeshProduct = "DEFERRED";
    public const string AutomatedChaosEngineering = "DEFERRED";
    public const string MalwareAvScannerProduct = "DEFERRED";
    public const string SecretManagerVendorIntegration = "DEFERRED";
    public const string CiCdYamlProduct = "DEFERRED";

    public const string HealthRichDiagnosticsPosture = "Health != rich diagnostics (minimal response only)";
    public const string ProductionSeoMobileA11yVerification = "Production SEO/mobile/a11y verification posture only";
    public const string OperationalRunbooksPosture = "Operational runbooks documentation posture";

    public const bool DeferredScopeBoundaryImplemented = true;
    public const bool PenetrationTestVendorImplemented = false;
    public const bool SiemProductImplemented = false;
    public const bool SecretManagerVendorImplemented = false;
    public const bool CiCdProductImplemented = false;
}
