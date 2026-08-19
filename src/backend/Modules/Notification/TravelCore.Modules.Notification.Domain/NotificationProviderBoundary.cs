namespace TravelCore.Modules.Notification.Domain;

/// <summary>
/// P25-R3 provider abstraction boundary marker. Port/contracts only — no named production adapter or delivery persistence in T006.
/// </summary>
public static class NotificationProviderBoundary
{
    public const string DeliveryOwner = "Notification";
    public const string PublisherDoesNotCallProviderDirectly = "Publishers do not call providers directly";
    public const string ZeroProviderPostureValid = "Zero registered providers is valid until explicit lock";
    public const string ProviderNeutralContractsOnly = "Provider-neutral delivery contracts only";

    public const bool NotificationOwnsProviderAbstraction = true;
    public const bool ProviderPortImplemented = true;
    public const bool NamedProductionAdapterImplemented = false;
    public const bool ProviderExecutionPersistenceImplemented = false;
    public const bool DeliveryStatePersistenceImplemented = false;
    public const bool SmtpClientImplemented = false;
    public const bool TwilioImplemented = false;
    public const bool PushChannelImplemented = false;
    public const bool PublicApiImplemented = false;
}
