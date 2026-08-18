using TravelCore.Modules.Flight.Contracts;

namespace TravelCore.Modules.Flight.Infrastructure.Reservations;

internal sealed class FlightReservationSourceResolver : IFlightReservationSourceResolver
{
    private readonly IReadOnlyDictionary<string, IFlightReservationSource> _sources;

    public FlightReservationSourceResolver(IEnumerable<IFlightReservationSource> sources)
    {
        ArgumentNullException.ThrowIfNull(sources);
        _sources = sources.ToDictionary(source => source.Key.Value, StringComparer.Ordinal);
    }

    public IFlightReservationSource? Resolve(FlightSourceKey sourceKey) =>
        _sources.TryGetValue(sourceKey.Value, out var source) ? source : null;

    public IReadOnlyList<FlightSourceKey> ListConfiguredKeys() =>
        _sources.Keys.Select(key => new FlightSourceKey(key)).ToArray();
}
