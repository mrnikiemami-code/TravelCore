using Microsoft.EntityFrameworkCore;
using NodaTime;
using TravelCore.Modules.Destination.Contracts;
using TravelCore.Modules.Place.Contracts;
using TravelCore.Modules.Tour.Contracts;
using TravelCore.Modules.Tour.Domain;

namespace TravelCore.Modules.Tour.Infrastructure.Services;

/// <summary>
/// Stop Destination/Place logical link mutations with Contracts validation (TC-P10-T003 / P10-R2).
/// </summary>
public sealed class ExperienceItineraryStopLinkService : IExperienceItineraryStopLinkService
{
    public const string AttractionKind = "Attraction";

    private readonly TourDbContext _db;
    private readonly IDestinationExistenceQuery _destinations;
    private readonly IPlaceService _places;
    private readonly IClock _clock;

    public ExperienceItineraryStopLinkService(
        TourDbContext db,
        IDestinationExistenceQuery destinations,
        IPlaceService places,
        IClock clock)
    {
        _db = db;
        _destinations = destinations;
        _places = places;
        _clock = clock;
    }

    public async Task<ExperienceItineraryStopLinksResponse?> GetAsync(
        Guid tourProductId,
        Guid stopId,
        CancellationToken cancellationToken = default)
    {
        var specialization = await FindSpecializationAsync(tourProductId, cancellationToken);
        if (specialization?.Itinerary is null)
        {
            return null;
        }

        ExperienceItineraryStop stop;
        try
        {
            stop = specialization.Itinerary.GetStop(ItineraryStopId.From(stopId));
        }
        catch (ArgumentException)
        {
            return null;
        }

        return Map(specialization.TourProductId, stop);
    }

    public async Task<ExperienceItineraryStopLinksResponse> SetLinksAsync(
        Guid tourProductId,
        Guid stopId,
        SetExperienceItineraryStopLinksRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (request.DestinationId is Guid destinationId)
        {
            await EnsureDestinationExistsAsync(destinationId, cancellationToken);
        }

        if (request.PlaceId is Guid placeId)
        {
            await EnsureAttractionPlaceExistsAsync(placeId, cancellationToken);
        }

        var specialization = await LoadSpecializationAsync(tourProductId, cancellationToken);
        var itinerary = specialization.Itinerary
            ?? throw new InvalidOperationException(
                "Experience itinerary was not found. Call EnsureItinerary before setting stop links.");

        var now = _clock.GetCurrentInstant();
        var typedStopId = ItineraryStopId.From(stopId);
        itinerary.SetStopDestinationLink(typedStopId, request.DestinationId, now);
        itinerary.SetStopPlaceLink(typedStopId, request.PlaceId, now);
        specialization.Touch(now);

        await _db.SaveChangesAsync(cancellationToken);
        return Map(specialization.TourProductId, itinerary.GetStop(typedStopId));
    }

    private async Task EnsureDestinationExistsAsync(Guid destinationId, CancellationToken cancellationToken)
    {
        if (!await _destinations.ExistsAsync(destinationId, cancellationToken))
        {
            throw new ArgumentException(
                $"Destination '{destinationId:D}' was not found.",
                nameof(destinationId));
        }
    }

    private async Task EnsureAttractionPlaceExistsAsync(Guid placeId, CancellationToken cancellationToken)
    {
        var place = await _places.GetByIdAsync(placeId, cancellationToken)
            ?? throw new ArgumentException($"Place '{placeId:D}' was not found.", nameof(placeId));

        if (!string.Equals(place.Kind, AttractionKind, StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException(
                $"Place '{placeId:D}' kind is '{place.Kind}'; Experience Stop PlaceId must be Attraction-kind (P10-R2).",
                nameof(placeId));
        }
    }

    private async Task<TourExperienceSpecialization?> FindSpecializationAsync(
        Guid tourProductId,
        CancellationToken cancellationToken)
    {
        var id = TourProductId.From(tourProductId);
        return await _db.ExperienceSpecializations
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.TourProductId == id, cancellationToken);
    }

    private async Task<TourExperienceSpecialization> LoadSpecializationAsync(
        Guid tourProductId,
        CancellationToken cancellationToken)
    {
        var id = TourProductId.From(tourProductId);
        return await _db.ExperienceSpecializations
                   .SingleOrDefaultAsync(x => x.TourProductId == id, cancellationToken)
               ?? throw new ArgumentException(
                   $"Experience specialization for TourProduct '{tourProductId:D}' was not found.",
                   nameof(tourProductId));
    }

    private static ExperienceItineraryStopLinksResponse Map(TourProductId tourProductId, ExperienceItineraryStop stop)
        => new(
            tourProductId.Value,
            stop.ItineraryDayId.Value,
            stop.Id.Value,
            stop.SortOrder,
            stop.DestinationId,
            stop.PlaceId);
}
