namespace TravelCore.Modules.Notification.Contracts;

/// <summary>
/// P25-R3 trust and provider-neutrality locks.
/// </summary>
public static class NotificationProviderTrustBoundary
{
    public const string PublisherCallIsNotDeliverySuccess = "PublisherCall != DeliverySuccess";
    public const string ProviderAckIsNotDownstreamCommit = "ProviderAck != DownstreamCommit";
    public const string ClientSuccessFlagIsNotDeliverySuccess = "ClientSuccessFlag != DeliverySuccess";
    public const string NamedProviderSelected = "NONE";
    public const bool ProviderPortImplemented = true;
    public const bool NamedProductionAdapterImplemented = false;
    public const bool ProductionProviderRegistered = false;
    public const bool ZeroProviderPostureValid = true;
    public const string ProviderInfrastructurePosture = "READY FOR ADAPTERS";
    public const string ProductionProviderPosture = "NOT CONFIGURED / NONE";
}
