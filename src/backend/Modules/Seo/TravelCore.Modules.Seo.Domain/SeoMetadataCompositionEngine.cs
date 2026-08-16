namespace TravelCore.Modules.Seo.Domain;

/// <summary>Inputs from Destination (or other domain) content contracts — not SEO SoR.</summary>
public sealed record SeoMetadataContentInput(
    string LocalizedTitle,
    string? LocalizedDescription);

/// <summary>Optional SEO technical overrides (never a Destination CMS copy).</summary>
public sealed record SeoMetadataOverrideValues(
    string? TitleOverride,
    string? DescriptionOverride);

/// <summary>Composed title/description after SEO rules (TC-P05-T007).</summary>
public sealed record SeoComposedTextMetadata(
    string Title,
    string? Description,
    bool UsedTitleOverride,
    bool UsedDescriptionOverride);

/// <summary>
/// Pure server-side title/description composition (TC-P05-T007).
/// Does not decide indexability — IndexPolicy engine remains authoritative for robots.
/// </summary>
public static class SeoMetadataCompositionEngine
{
    public const string DefaultBrandTitle = "TravelCore";

    public static SeoComposedTextMetadata Compose(
        SeoMetadataContentInput content,
        SeoMetadataOverrideValues? overrides = null)
    {
        ArgumentNullException.ThrowIfNull(content);

        var contentTitle = content.LocalizedTitle?.Trim() ?? string.Empty;
        var contentDescription = string.IsNullOrWhiteSpace(content.LocalizedDescription)
            ? null
            : content.LocalizedDescription.Trim();

        var titleOverride = string.IsNullOrWhiteSpace(overrides?.TitleOverride)
            ? null
            : overrides!.TitleOverride!.Trim();
        var descriptionOverride = string.IsNullOrWhiteSpace(overrides?.DescriptionOverride)
            ? null
            : overrides!.DescriptionOverride!.Trim();

        var usedTitleOverride = titleOverride is not null;
        var title = usedTitleOverride
            ? titleOverride!
            : (string.IsNullOrWhiteSpace(contentTitle) ? DefaultBrandTitle : contentTitle);

        var usedDescriptionOverride = descriptionOverride is not null;
        var description = usedDescriptionOverride ? descriptionOverride : contentDescription;

        return new SeoComposedTextMetadata(
            title,
            description,
            usedTitleOverride,
            usedDescriptionOverride);
    }
}
