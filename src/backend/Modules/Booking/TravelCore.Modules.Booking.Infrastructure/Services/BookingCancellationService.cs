using Microsoft.EntityFrameworkCore;
using NodaTime;
using TravelCore.Modules.Booking.Domain;

namespace TravelCore.Modules.Booking.Infrastructure.Services;

/// <summary>
/// Booking-owned Pending cancellation orchestration (TC-P19-T006 / P19-R6).
/// Atomically cancels a Pending Booking and releases an Active hold when present.
/// Does not confirm Booking, does not implement Payment, and does not reverse Consumed holds.
/// </summary>
public sealed class BookingCancellationService
{
    private readonly BookingDbContext _db;

    public BookingCancellationService(BookingDbContext db)
    {
        ArgumentNullException.ThrowIfNull(db);
        _db = db;
    }

    public async Task CancelPendingAsync(
        BookingId bookingId,
        Instant now,
        CancellationToken cancellationToken = default)
    {
        await using var tx = await _db.Database.BeginTransactionAsync(cancellationToken);
        await AcquireLockAsync(bookingId.Value, cancellationToken);

        var booking = await _db.Bookings
            .Include(x => x.MonetarySnapshot)
            .ThenInclude(x => x!.Components)
            .Include(x => x.Passengers)
            .SingleOrDefaultAsync(x => x.Id == bookingId, cancellationToken)
            ?? throw new InvalidOperationException("Booking was not found.");

        if (booking.Status == BookingStatus.Cancelled)
        {
            await tx.CommitAsync(cancellationToken);
            return;
        }

        booking.CancelPending(now);

        var hold = await _db.CapacityHolds
            .SingleOrDefaultAsync(
                x => x.BookingId == bookingId && x.Status == CapacityHoldStatus.Active,
                cancellationToken);
        if (hold is not null)
        {
            await AcquireLockAsync(hold.TourDeparture.LogicalId, cancellationToken);
            hold.Release(now);
            var account = await _db.DepartureCapacityAccounts
                .SingleAsync(x => x.TourDeparture == hold.TourDeparture, cancellationToken);
            account.ReleaseActive(hold.SeatCount);
        }

        await _db.SaveChangesAsync(cancellationToken);
        await tx.CommitAsync(cancellationToken);
    }

    private Task AcquireLockAsync(Guid id, CancellationToken cancellationToken)
    {
        if (!_db.Database.IsRelational())
        {
            return Task.CompletedTask;
        }

        var bytes = id.ToByteArray();
        var key1 = BitConverter.ToInt32(bytes, 0);
        var key2 = BitConverter.ToInt32(bytes, 4);
        return _db.Database.ExecuteSqlInterpolatedAsync(
            $"SELECT pg_advisory_xact_lock({key1}, {key2})",
            cancellationToken);
    }
}
