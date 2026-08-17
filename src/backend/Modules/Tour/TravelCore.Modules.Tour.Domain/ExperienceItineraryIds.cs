using TravelCore.Identifiers;

namespace TravelCore.Modules.Tour.Domain;

/// <summary>Strongly typed ItineraryDay identity (UUID v7). Owned by Experience itinerary (P10-R1).</summary>
public readonly record struct ItineraryDayId(Guid Value) : IEquatable<ItineraryDayId>
{
    public static ItineraryDayId New() => new(Uuid7.New());

    public static ItineraryDayId From(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("ItineraryDayId cannot be empty.", nameof(value));
        }

        return new ItineraryDayId(value);
    }

    public override string ToString() => Value.ToString("D");
}

/// <summary>Strongly typed itinerary Stop identity (UUID v7). Structure only in T002 — no Place/Destination links.</summary>
public readonly record struct ItineraryStopId(Guid Value) : IEquatable<ItineraryStopId>
{
    public static ItineraryStopId New() => new(Uuid7.New());

    public static ItineraryStopId From(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("ItineraryStopId cannot be empty.", nameof(value));
        }

        return new ItineraryStopId(value);
    }

    public override string ToString() => Value.ToString("D");
}
