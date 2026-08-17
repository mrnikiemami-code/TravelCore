namespace TravelCore.Modules.Tour.Contracts;

/// <summary>
/// Experience itinerary Stop semantic links (TC-P10-T003 / P10-R2).
/// DestinationId / PlaceId are logical Guids — validated via Destination/Place Contracts; no cross-schema FK.
/// Attraction is PlaceId with PlaceKind Attraction (no separate AttractionId).
/// </summary>
public sealed record ExperienceItineraryStopLinksResponse(
    Guid TourProductId,
    Guid ItineraryDayId,
    Guid StopId,
    int SortOrder,
    Guid? DestinationId,
    Guid? PlaceId);

public sealed record SetExperienceItineraryStopLinksRequest(
    Guid? DestinationId,
    Guid? PlaceId);

public interface IExperienceItineraryStopLinkService
{
    Task<ExperienceItineraryStopLinksResponse?> GetAsync(
        Guid tourProductId,
        Guid stopId,
        CancellationToken cancellationToken = default);

    Task<ExperienceItineraryStopLinksResponse> SetLinksAsync(
        Guid tourProductId,
        Guid stopId,
        SetExperienceItineraryStopLinksRequest request,
        CancellationToken cancellationToken = default);
}
