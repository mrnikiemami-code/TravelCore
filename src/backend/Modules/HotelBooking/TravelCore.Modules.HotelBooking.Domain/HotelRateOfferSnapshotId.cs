using TravelCore.Identifiers;

namespace TravelCore.Modules.HotelBooking.Domain;

public readonly record struct HotelRateOfferSnapshotId(Guid Value)
{
    public static HotelRateOfferSnapshotId New() => new(Uuid7.New());

    public static HotelRateOfferSnapshotId From(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("HotelRateOfferSnapshotId cannot be empty.", nameof(value));
        }

        return new HotelRateOfferSnapshotId(value);
    }

    public override string ToString() => Value.ToString("D");
}
