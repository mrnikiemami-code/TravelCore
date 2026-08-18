using TravelCore.Modules.Flight.Contracts;

namespace TravelCore.Modules.Flight.Infrastructure.Search;

internal sealed class FlightSearchSourceResolver : IFlightSearchSourceResolver
{
    private readonly IReadOnlyDictionary<string, IFlightSearchSource> _sources;

    public FlightSearchSourceResolver(IEnumerable<IFlightSearchSource> sources)
    {
        ArgumentNullException.ThrowIfNull(sources);
        _sources = sources.ToDictionary(source => source.Key.Value, StringComparer.Ordinal);
    }

    public IFlightSearchSource? Resolve(FlightSourceKey sourceKey) =>
        _sources.TryGetValue(sourceKey.Value, out var source) ? source : null;

    public IReadOnlyList<FlightSourceKey> ListConfiguredKeys() =>
        _sources.Keys.Select(key => new FlightSourceKey(key)).ToArray();
}
