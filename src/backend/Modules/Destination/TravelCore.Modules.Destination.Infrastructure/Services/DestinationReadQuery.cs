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
                x.Description))
            .ToList();
    }

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
