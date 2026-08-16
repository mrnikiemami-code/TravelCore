using Microsoft.EntityFrameworkCore;
using NodaTime;
using TravelCore.Modules.Place.Contracts;
using TravelCore.Modules.Place.Domain;
using PlaceAggregate = TravelCore.Modules.Place.Domain.Place;

namespace TravelCore.Modules.Place.Infrastructure.Services;

/// <summary>
/// Application service implementing Place create/get/list (catalog SoR only).
/// </summary>
public sealed class PlaceApplicationService : IPlaceService
{
    private const int MaxListTake = 200;

    private readonly PlaceDbContext _db;
    private readonly IClock _clock;

    public PlaceApplicationService(PlaceDbContext db, IClock clock)
    {
        _db = db;
        _clock = clock;
    }

    public async Task<PlaceResponse> CreateAsync(
        CreatePlaceRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var kind = ParseKind(request.Kind);
        RejectCrossKindPayload(kind, request);

        var now = _clock.GetCurrentInstant();
        PlaceAggregate place = kind switch
        {
            PlaceKind.Hotel => PlaceAggregate.CreateHotel(
                request.Code,
                request.EnglishName,
                now,
                request.StarRating),
            PlaceKind.Restaurant => PlaceAggregate.CreateRestaurant(
                request.Code,
                request.EnglishName,
                now,
                request.CuisineType),
            PlaceKind.Attraction => PlaceAggregate.CreateAttraction(
                request.Code,
                request.EnglishName,
                now,
                request.CategoryCode),
            _ => throw new ArgumentOutOfRangeException(nameof(request.Kind), request.Kind, "Unsupported PlaceKind.")
        };

        _db.Places.Add(place);

        try
        {
            await _db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
        {
            throw new InvalidOperationException(
                "Place persistence conflict (e.g. duplicate code).",
                ex);
        }

        return Map(place);
    }

    public async Task<PlaceResponse?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var placeId = PlaceId.From(id);
        var place = await _db.Places.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == placeId, cancellationToken);
        return place is null ? null : Map(place);
    }

    public async Task<IReadOnlyList<PlaceResponse>> ListAsync(
        string? kind = null,
        int take = 50,
        CancellationToken cancellationToken = default)
    {
        if (take <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(take), take, "take must be positive.");
        }

        take = Math.Min(take, MaxListTake);
        var query = _db.Places.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(kind))
        {
            var parsed = ParseKind(kind);
            query = query.Where(x => x.Kind == parsed);
        }

        var places = await query
            .OrderByDescending(x => x.CreatedAt)
            .ThenBy(x => x.Id)
            .Take(take)
            .ToListAsync(cancellationToken);

        return places.Select(Map).ToList();
    }

    private static PlaceKind ParseKind(string kind)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(kind);
        if (Enum.TryParse<PlaceKind>(kind.Trim(), ignoreCase: true, out var parsed)
            && Enum.IsDefined(parsed))
        {
            return parsed;
        }

        throw new ArgumentException($"Unsupported PlaceKind '{kind}'.", nameof(kind));
    }

    /// <summary>
    /// Reject specialization payload fields that do not belong to the requested kind.
    /// </summary>
    private static void RejectCrossKindPayload(PlaceKind kind, CreatePlaceRequest request)
    {
        switch (kind)
        {
            case PlaceKind.Hotel:
                if (!string.IsNullOrWhiteSpace(request.CuisineType)
                    || !string.IsNullOrWhiteSpace(request.CategoryCode))
                {
                    throw new ArgumentException(
                        "Hotel create must not set CuisineType or CategoryCode.",
                        nameof(request));
                }

                break;

            case PlaceKind.Restaurant:
                if (request.StarRating is not null
                    || !string.IsNullOrWhiteSpace(request.CategoryCode))
                {
                    throw new ArgumentException(
                        "Restaurant create must not set StarRating or CategoryCode.",
                        nameof(request));
                }

                break;

            case PlaceKind.Attraction:
                if (request.StarRating is not null
                    || !string.IsNullOrWhiteSpace(request.CuisineType))
                {
                    throw new ArgumentException(
                        "Attraction create must not set StarRating or CuisineType.",
                        nameof(request));
                }

                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(kind), kind, "Unsupported PlaceKind.");
        }
    }

    private static PlaceResponse Map(PlaceAggregate place) =>
        new(
            place.Id.Value,
            place.Kind.ToString(),
            place.Code,
            place.EnglishName,
            place.Hotel is null ? null : new HotelDetailsResponse(place.Hotel.StarRating),
            place.Restaurant is null ? null : new RestaurantDetailsResponse(place.Restaurant.CuisineType),
            place.Attraction is null ? null : new AttractionDetailsResponse(place.Attraction.CategoryCode),
            place.CreatedAt.ToString(),
            place.UpdatedAt.ToString());
}
