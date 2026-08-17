namespace TravelCore.Modules.Tour.Domain;

/// <summary>
/// Ordered stop under an <see cref="ExperienceItineraryDay"/> (TC-P10-T002 structure).
/// Destination / Attraction (Place) semantic links are deferred to TC-P10-T003 / P10-R2.
/// </summary>
public sealed class ExperienceItineraryStop
{
    private ExperienceItineraryStop()
    {
    }

    private ExperienceItineraryStop(ItineraryStopId id, ItineraryDayId itineraryDayId, int sortOrder)
    {
        if (id.Value == Guid.Empty)
        {
            throw new ArgumentException("ItineraryStopId cannot be empty.", nameof(id));
        }

        if (itineraryDayId.Value == Guid.Empty)
        {
            throw new ArgumentException("ItineraryDayId cannot be empty.", nameof(itineraryDayId));
        }

        if (sortOrder < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(sortOrder), sortOrder, "SortOrder must be >= 0.");
        }

        Id = id;
        ItineraryDayId = itineraryDayId;
        SortOrder = sortOrder;
    }

    public ItineraryStopId Id { get; private set; }

    public ItineraryDayId ItineraryDayId { get; private set; }

    public int SortOrder { get; private set; }

    internal static ExperienceItineraryStop Create(ItineraryStopId id, ItineraryDayId itineraryDayId, int sortOrder)
        => new(id, itineraryDayId, sortOrder);

    public static ExperienceItineraryStop Reconstitute(
        ItineraryStopId id,
        ItineraryDayId itineraryDayId,
        int sortOrder)
        => new(id, itineraryDayId, sortOrder);
}
