using TravelCore.Identifiers;

namespace TravelCore.Modules.Booking.Domain;

public readonly record struct BookingMonetarySnapshotId(Guid Value)
{
    public static BookingMonetarySnapshotId New() => new(Uuid7.New());

    public static BookingMonetarySnapshotId From(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("BookingMonetarySnapshotId cannot be empty.", nameof(value));
        }

        return new BookingMonetarySnapshotId(value);
    }

    public override string ToString() => Value.ToString("D");
}
