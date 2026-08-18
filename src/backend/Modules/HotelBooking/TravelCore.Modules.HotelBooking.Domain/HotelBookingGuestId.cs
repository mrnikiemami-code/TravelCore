using TravelCore.Identifiers;

namespace TravelCore.Modules.HotelBooking.Domain;

public readonly record struct HotelBookingGuestId(Guid Value)
{
    public static HotelBookingGuestId New() => new(Uuid7.New());

    public static HotelBookingGuestId From(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("HotelBookingGuestId cannot be empty.", nameof(value));
        }

        return new HotelBookingGuestId(value);
    }

    public override string ToString() => Value.ToString("D");
}
