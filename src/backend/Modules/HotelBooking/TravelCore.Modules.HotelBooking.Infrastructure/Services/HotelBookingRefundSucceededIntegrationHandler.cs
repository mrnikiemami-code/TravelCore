using Microsoft.EntityFrameworkCore;
using NodaTime;
using Npgsql;
using TravelCore.Modules.HotelBooking.Domain;
using TravelCore.Modules.Payment.Contracts;

namespace TravelCore.Modules.HotelBooking.Infrastructure.Services;

/// <summary>
/// RefundSucceeded is a trigger. Compensation may Cancel Pending only. Confirmed stays Confirmed (R7).
/// </summary>
internal sealed class HotelBookingRefundSucceededIntegrationHandler : IHotelBookingRefundSucceededIntegrationHandler
{
    private readonly HotelBookingDbContext _db;
    private readonly HotelAvailabilityHoldService _holds;
    private readonly IClock _clock;

    public HotelBookingRefundSucceededIntegrationHandler(
        HotelBookingDbContext db,
        HotelAvailabilityHoldService holds,
        IClock clock)
    {
        _db = db;
        _holds = holds;
        _clock = clock;
    }

    public async Task HandleAsync(
        HotelBookingRefundSucceededIntegrationEvent message,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);

        var existing = await _db.RefundSuccessInbox
            .SingleOrDefaultAsync(x => x.RefundId == message.RefundId, cancellationToken);
        if (existing is not null)
        {
            return;
        }

        var now = _clock.GetCurrentInstant();
        var bookingId = HotelBookingId.From(message.HotelBookingId);
        var booking = await _db.HotelBookings
            .Include(x => x.Rooms)
            .SingleOrDefaultAsync(x => x.Id == bookingId, cancellationToken)
            ?? throw new InvalidOperationException("HotelBooking was not found.");

        var reservation = await _db.HotelSupplierReservations
            .SingleOrDefaultAsync(x => x.HotelBookingId == bookingId, cancellationToken);

        if (booking.Status == HotelBookingStatus.Confirmed
            || reservation is { Status: HotelSupplierReservationStatus.Confirmed })
        {
            var kind = booking.Status == HotelBookingStatus.Confirmed
                ? HotelBookingRefundInvariantIssueKind.ConfirmedBooking
                : HotelBookingRefundInvariantIssueKind.UnexpectedSupplierConfirmed;
            if (!await _db.RefundInvariantIssues.AnyAsync(x => x.RefundId == message.RefundId, cancellationToken))
            {
                _db.RefundInvariantIssues.Add(
                    HotelBookingRefundInvariantIssue.Create(
                        booking.Id,
                        message.RefundId,
                        message.PaymentId,
                        kind,
                        now));
            }
        }
        else
        {
            booking.CancelFromAuthoritativePaymentCompensation(now);
            await TryReleaseHoldAsync(bookingId, now, cancellationToken);
        }

        _db.RefundSuccessInbox.Add(
            HotelBookingRefundSuccessInboxRecord.Create(message.RefundId, now));
        try
        {
            await _db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (IsUniqueViolation(ex))
        {
            _db.ChangeTracker.Clear();
        }
    }

    private async Task TryReleaseHoldAsync(
        HotelBookingId hotelBookingId,
        Instant now,
        CancellationToken cancellationToken)
    {
        var holds = await _db.HotelAvailabilityHolds
            .Where(x => x.HotelBookingId == hotelBookingId)
            .ToListAsync(cancellationToken);
        var active = holds.FirstOrDefault(h => h.IsActiveAndUnexpired(now));
        if (active is null)
        {
            return;
        }

        try
        {
            await _holds.ReleaseAsync(active.Id, cancellationToken);
        }
        catch (TimeoutException)
        {
            PersistAmbiguousRelease(hotelBookingId, now);
        }
        catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            PersistAmbiguousRelease(hotelBookingId, now);
        }
    }

    private void PersistAmbiguousRelease(HotelBookingId hotelBookingId, Instant now) =>
        _db.HotelBookingReconciliationIssues.Add(
            new HotelBookingReconciliationIssue(
                hotelBookingId,
                HotelBookingReconciliationIssueKind.AmbiguousReservationOutcome,
                now,
                detail: "Hold release timeout does not prove Released."));

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
