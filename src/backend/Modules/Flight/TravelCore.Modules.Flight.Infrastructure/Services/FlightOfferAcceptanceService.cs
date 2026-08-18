using Microsoft.EntityFrameworkCore;
using NodaTime;
using Npgsql;
using TravelCore.Modules.Flight.Contracts;
using TravelCore.Modules.Flight.Domain;
using TravelCore.Money;
using MoneyValue = TravelCore.Money.Money;
using FlightBookingAggregate = TravelCore.Modules.Flight.Domain.FlightBooking;

namespace TravelCore.Modules.Flight.Infrastructure.Services;

public sealed class FlightOfferAcceptanceService
{
    public const string UnconfiguredSourceKey = "unconfigured";

    private readonly FlightDbContext _db;
    private readonly IFlightOfferSourceResolver _resolver;
    private readonly IClock _clock;

    public FlightOfferAcceptanceService(
        FlightDbContext db,
        IFlightOfferSourceResolver resolver,
        IClock clock)
    {
        _db = db;
        _resolver = resolver;
        _clock = clock;
    }

    public async Task<FlightOfferSnapshot> AcceptAsync(
        FlightBookingId flightBookingId,
        string idempotencyKey,
        FlightSourceKey? requestedSourceKey = null,
        MoneyValue? previouslyObservedTotal = null,
        string? sourceOfferReference = null,
        CancellationToken cancellationToken = default)
    {
        var now = _clock.GetCurrentInstant();
        var existingIdempotency = await _db.FlightOfferIdempotency
            .SingleOrDefaultAsync(
                x => x.FlightBookingId == flightBookingId && x.IdempotencyKey == idempotencyKey.Trim(),
                cancellationToken);
        if (existingIdempotency is not null)
        {
            return await LoadSnapshotAsync(existingIdempotency.SnapshotId, cancellationToken);
        }

        var booking = await _db.FlightBookings
            .Include(x => x.Journeys)
            .ThenInclude(x => x.Segments)
            .Include(x => x.Passengers)
            .SingleAsync(x => x.Id == flightBookingId, cancellationToken);

        var existingSnapshot = await _db.FlightOfferSnapshots
            .Include(x => x.Monetary)
            .ThenInclude(x => x.CategoryFares)
            .Include(x => x.FareRules)
            .ThenInclude(x => x.Baggage)
            .SingleOrDefaultAsync(x => x.FlightBookingId == flightBookingId, cancellationToken);

        var sourceKey = requestedSourceKey ?? new FlightSourceKey(UnconfiguredSourceKey);
        if (requestedSourceKey is { } explicitKey && _resolver.Resolve(explicitKey) is null
            && explicitKey.Value != UnconfiguredSourceKey)
        {
            throw new InvalidOperationException("Offer source selection is server-controlled.");
        }

        var configured = _resolver.ListConfiguredKeys();
        if (configured.Count == 1)
        {
            sourceKey = configured[0];
        }
        else if (configured.Count > 1)
        {
            throw new InvalidOperationException("Automatic supplier routing/failover is not implemented.");
        }

        var source = _resolver.Resolve(sourceKey);
        if (source is null || !source.Capabilities.Contains(FlightSourceCapability.OfferRevalidation))
        {
            throw new InvalidOperationException(
                "Flight offer source is unconfigured; commercial prices cannot be fabricated.");
        }

        FlightOfferSourceResult offer;
        try
        {
            offer = await source.GetOfferAsync(ToRequest(booking, sourceOfferReference, previouslyObservedTotal), cancellationToken);
        }
        catch (OperationCanceledException)
        {
            offer = FlightOfferAcceptanceCoordinator.MapCanceledToUnknown(sourceKey, now);
        }

        if (offer.SourceKey.Value != sourceKey.Value)
        {
            throw new InvalidOperationException("Cross-source offer revalidation is forbidden.");
        }

        var snapshot = FlightOfferAcceptanceCoordinator.Accept(
            booking,
            now,
            offer,
            existingSnapshot,
            previouslyObservedTotal);

        if (existingSnapshot is not null && ReferenceEquals(snapshot, existingSnapshot))
        {
            return snapshot;
        }

        _db.FlightOfferSnapshots.Add(snapshot);
        _db.FlightOfferIdempotency.Add(
            new FlightOfferIdempotencyRecord(booking.Id, idempotencyKey, snapshot.Id, now));

        try
        {
            await _db.SaveChangesAsync(cancellationToken);
            return snapshot;
        }
        catch (DbUpdateException ex) when (IsUniqueViolation(ex))
        {
            _db.ChangeTracker.Clear();
            var winner = await _db.FlightOfferSnapshots
                .Include(x => x.Monetary)
                .ThenInclude(x => x.CategoryFares)
                .Include(x => x.FareRules)
                .ThenInclude(x => x.Baggage)
                .SingleAsync(x => x.FlightBookingId == flightBookingId, cancellationToken);
            if (winner.IsSameOfferIdentity(sourceKey.Value, offer.SourceOfferReference ?? string.Empty)
                && winner.Monetary.Total.Equals(offer.TotalAmount))
            {
                return winner;
            }

            throw new InvalidOperationException(
                "A different flight offer is already accepted; requote is required.");
        }
    }

    private async Task<FlightOfferSnapshot> LoadSnapshotAsync(
        FlightOfferSnapshotId snapshotId,
        CancellationToken cancellationToken) =>
        await _db.FlightOfferSnapshots
            .Include(x => x.Monetary)
            .ThenInclude(x => x.CategoryFares)
            .Include(x => x.FareRules)
            .ThenInclude(x => x.Baggage)
            .SingleAsync(x => x.Id == snapshotId, cancellationToken);

    private static FlightOfferRequest ToRequest(
        FlightBookingAggregate booking,
        string? sourceOfferReference,
        MoneyValue? previouslyObservedTotal) =>
        new(
            booking.Id.Value,
            booking.TripType,
            booking.Journeys
                .OrderBy(j => j.Ordinal)
                .SelectMany(j => j.Segments
                    .OrderBy(s => s.Ordinal)
                    .Select(s => new FlightOfferSegmentIdentity(
                        j.Ordinal,
                        s.Ordinal,
                        s.Origin,
                        s.Destination,
                        s.DepartureAt,
                        s.ArrivalAt,
                        s.MarketingCarrier,
                        s.OperatingCarrier,
                        s.FlightNumber)))
                .ToArray(),
            new FlightPassengerCount(
                booking.Passengers.Count(p => p.Category == FlightPassengerCategory.Adult),
                booking.Passengers.Count(p => p.Category == FlightPassengerCategory.Child),
                booking.Passengers.Count(p => p.Category == FlightPassengerCategory.Infant)),
            sourceOfferReference,
            previouslyObservedTotal);

    private static bool IsUniqueViolation(DbUpdateException exception)
    {
        for (Exception? current = exception; current is not null; current = current.InnerException)
        {
            if (current is PostgresException postgres
                && postgres.SqlState == PostgresErrorCodes.UniqueViolation)
            {
                return true;
            }
        }

        return false;
    }
}
