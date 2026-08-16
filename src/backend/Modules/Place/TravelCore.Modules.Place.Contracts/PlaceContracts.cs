using TravelCore.Modules.Media.Contracts;

namespace TravelCore.Modules.Place.Contracts;

/// <summary>
/// Public DTO for a Place catalog entry. Never exposes EF entities.
/// Canonical identity is PlaceId only (P07-R1) — no independent HotelId/RestaurantId/AttractionId.
/// DestinationId is optional association only (P07-R2) — not Place identity / address / geo / slug SoR.
/// CatalogStatus / ClassificationCode / Facilities are catalog ops (T004) — not delete/archive, not bookable-now.
/// </summary>
public sealed record PlaceResponse(
    Guid Id,
    string Kind,
    string Code,
    string EnglishName,
    string CatalogStatus,
    string? ClassificationCode,
    IReadOnlyList<string> Facilities,
    Guid? DestinationId,
    decimal? Latitude,
    decimal? Longitude,
    PlaceAddressResponse? Address,
    HotelDetailsResponse? Hotel,
    RestaurantDetailsResponse? Restaurant,
    AttractionDetailsResponse? Attraction,
    string CreatedAt,
    string UpdatedAt,
    string? LocalizedName = null,
    string? LocalizedDescription = null,
    string? Locale = null);

public sealed record PlaceAddressResponse(
    string? Line1,
    string? Line2,
    string? Locality,
    string? AdministrativeArea,
    string? PostalCode,
    string? CountryCode);

public sealed record HotelDetailsResponse(short? StarRating);

public sealed record RestaurantDetailsResponse(string? CuisineType);

public sealed record AttractionDetailsResponse(string? CategoryCode);

/// <summary>
/// Create a Place of one kind. Only the specialization fields for that kind may be set.
/// Optional DestinationId is validated via Destination.Contracts when supplied.
/// New Places start as CatalogStatus Draft unless overridden later via SetCatalogStatus.
/// </summary>
public sealed record CreatePlaceRequest(
    string Kind,
    string Code,
    string EnglishName,
    short? StarRating = null,
    string? CuisineType = null,
    string? CategoryCode = null,
    Guid? DestinationId = null);

public sealed record UpsertPlaceTranslationRequest(
    string Name,
    string? Description);

public sealed record PlaceTranslationResponse(
    Guid PlaceId,
    string LocaleCode,
    string Name,
    string? Description);

public sealed record SetPlaceDestinationLinkRequest(Guid? DestinationId);

public sealed record SetPlaceGeoRequest(
    decimal? Latitude,
    decimal? Longitude);

public sealed record SetPlaceAddressRequest(
    string? Line1,
    string? Line2,
    string? Locality,
    string? AdministrativeArea,
    string? PostalCode,
    string? CountryCode);

public sealed record SetPlaceCatalogStatusRequest(string CatalogStatus);

public sealed record SetPlaceClassificationRequest(string? ClassificationCode);

public sealed record SetPlaceFacilitiesRequest(IReadOnlyList<string> FacilityCodes);

public sealed record PlaceMediaLinkResponse(
    Guid PlaceId,
    Guid MediaAssetId,
    string Role,
    int SortOrder);

public sealed record SetPlaceCoverRequest(Guid MediaAssetId);

public sealed record AddPlaceGalleryItemRequest(
    Guid MediaAssetId,
    int? SortOrder = null);

public sealed record ReorderPlaceGalleryRequest(IReadOnlyList<Guid> OrderedMediaAssetIds);

public sealed record PlaceMediaItemPresentation(
    Guid MediaAssetId,
    string Role,
    int SortOrder,
    MediaAssetPresentationResponse? Presentation);

public sealed record PlaceMediaPresentationResponse(
    Guid PlaceId,
    PlaceMediaItemPresentation? Cover,
    IReadOnlyList<PlaceMediaItemPresentation> Gallery);

/// <summary>
/// Cross-module contract for Place catalog + Place↔Media relations (TC-P07-T005).
/// </summary>
public interface IPlaceService
{
    Task<PlaceResponse> CreateAsync(
        CreatePlaceRequest request,
        CancellationToken cancellationToken = default);

    Task<PlaceResponse?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<PlaceResponse?> GetByIdAsync(
        Guid id,
        string? locale,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Admin-friendly lookup by unique Place.Code (not slug — P07-R4 unresolved).
    /// </summary>
    Task<PlaceResponse?> GetByCodeAsync(
        string code,
        string? locale = null,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PlaceResponse>> ListAsync(
        string? kind = null,
        int take = 50,
        CancellationToken cancellationToken = default);

    Task<PlaceTranslationResponse> UpsertTranslationAsync(
        Guid placeId,
        string localeCode,
        UpsertPlaceTranslationRequest request,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PlaceTranslationResponse>> ListTranslationsAsync(
        Guid placeId,
        CancellationToken cancellationToken = default);

    Task<PlaceResponse> SetDestinationLinkAsync(
        Guid placeId,
        SetPlaceDestinationLinkRequest request,
        CancellationToken cancellationToken = default);

    Task<PlaceResponse> SetGeoAsync(
        Guid placeId,
        SetPlaceGeoRequest request,
        CancellationToken cancellationToken = default);

    Task<PlaceResponse> SetAddressAsync(
        Guid placeId,
        SetPlaceAddressRequest request,
        CancellationToken cancellationToken = default);

    Task<PlaceResponse> SetCatalogStatusAsync(
        Guid placeId,
        SetPlaceCatalogStatusRequest request,
        CancellationToken cancellationToken = default);

    Task<PlaceResponse> SetClassificationAsync(
        Guid placeId,
        SetPlaceClassificationRequest request,
        CancellationToken cancellationToken = default);

    Task<PlaceResponse> SetFacilitiesAsync(
        Guid placeId,
        SetPlaceFacilitiesRequest request,
        CancellationToken cancellationToken = default);

    Task<PlaceMediaLinkResponse> SetCoverAsync(
        Guid placeId,
        SetPlaceCoverRequest request,
        CancellationToken cancellationToken = default);

    Task RemoveCoverAsync(
        Guid placeId,
        CancellationToken cancellationToken = default);

    Task<PlaceMediaLinkResponse> AddGalleryItemAsync(
        Guid placeId,
        AddPlaceGalleryItemRequest request,
        CancellationToken cancellationToken = default);

    Task RemoveGalleryItemAsync(
        Guid placeId,
        Guid mediaAssetId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PlaceMediaLinkResponse>> ReorderGalleryAsync(
        Guid placeId,
        ReorderPlaceGalleryRequest request,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PlaceMediaLinkResponse>> ListMediaLinksAsync(
        Guid placeId,
        CancellationToken cancellationToken = default);

    Task<PlaceMediaPresentationResponse?> GetMediaPresentationAsync(
        Guid placeId,
        string? locale = null,
        CancellationToken cancellationToken = default);
}
