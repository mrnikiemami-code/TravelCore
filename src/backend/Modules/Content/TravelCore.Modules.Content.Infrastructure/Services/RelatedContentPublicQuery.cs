using Microsoft.EntityFrameworkCore;
using TravelCore.Modules.Content.Contracts;
using TravelCore.Modules.Content.Domain;

namespace TravelCore.Modules.Content.Infrastructure.Services;

/// <summary>
/// Deterministic same-destination related content (TC-P14-T006). Title+slug gate. No score.
/// </summary>
public sealed class RelatedContentPublicQuery : IRelatedContentPublicQuery
{
    private readonly ContentDbContext _db;

    public RelatedContentPublicQuery(ContentDbContext db)
    {
        _db = db;
    }

    public Task<IReadOnlyList<RelatedPublishedContent>> GetByDestinationAsync(
        Guid destinationId,
        string localeCode,
        CancellationToken cancellationToken = default)
    {
        if (destinationId == Guid.Empty)
        {
            throw new ArgumentException("DestinationId cannot be empty.", nameof(destinationId));
        }

        return QueryAsync([destinationId], localeCode, cancellationToken);
    }

    public Task<IReadOnlyList<RelatedPublishedContent>> GetByDestinationsAsync(
        IReadOnlyCollection<Guid> destinationIds,
        string localeCode,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(destinationIds);
        var ids = destinationIds.Where(id => id != Guid.Empty).Distinct().ToList();
        if (ids.Count == 0)
        {
            throw new ArgumentException("At least one DestinationId is required.", nameof(destinationIds));
        }

        return QueryAsync(ids, localeCode, cancellationToken);
    }

    private async Task<IReadOnlyList<RelatedPublishedContent>> QueryAsync(
        IReadOnlyCollection<Guid> destinationIds,
        string localeCode,
        CancellationToken cancellationToken)
    {
        var locale = string.IsNullOrWhiteSpace(localeCode) ? "en" : localeCode.Trim();
        var destinationIdList = destinationIds.ToList();
        var items = await _db.ContentItems
            .AsNoTracking()
            .Where(x => x.Destinations.Any(d => destinationIdList.Contains(d.DestinationId)))
            .ToListAsync(cancellationToken);

        return items
            .OrderBy(x => x.Code, StringComparer.Ordinal)
            .ThenBy(x => x.Id.Value)
            .Select(x => Map(x, locale))
            .Where(x => x is not null)
            .Cast<RelatedPublishedContent>()
            .Take(RelatedContentPublicEligibility.MaxItems)
            .ToList();
    }

    private static RelatedPublishedContent? Map(ContentItem item, string localeCode)
    {
        var translation = item.Translations.FirstOrDefault(t =>
            string.Equals(t.LocaleCode, localeCode, StringComparison.OrdinalIgnoreCase));
        var title = translation?.Title?.Trim();
        var slug = translation?.Slug?.Trim();
        if (!RelatedContentPublicEligibility.IsPubliclyEligible(title, slug))
        {
            return null;
        }

        return new RelatedPublishedContent(
            item.Id.Value,
            item.Kind.ToString(),
            item.Code,
            title!,
            slug!);
    }
}
