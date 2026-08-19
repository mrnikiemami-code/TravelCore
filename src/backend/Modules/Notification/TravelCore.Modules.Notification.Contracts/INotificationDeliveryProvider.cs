namespace TravelCore.Modules.Notification.Contracts;

/// <summary>
/// Provider-neutral Notification delivery port. Vendor-specific types stay in adapters (P25-R3).
/// </summary>
public interface INotificationDeliveryProvider
{
    NotificationProviderKey Key { get; }

    NotificationProviderCapability Capabilities { get; }

    Task<NotificationDeliveryResult> SendAsync(
        NotificationDeliveryRequest request,
        CancellationToken cancellationToken = default);
}
