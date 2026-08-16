using Microsoft.EntityFrameworkCore;
using TravelCore.Modules.Seo.Contracts;
using TravelCore.Modules.Seo.Domain;

namespace TravelCore.Modules.Seo.Infrastructure.Services;

/// <summary>
/// Sitemap/robots framework (TC-P05-T009): include only IndexPolicy-eligible routes.
/// </summary>
public sealed class SeoSitemapApplicationService : ISeoSitemapService
{
    private readonly SeoDbContext _db;
    private readonly ISeoIndexPolicyService _indexPolicies;

    public SeoSitemapApplicationService(SeoDbContext db, ISeoIndexPolicyService indexPolicies)
    {
        _db = db;
        _indexPolicies = indexPolicies;
    }

    public async Task<SeoSitemapDocumentResponse> BuildAsync(CancellationToken cancellationToken = default)
    {
        var routes = await _db.SeoRoutes.AsNoTracking().ToListAsync(cancellationToken);
        var candidates = routes
            .GroupBy(r => (r.Locale, r.Path), StringTupleComparer.Instance)
            .Select(g => g.First())
            .Select(r => new SeoSitemapCandidate(r.Locale, r.Path, r.ResourceType, r.ResourceId))
            .ToList();

        var evaluationByKey = new Dictionary<(string Locale, string Path), bool>();
        foreach (var candidate in candidates)
        {
            var key = (candidate.Locale, candidate.Path);
            if (evaluationByKey.ContainsKey(key))
            {
                continue;
            }

            var evaluation = await _indexPolicies.EvaluatePathAsync(
                candidate.Locale,
                candidate.Path,
                cancellationToken);
            evaluationByKey[key] = evaluation.IsIndexable;
        }

        var urls = SeoSitemapEngine.SelectIndexableUrls(
            candidates,
            c => evaluationByKey[(c.Locale, c.Path)]
                ? SeoIndexabilityEvaluation.Indexable(
                    c.Locale,
                    c.Path,
                    SeoFollowDirective.Follow,
                    SeoIndexDirective.Index,
                    SeoFollowDirective.Follow,
                    "indexable")
                : SeoIndexabilityEvaluation.ConservativeNoIndex(
                    c.Locale,
                    c.Path,
                    null,
                    null,
                    "not-indexable"));

        return new SeoSitemapDocumentResponse(
            urls.Select(u => new SeoSitemapUrlResponse(u.Locale, u.Path, u.Loc)).ToList(),
            urls.Count,
            candidates.Count);
    }

    public async Task<string> RenderSitemapXmlAsync(CancellationToken cancellationToken = default)
    {
        var doc = await BuildAsync(cancellationToken);
        var urls = doc.Urls.Select(u => new SeoSitemapUrl(u.Locale, u.Path, u.Loc));
        return SeoSitemapEngine.RenderUrlSetXml(urls);
    }

    public string RenderRobotsTxt() => SeoRobotsTxtEngine.Render();

    private sealed class StringTupleComparer : IEqualityComparer<(string Locale, string Path)>
    {
        public static readonly StringTupleComparer Instance = new();

        public bool Equals((string Locale, string Path) x, (string Locale, string Path) y) =>
            string.Equals(x.Locale, y.Locale, StringComparison.Ordinal)
            && string.Equals(x.Path, y.Path, StringComparison.Ordinal);

        public int GetHashCode((string Locale, string Path) obj) =>
            HashCode.Combine(obj.Locale, obj.Path);
    }
}
