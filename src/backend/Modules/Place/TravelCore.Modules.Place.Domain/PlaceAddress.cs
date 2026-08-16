namespace TravelCore.Modules.Place.Domain;

/// <summary>
/// Place-owned postal/street address baseline (not Destination hierarchy; not SEO/slug source).
/// CountryCode is an opaque ISO alpha-2 hint — not a cross-schema FK to ReferenceData.
/// </summary>
public sealed class PlaceAddress
{
    public const int LineMaxLength = 200;
    public const int LocalityMaxLength = 100;
    public const int AdministrativeAreaMaxLength = 100;
    public const int PostalCodeMaxLength = 32;
    public const int CountryCodeMaxLength = 2;

    private PlaceAddress()
    {
    }

    private PlaceAddress(
        string? line1,
        string? line2,
        string? locality,
        string? administrativeArea,
        string? postalCode,
        string? countryCode)
    {
        Line1 = line1;
        Line2 = line2;
        Locality = locality;
        AdministrativeArea = administrativeArea;
        PostalCode = postalCode;
        CountryCode = countryCode;
    }

    public string? Line1 { get; private set; }

    public string? Line2 { get; private set; }

    public string? Locality { get; private set; }

    public string? AdministrativeArea { get; private set; }

    public string? PostalCode { get; private set; }

    /// <summary>Optional ISO 3166-1 alpha-2 country code (opaque string; not DestinationId).</summary>
    public string? CountryCode { get; private set; }

    /// <summary>
    /// Builds an address when at least one field is present; returns null when all cleared.
    /// </summary>
    public static PlaceAddress? Create(
        string? line1,
        string? line2,
        string? locality,
        string? administrativeArea,
        string? postalCode,
        string? countryCode)
    {
        var normalized = new PlaceAddress(
            NormalizeOptional(line1, LineMaxLength, nameof(line1)),
            NormalizeOptional(line2, LineMaxLength, nameof(line2)),
            NormalizeOptional(locality, LocalityMaxLength, nameof(locality)),
            NormalizeOptional(administrativeArea, AdministrativeAreaMaxLength, nameof(administrativeArea)),
            NormalizeOptional(postalCode, PostalCodeMaxLength, nameof(postalCode)),
            NormalizeCountryCode(countryCode));

        if (normalized.IsEmpty)
        {
            return null;
        }

        return normalized;
    }

    public bool IsEmpty =>
        Line1 is null
        && Line2 is null
        && Locality is null
        && AdministrativeArea is null
        && PostalCode is null
        && CountryCode is null;

    private static string? NormalizeOptional(string? value, int maxLength, string paramName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var trimmed = value.Trim();
        if (trimmed.Length > maxLength)
        {
            throw new ArgumentException($"{paramName} max length is {maxLength}.", paramName);
        }

        return trimmed;
    }

    private static string? NormalizeCountryCode(string? countryCode)
    {
        if (string.IsNullOrWhiteSpace(countryCode))
        {
            return null;
        }

        var trimmed = countryCode.Trim().ToUpperInvariant();
        if (trimmed.Length != CountryCodeMaxLength
            || !trimmed.All(static c => c is >= 'A' and <= 'Z'))
        {
            throw new ArgumentException(
                "CountryCode must be ISO 3166-1 alpha-2 (two letters).",
                nameof(countryCode));
        }

        return trimmed;
    }
}
