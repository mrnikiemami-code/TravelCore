namespace TravelCore.Modules.Tour.Contracts;

/// <summary>
/// Deterministic public related-tour read (TC-P14-T005 / P14-R5).
/// Shared Destination only. Published catalog only. Not ranking / recommendation.
/// </summary>
public static class RelatedTourPublicEligibility
{
    public const int MaxItems = 6;

    public static bool IsEligible(string catalogStatus)
        => string.Equals(catalogStatus, "Published", StringComparison.Ordinal);
}

public sealed record RelatedPublishedTour(
    Guid TourProductId,
    string Kind,
    string Code,
    string Name,
    string Slug);

public interface IRelatedTourPublicQuery
{
    Task<IReadOnlyList<RelatedPublishedTour>> GetByTourProductAsync(
        Guid tourProductId,
        string localeCode,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<RelatedPublishedTour>> GetByDestinationAsync(
        Guid destinationId,
        string localeCode,
        Guid? excludeTourProductId,
        CancellationToken cancellationToken = default);
}
