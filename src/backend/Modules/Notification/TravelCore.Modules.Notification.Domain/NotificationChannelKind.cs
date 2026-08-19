namespace TravelCore.Modules.Notification.Domain;

/// <summary>
/// P25-R2 planned channel taxonomy. Notification owns channel semantics; publishers do not call providers directly.
/// </summary>
public enum NotificationChannelKind
{
    Email = 1,
    Sms = 2,
    InApp = 3,
}
