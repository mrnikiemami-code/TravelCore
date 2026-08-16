using NodaTime;

namespace TravelCore.Modules.Seo.Domain;

/// <summary>
/// SEO-owned technical metadata override for a resource+locale binding (TC-P05-T007).
/// Does not duplicate Destination CMS — only optional title/description mechanics.
/// </summary>
public sealed class SeoMetadataOverride
{
    public const int TitleMaxLength = 200;
    public const int DescriptionMaxLength = 500;

    private SeoMetadataOverride()
    {
        Locale = null!;
    }

    private SeoMetadataOverride(
        SeoMetadataOverrideId id,
        SeoResourceType resourceType,
        Guid resourceId,
        string locale,
        string? titleOverride,
        string? descriptionOverride,
        Instant updatedAt)
    {
        Id = id;
        ResourceType = resourceType;
        ResourceId = resourceId;
        Locale = locale;
        TitleOverride = titleOverride;
        DescriptionOverride = descriptionOverride;
        UpdatedAt = updatedAt;
    }

    public SeoMetadataOverrideId Id { get; private set; }

    public SeoResourceType ResourceType { get; private set; }

    public Guid ResourceId { get; private set; }

    public string Locale { get; private set; }

    public string? TitleOverride { get; private set; }

    public string? DescriptionOverride { get; private set; }

    public Instant UpdatedAt { get; private set; }

    public static SeoMetadataOverride Create(
        SeoResourceType resourceType,
        Guid resourceId,
        string locale,
        string? titleOverride,
        string? descriptionOverride,
        Instant now,
        SeoMetadataOverrideId? id = null)
    {
        if (!Enum.IsDefined(resourceType))
        {
            throw new ArgumentOutOfRangeException(nameof(resourceType));
        }

        if (resourceId == Guid.Empty)
        {
            throw new ArgumentException("ResourceId cannot be empty.", nameof(resourceId));
        }

        return new SeoMetadataOverride(
            id ?? SeoMetadataOverrideId.New(),
            resourceType,
            resourceId,
            SeoRoute.NormalizeLocale(locale),
            NormalizeOptional(titleOverride, TitleMaxLength, nameof(titleOverride)),
            NormalizeOptional(descriptionOverride, DescriptionMaxLength, nameof(descriptionOverride)),
            now);
    }

    public void Replace(string? titleOverride, string? descriptionOverride, Instant now)
    {
        TitleOverride = NormalizeOptional(titleOverride, TitleMaxLength, nameof(titleOverride));
        DescriptionOverride = NormalizeOptional(descriptionOverride, DescriptionMaxLength, nameof(descriptionOverride));
        UpdatedAt = now;
    }

    private static string? NormalizeOptional(string? value, int maxLength, string paramName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var trimmed = value.Trim();
        if (trimmed.Length > maxLength)
        {
            throw new ArgumentException($"{paramName} cannot exceed {maxLength} characters.", paramName);
        }

        return trimmed;
    }
}
