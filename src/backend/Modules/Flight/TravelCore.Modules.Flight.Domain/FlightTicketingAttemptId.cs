using TravelCore.Identifiers;

namespace TravelCore.Modules.Flight.Domain;

public readonly record struct FlightTicketingAttemptId(Guid Value)
{
    public static FlightTicketingAttemptId New() => new(Uuid7.New());

    public static FlightTicketingAttemptId From(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("FlightTicketingAttemptId cannot be empty.", nameof(value));
        }

        return new FlightTicketingAttemptId(value);
    }

    public override string ToString() => Value.ToString("D");
}
