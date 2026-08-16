using NodaTime;

namespace TravelCore.Modules.Place.Domain;

/// <summary>
/// Place catalog aggregate root (P07-R1).
/// Shared catalog facts live here; type-specific facts live on Hotel/Restaurant/Attraction rows (1:1).
/// Translations / geo / Destination / Media / SEO / slug are later P07 tasks.
/// </summary>
public sealed class Place
{
    public const int CodeMaxLength = 64;
    public const int NameMaxLength = 200;

    private Place()
    {
        Code = null!;
        EnglishName = null!;
    }

    private Place(
        PlaceId id,
        PlaceKind kind,
        string code,
        string englishName,
        Instant createdAt,
        Hotel? hotel,
        Restaurant? restaurant,
        Attraction? attraction)
    {
        Id = id;
        Kind = kind;
        Code = code;
        EnglishName = englishName;
        CreatedAt = createdAt;
        UpdatedAt = createdAt;
        Hotel = hotel;
        Restaurant = restaurant;
        Attraction = attraction;
        ValidateSpecializationInvariant(id, kind, hotel, restaurant, attraction);
    }

    public PlaceId Id { get; private set; }

    public PlaceKind Kind { get; private set; }

    /// <summary>Stable opaque place code within TravelCore (not SEO slug).</summary>
    public string Code { get; private set; }

    /// <summary>Baseline English display name (localized names arrive in later P07 tasks).</summary>
    public string EnglishName { get; private set; }

    public Instant CreatedAt { get; private set; }

    public Instant UpdatedAt { get; private set; }

    public Hotel? Hotel { get; private set; }

    public Restaurant? Restaurant { get; private set; }

    public Attraction? Attraction { get; private set; }

    public static Place CreateHotel(
        string code,
        string englishName,
        Instant now,
        short? starRating = null,
        PlaceId? id = null)
    {
        var placeId = id ?? PlaceId.New();
        return new Place(
            placeId,
            PlaceKind.Hotel,
            NormalizeCode(code),
            NormalizeName(englishName),
            now,
            hotel: Domain.Hotel.Create(placeId, starRating),
            restaurant: null,
            attraction: null);
    }

    public static Place CreateRestaurant(
        string code,
        string englishName,
        Instant now,
        string? cuisineType = null,
        PlaceId? id = null)
    {
        var placeId = id ?? PlaceId.New();
        return new Place(
            placeId,
            PlaceKind.Restaurant,
            NormalizeCode(code),
            NormalizeName(englishName),
            now,
            hotel: null,
            restaurant: Domain.Restaurant.Create(placeId, cuisineType),
            attraction: null);
    }

    public static Place CreateAttraction(
        string code,
        string englishName,
        Instant now,
        string? categoryCode = null,
        PlaceId? id = null)
    {
        var placeId = id ?? PlaceId.New();
        return new Place(
            placeId,
            PlaceKind.Attraction,
            NormalizeCode(code),
            NormalizeName(englishName),
            now,
            hotel: null,
            restaurant: null,
            attraction: Domain.Attraction.Create(placeId, categoryCode));
    }

    /// <summary>
    /// Reconstitute a Place with explicit specializations (tests / guarded composition).
    /// Enforces kind↔specialization match and single-kind ownership.
    /// </summary>
    public static Place Reconstitute(
        PlaceId id,
        PlaceKind kind,
        string code,
        string englishName,
        Instant createdAt,
        Instant updatedAt,
        Hotel? hotel,
        Restaurant? restaurant,
        Attraction? attraction)
    {
        var place = new Place(
            id,
            kind,
            NormalizeCode(code),
            NormalizeName(englishName),
            createdAt,
            hotel,
            restaurant,
            attraction)
        {
            UpdatedAt = updatedAt
        };
        return place;
    }

    /// <summary>
    /// Kind must match exactly one specialization row; multi-kind and mismatch are rejected.
    /// Specialization PlaceId must equal the Place aggregate id.
    /// </summary>
    public static void ValidateSpecializationInvariant(
        PlaceId placeId,
        PlaceKind kind,
        Hotel? hotel,
        Restaurant? restaurant,
        Attraction? attraction)
    {
        if (placeId.Value == Guid.Empty)
        {
            throw new ArgumentException("PlaceId cannot be empty.", nameof(placeId));
        }

        if (!Enum.IsDefined(kind))
        {
            throw new ArgumentOutOfRangeException(nameof(kind), kind, "Unsupported PlaceKind.");
        }

        var specializationCount =
            (hotel is null ? 0 : 1)
            + (restaurant is null ? 0 : 1)
            + (attraction is null ? 0 : 1);

        if (specializationCount > 1)
        {
            throw new ArgumentException(
                "A Place may have only one typed specialization (Hotel, Restaurant, or Attraction).",
                nameof(kind));
        }

        if (specializationCount == 0)
        {
            throw new ArgumentException(
                "A Place requires exactly one typed specialization matching its PlaceKind.",
                nameof(kind));
        }

        switch (kind)
        {
            case PlaceKind.Hotel:
                if (hotel is null)
                {
                    throw new ArgumentException(
                        "PlaceKind.Hotel requires a Hotel specialization.",
                        nameof(hotel));
                }

                if (restaurant is not null || attraction is not null)
                {
                    throw new ArgumentException(
                        "PlaceKind.Hotel cannot carry Restaurant or Attraction specializations.",
                        nameof(kind));
                }

                EnsureSamePlaceId(placeId, hotel.PlaceId, nameof(hotel));
                break;

            case PlaceKind.Restaurant:
                if (restaurant is null)
                {
                    throw new ArgumentException(
                        "PlaceKind.Restaurant requires a Restaurant specialization.",
                        nameof(restaurant));
                }

                if (hotel is not null || attraction is not null)
                {
                    throw new ArgumentException(
                        "PlaceKind.Restaurant cannot carry Hotel or Attraction specializations.",
                        nameof(kind));
                }

                EnsureSamePlaceId(placeId, restaurant.PlaceId, nameof(restaurant));
                break;

            case PlaceKind.Attraction:
                if (attraction is null)
                {
                    throw new ArgumentException(
                        "PlaceKind.Attraction requires an Attraction specialization.",
                        nameof(attraction));
                }

                if (hotel is not null || restaurant is not null)
                {
                    throw new ArgumentException(
                        "PlaceKind.Attraction cannot carry Hotel or Restaurant specializations.",
                        nameof(kind));
                }

                EnsureSamePlaceId(placeId, attraction.PlaceId, nameof(attraction));
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(kind), kind, "Unsupported PlaceKind.");
        }
    }

    public static string NormalizeCode(string code)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        var trimmed = code.Trim();
        if (trimmed.Length > CodeMaxLength)
        {
            throw new ArgumentException($"Place code max length is {CodeMaxLength}.", nameof(code));
        }

        return trimmed;
    }

    public static string NormalizeName(string englishName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(englishName);
        var trimmed = englishName.Trim();
        if (trimmed.Length > NameMaxLength)
        {
            throw new ArgumentException($"Place name max length is {NameMaxLength}.", nameof(englishName));
        }

        return trimmed;
    }

    private static void EnsureSamePlaceId(PlaceId expected, PlaceId actual, string paramName)
    {
        if (expected != actual)
        {
            throw new ArgumentException("Specialization PlaceId mismatch.", paramName);
        }
    }
}
