using Microsoft.EntityFrameworkCore;
using NodaTime;
using NodaTime.Text;
using TravelCore.Modules.Tour.Contracts;
using TravelCore.Modules.Tour.Domain;

namespace TravelCore.Modules.Tour.Infrastructure.Services;

/// <summary>
/// Admin TourDeparture queries/mutations (TC-P11-T008). Execution data only.
/// </summary>
public sealed class TourDepartureAdminService : ITourDepartureAdminService
{
    private const int MaxListTake = 200;
    private static readonly LocalDatePattern DatePattern = LocalDatePattern.Iso;

    private readonly TourDbContext _db;
    private readonly IClock _clock;

    public TourDepartureAdminService(TourDbContext db, IClock clock)
    {
        _db = db;
        _clock = clock;
    }

    public async Task<TourDepartureResponse> CreateAsync(
        CreateTourDepartureRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var product = await _db.TourProducts
            .FirstOrDefaultAsync(x => x.Id == TourProductId.From(request.TourProductId), cancellationToken)
            ?? throw new InvalidOperationException("TourProduct was not found.");

        var now = _clock.GetCurrentInstant();
        var departure = TourDeparture.Create(product, now);
        _db.TourDepartures.Add(departure);
        await _db.SaveChangesAsync(cancellationToken);
        return Map(departure);
    }

    public async Task<TourDepartureResponse?> GetAsync(
        Guid departureId,
        CancellationToken cancellationToken = default)
    {
        var departure = await FindAsync(departureId, cancellationToken);
        return departure is null ? null : Map(departure);
    }

    public async Task<IReadOnlyList<TourDepartureResponse>> ListAsync(
        Guid? tourProductId = null,
        string? status = null,
        int take = 50,
        CancellationToken cancellationToken = default)
    {
        if (take < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(take), take, "take must be >= 1.");
        }

        take = Math.Min(take, MaxListTake);
        var query = _db.TourDepartures.AsNoTracking().AsQueryable();
        if (tourProductId is Guid productId)
        {
            var id = TourProductId.From(productId);
            query = query.Where(x => x.TourProductId == id);
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            var parsed = ParseStatus(status);
            query = query.Where(x => x.Status == parsed);
        }

        var items = await query
            .OrderByDescending(x => x.CreatedAt)
            .ThenBy(x => x.Id)
            .Take(take)
            .ToListAsync(cancellationToken);

        return items.Select(Map).ToList();
    }

    public async Task<TourDepartureResponse> SetScheduleAsync(
        Guid departureId,
        SetTourDepartureScheduleRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var departure = await FindAsync(departureId, cancellationToken)
            ?? throw new InvalidOperationException("TourDeparture was not found.");

        var start = ParseDate(request.StartDate, nameof(request.StartDate));
        var end = ParseDate(request.EndDate, nameof(request.EndDate));
        departure.SetSchedule(start, end, request.TimeZoneId, _clock.GetCurrentInstant());
        await _db.SaveChangesAsync(cancellationToken);
        return Map(departure);
    }

    public async Task<TourDepartureResponse> SetCapacityAsync(
        Guid departureId,
        SetTourDepartureCapacityRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var departure = await FindAsync(departureId, cancellationToken)
            ?? throw new InvalidOperationException("TourDeparture was not found.");

        departure.SetCapacity(request.MinimumPax, request.MaximumPax, _clock.GetCurrentInstant());
        await _db.SaveChangesAsync(cancellationToken);
        return Map(departure);
    }

    public async Task<TourDepartureResponse> SetStatusAsync(
        Guid departureId,
        SetTourDepartureStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var departure = await FindAsync(departureId, cancellationToken)
            ?? throw new InvalidOperationException("TourDeparture was not found.");

        var status = ParseStatus(request.Status);
        departure.SetStatus(status, _clock.GetCurrentInstant());
        await _db.SaveChangesAsync(cancellationToken);
        return Map(departure);
    }

    private async Task<TourDeparture?> FindAsync(Guid departureId, CancellationToken cancellationToken)
    {
        var id = TourDepartureId.From(departureId);
        return await _db.TourDepartures.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    private static LocalDate ParseDate(string value, string paramName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value, paramName);
        var parsed = DatePattern.Parse(value.Trim());
        if (!parsed.Success)
        {
            throw new ArgumentException("Date must be ISO LocalDate (yyyy-MM-dd).", paramName);
        }

        return parsed.Value;
    }

    private static TourDepartureStatus ParseStatus(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        if (!Enum.TryParse<TourDepartureStatus>(value.Trim(), ignoreCase: true, out var status)
            || !Enum.IsDefined(status))
        {
            throw new ArgumentException($"Unsupported TourDepartureStatus '{value}'.", nameof(value));
        }

        return status;
    }

    private static TourDepartureResponse Map(TourDeparture departure) =>
        new(
            departure.Id.Value,
            departure.TourProductId.Value,
            departure.Status.ToString(),
            departure.Schedule is null ? null : DatePattern.Format(departure.Schedule.StartDate),
            departure.Schedule is null ? null : DatePattern.Format(departure.Schedule.EndDate),
            departure.Schedule?.TimeZoneId,
            departure.Capacity?.MinimumPax,
            departure.Capacity?.MaximumPax,
            departure.CreatedAt.ToString(),
            departure.UpdatedAt.ToString());
}
