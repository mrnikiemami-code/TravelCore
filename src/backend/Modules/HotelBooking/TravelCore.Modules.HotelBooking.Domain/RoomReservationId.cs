using TravelCore.Identifiers;

namespace TravelCore.Modules.HotelBooking.Domain;

public readonly record struct RoomReservationId(Guid Value)
{
    public static RoomReservationId New() => new(Uuid7.New());

    public static RoomReservationId From(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("RoomReservationId cannot be empty.", nameof(value));
        }

        return new RoomReservationId(value);
    }

    public override string ToString() => Value.ToString("D");
}
