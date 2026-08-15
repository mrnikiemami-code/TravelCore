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
    public const int SlugMaxLength = 120;

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
        string? slug,
        Instant updatedAt)
    {
        DestinationId = destinationId;
        LocaleCode = localeCode;
        Name = name;
        Description = description;
        Slug = slug;
        UpdatedAt = updatedAt;
    }

    public DestinationId DestinationId { get; private set; }

    public string LocaleCode { get; private set; }

    public string Name { get; private set; }

    public string? Description { get; private set; }

    /// <summary>
    /// Optional localized slug hook for future P05 SeoRoute binding. Not an SEO engine.
    /// </summary>
    public string? Slug { get; private set; }

    public Instant UpdatedAt { get; private set; }

    internal static DestinationTranslation Create(
        DestinationId destinationId,
        string localeCode,
        string name,
        string? description,
        Instant now,
        string? slug = null)
    {
        return new DestinationTranslation(
            destinationId,
            NormalizeLocaleCode(localeCode),
            NormalizeName(name),
            NormalizeDescription(description),
            NormalizeSlug(slug),
            now);
    }

    internal void Update(string name, string? description, Instant now)
    {
        Name = NormalizeName(name);
        Description = NormalizeDescription(description);
        UpdatedAt = now;
    }

    internal void SetSlug(string? slug, Instant now)
    {
        Slug = NormalizeSlug(slug);
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

    public static string? NormalizeSlug(string? slug)
    {
        if (string.IsNullOrWhiteSpace(slug))
        {
            return null;
        }

        var trimmed = slug.Trim().ToLowerInvariant();
        if (trimmed.Length > DestinationTranslation.SlugMaxLength)
        {
            throw new ArgumentException($"Slug max length is {SlugMaxLength}.", nameof(slug));
        }

        // Opaque URL segment hook: lowercase letters/digits/hyphen only (no SEO engine).
        if (trimmed.Any(static c => !(char.IsAsciiLetterOrDigit(c) || c == '-')))
        {
            throw new ArgumentException("Slug may contain only a-z, 0-9, and hyphen.", nameof(slug));
        }

        if (trimmed.StartsWith('-') || trimmed.EndsWith('-') || trimmed.Contains("--", StringComparison.Ordinal))
        {
            throw new ArgumentException("Slug must not start/end with hyphen or contain consecutive hyphens.", nameof(slug));
        }

        return trimmed;
    }
}
