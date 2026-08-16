using Microsoft.EntityFrameworkCore;
using NodaTime;
using TravelCore.Modules.Destination.Contracts;
using TravelCore.Modules.Media.Contracts;
using TravelCore.Modules.Place.Contracts;
using TravelCore.Modules.Place.Domain;
using TravelCore.Modules.ReferenceData.Contracts;
using PlaceAggregate = TravelCore.Modules.Place.Domain.Place;

namespace TravelCore.Modules.Place.Infrastructure.Services;

/// <summary>
/// Application service implementing Place catalog + Place↔Media relations (TC-P07-T005).
/// </summary>
public sealed class PlaceApplicationService : IPlaceService
{
    private const int MaxListTake = 200;

    private readonly PlaceDbContext _db;
    private readonly IClock _clock;
    private readonly IDestinationExistenceQuery _destinations;
    private readonly IReferenceDataCatalogQuery _referenceData;
    private readonly IMediaAssetReadinessQuery _mediaReadiness;
    private readonly IMediaPresentationService _mediaPresentation;

    public PlaceApplicationService(
        PlaceDbContext db,
        IClock clock,
        IDestinationExistenceQuery destinations,
        IReferenceDataCatalogQuery referenceData,
        IMediaAssetReadinessQuery mediaReadiness,
        IMediaPresentationService mediaPresentation)
    {
        _db = db;
        _clock = clock;
        _destinations = destinations;
        _referenceData = referenceData;
        _mediaReadiness = mediaReadiness;
        _mediaPresentation = mediaPresentation;
    }

    public async Task<PlaceResponse> CreateAsync(
        CreatePlaceRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var kind = ParseKind(request.Kind);
        RejectCrossKindPayload(kind, request);
        await EnsureDestinationLinkValidAsync(request.DestinationId, cancellationToken);

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

        if (request.DestinationId is not null)
        {
            place.SetDestinationLink(request.DestinationId, now);
        }

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

    public Task<PlaceResponse?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default) =>
        GetByIdAsync(id, locale: null, cancellationToken);

    public async Task<PlaceResponse?> GetByIdAsync(
        Guid id,
        string? locale,
        CancellationToken cancellationToken = default)
    {
        var placeId = PlaceId.From(id);
        var place = await _db.Places.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == placeId, cancellationToken);
        return place is null ? null : Map(place, locale);
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

        return places.Select(x => Map(x)).ToList();
    }

    public async Task<PlaceTranslationResponse> UpsertTranslationAsync(
        Guid placeId,
        string localeCode,
        UpsertPlaceTranslationRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var locale = await _referenceData.GetLocaleAsync(localeCode, cancellationToken)
            ?? throw new ArgumentException(
                $"Locale '{localeCode}' was not found in ReferenceData locale catalog.",
                nameof(localeCode));

        var place = await LoadTrackedAsync(placeId, cancellationToken);
        var now = _clock.GetCurrentInstant();
        var translation = place.UpsertTranslation(locale.Code, request.Name, request.Description, now);
        await _db.SaveChangesAsync(cancellationToken);

        return new PlaceTranslationResponse(
            translation.PlaceId.Value,
            translation.LocaleCode,
            translation.Name,
            translation.Description);
    }

    public async Task<IReadOnlyList<PlaceTranslationResponse>> ListTranslationsAsync(
        Guid placeId,
        CancellationToken cancellationToken = default)
    {
        var id = PlaceId.From(placeId);
        var place = await _db.Places.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (place is null)
        {
            return [];
        }

        return place.Translations
            .OrderBy(x => x.LocaleCode, StringComparer.Ordinal)
            .Select(x => new PlaceTranslationResponse(
                x.PlaceId.Value,
                x.LocaleCode,
                x.Name,
                x.Description))
            .ToList();
    }

    public async Task<PlaceResponse> SetDestinationLinkAsync(
        Guid placeId,
        SetPlaceDestinationLinkRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        await EnsureDestinationLinkValidAsync(request.DestinationId, cancellationToken);

        var place = await LoadTrackedAsync(placeId, cancellationToken);
        var now = _clock.GetCurrentInstant();
        place.SetDestinationLink(request.DestinationId, now);
        await _db.SaveChangesAsync(cancellationToken);
        return Map(place);
    }

    public async Task<PlaceResponse> SetGeoAsync(
        Guid placeId,
        SetPlaceGeoRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var place = await LoadTrackedAsync(placeId, cancellationToken);
        var now = _clock.GetCurrentInstant();
        place.SetGeographicCoordinates(request.Latitude, request.Longitude, now);
        await _db.SaveChangesAsync(cancellationToken);
        return Map(place);
    }

    public async Task<PlaceResponse> SetAddressAsync(
        Guid placeId,
        SetPlaceAddressRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var place = await LoadTrackedAsync(placeId, cancellationToken);
        var now = _clock.GetCurrentInstant();
        var address = PlaceAddress.Create(
            request.Line1,
            request.Line2,
            request.Locality,
            request.AdministrativeArea,
            request.PostalCode,
            request.CountryCode);
        place.SetAddress(address, now);
        await _db.SaveChangesAsync(cancellationToken);
        return Map(place);
    }

    public async Task<PlaceResponse> SetCatalogStatusAsync(
        Guid placeId,
        SetPlaceCatalogStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var place = await LoadTrackedAsync(placeId, cancellationToken);
        var now = _clock.GetCurrentInstant();
        place.SetCatalogStatus(ParseCatalogStatus(request.CatalogStatus), now);
        await _db.SaveChangesAsync(cancellationToken);
        return Map(place);
    }

    public async Task<PlaceResponse> SetClassificationAsync(
        Guid placeId,
        SetPlaceClassificationRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var place = await LoadTrackedAsync(placeId, cancellationToken);
        var now = _clock.GetCurrentInstant();
        place.SetClassificationCode(request.ClassificationCode, now);
        await _db.SaveChangesAsync(cancellationToken);
        return Map(place);
    }

    public async Task<PlaceResponse> SetFacilitiesAsync(
        Guid placeId,
        SetPlaceFacilitiesRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.FacilityCodes);

        var place = await LoadTrackedAsync(placeId, cancellationToken);
        var now = _clock.GetCurrentInstant();
        place.ReplaceFacilities(request.FacilityCodes, now);
        await _db.SaveChangesAsync(cancellationToken);
        return Map(place);
    }

    public async Task<PlaceMediaLinkResponse> SetCoverAsync(
        Guid placeId,
        SetPlaceCoverRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        await EnsureMediaReadyAsync(request.MediaAssetId, cancellationToken);

        var place = await LoadTrackedAsync(placeId, cancellationToken);
        var now = _clock.GetCurrentInstant();
        var link = place.SetCover(request.MediaAssetId, now);
        await _db.SaveChangesAsync(cancellationToken);
        return MapMediaLink(link);
    }

    public async Task RemoveCoverAsync(
        Guid placeId,
        CancellationToken cancellationToken = default)
    {
        var place = await LoadTrackedAsync(placeId, cancellationToken);
        var now = _clock.GetCurrentInstant();
        place.RemoveCover(now);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task<PlaceMediaLinkResponse> AddGalleryItemAsync(
        Guid placeId,
        AddPlaceGalleryItemRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        await EnsureMediaReadyAsync(request.MediaAssetId, cancellationToken);

        var place = await LoadTrackedAsync(placeId, cancellationToken);
        var now = _clock.GetCurrentInstant();
        var link = place.AddGalleryItem(request.MediaAssetId, now, request.SortOrder);
        await _db.SaveChangesAsync(cancellationToken);
        return MapMediaLink(link);
    }

    public async Task RemoveGalleryItemAsync(
        Guid placeId,
        Guid mediaAssetId,
        CancellationToken cancellationToken = default)
    {
        var place = await LoadTrackedAsync(placeId, cancellationToken);
        var now = _clock.GetCurrentInstant();
        place.RemoveGalleryItem(mediaAssetId, now);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<PlaceMediaLinkResponse>> ReorderGalleryAsync(
        Guid placeId,
        ReorderPlaceGalleryRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.OrderedMediaAssetIds);

        var place = await LoadTrackedAsync(placeId, cancellationToken);
        var now = _clock.GetCurrentInstant();
        var links = place.ReorderGallery(request.OrderedMediaAssetIds, now);
        await _db.SaveChangesAsync(cancellationToken);
        return links.Select(MapMediaLink).ToList();
    }

    public async Task<IReadOnlyList<PlaceMediaLinkResponse>> ListMediaLinksAsync(
        Guid placeId,
        CancellationToken cancellationToken = default)
    {
        var id = PlaceId.From(placeId);
        var place = await _db.Places.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (place is null)
        {
            return [];
        }

        return place.MediaLinks
            .OrderBy(x => x.Role)
            .ThenBy(x => x.SortOrder)
            .ThenBy(x => x.MediaAssetId)
            .Select(MapMediaLink)
            .ToList();
    }

    public async Task<PlaceMediaPresentationResponse?> GetMediaPresentationAsync(
        Guid placeId,
        string? locale = null,
        CancellationToken cancellationToken = default)
    {
        var id = PlaceId.From(placeId);
        var place = await _db.Places.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (place is null)
        {
            return null;
        }

        PlaceMediaItemPresentation? cover = null;
        if (place.Cover is not null)
        {
            cover = new PlaceMediaItemPresentation(
                place.Cover.MediaAssetId,
                place.Cover.Role.ToString(),
                place.Cover.SortOrder,
                await _mediaPresentation.GetPresentationAsync(
                    place.Cover.MediaAssetId,
                    locale,
                    cancellationToken));
        }

        var gallery = new List<PlaceMediaItemPresentation>();
        foreach (var link in place.GalleryOrdered)
        {
            gallery.Add(new PlaceMediaItemPresentation(
                link.MediaAssetId,
                link.Role.ToString(),
                link.SortOrder,
                await _mediaPresentation.GetPresentationAsync(
                    link.MediaAssetId,
                    locale,
                    cancellationToken)));
        }

        return new PlaceMediaPresentationResponse(place.Id.Value, cover, gallery);
    }

    private async Task EnsureMediaReadyAsync(Guid mediaAssetId, CancellationToken cancellationToken)
    {
        if (mediaAssetId == Guid.Empty)
        {
            throw new ArgumentException("MediaAssetId cannot be empty.", nameof(mediaAssetId));
        }

        if (!await _mediaReadiness.IsReadyAsync(mediaAssetId, cancellationToken))
        {
            throw new InvalidOperationException(
                "MediaAsset must exist and be Ready to attach to a Place.");
        }
    }

    private static PlaceMediaLinkResponse MapMediaLink(PlaceMediaLink link) =>
        new(link.PlaceId.Value, link.MediaAssetId, link.Role.ToString(), link.SortOrder);

    private async Task<PlaceAggregate> LoadTrackedAsync(Guid placeId, CancellationToken cancellationToken)
    {
        var id = PlaceId.From(placeId);
        var place = await _db.Places.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (place is null)
        {
            throw new ArgumentException("Place was not found.", nameof(placeId));
        }

        return place;
    }

    /// <summary>
    /// P07-R2: null is valid; empty Guid invalid; nonexistent Destination identity rejects the mutation.
    /// </summary>
    private async Task EnsureDestinationLinkValidAsync(
        Guid? destinationId,
        CancellationToken cancellationToken)
    {
        if (destinationId is null)
        {
            return;
        }

        if (destinationId == Guid.Empty)
        {
            throw new ArgumentException(
                "DestinationId cannot be empty. Use null to clear the Destination link.",
                nameof(destinationId));
        }

        if (!await _destinations.ExistsAsync(destinationId.Value, cancellationToken))
        {
            throw new InvalidOperationException("Destination does not exist.");
        }
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

    private static PlaceCatalogStatus ParseCatalogStatus(string status)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(status);
        if (Enum.TryParse<PlaceCatalogStatus>(status.Trim(), ignoreCase: true, out var parsed)
            && Enum.IsDefined(parsed))
        {
            return parsed;
        }

        throw new ArgumentException(
            $"Unsupported PlaceCatalogStatus '{status}'. Allowed: Draft, Active, Inactive.",
            nameof(status));
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

    private static PlaceResponse Map(PlaceAggregate place, string? locale = null)
    {
        string? localizedName = null;
        string? localizedDescription = null;
        string? resolvedLocale = null;

        // ADR 0008: exact-locale overlay only — no silent cross-language invent.
        if (!string.IsNullOrWhiteSpace(locale))
        {
            var translation = place.FindTranslation(locale);
            if (translation is not null)
            {
                localizedName = translation.Name;
                localizedDescription = translation.Description;
                resolvedLocale = translation.LocaleCode;
            }
        }

        return new PlaceResponse(
            place.Id.Value,
            place.Kind.ToString(),
            place.Code,
            place.EnglishName,
            place.CatalogStatus.ToString(),
            place.ClassificationCode,
            place.Facilities
                .Select(f => f.Code)
                .OrderBy(c => c, StringComparer.Ordinal)
                .ToList(),
            place.DestinationId,
            place.Latitude,
            place.Longitude,
            place.Address is null
                ? null
                : new PlaceAddressResponse(
                    place.Address.Line1,
                    place.Address.Line2,
                    place.Address.Locality,
                    place.Address.AdministrativeArea,
                    place.Address.PostalCode,
                    place.Address.CountryCode),
            place.Hotel is null ? null : new HotelDetailsResponse(place.Hotel.StarRating),
            place.Restaurant is null ? null : new RestaurantDetailsResponse(place.Restaurant.CuisineType),
            place.Attraction is null ? null : new AttractionDetailsResponse(place.Attraction.CategoryCode),
            place.CreatedAt.ToString(),
            place.UpdatedAt.ToString(),
            localizedName,
            localizedDescription,
            resolvedLocale);
    }
}
