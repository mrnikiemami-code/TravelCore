namespace TravelCore.Modules.Notification.Contracts;

/// <summary>
/// Resolves a trusted configured NotificationProviderKey to a delivery adapter. No ranking or failover (P25-R3).
/// Zero registered providers is valid until explicit lock.
/// </summary>
public interface INotificationProviderResolver
{
    INotificationDeliveryProvider? Resolve(NotificationProviderKey providerKey);

    NotificationProviderDescriptor? Describe(NotificationProviderKey providerKey);

    IReadOnlyList<NotificationProviderDescriptor> ListDescriptors();

    NotificationProviderCapabilityStatus Check(
        NotificationProviderKey providerKey,
        NotificationProviderCapability required);
}
