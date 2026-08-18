using TravelCore.Modules.Flight.Contracts;

namespace TravelCore.Modules.Flight.Infrastructure.Search;

internal sealed class FlightOfferAvailabilitySourceResolver : IFlightOfferAvailabilitySourceResolver
{
    private readonly IReadOnlyDictionary<string, IFlightOfferAvailabilitySource> _sources;

    public FlightOfferAvailabilitySourceResolver(IEnumerable<IFlightOfferAvailabilitySource> sources)
    {
        ArgumentNullException.ThrowIfNull(sources);
        _sources = sources.ToDictionary(source => source.Key.Value, StringComparer.Ordinal);
    }

    public IFlightOfferAvailabilitySource? Resolve(FlightSourceKey sourceKey) =>
        _sources.TryGetValue(sourceKey.Value, out var source) ? source : null;

    public IReadOnlyList<FlightSourceKey> ListConfiguredKeys() =>
        _sources.Keys.Select(key => new FlightSourceKey(key)).ToArray();
}
