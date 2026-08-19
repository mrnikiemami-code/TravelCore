namespace TravelCore.Performance;

/// <summary>
/// P28 runtime interaction model. Domain modules retain execution ownership; Performance declares platform interaction contracts only.
/// </summary>
public static class PerformanceModuleInteractionBoundary
{
    public const string DomainModulesRetainExecutionOwnership = "Domain modules retain business execution ownership";
    public const string PerformanceDoesNotInterceptBookingExecution = "Performance does not intercept Booking execution";
    public const string PerformanceDoesNotInterceptPaymentExecution = "Performance does not intercept Payment execution";
    public const string PerformanceDoesNotInterceptSearchRanking = "Performance does not intercept Search ranking";
    public const string PlatformOwnsPerformanceInteractionContracts =
        "Platform owns performance interaction contracts";

    public const bool ModuleInteractionBoundaryImplemented = true;
    public const bool CrossModulePerformanceHookImplemented = false;
    public const bool DomainExecutionOwnershipTransferred = false;
    public const bool PublicApiImplemented = false;
    public const bool AdminApiImplemented = false;
}
