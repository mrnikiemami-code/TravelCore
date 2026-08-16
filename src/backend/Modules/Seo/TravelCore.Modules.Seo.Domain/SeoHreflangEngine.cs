namespace TravelCore.Modules.Seo.Domain;

/// <summary>
/// One genuine alternate-locale public path for hreflang (TC-P05-T006 / ADR 0008).
/// Never fabricates missing locale routes.
/// </summary>
public sealed record SeoHreflangAlternate(
    string Locale,
    string Path,
    string Href);

/// <summary>
/// Pure hreflang binding rules over active SeoRoute rows for one resource.
/// </summary>
public static class SeoHreflangEngine
{
    public static IReadOnlyList<SeoHreflangAlternate> BuildAlternates(
        SeoResourceType resourceType,
        Guid resourceId,
        IEnumerable<SeoRoute> activeRoutesForResource)
    {
        ArgumentNullException.ThrowIfNull(activeRoutesForResource);
        if (resourceId == Guid.Empty)
        {
            throw new ArgumentException("ResourceId cannot be empty.", nameof(resourceId));
        }

        return activeRoutesForResource
            .Where(r => r.ResourceType == resourceType && r.ResourceId == resourceId)
            .GroupBy(r => r.Locale, StringComparer.Ordinal)
            .Select(g => g.OrderBy(r => r.Path, StringComparer.Ordinal).First())
            .OrderBy(r => r.Locale, StringComparer.Ordinal)
            .Select(r => new SeoHreflangAlternate(
                r.Locale,
                r.Path,
                BuildHref(r.Locale, r.Path)))
            .ToList();
    }

    /// <summary>
    /// Omits locales that are not present as active SeoRoutes — never invents them.
    /// </summary>
    public static IReadOnlyList<SeoHreflangAlternate> BuildAlternatesOmittingMissing(
        SeoResourceType resourceType,
        Guid resourceId,
        IEnumerable<SeoRoute> activeRoutesForResource,
        IEnumerable<string> requestedLocales)
    {
        ArgumentNullException.ThrowIfNull(requestedLocales);
        var available = BuildAlternates(resourceType, resourceId, activeRoutesForResource)
            .ToDictionary(a => a.Locale, StringComparer.Ordinal);

        var result = new List<SeoHreflangAlternate>();
        foreach (var locale in requestedLocales)
        {
            var normalized = SeoRoute.NormalizeLocale(locale);
            if (available.TryGetValue(normalized, out var alt))
            {
                result.Add(alt);
            }
        }

        return result;
    }

    public static string BuildHref(string locale, string path) =>
        $"/{SeoRoute.NormalizeLocale(locale)}/{SeoRoute.NormalizePath(path)}";
}
