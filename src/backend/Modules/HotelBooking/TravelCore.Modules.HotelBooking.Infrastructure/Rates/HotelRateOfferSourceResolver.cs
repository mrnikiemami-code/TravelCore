using TravelCore.Modules.HotelBooking.Contracts;

namespace TravelCore.Modules.HotelBooking.Infrastructure.Rates;

internal sealed class HotelRateOfferSourceResolver : IHotelRateOfferSourceResolver
{
    private readonly IReadOnlyDictionary<string, IHotelRateOfferSource> _sources;

    public HotelRateOfferSourceResolver(IEnumerable<IHotelRateOfferSource> sources)
    {
        ArgumentNullException.ThrowIfNull(sources);
        _sources = sources.ToDictionary(source => source.Key.Value, StringComparer.Ordinal);
    }

    public IHotelRateOfferSource? Resolve(RateSourceKey sourceKey) =>
        _sources.TryGetValue(sourceKey.Value, out var source) ? source : null;

    public IReadOnlyList<RateSourceKey> ListConfiguredKeys() =>
        _sources.Keys.Select(key => new RateSourceKey(key)).ToArray();
}
