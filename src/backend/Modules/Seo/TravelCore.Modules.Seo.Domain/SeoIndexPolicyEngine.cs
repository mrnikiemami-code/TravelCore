namespace TravelCore.Modules.Seo.Domain;

/// <summary>
/// Effective robots/indexability after configured policy + eligibility (TC-P05-T005 / R2).
/// </summary>
public sealed record SeoIndexabilityEvaluation(
    string Locale,
    string Path,
    SeoIndexDirective EffectiveIndex,
    SeoFollowDirective EffectiveFollow,
    string RobotsDirective,
    SeoIndexDirective? ConfiguredIndex,
    SeoFollowDirective? ConfiguredFollow,
    bool IsIndexable,
    IReadOnlyList<string> Reasons)
{
    public static SeoIndexabilityEvaluation ConservativeNoIndex(
        string locale,
        string path,
        SeoIndexDirective? configuredIndex,
        SeoFollowDirective? configuredFollow,
        params string[] reasons) =>
        new(
            locale,
            path,
            SeoIndexDirective.NoIndex,
            configuredFollow ?? SeoFollowDirective.Follow,
            BuildRobots(SeoIndexDirective.NoIndex, configuredFollow ?? SeoFollowDirective.Follow),
            configuredIndex,
            configuredFollow,
            IsIndexable: false,
            reasons);

    public static SeoIndexabilityEvaluation Indexable(
        string locale,
        string path,
        SeoFollowDirective follow,
        SeoIndexDirective configuredIndex,
        SeoFollowDirective configuredFollow,
        params string[] reasons) =>
        new(
            locale,
            path,
            SeoIndexDirective.Index,
            follow,
            BuildRobots(SeoIndexDirective.Index, follow),
            configuredIndex,
            configuredFollow,
            IsIndexable: true,
            reasons);

    public static string BuildRobots(SeoIndexDirective index, SeoFollowDirective follow)
    {
        var indexPart = index == SeoIndexDirective.Index ? "index" : "noindex";
        var followPart = follow == SeoFollowDirective.Follow ? "follow" : "nofollow";
        return $"{indexPart}, {followPart}";
    }
}

/// <summary>
/// Pure IndexPolicy evaluation: missing/deny => noindex; allow requires eligibility.
/// </summary>
public static class SeoIndexPolicyEngine
{
    public static SeoIndexabilityEvaluation Evaluate(
        string locale,
        string path,
        SeoIndexPolicy? configuredPolicy,
        SeoPathResolution pathResolution,
        SeoCanonicalSelection? canonical)
    {
        var normalizedLocale = SeoRoute.NormalizeLocale(locale);
        var normalizedPath = SeoRoute.NormalizePath(path);

        var configuredIndex = configuredPolicy?.IndexDirective;
        var configuredFollow = configuredPolicy?.FollowDirective;

        if (configuredPolicy is null)
        {
            return SeoIndexabilityEvaluation.ConservativeNoIndex(
                normalizedLocale,
                normalizedPath,
                configuredIndex,
                configuredFollow,
                "missing-policy-default-noindex");
        }

        if (configuredPolicy.IndexDirective == SeoIndexDirective.NoIndex)
        {
            return SeoIndexabilityEvaluation.ConservativeNoIndex(
                normalizedLocale,
                normalizedPath,
                configuredIndex,
                configuredFollow,
                "explicit-noindex");
        }

        // Explicit Index — eligibility required.
        if (pathResolution.Kind == SeoPathResolutionKind.NotFound)
        {
            return SeoIndexabilityEvaluation.ConservativeNoIndex(
                normalizedLocale,
                normalizedPath,
                configuredIndex,
                configuredFollow,
                "explicit-index-but-path-not-found");
        }

        if (pathResolution.Kind == SeoPathResolutionKind.PermanentRedirect)
        {
            return SeoIndexabilityEvaluation.ConservativeNoIndex(
                normalizedLocale,
                normalizedPath,
                configuredIndex,
                configuredFollow,
                "explicit-index-but-historical-redirect-source");
        }

        if (pathResolution.Kind == SeoPathResolutionKind.Gone)
        {
            return SeoIndexabilityEvaluation.ConservativeNoIndex(
                normalizedLocale,
                normalizedPath,
                configuredIndex,
                configuredFollow,
                "explicit-index-but-gone");
        }

        if (pathResolution.Kind != SeoPathResolutionKind.CurrentRoute)
        {
            return SeoIndexabilityEvaluation.ConservativeNoIndex(
                normalizedLocale,
                normalizedPath,
                configuredIndex,
                configuredFollow,
                "explicit-index-but-not-current-route");
        }

        if (canonical is null || !canonical.IsSelfCanonical)
        {
            return SeoIndexabilityEvaluation.ConservativeNoIndex(
                normalizedLocale,
                normalizedPath,
                configuredIndex,
                configuredFollow,
                "explicit-index-but-canonical-ineligible");
        }

        if (!string.Equals(canonical.Path, normalizedPath, StringComparison.Ordinal)
            || !string.Equals(canonical.Locale, normalizedLocale, StringComparison.Ordinal))
        {
            return SeoIndexabilityEvaluation.ConservativeNoIndex(
                normalizedLocale,
                normalizedPath,
                configuredIndex,
                configuredFollow,
                "explicit-index-but-canonical-mismatch");
        }

        if (configuredPolicy.ResourceType != pathResolution.ResourceType
            || configuredPolicy.ResourceId != pathResolution.ResourceId
            || !string.Equals(configuredPolicy.Locale, normalizedLocale, StringComparison.Ordinal))
        {
            return SeoIndexabilityEvaluation.ConservativeNoIndex(
                normalizedLocale,
                normalizedPath,
                configuredIndex,
                configuredFollow,
                "explicit-index-but-policy-resource-mismatch");
        }

        return SeoIndexabilityEvaluation.Indexable(
            normalizedLocale,
            normalizedPath,
            configuredPolicy.FollowDirective,
            configuredPolicy.IndexDirective,
            configuredPolicy.FollowDirective,
            "explicit-index-and-eligible-current-route");
    }
}
