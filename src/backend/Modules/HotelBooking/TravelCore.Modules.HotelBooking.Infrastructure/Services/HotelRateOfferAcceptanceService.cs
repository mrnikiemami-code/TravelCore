using Microsoft.EntityFrameworkCore;
using NodaTime;
using Npgsql;
using TravelCore.Modules.HotelBooking.Contracts;
using TravelCore.Modules.HotelBooking.Domain;
using Stay = TravelCore.Modules.HotelBooking.Domain.HotelBooking;

namespace TravelCore.Modules.HotelBooking.Infrastructure.Services;

public sealed class HotelRateOfferAcceptanceService
{
    public const string UnconfiguredSourceKey = "unconfigured";

    private readonly HotelBookingDbContext _db;
    private readonly IHotelRateOfferSourceResolver _resolver;
    private readonly IClock _clock;

    public HotelRateOfferAcceptanceService(
        HotelBookingDbContext db,
        IHotelRateOfferSourceResolver resolver,
        IClock clock)
    {
        _db = db;
        _resolver = resolver;
        _clock = clock;
    }

    public async Task<HotelRateOfferSnapshot> AcceptAsync(
        HotelBookingId hotelBookingId,
        string idempotencyKey,
        RateSourceKey? requestedSourceKey = null,
        CancellationToken cancellationToken = default)
    {
        var now = _clock.GetCurrentInstant();
        var existingIdempotency = await _db.HotelRateOfferIdempotency
            .SingleOrDefaultAsync(
                x => x.HotelBookingId == hotelBookingId && x.IdempotencyKey == idempotencyKey.Trim(),
                cancellationToken);
        if (existingIdempotency is not null)
        {
            return await LoadSnapshotAsync(existingIdempotency.SnapshotId, cancellationToken);
        }

        var booking = await _db.HotelBookings
            .Include(x => x.Rooms)
            .ThenInclude(x => x.Guests)
            .SingleAsync(x => x.Id == hotelBookingId, cancellationToken);

        var existingSnapshot = await _db.HotelRateOfferSnapshots
            .Include(x => x.Rooms)
            .Include(x => x.Monetary)
            .ThenInclude(x => x.Charges)
            .Include(x => x.CancellationPolicy)
            .ThenInclude(x => x.Rules)
            .SingleOrDefaultAsync(x => x.HotelBookingId == hotelBookingId, cancellationToken);

        var sourceKey = requestedSourceKey ?? new RateSourceKey(UnconfiguredSourceKey);
        if (requestedSourceKey is { } explicitKey && _resolver.Resolve(explicitKey) is null
            && explicitKey.Value != UnconfiguredSourceKey)
        {
            throw new InvalidOperationException("Rate source selection is server-controlled.");
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
        if (source is null)
        {
            throw new InvalidOperationException(
                "Hotel rate source is unconfigured; commercial prices cannot be fabricated.");
        }

        var offer = await source.GetOfferAsync(ToRequest(booking), cancellationToken);
        if (existingSnapshot is not null)
        {
            return HotelRateOfferSnapshot.Accept(
                booking,
                now,
                booking.Place,
                booking.CheckInDate,
                booking.CheckOutDate,
                sourceKey.Value,
                offer.SourceOfferReference,
                offer.QuotedAt,
                offer.OfferExpiresAt,
                offer.Total,
                ToRoomLines(offer),
                ToPenaltyDrafts(offer),
                existingSnapshot,
                offer.PayableNow,
                offer.PayableAtProperty,
                ToCharges(offer),
                offer.PropertyTimeZoneId,
                offer.PublicExplanation);
        }

        var snapshot = HotelRateOfferSnapshot.Accept(
            booking,
            now,
            booking.Place,
            booking.CheckInDate,
            booking.CheckOutDate,
            sourceKey.Value,
            offer.SourceOfferReference,
            offer.QuotedAt,
            offer.OfferExpiresAt,
            offer.Total,
            ToRoomLines(offer),
            ToPenaltyDrafts(offer),
            existingAccepted: null,
            offer.PayableNow,
            offer.PayableAtProperty,
            ToCharges(offer),
            offer.PropertyTimeZoneId,
            offer.PublicExplanation);

        _db.HotelRateOfferSnapshots.Add(snapshot);
        _db.HotelRateOfferIdempotency.Add(
            new HotelRateOfferIdempotencyRecord(booking.Id, idempotencyKey, snapshot.Id, now));

        try
        {
            await _db.SaveChangesAsync(cancellationToken);
            return snapshot;
        }
        catch (DbUpdateException ex) when (IsUniqueViolation(ex))
        {
            _db.ChangeTracker.Clear();
            var winner = await _db.HotelRateOfferSnapshots
                .Include(x => x.Rooms)
                .Include(x => x.Monetary)
                .ThenInclude(x => x.Charges)
                .Include(x => x.CancellationPolicy)
                .ThenInclude(x => x.Rules)
                .SingleAsync(x => x.HotelBookingId == hotelBookingId, cancellationToken);
            if (winner.IsSameOfferIdentity(sourceKey.Value, offer.SourceOfferReference))
            {
                return winner;
            }

            throw new InvalidOperationException(
                "A different rate offer is already accepted; requote is required.");
        }
    }

    private async Task<HotelRateOfferSnapshot> LoadSnapshotAsync(
        HotelRateOfferSnapshotId snapshotId,
        CancellationToken cancellationToken) =>
        await _db.HotelRateOfferSnapshots
            .Include(x => x.Rooms)
            .Include(x => x.Monetary)
            .ThenInclude(x => x.Charges)
            .Include(x => x.CancellationPolicy)
            .ThenInclude(x => x.Rules)
            .SingleAsync(x => x.Id == snapshotId, cancellationToken);

    private static HotelRateOfferRequest ToRequest(Stay booking) =>
        new(
            booking.Id.Value,
            booking.Place.PlaceId,
            booking.CheckInDate,
            booking.CheckOutDate,
            booking.Rooms.Select(room => new HotelRateOfferRoomRequest(
                room.Id.Value,
                room.AdultCount,
                room.Guests
                    .Where(g => g.Category == HotelGuestCategory.Child)
                    .Select(g => g.AgeAtCheckIn!.Value.Years)
                    .ToArray()))
            .ToArray());

    private static IReadOnlyList<HotelRoomRateLine> ToRoomLines(HotelRateOfferSourceResult offer) =>
        offer.Rooms.Select(room => new HotelRoomRateLine(
            RoomReservationId.From(room.RoomReservationId),
            room.Amount,
            room.AvailabilitySelectionReference,
            room.SourceRateReference,
            room.BoardBasisCode))
        .ToArray();

    private static IReadOnlyList<HotelCancellationPenaltyRuleDraft> ToPenaltyDrafts(
        HotelRateOfferSourceResult offer) =>
        offer.PenaltyRules.Select(rule => new HotelCancellationPenaltyRuleDraft(
            rule.EffectiveFrom,
            rule.EffectiveUntil,
            rule.Penalty))
        .ToArray();

    private static IReadOnlyList<HotelChargeComponentLine>? ToCharges(HotelRateOfferSourceResult offer) =>
        offer.Charges is null
            ? null
            : offer.Charges.Select(c => new HotelChargeComponentLine(c.Code, c.Amount)).ToArray();

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
