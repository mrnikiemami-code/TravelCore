namespace TravelCore.Modules.Tour.Domain;

/// <summary>
/// Accommodation plan entry owned by Experience specialization (P10-R3 · TC-P10-T004).
/// Optional logical PlaceId (Hotel-kind validated at application boundary). Not TourHotelOption / HotelBooking.
/// </summary>
public sealed class ExperienceAccommodationPlanEntry
{
    public const int MaxEntriesPerExperience = 40;

    private ExperienceAccommodationPlanEntry()
    {
    }

    private ExperienceAccommodationPlanEntry(
        ExperienceAccommodationPlanId id,
        TourProductId tourProductId,
        int sortOrder,
        Guid? placeId)
    {
        if (id.Value == Guid.Empty)
        {
            throw new ArgumentException("ExperienceAccommodationPlanId cannot be empty.", nameof(id));
        }

        if (tourProductId.Value == Guid.Empty)
        {
            throw new ArgumentException("TourProductId cannot be empty.", nameof(tourProductId));
        }

        if (sortOrder < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(sortOrder), sortOrder, "SortOrder must be >= 0.");
        }

        Id = id;
        TourProductId = tourProductId;
        SortOrder = sortOrder;
        PlaceId = NormalizeOptionalPlaceId(placeId);
    }

    public ExperienceAccommodationPlanId Id { get; private set; }

    public TourProductId TourProductId { get; private set; }

    public int SortOrder { get; private set; }

    /// <summary>Optional logical Place Hotel identity (0..1 per entry). Place remains SoR.</summary>
    public Guid? PlaceId { get; private set; }

    internal static ExperienceAccommodationPlanEntry Create(
        ExperienceAccommodationPlanId id,
        TourProductId tourProductId,
        int sortOrder,
        Guid? placeId)
        => new(id, tourProductId, sortOrder, placeId);

    public static ExperienceAccommodationPlanEntry Reconstitute(
        ExperienceAccommodationPlanId id,
        TourProductId tourProductId,
        int sortOrder,
        Guid? placeId)
        => new(id, tourProductId, sortOrder, placeId);

    public void SetPlaceLink(Guid? placeId) => PlaceId = NormalizeOptionalPlaceId(placeId);

    private static Guid? NormalizeOptionalPlaceId(Guid? placeId)
    {
        if (placeId == Guid.Empty)
        {
            throw new ArgumentException(
                "PlaceId cannot be empty. Use null to clear the hotel reference.",
                nameof(placeId));
        }

        return placeId;
    }
}
