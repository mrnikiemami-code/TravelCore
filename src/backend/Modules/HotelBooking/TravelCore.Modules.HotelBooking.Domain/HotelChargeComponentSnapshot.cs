using TravelCore.Money;
using MoneyValue = TravelCore.Money.Money;

namespace TravelCore.Modules.HotelBooking.Domain;

/// <summary>
/// Source-authored charge disclosure. Opaque Code — not a tax/fee engine and not a giant enum.
/// </summary>
public sealed class HotelChargeComponentSnapshot
{
    public const int CodeMaxLength = 64;

    private HotelChargeComponentSnapshot()
    {
        Code = string.Empty;
        Amount = null!;
    }

    internal HotelChargeComponentSnapshot(
        HotelRateOfferSnapshotId hotelRateOfferSnapshotId,
        int ordinal,
        string code,
        MoneyValue amount)
    {
        HotelRateOfferSnapshotId = hotelRateOfferSnapshotId;
        Ordinal = ordinal;
        Code = code;
        Amount = amount;
    }

    public HotelRateOfferSnapshotId HotelRateOfferSnapshotId { get; private set; }

    public int Ordinal { get; private set; }

    public string Code { get; private set; }

    public MoneyValue Amount { get; private set; }
}
