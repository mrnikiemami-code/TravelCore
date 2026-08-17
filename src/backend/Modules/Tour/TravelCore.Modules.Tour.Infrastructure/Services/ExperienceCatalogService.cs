using Microsoft.EntityFrameworkCore;
using NodaTime;
using TravelCore.Modules.Tour.Contracts;
using TravelCore.Modules.Tour.Domain;

namespace TravelCore.Modules.Tour.Infrastructure.Services;

/// <summary>
/// Experience catalog publishability (TC-P10-T008 / P10-R8).
/// Mutates TourProduct.CatalogStatus with Experience completeness gates for Published.
/// </summary>
public sealed class ExperienceCatalogService : IExperienceCatalogService
{
    private readonly TourDbContext _db;
    private readonly IClock _clock;

    public ExperienceCatalogService(TourDbContext db, IClock clock)
    {
        _db = db;
        _clock = clock;
    }

    public async Task<ExperiencePublishabilityResponse?> GetPublishabilityAsync(
        Guid tourProductId,
        CancellationToken cancellationToken = default)
    {
        var product = await FindExperienceAsync(tourProductId, cancellationToken);
        if (product is null)
        {
            return null;
        }

        var specialization = await FindSpecializationAsync(tourProductId, cancellationToken);
        return Map(product, specialization);
    }

    public async Task<ExperiencePublishabilityResponse> SetCatalogStatusAsync(
        Guid tourProductId,
        SetExperienceCatalogStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var status = ParseCatalogStatus(request.CatalogStatus);
        var product = await LoadExperienceAsync(tourProductId, cancellationToken);
        var specialization = await FindSpecializationAsync(tourProductId, cancellationToken);

        if (status == TourCatalogStatus.Published)
        {
            ExperiencePublishability.EnsureCanPublish(product, specialization);
        }

        product.SetCatalogStatus(status, _clock.GetCurrentInstant());
        await _db.SaveChangesAsync(cancellationToken);
        return Map(product, specialization);
    }

    private async Task<TourProduct?> FindExperienceAsync(
        Guid tourProductId,
        CancellationToken cancellationToken)
    {
        var id = TourProductId.From(tourProductId);
        var product = await _db.TourProducts.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (product is null || product.Kind != TourKind.Experience)
        {
            return null;
        }

        return product;
    }

    private async Task<TourProduct> LoadExperienceAsync(
        Guid tourProductId,
        CancellationToken cancellationToken)
    {
        return await FindExperienceAsync(tourProductId, cancellationToken)
            ?? throw new KeyNotFoundException(
                $"Experience TourProduct '{tourProductId}' was not found.");
    }

    private async Task<TourExperienceSpecialization?> FindSpecializationAsync(
        Guid tourProductId,
        CancellationToken cancellationToken)
    {
        var id = TourProductId.From(tourProductId);
        return await _db.Set<TourExperienceSpecialization>()
            .FirstOrDefaultAsync(x => x.TourProductId == id, cancellationToken);
    }

    private static TourCatalogStatus ParseCatalogStatus(string catalogStatus)
    {
        if (string.IsNullOrWhiteSpace(catalogStatus))
        {
            throw new ArgumentException("CatalogStatus is required.", nameof(catalogStatus));
        }

        if (Enum.TryParse<TourCatalogStatus>(catalogStatus.Trim(), ignoreCase: true, out var parsed)
            && Enum.IsDefined(parsed))
        {
            return parsed;
        }

        throw new ArgumentException(
            "CatalogStatus must be one of: Draft, Published, Inactive.",
            nameof(catalogStatus));
    }

    private static ExperiencePublishabilityResponse Map(
        TourProduct product,
        TourExperienceSpecialization? specialization)
    {
        var reasons = ExperiencePublishability.EvaluateBlockingReasons(product, specialization);
        return new ExperiencePublishabilityResponse(
            product.Id.Value,
            product.CatalogStatus.ToString(),
            CanPublish: reasons.Count == 0,
            BlockingReasons: reasons);
    }
}
