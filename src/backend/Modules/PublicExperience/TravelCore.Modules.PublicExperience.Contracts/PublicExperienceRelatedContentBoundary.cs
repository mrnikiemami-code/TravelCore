namespace TravelCore.Modules.PublicExperience.Contracts;

/// <summary>
/// P14-R6: Content enrichment is presentation/composition only.
/// Content remains editorial SoT. Tour remains tour-facts SoT.
/// Related ≠ copying CMS into TourProduct. Content publication ≠ SEO IndexPolicy.
/// </summary>
public static class PublicExperienceRelatedContentBoundary
{
    public const string PresentationOwner = "PublicExperience";
    public const string FactOwner = "Content";
    public const string CatalogFactOwner = "Tour";
    public const string IndexPolicyOwner = "Seo";
    public const string RelationKind = "SharedDestination";
    public const string RetrievalPosture = "DeterministicReplaceableQuery";
    public const bool CopyContentIntoTourAllowed = false;
    public const bool ContentPublicationOwnsIndexPolicy = false;
    public const int MaxItems = 6;
}
