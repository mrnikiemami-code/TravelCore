using TravelCore.Identifiers;

namespace TravelCore.Modules.Flight.Domain;

/// <summary>
/// Strongly typed FlightBookingCancellation identity (UUID v7).
/// </summary>
public readonly record struct FlightBookingCancellationId(Guid Value)
{
    public static FlightBookingCancellationId New() => new(Uuid7.New());

    public static FlightBookingCancellationId From(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("FlightBookingCancellationId cannot be empty.", nameof(value));
        }

        return new FlightBookingCancellationId(value);
    }

    public override string ToString() => Value.ToString("D");
}
