namespace TravelCore.Modules.Notification.Contracts;

/// <summary>
/// Template orchestration port owned by Notification. Rendering engines and persistence remain deferred (P25-R4).
/// </summary>
public interface INotificationTemplateOrchestrator
{
    NotificationTemplateResolution Resolve(string templateKey, string localeCode);
}

/// <summary>
/// Neutral template resolution result. Content storage/rendering deferred beyond T007.
/// </summary>
public sealed record NotificationTemplateResolution(
    string TemplateKey,
    string LocaleCode,
    string ContentReference,
    bool Found);
