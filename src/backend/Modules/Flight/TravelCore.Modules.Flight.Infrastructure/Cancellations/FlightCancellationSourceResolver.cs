using TravelCore.Modules.Flight.Contracts;

namespace TravelCore.Modules.Flight.Infrastructure.Cancellations;

internal sealed class FlightCancellationSourceResolver : IFlightCancellationSourceResolver
{
    private readonly IReadOnlyDictionary<string, IFlightCancellationSource> _sources;

    public FlightCancellationSourceResolver(IEnumerable<IFlightCancellationSource> sources)
    {
        ArgumentNullException.ThrowIfNull(sources);
        _sources = sources.ToDictionary(source => source.Key.Value, StringComparer.Ordinal);
    }

    public IFlightCancellationSource? Resolve(FlightSourceKey sourceKey) =>
        _sources.TryGetValue(sourceKey.Value, out var source) ? source : null;

    public IReadOnlyList<FlightSourceKey> ListConfiguredKeys() =>
        _sources.Keys.Select(key => new FlightSourceKey(key)).ToArray();
}
