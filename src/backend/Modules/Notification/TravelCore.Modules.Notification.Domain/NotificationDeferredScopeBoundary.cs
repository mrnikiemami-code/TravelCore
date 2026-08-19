namespace TravelCore.Modules.Notification.Domain;

/// <summary>
/// P25-R8 deferred/out-of-scope posture. Push/webhook/campaign tooling remain deferred unless explicitly locked.
/// </summary>
public static class NotificationDeferredScopeBoundary
{
    public const string PushNotifications = "DEFERRED";
    public const string WebhookDelivery = "DEFERRED";
    public const string MarketingCampaignPlatform = "DEFERRED";
    public const string AdvancedRoutingEngine = "DEFERRED";

    public const bool DeferredScopeBoundaryImplemented = true;
    public const bool PushChannelImplemented = false;
    public const bool WebhookEndpointImplemented = false;
    public const bool CampaignPlatformImplemented = false;
    public const bool AdvancedRoutingImplemented = false;
}
