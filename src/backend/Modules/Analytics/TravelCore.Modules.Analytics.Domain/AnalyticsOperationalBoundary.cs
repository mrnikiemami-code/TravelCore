namespace TravelCore.Modules.Analytics.Domain;

/// <summary>
/// P27-R7 operational boundary marker. Internal read/ops posture only; no public analytics query/mutation API by default.
/// </summary>
public static class AnalyticsOperationalBoundary
{
    public const string FakeProductionDispatchSuccess = "NOT ALLOWED";
    public const string PublicOperationalApiPosture = "NOT IMPLEMENTED";
    public const string AdminOperationalApiPosture = "NOT IMPLEMENTED";
    public const string InternalReadOpsPosture = "BOUNDARY ONLY";

    public const bool OperationalBoundaryImplemented = true;
    public const bool PublicApiImplemented = false;
    public const bool AdminApiImplemented = false;
    public const bool FakeDispatchSuccessImplemented = false;
}
