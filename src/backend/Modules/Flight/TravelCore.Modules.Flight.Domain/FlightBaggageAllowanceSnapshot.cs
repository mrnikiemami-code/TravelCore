namespace TravelCore.Modules.Flight.Domain;

/// <summary>
/// Optional simple structured baggage allowance fact. Not an ancillary purchase catalog.
/// </summary>
public sealed class FlightBaggageAllowanceSnapshot
{
    public const int UnitMaxLength = 8;
    public const int CategoryMaxLength = 32;

    private FlightBaggageAllowanceSnapshot()
    {
    }

    internal FlightBaggageAllowanceSnapshot(
        FlightOfferSnapshotId flightOfferSnapshotId,
        int ordinal,
        int? quantity,
        decimal? weight,
        string? unit,
        string? category,
        FlightPassengerCategory? passengerCategory)
    {
        FlightOfferSnapshotId = flightOfferSnapshotId;
        Ordinal = ordinal;
        Quantity = quantity;
        Weight = weight;
        Unit = unit;
        Category = category;
        PassengerCategory = passengerCategory;
    }

    public FlightOfferSnapshotId FlightOfferSnapshotId { get; private set; }

    public int Ordinal { get; private set; }

    public int? Quantity { get; private set; }

    public decimal? Weight { get; private set; }

    public string? Unit { get; private set; }

    public string? Category { get; private set; }

    public FlightPassengerCategory? PassengerCategory { get; private set; }
}
