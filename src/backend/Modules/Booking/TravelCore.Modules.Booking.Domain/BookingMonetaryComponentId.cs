using TravelCore.Identifiers;

namespace TravelCore.Modules.Booking.Domain;

public readonly record struct BookingMonetaryComponentId(Guid Value)
{
    public static BookingMonetaryComponentId New() => new(Uuid7.New());

    public static BookingMonetaryComponentId From(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("BookingMonetaryComponentId cannot be empty.", nameof(value));
        }

        return new BookingMonetaryComponentId(value);
    }

    public override string ToString() => Value.ToString("D");
}
