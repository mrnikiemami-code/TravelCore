using TravelCore.Identifiers;

namespace TravelCore.Modules.Flight.Domain;

public readonly record struct FlightSupplierReservationAttemptId(Guid Value)
{
    public static FlightSupplierReservationAttemptId New() => new(Uuid7.New());

    public static FlightSupplierReservationAttemptId From(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("FlightSupplierReservationAttemptId cannot be empty.", nameof(value));
        }

        return new FlightSupplierReservationAttemptId(value);
    }

    public override string ToString() => Value.ToString("D");
}
