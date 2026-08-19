using TravelCore.Modules.Flight.Contracts;

namespace TravelCore.Modules.Flight.Infrastructure.Ticketing;

internal sealed class FlightTicketingSourceResolver : IFlightTicketingSourceResolver
{
    private readonly IReadOnlyDictionary<string, IFlightTicketingSource> _sources;

    public FlightTicketingSourceResolver(IEnumerable<IFlightTicketingSource> sources)
    {
        ArgumentNullException.ThrowIfNull(sources);
        _sources = sources.ToDictionary(source => source.Key.Value, StringComparer.Ordinal);
    }

    public IFlightTicketingSource? Resolve(FlightSourceKey sourceKey) =>
        _sources.TryGetValue(sourceKey.Value, out var source) ? source : null;

    public IReadOnlyList<FlightSourceKey> ListConfiguredKeys() =>
        _sources.Keys.Select(key => new FlightSourceKey(key)).ToArray();
}
