namespace TravelCore.Modules.PublicExperience.Contracts;

/// <summary>
/// Ownership boundary for public tour experience (TC-P14-T001 / P14-R1).
/// Public Experience Layer owns Detail/Listing/Landing presentation surfaces.
/// Tour remains catalog SoR. SEO remains IndexPolicy SoR. Search engine is P15.
/// </summary>
public static class PublicExperienceOwnershipBoundary
{
    public const string SurfaceOwnerModule = "PublicExperience";
    public const string CatalogOwnerModule = "Tour";
    public const string SeoOwnerModule = "Seo";
    public const string SearchOwnerModule = "Search";
    public const string CompositionPosture = "PresentationAndSeoComposition";
}
