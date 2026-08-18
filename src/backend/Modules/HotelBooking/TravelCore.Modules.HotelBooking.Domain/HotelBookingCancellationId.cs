using TravelCore.Identifiers;

namespace TravelCore.Modules.HotelBooking.Domain;

/// <summary>
/// Strongly typed HotelBookingCancellation identity (UUID v7).
/// </summary>
public readonly record struct HotelBookingCancellationId(Guid Value)
{
    public static HotelBookingCancellationId New() => new(Uuid7.New());

    public static HotelBookingCancellationId From(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("HotelBookingCancellationId cannot be empty.", nameof(value));
        }

        return new HotelBookingCancellationId(value);
    }

    public override string ToString() => Value.ToString("D");
}
