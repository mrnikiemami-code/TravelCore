namespace TravelCore.Modules.Tour.Domain;

/// <summary>
/// Descriptive accommodation option on a TourDeparture (P11-R6 · TC-P11-T006).
/// Logical PlaceId only — Place remains SoR of hotel identity.
/// Not HotelBooking, Room, Rate, inventory, or pricing.
/// Named AccommodationOption (not TourHotelOption) to avoid commerce conflation.
/// </summary>
public sealed class TourDepartureAccommodationOption
{
    private TourDepartureAccommodationOption()
    {
    }

    private TourDepartureAccommodationOption(
        TourDepartureAccommodationOptionId id,
        TourDepartureId tourDepartureId,
        Guid placeId,
        int nights,
        TourDepartureBoardType boardType)
    {
        Id = id;
        TourDepartureId = tourDepartureId;
        PlaceId = placeId;
        Nights = nights;
        BoardType = boardType;
    }

    public TourDepartureAccommodationOptionId Id { get; private set; }

    public TourDepartureId TourDepartureId { get; private set; }

    /// <summary>Logical Place identity (hotel). No EF FK to Place.</summary>
    public Guid PlaceId { get; private set; }

    public int Nights { get; private set; }

    public TourDepartureBoardType BoardType { get; private set; }

    internal static TourDepartureAccommodationOption Create(
        TourDepartureId tourDepartureId,
        Guid placeId,
        int nights,
        TourDepartureBoardType boardType)
    {
        if (placeId == Guid.Empty)
        {
            throw new ArgumentException("PlaceId cannot be empty.", nameof(placeId));
        }

        if (nights <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(nights), nights, "Nights must be > 0.");
        }

        if (!Enum.IsDefined(boardType))
        {
            throw new ArgumentOutOfRangeException(nameof(boardType), boardType, "Unsupported board type.");
        }

        return new TourDepartureAccommodationOption(
            TourDepartureAccommodationOptionId.New(),
            tourDepartureId,
            placeId,
            nights,
            boardType);
    }
}
