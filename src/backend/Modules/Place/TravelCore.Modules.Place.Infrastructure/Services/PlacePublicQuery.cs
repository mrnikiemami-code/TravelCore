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
        var rows = await _db.Places
            .AsNoTracking()
            .Where(x =>
                x.Kind == PlaceKind.Hotel
                && x.CatalogStatus == PlaceCatalogStatus.Active)
            .SelectMany(p => p.Translations.Select(t => new { Place = p, Translation = t }))
            .Where(x =>
                x.Translation.LocaleCode == locale
                && x.Translation.Slug != null)
            .OrderBy(x => x.Translation.Name)
            .ThenBy(x => x.Place.Id.Value)
            .Take(take)
            .ToListAsync(cancellationToken);

        return rows
            .Select(x => new PublicHotelBrowseItem(
                x.Place.Id.Value,
                x.Translation.LocaleCode,
                x.Translation.Slug!,
                x.Translation.Name,
                x.Translation.Description,
                x.Place.Hotel?.StarRating))
            .ToList();
    }
}
