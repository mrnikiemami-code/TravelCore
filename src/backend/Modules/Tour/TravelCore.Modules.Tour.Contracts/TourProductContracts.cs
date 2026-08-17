namespace TravelCore.Modules.Tour.Contracts;

/// <summary>
/// TourProduct catalog publication + slug + public read (TC-P09-T008 / P09-R4/R5/R6).
/// </summary>
public sealed record TourProductResponse(
    Guid Id,
    string Kind,
    string Code,
    string EnglishName,
    string CatalogStatus,
    string? ClassificationCode,
    Guid? OriginDestinationId,
    Guid? AgencyId,
    string CreatedAt,
    string UpdatedAt,
    string? LocalizedTitle = null,
    string? LocalizedDescription = null,
    string? LocalizedSlug = null,
    IReadOnlyList<Guid>? DestinationIds = null);

public sealed record SetTourCatalogStatusRequest(string CatalogStatus);

public sealed record SetTourProductTranslationSlugRequest(string? Slug);

public sealed record TourProductSlugLookupResponse(
    Guid TourProductId,
    string LocaleCode,
    string Slug,
    string Kind,
    string Code,
    string EnglishName,
    string CatalogStatus);

public interface ITourProductService
{
    Task<TourProductResponse?> GetAsync(
        Guid tourProductId,
        string? localeCode = null,
        CancellationToken cancellationToken = default);

    Task<TourProductResponse> SetCatalogStatusAsync(
        Guid tourProductId,
        SetTourCatalogStatusRequest request,
        CancellationToken cancellationToken = default);

    Task<TourProductResponse> SetTranslationSlugAsync(
        Guid tourProductId,
        string localeCode,
        SetTourProductTranslationSlugRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// When <paramref name="publicOnly"/> is true, only Published products with a title+slug row resolve.
    /// </summary>
    Task<TourProductSlugLookupResponse?> FindBySlugAsync(
        string localeCode,
        string slug,
        bool publicOnly = true,
        CancellationToken cancellationToken = default);
}
