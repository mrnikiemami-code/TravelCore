using TravelCore.Identifiers;

namespace TravelCore.Modules.HotelBooking.Domain;

public readonly record struct HotelAvailabilityHoldId(Guid Value)
{
    public static HotelAvailabilityHoldId New() => new(Uuid7.New());

    public static HotelAvailabilityHoldId From(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("HotelAvailabilityHoldId cannot be empty.", nameof(value));
        }

        return new HotelAvailabilityHoldId(value);
    }

    public override string ToString() => Value.ToString("D");
}
