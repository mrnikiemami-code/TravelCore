using Microsoft.EntityFrameworkCore;
using TravelCore.Modules.Place.Contracts;
using TravelCore.Modules.Place.Domain;

namespace TravelCore.Modules.Place.Infrastructure.Services;

/// <summary>
/// Anonymous public hotel browse reads (TC-HOTIDX-T002).
/// Active hotels with locale slug only — not admin catalog list · not Search.
/// </summary>
internal sealed class PlacePublicQuery : IPlacePublicHotelBrowseQuery
{
    private readonly PlaceDbContext _db;

    public PlacePublicQuery(PlaceDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<PublicHotelBrowseItem>> ListByLocaleAsync(
        string localeCode,
        int take,
        CancellationToken cancellationToken = default)
    {
        if (take <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(take), "Take must be positive.");
        }

        if (take > PlacePublicBrowseLimits.MaxPublicHotels)
        {
            throw new ArgumentOutOfRangeException(
                nameof(take),
                $"Take cannot exceed {PlacePublicBrowseLimits.MaxPublicHotels}.");
        }

        var locale = PlaceTranslation.NormalizeLocaleCode(localeCode);
        // Avoid PlaceId.Value / owned Hotel inside SelectMany+OrderBy (EF translation failure — TC-P32-T004).
        // Star ratings omitted from this browse projection to keep the query translatable;
        // listing cards do not require Hotel owned-type join here.
        var rows = await _db.Places
            .AsNoTracking()
            .Where(x =>
                x.Kind == PlaceKind.Hotel
                && x.CatalogStatus == PlaceCatalogStatus.Active)
            .SelectMany(p => p.Translations
                .Where(t => t.LocaleCode == locale && t.Slug != null)
                .Select(t => new
                {
                    PlaceId = p.Id,
                    LocaleCode = t.LocaleCode,
                    Slug = t.Slug,
                    Name = t.Name,
                    Description = t.Description,
                }))
            .OrderBy(x => x.Name)
            .ThenBy(x => x.PlaceId)
            .Take(take)
            .ToListAsync(cancellationToken);

        return rows
            .Select(x => new PublicHotelBrowseItem(
                x.PlaceId.Value,
                x.LocaleCode,
                x.Slug!,
                x.Name,
                x.Description,
                StarRating: null))
            .ToList();
    }
}
