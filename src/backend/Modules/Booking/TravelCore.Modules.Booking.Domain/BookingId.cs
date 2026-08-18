using TravelCore.Identifiers;

namespace TravelCore.Modules.Booking.Domain;

/// <summary>
/// Strongly typed Booking identity (UUID v7). Not a human booking reference (P19-R2).
/// </summary>
public readonly record struct BookingId(Guid Value)
{
    public static BookingId New() => new(Uuid7.New());

    public static BookingId From(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("BookingId cannot be empty.", nameof(value));
        }

        return new BookingId(value);
    }

    public override string ToString() => Value.ToString("D");
}
