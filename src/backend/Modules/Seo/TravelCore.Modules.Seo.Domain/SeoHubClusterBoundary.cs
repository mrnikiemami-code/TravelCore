namespace TravelCore.Modules.Seo.Domain;

/// <summary>
/// P26-R2 hub/cluster boundary marker. Taxonomy and ownership only — no hub editorial duplication in T005.
/// </summary>
public static class SeoHubClusterBoundary
{
    public const string HubClusterTaxonomy = "DestinationHub · ContentCluster";
    public const string GraphMechanicsOwner = "Seo";
    public const string ContentPublisherOwner = "Content";
    public const string DestinationPublisherOwner = "Destination";
    public const string NoHubContentDuplication = "Hub/cluster taxonomy must not duplicate editorial or hierarchy SoR";

    public const bool SeoOwnsHubClusterTaxonomy = true;
    public const bool HubClusterPersistenceImplemented = false;
    public const bool HubEditorialDuplicationImplemented = false;
    public const bool DestinationHierarchySoRImplemented = false;
    public const bool PublicApiImplemented = false;
}
