namespace TravelCore.Modules.Seo.Contracts;

/// <summary>
/// P26-R1 publisher vs graph-owner posture. Business modules publish facts; SEO owns graph/route/indexation mechanics.
/// </summary>
public static class SeoResourcePublisherBoundary
{
    public const string ContentPublisherOwner = "Content";
    public const string DestinationPublisherOwner = "Destination";
    public const string SearchIndexOwner = "Search";
    public const string GraphMechanicsOwner = "Seo";

    public const string PublisherPosture =
        "Business modules publish publishable facts; SEO owns graph mechanics only";
}
