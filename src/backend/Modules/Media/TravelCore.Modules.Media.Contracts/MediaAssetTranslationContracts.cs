namespace TravelCore.Modules.Media.Contracts;

/// <summary>
/// Admin upsert for Media-owned alt/caption translation rows (TC-P06-T007).
/// PublicationStatus defaults to Draft on create when omitted (existence ≠ publication).
/// </summary>
public sealed record UpsertMediaAssetTranslationRequest(
    string AltText,
    string? Caption = null,
    string? PublicationStatus = null);

/// <summary>
/// Admin/internal translation DTO including publication lifecycle.
/// </summary>
public sealed record MediaAssetTranslationResponse(
    Guid MediaAssetId,
    string LocaleCode,
    string AltText,
    string? Caption,
    string PublicationStatus,
    string CreatedAt,
    string UpdatedAt);

/// <summary>
/// Presentation contract for a published locale alt/caption.
/// Returned only when the requested locale row is Published — never invented from another locale.
/// </summary>
public sealed record MediaAssetAltCaptionPresentation(
    Guid MediaAssetId,
    string LocaleCode,
    string AltText,
    string? Caption);

/// <summary>
/// Media-owned alt/caption translations. Admin mutations require Access.Media.Assets.Write.
/// Public presentation reads must not silently fall back across locales (ADR 0008).
/// </summary>
public interface IMediaAssetTranslationService
{
    Task<MediaAssetTranslationResponse> UpsertAsync(
        Guid mediaAssetId,
        string localeCode,
        UpsertMediaAssetTranslationRequest request,
        CancellationToken cancellationToken = default);

    Task<MediaAssetTranslationResponse?> GetAsync(
        Guid mediaAssetId,
        string localeCode,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<MediaAssetTranslationResponse>> ListAsync(
        Guid mediaAssetId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns published alt/caption for the exact requested locale, or null when missing/Draft/Ready/Archived.
    /// Does not invent content from another locale.
    /// </summary>
    Task<MediaAssetAltCaptionPresentation?> GetPublishedForPresentationAsync(
        Guid mediaAssetId,
        string localeCode,
        CancellationToken cancellationToken = default);
}
