using TravelCore.Money;
using MoneyValue = TravelCore.Money.Money;

namespace TravelCore.Modules.Flight.Domain;

/// <summary>
/// Optional source-supplied per-passenger-category fare line. Not a tax engine.
/// </summary>
public sealed class FlightPassengerCategoryFareSnapshot
{
    private FlightPassengerCategoryFareSnapshot()
    {
        Amount = null!;
    }

    internal FlightPassengerCategoryFareSnapshot(
        FlightOfferSnapshotId flightOfferSnapshotId,
        int ordinal,
        FlightPassengerCategory category,
        int passengerCount,
        MoneyValue amount)
    {
        FlightOfferSnapshotId = flightOfferSnapshotId;
        Ordinal = ordinal;
        Category = category;
        PassengerCount = passengerCount;
        Amount = amount;
    }

    public FlightOfferSnapshotId FlightOfferSnapshotId { get; private set; }

    public int Ordinal { get; private set; }

    public FlightPassengerCategory Category { get; private set; }

    public int PassengerCount { get; private set; }

    public MoneyValue Amount { get; private set; }
}
