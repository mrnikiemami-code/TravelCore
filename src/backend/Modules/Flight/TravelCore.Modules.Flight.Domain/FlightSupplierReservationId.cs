using TravelCore.Identifiers;

namespace TravelCore.Modules.Flight.Domain;

public readonly record struct FlightSupplierReservationId(Guid Value)
{
    public static FlightSupplierReservationId New() => new(Uuid7.New());

    public static FlightSupplierReservationId From(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("FlightSupplierReservationId cannot be empty.", nameof(value));
        }

        return new FlightSupplierReservationId(value);
    }

    public override string ToString() => Value.ToString("D");
}
