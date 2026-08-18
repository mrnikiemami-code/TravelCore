namespace TravelCore.Modules.HotelBooking.Contracts;

/// <summary>
/// Server-controlled rate source resolution. No ranking, failover, or user-selected adapter.
/// </summary>
public interface IHotelRateOfferSourceResolver
{
    IHotelRateOfferSource? Resolve(RateSourceKey sourceKey);

    IReadOnlyList<RateSourceKey> ListConfiguredKeys();
}
