using TravelCore.Identifiers;

namespace TravelCore.Modules.DynamicPackage.Domain;

/// <summary>
/// Strongly typed FlightBooking reference identity for DynamicPackage composition.
/// UUID v7.
/// </summary>
public readonly record struct FlightBookingId(Guid Value)
{
    public static FlightBookingId New() => new(Uuid7.New());

    public static FlightBookingId From(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("FlightBookingId cannot be empty.", nameof(value));
        }

        return new FlightBookingId(value);
    }

    public override string ToString() => Value.ToString("D");
}

