namespace TravelCore.Hardening;

/// <summary>
/// P29 ownership separation. Hardening abstractions must not become business-module SoR or replace domain authorization.
/// </summary>
public static class HardeningOwnershipBoundary
{
    public const string OwnerPlatform = "Platform";
    public const string HealthOwner = "Health";
    public const string ObservabilityOwner = "Observability";
    public const string AnalyticsOwner = "Analytics";
    public const string MediaOwner = "Media";
    public const string PerformanceOwner = "Performance";

    public const string HardeningIsNotObservability = "Hardening != Observability";
    public const string HardeningIsNotProductAnalytics = "Hardening != ProductAnalytics";
    public const string HardeningIsNotPerformanceOptimization = "Hardening != PerformanceOptimization";
    public const string HardeningIsNotMediaDelivery = "Hardening != MediaDelivery";
    public const string HardeningIsNotDomainAuthorization = "Hardening != DomainAuthorization";
    public const string HardeningIsNotBookingExecution = "Hardening != BookingExecution";
    public const string HardeningIsNotPaymentExecution = "Hardening != PaymentExecution";

    public const bool OwnsPlatformTelemetry = false;
    public const bool OwnsProductAnalytics = false;
    public const bool OwnsPerformanceOptimization = false;
    public const bool OwnsMediaBinaryStorage = false;
    public const bool OwnsDomainAuthorizationFacts = false;
    public const bool OwnsBookingExecution = false;
    public const bool OwnsPaymentExecution = false;
    public const bool FoundationBoundaryImplemented = true;
    public const bool HardeningGuardrailsImplemented = true;
}
