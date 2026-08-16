using NodaTime;

namespace TravelCore.Modules.Content.Domain;

/// <summary>
/// Locale-specific title/body/excerpt/slug for a ContentItem (TC-P08-T003 / T008).
/// Uses locale rows (ADR 0008) — never per-language columns on the aggregate root.
/// Locale codes are ReferenceData-owned; Content stores the opaque code only (no cross-schema FK).
/// P08-R3: ContentItemTranslation owns localized current slug; SEO owns route binding,
/// redirect history, canonical/history, and publication SEO state. No global slug engine in Content.
/// </summary>
public sealed class ContentItemTranslation
{
    public const int LocaleCodeMaxLength = 16;
    public const int TitleMaxLength = 300;
    public const int ExcerptMaxLength = 1000;
    public const int BodyMaxLength = 50_000;
    public const int SlugMaxLength = 120;

    private ContentItemTranslation()
    {
        LocaleCode = null!;
        Title = null!;
    }

    private ContentItemTranslation(
        ContentItemId contentItemId,
        string localeCode,
        string title,
        string? body,
        string? excerpt,
        string? slug,
        Instant updatedAt)
    {
        ContentItemId = contentItemId;
        LocaleCode = localeCode;
        Title = title;
        Body = body;
        Excerpt = excerpt;
        Slug = slug;
        UpdatedAt = updatedAt;
    }

    public ContentItemId ContentItemId { get; private set; }

    public string LocaleCode { get; private set; }

    public string Title { get; private set; }

    /// <summary>
    /// Optional plain editorial body for early localization baseline.
    /// Structured Content Blocks are a later P08 task (P08-R2) — not invented here.
    /// </summary>
    public string? Body { get; private set; }

    public string? Excerpt { get; private set; }

    /// <summary>
    /// Current locale-specific public slug SoR (P08-R3). Not SEO history / redirect / IndexPolicy.
    /// </summary>
    public string? Slug { get; private set; }

    public Instant UpdatedAt { get; private set; }

    internal static ContentItemTranslation Create(
        ContentItemId contentItemId,
        string localeCode,
        string title,
        string? body,
        string? excerpt,
        Instant now,
        string? slug = null)
    {
        return new ContentItemTranslation(
            contentItemId,
            NormalizeLocaleCode(localeCode),
            NormalizeTitle(title),
            NormalizeBody(body),
            NormalizeExcerpt(excerpt),
            NormalizeSlug(slug),
            now);
    }

    internal void Update(string title, string? body, string? excerpt, Instant now)
    {
        Title = NormalizeTitle(title);
        Body = NormalizeBody(body);
        Excerpt = NormalizeExcerpt(excerpt);
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

    private static string NormalizeTitle(string title)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        var trimmed = title.Trim();
        if (trimmed.Length > TitleMaxLength)
        {
            throw new ArgumentException($"Translation title max length is {TitleMaxLength}.", nameof(title));
        }

        return trimmed;
    }

    private static string? NormalizeBody(string? body)
    {
        if (string.IsNullOrWhiteSpace(body))
        {
            return null;
        }

        var trimmed = body.Trim();
        if (trimmed.Length > BodyMaxLength)
        {
            throw new ArgumentException($"Translation body max length is {BodyMaxLength}.", nameof(body));
        }

        return trimmed;
    }

    private static string? NormalizeExcerpt(string? excerpt)
    {
        if (string.IsNullOrWhiteSpace(excerpt))
        {
            return null;
        }

        var trimmed = excerpt.Trim();
        if (trimmed.Length > ExcerptMaxLength)
        {
            throw new ArgumentException(
                $"Translation excerpt max length is {ExcerptMaxLength}.",
                nameof(excerpt));
        }

        return trimmed;
    }

    /// <summary>
    /// Same opaque URL-segment rules as PlaceTranslation / DestinationTranslation (P04 / SEO conventions).
    /// </summary>
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
            throw new ArgumentException("Slug must not start/end with hyphen or contain consecutive hyphens.", nameof(slug));
        }

        return trimmed;
    }
}
