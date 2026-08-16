using NodaTime;

namespace TravelCore.Modules.Media.Domain;

/// <summary>
/// Media-owned locale row for default alt/caption (TC-P06-T007).
/// Forbids AltFa/AltEn columns. Consumer context overrides remain deferred (P06-R9).
/// Locale codes are opaque ReferenceData-owned strings (no cross-schema FK).
/// </summary>
public sealed class MediaAssetTranslation
{
    public const int LocaleCodeMaxLength = 16;
    public const int AltTextMaxLength = 500;
    public const int CaptionMaxLength = 1000;

    private MediaAssetTranslation()
    {
        LocaleCode = null!;
        AltText = null!;
    }

    private MediaAssetTranslation(
        MediaAssetId mediaAssetId,
        string localeCode,
        string altText,
        string? caption,
        MediaTranslationPublicationStatus publicationStatus,
        Instant createdAt)
    {
        MediaAssetId = mediaAssetId;
        LocaleCode = localeCode;
        AltText = altText;
        Caption = caption;
        PublicationStatus = publicationStatus;
        CreatedAt = createdAt;
        UpdatedAt = createdAt;
    }

    public MediaAssetId MediaAssetId { get; private set; }

    public string LocaleCode { get; private set; }

    public string AltText { get; private set; }

    public string? Caption { get; private set; }

    public MediaTranslationPublicationStatus PublicationStatus { get; private set; }

    public Instant CreatedAt { get; private set; }

    public Instant UpdatedAt { get; private set; }

    public bool IsPublished => PublicationStatus == MediaTranslationPublicationStatus.Published;

    public static MediaAssetTranslation Create(
        MediaAssetId mediaAssetId,
        string localeCode,
        string altText,
        Instant now,
        string? caption = null,
        MediaTranslationPublicationStatus publicationStatus = MediaTranslationPublicationStatus.Draft)
    {
        if (!Enum.IsDefined(publicationStatus))
        {
            throw new ArgumentOutOfRangeException(
                nameof(publicationStatus),
                publicationStatus,
                "Unsupported MediaTranslationPublicationStatus.");
        }

        return new MediaAssetTranslation(
            mediaAssetId,
            NormalizeLocaleCode(localeCode),
            NormalizeAltText(altText),
            NormalizeCaption(caption),
            publicationStatus,
            now);
    }

    public void Update(
        string altText,
        string? caption,
        Instant now,
        MediaTranslationPublicationStatus? publicationStatus = null)
    {
        AltText = NormalizeAltText(altText);
        Caption = NormalizeCaption(caption);
        if (publicationStatus is not null)
        {
            if (!Enum.IsDefined(publicationStatus.Value))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(publicationStatus),
                    publicationStatus,
                    "Unsupported MediaTranslationPublicationStatus.");
            }

            PublicationStatus = publicationStatus.Value;
        }

        UpdatedAt = now;
    }

    public void SetPublicationStatus(MediaTranslationPublicationStatus publicationStatus, Instant now)
    {
        if (!Enum.IsDefined(publicationStatus))
        {
            throw new ArgumentOutOfRangeException(
                nameof(publicationStatus),
                publicationStatus,
                "Unsupported MediaTranslationPublicationStatus.");
        }

        PublicationStatus = publicationStatus;
        UpdatedAt = now;
    }

    public static string NormalizeLocaleCode(string localeCode)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(localeCode);
        var trimmed = localeCode.Trim();
        if (trimmed.Length > LocaleCodeMaxLength)
        {
            throw new ArgumentException(
                $"Locale code max length is {LocaleCodeMaxLength}.",
                nameof(localeCode));
        }

        // Preserve BCP-47 casing shape: language lower, region upper when present (fa, en-US).
        var parts = trimmed.Split('-', 2, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (parts.Length == 1)
        {
            return parts[0].ToLowerInvariant();
        }

        return $"{parts[0].ToLowerInvariant()}-{parts[1].ToUpperInvariant()}";
    }

    public static string NormalizeAltText(string altText)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(altText);
        var trimmed = altText.Trim();
        if (trimmed.Length > AltTextMaxLength)
        {
            throw new ArgumentException(
                $"Alt text max length is {AltTextMaxLength}.",
                nameof(altText));
        }

        return trimmed;
    }

    public static string? NormalizeCaption(string? caption)
    {
        if (string.IsNullOrWhiteSpace(caption))
        {
            return null;
        }

        var trimmed = caption.Trim();
        if (trimmed.Length > CaptionMaxLength)
        {
            throw new ArgumentException(
                $"Caption max length is {CaptionMaxLength}.",
                nameof(caption));
        }

        return trimmed;
    }
}
