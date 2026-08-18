using NodaTime;
using TravelCore.Money;
using MoneyValue = TravelCore.Money.Money;

namespace TravelCore.Modules.HotelBooking.Domain;

/// <summary>
/// One ordered cancellation penalty window. Instant deadlines. Concrete penalty in booking currency.
/// Zero penalty = free window; full penalty = TotalAmount; partial is a POLICY FACT only.
/// </summary>
public sealed class HotelCancellationPenaltyRule
{
    private HotelCancellationPenaltyRule()
    {
        Penalty = null!;
    }

    internal HotelCancellationPenaltyRule(
        HotelRateOfferSnapshotId hotelRateOfferSnapshotId,
        int ordinal,
        Instant effectiveFrom,
        Instant? effectiveUntil,
        MoneyValue penalty)
    {
        HotelRateOfferSnapshotId = hotelRateOfferSnapshotId;
        Ordinal = ordinal;
        EffectiveFrom = effectiveFrom;
        EffectiveUntil = effectiveUntil;
        Penalty = penalty;
    }

    public HotelRateOfferSnapshotId HotelRateOfferSnapshotId { get; private set; }

    public int Ordinal { get; private set; }

    public Instant EffectiveFrom { get; private set; }

    public Instant? EffectiveUntil { get; private set; }

    public MoneyValue Penalty { get; private set; }
}
