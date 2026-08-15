using NodaTime;

namespace TravelCore.Modules.ReferenceData.Domain;

/// <summary>
/// IANA time-zone catalog row. TimeZone ≠ Locale ≠ Currency.
/// </summary>
public sealed class TimeZoneCatalogEntry
{
    public const int MaxIdLength = 64;
    public const int MaxNameLength = 128;

    private TimeZoneCatalogEntry()
    {
        Id = null!;
        EnglishName = null!;
    }

    private TimeZoneCatalogEntry(string id, string englishName)
    {
        Id = id;
        EnglishName = englishName;
    }

    public string Id { get; private set; }

    public string EnglishName { get; private set; }

    public static TimeZoneCatalogEntry Create(string id, string englishName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        var trimmed = id.Trim();
        if (trimmed.Length > MaxIdLength)
        {
            throw new ArgumentException($"Time zone id max length is {MaxIdLength}.", nameof(id));
        }

        var zone = DateTimeZoneProviders.Tzdb.GetZoneOrNull(trimmed)
            ?? throw new ArgumentException($"Unknown IANA time zone id '{trimmed}'.", nameof(id));

        ArgumentException.ThrowIfNullOrWhiteSpace(englishName);
        var name = englishName.Trim();
        if (name.Length > MaxNameLength)
        {
            throw new ArgumentException($"Time zone name max length is {MaxNameLength}.", nameof(englishName));
        }

        return new TimeZoneCatalogEntry(zone.Id, name);
    }
}
