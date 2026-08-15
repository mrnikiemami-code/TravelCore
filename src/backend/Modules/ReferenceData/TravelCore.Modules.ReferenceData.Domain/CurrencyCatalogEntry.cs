using TravelCore.Money;

namespace TravelCore.Modules.ReferenceData.Domain;

/// <summary>
/// Stable currency catalog row. Aligns with Platform <see cref="CurrencyCode"/>; does not own Money.
/// </summary>
public sealed class CurrencyCatalogEntry
{
    public const int MaxNameLength = 128;
    public const int MaxSymbolLength = 16;

    private CurrencyCatalogEntry()
    {
        Code = null!;
        EnglishName = null!;
    }

    private CurrencyCatalogEntry(string code, string englishName, int minorUnits, string? symbol)
    {
        Code = code;
        EnglishName = englishName;
        MinorUnits = minorUnits;
        Symbol = symbol;
    }

    public string Code { get; private set; }

    public string EnglishName { get; private set; }

    public int MinorUnits { get; private set; }

    public string? Symbol { get; private set; }

    public static CurrencyCatalogEntry Create(string code, string englishName, int minorUnits, string? symbol = null)
    {
        var parsed = CurrencyCode.Parse(code);
        var name = NormalizeName(englishName);
        if (minorUnits is < 0 or > 6)
        {
            throw new ArgumentOutOfRangeException(nameof(minorUnits), "Minor units must be between 0 and 6.");
        }

        string? normalizedSymbol = null;
        if (!string.IsNullOrWhiteSpace(symbol))
        {
            normalizedSymbol = symbol.Trim();
            if (normalizedSymbol.Length > MaxSymbolLength)
            {
                throw new ArgumentException($"Currency symbol max length is {MaxSymbolLength}.", nameof(symbol));
            }
        }

        return new CurrencyCatalogEntry(parsed.Value, name, minorUnits, normalizedSymbol);
    }

    private static string NormalizeName(string englishName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(englishName);
        var trimmed = englishName.Trim();
        if (trimmed.Length > MaxNameLength)
        {
            throw new ArgumentException($"Currency name max length is {MaxNameLength}.", nameof(englishName));
        }

        return trimmed;
    }
}
