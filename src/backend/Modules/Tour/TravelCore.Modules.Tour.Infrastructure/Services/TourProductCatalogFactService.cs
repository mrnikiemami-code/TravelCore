using Microsoft.EntityFrameworkCore;
using NodaTime;
using TravelCore.Modules.Tour.Contracts;
using TravelCore.Modules.Tour.Domain;

namespace TravelCore.Modules.Tour.Infrastructure.Services;

/// <summary>
/// TourProduct services / policies / requirements catalog-fact mutations (TC-P09-T006).
/// </summary>
public sealed class TourProductCatalogFactService : ITourProductCatalogFactService
{
    private readonly TourDbContext _db;
    private readonly IClock _clock;

    public TourProductCatalogFactService(TourDbContext db, IClock clock)
    {
        _db = db;
        _clock = clock;
    }

    public async Task<TourProductCatalogFactsResponse?> GetAsync(
        Guid tourProductId,
        CancellationToken cancellationToken = default)
    {
        var product = await FindAsync(tourProductId, cancellationToken);
        return product is null ? null : Map(product);
    }

    public async Task<TourProductCatalogFactsResponse> ReplaceServicesAsync(
        Guid tourProductId,
        ReplaceTourCatalogFactsRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var product = await LoadTrackedAsync(tourProductId, cancellationToken);
        product.ReplaceServices(MapInputs(request.Items), _clock.GetCurrentInstant());
        await _db.SaveChangesAsync(cancellationToken);
        return Map(product);
    }

    public async Task<TourProductCatalogFactsResponse> ReplacePoliciesAsync(
        Guid tourProductId,
        ReplaceTourCatalogFactsRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var product = await LoadTrackedAsync(tourProductId, cancellationToken);
        product.ReplacePolicies(MapInputs(request.Items), _clock.GetCurrentInstant());
        await _db.SaveChangesAsync(cancellationToken);
        return Map(product);
    }

    public async Task<TourProductCatalogFactsResponse> ReplaceRequirementsAsync(
        Guid tourProductId,
        ReplaceTourCatalogFactsRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var product = await LoadTrackedAsync(tourProductId, cancellationToken);
        product.ReplaceRequirements(MapInputs(request.Items), _clock.GetCurrentInstant());
        await _db.SaveChangesAsync(cancellationToken);
        return Map(product);
    }

    private static IEnumerable<TourCatalogFactInput> MapInputs(IReadOnlyList<TourCatalogFactDto> items)
    {
        ArgumentNullException.ThrowIfNull(items);
        return items.Select(x => new TourCatalogFactInput(x.Code, x.Detail));
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

    private static TourProductCatalogFactsResponse Map(TourProduct product) =>
        new(
            product.Id.Value,
            product.Code,
            product.Services.Select(x => new TourCatalogFactDto(x.Code, x.Detail)).ToArray(),
            product.Policies.Select(x => new TourCatalogFactDto(x.Code, x.Detail)).ToArray(),
            product.Requirements.Select(x => new TourCatalogFactDto(x.Code, x.Detail)).ToArray());
}
