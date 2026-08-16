namespace TravelCore.Modules.Place.Contracts;

/// <summary>
/// Public DTO for a Place catalog entry. Never exposes EF entities.
/// Canonical identity is PlaceId only (P07-R1) — no independent HotelId/RestaurantId/AttractionId.
/// </summary>
public sealed record PlaceResponse(
    Guid Id,
    string Kind,
    string Code,
    string EnglishName,
    HotelDetailsResponse? Hotel,
    RestaurantDetailsResponse? Restaurant,
    AttractionDetailsResponse? Attraction,
    string CreatedAt,
    string UpdatedAt);

public sealed record HotelDetailsResponse(short? StarRating);

public sealed record RestaurantDetailsResponse(string? CuisineType);

public sealed record AttractionDetailsResponse(string? CategoryCode);

/// <summary>
/// Create a Place of one kind. Only the specialization fields for that kind may be set.
/// </summary>
public sealed record CreatePlaceRequest(
    string Kind,
    string Code,
    string EnglishName,
    short? StarRating = null,
    string? CuisineType = null,
    string? CategoryCode = null);

/// <summary>
/// Cross-module contract for Place create/get/list (TC-P07-T002 baseline).
/// </summary>
public interface IPlaceService
{
    Task<PlaceResponse> CreateAsync(
        CreatePlaceRequest request,
        CancellationToken cancellationToken = default);

    Task<PlaceResponse?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PlaceResponse>> ListAsync(
        string? kind = null,
        int take = 50,
        CancellationToken cancellationToken = default);
}
