namespace TravelCore.Modules.HotelBooking.Domain;

/// <summary>
/// Opaque logical Place identifier for the hotel/accommodation catalog entity being reserved.
/// HotelBooking does not clone Place catalog facts and does not reference Place.Domain.
/// </summary>
public readonly record struct HotelPlaceReference
{
    public Guid PlaceId { get; }

    public HotelPlaceReference(Guid placeId)
    {
        if (placeId == Guid.Empty)
        {
            throw new ArgumentException(
                "HotelPlaceReference requires a non-empty Place identifier.",
                nameof(placeId));
        }

        PlaceId = placeId;
    }
}
