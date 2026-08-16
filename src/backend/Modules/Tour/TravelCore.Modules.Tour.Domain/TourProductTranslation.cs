using NodaTime;

namespace TravelCore.Modules.Tour.Domain;

/// <summary>
/// Locale-specific title/description for a TourProduct (TC-P09-T003 / ADR 0008).
/// Locale rows only — never per-language columns on the aggregate (ADR 0008).
/// Locale codes are ReferenceData-owned; Tour stores the opaque code only (no cross-schema FK).
/// Slug deferred until P09-R5 is locked (architect: no slug in T003).
/// </summary>
public sealed class TourProductTranslation
{
    public const int LocaleCodeMaxLength = 16;
    public const int TitleMaxLength = 200;
    public const int DescriptionMaxLength = 2000;

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
        Instant updatedAt)
    {
        TourProductId = tourProductId;
        LocaleCode = localeCode;
        Title = title;
        Description = description;
        UpdatedAt = updatedAt;
    }

    public TourProductId TourProductId { get; private set; }

    public string LocaleCode { get; private set; }

    public string Title { get; private set; }

    public string? Description { get; private set; }

    public Instant UpdatedAt { get; private set; }

    internal static TourProductTranslation Create(
        TourProductId tourProductId,
        string localeCode,
        string title,
        string? description,
        Instant now)
    {
        return new TourProductTranslation(
            tourProductId,
            NormalizeLocaleCode(localeCode),
            NormalizeTitle(title),
            NormalizeDescription(description),
            now);
    }

    internal void Update(string title, string? description, Instant now)
    {
        Title = NormalizeTitle(title);
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
