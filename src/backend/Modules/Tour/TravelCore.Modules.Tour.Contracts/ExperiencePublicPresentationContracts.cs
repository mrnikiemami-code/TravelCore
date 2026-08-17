namespace TravelCore.Modules.Tour.Contracts;

/// <summary>
/// Public read of existing Experience specialization facts (TC-P14-T004 / P14-R4).
/// Tour remains owner. No new Experience tables. Not bookable. Not Package specialty.
/// </summary>
public sealed record PublishedExperiencePresentation(
    Guid TourProductId,
    string? Difficulty,
    IReadOnlyList<PublishedExperienceItineraryDay> ItineraryDays,
    IReadOnlyList<PublishedExperienceFact> Eligibility,
    IReadOnlyList<PublishedExperienceEquipment> Equipment,
    IReadOnlyList<PublishedExperienceFact> LocalTransport,
    IReadOnlyList<PublishedExperienceGuide> Guides,
    IReadOnlyList<PublishedExperienceStay> AccommodationPlan);

public sealed record PublishedExperienceItineraryDay(
    int DayNumber,
    IReadOnlyList<PublishedExperienceStop> Stops,
    IReadOnlyList<string> Meals);

public sealed record PublishedExperienceStop(
    int SortOrder,
    Guid? DestinationId,
    Guid? PlaceId);

public sealed record PublishedExperienceFact(string Code, string? Value, string? Detail);

public sealed record PublishedExperienceEquipment(string Code, string Kind, string? Detail);

public sealed record PublishedExperienceGuide(Guid GuidePartyId, string Role, string? Note);

public sealed record PublishedExperienceStay(int SortOrder, Guid? PlaceId);

public interface IExperiencePublicPresentationQuery
{
    Task<PublishedExperiencePresentation?> GetPublishedAsync(
        Guid tourProductId,
        CancellationToken cancellationToken = default);
}
