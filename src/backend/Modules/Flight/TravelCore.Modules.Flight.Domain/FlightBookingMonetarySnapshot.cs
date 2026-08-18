using TravelCore.Money;
using MoneyValue = TravelCore.Money.Money;

namespace TravelCore.Modules.Flight.Domain;

/// <summary>
/// FlightBooking-owned immutable transaction-time monetary truth (TC-P22-T004 / P22-R4).
/// Not a live search price, not Payment, and not an FX conversion.
/// </summary>
public sealed class FlightBookingMonetarySnapshot
{
    private readonly List<FlightPassengerCategoryFareSnapshot> _categoryFares = [];

    private FlightBookingMonetarySnapshot()
    {
        BaseFare = null!;
        Taxes = null!;
        Fees = null!;
        Total = null!;
    }

    internal FlightBookingMonetarySnapshot(
        FlightOfferSnapshotId flightOfferSnapshotId,
        FlightBookingId flightBookingId,
        MoneyValue baseFare,
        MoneyValue taxes,
        MoneyValue fees,
        MoneyValue total)
    {
        FlightOfferSnapshotId = flightOfferSnapshotId;
        FlightBookingId = flightBookingId;
        BaseFare = baseFare;
        Taxes = taxes;
        Fees = fees;
        Total = total;
    }

    public FlightOfferSnapshotId FlightOfferSnapshotId { get; private set; }

    public FlightBookingId FlightBookingId { get; private set; }

    public MoneyValue BaseFare { get; private set; }

    public MoneyValue Taxes { get; private set; }

    public MoneyValue Fees { get; private set; }

    public MoneyValue Total { get; private set; }

    public CurrencyCode CurrencyCode => Total.Currency;

    public IReadOnlyList<FlightPassengerCategoryFareSnapshot> CategoryFares => _categoryFares;

    internal void AddCategoryFare(FlightPassengerCategoryFareSnapshot fare)
    {
        ArgumentNullException.ThrowIfNull(fare);
        _categoryFares.Add(fare);
    }
}
