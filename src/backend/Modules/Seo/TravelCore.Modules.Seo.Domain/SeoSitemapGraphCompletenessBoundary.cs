namespace TravelCore.Modules.Seo.Domain;

/// <summary>
/// P26-R6 graph-aware sitemap/structured-data completeness posture building on P05 frameworks.
/// </summary>
public static class SeoSitemapGraphCompletenessBoundary
{
    public const string TruthfulStructuredDataOnly = "Structured data must remain truthful on real published surfaces";
    public const string GraphAwareSitemapPosture = "Sitemap scaling extends existing P05 frameworks for graph-aware surfaces";

    public const bool GraphAwareSitemapPostureImplemented = true;
    public const bool GraphAwareStructuredDataPostureImplemented = true;
    public const bool FakeStructuredDataImplemented = false;
}
