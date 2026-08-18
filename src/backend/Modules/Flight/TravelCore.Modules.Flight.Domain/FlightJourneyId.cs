using TravelCore.Identifiers;

namespace TravelCore.Modules.Flight.Domain;

public readonly record struct FlightJourneyId(Guid Value)
{
    public static FlightJourneyId New() => new(Uuid7.New());

    public static FlightJourneyId From(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("FlightJourneyId cannot be empty.", nameof(value));
        }

        return new FlightJourneyId(value);
    }

    public override string ToString() => Value.ToString("D");
}
