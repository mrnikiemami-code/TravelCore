using TravelCore.Identifiers;

namespace TravelCore.Modules.Flight.Domain;

public readonly record struct FlightPassengerId(Guid Value)
{
    public static FlightPassengerId New() => new(Uuid7.New());

    public static FlightPassengerId From(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("FlightPassengerId cannot be empty.", nameof(value));
        }

        return new FlightPassengerId(value);
    }

    public override string ToString() => Value.ToString("D");
}
