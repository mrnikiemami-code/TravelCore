namespace TravelCore.Modules.Seo.Domain;

/// <summary>Candidate public route considered for sitemap inclusion (TC-P05-T009).</summary>
public sealed record SeoSitemapCandidate(
    string Locale,
    string Path,
    SeoResourceType ResourceType,
    Guid ResourceId);

/// <summary>One sitemap URL entry after IndexPolicy gate.</summary>
public sealed record SeoSitemapUrl(
    string Locale,
    string Path,
    string Loc);

/// <summary>
/// Sitemap inclusion rules: only effectively indexable routes (R2).
/// Never dumps all DB rows; missing policy is excluded.
/// </summary>
public static class SeoSitemapEngine
{
    public static IReadOnlyList<SeoSitemapUrl> SelectIndexableUrls(
        IEnumerable<SeoSitemapCandidate> candidates,
        Func<SeoSitemapCandidate, SeoIndexabilityEvaluation> evaluate)
    {
        ArgumentNullException.ThrowIfNull(candidates);
        ArgumentNullException.ThrowIfNull(evaluate);

        var urls = new List<SeoSitemapUrl>();
        foreach (var candidate in candidates)
        {
            ArgumentNullException.ThrowIfNull(candidate);
            var evaluation = evaluate(candidate);
            if (!evaluation.IsIndexable)
            {
                continue;
            }

            var locale = SeoRoute.NormalizeLocale(candidate.Locale);
            var path = SeoRoute.NormalizePath(candidate.Path);
            urls.Add(new SeoSitemapUrl(locale, path, $"/{locale}/{path}"));
        }

        return urls
            .OrderBy(u => u.Locale, StringComparer.Ordinal)
            .ThenBy(u => u.Path, StringComparer.Ordinal)
            .ToList();
    }

    public static string RenderUrlSetXml(IEnumerable<SeoSitemapUrl> urls)
    {
        ArgumentNullException.ThrowIfNull(urls);
        var sb = new System.Text.StringBuilder();
        sb.Append("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
        sb.Append("<urlset xmlns=\"http://www.sitemaps.org/schemas/sitemap/0.9\">");
        foreach (var url in urls)
        {
            sb.Append("<url><loc>");
            sb.Append(System.Security.SecurityElement.Escape(url.Loc));
            sb.Append("</loc></url>");
        }

        sb.Append("</urlset>");
        return sb.ToString();
    }
}

/// <summary>Minimal robots.txt framework referencing the SEO sitemap endpoint.</summary>
public static class SeoRobotsTxtEngine
{
    public const string DefaultSitemapPath = "/api/seo/sitemap.xml";

    public static string Render(string sitemapPath = DefaultSitemapPath)
    {
        var path = string.IsNullOrWhiteSpace(sitemapPath)
            ? DefaultSitemapPath
            : sitemapPath.Trim();
        if (!path.StartsWith('/'))
        {
            path = "/" + path;
        }

        return $"User-agent: *{Environment.NewLine}Allow: /{Environment.NewLine}Sitemap: {path}{Environment.NewLine}";
    }
}
