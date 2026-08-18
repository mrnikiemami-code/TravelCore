using TravelCore.Modules.HotelBooking.Contracts;

namespace TravelCore.Modules.HotelBooking.Infrastructure.Services;

/// <summary>
/// Explicit capability catalog. Capabilities are declared by registrants, never inferred from SourceKey.
/// Zero production sources is a valid empty catalog.
/// </summary>
internal sealed class HotelSourceCatalog : IHotelSourceCatalog
{
    private readonly IReadOnlyDictionary<string, HotelSourceDescriptor> _descriptors;

    public HotelSourceCatalog(IEnumerable<IDeclaredHotelSourceCapabilities> declarations)
    {
        ArgumentNullException.ThrowIfNull(declarations);
        var map = new Dictionary<string, HotelSourceDescriptor>(StringComparer.Ordinal);
        foreach (var declaration in declarations)
        {
            ArgumentNullException.ThrowIfNull(declaration);
            if (string.IsNullOrWhiteSpace(declaration.SourceKey))
            {
                throw new ArgumentException("SourceKey is required.", nameof(declarations));
            }

            if (map.ContainsKey(declaration.SourceKey))
            {
                throw new InvalidOperationException("Duplicate SourceKey is not allowed.");
            }

            map[declaration.SourceKey] = new HotelSourceDescriptor(
                declaration.SourceKey,
                declaration.Enabled,
                declaration.Capabilities.ToArray(),
                declaration.DisplayName);
        }

        _descriptors = map;
    }

    public IReadOnlyList<HotelSourceDescriptor> List() => _descriptors.Values.ToArray();

    public HotelSourceDescriptor? Find(string sourceKey) =>
        _descriptors.TryGetValue(sourceKey, out var descriptor) ? descriptor : null;
}
