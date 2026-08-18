using TravelCore.Money;
using MoneyValue = TravelCore.Money.Money;

namespace TravelCore.Modules.HotelBooking.Domain;

/// <summary>
/// HotelBooking-owned immutable transaction-time monetary truth (TC-P21-T004 / P21-R4).
/// Not a live rate, not Payment, and not an FX conversion.
/// </summary>
public sealed class HotelBookingMonetarySnapshot
{
    private readonly List<HotelChargeComponentSnapshot> _charges = [];

    private HotelBookingMonetarySnapshot()
    {
        Total = null!;
    }

    internal HotelBookingMonetarySnapshot(
        HotelRateOfferSnapshotId hotelRateOfferSnapshotId,
        HotelBookingId hotelBookingId,
        MoneyValue total,
        MoneyValue? payableNow,
        MoneyValue? payableAtProperty)
    {
        HotelRateOfferSnapshotId = hotelRateOfferSnapshotId;
        HotelBookingId = hotelBookingId;
        Total = total;
        PayableNow = payableNow;
        PayableAtProperty = payableAtProperty;
    }

    public HotelRateOfferSnapshotId HotelRateOfferSnapshotId { get; private set; }

    public HotelBookingId HotelBookingId { get; private set; }

    public MoneyValue Total { get; private set; }

    public CurrencyCode CurrencyCode => Total.Currency;

    public MoneyValue? PayableNow { get; private set; }

    public MoneyValue? PayableAtProperty { get; private set; }

    public IReadOnlyList<HotelChargeComponentSnapshot> Charges => _charges;

    internal void AddCharge(HotelChargeComponentSnapshot charge)
    {
        ArgumentNullException.ThrowIfNull(charge);
        _charges.Add(charge);
    }
}
