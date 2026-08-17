using Microsoft.EntityFrameworkCore;
using TravelCore.Modules.Tour.Contracts;
using TravelCore.Modules.Tour.Domain;

namespace TravelCore.Modules.Tour.Infrastructure.Services;

/// <summary>
/// Deterministic same-destination related tours (TC-P14-T005). Published only. No score.
/// </summary>
public sealed class RelatedTourPublicQuery : IRelatedTourPublicQuery
{
    private readonly TourDbContext _db;

    public RelatedTourPublicQuery(TourDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<RelatedPublishedTour>> GetByTourProductAsync(
        Guid tourProductId,
        string localeCode,
        CancellationToken cancellationToken = default)
    {
        if (tourProductId == Guid.Empty)
        {
            throw new ArgumentException("TourProductId cannot be empty.", nameof(tourProductId));
        }

        var id = TourProductId.From(tourProductId);
        var source = await _db.TourProducts
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (source is null || source.CatalogStatus != TourCatalogStatus.Published)
        {
            return [];
        }

        var destinationIds = source.Destinations.Select(d => d.DestinationId).ToList();
        if (destinationIds.Count == 0)
        {
            return [];
        }

        return await QueryAsync(destinationIds, localeCode, exclude: id, cancellationToken);
    }

    public async Task<IReadOnlyList<RelatedPublishedTour>> GetByDestinationAsync(
        Guid destinationId,
        string localeCode,
        Guid? excludeTourProductId,
        CancellationToken cancellationToken = default)
    {
        if (destinationId == Guid.Empty)
        {
            throw new ArgumentException("DestinationId cannot be empty.", nameof(destinationId));
        }

        TourProductId? exclude = excludeTourProductId is { } value && value != Guid.Empty
            ? TourProductId.From(value)
            : null;
        return await QueryAsync([destinationId], localeCode, exclude, cancellationToken);
    }

    private async Task<IReadOnlyList<RelatedPublishedTour>> QueryAsync(
        IReadOnlyCollection<Guid> destinationIds,
        string localeCode,
        TourProductId? exclude,
        CancellationToken cancellationToken)
    {
        var locale = string.IsNullOrWhiteSpace(localeCode) ? "en" : localeCode.Trim();
        var destinationIdList = destinationIds.ToList();
        var query = _db.TourProducts
            .AsNoTracking()
            .Where(x => x.CatalogStatus == TourCatalogStatus.Published)
            .Where(x => x.Destinations.Any(d => destinationIdList.Contains(d.DestinationId)));
        if (exclude is { } excluded)
        {
            query = query.Where(x => x.Id != excluded);
        }

        var products = await query.ToListAsync(cancellationToken);

        return products
            .OrderBy(x => x.Code, StringComparer.Ordinal)
            .ThenBy(x => x.Id.Value)
            .Select(x => Map(x, locale))
            .Where(x => x is not null)
            .Cast<RelatedPublishedTour>()
            .Take(RelatedTourPublicEligibility.MaxItems)
            .ToList();
    }

    private static RelatedPublishedTour? Map(TourProduct product, string localeCode)
    {
        var translation = product.Translations.FirstOrDefault(t =>
            string.Equals(t.LocaleCode, localeCode, StringComparison.OrdinalIgnoreCase));
        var title = translation?.Title?.Trim();
        var slug = translation?.Slug?.Trim();
        if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(slug))
        {
            return null;
        }

        if (!RelatedTourPublicEligibility.IsEligible(product.CatalogStatus.ToString()))
        {
            return null;
        }

        return new RelatedPublishedTour(
            product.Id.Value,
            product.Kind.ToString(),
            product.Code,
            title,
            slug);
    }
}
