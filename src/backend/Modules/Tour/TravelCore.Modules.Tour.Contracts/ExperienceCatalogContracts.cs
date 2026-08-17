namespace TravelCore.Modules.Tour.Contracts;

/// <summary>
/// Experience catalog publishability (TC-P10-T008 / P10-R8).
/// Reuses TourProduct CatalogStatus (Draft|Published|Inactive) — no second status column.
/// Published = public catalog visibility ≠ bookable ≠ priced.
/// </summary>
public sealed record ExperiencePublishabilityResponse(
    Guid TourProductId,
    string CatalogStatus,
    bool CanPublish,
    IReadOnlyList<string> BlockingReasons);

public sealed record SetExperienceCatalogStatusRequest(string CatalogStatus);

public interface IExperienceCatalogService
{
    Task<ExperiencePublishabilityResponse?> GetPublishabilityAsync(
        Guid tourProductId,
        CancellationToken cancellationToken = default);

    Task<ExperiencePublishabilityResponse> SetCatalogStatusAsync(
        Guid tourProductId,
        SetExperienceCatalogStatusRequest request,
        CancellationToken cancellationToken = default);
}
