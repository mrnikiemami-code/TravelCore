using TravelCore.Identifiers;

namespace TravelCore.Modules.HotelBooking.Domain;

/// <summary>
/// Strongly typed HotelBooking identity (UUID v7).
/// </summary>
public readonly record struct HotelBookingId(Guid Value)
{
    public static HotelBookingId New() => new(Uuid7.New());

    public static HotelBookingId From(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("HotelBookingId cannot be empty.", nameof(value));
        }

        return new HotelBookingId(value);
    }

    public override string ToString() => Value.ToString("D");
}
