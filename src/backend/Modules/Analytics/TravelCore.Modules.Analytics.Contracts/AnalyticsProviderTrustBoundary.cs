namespace TravelCore.Modules.Analytics.Contracts;

/// <summary>
/// P27-R3 trust and provider-neutrality locks.
/// </summary>
public static class AnalyticsProviderTrustBoundary
{
    public const string PublisherCallIsNotDispatchSuccess = "PublisherCall != DispatchSuccess";
    public const string ProviderAckIsNotDownstreamCommit = "ProviderAck != DownstreamCommit";
    public const string ClientSuccessFlagIsNotDispatchSuccess = "ClientSuccessFlag != DispatchSuccess";
    public const string NamedProviderSelected = "NONE";
    public const bool ProviderPortImplemented = true;
    public const bool NamedProductionAdapterImplemented = false;
    public const bool ProductionProviderRegistered = false;
    public const bool ZeroProviderPostureValid = true;
    public const string ProviderInfrastructurePosture = "READY FOR ADAPTERS";
    public const string ProductionProviderPosture = "NOT CONFIGURED / NONE";
}
