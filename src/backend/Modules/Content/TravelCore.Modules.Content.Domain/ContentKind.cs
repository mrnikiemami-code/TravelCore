namespace TravelCore.Modules.Content.Domain;

/// <summary>
/// Closed ContentKind classification (P08-R1).
/// One ContentItem has exactly one primary kind — no multi-kind items.
/// </summary>
public enum ContentKind : short
{
    Article = 1,
    LandingPage = 2,
    Guide = 3
}
