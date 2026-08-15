namespace TravelCore.Modules.ReferenceData.Domain;

/// <summary>
/// ISO 3166-1 country reference. Not a Destination discovery node.
/// </summary>
public sealed class CountryCatalogEntry
{
    public const int MaxNameLength = 128;

    private CountryCatalogEntry()
    {
        Alpha2Code = null!;
        Alpha3Code = null!;
        EnglishName = null!;
    }

    private CountryCatalogEntry(string alpha2, string alpha3, string? numericCode, string englishName)
    {
        Alpha2Code = alpha2;
        Alpha3Code = alpha3;
        NumericCode = numericCode;
        EnglishName = englishName;
    }

    public string Alpha2Code { get; private set; }

    public string Alpha3Code { get; private set; }

    public string? NumericCode { get; private set; }

    public string EnglishName { get; private set; }

    public static CountryCatalogEntry Create(string alpha2, string alpha3, string englishName, string? numericCode = null)
    {
        var a2 = NormalizeAlpha(alpha2, 2, nameof(alpha2));
        var a3 = NormalizeAlpha(alpha3, 3, nameof(alpha3));
        ArgumentException.ThrowIfNullOrWhiteSpace(englishName);
        var name = englishName.Trim();
        if (name.Length > MaxNameLength)
        {
            throw new ArgumentException($"Country name max length is {MaxNameLength}.", nameof(englishName));
        }

        string? numeric = null;
        if (!string.IsNullOrWhiteSpace(numericCode))
        {
            numeric = numericCode.Trim();
            if (numeric.Length is < 1 or > 3 || !numeric.All(char.IsDigit))
            {
                throw new ArgumentException("ISO numeric country code must be 1–3 digits.", nameof(numericCode));
            }
        }

        return new CountryCatalogEntry(a2, a3, numeric, name);
    }

    private static string NormalizeAlpha(string value, int expectedLength, string paramName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        var trimmed = value.Trim().ToUpperInvariant();
        if (trimmed.Length != expectedLength || !trimmed.All(static c => c is >= 'A' and <= 'Z'))
        {
            throw new ArgumentException($"Expected {expectedLength} ASCII letters A–Z.", paramName);
        }

        return trimmed;
    }
}
