using TravelCore.Identifiers;

namespace TravelCore.Modules.Flight.Domain;

public readonly record struct FlightTicketId(Guid Value)
{
    public static FlightTicketId New() => new(Uuid7.New());

    public static FlightTicketId From(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("FlightTicketId cannot be empty.", nameof(value));
        }

        return new FlightTicketId(value);
    }

    public override string ToString() => Value.ToString("D");
}
