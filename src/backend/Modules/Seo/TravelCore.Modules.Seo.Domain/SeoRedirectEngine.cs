using NodaTime;

namespace TravelCore.Modules.Seo.Domain;

/// <summary>
/// Pure redirect/canonical resolution rules for TC-P05-T004.
/// Chain-free permanent targets, loop-bounded failure, locale-preserving lookup.
/// </summary>
public static class SeoRedirectEngine
{
    public static SeoPathResolution Resolve(
        string locale,
        string path,
        IEnumerable<SeoRoute> activeRoutes,
        IEnumerable<SeoRedirect> redirects,
        int maxHops = SeoRedirect.MaxResolutionHops)
    {
        ArgumentNullException.ThrowIfNull(activeRoutes);
        ArgumentNullException.ThrowIfNull(redirects);

        var normalizedLocale = SeoRoute.NormalizeLocale(locale);
        var normalizedPath = SeoRoute.NormalizePath(path);

        var routesByPath = activeRoutes
            .Where(r => string.Equals(r.Locale, normalizedLocale, StringComparison.Ordinal))
            .ToDictionary(r => r.Path, StringComparer.Ordinal);

        var redirectsByFrom = redirects
            .Where(r => string.Equals(r.Locale, normalizedLocale, StringComparison.Ordinal))
            .GroupBy(r => r.FromPath, StringComparer.Ordinal)
            .ToDictionary(g => g.Key, g => g.OrderByDescending(x => x.CreatedAt).First(), StringComparer.Ordinal);

        if (routesByPath.TryGetValue(normalizedPath, out var current))
        {
            // Active current route wins — never treat it as a redirect source.
            return SeoPathResolution.Current(
                normalizedLocale,
                normalizedPath,
                current.ResourceType,
                current.ResourceId,
                current.Id.Value);
        }

        if (!redirectsByFrom.TryGetValue(normalizedPath, out var first))
        {
            return SeoPathResolution.Missing(normalizedLocale, normalizedPath);
        }

        if (first.Status == SeoRedirectStatus.Gone)
        {
            return SeoPathResolution.GonePath(
                normalizedLocale,
                normalizedPath,
                first.ResourceType,
                first.ResourceId,
                first.SeoRouteId?.Value);
        }

        var finalTarget = ResolveFinalPermanentTarget(
            first,
            routesByPath,
            redirectsByFrom,
            maxHops);

        return SeoPathResolution.Permanent(
            normalizedLocale,
            normalizedPath,
            finalTarget,
            first.ResourceType,
            first.ResourceId,
            first.SeoRouteId?.Value);
    }

    public static SeoCanonicalSelection? SelectCanonical(
        string locale,
        string path,
        IEnumerable<SeoRoute> activeRoutes,
        IEnumerable<SeoRedirect> redirects)
    {
        var resolution = Resolve(locale, path, activeRoutes, redirects);
        return resolution.Kind switch
        {
            SeoPathResolutionKind.CurrentRoute => new SeoCanonicalSelection(
                resolution.Locale,
                resolution.RequestedPath,
                resolution.ResourceType!.Value,
                resolution.ResourceId!.Value,
                resolution.SeoRouteId!.Value,
                IsSelfCanonical: true),
            SeoPathResolutionKind.PermanentRedirect =>
                // Canonical for historical URLs is the final current path when it exists as a route.
                TryCanonicalForTarget(resolution.Locale, resolution.TargetPath!, activeRoutes),
            _ => null
        };
    }

    /// <summary>
    /// Computes the chain-free permanent target for a new A→B activation and validates loops/self.
    /// </summary>
    public static string ComputePermanentTarget(
        string locale,
        string fromPath,
        string proposedToPath,
        IEnumerable<SeoRoute> activeRoutes,
        IEnumerable<SeoRedirect> existingRedirects,
        SeoRouteId? movingRouteId = null,
        int maxHops = SeoRedirect.MaxResolutionHops)
    {
        ArgumentNullException.ThrowIfNull(activeRoutes);
        ArgumentNullException.ThrowIfNull(existingRedirects);

        var normalizedLocale = SeoRoute.NormalizeLocale(locale);
        var normalizedFrom = SeoRoute.NormalizePath(fromPath);
        var normalizedTo = SeoRoute.NormalizePath(proposedToPath);

        if (string.Equals(normalizedFrom, normalizedTo, StringComparison.Ordinal))
        {
            throw new SeoRedirectException("Permanent redirect cannot target its own path (self-redirect).");
        }

        var routesByPath = activeRoutes
            .Where(r => string.Equals(r.Locale, normalizedLocale, StringComparison.Ordinal))
            .ToDictionary(r => r.Path, StringComparer.Ordinal);

        if (routesByPath.TryGetValue(normalizedFrom, out var routeAtFrom)
            && (movingRouteId is null || routeAtFrom.Id != movingRouteId.Value))
        {
            throw new SeoRedirectException(
                "Active current SeoRoute path cannot simultaneously behave as a redirect source.");
        }

        var redirectsByFrom = existingRedirects
            .Where(r => string.Equals(r.Locale, normalizedLocale, StringComparison.Ordinal)
                        && !string.Equals(r.FromPath, normalizedFrom, StringComparison.Ordinal))
            .GroupBy(r => r.FromPath, StringComparer.Ordinal)
            .ToDictionary(g => g.Key, g => g.OrderByDescending(x => x.CreatedAt).First(), StringComparer.Ordinal);

        // Treat the moving route as already residing at the proposed target for chain resolution.
        if (movingRouteId is not null
            && routesByPath.TryGetValue(normalizedFrom, out var moving)
            && moving.Id == movingRouteId.Value)
        {
            routesByPath.Remove(normalizedFrom);
            routesByPath[normalizedTo] = moving;
        }

        if (routesByPath.ContainsKey(normalizedTo))
        {
            return normalizedTo;
        }

        if (!redirectsByFrom.TryGetValue(normalizedTo, out var next))
        {
            // Target may be the brand-new current path about to be persisted; allow as final.
            return normalizedTo;
        }

        if (next.Status == SeoRedirectStatus.Gone)
        {
            throw new SeoRedirectException(
                "Cannot activate a permanent redirect whose target path is intentionally gone.");
        }

        var synthetic = SeoRedirect.CreatePermanent(
            next.SeoRouteId,
            next.ResourceType,
            next.ResourceId,
            normalizedLocale,
            normalizedTo,
            next.ToPath!,
            next.CreatedAt);

        return ResolveFinalPermanentTarget(
            synthetic,
            routesByPath,
            redirectsByFrom,
            maxHops,
            seedVisited: new HashSet<string>(StringComparer.Ordinal) { normalizedFrom });
    }

    /// <summary>
    /// After A→C activation, any existing redirect that targeted A (or intermediate B) must retarget to C.
    /// </summary>
    public static void FlattenRedirectGraph(
        string locale,
        string activatedFromPath,
        string finalToPath,
        IList<SeoRedirect> mutableRedirects,
        Instant now)
    {
        ArgumentNullException.ThrowIfNull(mutableRedirects);

        var normalizedLocale = SeoRoute.NormalizeLocale(locale);
        var from = SeoRoute.NormalizePath(activatedFromPath);
        var final = SeoRoute.NormalizePath(finalToPath);

        foreach (var redirect in mutableRedirects
                     .Where(r => string.Equals(r.Locale, normalizedLocale, StringComparison.Ordinal)
                                 && r.Status == SeoRedirectStatus.PermanentMoved
                                 && r.ToPath is not null))
        {
            if (string.Equals(redirect.ToPath, from, StringComparison.Ordinal))
            {
                if (string.Equals(redirect.FromPath, final, StringComparison.Ordinal))
                {
                    throw new SeoRedirectException("Flattening would create a self-redirect loop.");
                }

                redirect.RetargetPermanent(final, now);
            }
        }
    }

    private static SeoCanonicalSelection? TryCanonicalForTarget(
        string locale,
        string targetPath,
        IEnumerable<SeoRoute> activeRoutes)
    {
        var route = activeRoutes.FirstOrDefault(r =>
            string.Equals(r.Locale, locale, StringComparison.Ordinal)
            && string.Equals(r.Path, targetPath, StringComparison.Ordinal));
        if (route is null)
        {
            return null;
        }

        return new SeoCanonicalSelection(
            locale,
            targetPath,
            route.ResourceType,
            route.ResourceId,
            route.Id.Value,
            IsSelfCanonical: true);
    }

    private static string ResolveFinalPermanentTarget(
        SeoRedirect start,
        IReadOnlyDictionary<string, SeoRoute> routesByPath,
        IReadOnlyDictionary<string, SeoRedirect> redirectsByFrom,
        int maxHops,
        HashSet<string>? seedVisited = null)
    {
        if (start.Status != SeoRedirectStatus.PermanentMoved || start.ToPath is null)
        {
            throw new SeoRedirectException("Expected a permanent redirect with a target path.");
        }

        var visited = seedVisited ?? new HashSet<string>(StringComparer.Ordinal);
        visited.Add(start.FromPath);

        var current = start.ToPath;
        var hops = 0;

        while (true)
        {
            if (hops++ >= maxHops)
            {
                throw new SeoRedirectException("Redirect resolution exceeded hop bound (possible corruption).");
            }

            if (!visited.Add(current))
            {
                throw new SeoRedirectException("Redirect loop detected.");
            }

            if (routesByPath.ContainsKey(current))
            {
                return current;
            }

            if (!redirectsByFrom.TryGetValue(current, out var next))
            {
                return current;
            }

            if (next.Status == SeoRedirectStatus.Gone)
            {
                throw new SeoRedirectException("Redirect chain ended at an intentionally gone path.");
            }

            if (next.ToPath is null)
            {
                throw new SeoRedirectException("Permanent redirect is missing a target path.");
            }

            current = next.ToPath;
        }
    }
}
