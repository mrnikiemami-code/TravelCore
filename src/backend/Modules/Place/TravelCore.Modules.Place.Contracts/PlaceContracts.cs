namespace TravelCore.Modules.Place.Contracts;

/// <summary>
/// Public DTO for a Place catalog entry. Never exposes EF entities.
/// Canonical identity is PlaceId only (P07-R1) — no independent HotelId/RestaurantId/AttractionId.
/// DestinationId is optional association only (P07-R2) — not Place identity / address / geo / slug SoR.
/// </summary>
public sealed record PlaceResponse(
    Guid Id,
    string Kind,
    string Code,
    string EnglishName,
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

/// <summary>
/// Cross-module contract for Place create/get/list + localization / Destination link / geo-address (TC-P07-T003).
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
}
