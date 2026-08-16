namespace TravelCore.Modules.Seo.Domain;

/// <summary>
/// Publishable resource kinds that may bind a public SeoRoute.
/// Business modules own content; SEO only references identity.
/// </summary>
public enum SeoResourceType : short
{
    Destination = 1,
    Place = 2,
    Article = 3,
    LandingPage = 4,
}
