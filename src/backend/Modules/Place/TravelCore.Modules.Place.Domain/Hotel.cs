namespace TravelCore.Modules.Place.Domain;

/// <summary>
/// Hotel catalog specialization (1:1 with <see cref="Place"/> via <see cref="PlaceId"/>).
/// Catalog facts only — no rooms/rates/availability/reservation (HotelBooking owns those).
/// </summary>
public sealed class Hotel
{
    private Hotel()
    {
    }

    private Hotel(PlaceId placeId, short? starRating)
    {
        PlaceId = placeId;
        StarRating = starRating;
    }

    public PlaceId PlaceId { get; private set; }

    /// <summary>Optional catalog star rating 1–5. Not live bookability.</summary>
    public short? StarRating { get; private set; }

    public static Hotel Create(PlaceId placeId, short? starRating = null)
    {
        if (placeId.Value == Guid.Empty)
        {
            throw new ArgumentException("PlaceId cannot be empty.", nameof(placeId));
        }

        return new Hotel(placeId, NormalizeStarRating(starRating));
    }

    public static short? NormalizeStarRating(short? starRating)
    {
        if (starRating is null)
        {
            return null;
        }

        if (starRating is < 1 or > 5)
        {
            throw new ArgumentOutOfRangeException(
                nameof(starRating),
                starRating,
                "Hotel star rating must be between 1 and 5 when provided.");
        }

        return starRating;
    }
}
