namespace TravelCore.Modules.Notification.Domain;

/// <summary>
/// P25-R4 template orchestration boundary marker. Notification owns orchestration semantics only.
/// </summary>
public static class NotificationTemplateBoundary
{
    public const string TemplateOrchestrationOwner = "Notification";
    public const string BusinessModulePublishesIntentOnly = "Business modules publish semantic intent/facts only";
    public const string TemplateRenderingEngineImplemented = "NOT IMPLEMENTED";
    public const string TemplatePersistenceImplemented = "NOT IMPLEMENTED";

    public const bool NotificationOwnsTemplateOrchestration = true;
    public const bool TemplatePersistenceImplementedFlag = false;
    public const bool TemplateRenderingImplemented = false;
    public const bool PublicTemplateAdminImplemented = false;
}
