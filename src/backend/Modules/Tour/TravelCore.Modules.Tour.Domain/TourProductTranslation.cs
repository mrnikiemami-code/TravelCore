using NodaTime;

namespace TravelCore.Modules.Tour.Domain;

/// <summary>
/// Locale-specific title/description/slug for a TourProduct (ADR 0008 / P09-R5).
/// Locale rows only — never per-language columns on the aggregate.
/// Current localized slug is owned here; SEO owns route history/redirects/IndexPolicy.
/// </summary>
public sealed class TourProductTranslation
{
    public const int LocaleCodeMaxLength = 16;
    public const int TitleMaxLength = 200;
    public const int DescriptionMaxLength = 2000;
    public const int SlugMaxLength = 120;

    private TourProductTranslation()
    {
        LocaleCode = null!;
        Title = null!;
    }

    private TourProductTranslation(
        TourProductId tourProductId,
        string localeCode,
        string title,
        string? description,
        string? slug,
        Instant updatedAt)
    {
        TourProductId = tourProductId;
        LocaleCode = localeCode;
        Title = title;
        Description = description;
        Slug = slug;
        UpdatedAt = updatedAt;
    }

    public TourProductId TourProductId { get; private set; }

    public string LocaleCode { get; private set; }

    public string Title { get; private set; }

    public string? Description { get; private set; }

    /// <summary>Localized current slug (P09-R5). Null until set; SEO owns history/redirects.</summary>
    public string? Slug { get; private set; }

    public Instant UpdatedAt { get; private set; }

    internal static TourProductTranslation Create(
        TourProductId tourProductId,
        string localeCode,
        string title,
        string? description,
        Instant now,
        string? slug = null)
    {
        return new TourProductTranslation(
            tourProductId,
            NormalizeLocaleCode(localeCode),
            NormalizeTitle(title),
            NormalizeDescription(description),
            NormalizeSlug(slug),
            now);
    }

    internal void Update(string title, string? description, Instant now)
    {
        Title = NormalizeTitle(title);
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

        var parts = trimmed.Split('-', 2, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (parts.Length == 1)
        {
            return parts[0].ToLowerInvariant();
        }

        return $"{parts[0].ToLowerInvariant()}-{parts[1].ToUpperInvariant()}";
    }

    public static string? NormalizeSlug(string? slug)
    {
        if (string.IsNullOrWhiteSpace(slug))
        {
            return null;
        }

        var trimmed = slug.Trim().ToLowerInvariant();
        if (trimmed.Length > SlugMaxLength)
        {
            throw new ArgumentException($"Slug max length is {SlugMaxLength}.", nameof(slug));
        }

        if (trimmed.Any(static c => !(char.IsAsciiLetterOrDigit(c) || c == '-')))
        {
            throw new ArgumentException("Slug may contain only a-z, 0-9, and hyphen.", nameof(slug));
        }

        if (trimmed.StartsWith('-') || trimmed.EndsWith('-') || trimmed.Contains("--", StringComparison.Ordinal))
        {
            throw new ArgumentException(
                "Slug must not start/end with hyphen or contain consecutive hyphens.",
                nameof(slug));
        }

        return trimmed;
    }

    private static string NormalizeTitle(string title)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        var trimmed = title.Trim();
        if (trimmed.Length > TitleMaxLength)
        {
            throw new ArgumentException($"Tour translation title max length is {TitleMaxLength}.", nameof(title));
        }

        return trimmed;
    }

    private static string? NormalizeDescription(string? description)
    {
        if (description is null)
        {
            return null;
        }

        var trimmed = description.Trim();
        if (trimmed.Length == 0)
        {
            return null;
        }

        if (trimmed.Length > DescriptionMaxLength)
        {
            throw new ArgumentException(
                $"Tour translation description max length is {DescriptionMaxLength}.",
                nameof(description));
        }

        return trimmed;
    }
}
