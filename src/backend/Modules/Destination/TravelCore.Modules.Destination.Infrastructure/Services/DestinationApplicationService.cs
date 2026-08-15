using Microsoft.EntityFrameworkCore;
using NodaTime;
using TravelCore.Modules.Destination.Contracts;
using TravelCore.Modules.Destination.Domain;
using TravelCore.Modules.ReferenceData.Contracts;
using DestinationAggregate = TravelCore.Modules.Destination.Domain.Destination;

namespace TravelCore.Modules.Destination.Infrastructure.Services;

/// <summary>
/// Destination application service for create/get/children/translations/geo.
/// </summary>
public sealed class DestinationApplicationService
{
    private readonly DestinationDbContext _db;
    private readonly IClock _clock;
    private readonly IReferenceDataCatalogQuery _referenceData;

    public DestinationApplicationService(
        DestinationDbContext db,
        IClock clock,
        IReferenceDataCatalogQuery referenceData)
    {
        _db = db;
        _clock = clock;
        _referenceData = referenceData;
    }

    public async Task<DestinationResponse> CreateAsync(
        CreateDestinationRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var kind = ParseKind(request.Kind);
        DestinationId? parentId = request.ParentId is null
            ? null
            : DestinationId.From(request.ParentId.Value);

        DestinationAggregate? parent = null;
        if (parentId is not null)
        {
            parent = await _db.Destinations
                .FirstOrDefaultAsync(x => x.Id == parentId.Value, cancellationToken)
                ?? throw new ArgumentException("Parent destination was not found.", nameof(request.ParentId));
        }

        if (kind == DestinationKind.Country)
        {
            var iso = request.IsoCountryCode?.Trim();
            if (string.IsNullOrWhiteSpace(iso))
            {
                throw new ArgumentException(
                    "Country destinations require IsoCountryCode (ReferenceData alpha-2).",
                    nameof(request.IsoCountryCode));
            }

            var country = await _referenceData.GetCountryAsync(iso, cancellationToken);
            if (country is null)
            {
                throw new ArgumentException(
                    $"IsoCountryCode '{iso}' was not found in ReferenceData country catalog.",
                    nameof(request.IsoCountryCode));
            }
        }

        var now = _clock.GetCurrentInstant();
        var destination = DestinationAggregate.Create(
            kind,
            request.Code,
            request.EnglishName,
            now,
            parentId,
            request.IsoCountryCode,
            parent);

        _db.Destinations.Add(destination);
        await _db.SaveChangesAsync(cancellationToken);
        return Map(destination);
    }

    public async Task<DestinationResponse?> GetByIdAsync(
        Guid id,
        string? locale,
        CancellationToken cancellationToken)
    {
        var destinationId = DestinationId.From(id);
        var destination = await _db.Destinations.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == destinationId, cancellationToken);
        return destination is null ? null : Map(destination, locale);
    }

    public async Task<IReadOnlyList<DestinationResponse>> ListChildrenAsync(
        Guid parentId,
        CancellationToken cancellationToken)
    {
        var id = DestinationId.From(parentId);
        var children = await _db.Destinations.AsNoTracking()
            .Where(x => x.ParentId == id)
            .OrderBy(x => x.EnglishName)
            .ThenBy(x => x.Code)
            .ToListAsync(cancellationToken);

        return children.Select(x => Map(x)).ToList();
    }

    public async Task<DestinationTranslationResponse> UpsertTranslationAsync(
        Guid destinationId,
        string localeCode,
        UpsertDestinationTranslationRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var locale = await _referenceData.GetLocaleAsync(localeCode, cancellationToken)
            ?? throw new ArgumentException(
                $"Locale '{localeCode}' was not found in ReferenceData locale catalog.",
                nameof(localeCode));

        var id = DestinationId.From(destinationId);
        var destination = await _db.Destinations
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new ArgumentException("Destination was not found.", nameof(destinationId));

        var now = _clock.GetCurrentInstant();
        var setSlug = request.Slug is not null;
        var translation = destination.UpsertTranslation(
            locale.Code,
            request.Name,
            request.Description,
            now,
            request.Slug,
            setSlug);

        if (setSlug && translation.Slug is not null)
        {
            await EnsureSlugUniqueAsync(locale.Code, translation.Slug, destination.Id, cancellationToken);
        }

        await _db.SaveChangesAsync(cancellationToken);

        return new DestinationTranslationResponse(
            translation.DestinationId.Value,
            translation.LocaleCode,
            translation.Name,
            translation.Description,
            translation.Slug);
    }

    public async Task<DestinationTranslationResponse> SetTranslationSlugAsync(
        Guid destinationId,
        string localeCode,
        SetDestinationTranslationSlugRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var locale = await _referenceData.GetLocaleAsync(localeCode, cancellationToken)
            ?? throw new ArgumentException(
                $"Locale '{localeCode}' was not found in ReferenceData locale catalog.",
                nameof(localeCode));

        var id = DestinationId.From(destinationId);
        var destination = await _db.Destinations
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new ArgumentException("Destination was not found.", nameof(destinationId));

        var now = _clock.GetCurrentInstant();
        var translation = destination.SetTranslationSlug(locale.Code, request.Slug, now);
        if (translation.Slug is not null)
        {
            await EnsureSlugUniqueAsync(locale.Code, translation.Slug, destination.Id, cancellationToken);
        }

        await _db.SaveChangesAsync(cancellationToken);
        return new DestinationTranslationResponse(
            translation.DestinationId.Value,
            translation.LocaleCode,
            translation.Name,
            translation.Description,
            translation.Slug);
    }

    private async Task EnsureSlugUniqueAsync(
        string localeCode,
        string slug,
        DestinationId ownerId,
        CancellationToken cancellationToken)
    {
        var conflict = await _db.Destinations
            .AsNoTracking()
            .SelectMany(d => d.Translations.Select(t => new { DestinationId = d.Id, t.LocaleCode, t.Slug }))
            .FirstOrDefaultAsync(
                x => x.LocaleCode == localeCode && x.Slug == slug && x.DestinationId != ownerId,
                cancellationToken);

        if (conflict is not null)
        {
            throw new ArgumentException(
                $"Slug '{slug}' is already used for locale '{localeCode}'.",
                nameof(slug));
        }
    }

    public async Task<IReadOnlyList<DestinationTranslationResponse>> ListTranslationsAsync(
        Guid destinationId,
        CancellationToken cancellationToken)
    {
        var id = DestinationId.From(destinationId);
        var destination = await _db.Destinations.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (destination is null)
        {
            return Array.Empty<DestinationTranslationResponse>();
        }

        return destination.Translations
            .OrderBy(x => x.LocaleCode, StringComparer.Ordinal)
            .Select(x => new DestinationTranslationResponse(
                x.DestinationId.Value,
                x.LocaleCode,
                x.Name,
                x.Description,
                x.Slug))
            .ToList();
    }

    public async Task<DestinationResponse> SetGeoAsync(
        Guid destinationId,
        SetDestinationGeoRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var id = DestinationId.From(destinationId);
        var destination = await _db.Destinations
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new ArgumentException("Destination was not found.", nameof(destinationId));

        var now = _clock.GetCurrentInstant();
        destination.SetGeographicIdentity(request.Latitude, request.Longitude, now);
        await _db.SaveChangesAsync(cancellationToken);
        return Map(destination);
    }

    private static DestinationKind ParseKind(string kind)
    {
        if (Enum.TryParse<DestinationKind>(kind, ignoreCase: true, out var parsed)
            && Enum.IsDefined(parsed))
        {
            return parsed;
        }

        throw new ArgumentException(
            "Kind must be one of: Country, Region, City, Area.",
            nameof(kind));
    }

    private static DestinationResponse Map(DestinationAggregate destination, string? locale = null)
    {
        string? localizedName = null;
        string? localizedDescription = null;
        string? resolvedLocale = null;

        if (!string.IsNullOrWhiteSpace(locale))
        {
            var translation = destination.FindTranslation(locale);
            if (translation is not null)
            {
                localizedName = translation.Name;
                localizedDescription = translation.Description;
                resolvedLocale = translation.LocaleCode;
            }
        }

        return new DestinationResponse(
            destination.Id.Value,
            destination.Kind.ToString(),
            destination.Code,
            destination.EnglishName,
            destination.ParentId?.Value,
            destination.IsoCountryCode,
            destination.Latitude,
            destination.Longitude,
            localizedName,
            localizedDescription,
            resolvedLocale);
    }
}
