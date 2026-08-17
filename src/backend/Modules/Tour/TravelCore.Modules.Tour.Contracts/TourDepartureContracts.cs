namespace TravelCore.Modules.Tour.Contracts;

/// <summary>
/// Admin TourDeparture execution surface (TC-P11-T008). Not Booking / Pricing.
/// </summary>
public sealed record TourDepartureResponse(
    Guid Id,
    Guid TourProductId,
    string Status,
    string? StartDate,
    string? EndDate,
    string? TimeZoneId,
    int? MinimumPax,
    int? MaximumPax,
    string CreatedAt,
    string UpdatedAt);

public sealed record CreateTourDepartureRequest(Guid TourProductId);

public sealed record SetTourDepartureScheduleRequest(
    string StartDate,
    string EndDate,
    string TimeZoneId);

public sealed record SetTourDepartureCapacityRequest(
    int MinimumPax,
    int MaximumPax);

public sealed record SetTourDepartureStatusRequest(string Status);

public interface ITourDepartureAdminService
{
    Task<TourDepartureResponse> CreateAsync(
        CreateTourDepartureRequest request,
        CancellationToken cancellationToken = default);

    Task<TourDepartureResponse?> GetAsync(
        Guid departureId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TourDepartureResponse>> ListAsync(
        Guid? tourProductId = null,
        string? status = null,
        int take = 50,
        CancellationToken cancellationToken = default);

    Task<TourDepartureResponse> SetScheduleAsync(
        Guid departureId,
        SetTourDepartureScheduleRequest request,
        CancellationToken cancellationToken = default);

    Task<TourDepartureResponse> SetCapacityAsync(
        Guid departureId,
        SetTourDepartureCapacityRequest request,
        CancellationToken cancellationToken = default);

    Task<TourDepartureResponse> SetStatusAsync(
        Guid departureId,
        SetTourDepartureStatusRequest request,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Public published-departure summary (TC-P11-T009 · P11-R8). Published ≠ bookable.
/// </summary>
public sealed record PublishedDepartureTransportSummary(
    int Sequence,
    string TransportMode,
    string Origin,
    string Destination);

public sealed record PublishedDepartureAccommodationSummary(
    Guid PlaceId,
    int Nights,
    string BoardType);

public sealed record PublishedDepartureCapacitySummary(
    int? MinimumPax,
    int? MaximumPax);

public sealed record PublishedDeparturePublicSummary(
    Guid Id,
    Guid TourProductId,
    string Status,
    string? StartDate,
    string? EndDate,
    string? TimeZoneId,
    int? DurationDays,
    PublishedDepartureCapacitySummary Capacity,
    IReadOnlyList<PublishedDepartureTransportSummary> Transport,
    IReadOnlyList<PublishedDepartureAccommodationSummary> Accommodation);

/// <summary>
/// Public read query for Published TourDeparture facts only (P11-R8).
/// </summary>
public interface ITourDeparturePublicQuery
{
    Task<IReadOnlyList<PublishedDeparturePublicSummary>> GetPublishedByTourProductAsync(
        Guid tourProductId,
        CancellationToken cancellationToken = default);
}
