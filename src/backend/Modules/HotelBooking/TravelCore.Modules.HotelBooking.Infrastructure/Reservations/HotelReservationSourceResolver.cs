using TravelCore.Modules.HotelBooking.Contracts;

namespace TravelCore.Modules.HotelBooking.Infrastructure.Reservations;

internal sealed class HotelReservationSourceResolver : IHotelReservationSourceResolver
{
    private readonly IReadOnlyDictionary<string, IHotelReservationSource> _sources;

    public HotelReservationSourceResolver(IEnumerable<IHotelReservationSource> sources)
    {
        ArgumentNullException.ThrowIfNull(sources);
        _sources = sources.ToDictionary(source => source.Key.Value, StringComparer.Ordinal);
    }

    public IHotelReservationSource? Resolve(ReservationSourceKey sourceKey) =>
        _sources.TryGetValue(sourceKey.Value, out var source) ? source : null;

    public IReadOnlyList<ReservationSourceKey> ListConfiguredKeys() =>
        _sources.Keys.Select(key => new ReservationSourceKey(key)).ToArray();
}
