using Microsoft.EntityFrameworkCore;
using NodaTime;
using TravelCore.Modules.Tour.Contracts;
using TravelCore.Modules.Tour.Domain;

namespace TravelCore.Modules.Tour.Infrastructure.Services;

/// <summary>
/// TourProduct catalog publication + slug + public lookup (TC-P09-T008).
/// </summary>
public sealed class TourProductService : ITourProductService
{
    private readonly TourDbContext _db;
    private readonly IClock _clock;

    public TourProductService(TourDbContext db, IClock clock)
    {
        _db = db;
        _clock = clock;
    }

    public async Task<TourProductResponse?> GetAsync(
        Guid tourProductId,
        string? localeCode = null,
        CancellationToken cancellationToken = default)
    {
        var product = await FindAsync(tourProductId, cancellationToken);
        return product is null ? null : Map(product, localeCode);
    }

    public async Task<TourProductResponse> SetCatalogStatusAsync(
        Guid tourProductId,
        SetTourCatalogStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var status = ParseCatalogStatus(request.CatalogStatus);
        var product = await LoadTrackedAsync(tourProductId, cancellationToken);
        product.SetCatalogStatus(status, _clock.GetCurrentInstant());
        await _db.SaveChangesAsync(cancellationToken);
        return Map(product);
    }

    public async Task<TourProductResponse> SetTranslationSlugAsync(
        Guid tourProductId,
        string localeCode,
        SetTourProductTranslationSlugRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var normalizedLocale = TourProductTranslation.NormalizeLocaleCode(localeCode);
        var normalizedSlug = TourProductTranslation.NormalizeSlug(request.Slug);
        if (normalizedSlug is not null)
        {
            await EnsureSlugUniqueAsync(normalizedLocale, normalizedSlug, tourProductId, cancellationToken);
        }

        var product = await LoadTrackedAsync(tourProductId, cancellationToken);
        product.SetTranslationSlug(normalizedLocale, normalizedSlug, _clock.GetCurrentInstant());
        await _db.SaveChangesAsync(cancellationToken);
        return Map(product, normalizedLocale);
    }

    public async Task<TourProductSlugLookupResponse?> FindBySlugAsync(
        string localeCode,
        string slug,
        bool publicOnly = true,
        CancellationToken cancellationToken = default)
    {
        var normalizedLocale = TourProductTranslation.NormalizeLocaleCode(localeCode);
        var normalizedSlug = TourProductTranslation.NormalizeSlug(slug)
            ?? throw new ArgumentException("Slug cannot be empty.", nameof(slug));

        var product = await _db.TourProducts
            .AsNoTracking()
            .FirstOrDefaultAsync(
                p => p.Translations.Any(t =>
                    t.LocaleCode == normalizedLocale
                    && t.Slug == normalizedSlug),
                cancellationToken);

        if (product is null)
        {
            return null;
        }

        if (publicOnly && product.CatalogStatus != TourCatalogStatus.Published)
        {
            return null;
        }

        var translation = product.FindTranslation(normalizedLocale);
        if (translation?.Slug is null || string.IsNullOrWhiteSpace(translation.Title))
        {
            return null;
        }

        return new TourProductSlugLookupResponse(
            product.Id.Value,
            translation.LocaleCode,
            translation.Slug!,
            product.Kind.ToString(),
            product.Code,
            product.EnglishName,
            product.CatalogStatus.ToString());
    }

    private async Task EnsureSlugUniqueAsync(
        string localeCode,
        string slug,
        Guid ownerId,
        CancellationToken cancellationToken)
    {
        var owner = TourProductId.From(ownerId);
        var conflict = await _db.TourProducts.AsNoTracking()
            .SelectMany(p => p.Translations.Select(t => new { TourProductId = p.Id, t.LocaleCode, t.Slug }))
            .FirstOrDefaultAsync(
                x => x.LocaleCode == localeCode && x.Slug == slug && x.TourProductId != owner,
                cancellationToken);

        if (conflict is not null)
        {
            throw new ArgumentException(
                $"Slug '{slug}' is already used for locale '{localeCode}'.",
                nameof(slug));
        }
    }

    private static TourCatalogStatus ParseCatalogStatus(string catalogStatus)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(catalogStatus);
        if (Enum.TryParse<TourCatalogStatus>(catalogStatus.Trim(), ignoreCase: true, out var parsed)
            && Enum.IsDefined(parsed))
        {
            return parsed;
        }

        throw new ArgumentException(
            "CatalogStatus must be one of: Draft, Published, Inactive.",
            nameof(catalogStatus));
    }

    private async Task<TourProduct?> FindAsync(Guid tourProductId, CancellationToken cancellationToken)
    {
        var id = TourProductId.From(tourProductId);
        return await _db.TourProducts.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    private async Task<TourProduct> LoadTrackedAsync(Guid tourProductId, CancellationToken cancellationToken)
    {
        return await FindAsync(tourProductId, cancellationToken)
            ?? throw new KeyNotFoundException($"TourProduct '{tourProductId}' was not found.");
    }

    private static TourProductResponse Map(TourProduct product, string? localeCode = null)
    {
        TourProductTranslation? localized = null;
        if (!string.IsNullOrWhiteSpace(localeCode))
        {
            localized = product.FindTranslation(localeCode);
        }

        return new TourProductResponse(
            product.Id.Value,
            product.Kind.ToString(),
            product.Code,
            product.EnglishName,
            product.CatalogStatus.ToString(),
            product.ClassificationCode,
            product.OriginDestinationId,
            product.AgencyId,
            product.CreatedAt.ToString(),
            product.UpdatedAt.ToString(),
            localized?.Title,
            localized?.Description,
            localized?.Slug,
            product.Destinations.Select(x => x.DestinationId).OrderBy(x => x).ToArray());
    }
}
