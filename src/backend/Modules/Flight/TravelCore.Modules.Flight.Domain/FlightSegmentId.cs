using TravelCore.Identifiers;

namespace TravelCore.Modules.Flight.Domain;

public readonly record struct FlightSegmentId(Guid Value)
{
    public static FlightSegmentId New() => new(Uuid7.New());

    public static FlightSegmentId From(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("FlightSegmentId cannot be empty.", nameof(value));
        }

        return new FlightSegmentId(value);
    }

    public override string ToString() => Value.ToString("D");
}
