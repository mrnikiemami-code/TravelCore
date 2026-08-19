namespace TravelCore.Modules.Place.Contracts;

/// <summary>
/// Public hotel browse read model (TC-HOTIDX-T001).
/// Active Place.Hotel with locale slug — not HotelBooking availability or Search.
/// </summary>
public static class PlacePublicBrowseLimits
{
    public const int MaxPublicHotels = 50;
}

public sealed record PublicHotelBrowseItem(
    Guid PlaceId,
    string LocaleCode,
    string Slug,
    string Name,
    string? Description,
    short? StarRating);

public interface IPlacePublicHotelBrowseQuery
{
    Task<IReadOnlyList<PublicHotelBrowseItem>> ListByLocaleAsync(
        string localeCode,
        int take,
        CancellationToken cancellationToken = default);
}
