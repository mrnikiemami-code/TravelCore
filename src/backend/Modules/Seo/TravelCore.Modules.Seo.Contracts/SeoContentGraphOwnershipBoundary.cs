namespace TravelCore.Modules.Seo.Contracts;

/// <summary>
/// P26-R1: SEO owns content graph mechanics in schema <c>seo</c>.
/// Content owns editorial bodies; Destination owns hierarchy; Search owns ranking/index posture.
/// </summary>
public static class SeoContentGraphOwnershipBoundary
{
    public const string OwnerModule = "Seo";
    public const string SchemaName = "seo";
    public const string IdentityConvention = "UUIDv7";
    public const string ReferenceSemantics = "SeoResourceType + ResourceId";

    public const string SeoIsNotContentEditorial = "SEO != Content editorial";
    public const string SeoIsNotDestinationHierarchy = "SEO != Destination hierarchy SoR";
    public const string SeoIsNotSearchRanking = "SEO != Search ranking SoR";
    public const string GraphExistenceIsNotIndexability = "Graph existence != indexability";
    public const string SearchUrlIsNotSeoLanding = "Search URL != SEO Landing";

    public const string ContentOwner = "Content";
    public const string DestinationOwner = "Destination";
    public const string SearchOwner = "Search";

    public const bool ContentGraphFoundationImplemented = true;
    public const bool HubClusterBoundaryImplemented = true;
    public const bool InternalLinkGraphBoundaryImplemented = true;
    public const bool ProgrammaticLandingBoundaryImplemented = true;
    public const bool RouteQualityBoundaryImplemented = true;
    public const bool SitemapGraphAwarenessImplemented = true;
    public const bool OperationalBoundaryImplemented = true;
    public const bool DeferredScopeBoundaryImplemented = true;
    public const bool HardeningGuardrailsImplemented = true;
    public const bool PublicGraphMutationApiImplemented = false;
    public const bool PeerSchemaForeignKeyImplemented = false;
    public const bool SharedDbContextImplemented = false;
    public const bool ContentEditorialSoRImplemented = false;
    public const bool DestinationHierarchySoRImplemented = false;
    public const bool SearchRankingSoRImplemented = false;
    public const bool ThinUrlFactoryImplemented = false;
}
