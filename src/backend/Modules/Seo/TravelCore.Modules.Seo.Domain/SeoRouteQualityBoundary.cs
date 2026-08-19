namespace TravelCore.Modules.Seo.Domain;

/// <summary>
/// P26-R5 route quality / orphan / indexation quality observability posture.
/// </summary>
public static class SeoRouteQualityBoundary
{
    public const string OrphanDetectionPosture = "Orphan/unpublished route detection is observability-only";
    public const string NoFakeIndexSuccess = "Graph/route existence must not imply index success";

    public const bool RouteQualityMarkersImplemented = true;
    public const bool OrphanDetectionImplemented = true;
    public const bool FakeIndexSuccessImplemented = false;
    public const bool RouteQualityPersistenceImplemented = false;
}
