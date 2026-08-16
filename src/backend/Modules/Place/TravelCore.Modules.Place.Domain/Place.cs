using NodaTime;

namespace TravelCore.Modules.Place.Domain;

/// <summary>
/// Place catalog aggregate root (P07-R1).
/// Shared catalog facts live here; type-specific facts live on Hotel/Restaurant/Attraction rows (1:1).
/// DestinationId is an optional logical association only (P07-R2) — not Place identity, not address/geo/slug SoR.
/// CatalogStatus / ClassificationCode / Facilities are catalog ops baseline (T004) — not R3 delete/archive.
/// Place↔Media links (Cover/Gallery) are Place-owned gallery meaning (T005) — logical MediaAssetId only.
/// </summary>
public sealed class Place
{
    public const int CodeMaxLength = 64;
    public const int NameMaxLength = 200;
    public const int ClassificationCodeMaxLength = 64;
    public const int MaxFacilities = 64;

    private readonly List<PlaceTranslation> _translations = [];
    private readonly List<PlaceFacility> _facilities = [];
    private readonly List<PlaceMediaLink> _mediaLinks = [];

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
        CatalogStatus = PlaceCatalogStatus.Draft;
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

    /// <summary>Baseline English display name (localized names live in translations).</summary>
    public string EnglishName { get; private set; }

    /// <summary>
    /// Optional single logical Destination reference (0..1). Stored as Guid identity only —
    /// no cross-schema FK, no EF navigation to Destination.
    /// </summary>
    public Guid? DestinationId { get; private set; }

    /// <summary>Optional WGS84 latitude. Place catalog geo — not Destination hierarchy.</summary>
    public decimal? Latitude { get; private set; }

    /// <summary>Optional WGS84 longitude. Place catalog geo — not Destination hierarchy.</summary>
    public decimal? Longitude { get; private set; }

    /// <summary>Optional Place-owned postal/street address (independent of Destination).</summary>
    public PlaceAddress? Address { get; private set; }

    /// <summary>
    /// Catalog operational status (Draft/Active/Inactive). Not delete/archive (P07-R3) and not bookable-now.
    /// </summary>
    public PlaceCatalogStatus CatalogStatus { get; private set; }

    /// <summary>
    /// Optional Place-owned classification code (opaque). Not a ReferenceData taxonomy product;
    /// distinct from kind specialization fields (CuisineType / CategoryCode / StarRating).
    /// </summary>
    public string? ClassificationCode { get; private set; }

    public Instant CreatedAt { get; private set; }

    public Instant UpdatedAt { get; private set; }

    public Hotel? Hotel { get; private set; }

    public Restaurant? Restaurant { get; private set; }

    public Attraction? Attraction { get; private set; }

    public IReadOnlyCollection<PlaceTranslation> Translations => _translations;

    public IReadOnlyCollection<PlaceFacility> Facilities => _facilities;

    public IReadOnlyCollection<PlaceMediaLink> MediaLinks => _mediaLinks;

    public PlaceMediaLink? Cover =>
        _mediaLinks.FirstOrDefault(x => x.Role == PlaceMediaRole.Cover);

    public IReadOnlyList<PlaceMediaLink> GalleryOrdered =>
        _mediaLinks
            .Where(x => x.Role == PlaceMediaRole.Gallery)
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.MediaAssetId)
            .ToList();

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
        Attraction? attraction,
        Guid? destinationId = null,
        decimal? latitude = null,
        decimal? longitude = null,
        PlaceAddress? address = null,
        IEnumerable<PlaceTranslation>? translations = null,
        PlaceCatalogStatus catalogStatus = PlaceCatalogStatus.Draft,
        string? classificationCode = null,
        IEnumerable<PlaceFacility>? facilities = null)
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

        place.SetCatalogStatus(catalogStatus, updatedAt);
        place.SetClassificationCode(classificationCode, updatedAt);

        if (destinationId is not null)
        {
            place.SetDestinationLink(destinationId, updatedAt);
        }

        if (latitude is not null || longitude is not null)
        {
            place.SetGeographicCoordinates(latitude, longitude, updatedAt);
        }

        if (address is not null)
        {
            place.Address = address;
        }

        if (translations is not null)
        {
            place._translations.AddRange(translations);
        }

        if (facilities is not null)
        {
            place.ReplaceFacilities(facilities.Select(f => f.Code), updatedAt);
        }

        return place;
    }

    /// <summary>
    /// Optional single Destination association (P07-R2). Null clears. Empty Guid is invalid.
    /// Existence of a non-null id is validated at the application boundary via Destination.Contracts.
    /// </summary>
    public void SetDestinationLink(Guid? destinationId, Instant now)
    {
        if (destinationId == Guid.Empty)
        {
            throw new ArgumentException(
                "DestinationId cannot be empty. Use null to clear the Destination link.",
                nameof(destinationId));
        }

        DestinationId = destinationId;
        UpdatedAt = now;
    }

    public void SetGeographicCoordinates(decimal? latitude, decimal? longitude, Instant now)
    {
        if (latitude is null && longitude is null)
        {
            Latitude = null;
            Longitude = null;
            UpdatedAt = now;
            return;
        }

        if (latitude is null || longitude is null)
        {
            throw new ArgumentException("Latitude and Longitude must both be set or both cleared.");
        }

        if (latitude is < -90m or > 90m)
        {
            throw new ArgumentOutOfRangeException(nameof(latitude), "Latitude must be between -90 and 90.");
        }

        if (longitude is < -180m or > 180m)
        {
            throw new ArgumentOutOfRangeException(nameof(longitude), "Longitude must be between -180 and 180.");
        }

        Latitude = decimal.Round(latitude.Value, 6, MidpointRounding.AwayFromZero);
        Longitude = decimal.Round(longitude.Value, 6, MidpointRounding.AwayFromZero);
        UpdatedAt = now;
    }

    public void SetAddress(PlaceAddress? address, Instant now)
    {
        Address = address;
        UpdatedAt = now;
    }

    /// <summary>
    /// Sets catalog operational status. Closed set Draft/Active/Inactive only —
    /// does not invent Deleted/Archived/Retired and does not resolve P07-R3.
    /// </summary>
    public void SetCatalogStatus(PlaceCatalogStatus status, Instant now)
    {
        if (!Enum.IsDefined(status))
        {
            throw new ArgumentOutOfRangeException(nameof(status), status, "Unsupported PlaceCatalogStatus.");
        }

        CatalogStatus = status;
        UpdatedAt = now;
    }

    public void SetClassificationCode(string? classificationCode, Instant now)
    {
        ClassificationCode = NormalizeClassificationCode(classificationCode);
        UpdatedAt = now;
    }

    /// <summary>
    /// Replaces the Place facility code set (deduped). Empty clears all facilities.
    /// </summary>
    public void ReplaceFacilities(IEnumerable<string> facilityCodes, Instant now)
    {
        ArgumentNullException.ThrowIfNull(facilityCodes);

        var normalized = facilityCodes
            .Select(PlaceFacility.NormalizeCode)
            .Distinct(StringComparer.Ordinal)
            .OrderBy(x => x, StringComparer.Ordinal)
            .ToList();

        if (normalized.Count > MaxFacilities)
        {
            throw new ArgumentException(
                $"A Place may have at most {MaxFacilities} facility codes.",
                nameof(facilityCodes));
        }

        _facilities.Clear();
        foreach (var code in normalized)
        {
            _facilities.Add(PlaceFacility.Create(Id, code));
        }

        UpdatedAt = now;
    }

    public static string? NormalizeClassificationCode(string? classificationCode)
    {
        if (string.IsNullOrWhiteSpace(classificationCode))
        {
            return null;
        }

        var trimmed = classificationCode.Trim().ToLowerInvariant();
        if (trimmed.Length > ClassificationCodeMaxLength)
        {
            throw new ArgumentException(
                $"Classification code max length is {ClassificationCodeMaxLength}.",
                nameof(classificationCode));
        }

        if (trimmed.Any(static c => !(char.IsAsciiLetterOrDigit(c) || c is '-' or '_')))
        {
            throw new ArgumentException(
                "Classification code may contain only a-z, 0-9, hyphen, and underscore.",
                nameof(classificationCode));
        }

        return trimmed;
    }

    /// <summary>
    /// Sets Cover with replacement semantics (0..1). SortOrder fixed at 0 (no Cover reorder).
    /// Same MediaAssetId cannot already be Gallery for this Place (UNIQUE PlaceId+MediaAssetId).
    /// </summary>
    public PlaceMediaLink SetCover(Guid mediaAssetId, Instant now)
    {
        if (mediaAssetId == Guid.Empty)
        {
            throw new ArgumentException("MediaAssetId cannot be empty.", nameof(mediaAssetId));
        }

        var existingSameAsset = _mediaLinks.FirstOrDefault(x => x.MediaAssetId == mediaAssetId);
        if (existingSameAsset is not null)
        {
            if (existingSameAsset.Role == PlaceMediaRole.Cover)
            {
                UpdatedAt = now;
                return existingSameAsset;
            }

            throw new InvalidOperationException(
                "MediaAssetId is already linked as Gallery for this Place; Cover and Gallery are mutually exclusive per asset.");
        }

        var existingCover = _mediaLinks.FirstOrDefault(x => x.Role == PlaceMediaRole.Cover);
        if (existingCover is not null)
        {
            _mediaLinks.Remove(existingCover);
        }

        var cover = PlaceMediaLink.CreateCover(Id, mediaAssetId);
        _mediaLinks.Add(cover);
        UpdatedAt = now;
        return cover;
    }

    public void RemoveCover(Instant now)
    {
        var cover = _mediaLinks.FirstOrDefault(x => x.Role == PlaceMediaRole.Cover);
        if (cover is null)
        {
            return;
        }

        _mediaLinks.Remove(cover);
        UpdatedAt = now;
    }

    /// <summary>
    /// Adds a Gallery item. SortOrder unique among Gallery for this Place; contiguity not required.
    /// When omitted, assigns max(Gallery SortOrder)+1 (or 0 when empty).
    /// </summary>
    public PlaceMediaLink AddGalleryItem(Guid mediaAssetId, Instant now, int? sortOrder = null)
    {
        if (mediaAssetId == Guid.Empty)
        {
            throw new ArgumentException("MediaAssetId cannot be empty.", nameof(mediaAssetId));
        }

        if (_mediaLinks.Any(x => x.MediaAssetId == mediaAssetId))
        {
            throw new InvalidOperationException(
                "MediaAssetId is already linked for this Place (UNIQUE PlaceId, MediaAssetId).");
        }

        var resolvedSort = sortOrder ?? NextGallerySortOrder();
        if (resolvedSort < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(sortOrder), resolvedSort, "Gallery SortOrder must be >= 0.");
        }

        if (_mediaLinks.Any(x => x.Role == PlaceMediaRole.Gallery && x.SortOrder == resolvedSort))
        {
            throw new ArgumentException(
                $"Gallery SortOrder {resolvedSort} is already used for this Place.",
                nameof(sortOrder));
        }

        var link = PlaceMediaLink.CreateGallery(Id, mediaAssetId, resolvedSort);
        _mediaLinks.Add(link);
        UpdatedAt = now;
        return link;
    }

    public void RemoveGalleryItem(Guid mediaAssetId, Instant now)
    {
        if (mediaAssetId == Guid.Empty)
        {
            throw new ArgumentException("MediaAssetId cannot be empty.", nameof(mediaAssetId));
        }

        var link = _mediaLinks.FirstOrDefault(x =>
            x.MediaAssetId == mediaAssetId && x.Role == PlaceMediaRole.Gallery)
            ?? throw new ArgumentException(
                "Gallery MediaAssetId was not found for this Place.",
                nameof(mediaAssetId));

        _mediaLinks.Remove(link);
        UpdatedAt = now;
    }

    /// <summary>
    /// Reorders Gallery to the given MediaAssetId sequence and normalizes contiguous SortOrder 0..n-1.
    /// Must enumerate exactly the current Gallery set.
    /// </summary>
    public IReadOnlyList<PlaceMediaLink> ReorderGallery(IReadOnlyList<Guid> orderedMediaAssetIds, Instant now)
    {
        ArgumentNullException.ThrowIfNull(orderedMediaAssetIds);

        var gallery = _mediaLinks.Where(x => x.Role == PlaceMediaRole.Gallery).ToList();
        if (orderedMediaAssetIds.Count != gallery.Count)
        {
            throw new ArgumentException(
                "ReorderGallery must include exactly the current Gallery MediaAssetId set.",
                nameof(orderedMediaAssetIds));
        }

        if (orderedMediaAssetIds.Any(id => id == Guid.Empty))
        {
            throw new ArgumentException("MediaAssetId cannot be empty.", nameof(orderedMediaAssetIds));
        }

        if (orderedMediaAssetIds.Distinct().Count() != orderedMediaAssetIds.Count)
        {
            throw new ArgumentException("ReorderGallery MediaAssetId list must be unique.", nameof(orderedMediaAssetIds));
        }

        var byId = gallery.ToDictionary(x => x.MediaAssetId);
        foreach (var id in orderedMediaAssetIds)
        {
            if (!byId.ContainsKey(id))
            {
                throw new ArgumentException(
                    $"MediaAssetId '{id:D}' is not a Gallery item for this Place.",
                    nameof(orderedMediaAssetIds));
            }
        }

        for (var i = 0; i < orderedMediaAssetIds.Count; i++)
        {
            byId[orderedMediaAssetIds[i]].SetGallerySortOrder(i);
        }

        UpdatedAt = now;
        return GalleryOrdered;
    }

    private int NextGallerySortOrder()
    {
        var gallery = _mediaLinks.Where(x => x.Role == PlaceMediaRole.Gallery).ToList();
        if (gallery.Count == 0)
        {
            return 0;
        }

        return gallery.Max(x => x.SortOrder) + 1;
    }

    public PlaceTranslation UpsertTranslation(
        string localeCode,
        string name,
        string? description,
        Instant now,
        string? slug = null,
        bool setSlug = false)
    {
        var normalizedLocale = PlaceTranslation.NormalizeLocaleCode(localeCode);
        var existing = _translations.FirstOrDefault(x =>
            string.Equals(x.LocaleCode, normalizedLocale, StringComparison.Ordinal));

        if (existing is null)
        {
            var created = PlaceTranslation.Create(
                Id,
                normalizedLocale,
                name,
                description,
                now,
                setSlug ? slug : null);
            _translations.Add(created);
            UpdatedAt = now;
            return created;
        }

        existing.Update(name, description, now);
        if (setSlug)
        {
            existing.SetSlug(slug, now);
        }

        UpdatedAt = now;
        return existing;
    }

    public PlaceTranslation SetTranslationSlug(string localeCode, string? slug, Instant now)
    {
        var normalizedLocale = PlaceTranslation.NormalizeLocaleCode(localeCode);
        var existing = _translations.FirstOrDefault(x =>
            string.Equals(x.LocaleCode, normalizedLocale, StringComparison.Ordinal))
            ?? throw new ArgumentException(
                $"Translation for locale '{normalizedLocale}' was not found.",
                nameof(localeCode));

        existing.SetSlug(slug, now);
        UpdatedAt = now;
        return existing;
    }

    public PlaceTranslation? FindTranslation(string localeCode)
    {
        var normalizedLocale = PlaceTranslation.NormalizeLocaleCode(localeCode);
        return _translations.FirstOrDefault(x =>
            string.Equals(x.LocaleCode, normalizedLocale, StringComparison.Ordinal));
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
