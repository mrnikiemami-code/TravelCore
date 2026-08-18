using TravelCore.Modules.HotelBooking.Contracts;

namespace TravelCore.Modules.HotelBooking.Infrastructure.Availability;

internal sealed class HotelAvailabilitySourceResolver : IHotelAvailabilitySourceResolver
{
    private readonly IReadOnlyDictionary<string, IHotelAvailabilitySource> _sources;

    public HotelAvailabilitySourceResolver(IEnumerable<IHotelAvailabilitySource> sources)
    {
        ArgumentNullException.ThrowIfNull(sources);
        _sources = sources.ToDictionary(source => source.Key.Value, StringComparer.Ordinal);
    }

    public IHotelAvailabilitySource? Resolve(AvailabilitySourceKey sourceKey) =>
        _sources.TryGetValue(sourceKey.Value, out var source) ? source : null;

    public IReadOnlyList<AvailabilitySourceKey> ListConfiguredKeys() =>
        _sources.Keys.Select(key => new AvailabilitySourceKey(key)).ToArray();
}
