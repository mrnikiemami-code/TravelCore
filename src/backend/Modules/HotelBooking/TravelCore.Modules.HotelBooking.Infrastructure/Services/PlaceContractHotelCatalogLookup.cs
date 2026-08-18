using TravelCore.Modules.HotelBooking.Contracts;
using TravelCore.Modules.Place.Contracts;

namespace TravelCore.Modules.HotelBooking.Infrastructure.Services;

/// <summary>
/// Place catalog validation through Place.Contracts only. HotelBooking does not query Place persistence.
/// </summary>
internal sealed class PlaceContractHotelCatalogLookup : IHotelPlaceCatalogLookup
{
    private readonly IPlaceService _places;

    public PlaceContractHotelCatalogLookup(IPlaceService places)
    {
        ArgumentNullException.ThrowIfNull(places);
        _places = places;
    }

    public async Task<bool> IsActiveHotelPlaceAsync(
        Guid placeId,
        CancellationToken cancellationToken = default)
    {
        if (placeId == Guid.Empty)
        {
            return false;
        }

        var place = await _places.GetByIdAsync(placeId, cancellationToken);
        if (place is null)
        {
            return false;
        }

        return string.Equals(place.Kind, "Hotel", StringComparison.Ordinal)
            && string.Equals(place.CatalogStatus, "Active", StringComparison.Ordinal);
    }
}
