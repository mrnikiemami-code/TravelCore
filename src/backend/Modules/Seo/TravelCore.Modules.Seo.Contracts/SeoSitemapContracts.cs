namespace TravelCore.Modules.Seo.Contracts;

public sealed record SeoSitemapUrlResponse(
    string Locale,
    string Path,
    string Loc);

public sealed record SeoSitemapDocumentResponse(
    IReadOnlyList<SeoSitemapUrlResponse> Urls,
    int IncludedCount,
    int ConsideredCount);

/// <summary>Sitemap/robots framework contracts (TC-P05-T009) — IndexPolicy-gated.</summary>
public interface ISeoSitemapService
{
    Task<SeoSitemapDocumentResponse> BuildAsync(CancellationToken cancellationToken = default);

    Task<string> RenderSitemapXmlAsync(CancellationToken cancellationToken = default);

    string RenderRobotsTxt();
}
