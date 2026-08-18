using Microsoft.EntityFrameworkCore;
using NodaTime;
using NodaTime.Text;
using TravelCore.Modules.Tour.Contracts;
using TravelCore.Modules.Tour.Domain;

namespace TravelCore.Modules.Tour.Infrastructure.Services;

/// <summary>
/// Public Published TourDeparture query (TC-P11-T009 · P11-R8). Visibility facts only — no commerce.
/// </summary>
public sealed class TourDeparturePublicQuery : ITourDeparturePublicQuery
{
    private static readonly LocalDatePattern DatePattern = LocalDatePattern.Iso;

    private readonly TourDbContext _db;

    public TourDeparturePublicQuery(TourDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<PublishedDeparturePublicSummary>> GetPublishedByTourProductAsync(
        Guid tourProductId,
        CancellationToken cancellationToken = default)
    {
        if (tourProductId == Guid.Empty)
        {
            throw new ArgumentException("TourProductId cannot be empty.", nameof(tourProductId));
        }

        var productId = TourProductId.From(tourProductId);
        var items = await _db.TourDepartures
            .AsNoTracking()
            .Where(x => x.TourProductId == productId && x.Status == TourDepartureStatus.Published)
            .ToListAsync(cancellationToken);

        return items
            .OrderBy(x => x.Schedule?.StartDate)
            .ThenBy(x => x.Id.Value)
            .Select(Map)
            .ToList();
    }

    public async Task<PublishedDeparturePublicSummary?> GetPublishedByIdAsync(
        Guid tourDepartureId,
        CancellationToken cancellationToken = default)
    {
        if (tourDepartureId == Guid.Empty)
        {
            throw new ArgumentException("TourDepartureId cannot be empty.", nameof(tourDepartureId));
        }

        var departure = await _db.TourDepartures
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == TourDepartureId.From(tourDepartureId), cancellationToken);

        if (departure is null || departure.Status != TourDepartureStatus.Published)
        {
            return null;
        }

        return Map(departure);
    }

    private static PublishedDeparturePublicSummary Map(TourDeparture departure)
    {
        int? durationDays = null;
        if (departure.Schedule is { } schedule)
        {
            durationDays = Period.DaysBetween(schedule.StartDate, schedule.EndDate) + 1;
        }

        var transport = departure.TransportSegmentsOrdered
            .Select(s => new PublishedDepartureTransportSummary(
                s.Sequence,
                s.TransportMode.ToString(),
                s.Origin,
                s.Destination))
            .ToList();

        var accommodation = departure.AccommodationOptions
            .OrderBy(x => x.Id.Value)
            .Select(a => new PublishedDepartureAccommodationSummary(
                a.PlaceId,
                a.Nights,
                a.BoardType.ToString()))
            .ToList();

        return new PublishedDeparturePublicSummary(
            departure.Id.Value,
            departure.TourProductId.Value,
            departure.Status.ToString(),
            departure.Schedule is null ? null : DatePattern.Format(departure.Schedule.StartDate),
            departure.Schedule is null ? null : DatePattern.Format(departure.Schedule.EndDate),
            departure.Schedule?.TimeZoneId,
            durationDays,
            new PublishedDepartureCapacitySummary(
                departure.Capacity?.MinimumPax,
                departure.Capacity?.MaximumPax),
            transport,
            accommodation);
    }
}
