namespace TravelCore.Modules.Tour.Contracts;

/// <summary>
/// TourProduct catalog publication + slug + public/admin read (TC-P09-T008/T009 / P09-R4/R5/R6).
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

public sealed record CreateTourProductRequest(
    string Kind,
    string Code,
    string EnglishName);

public sealed record UpsertTourProductTranslationRequest(
    string Title,
    string? Description);

public sealed record TourProductTranslationResponse(
    Guid TourProductId,
    string LocaleCode,
    string Title,
    string? Description,
    string? Slug,
    string UpdatedAt);

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
    Task<TourProductResponse> CreateAsync(
        CreateTourProductRequest request,
        CancellationToken cancellationToken = default);

    Task<TourProductResponse?> GetAsync(
        Guid tourProductId,
        string? localeCode = null,
        CancellationToken cancellationToken = default);

    Task<TourProductResponse?> GetByCodeAsync(
        string code,
        string? localeCode = null,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TourProductResponse>> ListAsync(
        string? kind = null,
        int take = 50,
        CancellationToken cancellationToken = default);

    Task<TourProductTranslationResponse> UpsertTranslationAsync(
        Guid tourProductId,
        string localeCode,
        UpsertTourProductTranslationRequest request,
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
