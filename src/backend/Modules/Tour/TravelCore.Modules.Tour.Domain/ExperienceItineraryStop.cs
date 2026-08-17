namespace TravelCore.Modules.Tour.Domain;

/// <summary>
/// Ordered stop under an <see cref="ExperienceItineraryDay"/> (TC-P10-T002/T003 · P10-R2).
/// Optional logical <see cref="DestinationId"/> / <see cref="PlaceId"/> refs — no cross-schema FK.
/// Attraction = PlaceId with PlaceKind Attraction validated at application boundary.
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

    /// <summary>Optional logical Destination identity (0..1; P10-R2). Destination remains SoR.</summary>
    public Guid? DestinationId { get; private set; }

    /// <summary>
    /// Optional logical Place identity (0..1; P10-R2). Attraction is PlaceId + Attraction kind — not a separate aggregate id.
    /// </summary>
    public Guid? PlaceId { get; private set; }

    internal static ExperienceItineraryStop Create(ItineraryStopId id, ItineraryDayId itineraryDayId, int sortOrder)
        => new(id, itineraryDayId, sortOrder);

    public static ExperienceItineraryStop Reconstitute(
        ItineraryStopId id,
        ItineraryDayId itineraryDayId,
        int sortOrder,
        Guid? destinationId = null,
        Guid? placeId = null)
    {
        var stop = new ExperienceItineraryStop(id, itineraryDayId, sortOrder);
        stop.DestinationId = NormalizeOptionalId(destinationId, nameof(destinationId));
        stop.PlaceId = NormalizeOptionalId(placeId, nameof(placeId));
        return stop;
    }

    /// <summary>Sets Destination logical link (0..1). Null clears; empty Guid rejected.</summary>
    public void SetDestinationLink(Guid? destinationId)
    {
        DestinationId = NormalizeOptionalId(destinationId, nameof(destinationId));
    }

    /// <summary>Sets Place logical link (0..1). Null clears; empty Guid rejected. Attraction kind validated outside Domain.</summary>
    public void SetPlaceLink(Guid? placeId)
    {
        PlaceId = NormalizeOptionalId(placeId, nameof(placeId));
    }

    private static Guid? NormalizeOptionalId(Guid? id, string paramName)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException(
                $"{paramName} cannot be empty. Use null to clear the link.",
                paramName);
        }

        return id;
    }
}
