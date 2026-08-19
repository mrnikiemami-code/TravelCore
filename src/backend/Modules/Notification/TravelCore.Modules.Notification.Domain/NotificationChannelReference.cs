namespace TravelCore.Modules.Notification.Domain;

/// <summary>
/// Logical channel intent reference for Notification-owned taxonomy. No persistence in T005.
/// </summary>
public readonly record struct NotificationChannelReference(NotificationChannelKind Kind)
{
    public static NotificationChannelReference Email => new(NotificationChannelKind.Email);

    public static NotificationChannelReference Sms => new(NotificationChannelKind.Sms);

    public static NotificationChannelReference InApp => new(NotificationChannelKind.InApp);
}
