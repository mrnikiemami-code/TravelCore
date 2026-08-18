using TravelCore.Modules.Flight.Contracts;

namespace TravelCore.Modules.Flight.Infrastructure.Search;

internal sealed class FlightOfferSourceResolver : IFlightOfferSourceResolver
{
    private readonly IReadOnlyDictionary<string, IFlightOfferSource> _sources;

    public FlightOfferSourceResolver(IEnumerable<IFlightOfferSource> sources)
    {
        ArgumentNullException.ThrowIfNull(sources);
        _sources = sources.ToDictionary(source => source.Key.Value, StringComparer.Ordinal);
    }

    public IFlightOfferSource? Resolve(FlightSourceKey sourceKey) =>
        _sources.TryGetValue(sourceKey.Value, out var source) ? source : null;

    public IReadOnlyList<FlightSourceKey> ListConfiguredKeys() =>
        _sources.Keys.Select(key => new FlightSourceKey(key)).ToArray();
}
