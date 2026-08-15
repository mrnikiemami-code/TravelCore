using Microsoft.EntityFrameworkCore;
using TravelCore.Modules.Destination.Contracts;
using TravelCore.Modules.Destination.Domain;

namespace TravelCore.Modules.Destination.Infrastructure.Services;

public sealed class DestinationReadQuery : IDestinationReadQuery
{
    private readonly DestinationDbContext _db;

    public DestinationReadQuery(DestinationDbContext db)
    {
        _db = db;
    }

    public Task<DestinationResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        GetByIdAsync(id, locale: null, cancellationToken);

    public async Task<DestinationResponse?> GetByIdAsync(
        Guid id,
        string? locale,
        CancellationToken cancellationToken = default)
    {
        var destinationId = DestinationId.From(id);
        var destination = await _db.Destinations.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == destinationId, cancellationToken);
        return destination is null ? null : Map(destination, locale);
    }

    public async Task<IReadOnlyList<DestinationResponse>> ListChildrenAsync(
        Guid parentId,
        CancellationToken cancellationToken = default)
    {
        var id = DestinationId.From(parentId);
        var children = await _db.Destinations.AsNoTracking()
            .Where(x => x.ParentId == id)
            .OrderBy(x => x.EnglishName)
            .ThenBy(x => x.Code)
            .ToListAsync(cancellationToken);

        return children.Select(x => Map(x)).ToList();
    }

    public async Task<IReadOnlyList<DestinationTranslationResponse>> ListTranslationsAsync(
        Guid destinationId,
        CancellationToken cancellationToken = default)
    {
        var id = DestinationId.From(destinationId);
        var destination = await _db.Destinations.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (destination is null)
        {
            return Array.Empty<DestinationTranslationResponse>();
        }

        return destination.Translations
            .OrderBy(x => x.LocaleCode, StringComparer.Ordinal)
            .Select(x => new DestinationTranslationResponse(
                x.DestinationId.Value,
                x.LocaleCode,
                x.Name,
                x.Description,
                x.Slug))
            .ToList();
    }

    public async Task<DestinationSlugLookupResponse?> FindBySlugAsync(
        string localeCode,
        string slug,
        CancellationToken cancellationToken = default)
    {
        var normalizedLocale = DestinationTranslation.NormalizeLocaleCode(localeCode);
        var normalizedSlug = DestinationTranslation.NormalizeSlug(slug)
            ?? throw new ArgumentException("Slug is required.", nameof(slug));

        var hit = await _db.Destinations.AsNoTracking()
            .SelectMany(d => d.Translations.Select(t => new { Destination = d, Translation = t }))
            .FirstOrDefaultAsync(
                x => x.Translation.LocaleCode == normalizedLocale && x.Translation.Slug == normalizedSlug,
                cancellationToken);

        if (hit is null)
        {
            return null;
        }

        return new DestinationSlugLookupResponse(
            hit.Destination.Id.Value,
            hit.Translation.LocaleCode,
            hit.Translation.Slug!,
            hit.Destination.Kind.ToString(),
            hit.Destination.Code,
            hit.Destination.EnglishName);
    }

    public async Task<IReadOnlyList<DestinationPathNode>> ListAncestorsAsync(
        Guid destinationId,
        CancellationToken cancellationToken = default)
    {
        var path = await GetPathAsync(destinationId, cancellationToken);
        return path?.AncestorsRootFirst ?? Array.Empty<DestinationPathNode>();
    }

    public async Task<DestinationPathResponse?> GetPathAsync(
        Guid destinationId,
        CancellationToken cancellationToken = default)
    {
        var id = DestinationId.From(destinationId);
        var current = await _db.Destinations.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (current is null)
        {
            return null;
        }

        var chainLeafToRoot = new List<Domain.Destination> { current };
        var guard = 0;
        while (current.ParentId is not null)
        {
            if (++guard > 64)
            {
                throw new InvalidOperationException("Destination parent chain exceeded safety depth.");
            }

            var parentId = current.ParentId.Value;
            current = await _db.Destinations.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == parentId, cancellationToken)
                ?? throw new InvalidOperationException($"Broken destination parent link at {parentId}.");
            chainLeafToRoot.Add(current);
        }

        chainLeafToRoot.Reverse();
        var nodes = chainLeafToRoot
            .Select((x, index) => ToPathNode(x, index))
            .ToList();

        var self = nodes[^1];
        IReadOnlyList<DestinationPathNode> ancestors = nodes.Count == 1
            ? Array.Empty<DestinationPathNode>()
            : nodes.Take(nodes.Count - 1).ToList();
        return new DestinationPathResponse(destinationId, ancestors, self, nodes);
    }

    public async Task<DestinationDescendantsResponse?> ListDescendantsAsync(
        Guid destinationId,
        int maxDepth,
        CancellationToken cancellationToken = default)
    {
        if (maxDepth < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxDepth), "maxDepth must be >= 0.");
        }

        // Cap to prevent accidental unbounded scans; hierarchy is shallow by design.
        maxDepth = Math.Min(maxDepth, 16);

        var rootId = DestinationId.From(destinationId);
        var root = await _db.Destinations.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == rootId, cancellationToken);
        if (root is null)
        {
            return null;
        }

        if (maxDepth == 0)
        {
            return new DestinationDescendantsResponse(destinationId, maxDepth, Array.Empty<DestinationPathNode>());
        }

        var results = new List<DestinationPathNode>();
        var frontier = new List<DestinationId> { rootId };
        var depthFromRoot = 0;

        for (var level = 1; level <= maxDepth && frontier.Count > 0; level++)
        {
            depthFromRoot = level;
            var parentIds = frontier.Select(x => (DestinationId?)x).ToList();
            var children = await _db.Destinations.AsNoTracking()
                .Where(x => x.ParentId != null && parentIds.Contains(x.ParentId))
                .OrderBy(x => x.EnglishName)
                .ThenBy(x => x.Code)
                .ToListAsync(cancellationToken);

            results.AddRange(children.Select(x => ToPathNode(x, depthFromRoot)));
            frontier = children.Select(x => x.Id).ToList();
        }

        return new DestinationDescendantsResponse(destinationId, maxDepth, results);
    }

    private static DestinationPathNode ToPathNode(Domain.Destination destination, int depthFromRoot) =>
        new(
            destination.Id.Value,
            destination.Kind.ToString(),
            destination.Code,
            destination.EnglishName,
            destination.ParentId?.Value,
            depthFromRoot);

    private static DestinationResponse Map(Domain.Destination destination, string? locale = null)
    {
        string? localizedName = null;
        string? localizedDescription = null;
        string? resolvedLocale = null;

        if (!string.IsNullOrWhiteSpace(locale))
        {
            var translation = destination.FindTranslation(locale);
            if (translation is not null)
            {
                localizedName = translation.Name;
                localizedDescription = translation.Description;
                resolvedLocale = translation.LocaleCode;
            }
        }

        return new(
            destination.Id.Value,
            destination.Kind.ToString(),
            destination.Code,
            destination.EnglishName,
            destination.ParentId?.Value,
            destination.IsoCountryCode,
            destination.Latitude,
            destination.Longitude,
            localizedName,
            localizedDescription,
            resolvedLocale);
    }
}
