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
        // Star ratings loaded via a separate query using PlaceId struct Contains (TC-P32-T005).
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

        var placeIds = rows.Select(x => x.PlaceId).ToList();
        Dictionary<Guid, short?> starByPlaceId = [];
        if (placeIds.Count > 0)
        {
            var starRows = await _db.Places
                .AsNoTracking()
                .Where(p => placeIds.Contains(p.Id))
                .Select(p => new
                {
                    PlaceId = p.Id,
                    StarRating = p.Hotel == null ? (short?)null : p.Hotel.StarRating,
                })
                .ToListAsync(cancellationToken);
            starByPlaceId = starRows.ToDictionary(x => x.PlaceId.Value, x => x.StarRating);
        }

        return rows
            .Select(x => new PublicHotelBrowseItem(
                x.PlaceId.Value,
                x.LocaleCode,
                x.Slug!,
                x.Name,
                x.Description,
                starByPlaceId.GetValueOrDefault(x.PlaceId.Value)))
            .ToList();
    }
}
