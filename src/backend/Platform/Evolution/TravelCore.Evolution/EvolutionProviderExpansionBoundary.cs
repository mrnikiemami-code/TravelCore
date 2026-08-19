namespace TravelCore.Evolution;

/// <summary>
/// Post-P29-R3 provider expansion posture without provider registry product.
/// </summary>
public static class EvolutionProviderExpansionBoundary
{
    public const string ProviderExpansionIsModuleOwned = "Provider expansion remains module-owned";
    public const string NoGlobalProviderRegistryMegaTable = "No global provider registry mega-table without ADR";
    public const string ExternalIntegrationPerModule = "External integrations remain per owning module";
    public const string ProviderLockInForbiddenInEvolutionModule = "Provider lock-in forbidden in Evolution module";

    public const bool ProviderExpansionBoundaryImplemented = true;
    public const bool ProviderRegistryProductImplemented = false;
    public const bool ExternalIntegrationRewriteImplemented = false;
}
