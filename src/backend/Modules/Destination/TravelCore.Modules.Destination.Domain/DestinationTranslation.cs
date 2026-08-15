using NodaTime;

namespace TravelCore.Modules.Destination.Domain;

/// <summary>
/// Locale-specific name/description for a Destination. Same DestinationId across locales.
/// Locale codes are ReferenceData-owned; Destination stores the opaque code only (no cross-schema FK).
/// </summary>
public sealed class DestinationTranslation
{
    public const int LocaleCodeMaxLength = 16;
    public const int NameMaxLength = 200;
    public const int DescriptionMaxLength = 2000;

    private DestinationTranslation()
    {
        LocaleCode = null!;
        Name = null!;
    }

    private DestinationTranslation(
        DestinationId destinationId,
        string localeCode,
        string name,
        string? description,
        Instant updatedAt)
    {
        DestinationId = destinationId;
        LocaleCode = localeCode;
        Name = name;
        Description = description;
        UpdatedAt = updatedAt;
    }

    public DestinationId DestinationId { get; private set; }

    public string LocaleCode { get; private set; }

    public string Name { get; private set; }

    public string? Description { get; private set; }

    public Instant UpdatedAt { get; private set; }

    internal static DestinationTranslation Create(
        DestinationId destinationId,
        string localeCode,
        string name,
        string? description,
        Instant now)
    {
        return new DestinationTranslation(
            destinationId,
            NormalizeLocaleCode(localeCode),
            NormalizeName(name),
            NormalizeDescription(description),
            now);
    }

    internal void Update(string name, string? description, Instant now)
    {
        Name = NormalizeName(name);
        Description = NormalizeDescription(description);
        UpdatedAt = now;
    }

    public static string NormalizeLocaleCode(string localeCode)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(localeCode);
        var trimmed = localeCode.Trim();
        if (trimmed.Length > LocaleCodeMaxLength)
        {
            throw new ArgumentException($"Locale code max length is {LocaleCodeMaxLength}.", nameof(localeCode));
        }

        // Preserve BCP-47 casing shape: language lower, region upper when present (fa, en-US).
        var parts = trimmed.Split('-', 2, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (parts.Length == 1)
        {
            return parts[0].ToLowerInvariant();
        }

        return $"{parts[0].ToLowerInvariant()}-{parts[1].ToUpperInvariant()}";
    }

    private static string NormalizeName(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        var trimmed = name.Trim();
        if (trimmed.Length > NameMaxLength)
        {
            throw new ArgumentException($"Translation name max length is {NameMaxLength}.", nameof(name));
        }

        return trimmed;
    }

    private static string? NormalizeDescription(string? description)
    {
        if (string.IsNullOrWhiteSpace(description))
        {
            return null;
        }

        var trimmed = description.Trim();
        if (trimmed.Length > DescriptionMaxLength)
        {
            throw new ArgumentException($"Translation description max length is {DescriptionMaxLength}.", nameof(description));
        }

        return trimmed;
    }
}
