namespace TravelCore.Modules.Seo.Contracts;

public sealed record SeoHreflangAlternateResponse(
    string Locale,
    string Path,
    string Href);

public sealed record SeoHreflangBindingsResponse(
    string ResourceType,
    Guid ResourceId,
    IReadOnlyList<SeoHreflangAlternateResponse> Alternates);

/// <summary>
/// Alternate-locale (hreflang) bindings for genuine SeoRoute locales only (TC-P05-T006 / ADR 0008).
/// </summary>
public interface ISeoHreflangService
{
    Task<SeoHreflangBindingsResponse?> GetByResourceAsync(
        string resourceType,
        Guid resourceId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Resolves the resource bound to locale+path (current route only), then returns alternates.
    /// Historical/redirect/gone paths yield null (no fabricated bindings).
    /// </summary>
    Task<SeoHreflangBindingsResponse?> GetByPathAsync(
        string locale,
        string path,
        CancellationToken cancellationToken = default);
}
