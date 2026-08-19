namespace TravelCore.Modules.Notification.Domain;

/// <summary>
/// Logical template intent reference for Notification-owned orchestration. No persistence in T007 (P25-R4).
/// </summary>
public readonly record struct NotificationTemplateReference(string TemplateKey)
{
    public static NotificationTemplateReference FromKey(string templateKey) => new(templateKey);
}
