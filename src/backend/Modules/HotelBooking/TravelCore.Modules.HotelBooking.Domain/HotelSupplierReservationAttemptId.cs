using TravelCore.Identifiers;

namespace TravelCore.Modules.HotelBooking.Domain;

public readonly record struct HotelSupplierReservationAttemptId(Guid Value)
{
    public static HotelSupplierReservationAttemptId New() => new(Uuid7.New());

    public static HotelSupplierReservationAttemptId From(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("HotelSupplierReservationAttemptId cannot be empty.", nameof(value));
        }

        return new HotelSupplierReservationAttemptId(value);
    }

    public override string ToString() => Value.ToString("D");
}
