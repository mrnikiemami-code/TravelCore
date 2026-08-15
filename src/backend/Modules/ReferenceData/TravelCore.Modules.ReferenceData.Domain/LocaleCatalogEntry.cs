using System.Text.RegularExpressions;

namespace TravelCore.Modules.ReferenceData.Domain;

/// <summary>
/// Locale / language catalog row (BCP-47-ish tags). Locale ≠ Currency ≠ TimeZone.
/// </summary>
public sealed class LocaleCatalogEntry
{
    private static readonly Regex TagPattern = new(
        @"^[A-Za-z]{2,3}(-[A-Za-z0-9]{2,8})*$",
        RegexOptions.CultureInvariant | RegexOptions.Compiled);

    public const int MaxCodeLength = 32;
    public const int MaxNameLength = 128;

    private LocaleCatalogEntry()
    {
        Code = null!;
        EnglishName = null!;
    }

    private LocaleCatalogEntry(string code, string englishName)
    {
        Code = code;
        EnglishName = englishName;
    }

    public string Code { get; private set; }

    public string EnglishName { get; private set; }

    public static LocaleCatalogEntry Create(string code, string englishName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        var trimmed = code.Trim();
        if (trimmed.Length > MaxCodeLength || !TagPattern.IsMatch(trimmed))
        {
            throw new ArgumentException("Locale code must be a short BCP-47-like tag (e.g. fa, en-US).", nameof(code));
        }

        // Canonicalize language subtag to lowercase; keep region casing as uppercase when present.
        var parts = trimmed.Split('-', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        parts[0] = parts[0].ToLowerInvariant();
        for (var i = 1; i < parts.Length; i++)
        {
            parts[i] = parts[i].Length == 2
                ? parts[i].ToUpperInvariant()
                : parts[i];
        }

        var canonical = string.Join('-', parts);
        ArgumentException.ThrowIfNullOrWhiteSpace(englishName);
        var name = englishName.Trim();
        if (name.Length > MaxNameLength)
        {
            throw new ArgumentException($"Locale name max length is {MaxNameLength}.", nameof(englishName));
        }

        return new LocaleCatalogEntry(canonical, name);
    }
}
