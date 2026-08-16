namespace TravelCore.Modules.Seo.Domain;

/// <summary>
/// Side-effects of an SEO-bound path change: history row + redirect-candidate hook.
/// Not a Destination translation mutation and not a published Redirect (T004).
/// </summary>
public sealed record SeoRoutePathChange(
    SeoPathHistoryEntry History,
    SeoRedirectCandidate RedirectCandidate);
