namespace TravelCore.Modules.HotelBooking.Contracts;

/// <summary>
/// Provider-neutral authoritative hotel commercial-rate port. Named production source remains NONE.
/// Rate source responsibility is conceptually distinct from availability source responsibility.
/// </summary>
public interface IHotelRateOfferSource
{
    RateSourceKey Key { get; }

    Task<HotelRateOfferSourceResult> GetOfferAsync(
        HotelRateOfferRequest request,
        CancellationToken cancellationToken = default);
}
