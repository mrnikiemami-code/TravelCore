namespace TravelCore.Modules.Seo.Domain;

/// <summary>
/// P26-R3 internal link graph boundary marker. Graph orchestration only — no Content editorial SoR or external crawl.
/// </summary>
public static class SeoInternalLinkGraphBoundary
{
    public const string GraphMechanicsOwner = "Seo";
    public const string EditorialLinkOwner = "Content";
    public const string NoExternalCrawl = "External link crawling remains DEFERRED";
    public const string NoPublicGraphMutationApi = "No public graph mutation API by default";

    public const bool SeoOwnsGraphOrchestration = true;
    public const bool EditorialLinkSoRImplemented = false;
    public const bool ExternalCrawlImplemented = false;
    public const bool LinkEdgePersistenceImplemented = false;
    public const bool PublicGraphMutationApiImplemented = false;
}
