namespace TravelCore.Modules.Place.Domain;

/// <summary>
/// Place-owned facility code row (minimal typed list — not a generic taxonomy engine).
/// Opaque catalog codes only; not live inventory / room amenity booking facts.
/// </summary>
public sealed class PlaceFacility
{
    public const int CodeMaxLength = 64;

    private PlaceFacility()
    {
        Code = null!;
    }

    private PlaceFacility(PlaceId placeId, string code)
    {
        PlaceId = placeId;
        Code = code;
    }

    public PlaceId PlaceId { get; private set; }

    /// <summary>Opaque facility code (e.g. wifi, parking). Place-owned; not ReferenceData SoR.</summary>
    public string Code { get; private set; }

    internal static PlaceFacility Create(PlaceId placeId, string code)
    {
        if (placeId.Value == Guid.Empty)
        {
            throw new ArgumentException("PlaceId cannot be empty.", nameof(placeId));
        }

        return new PlaceFacility(placeId, NormalizeCode(code));
    }

    public static string NormalizeCode(string code)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        var trimmed = code.Trim().ToLowerInvariant();
        if (trimmed.Length > CodeMaxLength)
        {
            throw new ArgumentException($"Facility code max length is {CodeMaxLength}.", nameof(code));
        }

        // Opaque segment: lowercase letters/digits/hyphen/underscore only.
        if (trimmed.Any(static c => !(char.IsAsciiLetterOrDigit(c) || c is '-' or '_')))
        {
            throw new ArgumentException(
                "Facility code may contain only a-z, 0-9, hyphen, and underscore.",
                nameof(code));
        }

        return trimmed;
    }
}
