namespace TravelCore.Modules.Content.Contracts;

/// <summary>
/// Deterministic public related-content read (TC-P14-T006 / P14-R6).
/// Shared Destination only. Locale title+slug public gate. Not ranking / IndexPolicy.
/// </summary>
public static class RelatedContentPublicEligibility
{
    public const int MaxItems = 6;

    public static bool IsPubliclyEligible(string? title, string? slug)
        => !string.IsNullOrWhiteSpace(title) && !string.IsNullOrWhiteSpace(slug);
}

public sealed record RelatedPublishedContent(
    Guid ContentItemId,
    string Kind,
    string Code,
    string Name,
    string Slug);

public interface IRelatedContentPublicQuery
{
    Task<IReadOnlyList<RelatedPublishedContent>> GetByDestinationAsync(
        Guid destinationId,
        string localeCode,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<RelatedPublishedContent>> GetByDestinationsAsync(
        IReadOnlyCollection<Guid> destinationIds,
        string localeCode,
        CancellationToken cancellationToken = default);
}
