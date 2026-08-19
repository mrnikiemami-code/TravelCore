namespace TravelCore.Performance;

/// <summary>
/// P28 ownership separation. Performance abstractions must not become business-module SoR or ranking engines.
/// </summary>
public static class PerformanceOwnershipBoundary
{
    public const string OwnerPlatform = "Platform";
    public const string ObservabilityOwner = "Observability";
    public const string AnalyticsOwner = "Analytics";
    public const string SearchOwner = "Search";
    public const string MediaOwner = "Media";

    public const string PerformanceIsNotObservability = "Performance != Observability";
    public const string PerformanceIsNotProductAnalytics = "Performance != ProductAnalytics";
    public const string PerformanceIsNotSearchRanking = "Performance != SearchRanking";
    public const string PerformanceIsNotBookingExecution = "Performance != BookingExecution";
    public const string PerformanceIsNotPaymentExecution = "Performance != PaymentExecution";
    public const string PerformanceIsNotSeoEditorial = "Performance != SEO";

    public const bool OwnsPlatformTelemetry = false;
    public const bool OwnsProductAnalytics = false;
    public const bool OwnsSearchRanking = false;
    public const bool OwnsBookingExecution = false;
    public const bool OwnsPaymentExecution = false;
    public const bool OwnsSeoEditorial = false;
    public const bool OwnsMediaBinaryStorage = false;
    public const bool FoundationBoundaryImplemented = true;
    public const bool HardeningGuardrailsImplemented = true;
}
