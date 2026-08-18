using Microsoft.EntityFrameworkCore;
using NodaTime;
using TravelCore.Modules.Booking.Contracts;
using TravelCore.Modules.Booking.Domain;

namespace TravelCore.Modules.Booking.Infrastructure.Services;

/// <summary>
/// Booking-owned capacity consumption application boundary (TC-P19-T003 / P19-R3).
/// Correctness uses PostgreSQL transaction-scoped advisory locks plus unique constraints.
/// Process-local locks are not the authoritative mechanism.
/// ConfiguredCapacity is an authoritative Tour read result supplied per operation.
/// </summary>
public sealed class BookingCapacityService
{
    private readonly BookingDbContext _db;

    public BookingCapacityService(BookingDbContext db)
    {
        _db = db;
    }

    public async Task<CapacityHold> HoldAsync(
        HoldCapacityCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);
        var key = CapacityHold.NormalizeIdempotencyKey(command.IdempotencyKey);

        await using var tx = await _db.Database.BeginTransactionAsync(cancellationToken);
        var booking = await _db.Bookings.SingleOrDefaultAsync(x => x.Id == command.BookingId, cancellationToken)
            ?? throw new InvalidOperationException("Booking was not found.");

        await AcquireDepartureLockAsync(booking.TourDeparture.LogicalId, cancellationToken);

        var existingByKey = await _db.CapacityHolds
            .SingleOrDefaultAsync(x => x.IdempotencyKey == key, cancellationToken);
        if (existingByKey is not null)
        {
            if (existingByKey.BookingId != command.BookingId
                || existingByKey.SeatCount != command.SeatCount
                || existingByKey.TourDeparture != booking.TourDeparture)
            {
                throw new InvalidOperationException("Idempotency key is already bound to a different hold.");
            }

            await tx.CommitAsync(cancellationToken);
            return existingByKey;
        }

        var hasActive = await _db.CapacityHolds
            .AnyAsync(
                x => x.BookingId == command.BookingId && x.Status == CapacityHoldStatus.Active,
                cancellationToken);
        if (hasActive)
        {
            throw new InvalidOperationException("Booking already has an Active CapacityHold.");
        }

        var account = await LoadOrCreateAccountAsync(booking.TourDeparture, cancellationToken);
        account.Reserve(command.SeatCount, command.ConfiguredCapacity);

        var hold = CapacityHold.Create(
            command.BookingId,
            booking.TourDeparture,
            command.SeatCount,
            command.ConfiguredCapacity,
            command.Now,
            command.ExpiresAt,
            key);
        _db.CapacityHolds.Add(hold);

        await _db.SaveChangesAsync(cancellationToken);
        await tx.CommitAsync(cancellationToken);
        return hold;
    }

    public Task ReleaseAsync(
        CapacityHoldId holdId,
        Instant now,
        CancellationToken cancellationToken = default) =>
        MutateActiveAsync(
            holdId,
            hold =>
            {
                if (hold.Status == CapacityHoldStatus.Released)
                {
                    return false;
                }

                hold.Release(now);
                return true;
            },
            account => account.ReleaseActive,
            cancellationToken);

    public Task ExpireAsync(
        CapacityHoldId holdId,
        Instant now,
        CancellationToken cancellationToken = default) =>
        MutateActiveAsync(
            holdId,
            hold =>
            {
                if (hold.Status == CapacityHoldStatus.Expired)
                {
                    return false;
                }

                hold.Expire(now);
                return true;
            },
            account => account.ReleaseActive,
            cancellationToken);

    public Task ConsumeAsync(
        CapacityHoldId holdId,
        Instant now,
        CancellationToken cancellationToken = default) =>
        MutateActiveAsync(
            holdId,
            hold =>
            {
                if (hold.Status == CapacityHoldStatus.Consumed)
                {
                    return false;
                }

                hold.Consume(now);
                return true;
            },
            account => account.ConsumeActive,
            cancellationToken);

    public async Task<int> ExpireDueAsync(Instant now, CancellationToken cancellationToken = default)
    {
        var dueIds = await _db.CapacityHolds
            .Where(x => x.Status == CapacityHoldStatus.Active && x.ExpiresAt <= now)
            .Select(x => x.Id)
            .ToListAsync(cancellationToken);

        foreach (var id in dueIds)
        {
            await ExpireAsync(id, now, cancellationToken);
        }

        return dueIds.Count;
    }

    private async Task MutateActiveAsync(
        CapacityHoldId holdId,
        Func<CapacityHold, bool> mutateHold,
        Func<DepartureCapacityAccount, Action<int>> accountMutation,
        CancellationToken cancellationToken)
    {
        await using var tx = await _db.Database.BeginTransactionAsync(cancellationToken);
        var hold = await _db.CapacityHolds.SingleOrDefaultAsync(x => x.Id == holdId, cancellationToken)
            ?? throw new InvalidOperationException("CapacityHold was not found.");

        await AcquireDepartureLockAsync(hold.TourDeparture.LogicalId, cancellationToken);
        if (!mutateHold(hold))
        {
            await tx.CommitAsync(cancellationToken);
            return;
        }

        var account = await LoadOrCreateAccountAsync(hold.TourDeparture, cancellationToken);
        accountMutation(account)(hold.SeatCount);
        await _db.SaveChangesAsync(cancellationToken);
        await tx.CommitAsync(cancellationToken);
    }

    private async Task<DepartureCapacityAccount> LoadOrCreateAccountAsync(
        TourDepartureReference departure,
        CancellationToken cancellationToken)
    {
        var existing = await _db.DepartureCapacityAccounts
            .SingleOrDefaultAsync(x => x.TourDeparture == departure, cancellationToken);
        if (existing is not null)
        {
            return existing;
        }

        var created = DepartureCapacityAccount.Create(departure);
        _db.DepartureCapacityAccounts.Add(created);
        return created;
    }

    private Task AcquireDepartureLockAsync(Guid tourDepartureId, CancellationToken cancellationToken)
    {
        var bytes = tourDepartureId.ToByteArray();
        var key1 = BitConverter.ToInt32(bytes, 0);
        var key2 = BitConverter.ToInt32(bytes, 4);
        return _db.Database.ExecuteSqlInterpolatedAsync(
            $"SELECT pg_advisory_xact_lock({key1}, {key2})",
            cancellationToken);
    }
}
