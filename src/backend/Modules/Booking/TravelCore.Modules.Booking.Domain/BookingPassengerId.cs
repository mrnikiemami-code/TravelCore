using TravelCore.Identifiers;

namespace TravelCore.Modules.Booking.Domain;

public readonly record struct BookingPassengerId(Guid Value)
{
    public static BookingPassengerId New() => new(Uuid7.New());

    public static BookingPassengerId From(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("BookingPassengerId cannot be empty.", nameof(value));
        }

        return new BookingPassengerId(value);
    }

    public override string ToString() => Value.ToString("D");
}
