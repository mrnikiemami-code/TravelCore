using TravelCore.Identifiers;

namespace TravelCore.Modules.Flight.Domain;

public readonly record struct FlightOfferSnapshotId(Guid Value)
{
    public static FlightOfferSnapshotId New() => new(Uuid7.New());

    public static FlightOfferSnapshotId From(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("FlightOfferSnapshotId cannot be empty.", nameof(value));
        }

        return new FlightOfferSnapshotId(value);
    }

    public override string ToString() => Value.ToString("D");
}
