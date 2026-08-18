using TravelCore.Identifiers;

namespace TravelCore.Modules.HotelBooking.Domain;

public readonly record struct HotelSupplierReservationId(Guid Value)
{
    public static HotelSupplierReservationId New() => new(Uuid7.New());

    public static HotelSupplierReservationId From(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("HotelSupplierReservationId cannot be empty.", nameof(value));
        }

        return new HotelSupplierReservationId(value);
    }

    public override string ToString() => Value.ToString("D");
}
